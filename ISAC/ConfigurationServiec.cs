using System;
using System.IO;
using System.Text.Json;
using System.Diagnostics;

namespace ImageSearchAndCopy
{
    public class AppSettings
    {
        public string LastSearchFolder { get; set; } = string.Empty;
        public string LastTargetFolder { get; set; } = string.Empty;
        public string LastCsvFile { get; set; } = string.Empty;
    }

    public static class ConfigurationService
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        public static AppSettings Load()
        {
            if (!File.Exists(ConfigPath)) return new AppSettings();

            try
            {
                string json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"加载配置失败: {ex.Message}");
                return new AppSettings();
            }
        }

        public static void Save(AppSettings settings)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"保存配置失败: {ex.Message}");
            }
        }
    }
}