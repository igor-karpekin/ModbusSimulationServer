using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace ModbusSimulationServer
{
    /// <summary>
    /// Thread-safe logging infrastructure that writes to UI and optionally to file
    /// </summary>
    public class Logger : IDisposable
    {
        public enum MessageType
        {
            Info,
            Error,
            Debug
        }

        // Static shared infrastructure for all Logger instances
        private static readonly ConcurrentQueue<LogEntry> _logQueue = new ConcurrentQueue<LogEntry>();
        private static System.Windows.Forms.Timer? _uiTimer;
        private static ListBox? _currentLogListBox;
        private static readonly object _staticLock = new object();
        
        // Instance-specific fields
        private readonly string? _logFilePath;
        private readonly object _fileLock = new object();
        private const int MaxListBoxItems = 1000;
        private const int MaxQueueSize = 500;
        private static DateTime _lastDateHeader = DateTime.MinValue;
        private bool _disposed = false;

        /// <summary>
        /// Controls whether debug messages are logged
        /// </summary>
        public bool DebugEnabled { get; set; } = true;

        public Logger(ListBox? logListBox, string? logFilePath = null, bool debugEnabled = true)
        {
            _logFilePath = logFilePath;
            DebugEnabled = debugEnabled;

            lock (_staticLock)
            {
                // Update the shared listbox reference
                _currentLogListBox = logListBox;

                // Initialize timer only once
                if (_uiTimer == null)
                {
                    _uiTimer = new System.Windows.Forms.Timer
                    {
                        Interval = 100 // Update UI every 100ms
                    };
                    _uiTimer.Tick += OnTimerTick;
                    _uiTimer.Start();
                }
            }

            // Write CSV header to log file if it doesn't exist
            InitializeLogFile();
        }

        private void InitializeLogFile()
        {
            if (string.IsNullOrEmpty(_logFilePath))
                return;

            try
            {
                // Create directory if it doesn't exist
                var directory = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Write header if file doesn't exist
                if (!File.Exists(_logFilePath))
                {
                    lock (_fileLock)
                    {
                        File.WriteAllText(_logFilePath, "Date,Time,ThreadID,MessageType,Source,Message\r\n", Encoding.UTF8);
                    }
                }
            }
            catch (Exception ex)
            {
                // Can't log this error to the logger, so just write to debug output
                Debug.WriteLine($"Failed to initialize log file: {ex.Message}");
            }
        }

        /// <summary>
        /// Log a message with automatic source detection using stack trace
        /// </summary>
        public void Log(string message, MessageType messageType = MessageType.Info)
        {
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(2);
            var method = frame?.GetMethod();
            var source = method != null ? $"{method.DeclaringType?.Name}.{method.Name}" : "Unknown";

            LogInternal(source, message, messageType);
        }

        /// <summary>
        /// Log a message with explicit source specification
        /// </summary>
        public void Log(string source, string message, MessageType messageType = MessageType.Info)
        {
            LogInternal(source, message, messageType);
        }

        private void LogInternal(string source, string message, MessageType messageType)
        {
            // Don't log if disposed
            if (_disposed)
                return;

            // Skip debug messages if debug is disabled
            if (messageType == MessageType.Debug && !DebugEnabled)
                return;

            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                ThreadId = Environment.CurrentManagedThreadId,
                MessageType = messageType,
                Source = source,
                Message = message,
                LogFilePath = _logFilePath  // Store the log file path with the entry
            };

            _logQueue.Enqueue(entry);

            // Prevent queue from growing too large
            if (_logQueue.Count > MaxQueueSize)
            {
                CleanupQueue();
            }
        }

        private static void CleanupQueue()
        {
            int itemsToRemove = _logQueue.Count / 3;
            for (int i = 0; i < itemsToRemove; i++)
            {
                _logQueue.TryDequeue(out _);
            }

            var warningEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                ThreadId = Environment.CurrentManagedThreadId,
                MessageType = MessageType.Error,
                Source = "Logger",
                Message = $"{itemsToRemove} log entries were dropped due to queue overflow",
                LogFilePath = null
            };
            _logQueue.Enqueue(warningEntry);
        }

        private static void OnTimerTick(object? sender, EventArgs e)
        {
            if (_currentLogListBox == null)
                return;

            try
            {
                // Process all queued log entries
                while (_logQueue.TryDequeue(out var entry))
                {
                    // Add date separator if day changed
                    if (entry.Timestamp.Date != _lastDateHeader.Date)
                    {
                        _currentLogListBox.Items.Add("");
                        _currentLogListBox.Items.Add($"===============   {entry.Timestamp:yyyy-MM-dd}   =======================");
                        _lastDateHeader = entry.Timestamp;
                    }

                    // Format the log entry for display
                    var displayText = FormatForDisplay(entry);
                    _currentLogListBox.Items.Add(displayText);

                    // Maintain max size of 1000 lines
                    while (_currentLogListBox.Items.Count > MaxListBoxItems)
                    {
                        _currentLogListBox.Items.RemoveAt(0);
                    }

                    // Auto-scroll to bottom
                    _currentLogListBox.TopIndex = _currentLogListBox.Items.Count - 1;

                    // Write to file if specified
                    if (!string.IsNullOrEmpty(entry.LogFilePath))
                    {
                        Task.Run(() => WriteToFile(entry));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating log UI: {ex.Message}");
            }
        }

        private static string FormatForDisplay(LogEntry entry)
        {
            // Use tabs between fields for better alignment
            var typePrefix = entry.MessageType switch
            {
                MessageType.Error => "ERROR",
                MessageType.Debug => "DEBUG",
                MessageType.Info => "INFO",
                _ => "?????"
            };

            // Format: Time\tThread\tType\tSource\tMessage
            return $"{entry.Timestamp:HH:mm:ss.fff}\tT{entry.ThreadId:000}\t{typePrefix}\t{entry.Source}\t{entry.Message}";
        }

        private static void WriteToFile(LogEntry entry)
        {
            if (string.IsNullOrEmpty(entry.LogFilePath))
                return;

            try
            {
                // Format as CSV: Date,Time,ThreadID,MessageType,Source,Message
                var csvLine = $"{entry.Timestamp:yyyy-MM-dd}," +
                              $"{entry.Timestamp:HH:mm:ss.fff}," +
                              $"{entry.ThreadId}," +
                              $"{entry.MessageType}," +
                              $"\"{EscapeCsv(entry.Source)}\"," +
                              $"\"{EscapeCsv(entry.Message)}\"\r\n";

                // Use a global lock for file writing to prevent conflicts
                lock (_staticLock)
                {
                    File.AppendAllText(entry.LogFilePath, csvLine, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Escape quotes by doubling them
            return value.Replace("\"", "\"\"");
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            // Note: We don't dispose the static timer or queue since they're shared
            // The timer will continue to run for other Logger instances
        }

        private class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public int ThreadId { get; set; }
            public MessageType MessageType { get; set; }
            public string Source { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string? LogFilePath { get; set; }  // Each entry knows which file to write to
        }
    }
}
