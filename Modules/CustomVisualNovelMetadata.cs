using VndbSharp.Models.Character;
using VndbSharp.Models.Common;

namespace TMP.NET.Modules
{
    public class CustomVisualNovelMetadata
    {
        //
        // Summary:
        //     Visual Novel ID
        public uint Id { get; set; }

        //
        // Summary:
        //     Visual Novel Release ID
        public uint ReleaseId { get; set; }

        //
        // Summary:
        //     Visual Novel Spoiler Level
        public SpoilerLevel SpoilerLevel { get; set; }

        //
        // Summary:
        //     Character Role
        public CharacterRole Role { get; set; }
    }
}
