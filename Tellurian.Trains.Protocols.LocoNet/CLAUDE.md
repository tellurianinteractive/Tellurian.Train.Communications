# Tellurian.Protocols.LocoNet

This project provides a **comprehensive implementation** (~75%) of the Digitrax LocoNet Personal Use Edition 1.0 protocol.

## Protocol Overview

LocoNet is a peer-to-peer network protocol for digital model railroad control developed by Digitrax. This implementation handles byte-level protocol encoding/decoding for LocoNet commands and notifications.

## Implementation Status

✅ **Comprehensive Implementation (~75%)** - Production-ready for essential features.

See `README.md` in this folder for complete feature documentation.
See `API-REFERENCE.md` in this folder for detailed API usage examples.

## Key Features

### Implemented
- Power control (ON/OFF, emergency stop)
- Full locomotive control (speed, direction, functions F0-F8)
- Slot management (request, activate, dispatch, keep-alive)
- Switch/turnout control with feedback
- Sensor and occupancy detection
- Transponding (OPC_MULTI_SENSE 0xD0) - present/absent events with locomotive address
- LISSY/RailCom identification (OPC_LISSY_UPDATE 0xE4) - locomotive address, direction, category
- Programming track operations (all modes: paged, direct byte/bit, register, POM)
- Multi-locomotive consisting (link/unlink, member control)

### Not Implemented
- Fast clock synchronization (slot 123) - specialized feature
- Raw DCC packet operations (OPC_IMM_PACKET) - rarely used
- Advanced peer transfer operations - rarely used

## Protocol Details

### Slot-Based Architecture
- **120 slots** (0-119) for locomotive control
- **Slot 123**: Fast clock (not implemented)
- **Slot 124**: Programming track operations

### Message Structure
- **Opcode** (1 byte, MSB=1): Defines message type and length
- **Arguments** (n bytes, MSB=0): Message-specific data
- **Checksum** (1 byte): 1's complement of XOR of all bytes

### Checksum Algorithm
Different from XpressNet:
```
checksum = ~(byte1 ^ byte2 ^ ... ^ byteN) & 0xFF
```

### Operation Codes
Defined in `OperationCodes` class with length encoding:
- Bits 7: Always 1 for opcodes
- Bits 6-5: Encode message length (2, 4, 6, or variable bytes)
- Bits 4-0: Specific operation

### 14-Byte Slot Messages
Complete locomotive state in a single message:
- Address (short/long)
- Speed (0-127)
- Direction
- Functions (F0-F8)
- Slot status and consist information
- Decoder type and track status

See `SlotData.cs` for the complete structure.

### Programming
- **Service Mode**: Paged, Direct (byte/bit), Register modes via slot 124
- **Operations Mode (POM)**: CV read/write on main track with optional feedback
- **CV Range**: 1-1024 with special encoding (CVH/CVL bytes)

### Consisting
Multi-locomotive control via slot linking:
- **SL_CONUP**: Part of consist, not top
- **SL_CONDN**: Part of consist, not bottom
- Link/unlink commands manage consist membership
- Consist function commands control all members

## Testing

The `Tellurian.Protocols.LocoNet.Tests` project validates:
- Command byte generation matches specification
- Slot data parsing and encoding
- Checksum calculation and validation
- Address encoding (short and long)
- CV number encoding for programming

## Accessory Addressing

### User vs Wire Addressing
LocoNet uses **0-based wire addressing** internally, but this library uses **1-based user addressing** for consistency with how users configure their systems:

| User Address | Wire Address | Notes |
|--------------|--------------|-------|
| 1 | 0 | First address |
| 128 | 127 | Last address in first 7 bits |
| 129 | 128 | First address requiring high bits |
| 2048 | 2047 | Maximum address |

The `AccessoryAddressExtensions` class handles this conversion:
- **Encoding** (user → wire): Subtracts 1 before encoding to SW1/SW2 bytes
- **Decoding** (wire → user): Adds 1 after decoding from SW1/SW2 bytes

### Wire Encoding
Switch addresses are encoded in two bytes:
- **SW1**: Low 7 bits of wire address (bits 0-6)
- **SW2**: High 4 bits of wire address (bits 7-10) plus direction and output flags

## Usage Notes

- All messages automatically calculate checksums via `GetBytesWithChecksum()`
- Use `SlotData.FromBytes()` to parse 14-byte slot messages
- Use `MessageFactory.CreateMessage()` to parse incoming messages
- Commands use static factory methods for common operations
- Keep-alive messages must be sent every ~120 seconds for active slots
- Programming operations use slot 124 - read `SlotNotification` after programming commands

## Architecture

- **Commands**: Outgoing messages (power control, locomotive control, slot management, etc.)
- **Notifications**: Incoming messages (slot data, sensor reports, acknowledgments, etc.)
- **Data Types**: `SlotData`, `LocoAddress`, `AccessoryAddress`, status enums
- **Shared Types**: Uses `CV` struct from `Tellurian.Trains.Communications.Interfaces.Decoder` for protocol-agnostic CV operations
- **Helpers**: `CvExtensions` for LocoNet-specific CV byte encoding (CVH/CVL/DATA7), `ConsistExtensions` for complex operations
- **Factory**: `MessageFactory` for parsing incoming byte arrays
