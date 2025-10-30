namespace ModbusSimulationServer
{
    /// <summary>
    /// Manages playback of CSV data rows with timing control
    /// </summary>
    public class DataPlayback : IDisposable
    {
        public enum PlaybackState
        {
            Stopped,
            Playing,
            Paused
        }

        private readonly RegisterMap _registerMap;
        private readonly CsvDataSection _dataSection;
        private readonly int _loopCount; // 0 = infinite
        private readonly Logger? _logger;
        
        private System.Threading.Timer? _timer;
        private int _currentRowIndex = 0;
        private int _currentLoop = 0;
        private PlaybackState _state = PlaybackState.Stopped;
        private readonly object _lock = new object();

        // Event for row change notifications
        public event EventHandler<int>? RowChanged;

        public PlaybackState State
        {
            get { lock (_lock) return _state; }
            private set { lock (_lock) _state = value; }
        }

        public int CurrentRowIndex => _currentRowIndex;
        public int CurrentLoop => _currentLoop;

        public DataPlayback(RegisterMap registerMap, CsvDataSection dataSection, int loopCount, Logger? logger = null)
        {
            _registerMap = registerMap;
            _dataSection = dataSection;
            _loopCount = loopCount;
            _logger = logger;
        }

        public void Start()
        {
            lock (_lock)
            {
                if (_state == PlaybackState.Playing)
                    return;

                _logger?.LogInfo("Starting playback");
                
                // Reset to beginning
                _currentRowIndex = 0;
                _currentLoop = 0;
                
                // Apply first row immediately
                ApplyRow(_dataSection.Rows[0]);
                
                _state = PlaybackState.Playing;
                
                // Notify row change
                RowChanged?.Invoke(this, 0);
                
                // Schedule next row
                ScheduleNextRow(_dataSection.Rows[0].DeltaTime);
            }
        }

        public void Pause()
        {
            lock (_lock)
            {
                if (_state != PlaybackState.Playing)
                    return;

                _logger?.LogInfo($"Pausing playback at row {_currentRowIndex + 1}");
                
                _timer?.Dispose();
                _timer = null;
                _state = PlaybackState.Paused;
            }
        }

        public void Resume()
        {
            lock (_lock)
            {
                if (_state != PlaybackState.Paused)
                    return;

                _logger?.LogInfo($"Resuming playback from row {_currentRowIndex + 1}");
                
                _state = PlaybackState.Playing;
                
                // Continue from current row with its delay
                var currentRow = _dataSection.Rows[_currentRowIndex];
                ScheduleNextRow(currentRow.DeltaTime);
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                if (_state == PlaybackState.Stopped)
                    return;

                _logger?.LogInfo("Stopping playback");
                
                _timer?.Dispose();
                _timer = null;
                _state = PlaybackState.Stopped;
                _currentRowIndex = 0;
                _currentLoop = 0;
            }
        }

        private void ScheduleNextRow(int delayMs)
        {
            _timer = new System.Threading.Timer(
                OnTimerTick,
                null,
                delayMs,
                Timeout.Infinite);
        }

        private void OnTimerTick(object? state)
        {
            lock (_lock)
            {
                if (_state != PlaybackState.Playing)
                    return;

                // Move to next row
                _currentRowIndex++;

                // Check if we reached end of data
                if (_currentRowIndex >= _dataSection.Rows.Count)
                {
                    _currentLoop++;
                    
                    // Check loop count
                    if (_loopCount > 0 && _currentLoop >= _loopCount)
                    {
                        // Finished all loops - stay on last row
                        _logger?.LogInfo($"Playback completed after {_loopCount} loops");
                        _state = PlaybackState.Stopped;
                        _currentRowIndex = _dataSection.Rows.Count - 1;
                        return;
                    }

                    // Loop back to start
                    _logger?.LogDebug($"Loop {_currentLoop + 1} started");
                    _currentRowIndex = 0;
                }

                var row = _dataSection.Rows[_currentRowIndex];
                
                // Apply row data
                ApplyRow(row);
                
                // Notify row change
                RowChanged?.Invoke(this, _currentRowIndex);
                
                // Schedule next
                ScheduleNextRow(row.DeltaTime);
            }
        }

        private void ApplyRow(DataRow row)
        {
            // Don't log every row change - redundant with grid selection
            foreach (var column in _dataSection.Columns)
            {
                // Skip non-Modbus columns
                if (column.ModbusAddress == null)
                    continue;

                // Skip if value not present (inheritance - transparent)
                if (!row.HasValue.GetValueOrDefault(column.ColumnIndex, false))
                    continue;

                // Get value
                if (!row.Values.TryGetValue(column.ColumnIndex, out var value))
                    continue;

                // Apply to register map
                var addr = column.ModbusAddress;
                
                // Debug logging removed - redundant with grid display
                
                switch (addr.Space)
                {
                    case ModbusAddressAssignment.AddressSpace.HoldingRegister:
                        _registerMap.SetHolding(addr.Address, value, addr.Type);
                        break;

                    case ModbusAddressAssignment.AddressSpace.InputRegister:
                        _registerMap.SetInput(addr.Address, value, addr.Type);
                        break;

                    case ModbusAddressAssignment.AddressSpace.Coil:
                        _registerMap.SetCoil(addr.Address, value);
                        break;

                    case ModbusAddressAssignment.AddressSpace.DiscreteInput:
                        _registerMap.SetDiscrete(addr.Address, value);
                        break;
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
