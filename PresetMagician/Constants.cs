﻿using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Orc.Squirrel;

namespace PresetMagician
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class HelpLinks
    {
        #region Commands

        public static string COMMANDS_ANALYZE => "COMMANDS_ANALYZE";

        #endregion

        #region Reference

        public static string REFERENCE_TYPECHARACTERISTICEDITOR_ADDEDIT => "REFERENCE_TYPECHARACTERISTICEDITOR_ADDEDIT";
        public static string REFERENCE_TYPECHARACTERISTICEDITOR_DELETE => "REFERENCE_TYPECHARACTERISTICEDITOR_DELETE";

        public static string REFERENCE_TYPECHARACTERISTICEDITOR_SHOWUSAGES =>
            "REFERENCE_TYPECHARACTERISTICEDITOR_SHOWUSAGES";

        #endregion

        #region Settings

        public static string SETTINGS_PLUGIN_DLL => "SETTINGS_PLUGIN_DLL";
        public static string SETTINGS_PLUGIN_FXBFXPNOTES = "SETTINGS_PLUGIN_FXBFXPNOTES";

        #endregion

        #region Concepts

        public static string CONCEPTS_VST_WORKER_POOL => "CONCEPTS_VST_WORKER_POOL";

        #endregion
    }
}


namespace PresetMagician
{
    public static class FileLocations
    {
        public static string PresetMagicianLocalAppData =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Drachenkatze\PresetMagician");

        public static string LogFile = Path.Combine(PresetMagicianLocalAppData, @"Logs\PresetMagician.log");

        public static string LegacyDatabasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Drachenkatze\PresetMagician\PresetMagician.sqlite3");
    }

    public static class Settings
    {
        #region Links

        public class Links
        {
            private static readonly string _masterSite = "presetmagician.com";
            private static readonly string _masterProtocol = "https://";
            private static readonly string _supportSite = "support.presetmagician.com";

#if DEBUG
            private static readonly string _site = "localhost/presetmagiciansite/public";
            private static readonly string _protocol = "http://";
#else
            private static readonly string _site = "presetmagician.com";
            private static readonly string _protocol = "https://";
#endif

            public static readonly string Documentation = $"https://presetmagician.gitbook.io/help/";
            public static readonly string HelpLink = $"https://presetmagician.com/help/";
            public static readonly string Chat = "http://discord.gg/DraJRzv";
            public static readonly string CreateFeatureRequest = "https://github.com/PresetMagician/PresetMagician/issues/new/choose";
            public static readonly string CreateBugReport = "https://github.com/PresetMagician/PresetMagician/issues/new/choose";

            public static readonly string SubmitPlugins = $"{_protocol}{_site}/plugins/submit";
            public static readonly string SubmitPluginsLive = $"https://presetmagician.com/plugins/submit";
            public static readonly string SubmitResource = $"{_protocol}{_site}/plugins/submitResource";
            public static readonly string GetOnlineResources = $"{_protocol}{_site}/plugins/getResources/";
            public static readonly string GetOnlineResource = $"{_protocol}{_site}/plugins/getResource/";

            public static readonly string Homepage = $"{_masterProtocol}{_masterSite}";
        }

        #endregion

        #region Application

        public static class Application
        {
            public static class AutomaticUpdates
            {
                public const bool CheckForUpdatesDefaultValue = true;

                public static readonly ImmutableArray<UpdateChannel> AvailableChannels = ImmutableArray.Create(
                    new UpdateChannel("Stable", "https://presetmagician.com/downloads/stable"),
                    new UpdateChannel("Beta", "https://presetmagician.com/downloads/beta")
                        {IsPrerelease = true},
                    new UpdateChannel("Alpha", "https://presetmagician.com/downloads/alpha")
                        {IsPrerelease = true}
                );

                public static readonly UpdateChannel DefaultChannel = AvailableChannels[2];
            }
        }
    }

    #endregion

    #region Commands

    public static class Commands
    {
        public static class Application
        {
            public const string CancelOperation = "Application.CancelOperation";
            public const string ClearLastOperationErrors = "Application.ClearLastOperationErrors";
            public const string ApplyConfiguration = "Application.ApplyConfiguration";
            public const string NotImplemented = "Application.NotImplemented";
        }

        public static class Plugin
        {
            public const string RefreshPlugins = "Plugin.RefreshPlugins";
            public const string ScanPlugins = "Plugin.ScanPlugins";
            public const string ScanSelectedPlugins = "Plugin.ScanSelectedPlugins";
            public const string ScanSelectedPlugin = "Plugin.ScanSelectedPlugin";
            public const string AllToPresetExportList = "Plugin.AllToPresetExportList";
            public const string SelectedToPresetExportList = "Plugin.SelectedToPresetExportList";
            public const string NotExportedAllToPresetExportList = "Plugin.NotExportedAllToPresetExportList";
            public const string NotExportedSelectedToPresetExportList = "Plugin.NotExportedSelectedToPresetExportList";
            public const string ReportUnsupportedPlugins = "Plugin.ReportUnsupportedPlugins";
            public const string ForceReportPluginsToLive = "Plugin.ForceReportPluginsToLive";
            public const string ForceReportPluginsToDev = "Plugin.ForceReportPluginsToDev";
            public const string ForceReloadMetadata = "Plugin.ForceReloadMetadata";
            public const string RemoveSelectedPlugins = "Plugin.RemoveSelectedPlugins";
        }

        public static class PluginTools
        {
            public const string EnablePlugins = "PluginTools.EnablePlugins";
            public const string DisablePlugins = "PluginTools.DisablePlugins";
            public const string ViewSettings = "PluginTools.ViewSettings";
            public const string ViewPresets = "PluginTools.ViewPresets";
            public const string ViewErrors = "PluginTools.ViewErrors";
            public const string ShowPluginInfo = "PluginTools.ShowPluginInfo";
            public const string ShowPluginEditor = "PluginTools.ShowPluginEditor";
            public const string ShowPluginChunk = "PluginTools.ShowPluginChunk";
            public const string LoadPlugin = "PluginTools.LoadPlugin";
            public const string UnloadPlugin = "PluginTools.UnloadPlugin";
            public const string PatchPluginToAudioOutput = "PluginTools.PatchPluginToAudioOutput";

            public const string ReportSinglePluginToLive = "PluginTools.ReportSinglePluginToLive";
        }

        public static class PresetExport
        {
            public const string ActivatePresetView = "PresetExport.ActivatePresetView";
            public const string DoExport = "PresetExport.DoExport";
            public const string ClearList = "PresetExport.ClearList";
            public const string ClearSelected = "PresetExport.ClearSelected";
        }

        public static class PresetTools
        {
            public const string ShowPresetData = "PresetTools.ShowPresetData";
        }

        public static class Developer
        {
            public const string SetCatelLogging = "Developer.SetCatelLogging";
        }

        public static class Tools
        {
            public const string NksfView = "Tools.NksfView";
            public const string EditTypesCharacteristics = "Tools.EditTypesCharacteristics";
            public const string SettingsView = "Tools.SettingsView";
            public const string UpdateLicense = "Tools.UpdateLicense";
        }

        public static class Help
        {
            public const string CreateBugReport = "Help.CreateBugReport";
            public const string CreateFeatureRequest = "Help.CreateFeatureRequest";
            public const string RequestSupport = "Help.RequestSupport";
            public const string OpenChatLink = "Help.OpenChatLink";
            public const string OpenDocumentationLink = "Help.OpenDocumentationLink";
        }
    }

    #endregion
}