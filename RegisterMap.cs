using System.Globalization;
using System.Runtime.InteropServices;

namespace ModbusSimulationServer
{
    /// <summary>
    /// Manages Modbus register memory - wraps FluentModbus server buffers
    /// </summary>
    public class RegisterMap
    {
        private readonly FluentModbus.ModbusTcpServer _server;
        private readonly byte _unitId;

        public RegisterMap(FluentModbus.ModbusTcpServer server, byte unitId = 1)
        {
            _server = server;
            _unitId = unitId;
        }

        // Direct access to FluentModbus buffers - cast signed to unsigned
        public Span<ushort> HoldingRegisters => MemoryMarshal.Cast<short, ushort>(_server.GetHoldingRegisters(_unitId));
        public Span<ushort> InputRegisters => MemoryMarshal.Cast<short, ushort>(_server.GetInputRegisters(_unitId));
        public Span<byte> Coils => _server.GetCoils(_unitId);
        public Span<byte> DiscreteInputs => _server.GetDiscreteInputs(_unitId);

        public void SetHolding(int address, string value, ModbusAddressAssignment.DataType dataType)
        {
            var regs = HoldingRegisters;
            SetRegister(regs, address, value, dataType);
        }

        public void SetInput(int address, string value, ModbusAddressAssignment.DataType dataType)
        {
            var regs = InputRegisters;
            SetRegister(regs, address, value, dataType);
        }

        public void SetCoil(int address, string value)
        {
            var coils = Coils;
            bool bitValue = false;
            
            if (int.TryParse(value, out int intVal))
                bitValue = intVal != 0;
            else if (bool.TryParse(value, out bool boolVal))
                bitValue = boolVal;

            int byteIndex = address / 8;
            int bitIndex = address % 8;
            
            if (bitValue)
                coils[byteIndex] |= (byte)(1 << bitIndex);
            else
                coils[byteIndex] &= (byte)~(1 << bitIndex);
        }

        public void SetDiscrete(int address, string value)
        {
            var discretes = DiscreteInputs;
            bool bitValue = false;
            
            if (int.TryParse(value, out int intVal))
                bitValue = intVal != 0;
            else if (bool.TryParse(value, out bool boolVal))
                bitValue = boolVal;

            int byteIndex = address / 8;
            int bitIndex = address % 8;
            
            if (bitValue)
                discretes[byteIndex] |= (byte)(1 << bitIndex);
            else
                discretes[byteIndex] &= (byte)~(1 << bitIndex);
        }

        private void SetRegister(Span<ushort> registers, int address, string value, ModbusAddressAssignment.DataType dataType)
        {
            switch (dataType)
            {
                case ModbusAddressAssignment.DataType.Unsigned:
                    if (ushort.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort uval))
                        registers[address] = uval;
                    break;

                case ModbusAddressAssignment.DataType.Signed:
                    if (short.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out short sval))
                        registers[address] = unchecked((ushort)sval);
                    break;

                case ModbusAddressAssignment.DataType.Float:
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float fval))
                    {
                        byte[] bytes = BitConverter.GetBytes(fval);
                        // Modbus uses big-endian: swap word order
                        registers[address] = BitConverter.ToUInt16(bytes, 2);      // High word
                        registers[address + 1] = BitConverter.ToUInt16(bytes, 0);  // Low word
                    }
                    break;
            }
        }

        public void Clear()
        {
            HoldingRegisters.Clear();
            InputRegisters.Clear();
            Coils.Clear();
            DiscreteInputs.Clear();
        }
    }
}
