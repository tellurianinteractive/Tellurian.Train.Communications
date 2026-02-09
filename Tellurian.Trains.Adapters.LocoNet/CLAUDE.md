# Tellurian.Trains.Adapters.LocoNet

This project provides the **LocoNet Adapter** - a protocol adapter for LocoNet command stations via serial port.

## Purpose

The LocoNet adapter acts as a bridge between the protocol-agnostic interfaces (`ILoco`, `IAccessory`, `ITurnout`, `IDecoder`) and LocoNet hardware. It handles the slot-based architecture of LocoNet while presenting a simple address-based API.

## Implementation Status

✅ **Implemented** - Core functionality with slot management.

## Key Components

### Adapter
Main adapter class that:
- Implements `ILoco`, `IAccessory`, `ITurnout`, and `IDecoder` interfaces
- Manages LocoNet slot allocation and caching
- Distributes notifications to multiple observers
- Handles serial port framing via `SerialDataChannel` and `LocoNetFramer`

### Slot Management
LocoNet uses 120 slots (0-119) for locomotive control:
- **GetLocoAddressCommand**: Requests a slot for a locomotive address
- **SlotData**: Contains complete locomotive state
- Slot assignments are cached to avoid repeated requests

### Interface Implementations

#### ILoco (LocoControlAdapter.cs)
- `DriveAsync`: Sets speed and direction (requires slot)
- `EmergencyStopAsync`: Emergency stop via slot
- `SetFunctionAsync`: Function F0-F12 control

#### IAccessory & ITurnout (AccessoryControlAdapter.cs)
- `SetAccessoryAsync`: Generic accessory control
- `QueryAccessoryStateAsync`: Query switch state
- `SetThrownAsync`/`SetClosedAsync`/`TurnOffAsync`: Convenience methods

#### IDecoder (DecoderControlAdapter.cs)
- `ReadCVAsync`: Service mode CV read (uses slot 124)
- `WriteCVAsync`: Service mode CV write (uses slot 124)

## Notification Flow

1. `SerialDataChannel` receives bytes from serial port
2. `LocoNetFramer` assembles complete messages
3. `Adapter.ReceiveData` parses messages
4. `ProcessNotification` updates caches and pending requests
5. `MapToInterfaceNotification` converts to interface types
6. Observers receive interface notifications

## Testing

Integration tests require actual LocoNet hardware connected via serial port.

## Usage Notes

- Serial port settings: typically 57600 baud, 8N1
- The adapter caches slot assignments for efficiency
- Programming operations use slot 124
- Dispose the adapter to cleanly release resources
