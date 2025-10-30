namespace ModbusSimulationServer
{
    /// <summary>
    /// Configuration loaded from CSV file
    /// </summary>
    public class CsvConfiguration
    {
        public const string CFG_VERSION = "1.2";

        // Global parameters with defaults
        public string Version { get; set; } = "1";
        public int Port { get; set; } = 502;
        public int Loops { get; set; } = 1;
        public string? LogFile { get; set; } = null;
        public bool Debug { get; set; } = true;

        // Other discovered parameters
        public Dictionary<string, string> AdditionalParameters { get; set; } = new Dictionary<string, string>();

        // Data section
        public CsvDataSection? DataSection { get; set; }

        /// <summary>
        /// Validates the configuration
        /// </summary>
        public List<string> Validate()
        {
            var errors = new List<string>();

            // Validate version
            if (!IsVersionCompatible(Version, CFG_VERSION))
            {
                errors.Add($"Configuration version '{Version}' is not compatible with application version '{CFG_VERSION}'");
            }

            // Validate port
            if (Port < 1 || Port > 65535)
            {
                errors.Add($"Port {Port} is out of valid range (1-65535)");
            }

            // Validate loops
            if (Loops < 0)
            {
                errors.Add($"Loops value {Loops} cannot be negative");
            }

            // Validate data section if present
            if (DataSection != null)
            {
                errors.AddRange(DataSection.Validate());
            }

            return errors;
        }

        /// <summary>
        /// Checks if config version is compatible with app version
        /// </summary>
        private bool IsVersionCompatible(string configVersion, string appVersion)
        {
            try
            {
                // Parse versions (support formats like "1", "1.1", "1.2.a")
                var configParts = configVersion.Split('.');
                var appParts = appVersion.Split('.');

                // Compare major version
                if (configParts.Length > 0 && appParts.Length > 0)
                {
                    if (!int.TryParse(configParts[0], out int configMajor))
                        return false;
                    if (!int.TryParse(appParts[0], out int appMajor))
                        return false;

                    if (configMajor > appMajor)
                        return false;
                    if (configMajor < appMajor)
                        return true;
                }

                // Compare minor version if major is the same
                if (configParts.Length > 1 && appParts.Length > 1)
                {
                    if (!int.TryParse(configParts[1], out int configMinor))
                        return true; // If we can't parse, assume compatible
                    if (!int.TryParse(appParts[1], out int appMinor))
                        return true;

                    if (configMinor > appMinor)
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
