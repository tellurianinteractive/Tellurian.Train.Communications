# Tellurian.Trains.Protocols.XpressNet

This project provides a **complete implementation** of the XpressNet protocol used by Lenz and Z21 command stations.

## Protocol Overview

XpressNet is a serial bus protocol for digital model railroad control. This implementation handles the byte-level protocol encoding/decoding for all XpressNet commands and notifications.

## Implementation Status

✅ **Fully Implemented** - Production-ready with comprehensive feature coverage.

## Key Components

### Commands
Protocol commands that translate high-level operations to XpressNet byte sequences:
- **Locomotive Control**: Speed, direction, function control (F0-F28)
- **Track Power**: Power on/off, emergency stop
- **Accessory Control**: Turnout/accessory control (addresses 1-1024)
- **Programming**: CV read/write operations (CVs 1-1024)
- **System Queries**: Version info, status requests

### Notifications
Protocol notifications parsed from incoming XpressNet messages:
- **System State**: Power status, short circuit, emergency stop
- **Locomotive Info**: Address, speed, direction, function states
- **Programming Responses**: CV values, acknowledgments, errors
- **Broadcasts**: Unsolicited status updates

### Message/Packet Structure
- **`Message`**: Base class for all XpressNet messages
- **`Command`**: Inherits from `Message`, represents commands sent to the station
- **`Notification`**: Inherits from `Message`, represents responses/broadcasts from the station
- **`Packet`**: Wrapper that handles XOR checksum calculation and validation

## Protocol Details

### Checksum
- All packets require **XOR checksum** (handled automatically by `Packet` class)
- Checksum is XOR of all bytes (header + data)
- Invalid checksums are rejected automatically

### Message Structure
```
[Header byte] [Data bytes...] [Checksum]
```

### Speed Steps
Supports multiple speed step modes:
- 14 speed steps (legacy)
- 27 speed steps
- 28 speed steps (common)
- 126 speed steps (high resolution)

### Address Ranges
- **Short addresses**: 1-127 (single byte)
- **Long addresses**: 128-9999 (two bytes, big-endian)

### Functions
- **F0-F28** supported across multiple command message types
- Functions grouped for encoding efficiency (F0-F4, F5-F8, F9-F12, F13-F20, F21-F28)

### CV Range
- Configuration Variables: **1-1024**
- Both service mode (programming track) and operations mode (main track) supported

## Testing

The `Tellurian.Trains.Protocols.XpressNet.Tests` project validates:
- Command byte generation matches protocol specification
- Notification parsing handles all message types correctly
- Checksum calculation and validation
- Edge cases (invalid addresses, out-of-range values)

## Usage Notes

- All commands automatically calculate checksums via `GetBytesWithChecksum()`
- Notification parsing is lazy - only create notifications for messages you care about
- Use `NotificationFactory` to parse incoming byte arrays into notification objects
- Commands are immutable - create new instances for each operation
