using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CannedBytes.Midi.Message;
using Catel;
using Drachenkatze.PresetMagician.Utils;
using InteractivePreGeneratedViews;
using MethodTimer;
using PresetMagician.Migrations;
using PresetMagician.Models.EventArgs;
using SQLite.CodeFirst;
using Orc.EntityFramework;
using PresetMagician.SharedModels;

namespace SharedModels
{
    public class ApplicationDatabaseContext : DbContext, IDataPersistence
    {
        private int persistPresetCount;
        private const int PersistInterval = 400;
        public event EventHandler<PresetUpdatedEventArgs> PresetUpdated;
        public bool CompressPresetData { private get; set; }
        public MidiNoteName PreviewNote { private get; set; }
        private Dictionary<(string, string), Type> TypeCache = new Dictionary<(string, string), Type>();
        private Dictionary<string, Mode> ModeCache = new Dictionary<string, Mode>();
        public static string OverrideDbPath;

        private readonly List<(Preset preset, byte[] presetData)> presetDataList =
            new List<(Preset preset, byte[] presetData)>();

        public ApplicationDatabaseContext() : base(new SQLiteConnection(GetConnectionString()), true)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ValidateOnSaveEnabled = false;
        }

        public ApplicationDatabaseContext(string connectionString) : base(connectionString)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ValidateOnSaveEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer =
                new SqliteCreateDatabaseIfNotExists<ApplicationDatabaseContext>(modelBuilder);


            modelBuilder.Entity<Plugin>().HasMany(p => p.DefaultModes).WithMany(q => q.Plugins).Map(mc =>
                mc.MapLeftKey("PluginId").MapRightKey("ModeId").ToTable("PluginModes"));
            modelBuilder.Entity<Plugin>().HasMany(p => p.DefaultTypes).WithMany(q => q.Plugins).Map(mc =>
                mc.MapLeftKey("PluginId").MapRightKey("TypeId").ToTable("PluginTypes"));
            modelBuilder.Entity<Plugin>().IgnoreCatelProperties();
            modelBuilder.Entity<BankFile>().IgnoreCatelProperties();

            modelBuilder.Entity<Preset>().HasMany(p => p.Types).WithMany(q => q.Presets).Map(mc =>
                mc.MapLeftKey("PresetId").MapRightKey("TypeId").ToTable("PresetTypes"));

            modelBuilder.Entity<Preset>().HasMany(p => p.Modes).WithMany(q => q.Presets).Map(mc =>
                mc.MapLeftKey("PresetId").MapRightKey("ModeId").ToTable("PresetModes"));

            modelBuilder.Entity<PresetDataStorage>();
            modelBuilder.Entity<PluginLocation>();
            modelBuilder.Entity<SchemaVersion>();

