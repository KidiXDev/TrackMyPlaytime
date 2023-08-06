using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbSharp.Models.Common;

namespace TMP.NET.Modules
{
    public class CustomTraitMetadata
    {
        //
        // Summary:
        //     Trait ID
        public uint Id { get; set; }

        //
        // Summary:
        //     Spoiler level of Trait
        public SpoilerLevel SpoilerLevel { get; set; }
    }
}
