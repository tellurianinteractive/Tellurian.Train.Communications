# Tellurian.Trains.Interfaces

This project provides the **Interface Layer** - protocol-agnostic abstractions that define the public API for client applications.

## Purpose

Defines common interfaces and data types that are independent of any specific protocol (XpressNet, LocoNet, etc.). Applications program against these interfaces, allowing protocol implementations to be swapped without code changes.

## Key Interfaces

### ILocoControl
Locomotive control interface providing:
- `Drive(LocoAddress, LocoDrive)` - Speed and direction control
- `EmergencyStop(LocoAddress)` - Per-locomotive emergency stop
- `SetFunction(LocoAddress, LocoFunction)` - Function control (lights, sound, etc.)

### ILocoDecoder
Decoder programming interface providing:
- `ReadCV(LocoAddress, int cvNumber)` - Read configuration variable
- `WriteCV(LocoAddress, int cvNumber, byte value)` - Write configuration variable

## Data Types

### Core Types
- **`LocoAddress`**: Locomotive address (short 1-127, long 128-9999)
- **`LocoDrive`**: Combined speed and direction control
- **`LocoSpeed`**: Speed value with step mode (14/27/28/126 steps)
- **`LocoDirection`**: Forward/backward/unchanged
- **`LocoFunction`**: Function number (F0-F28) with on/off state

### Decoder Types
- **`CV`**: Configuration variable with number (1-1024) and value (0-255). Protocol-agnostic struct for CV operations.
- **`DecoderResponse`**: Result of a decoder programming operation (success/failure with CV data).

### Notifications
Abstract notification types for broadcasting state changes:
- System state changes (power, emergency stop, short circuit)
- Locomotive information updates
- Programming operation results
- Accessory/turnout feedback

## Extensions

Helper methods for data conversions:
- Byte manipulation utilities
- CV address encoding/decoding
- Address range validation
- Speed step conversions

## Design Philosophy

- **Protocol independence**: No protocol-specific details leak into interfaces
- **Type safety**: Strong typing for addresses, speeds, functions
- **Immutability**: Data types are immutable records where possible
- **Validation**: Input validation at interface boundaries

## Testing

The `Tellurian.Trains.Interfaces.Tests` project validates:
- Data type conversions and validation
- Address range checks
- Extension method correctness
- Notification type hierarchies
