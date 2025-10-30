# Logging Infrastructure

## Overview

The Modbus Simulation Server includes a comprehensive, thread-safe logging infrastructure that supports:

- Real-time display in the Application Log tab
- Optional CSV file logging
- Thread-safe operation from any thread
- Automatic source detection using stack traces
- Multiple message types (Info, Error, Debug)
- Tab-separated fields for better readability
- 1000-line rolling buffer
- **Debug message filtering** (controlled by CSV config)

## Features

### Thread Safety
The logger uses `ConcurrentQueue` and timer-based UI updates to ensure thread-safe operation. You can call logging methods from any thread without worrying about cross-thread exceptions.

### Debug Filtering
Debug messages can be enabled or disabled via the CSV configuration file:
- When `debug = 1` (or `true`): All messages including DEBUG are shown
- When `debug = 0` (or `false`): Only INFO and ERROR messages are shown

This helps reduce log clutter in production mode while still logging important operations and errors.

### Log Format

#### UI Display Format:
```
HH:mm:ss.fff	T000	INFO	Source	Message
```
Fields are separated by tabs for better alignment regardless of source name length.

#### CSV File Format:
```
Date,Time,ThreadID,MessageType,Source,Message
2024-01-15,14:23:45.123,1,Info,"MainForm.Form1_Load","Application started"
```

### Message Types
- **Info**: Normal operational messages (important steps) - **Always logged**
- **Error**: Error conditions and exceptions (all errors must be logged) - **Always logged**
- **Debug**: Detailed diagnostic information (minor operations) - **Filtered by debug flag**

### Display Limits
- **Maximum lines**: 1000 entries (oldest entries automatically removed)
- **Queue size**: 500 pending messages
- **Update interval**: 100ms

## Usage

### Basic Logging

```csharp
// Automatic source detection (recommended)
_logger.Log("Operation completed successfully", Logger.MessageType.Info);

// Explicit source specification
_logger.Log("MyMethod", "Custom source example", Logger.MessageType.Debug);

// Error logging
_logger.Log("Failed to connect to device", Logger.MessageType.Error);
```

### Initialization

The logger is initialized in `MainForm.Form1_Load()`:

```csharp
_logger = new Logger(lbLog, logFilePath, debugEnabled);
```

- `lbLog`: The ListBox control for UI display
- `logFilePath`: Optional file path for CSV logging (null to disable)
- `debugEnabled`: Whether to show debug messages (default: true)

### Configuration

The debug flag is controlled by the CSV configuration file:

```csv
debug,1,enable debug logging
```

Or via JSON (for initial startup):

```json
{
  "LogFilePath": "logs/ModbusSimulationServer.log"
}
```

### Dynamic Debug Control

The logger's debug flag can be changed at runtime:

```csharp
_logger.DebugEnabled = false;  // Disable debug messages
_logger.DebugEnabled = true;   // Enable debug messages
```

### Cleanup

The logger is automatically disposed when the form closes:

```csharp
protected override void OnFormClosing(FormClosingEventArgs e)
{
    _logger?.Dispose();
}
```

## Technical Details

### Automatic Source Detection

The logger uses `StackTrace` to automatically detect the calling method:

```csharp
var stackTrace = new StackTrace();
var frame = stackTrace.GetFrame(1);
var method = frame?.GetMethod();
var source = method != null ? $"{method.DeclaringType?.Name}.{method.Name}" : "Unknown";
```

This provides the format: `ClassName.MethodName`

### Debug Filtering Implementation

```csharp
private void LogInternal(string source, string message, MessageType messageType)
{
    // Skip debug messages if debug is disabled
    if (messageType == MessageType.Debug && !DebugEnabled)
        return;
    
    // ... rest of logging logic
}
```

Debug messages are filtered **before** queueing, so they don't consume any resources when disabled.

### Performance

- **Buffer Size**: Max 500 queued messages (older messages dropped if exceeded)
- **UI Update Interval**: 100ms
- **Max ListBox Items**: 1000 (auto-scrolling to bottom)
- **File Writing**: Asynchronous (non-blocking)
- **Debug Filtering**: Zero overhead when disabled (messages not queued)

### Date Separators

The logger automatically inserts date separator headers when the date changes:

```
===============   2024-01-15   =======================
```

## Best Practices

1. **Log all important operations**: Use `Info` level
2. **Log all errors**: Use `Error` level with full exception details
3. **Log minor operations**: Use `Debug` level
4. **Use automatic source detection**: Let the logger determine the calling method
5. **Keep messages concise**: Avoid multi-line messages in single log calls
6. **Always dispose**: The logger is disposed in `OnFormClosing`
7. **Use debug flag wisely**: Enable for development, disable for production

## Examples

```csharp
// Application lifecycle (always logged)
_logger.Log("Application started", Logger.MessageType.Info);

// Configuration details (only when debug enabled)
_logger.Log("Configuration loaded", Logger.MessageType.Debug);

// Operations (always logged)
_logger.Log("Starting Modbus server on port 502", Logger.MessageType.Info);

// Detailed diagnostics (only when debug enabled)
_logger.Log($"Column {i}: {col.Description}", Logger.MessageType.Debug);

// Errors (always logged)
try 
{
    // Some operation
}
catch (Exception ex)
{
    _logger.Log($"Operation failed: {ex.Message}", Logger.MessageType.Error);
}
```

## Debug Mode Example

**With `debug = 1` in CSV:**
```
14:23:45.123	T001	INFO	MainForm.Load	Application started
14:23:45.234	T001	DEBUG	Parser.Parse	Parsing column 0
14:23:45.345	T001	INFO	Parser.Parse	Data loaded: 1007 rows
14:23:45.456	T001	DEBUG	Parser.Parse	Column 0: Ignored
14:23:45.567	T001	ERROR	Server.Start	Failed to bind port
```

**With `debug = 0` in CSV:**
```
14:23:45.123	T001	INFO	MainForm.Load	Application started
14:23:45.345	T001	INFO	Parser.Parse	Data loaded: 1007 rows
14:23:45.567	T001	ERROR	Server.Start	Failed to bind port
```

Notice: All DEBUG messages are filtered out when debug is disabled.

## Display Format

The tab-separated format ensures columns align properly:

```
14:23:45.123	T001	INFO	MainForm.Form1_Load	Application started
14:23:45.234	T001	DEBUG	MainForm.LoadConfig	Configuration loaded
14:23:45.345	T005	ERROR	ModbusServer.Start	Failed to bind port 502
```

Benefits:
- Time column always aligned
- Thread ID always aligned
- Message type always aligned
- Source names can vary in length without affecting message alignment
- Easy to scan visually

## Files

- **Logger.cs**: Main logging implementation with debug filtering
- **AppConfiguration.cs**: Configuration management
- **config/app_config.json**: Initial configuration file
- **MainForm.cs**: Logger initialization and usage example
