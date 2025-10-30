namespace ModbusSimulationServer
{
    /// <summary>
    /// Represents a Modbus register address assignment
    /// </summary>
    public class ModbusAddressAssignment
    {
        public enum AddressSpace
        {
            HoldingRegister,    // H
            InputRegister,      // I
            Coil,              // C
            DiscreteInput      // D
        }

        public enum DataType
        {
            Unsigned,   // U
            Signed,     // S
            Float,      // F
            Bit         // B
        }

        public AddressSpace Space { get; set; }
        public int Address { get; set; }
        public DataType Type { get; set; }

        /// <summary>
        /// Parses address assignment from string format: [H|I|C|D][0-65535][U|S|F|B]
        /// Examples: H100U, I200S, C50B, D1000B
        /// </summary>
        public static ModbusAddressAssignment Parse(string assignment)
        {
            if (string.IsNullOrWhiteSpace(assignment))
                throw new ArgumentException("Address assignment cannot be empty");

            assignment = assignment.Trim().ToUpperInvariant();

            if (assignment.Length < 3)
                throw new FormatException($"Invalid address assignment format: '{assignment}'");

            // Parse address space (first character)
            var spaceChar = assignment[0];
            AddressSpace space = spaceChar switch
            {
                'H' => AddressSpace.HoldingRegister,
                'I' => AddressSpace.InputRegister,
                'C' => AddressSpace.Coil,
                'D' => AddressSpace.DiscreteInput,
                _ => throw new FormatException($"Invalid address space: '{spaceChar}'. Must be H, I, C, or D")
            };

            // Parse data type (last character)
            var typeChar = assignment[^1];
            DataType dataType = typeChar switch
            {
                'U' => DataType.Unsigned,
                'S' => DataType.Signed,
                'F' => DataType.Float,
                'B' => DataType.Bit,
                _ => throw new FormatException($"Invalid data type: '{typeChar}'. Must be U, S, F, or B")
            };

            // Validate data type for address space
            if ((space == AddressSpace.Coil || space == AddressSpace.DiscreteInput) && dataType != DataType.Bit)
            {
                throw new FormatException($"Address space {spaceChar} must use Bit (B) data type");
            }

            if ((space == AddressSpace.HoldingRegister || space == AddressSpace.InputRegister) && dataType == DataType.Bit)
            {
                throw new FormatException($"Address space {spaceChar} cannot use Bit (B) data type. Use U, S, or F");
            }

            // Parse address (middle part)
            var addressStr = assignment[1..^1];
            if (!int.TryParse(addressStr, out int address) || address < 0 || address > 65535)
            {
                throw new FormatException($"Invalid address: '{addressStr}'. Must be 0-65535");
            }

            return new ModbusAddressAssignment
            {
                Space = space,
                Address = address,
                Type = dataType
            };
        }

        public override string ToString()
        {
            var spaceChar = Space switch
            {
                AddressSpace.HoldingRegister => 'H',
                AddressSpace.InputRegister => 'I',
                AddressSpace.Coil => 'C',
                AddressSpace.DiscreteInput => 'D',
                _ => '?'
            };

            var typeChar = Type switch
            {
                DataType.Unsigned => 'U',
                DataType.Signed => 'S',
                DataType.Float => 'F',
                DataType.Bit => 'B',
                _ => '?'
            };

            return $"{spaceChar}{Address}{typeChar}";
        }
    }
}
