# Quick Logging Reference

## Simple Usage (Recommended)

```csharp
// Information messages (important steps)
_logger.LogInfo("Server started on port 502");

// Error messages (all errors must be logged)
_logger.LogError("Failed to connect to device");

// Debug messages (minor operations, diagnostics)
_logger.LogDebug($"Processing row {rowIndex} of {totalRows}");

// Exception logging (automatic stack trace and inner exception handling)
try 
{
    // Your code
}
catch (Exception ex)
{
    _logger.LogException(ex, "Failed to process CSV file");
}
```

## Advanced Usage

```csharp
// Explicit message type
_logger.Log("Custom message", Logger.MessageType.Info);

// Explicit source specification
_logger.Log("MyCustomSource", "Custom message", Logger.MessageType.Debug);
```

## Logging Guidelines

### What to Log as INFO
- Application start/stop
- Configuration loaded
- Server started/stopped
- File operations (loaded, saved)
- Major state changes
- User actions

### What to Log as ERROR
- **ALL exceptions** (use LogException)
- Connection failures
- File not found
- Configuration errors
- Data validation failures

### What to Log as DEBUG
- Loop iterations (every N items)
- Detailed state information
- Minor operations
- Variable values during debugging
- Performance metrics

## Thread Safety

All logging methods are thread-safe. Call from any thread:

```csharp
Task.Run(() => 
{
    _logger.LogInfo("Background task started");
    // Your background work
    _logger.LogInfo("Background task completed");
});
```

## Configuration

Edit `config/app_config.json`:

```json
{
  "LogFilePath": "logs/app.log"  // or null to disable file logging
}
```

## Output

### UI (ListBox) - Tab-Separated
```
14:23:45.123	T001	INFO	MainForm.Form1_Load	Application started
14:23:45.234	T001	DEBUG	MainForm.LoadConfig	Configuration loaded
14:23:45.345	T005	ERROR	Server.Start	Failed to bind port 502
```

### File (CSV)
```
Date,Time,ThreadID,MessageType,Source,Message
2024-01-15,14:23:45.123,1,Info,"MainForm.Form1_Load","Application started"
2024-01-15,14:23:45.234,1,Debug,"MainForm.LoadConfig","Configuration loaded"
2024-01-15,14:23:45.345,5,Error,"Server.Start","Failed to bind port 502"
```

## Display Limits
- **Maximum lines**: 1000 (oldest entries removed automatically)
- **Update rate**: 100ms
- **Auto-scroll**: Always scrolls to latest entry

## Common Patterns

### Function Entry/Exit
```csharp
private void ProcessData()
{
    _logger.LogDebug("ProcessData started");
    try 
    {
        // Your code
        _logger.LogInfo("Data processed successfully");
    }
    catch (Exception ex)
    {
        _logger.LogException(ex, "ProcessData failed");
        throw;
    }
}
```

### Progress Reporting
```csharp
for (int i = 0; i < items.Count; i++)
{
    ProcessItem(items[i]);
    
    // Log every 100 items
    if (i % 100 == 0)
    {
        _logger.LogDebug($"Processed {i}/{items.Count} items");
    }
}
```

### State Changes
```csharp
_logger.LogInfo($"Server state changed: {oldState} -> {newState}");
```

## Tab-Separated Format Benefits

The log display uses tabs between fields:
- ? Columns always aligned
- ? Easy to scan visually
- ? Works with variable-length source names
- ? Clean, readable output

```
Time          Thread  Type   Source                  Message
-------------------------------------------------------------------
14:23:45.123  T001    INFO   MainForm.Load          App started
14:23:45.234  T001    DEBUG  Config.Parse           Loaded config
14:23:45.345  T005    ERROR  ModbusServer.Start     Port bind failed
