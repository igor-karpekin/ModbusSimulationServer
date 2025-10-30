using System.Globalization;

namespace ModbusSimulationServer
{
    /// <summary>
    /// Parser for CSV configuration files
    /// </summary>
    public class CsvConfigurationParser
    {
        /// <summary>
        /// Parses a CSV configuration file
        /// </summary>
        /// <param name="filePath">Path to the CSV file</param>
        /// <returns>Parsed configuration</returns>
        /// <exception cref="InvalidDataException">Thrown when file format is invalid</exception>
        public static CsvConfiguration Parse(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Configuration file not found: {filePath}");
            }

            var config = new CsvConfiguration();
            var lines = File.ReadAllLines(filePath);

            if (lines.Length == 0)
            {
                throw new InvalidDataException("Configuration file is empty");
            }

            // First line must be the header
            var firstLine = lines[0].Trim();
            if (!firstLine.StartsWith("# simulation server configuration file", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "Invalid configuration file format. First line must be '# simulation server configuration file'");
            }

            // Parse global parameters until #VALUE LOG section
            bool inGlobalSection = true;
            int lineNumber = 0;
            int valueLogLineIndex = -1;

            foreach (var line in lines)
            {
                lineNumber++;

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Check for VALUE LOG section marker
                if (line.StartsWith("#VALUE LOG", StringComparison.OrdinalIgnoreCase))
                {
                    inGlobalSection = false;
                    valueLogLineIndex = lineNumber; // Store the index for data section parsing
                    break;
                }

                // Skip header line and other comment lines in global section
                if (line.StartsWith("#"))
                    continue;

                if (inGlobalSection)
                {
                    ParseGlobalParameter(config, line, lineNumber);
                }
            }

            // Parse data section if VALUE LOG marker was found
            if (valueLogLineIndex > 0 && valueLogLineIndex < lines.Length)
            {
                try
                {
                    config.DataSection = CsvDataSectionParser.Parse(lines, valueLogLineIndex);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Error parsing data section: {ex.Message}", ex);
                }
            }

            return config;
        }

        /// <summary>
        /// Parses a single global parameter line
        /// </summary>
        private static void ParseGlobalParameter(CsvConfiguration config, string line, int lineNumber)
        {
            try
            {
                // Parse CSV line (simple split, supports quoted values)
                var fields = ParseCsvLine(line);

                // Skip if first field (parameter name) is empty
                if (fields.Count == 0 || string.IsNullOrWhiteSpace(fields[0]))
                    return;

                string paramName = fields[0].Trim().ToLowerInvariant();
                
                // Value is in column 2 (index 1) based on actual CSV format
                // Format is: paramName,value,description,...
                if (fields.Count < 2 || string.IsNullOrWhiteSpace(fields[1]))
                    return;

                string value = fields[1].Trim();

                // Parse known parameters
                switch (paramName)
                {
                    case "version":
                        config.Version = value;
                        break;

                    case "port":
                    case "port ": // Handle trailing space as in the CSV
                        if (int.TryParse(value, out int port))
                        {
                            config.Port = port;
                        }
                        break;

                    case "loops":
                        if (int.TryParse(value, out int loops))
                        {
                            config.Loops = loops;
                        }
                        break;

                    case "logfile":
                        config.LogFile = value;
                        break;

                    case "debug":
                        config.Debug = ParseBool(value);
                        break;

                    // Ignore these known non-config parameters
                    case "step":
                    case "step delay":
                    case "step delay, ms":
                    case "vib1":
                    case "vib2":
                        // These might be simulation parameters, store in additional
                        config.AdditionalParameters[paramName] = value;
                        break;

                    default:
                        // Store unknown parameters for potential future use
                        // Only if it looks like a named parameter (not a number)
                        if (!string.IsNullOrWhiteSpace(paramName))
                        {
                            config.AdditionalParameters[paramName] = value;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Error parsing line {lineNumber}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Parses a CSV line into fields, handling quoted values
        /// </summary>
        private static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            bool inQuotes = false;
            var currentField = new System.Text.StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            // Add the last field
            fields.Add(currentField.ToString());

            return fields;
        }

        /// <summary>
        /// Parses a boolean value from various formats
        /// </summary>
        private static bool ParseBool(string value)
        {
            value = value.ToLowerInvariant().Trim();

            if (value == "1" || value == "true" || value == "yes")
                return true;

            if (value == "0" || value == "false" || value == "no")
                return false;

            // Default to true if can't parse
            return true;
        }
    }
}
