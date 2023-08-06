using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMP.NET.Modules
{
    public class CustomStaffVoiced
    {
        //
        // Summary:
        //     Staff Id
        [JsonProperty("id")]
        public uint VisualNovelID { get; set; }

        //
        // Summary:
        //     Alias Id
        [JsonProperty("aid")]
        public uint AliasId { get; set; }

        //
        // Summary:
        //     Character Id
        [JsonProperty("cid")]
        public uint CharacterId { get; set; }

        //
        // Summary:
        //     Notes
        [JsonProperty("Note")]
        public string Note { get; set; }
    }
}
