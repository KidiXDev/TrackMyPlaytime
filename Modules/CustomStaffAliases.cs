using Newtonsoft.Json;

namespace TMP.NET.Modules
{
    public class CustomStaffAliases
    {
        //
        // Summary:
        //     Staff Id
        public uint Id { get; set; }

        //
        // Summary:
        //     Staff Name
        public string Name { get; set; }

        //
        // Summary:
        //     Staff Original Name
        [JsonProperty("original")]
        public string OriginalName { get; set; }
    }
}
