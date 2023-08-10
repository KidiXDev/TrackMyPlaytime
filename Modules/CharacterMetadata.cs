using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TMP.NET.Modules.Ext;
using VndbSharp.Models.Character;
using VndbSharp.Models.Common;

namespace TMP.NET.Modules
{
    public class CharacterMetadata
    {
        //
        // Summary:
        //     Character's ID on Vndb
        public uint Id { get; set; }

        //
        // Summary:
        //     Character's Name
        public string Name { get; set; }

        //
        // Summary:
        //     Character's Japanese/Original Name
        [JsonProperty("original")]
        public string OriginalName { get; set; }

        //
        // Summary:
        //     Character's Gender
        public Gender? Gender { get; set; }

        //
        // Summary:
        //     Actual Sex, if the gender is a spoiler
        public Gender? SpoilGender { get; set; }

        //
        // Summary:
        //     Character's Gender
        [JsonProperty("bloodt")]
        public BloodType? BloodType { get; set; }

        //
        // Summary:
        //     Character's Birthday
        public SimpleDate Birthday { get; set; }

        //
        // Summary:
        //     Character's Aliases/Nicknames
        public ReadOnlyCollection<string> Aliases { get; set; }

        //
        // Summary:
        //     Description of the character
        public string Description { get; set; }

        //
        // Summary:
        //     Character's age in years
        public long? Age { get; set; }

        //
        // Summary:
        //     Url Image of the character
        public string Image { get; set; }

        //
        // Summary:
        //     Properties of the character's image. This determines how violent/sexual it is
        [JsonProperty("image_flagging")]
        public ImageRating ImageRating { get; set; }

        //
        // Summary:
        //     Size in Centimeters
        public long? Bust { get; set; }

        //
        // Summary:
        //     Size in Centimeters
        public long? Waist { get; set; }

        //
        // Summary:
        //     Size in Centimeters
        public long? Hip { get; set; }

        //
        // Summary:
        //     Height in Centimeters
        public long? Height { get; set; }

        //
        // Summary:
        //     Weight in Kilograms
        public long? Weight { get; set; }

        //
        // Summary:
        //     CupSize of the character
        [JsonProperty("cup_size")]
        public string CupSize { get; set; }

        //
        // Summary:
        //     List of traits the character has
        public ReadOnlyCollection<CustomTraitMetadata> Traits { get; set; }

        //
        // Summary:
        //     List of Visual Novels linked to this character
        [JsonProperty("vns")]
        public List<CustomVisualNovelMetadata> VisualNovels { get; set; }

        //
        // Summary:
        //     List of voice actresses (staff) that voiced this character, per VN
        [JsonProperty("voiced")]
        public ReadOnlyCollection<CustomVoiceActorMetadata> VoiceActorMetadata { get; set; }

        //
        // Summary:
        //     List of instances of this character (excluding the character entry itself)
        [JsonProperty("instances")]
        public ReadOnlyCollection<CharacterInstances> CharacterInstances { get; set; }

        public int GroupOrder
        {
            get
            {
                switch (CharacterRole)
                {
                    case "Protagonist":
                        return 1;
                    case "Main Characters":
                        return 2;
                    case "Side Characters":
                        return 3;
                    case "Make an Appearance":
                        return 4;
                    case "No Role":
                        return 5;
                    default:
                        return int.MaxValue;
                }
            }
        }

        public string CharacterRole 
        {
            get
            {
                foreach (var vn in VisualNovels)
                {
                    if (vn.Id.Equals(VNID))
                    {
                        if (vn.Role == VndbSharp.Models.Character.CharacterRole.Main)
                            return "Protagonist";
                        else if (vn.Role == VndbSharp.Models.Character.CharacterRole.Primary)
                            return "Main Characters";
                        else if (vn.Role == VndbSharp.Models.Character.CharacterRole.Side)
                            return "Side Characters";
                        else if (vn.Role == VndbSharp.Models.Character.CharacterRole.Appears)
                            return "Make an Appearance";
                    }
                }

                return "No Role";
            }
        }

        public SpoilerLevel CharacterSpoiler
        {
            get
            {
                foreach(var vn in VisualNovels)
                {
                    if(vn.Id.Equals(VNID))
                    {
                        return vn.SpoilerLevel;
                    }
                }

                return SpoilerLevel.None;
            }
        }

