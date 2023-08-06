using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbSharp.Models.Character;
using VndbSharp.Models.Common;

namespace TMP.NET.Modules
{
    public class CustomVisualNovelMetadata
    {
        //
        // Summary:
        //     Visual Novel ID
        public uint Id { get; private set; }

        //
        // Summary:
        //     Visual Novel Release ID
        public uint ReleaseId { get; private set; }

        //
        // Summary:
        //     Visual Novel Spoiler Level
        public SpoilerLevel SpoilerLevel { get; private set; }

        //
        // Summary:
        //     Character Role
        public CharacterRole Role { get; private set; }
    }
}