            Database.SetInitializer(sqliteConnectionInitializer);
        }


        public static string GetDatabasePath(string dbPath = null)
        {
            if (!string.IsNullOrEmpty(dbPath))
            {
                return dbPath;
            }

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"Drachenkatze\PresetMagician\PresetMagician.sqlite3");
        }

        private static string GetViewCachePath()
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"Drachenkatze\PresetMagician\Caches\");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return Path.Combine(directory, "ViewCache.xml");
        }

        public static string GetConnectionString()
        {
            var cs = new SQLiteConnectionStringBuilder()
            {
                DataSource = GetDatabasePath(OverrideDbPath), ForeignKeys = false, SyncMode = SynchronizationModes.Off,
                CacheSize = -10240
            };
            


            return cs.ConnectionString;
        }

        public static ApplicationDatabaseContext Create()
        {
            return new ApplicationDatabaseContext();
        }

        public static void InitializeViewCache()
        {
            using (var context = new ApplicationDatabaseContext())
            {
                InteractiveViews
                    .SetViewCacheFactory(
                        context,
                        new FileViewCacheFactory(GetViewCachePath()));

                context.Migrate();
            }
        }

        public List<PluginLocation> GetPluginLocations(Plugin plugin)
        {
            if (!plugin.HasMetadata)
            {
                return new List<PluginLocation>();
            }

            var list = (from pluginLocation in PluginLocations
                where pluginLocation.VstPluginId == plugin.VstPluginId &&
                      pluginLocation.IsPresent && pluginLocation.Id != plugin.PluginLocation.Id
                select pluginLocation).ToList();

            // Workaround so that the combobox has the correct hash for the pluginlocation object
            list.Add(plugin.PluginLocation);

            return list;
        }

        public async Task<List<PresetDatabaseStatistics>> GetPresetStatistics()
        {
            var x = from plugin in Plugins
                let presetCompressedSize = Presets.Where(p => p.PluginId == plugin.Id)
                    .Select(p => p.PresetCompressedSize).DefaultIfEmpty(0).Sum()
                let presetUncompressedSize = Presets.Where(p => p.PluginId == plugin.Id).Select(p => p.PresetSize)
                    .DefaultIfEmpty(0).Sum()
                let presetCount = Presets.Count(p => p.PluginId == plugin.Id)
                where presetCount > 0
                orderby plugin.PluginName
                select new PresetDatabaseStatistics
                {
                    PluginName = plugin.PluginName,
                    PresetCount = presetCount,
                    PresetCompressedSize = presetCompressedSize,
                    PresetUncompressedSize = presetUncompressedSize
                };
            return await x.ToListAsync();
        }

        public async Task CompressOrDecompressPresets(bool compress, CancellationToken cancellationToken,
            IProgress<int> progress, IProgress<int> total)
        {
            int counter = 0;
            using (var tempContext = Create())
            {
                if (total != null)
                {
                    var totalItems =
                        await (from p in tempContext.PresetDataStorage where p.IsCompressed == !compress select p)
                            .CountAsync();
                    total.Report(totalItems);
                }
            }

            var done = false;
            while (!done)
            {
                using (var tempContext = Create())
                {
                    var presets = await (from presetDataStorage in tempContext.PresetDataStorage
                        where presetDataStorage.IsCompressed == !compress
                        select presetDataStorage).Take(200).ToListAsync();

                    if (presets.Count < 200)
                    {
                        done = true;
                    }

                    foreach (var p in presets)
                    {
                        p.IsCompressed = compress;
                        counter++;

                        if (progress != null)
                        {
                            progress.Report(counter);
                        }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            done = true;
                            break;
                        }
                    }

                    await tempContext.SaveChangesAsync();
                }
            }
            
            Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, "VACUUM;");
        }


        public void LoadPresetsForPlugin(Plugin plugin)
        {
            Configuration.AutoDetectChangesEnabled = false;
            plugin.PresetCache.Clear();
            plugin.PresetHashCache.Clear();
            var deletedPresets =
                (from deletedPreset in Presets
                    where deletedPreset.Plugin.Id == plugin.Id && deletedPreset.IsDeleted
                    select deletedPreset).AsNoTracking().ToList();

            foreach (var preset in deletedPresets)
            {
                plugin.PresetCache.Add(preset.SourceFile, preset);
            }

            using (plugin.Presets.SuspendChangeNotifications())
            {
                Entry(plugin).Collection(p => p.Presets).Query().Include(p => p.Modes).Include(p => p.Types)
                    .Where(p => !p.IsDeleted).Load();
            }

            foreach (var preset in plugin.Presets)
            {
                plugin.PresetCache.Add(preset.SourceFile, preset);
                plugin.PresetHashCache.Add((preset.PresetHash, preset.SourceFile), preset);
            }

            Configuration.AutoDetectChangesEnabled = true;
        }

        public bool HasPresets(Plugin plugin)
        {
            using (var tempContext = Create())
            {
                return tempContext.Presets.Any();
            }
        }

        public void DisableSaveForPlugin(Plugin plugin)
        {
            Entry(plugin).State = EntityState.Unchanged;
        }


        private void SavePresetData(Preset preset, byte[] data)
        {
            var hash = HashUtils.getIxxHash(data);

            if (preset.PresetHash == hash)
            {
                return;
            }

            preset.PresetHash = hash;
            presetDataList.Add((preset, data));
        }


        public async Task PersistPreset(Preset preset, byte[] data)
        {
            if (preset.Plugin == null || string.IsNullOrEmpty(preset.SourceFile))
            {
                throw new ArgumentException(
                    "The presets plugin must be set and the source file must not be null or empty");
            }

            Configuration.AutoDetectChangesEnabled = false;

            PresetUpdated.SafeInvoke(this, new PresetUpdatedEventArgs(preset));

            if (!preset.Plugin.PresetCache.ContainsKey(preset.SourceFile))
            {
                preset.Plugin.Presets.Add(preset);
                preset.PreviewNote = PreviewNote;
                SavePresetData(preset, data);
                preset.Plugin.PresetCache.Add(preset.SourceFile, preset);
            }
            else
            {
                SavePresetData(preset.Plugin.PresetCache[preset.SourceFile], data);
            }

            persistPresetCount++;
            if (persistPresetCount > PersistInterval)
            {
                await Flush();
                persistPresetCount = 0;
            }
        }

        public byte[] GetPresetData(Preset preset)
        {
            using (var tempContext = Create())
            {
                return tempContext.PresetDataStorage.Find(preset.PresetId).PresetData;
            }
        }

        public async Task Flush()
        {
            using (var tempContext = Create())
            {
                tempContext.Configuration.AutoDetectChangesEnabled = false;
                foreach (var (preset, presetData) in presetDataList)
                {
                    var existingPresetData = tempContext.PresetDataStorage.Find(preset.PresetId);

                    if (existingPresetData == null)
                    {
                        existingPresetData = new PresetDataStorage {PresetDataStorageId = preset.PresetId};

                        tempContext.PresetDataStorage.Add(existingPresetData);
                    }

                    existingPresetData.IsCompressed = CompressPresetData;
                    existingPresetData.PresetData = presetData.ToArray();
                    preset.PresetSize = existingPresetData.PresetData.Length;

                    if (existingPresetData.IsCompressed)
                    {
                        preset.PresetCompressedSize = existingPresetData.CompressedPresetData.Length;
                    }
                    else
                    {
                        preset.PresetCompressedSize = preset.PresetSize;
                    }
                }

                await tempContext.SaveChangesAsync();
                presetDataList.Clear();
            }

            Configuration.AutoDetectChangesEnabled = true;
            try
            {
                //Database.Log = delegate(string s) { LogTo.Debug(s);  };
                await SaveChangesAsync();
                //Database.Log = null;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public void Migrate()
        {
            var type = typeof(IMigration);

            SchemaVersions.Load();
            var currentAssembly = Assembly.GetExecutingAssembly();

            var types = currentAssembly.GetTypes()
                .Where(p => p.GetInterfaces().Contains(type)).OrderBy(p => p.Name);

            foreach (var migration in types)
            {
                if (migration.IsAbstract)
                {
                    continue;
                }

                var migrationExecuted = (from schemaVersion in SchemaVersions
                    where schemaVersion.Version == migration.Name
                    select schemaVersion).Any();

                if (migrationExecuted)
                {
                    continue;
                }

                BaseMigration instance = (BaseMigration) Activator.CreateInstance(migration);
                instance.Database = Database;
                instance.Up();

                var executedMigration = new SchemaVersion();
                executedMigration.Version = migration.Name;
                //SchemaVersions.Add(executedMigration);
            }

            SaveChanges();
        }

        public Type GetOrCreateType(string typeName, string subTypeName)
        {
            if (TypeCache.ContainsKey((typeName, subTypeName)))
            {
                return TypeCache[(typeName, subTypeName)];
            }

            var type = (from st in Types
                where st.Name == typeName && st.SubTypeName == subTypeName
                select st).FirstOrDefault();

            if (type == null)
            {
                type = new Type {Name = typeName, SubTypeName = subTypeName};
                Types.Add(type);
            }

            TypeCache.Add((typeName, subTypeName), type);

            return type;
        }

        public Mode GetOrCreateMode(string modeName)
        {
            if (ModeCache.ContainsKey(modeName))
            {
                return ModeCache[modeName];
            }

            var mode = (from st in Modes
                where st.Name == modeName
                select st).FirstOrDefault();

            if (mode == null)
            {
                mode = new Mode {Name = modeName};
                Modes.Add(mode);
            }

            ModeCache.Add(modeName, mode);

            return mode;
        }

        /// <summary>
        /// Gets a plugin location by dll path and hash
        /// </summary>
        /// <param name="dllPath"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public PluginLocation GetPluginLocation(string dllPath, string hash)
        {
            return (from st in PluginLocations
                where st.DllHash == hash &&
                      st.DllPath == dllPath
                select st).FirstOrDefault();
        }

        public DbSet<Plugin> Plugins { get; set; }
        public DbSet<Preset> Presets { get; set; }

        public DbSet<PluginLocation> PluginLocations { get; set; }

        public DbSet<Type> Types { get; set; }

        public DbSet<Mode> Modes { get; set; }
        public DbSet<SchemaVersion> SchemaVersions { get; set; }
        public DbSet<PresetDataStorage> PresetDataStorage { get; set; }
    }

    public class PresetDatabaseStatistics
    {
        public string PluginName { get; set; }
        public int PresetCount { get; set; }
        public long PresetUncompressedSize { get; set; }
        public long PresetCompressedSize { get; set; }

        public double SavedSpace
        {
            get
            {
                if (PresetUncompressedSize != 0)
                {
                    return 1-PresetCompressedSize / (double) PresetUncompressedSize;
                }

                return 0;
            }
        }
    }
}