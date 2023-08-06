using Newtonsoft.Json;

namespace TMP.NET.Modules
{
    public class CustomTraits
    {
        public int Index { get; set; }
        [JsonProperty("id")]
        public uint TraitId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [JsonProperty("meta")]
        public bool IsMeta { get; set; }
        public bool IsSearchable { get; set; }
        public bool IsApplicable { get; set; }
        [JsonProperty("chars")]
        public uint Characters { get; set; }
        [JsonIgnore]
        public string Aliases { get; set; }
        public uint[] Parents { get; set; }
    }
}
