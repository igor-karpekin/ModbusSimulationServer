using System.Globalization;

namespace ModbusSimulationServer
{
    /// <summary>
    /// Parser for the data section of CSV configuration files
    /// </summary>
    public class CsvDataSectionParser
    {
        /// <summary>
        /// Parses the data section starting after #VALUE LOG marker
        /// </summary>
        /// <param name="lines">All lines from the CSV file</param>
        /// <param name="startLineIndex">Index of the line after #VALUE LOG</param>
        /// <returns>Parsed data section</returns>
        public static CsvDataSection Parse(string[] lines, int startLineIndex)
        {
            var dataSection = new CsvDataSection();

            if (startLineIndex >= lines.Length)
            {
                throw new InvalidDataException("No data section found after #VALUE LOG marker");
            }

            // Parse the two header lines
            int currentLine = startLineIndex;

            // First header line: descriptions
            var descriptionLine = lines[currentLine];
            var descriptions = ParseCsvLine(descriptionLine);
            currentLine++;

            if (currentLine >= lines.Length)
            {
                throw new InvalidDataException("Missing second header line (address assignments)");
            }

            // Second header line: address assignments
            var assignmentLine = lines[currentLine];
            var assignments = ParseCsvLine(assignmentLine);
            currentLine++;

            // Parse column definitions
            int columnCount = Math.Max(descriptions.Count, assignments.Count);
            for (int i = 0; i < columnCount; i++)
            {
                var description = i < descriptions.Count ? descriptions[i].Trim() : string.Empty;
                var assignment = i < assignments.Count ? assignments[i].Trim() : string.Empty;

                var column = ParseColumn(i, description, assignment);
                dataSection.Columns.Add(column);

                if (column.IsDeltaTime)
                    dataSection.DeltaTimeColumnIndex = i;
                if (column.IsRef)
                    dataSection.RefColumnIndex = i;
            }

            // Parse data rows
            int rowNumber = currentLine + 1; // Line number for error reporting
            for (int i = currentLine; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    rowNumber++;
                    continue;
                }

                // Skip comment lines
                if (line.StartsWith("#"))
                {
                    rowNumber++;
                    continue;
                }

                var dataRow = ParseDataRow(line, dataSection.Columns, rowNumber);
                dataSection.Rows.Add(dataRow);
                rowNumber++;
            }

            return dataSection;
        }

        private static DataColumn ParseColumn(int columnIndex, string description, string assignment)
        {
            var column = new DataColumn
            {
                ColumnIndex = columnIndex,
                Description = description,
                Assignment = assignment
            };

            // Check if column should be ignored (empty assignment)
            if (string.IsNullOrWhiteSpace(assignment))
            {
                column.IsIgnored = true;
                return column;
            }

            // Check for special columns (case insensitive)
            var assignmentUpper = assignment.ToUpperInvariant();

            if (assignmentUpper == "DELTATIME" || assignmentUpper == "DELAY")
            {
                column.IsDeltaTime = true;
                return column;
            }

            if (assignmentUpper == "REF" || assignmentUpper == "ID" || assignmentUpper == "TAG")
            {
                column.IsRef = true;
                return column;
            }

            // Try to parse as Modbus address assignment
            try
            {
                column.ModbusAddress = ModbusAddressAssignment.Parse(assignment);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException(
                    $"Invalid address assignment in column {columnIndex} ('{assignment}'): {ex.Message}");
            }

            return column;
        }

        private static DataRow ParseDataRow(string line, List<DataColumn> columns, int rowNumber)
        {
            var fields = ParseCsvLine(line);
            var dataRow = new DataRow
            {
                RowNumber = rowNumber
            };

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var value = i < fields.Count ? fields[i].Trim() : string.Empty;
                bool hasValue = !string.IsNullOrWhiteSpace(value);

                // Store value presence
                dataRow.HasValue[i] = hasValue;

                // Process based on column type
                if (column.IsDeltaTime)
                {
                    if (hasValue)
                    {
                        if (int.TryParse(value, out int deltaTime))
                        {
                            dataRow.DeltaTime = deltaTime;
                        }
                        else
                        {
                            throw new InvalidDataException(
                                $"Invalid DeltaTime value '{value}' at row {rowNumber}, column {i}. Must be an integer");
                        }
                    }
                    // If no value, DeltaTime remains 0 (will use previous row's time or default)
                }
                else if (column.IsRef)
                {
                    if (hasValue)
                    {
                        dataRow.Ref = value;
                    }
                }
                else if (!column.IsIgnored && hasValue)
                {
                    // Store the value for later conversion based on data type
                    dataRow.Values[i] = value;
                }
            }

            return dataRow;
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
    }
}
