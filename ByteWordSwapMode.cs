namespace ModbusSimulationServer
{
    /// <summary>
    /// Byte/word swapping modes for float encoding (2 registers = 4 bytes)
    /// </summary>
    public enum ByteWordSwapMode
    {
        /// <summary>ABCD - no swapping</summary>
        NoSwap = 0,
        
        /// <summary>BADC - swap bytes within each word</summary>
        SwapBytes = 1,
        
        /// <summary>CDAB - swap words</summary>
        SwapWords = 2,
        
        /// <summary>DCBA - swap both bytes and words</summary>
        SwapBoth = 3
    }
}
