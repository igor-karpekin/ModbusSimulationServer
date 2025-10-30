namespace ModbusSimulationServer
{
    /// <summary>
    /// Represents a single data row with values for all registers
    /// </summary>
    public class DataRow
    {
        /// <summary>
        /// Row number in the file (for error reporting)
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// Delay in milliseconds before moving to next row
        /// </summary>
        public int DeltaTime { get; set; }

        /// <summary>
        /// Optional reference/ID for this row
        /// </summary>
        public string? Ref { get; set; }

        /// <summary>
        /// Register values indexed by column
        /// Key: column index, Value: string value from CSV (will be converted based on data type)
        /// </summary>
        public Dictionary<int, string> Values { get; set; } = new Dictionary<int, string>();

        /// <summary>
        /// Indicates if a value is present (not empty) in the CSV for this column
        /// Key: column index, Value: true if value is present
        /// </summary>
        public Dictionary<int, bool> HasValue { get; set; } = new Dictionary<int, bool>();
    }
}
