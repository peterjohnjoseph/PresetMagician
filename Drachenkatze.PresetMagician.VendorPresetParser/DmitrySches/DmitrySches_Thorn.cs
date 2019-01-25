using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.DmitrySches
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class DmitrySches_Thorn : DmitrySchesPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1416122990};

        protected override string Extension { get; } = "thorn";
        
        protected override string GetParseDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"Dmitry Sches\Thorn\Plug-In Presets");
        }

    }
}