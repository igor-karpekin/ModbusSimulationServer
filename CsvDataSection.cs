namespace ModbusSimulationServer
{
    /// <summary>
    /// Contains the parsed data section from CSV configuration
    /// </summary>
    public class CsvDataSection
    {
        /// <summary>
        /// Column definitions
        /// </summary>
        public List<DataColumn> Columns { get; set; } = new List<DataColumn>();

        /// <summary>
        /// Data rows
        /// </summary>
        public List<DataRow> Rows { get; set; } = new List<DataRow>();

        /// <summary>
        /// Index of DeltaTime column (-1 if not found)
        /// </summary>
        public int DeltaTimeColumnIndex { get; set; } = -1;

        /// <summary>
        /// Index of Ref column (-1 if not found)
        /// </summary>
        public int RefColumnIndex { get; set; } = -1;

        /// <summary>
        /// Gets all columns that have Modbus address assignments
        /// </summary>
        public IEnumerable<DataColumn> ModbusColumns => Columns.Where(c => c.ModbusAddress != null);

        /// <summary>
        /// Validates the data section
        /// </summary>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (Columns.Count == 0)
            {
                errors.Add("No columns defined in data section");
                return errors;
            }

            if (ModbusColumns.Count() == 0)
            {
                errors.Add("No Modbus address columns defined");
            }

            if (DeltaTimeColumnIndex == -1)
            {
                errors.Add("DeltaTime column not found. At least one column must be marked as 'DeltaTime'");
            }

            if (Rows.Count == 0)
            {
                errors.Add("No data rows found");
            }

            // Check for duplicate addresses
            var addressGroups = ModbusColumns
                .GroupBy(c => $"{c.ModbusAddress!.Space}{c.ModbusAddress.Address}")
                .Where(g => g.Count() > 1);

            foreach (var group in addressGroups)
            {
                var cols = string.Join(", ", group.Select(c => $"Column {c.ColumnIndex}"));
                errors.Add($"Duplicate Modbus address {group.Key} used in: {cols}");
            }

            // Check for duplicate Ref values
            var refs = Rows.Where(r => !string.IsNullOrEmpty(r.Ref)).Select(r => r.Ref).ToList();
            var duplicateRefs = refs.GroupBy(r => r).Where(g => g.Count() > 1).Select(g => g.Key);
            foreach (var dupRef in duplicateRefs)
            {
                var rowNumbers = Rows.Where(r => r.Ref == dupRef).Select(r => r.RowNumber);
                errors.Add($"Duplicate Ref value '{dupRef}' found in rows: {string.Join(", ", rowNumbers)}");
            }

            return errors;
        }
    }
}
