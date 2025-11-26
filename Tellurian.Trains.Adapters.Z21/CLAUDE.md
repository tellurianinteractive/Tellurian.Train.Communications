# Tellurian.Trains.Adapters.Z21

This project provides the **Z21 Adapter** - a protocol adapter for Z21 command stations that wraps XpressNet and LocoNet protocols.

## Purpose

The Z21 adapter acts as a bridge between the protocol-agnostic interfaces (`ILocoControl`, `ILocoDecoder`) and the Z21 command station hardware. It handles Z21-specific framing and protocol encapsulation while delegating actual protocol work to XpressNet and LocoNet implementations.

## Implementation Status

✅ **Fully Implemented** - Production-ready with comprehensive Z21 support.

## Key Components

### Z21Adapter
Main adapter class that:
- Implements `ILocoControl` and `ILocoDecoder` interfaces
- Wraps XpressNet and LocoNet protocol handlers
- Manages Z21 frame encapsulation/decapsulation
- Distributes notifications to multiple observers
- Handles broadcast subscription configuration

### Frame Handling
Z21 uses a specific frame structure that wraps protocol messages:

```
[Length (2 bytes, little-endian)] [Header (2 bytes)] [Data (n bytes)]
```

The adapter automatically:
- Adds Z21 framing when sending commands
- Strips Z21 framing when receiving notifications
- Routes data to appropriate protocol handler (XpressNet or LocoNet)

### Notification Mapping
Maps Z21 frames to protocol-agnostic notifications:
- System state changes → interface notification types
- Locomotive information → `ILocoControl` event notifications
- Programming results → `ILocoDecoder` event notifications
- Error conditions → appropriate error notifications

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

### NotificationFactory
Parses incoming Z21 frames:
1. Extracts length and header
2. Validates frame completeness
3. Routes to appropriate protocol parser based on header
4. Creates typed notification objects
5. Distributes to registered observers

## Broadcast Subscriptions

The Z21 supports selective event filtering via `BroadcastSubjects`:
- Power status changes
- Locomotive information updates
- Turnout/accessory changes
- Sensor/feedback events
- System state changes

Configure subscriptions to reduce network traffic and processing overhead.

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
