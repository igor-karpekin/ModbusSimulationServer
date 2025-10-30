# CSV Configuration Format

## File Structure

The CSV configuration file must follow this structure:

### 1. Header Line (Required)
```csv
# simulation server configuration file
```
The first line **must** start with this exact text (case-insensitive).

### 2. Global Parameters Section
After the header, define global parameters until the `#VALUE LOG` marker.

Each parameter line format:
```
parameter_name,value,optional description
```

**Note:** The value is in the **second column** (index 1).

### Supported Parameters

| Parameter | Type | Valid Range | Default | Description |
|-----------|------|-------------|---------|-------------|
| `version` | string | ? 1.2 | 1 | Configuration version (must be ? app version) |
| `port` | int | 1-65535 | 502 | Modbus TCP server port |
| `loops` | int | 0+ | 1 | Replay count (0 = infinite loop) |
| `logfile` | string | - | none | Log file path (relative or absolute) |
| `debug` | bool | 0/1, true/false | true | Enable/disable debug messages |

### 3. Value Log Section Marker
```csv
#VALUE LOG
```
This marks the end of the global parameters section and the start of the data section.

### 4. Data Section (Not yet implemented)
Will contain the register values to publish.

## Example Configuration

```csv
# simulation server configuration file,,,,,,,
version,1,shall be less or equal the application version
step delay, ms,1000,delay before the new row will be published
port,502,
loops,0,infinite replay
logfile,logs/modbus_server.log,
debug,1,enable debug logging
#VALUE LOG,,counter,sin(),random(),speed,vib
step multiplier,delay,v1,v2,v3,v4,v5
20,20000,0,0,3.05,0,0
,1000,1,0.0299995,3.16,1,0
```

## Version Compatibility

The configuration `version` parameter is checked against the application version constant `CFG_VERSION = "1.2"`.

Version format: `major.minor.patch`
- Examples: `1`, `1.1`, `1.2`, `1.2.a`
- Configuration version must be ? application version
- Major version must match
- Minor version must be ? app minor version

## Relative Paths

If `logfile` is specified as a relative path, it will be created relative to the executable directory.

Examples:
- `logs/app.log` ? `{exe_dir}/logs/app.log`
- `../logs/app.log` ? `{exe_dir}/../logs/app.log`
- `C:/logs/app.log` ? Absolute path (used as-is)

## Boolean Values

The `debug` parameter accepts:
- **True**: `1`, `true`, `yes`
- **False**: `0`, `false`, `no`

## Empty Values

- If a parameter line has an empty first column, the line is ignored
- If a parameter value (column 2) is empty, default value is used
- Unknown parameters are stored in `AdditionalParameters` dictionary

## Error Handling

The parser will throw exceptions for:
- ? File not found
- ? Missing header line
- ? Invalid CSV format
- ? Invalid parameter values

Validation errors:
- ? Version incompatibility
- ? Port out of range (1-65535)
- ? Negative loops value

All errors are logged and displayed to the user.
