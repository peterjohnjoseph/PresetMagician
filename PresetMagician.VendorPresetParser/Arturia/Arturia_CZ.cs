using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.Arturia
{
    [UsedImplicitly]
    public class Arturia_CZ : Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1131627386};

        protected override List<string> GetInstrumentNames()
        {
            return new List<string> {"CZ"};
        }
    }
}