# Tellurian.Trains.Communications.Interfaces

Protocol-agnostic interfaces and types for model train control applications. Program against these abstractions to decouple your application from specific protocols (XpressNet, LocoNet, etc.).

## Interfaces

### ILocoControl

Control locomotives with speed, direction, and functions:

```csharp
public interface ILocoControl
{
    Task<bool> DriveAsync(LocoAddress address, LocoDrive drive, CancellationToken cancellationToken = default);
    Task<bool> EmergencyStopAsync(LocoAddress address, CancellationToken cancellationToken = default);
    Task<bool> SetFunctionAsync(LocoAddress address, LocoFunction locoFunction, CancellationToken cancellationToken = default);
}
```

### ILocoDecoder

Read and write decoder configuration variables (CVs):

```csharp
public interface ILocoDecoder
{
    Task<byte> ReadCVAsync(ushort number, CancellationToken cancellationToken = default);
    Task WriteCVAsync(ushort number, byte value, CancellationToken cancellationToken = default);
}
```

## Core Types

| Type | Description |
|------|-------------|
| `LocoAddress` | DCC address (short 1-127, long 128-9999) |
| `LocoDrive` | Speed and direction combined |
| `LocoSpeed` | Speed value with step mode (14/27/28/126 steps) |
| `LocoDirection` | Forward, Backward, or Unchanged |
| `LocoFunction` | Function number (F0-F28) with on/off state |
| `CV` | Configuration variable (number 1-1024, value 0-255) |

## Usage Example

```csharp
using Tellurian.Trains.Communications.Interfaces.Locos;

// Create address and drive command
var address = LocoAddress.From(3);
var drive = new LocoDrive
{
    Speed = new LocoSpeed(64, SpeedSteps.S128),
    Direction = LocoDirection.Forward
};

// Use any ILocoControl implementation
await locoControl.DriveAsync(address, drive);

// Control functions
await locoControl.SetFunctionAsync(address, new LocoFunction(0, true)); // Lights on
```

## Notifications

Subscribe to state changes via `IObservable<Notification>`:

- `LocoNotification` - Locomotive state updates
- `ShortCircuitNotification` - Track short circuit detected
- `MessageNotification` - General system messages

## Design Principles

- **Protocol independence** - No protocol-specific details in interfaces
- **Type safety** - Strong typing for addresses, speeds, functions
- **Async-first** - All operations support cancellation tokens
- **Immutability** - Value types are readonly structs