        public ImageSource GenderSelector { get { return _genderSelector(); } }
        public Visibility GenderVisible { get { return _GenderVisible(); } }
        private Visibility _GenderVisible()
        {
            if (Gender == null || Gender == VndbSharp.Models.Common.Gender.Unknown)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        private ImageSource _genderSelector()
        {
            if (Gender == VndbSharp.Models.Common.Gender.Female)
                return new BitmapImage(new Uri("pack://application:,,,/TMP.NET;component/Resources/female-icon.png"));
            else if (Gender == VndbSharp.Models.Common.Gender.Male)
                return new BitmapImage(new Uri("pack://application:,,,/TMP.NET;component/Resources/male-icon.png"));
            else
                return null;
        }

        public string GetAliases
        {
            get
            {
                if (Aliases != null || Aliases.Count != 0)
                    return string.Join(",", Aliases);
                else
                    return null;
            }
        }

        public string GetBirthday
        {
            get
            {
                if (Birthday != null)
                {
                    if (Birthday.ToString().Equals("tba"))
                        return "Unknown";
                    else
                    {
                        string date = Birthday.ToString();
                        string[] sourceFormat = { "yyyy", "yyyy-MM", "MM-dd", "yyyy-MM-dd" };

                        DateTime dateTime;

                        if (DateTime.TryParseExact(date, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                        {
                            foreach (var format in sourceFormat)
                            {
                                if (DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                                {
                                    return dateTime.ToString("dd MMM, yyyy");
                                }
                            }
                        }
                        else
                        {
                            if (DateTime.TryParseExact(date, "MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                            {
                                return dateTime.ToString("dd MMMM");
                            }
                        }

                        return "Unknown";
                    }

                }
                else
                    return "Unknown";
            }
        }

        /// <summary>
        /// Trait dumps
        /// </summary>
        public List<CustomStaffMetadata> StaffMetadata;
        public uint VNID { private get; set; }

        public string GetVoiced
        {
            get
            {
                if (VoiceActorMetadata == null || VoiceActorMetadata.Count == 0)
                    return "Not Voiced";
                else
                {
                    List<string> vaName = new List<string>();
                    foreach (var staff in StaffMetadata)
                    {
                        var vc = staff.Voiced;
                        var alias = staff.Aliases;

                        foreach (var al in alias)
                        {
                            foreach (var va in vc)
                            {
                                foreach (var v in VoiceActorMetadata)
                                {
                                    if (al.Id == v.AliasId && va.AliasId == al.Id && Id == va.CharacterId && va.VisualNovelID == VNID)
                                    {
                                        if (string.IsNullOrEmpty(va.Note))
                                        {
                                            var name = al.Name;
                                            if (!vaName.Contains(name))
                                            {
                                                vaName.Add(name);
                                            }
                                            break;
                                        }
                                        else
                                        {
                                            var name = al.Name + $" ({va.Note})";
                                            if (!vaName.Contains(name))
                                            {
                                                vaName.Add(name);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (vaName.Count > 1)
                    {
                        string va = string.Join("\n", vaName);
                        if (string.IsNullOrEmpty(va))
                            return "Unknown";
                        else
                            return va;
                    }
                    else
                    {
                        string va = vaName.FirstOrDefault();
                        if (string.IsNullOrEmpty(va))
                            return "Unknown";
                        else
                            return va;
                    }
                }
            }
        }

        public Config.VndbConfig setting;

        public Inline[] GetDescription
        {
            get
            {
                if (!string.IsNullOrEmpty(Description))
                    return TextExt.Helper(Description, setting);
                else
                    return new Inline[] { new Run("No Description") };
            }
        }

        public string GetMeasurements
        {
            get
            {
                var sb = new StringBuilder();

                if ((Height ?? 0) != 0)
                    sb.Append("Height: ").Append(Height).Append("cm, ");

                if ((Weight ?? 0) != 0)
                    sb.Append("Weight: ").Append(Weight).Append("kg, ");

                if ((Bust ?? 0) != 0)
                    sb.Append("Bust: ").Append(Bust).Append("cm, ");

                if ((Waist ?? 0) != 0)
                    sb.Append("Waist: ").Append(Waist).Append("cm, ");

                if ((Hip ?? 0) != 0)
                    sb.Append("Hip: ").Append(Hip).Append("cm, ");

                if (BloodType != null)
                    sb.Append("Blood Type: ").Append(BloodType).Append(", ");

                if (!string.IsNullOrEmpty(CupSize))
                    sb.Append("Cup Size: ").Append(CupSize).Append(" cup, ");

                if (sb.Length == 0)
                    return "No Data";
                else
                    return sb.ToString(0, sb.Length - 2);
            }
        }

        #region Traits

        public List<CustomTraits> traitDumps;

        private string GetTrait(uint parentId, SpoilerLevel spoilerLevel)
        {
            try
            {
                var sb = new StringBuilder();

                var traitList = FindParentTrait();

                foreach (var trait in traitList)
                {
                    // 1 is id for "Hair"
                    // you can also use if (trait.Parent.Contains("Hair"))
                    if (trait.ParentId == parentId)
                    {
                        if (trait.Spoiler == spoilerLevel)
                            sb.Append(trait.Child).Append(", ");
                    }
                }

                if (sb.Length == 0 && spoilerLevel == SpoilerLevel.None) // "No Data" only applied for non spoiler traits
                    return "No Data";
                else if (sb.Length == 0)
                    return null;
                else
                    return sb.ToString(0, sb.Length - 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        // Hair

        public string GetHairWithNoSpoiler
        {
            get
            {
                // 1 is id for "Hair"
                var trait = GetTrait(1, SpoilerLevel.None);
                return !string.IsNullOrEmpty(trait) ? trait : null;
            }
        }

        public string GetHairWithMinorSpoiler
        {
            get
            {
                var trait = GetTrait(1, SpoilerLevel.Minor);
                return setting.SpoilerSetting >= SpoilerLevel.Minor ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        public string GetHairWithMajorSpoiler
        {
            get
            {
                var trait = GetTrait(1, SpoilerLevel.Major);
                return setting.SpoilerSetting >= SpoilerLevel.Major ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        // Eyes

        public string GetEyesWithNoSpoiler
        {
            get
            {
                // 35 is id for "Eyes"
                var trait = GetTrait(35, SpoilerLevel.None);
                return !string.IsNullOrEmpty(trait) ? trait : null;
            }
        }

        public string GetEyesWithMinorSpoiler
        {
            get
            {
                var trait = GetTrait(35, SpoilerLevel.Minor);
                return setting.SpoilerSetting >= SpoilerLevel.Minor ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        public string GetEyesWithMajorSpoiler
        {
            get
            {
                var trait = GetTrait(35, SpoilerLevel.Major);
                return setting.SpoilerSetting >= SpoilerLevel.Major ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        // Body

        public string GetBodyWithNoSpoiler
        {
            get
            {
                // 36 is id for "Body"
                var trait = GetTrait(36, SpoilerLevel.None);
                return !string.IsNullOrEmpty(trait) ? trait : null;
            }
        }

        public string GetBodyWithMinorSpoiler
        {
            get
            {
                var trait = GetTrait(36, SpoilerLevel.Minor);
                return setting.SpoilerSetting >= SpoilerLevel.Minor ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        public string GetBodyWithMajorSpoiler
        {
            get
            {
                var trait = GetTrait(36, SpoilerLevel.Major);
                return setting.SpoilerSetting >= SpoilerLevel.Major ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        // Clothes

        public string GetClothesWithNoSpoiler
        {
            get
            {
                // 37 is id for "Clothes"
                var trait = GetTrait(37, SpoilerLevel.None);
                return !string.IsNullOrEmpty(trait) ? trait : null;
            }
        }

        public string GetClothesWithMinorSpoiler
        {
            get
            {
                var trait = GetTrait(37, SpoilerLevel.Minor);
                return setting.SpoilerSetting >= SpoilerLevel.Minor ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        public string GetClothesWithMajorSpoiler
        {
            get
            {
                var trait = GetTrait(37, SpoilerLevel.Major);
                return setting.SpoilerSetting >= SpoilerLevel.Major ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        // Item

        public string GetItemWithNoSpoiler
        {
            get
            {
                // 38 is id for "Item"
                var trait = GetTrait(38, SpoilerLevel.None);
                return !string.IsNullOrEmpty(trait) ? trait : null;
            }
        }

        public string GetItemWithMinorSpoiler
        {
            get
            {
                var trait = GetTrait(38, SpoilerLevel.Minor);
                return setting.SpoilerSetting >= SpoilerLevel.Minor ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        public string GetItemWithMajorSpoiler
        {
            get
            {
                var trait = GetTrait(38, SpoilerLevel.Major);
                return setting.SpoilerSetting >= SpoilerLevel.Major ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        // Personality

        public string GetPersonalityWithNoSpoiler
        {
            get
            {
                // 39 is id for "Personality"
                var trait = GetTrait(39, SpoilerLevel.None);
                return !string.IsNullOrEmpty(trait) ? trait : null;
            }
        }

        public string GetPersonalityWithMinorSpoiler
        {
            get
            {
                var trait = GetTrait(39, SpoilerLevel.Minor);
                return setting.SpoilerSetting >= SpoilerLevel.Minor ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        public string GetPersonalityWithMajorSpoiler
        {
            get
            {
                var trait = GetTrait(39, SpoilerLevel.Major);
                return setting.SpoilerSetting >= SpoilerLevel.Major ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        // Roles

        public string GetRoleWithNoSpoiler
        {
            get
            {
                // 40 is id for "Role"
                var trait = GetTrait(40, SpoilerLevel.None);
                return !string.IsNullOrEmpty(trait) ? trait : null;

            }
        }

        public string GetRoleWithMinorSpoiler
        {
            get
            {
                var trait = GetTrait(40, SpoilerLevel.Minor);
                return setting.SpoilerSetting >= SpoilerLevel.Minor ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        public string GetRoleWithMajorSpoiler
        {
            get
            {
                var trait = GetTrait(40, SpoilerLevel.Major);
                return setting.SpoilerSetting >= SpoilerLevel.Major ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        // Engages in

        public string GetEngagesInWithNoSpoiler
        {
            get
            {
                // 41 is id for "EngagesIn"
                var trait = GetTrait(41, SpoilerLevel.None);
                return !string.IsNullOrEmpty(trait) ? trait : null;
            }
        }

        public string GetEngagesInWithMinorSpoiler
        {
            get
            {
                var trait = GetTrait(41, SpoilerLevel.Minor);
                return setting.SpoilerSetting >= SpoilerLevel.Minor ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        public string GetEngagesInWithMajorSpoiler
        {
            get
            {
                var trait = GetTrait(41, SpoilerLevel.Major);
                return setting.SpoilerSetting >= SpoilerLevel.Major ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        // Subject of

        public string GetSubjectOfWithNoSpoiler
        {
            get
            {
                // 42 is id for "SubjectOf"
                var trait = GetTrait(42, SpoilerLevel.None);
                return !string.IsNullOrEmpty(trait) ? trait : null;
            }
        }

        public string GetSubjectOfWithMinorSpoiler
        {
            get
            {
                var trait = GetTrait(42, SpoilerLevel.Minor);
                return setting.SpoilerSetting >= SpoilerLevel.Minor ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        public string GetSubjectOfWithMajorSpoiler
        {
            get
            {
                var trait = GetTrait(42, SpoilerLevel.Major);
                return setting.SpoilerSetting >= SpoilerLevel.Major ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        // Engages in (Sexual)

        public string GetEngagesSexualWithNoSpoiler
        {
            get
            {
                // 43 is id for "Engages in (Sexual)"
                var trait = GetTrait(43, SpoilerLevel.None);
                return !string.IsNullOrEmpty(trait) ? trait : null;
            }
        }

        public string GetEngagesSexualWithMinorSpoiler
        {
            get
            {
                var trait = GetTrait(43, SpoilerLevel.Minor);
                return setting.SpoilerSetting >= SpoilerLevel.Minor ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        public string GetEngagesSexualWithMajorSpoiler
        {
            get
            {
                var trait = GetTrait(43, SpoilerLevel.Major);
                return setting.SpoilerSetting >= SpoilerLevel.Major ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        // Subject of (Sexual)

        public string GetSubjectSexualWithNoSpoiler
        {
            get
            {
                // 1625 is id for "Subject of (Sexual)"
                var trait = GetTrait(1625, SpoilerLevel.None);
                return !string.IsNullOrEmpty(trait) ? trait : null;
            }
        }

        public string GetSubjectSexualWithMinorSpoiler
        {
            get
            {
                var trait = GetTrait(1625, SpoilerLevel.Minor);
                return setting.SpoilerSetting >= SpoilerLevel.Minor ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        public string GetSubjectSexualWithMajorSpoiler
        {
            get
            {
                var trait = GetTrait(1625, SpoilerLevel.Major);
                return setting.SpoilerSetting >= SpoilerLevel.Major ? (!string.IsNullOrEmpty(trait) ? ", " + trait : null) : null;
            }
        }

        public List<(string Parent, uint? ParentId, string Child, SpoilerLevel Spoiler)> FindParentTrait()
        {
            List<(string Parent, uint? ParentId, string Child, SpoilerLevel Spoiler)> traitList = new List<(string Parent, uint? ParentId, string Child, SpoilerLevel Spoiler)>();
            foreach (var trait in Traits)
            {
                var tName = traitDumps.FirstOrDefault(x => x.TraitId == trait.Id)?.Name;
                if (string.IsNullOrEmpty(tName))
                    continue;

                var tData = traitDumps.FirstOrDefault(x => x.TraitId == trait.Id);

                while (tData != null && tData.Parents.Length > 0)
                    tData = traitDumps.FirstOrDefault(x => x.TraitId == tData.Parents.Last());

                var parentTag = tData?.Name;
                var parentId = tData?.TraitId;

                traitList.Add((parentTag, parentId, tName, trait.SpoilerLevel));
            }
            return traitList;
        }

        #endregion
    }
}
