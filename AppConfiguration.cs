using System.Text.Json;

namespace ModbusSimulationServer
{
    /// <summary>
    /// Configuration settings for the Modbus Simulation Server
    /// </summary>
    public class AppConfiguration
    {
        public string? LogFilePath { get; set; }
        public string? CsvDataFile { get; set; }
        public int ModbusPort { get; set; } = 502;
        public int UpdateIntervalMs { get; set; } = 1000;

        /// <summary>
        /// Load configuration from a JSON file
        /// </summary>
        public static AppConfiguration LoadFromFile(string configFilePath)
        {
            try
            {
                if (!File.Exists(configFilePath))
                {
                    // Return default configuration
                    return new AppConfiguration();
                }

                var json = File.ReadAllText(configFilePath);
                var config = JsonSerializer.Deserialize<AppConfiguration>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });

                return config ?? new AppConfiguration();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load configuration: {ex.Message}");
                return new AppConfiguration();
            }
        }

        /// <summary>
        /// Save configuration to a JSON file
        /// </summary>
        public void SaveToFile(string configFilePath)
        {
            try
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                var directory = Path.GetDirectoryName(configFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(configFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save configuration: {ex.Message}");
            }
        }
    }
}
