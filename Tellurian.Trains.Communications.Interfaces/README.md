# Tellurian.Trains.Communications.Interfaces

Protocol-agnostic interfaces and types for model train control applications. Program against these abstractions to decouple your application from specific protocols (XpressNet, LocoNet, etc.).

## Interfaces

### ILoco

Control locomotives and query their state:

```csharp
public interface ILoco
{
    Task<bool> DriveAsync(Address address, Drive drive, CancellationToken cancellationToken = default);
    Task<bool> EmergencyStopAsync(Address address, CancellationToken cancellationToken = default);
    Task<bool> SetFunctionAsync(Address address, Function locoFunction, CancellationToken cancellationToken = default);
    Task<LocoInfo?> GetLocoInfoAsync(Address address, CancellationToken cancellationToken = default);
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
| `Address` | DCC address (short 1-127, long 128-9999) |
| `Drive` | Speed and direction combined |
| `Speed` | Speed value with step mode (14/27/28/126 steps) |
| `Direction` | Forward or Backward |
| `Function` | Function number (F0-F28) with on/off state |
| `LocoInfo` | Current locomotive state (speed, direction, function states) as reported by the command station |

## Usage Example

```csharp
using Tellurian.Trains.Communications.Interfaces.Locos;

// Create address and drive command
var address = Address.From(3);
var drive = new Drive
{
    Speed = Speed.Set126(64),
    Direction = Direction.Forward
};

// Use any ILoco implementation
await loco.DriveAsync(address, drive);

// Control functions
await loco.SetFunctionAsync(address, Function.On(Functions.F0)); // Lights on

// Query current state from the command station
var info = await loco.GetLocoInfoAsync(address);
if (info is not null)
{
    Console.WriteLine($"Speed: {info.Speed.CurrentStep}, Direction: {info.Direction}");
    Console.WriteLine($"Lights: {info.FunctionStates[0]}");
}
```

## Notifications

Subscribe to state changes via `IObservable<Notification>`:

- `LocoNotification` - Locomotive state updates
- `AccessoryNotification` - Accessory/turnout state changes
- `OccupancyNotification` - Track section occupancy (occupied/free)
- `TransponderNotification` - Transponder/RailCom locomotive presence detection (entering/leaving)
- `RailComLocomotiveNotification` - RailCom/LISSY locomotive identification with direction and classification
- `ShortCircuitNotification` - Track short circuit detected
- `MessageNotification` - General system messages

## Design Principles

- **Protocol independence** - No protocol-specific details in interfaces
- **Type safety** - Strong typing for addresses, speeds, functions
- **Async-first** - All operations support cancellation tokens
- **Immutability** - Value types are readonly structs
