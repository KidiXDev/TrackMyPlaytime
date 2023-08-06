using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Navigation;
using VndbSharp.Models.Common;

namespace TMP.NET.Modules.Ext
{
    public class TextExt
    {
        // Most of the code i used on this class is inspired from VnManager made by micah686, Thank you very much.
        // Repo link: https://github.com/micah686/VnManager

        private const char SplitChar = '\x205E';
        private const char StartChar = '\x25C4';
        private const char EndChar = '\x25BA';
        private static readonly TimeSpan Timeout = new TimeSpan(0, 0, 0, 0, 500);

        public static Inline[] Helper(string text, Config.VndbConfig vndbConfig)
        {
            try
            {
                string modifiedText = text;
                if (string.IsNullOrEmpty(modifiedText))
                {
                    return new Inline[] { new Span() };
                }
                modifiedText = ReplaceSpoilers(text, vndbConfig);
                modifiedText = ReplaceVndbUrl(modifiedText);
                modifiedText = RemoveMark(modifiedText);
                modifiedText = ReplaceUrls(modifiedText);

                var inlineList = FormatUrlsInLine(modifiedText);


                return inlineList.ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new Inline[0];
            }
        }

        public static string ReplaceSpoilers(string text, Config.VndbConfig vndbConfig)
        {
            try
            {
                List<string> spoilList = new List<string>();
                string raw = text;

                Regex rgx = new Regex(@"\[spoiler\]((?:.|\n)+?)\[\/spoiler\]", RegexOptions.IgnoreCase);
                foreach (Match match in rgx.Matches(text))
                {
                    raw = raw.Replace(match.Groups[0].ToString(), match.Groups[1].ToString());
                    spoilList.Add(match.Groups[1].ToString());
                }

                if (vndbConfig.SpoilerSetting < SpoilerLevel.Major)
                {
                    raw = spoilList.Aggregate(raw,
                        (current, spoiler) => current.Replace(spoiler, "<Content hidden by spoiler setting>"));
                }

                return raw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public static string ReplaceVndbUrl(string text)
        {
            try
            {
                const int splitGroupCount = 4;
                string raw = text;
                Regex rgx = new Regex(@"(\[\burl\b=\/[a-z][0-9]+\])");
                foreach (var sp in rgx.Split(text))
                {
                    List<string> splitUrl = (new Regex(@"(\[)(\burl=)(\/[a-z][0-9]+)(\])", RegexOptions.Compiled | RegexOptions.IgnoreCase).Split(sp)).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                    if (splitUrl.Count != splitGroupCount)
                        continue;

                    if (splitUrl[1] != "url=")
                        continue;

                    splitUrl[1] = "url=http://vndb.org";
                    string combined = string.Join("", splitUrl);
                    raw = raw.Replace(sp, combined);
                }
                return raw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public static string RemoveMark(string text)
        {
            var rgx = new Regex(@"(\[(code|raw|s|u|i|b)\])(.+?(?=\[))(\[\/(code|raw|s|u|i|b)])", RegexOptions.IgnoreCase);
            var matches = rgx.Matches(text);
            var modString = text;
            foreach (Match match in matches)
            {
                var inputStr = match.Value;
                var outputStr = match.Groups[3].Value;
                modString = modString.Replace(inputStr, outputStr);
            }
            return modString;
        }

        public static string ReplaceUrls(string text)
        {
            var modifiedText = text;
            var rgx = new Regex(@"(\[url=)(.+?(?=\]))(\])(.+?(?=\[))(\[\/url])", RegexOptions.IgnoreCase, Timeout);
            var matches = rgx.Matches(modifiedText);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    var originalStr = match.Groups[0].Value;
                    var url = match.Groups[2].Value;
                    var displayName = match.Groups[4].Value;
                    var newUrlAndName = StartChar + url + SplitChar + displayName + EndChar;
                    modifiedText = modifiedText.Replace(originalStr, newUrlAndName);
                }
            }

            return modifiedText;
        }

        public static List<Inline> FormatUrlsInLine(string input)
        {
            const int splitTextValue = 2;
            List<Inline> inlineList = new List<Inline>();
            var rgx = new Regex(@"(\◄.+?\►)", RegexOptions.IgnoreCase);
            var str = rgx.Split(input);
            for (int i = 0; i < str.Length; i++)
            {
                if (i % splitTextValue == 0)
                {
                    inlineList.Add(new Run { Text = str[i] });
                }
                else
                {
                    var newText = str[i];
                    newText = newText.Replace($"{StartChar}", string.Empty);
                    newText = newText.Replace($"{EndChar}", string.Empty);
                    var split = newText.Split(SplitChar);
                    SplitUrl splitUrl = new SplitUrl { Url = split[0], Label = split[1] };
                    var run = new Run(splitUrl.Label);
                    Hyperlink link = new Hyperlink(run);


                    var isValid = Uri.IsWellFormedUriString(splitUrl.Url, UriKind.Absolute);
                    if (isValid)
                    {
                        link.NavigateUri = new Uri(splitUrl.Url);
                        link.RequestNavigate += Hyperlink_RequestNavigate;
                        inlineList.Add(link);
                    }
                    else
                    {
                        inlineList.Add(run);
                    }

                }
            }
            return inlineList;
        }

        internal struct SplitUrl : IEquatable<SplitUrl>
        {
            internal string Url;
            internal string Label;
            public bool Equals(SplitUrl other) =>
                (Url, Label) == (other.Url, other.Label);
        }

        private static void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
