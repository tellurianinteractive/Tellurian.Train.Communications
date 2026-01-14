# Tellurian.Trains.Communications.Interfaces

This project provides the **Interface Layer** - protocol-agnostic abstractions that define the public API for client applications.

## Purpose

Defines common interfaces and data types that are independent of any specific protocol (XpressNet, LocoNet, etc.). Applications program against these interfaces, allowing protocol implementations to be swapped without code changes.

## Key Interfaces

### ILoco
Locomotive control interface providing:
- `DriveAsync(Address, Drive)` - Speed and direction control
- `EmergencyStopAsync(Address)` - Per-locomotive emergency stop
- `SetFunctionAsync(Address, Function)` - Function control (lights, sound, etc.)

### IAccessory
Accessory control interface providing:
- `SetAccessoryAsync(Address, AccessoryCommand)` - Set accessory state
- `QueryAccessoryStateAsync(Address)` - Query current accessory state

### ISwitch
Switch control interface (convenience methods for accessories):
- `SetThrownAsync(Address, bool)` - Set switch to thrown/diverging position
- `SetClosedAsync(Address, bool)` - Set switch to closed/straight position
- `TurnOffAsync(Address)` - Turn off switch motor

### IDecoder
Decoder programming interface providing:
- `ReadCVAsync(ushort)` - Read configuration variable value
- `WriteCVAsync(ushort, byte)` - Write configuration variable value

## Data Types

### Locomotive Types (Locos namespace)
- **`Address`**: Locomotive address (short 1-127, long 128-9999)
- **`Drive`**: Combined speed and direction control
- **`Speed`**: Speed value with step mode (14/27/28/126 steps)
- **`Direction`**: Forward/Backward
- **`Function`**: Function number (F0-F28) with on/off state
- **`Functions`**: Enum of function numbers

### Accessory Types (Accessories namespace)
- **`Address`**: Accessory address (1-2048, user-facing 1-based addressing)
- **`AccessoryCommand`**: Command with position and motor state
- **`Position`**: ClosedOrGreen / ThrownOrRed
- **`MotorState`**: On / Off

### Accessory Addressing Convention
User-facing accessory addresses are **1-based** (1-2048). Protocol implementations convert to 0-based wire addresses internally (0-2047). This matches the convention used by most DCC systems where users configure turnouts starting at address 1, not 0.

### Decoder Types
- **`CV`**: Configuration variable with number (1-1024) and value (0-255)
- **`DecoderResponse`**: Result of a decoder programming operation (success/failure with CV data)

### Notifications
Abstract notification types for broadcasting state changes:
- **`Notification`**: Base class for all notifications
- **`LocoNotification`**: Base for locomotive notifications
- **`LocoMovementNotification`**: Speed/direction changes
- **`LocoFunctionsNotification`**: Function state changes
- **`AccessoryNotification`**: Accessory/switch state changes
- **`DecoderResponse`**: Programming operation results

## Design Philosophy

- **Protocol independence**: No protocol-specific details leak into interfaces
- **Type safety**: Strong typing for addresses, speeds, functions
- **Immutability**: Data types are immutable structs/records where possible
- **Validation**: Input validation at interface boundaries
- **Async-first**: All interface methods are async for non-blocking operations

## Testing

The `Tellurian.Trains.Communications.Interfaces.Tests` project validates:
- Data type conversions and validation
- Address range checks
- Extension method correctness
- Notification type hierarchies
