using System.Configuration;

namespace TMP.NET.Modules
{
    internal class ConfigurationManager
    {
        public static void EncryptConfig()
        {
            // Buka konfigurasi
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Dapatkan seksi yang ingin dienkripsi
            ConfigurationSection section = config.GetSection("configuration/EncryptedData");

            // Enkripsi seksi
            if (!section.SectionInformation.IsProtected)
            {
                section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
            }

            // Simpan perubahan
            config.Save();
        }
    }
}
