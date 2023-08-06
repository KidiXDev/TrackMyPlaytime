using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMP.NET.Modules
{
    public class CustomVoiceActorMetadata
    {
        ///
        // Summary:
        //     Staff Id
        [JsonProperty("id")]
        public int StaffId { get; set; }

        //
        // Summary:
        //     Staff Alias ID
        [JsonProperty("aid")]
        public int AliasId { get; set; }

        //
        // Summary:
        //     Visual Novel ID
        [JsonProperty("vid")]
        public int VisualNovelId { get; set; }

        //
        // Summary:
        //     Notes
        [JsonProperty("Note")]
        public string Note { get; set; }
    }
}
