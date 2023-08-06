using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VndbSharp.Models.Common;
using VndbSharp.Models.Staff;

namespace TMP.NET.Modules
{
    public class CustomStaffMetadata
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

        //
        // Summary:
        //     Staff Gender
        public Gender? Gender { get; set; }

        //
        // Summary:
        //     Primary Language
        public string Language { get; set; }

        //
        // Summary:
        //     Related Staff links
        [JsonProperty("links")]
        public StaffLinks StaffLinks { get; set; }

        //
        // Summary:
        //     Staff Description
        public string Description { get; set; }

        //
        // Summary:
        //     List of names and aliases
        public ReadOnlyCollection<CustomStaffAliases> Aliases { get; set; }

        //
        // Summary:
        //     Main alias
        [JsonProperty("main_alias")]
        public string MainAlias { get; set; }

        //
        // Summary:
        //     Vns that the staff member has worked on
        public StaffVns[] Vns { get; set; }

        //
        // Summary:
        //     List of Characters this staff has voiced
        public CustomStaffVoiced[] Voiced { get; set; }
    }
}
