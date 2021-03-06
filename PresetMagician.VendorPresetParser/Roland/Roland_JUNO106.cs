using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.Roland
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Roland_Juno106: RolandPlugoutParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1449227063};

        protected override string GetProductName()
        {
            return "JUNO-106";
        }

        protected override byte[] GetExportConfig()
        {
            return VendorResources.Roland_JUNO106_ExportConfig;
        }

        public override byte[] GetSuffixData()
        {
            return VendorResources.Roland_JUNO106_Suffix;
        }

        public override byte[] GetDefinitionData()
        {
            return VendorResources.Roland_JUNO106_Script;
        }
    }
    
    
}