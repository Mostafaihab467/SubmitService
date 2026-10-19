using System;
using System.IO;
using System.Xml.Linq;
using System.Configuration;
using SubmitServ.Models;

namespace SubmitServ.Helpers
{
    public static class SettingsManager
    {
        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "SubmitServ");
        
        private static readonly string SettingsFilePath = Path.Combine(AppDataFolder, "settings.xml");

        static SettingsManager()
        {
            if (!Directory.Exists(AppDataFolder))
            {
                Directory.CreateDirectory(AppDataFolder);
            }
        }

        public static void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var doc = XDocument.Load(SettingsFilePath);
                    var root = doc.Root;

                    AppSettings.client_id = GetXmlValue(root, "client_id", "5ee13adf-d438-48ac-b6f9-d30c2b0a4a62");
                    AppSettings.client_secret = GetXmlValue(root, "client_secret", "d2786429-f801-4b93-a709-353090a5a264");
                    AppSettings.posserial = GetXmlValue(root, "posserial", "E1EA0240-3140-49CF-AFA6-25EC735E018A");
                    AppSettings.pososversion = GetXmlValue(root, "pososversion", "Windows");
                    AppSettings.StatusLimit = GetXmlValue(root, "limit", "10");
                    AppSettings.connectionStr = GetXmlValue(root, "server", "Server=localhost;Database=TaxTech_Production;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true;");
                    AppSettings.qrURL = GetXmlValue(root, "qrURL", "https://invoicing.eta.gov.eg/receipts/search/");
                }
                else
                {
                    SetDefaults();
                    SaveSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                SetDefaults();
            }
        }

        public static void SaveSettings()
        {
            try
            {
                var doc = new XDocument(
                    new XElement("Settings",
                        new XElement("client_id", AppSettings.client_id ?? string.Empty),
                        new XElement("client_secret", AppSettings.client_secret ?? string.Empty),
                        new XElement("posserial", AppSettings.posserial ?? string.Empty),
                        new XElement("pososversion", AppSettings.pososversion ?? string.Empty),
                        new XElement("limit", AppSettings.StatusLimit ?? "10"),
                        new XElement("server", AppSettings.connectionStr ?? string.Empty),
                        new XElement("qrURL", AppSettings.qrURL ?? string.Empty)
                    )
                );

                doc.Save(SettingsFilePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        private static string GetXmlValue(XElement root, string elementName, string defaultValue)
        {
            var element = root?.Element(elementName);
            return !string.IsNullOrWhiteSpace(element?.Value) ? element.Value : defaultValue;
        }

        private static void SetDefaults()
        {
            AppSettings.client_id = "5ee13adf-d438-48ac-b6f9-d30c2b0a4a62";
            AppSettings.client_secret = "d2786429-f801-4b93-a709-353090a5a264";
            AppSettings.posserial = "E1EA0240-3140-49CF-AFA6-25EC735E018A";
            AppSettings.pososversion = "Windows";
            AppSettings.StatusLimit = "10";
            AppSettings.connectionStr = "Server=localhost;Database=TaxTech_Production;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true;";
            AppSettings.qrURL = "https://invoicing.eta.gov.eg/receipts/search/";
        }
    }
}
