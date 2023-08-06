using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;

namespace TMP.NET.Modules.Ext
{
    public class CheckIntegrity
    {
        private string[] fileList = { "TMP.NET.exe", "handler.exe", "uninstall.exe", "VndbSharp.dll", "TMP.NET.exe.config", "System.ValueTuple.dll",
        "ReadMe.txt", "Newtonsoft.Json.dll", "Microsoft.Toolkit.Uwp.Notifications.dll", "log4net.dll", "DiscordRPC.dll"};
        public async Task<List<string>> VerifyFileAsync()
        {
            List<string> missingFile = new List<string>();
            foreach (var file in fileList)
            {
                if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file)))
                    missingFile.Add(file);
            }

            return await Task.FromResult(missingFile);
        }

        public async Task RepairingMissingFile(List<string> missingFile, string input, string output)
        {
            await Task.Run(() =>
            {
                if (missingFile == null || missingFile.Count == 0)
                {
                    throw new FileNotFoundException("No missing file found!");
                }

                if (!File.Exists(input))
                {
                    throw new FileNotFoundException("No input file found!");
                }

                using (ZipArchive archive = ZipFile.OpenRead(input))
                {
                    foreach (var missing in missingFile)
                    {
                        string fileName = Path.GetFileName(missing);
                        ZipArchiveEntry entry = archive.GetEntry(fileName);

                        if (entry != null)
                        {
                            string extractPath = Path.Combine(output, fileName);
                            entry.ExtractToFile(extractPath, true);
                        }
                        else
                        {
                            Console.WriteLine($"File \"{fileName}\" not found inside input file");
                        }
                    }
                }
            });
        }
    }
}
