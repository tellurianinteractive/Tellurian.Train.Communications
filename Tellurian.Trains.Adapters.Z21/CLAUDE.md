# Tellurian.Trains.Adapters.Z21

This project provides the **Z21 Adapter** - a protocol adapter for Z21 command stations that wraps XpressNet and LocoNet protocols.

## Purpose

The Z21 adapter acts as a bridge between the protocol-agnostic interfaces (`ILoco`, `IAccessory`, `ISwitch`, `IDecoder`) and the Z21 command station hardware. It handles Z21-specific framing and protocol encapsulation while delegating actual protocol work to XpressNet implementations.

## Implementation Status

✅ **Fully Implemented** - Production-ready with comprehensive Z21 support.

## Key Components

### Adapter
Main adapter class (split across partial classes) that:
- Implements `ILoco`, `IAccessory`, `ISwitch`, and `IDecoder` interfaces
- Wraps XpressNet protocol handlers
- Manages Z21 frame encapsulation/decapsulation
- Distributes notifications to multiple observers
- Handles broadcast subscription configuration

### Interface Implementations

#### ILoco (LocoControlAdapter.cs)
- `DriveAsync` - Speed and direction control
- `EmergencyStopAsync` - Per-locomotive emergency stop
- `SetFunctionAsync` - Function control (F0-F28)

#### IAccessory & ISwitch (AccessoryControlAdapter.cs)
- `SetAccessoryAsync` - Generic accessory control via XpressNet AccessoryFunctionCommand
- `QueryAccessoryStateAsync` - Query accessory state via AccessoryInfoRequestCommand
- `SetThrownAsync`/`SetClosedAsync`/`TurnOffAsync` - Convenience methods

#### IDecoder (DecoderControlAdapter.cs)
- `ReadCVAsync` - CV read with async response handling
- `WriteCVAsync` - CV write with async response handling
- Uses TaskCompletionSource for async request/response correlation

### Frame Handling
Z21 uses a specific frame structure that wraps protocol messages:

```
[Length (2 bytes, little-endian)] [Header (2 bytes)] [Data (n bytes)]
```

The adapter automatically:
- Adds Z21 framing when sending commands
- Strips Z21 framing when receiving notifications
- Routes data to appropriate protocol handler

### Notification Mapping
Maps Z21 frames to protocol-agnostic notifications:
- System state changes → interface notification types
- Locomotive information → `LocoMovementNotification` / `LocoFunctionsNotification`
- Programming results → `DecoderResponse`
- Accessory changes → `AccessoryNotification`

### Z21-Specific Commands
Beyond protocol passthrough, provides Z21-specific functionality:
- **System queries**: Serial number, hardware info, firmware version
- **System state**: Voltage, current, temperature monitoring
- **Broadcast subscriptions**: Configurable event filtering via `BroadcastSubjects`
- **Hardware detection**: Identifies Z21 variant (old/new, SmartRail, z21, smart/start)

## Z21 Frame Structure Details

### Frame Format
```
Byte 0-1: Length (little-endian, includes length field itself)
Byte 2-3: Header (identifies command/notification type)
Byte 4+:  Data (protocol-specific payload)
```

### Encapsulation
- **XpressNet commands**: Wrapped in header `0x40 0x00`
- **LocoNet commands**: Wrapped in header `0x60 0x00` (send) / `0x61 0x00` (receive)
- **Z21 commands**: Direct Z21 headers (e.g., `0x10 0x00` for system state)

## Observer Pattern

The adapter supports multiple observers:
- Each observer receives all notifications
- Notifications delivered on background threads (thread-safety required)
- Observers can be added/removed at runtime
- Thread-safe observer management

## Testing

The `Tellurian.Trains.Adapters.Z21.Tests` project validates:
- Z21 frame encoding/decoding
- Protocol routing (XpressNet vs LocoNet)
- Notification factory message parsing
- Broadcast subscription management
- Observer notification delivery

## Usage Notes

- The adapter requires an `ICommunicationsChannel` (typically `UdpDataChannel`)
- Z21 hardware communicates via UDP on port 21105
- Default Z21 IP is typically 192.168.0.111 (check your hardware)
- Configure broadcast subscriptions early to avoid missing events
- Dispose the adapter to cleanly disconnect and release resources
- The adapter is thread-safe for concurrent operations
