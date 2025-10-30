# CSV Data Section Format

## Overview

The data section starts after the `#VALUE LOG` marker and contains column definitions and data rows for Modbus register values.

## Structure

```
#VALUE LOG
[Description Line - Column 1],[Description Line - Column 2],...
[Assignment Line - Column 1],[Assignment Line - Column 2],...
[Data Row 1 - Value 1],[Data Row 1 - Value 2],...
[Data Row 2 - Value 1],[Data Row 2 - Value 2],...
```

## Header Lines

### Line 1: Column Descriptions
Human-readable descriptions for each column. These will be displayed as tooltips when hovering over columns in the grid view.

### Line 2: Address Assignments
Defines what each column represents. Can be:

#### Special Columns
- **DeltaTime** or **Delay**: Time in milliseconds to wait after publishing this row before moving to the next
- **Ref**, **ID**, or **Tag**: Reference identifier for the row (must be unique)
- **Empty**: Column is ignored

#### Modbus Address Assignments
Format: `[AddressSpace][Address][DataType]`

**Address Spaces:**
- `H` - Holding Register (read/write)
- `I` - Input Register (read-only)
- `C` - Coil (read/write bit)
- `D` - Discrete Input (read-only bit)

**Data Types:**
- `U` - Unsigned integer (for H, I)
- `S` - Signed integer (for H, I)
- `F` - Float (for H, I)
- `B` - Bit (for C, D)

**Address Range:** 0-65535

### Examples

| Assignment | Description |
|------------|-------------|
| `H100U` | Holding register at address 100, unsigned integer |
| `I200S` | Input register at address 200, signed integer |
| `H300F` | Holding register at address 300, float |
| `C50B` | Coil at address 50, bit |
| `D1000B` | Discrete input at address 1000, bit |
| `DeltaTime` | Delay column |
| `Ref` | Reference/ID column |
| (empty) | Ignored column |

## Data Rows

Each row contains values for the defined columns:

### DeltaTime Column
- Integer value in milliseconds
- Delay before moving to next row
- If empty, uses 0 (immediate)

### Ref Column
- String identifier
- Must be unique across all rows
- Optional

### Modbus Value Columns
- Values are converted based on defined data type
- **Empty values**: Previous row's value is maintained (no change to Modbus server)
- Invalid values for data type will cause errors

## Example

```csv
#VALUE LOG,,counter,sin(),random(),speed,vib
step multiplier,delay,v1,v2,v3,v4,v5
20,20000,H100U,H200S,H300F,H400U,H500F
,1000,0,0,3.05,0,0
,1000,1,1,3.16,10,0.5
,1000,2,2,3.30,20,1.0
,1000,,,3.05,,1.5
```

### Breakdown:

**Line 1 (Descriptions):**
- Column 0: "step multiplier" (ignored - no assignment on line 2)
- Column 1: "delay" - DeltaTime column
- Column 2: "counter" - v1 description
- Column 3: "sin()" - v2 description  
- Column 4: "random()" - v3 description
- Column 5: "speed" - v4 description
- Column 6: "vib" - v5 description

**Line 2 (Assignments):**
- Column 0: (empty) - ignored
- Column 1: "delay" - DeltaTime
- Column 2: "H100U" - Holding register 100, unsigned
- Column 3: "H200S" - Holding register 200, signed
- Column 4: "H300F" - Holding register 300, float
- Column 5: "H400U" - Holding register 400, unsigned
- Column 6: "H500F" - Holding register 500, float

**Data Rows:**
- Row 1: Wait 20000ms, set all registers to specified values
- Row 2: Wait 1000ms, update registers
- Row 3: Wait 1000ms, update registers
- Row 4: Wait 1000ms, only update H300F (3.05), keep other values from row 3

## Validation Rules

### Required
? At least one DeltaTime column  
? At least one Modbus address column  
? At least one data row  

### Constraints
? No duplicate Modbus addresses  
? No duplicate Ref values  
? Address space C and D must use data type B  
? Address space H and I cannot use data type B  
? Addresses must be 0-65535  
? DeltaTime values must be valid integers  

## Value Inheritance

When a data value is empty (`,, ` in CSV), the Modbus server will:
1. Keep the previous value for that register
2. Not send any update for that register
3. Continue using that value until explicitly changed

This allows for efficient updates where only changed values are specified.

## Error Handling

All parsing errors are logged with:
- Line number
- Column index
- Specific error message
- Invalid value (if applicable)

Common errors:
- Invalid address format
- Duplicate addresses
- Data type mismatch
- Out of range address
- Invalid DeltaTime value
- Duplicate Ref values

## Implementation Notes

### Parser Classes
- `ModbusAddressAssignment` - Parses and validates address assignments
- `DataColumn` - Represents a column definition
- `DataRow` - Represents a data row with values
- `CsvDataSection` - Contains all columns and rows
- `CsvDataSectionParser` - Parses the data section from CSV

### Storage
- Column definitions stored in `CsvConfiguration.DataSection.Columns`
- Data rows stored in `CsvConfiguration.DataSection.Rows`
- Values preserved as strings until needed for Modbus conversion
