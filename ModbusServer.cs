using FluentModbus;
using System.Net;

namespace ModbusSimulationServer
{
    public class ModbusServer : IDisposable
    {
        private ModbusTcpServer? _server;
        private readonly Logger? _logger;
        private bool _isRunning;

        public bool IsRunning => _isRunning;
        public ModbusTcpServer? Server => _server;

        public ModbusServer(Logger? logger = null)
        {
            _logger = logger;
        }

        public void Start(int port)
        {
            if (_isRunning)
            {
                _logger?.LogError("Modbus server is already running");
                return;
            }

            try
            {
                _logger?.LogInfo($"Starting Modbus TCP server on port {port}");

                _server = new ModbusTcpServer();
                
                // Add unit with ID 1 and allocate register space
                _server.AddUnit(1);
                
                _server.Start(new IPEndPoint(IPAddress.Any, port));

                _isRunning = true;
                _logger?.LogInfo($"Modbus TCP server started on port {port}");
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "Failed to start Modbus server");
                throw;
            }
        }

        public void Stop()
        {
            if (!_isRunning)
                return;

            try
            {
                _logger?.LogInfo("Stopping Modbus TCP server");
                _isRunning = false;

                _server?.Stop();
                _server = null;

                _logger?.LogInfo("Modbus TCP server stopped");
            }
            catch (Exception ex)
            {
                _logger?.LogException(ex, "Error stopping Modbus server");
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
