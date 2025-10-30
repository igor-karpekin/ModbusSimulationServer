namespace ModbusSimulationServer
{
    /// <summary>
    /// Represents a column in the data section
    /// </summary>
    public class DataColumn
    {
        /// <summary>
        /// Column index in the CSV file
        /// </summary>
        public int ColumnIndex { get; set; }

        /// <summary>
        /// Column description (from first header line)
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Address assignment (from second header line)
        /// </summary>
        public string Assignment { get; set; } = string.Empty;

        /// <summary>
        /// Parsed Modbus address (null if special column like DeltaTime or Ref)
        /// </summary>
        public ModbusAddressAssignment? ModbusAddress { get; set; }

        /// <summary>
        /// True if this is the DeltaTime column
        /// </summary>
        public bool IsDeltaTime { get; set; }

        /// <summary>
        /// True if this is the Ref/ID column
        /// </summary>
        public bool IsRef { get; set; }

        /// <summary>
        /// True if this column should be ignored (empty assignment)
        /// </summary>
        public bool IsIgnored { get; set; }
    }
}
