# Z21 LAN Protocol Documentation

This folder contains comprehensive markdown documentation for the Z21 LAN Protocol, converted from the official PDF specification (Version 1.13, November 6, 2023).

## Documentation Files

### Core Protocol Documentation

1. **[Z21-Legal-Notices.md](Z21-Legal-Notices.md)**
   - Legal information, disclaimers, and liability exclusions
   - Trademark acknowledgments
   - Publishing information

2. **[Z21-Basics.md](Z21-Basics.md)**
   - Protocol fundamentals and communication overview
   - UDP port configuration (21105/21106)
   - Z21 packet structure (DataLen, Header, Data)
   - X-BUS protocol tunneling with XOR checksums
   - LocoNet protocol tunneling (FW 1.20+)
   - Combining multiple datasets in one UDP packet
   - Little-endian byte ordering

3. **[Z21-System-Status.md](Z21-System-Status.md)**
   - Serial number and login/logoff
   - Version information (X-Bus, firmware, hardware)
   - Track power control (on/off, emergency stop)
   - Broadcast flag configuration
   - System status monitoring (current, voltage, temperature)
   - Hardware identification and capabilities
   - Unlock code management (z21start)

4. **[Z21-Settings.md](Z21-Settings.md)**
   - Persistent locomotive format settings (DCC/MM)
   - Persistent accessory decoder format settings
   - Address encoding (big-endian for settings)
   - Format conversion helpers
   - Factory reset procedure

### Locomotive and Accessory Control

5. **[Z21-Driving.md](Z21-Driving.md)**
   - Locomotive subscription (max 16 per client)
   - Address encoding for short/long addresses
   - Speed control (14/28/128 speed steps)
   - Direction control
   - Function control (F0-F68, depending on firmware)
   - Function groups for batch operations
   - Binary state control (FW 1.42+)
   - Emergency stop and purge commands
   - Locomotive information broadcasts

6. **[Z21-Switching.md](Z21-Switching.md)**
   - Turnout/accessory decoder control
   - Address visualization differences (ESU, Roco, Twin Center, Zimo)
   - DCC address encoding and port mapping
   - Queue mode (Q=0 immediate, Q=1 queued - FW 1.24+)
   - Extended accessory decoders (FW 1.40+)
   - Z21 Switch Decoder (10836) - switching times
   - Z21 Signal Decoder (10837) - signal aspects
   - RCN-213 compliance

### Advanced Features

7. **[Z21-LocoNet.md](Z21-LocoNet.md)**
   - Ethernet/LocoNet gateway functionality (FW 1.20+)
   - Z21 as LocoNet master
   - Message forwarding (RX, TX, FROM_LAN)
   - Locomotive dispatch for handsets
   - Track occupancy detectors
   - Simplified detector API (FW 1.22+)
   - Transponder support (Blücher GBM16XN with RailCom)
   - LISSY support (loco address, block status, speed - FW 1.23+)
   - Network traffic considerations

8. **[Z21-Command-Reference.md](Z21-Command-Reference.md)**
   - Complete command overview (67 Client→Z21, 42 Z21→Client)
   - Device compatibility matrix for all Z21/zLink devices
   - Header reference with hex values
   - Command support indicators (✓/✗)
   - Firmware version requirements
   - Special notes and limitations
   - C# enumeration definitions

## Documentation Features

### ✅ Standard Detail Level
- Important details with clear explanations
- Omits only redundant or obvious information
- Focuses on practical usage and core concepts

### ✅ Markdown Tables
- All diagrams converted to well-formatted tables
- Byte layouts clearly structured
- Command structures easy to reference

### ✅ Valid C# Code
- All code examples use modern C# (.NET 10.0)
- Collection expressions (`[1, 2, 3]`)
- Required properties
- File-scoped namespaces
- Async/await patterns
- Extension methods and helper classes

### ✅ Conceptual Explanations
- Address encoding quirks explained
- Queue mode benefits and drawbacks
- Network traffic warnings
- Manufacturer-specific differences
- Firmware version dependencies

### ✅ Complete Examples
- Full C# implementation examples
- Helper classes for common operations
- Parsing and serialization code
- Error handling patterns
- Real-world usage scenarios

### ✅ Cross-References
- Links between related sections
- Forward references to detailed explanations
- Backward references to prerequisites

## Quick Reference

### Key Protocol Facts

- **Transport**: UDP on ports 21105 (primary) or 21106
- **Byte Order**: Little-endian (except loco/turnout settings which use big-endian)
- **Packet Structure**: `[DataLen (2)] [Header (2)] [Data (n)]`
- **Keep-Alive**: Minimum once per minute or client is disconnected
- **Max UDP Payload**: 1472 bytes (Ethernet MTU minus IP/UDP headers)
- **X-BUS Tunnel**: Header 0x40, includes XOR checksum
- **LocoNet Tunnel**: Headers 0xA0 (RX), 0xA1 (TX), 0xA2 (FROM_LAN)

### Supported Devices

| Device Type | Models | Full Support |
|-------------|--------|--------------|
| **Z21** | Z21, Z21 XL | ✓ All commands |
| **z21** | z21 (white) | ✓ Most commands |
| **z21 start** | z21 start | ⚠️ Limited (needs activation code 10814/10818) |
| **Boosters** | 10806, 10807, 10869 | ✓ Booster-specific commands |
| **Decoders** | 10836, 10837 | ✓ Decoder-specific commands |

### Command Categories

- **System**: 4 commands (serial, code, hardware info, logoff)
- **Track Power**: 4 commands (on, off, status, emergency stop)
- **Driving**: 8 commands (speed, direction, functions F0-F68, binary state)
- **Switching**: 6 commands (basic turnouts, extended accessories)
- **CV Programming**: 11 commands (service mode, POM for locos/accessories)
- **Broadcast**: 2 commands (set/get flags)
- **Settings**: 4 commands (loco/turnout format persistence)
- **System State**: 2 commands (get/broadcast)
- **Feedback**: 5 commands (R-Bus, RailCom)
- **LocoNet**: 5 commands (gateway, dispatch, detectors)
- **CAN-Bus**: 4 commands (detectors, device management)
- **Fast Clock**: 3 commands (control, data, settings)
- **Booster**: 4 commands (zLink boosters)
- **Decoder**: 3 commands (zLink decoders)

## Usage Notes

### Network Traffic Warnings

⚠️ **High-Traffic Broadcast Flags** (use with caution):
- `0x00010000` - All loco info without subscription (PC automation only)
- `0x00040000` - All RailCom data without subscription (PC automation only)
- `0x02000000` - LocoNet locomotive messages (gateway)
- `0x04000000` - LocoNet switch messages (gateway)
- `0x08000000` - LocoNet detector messages (gateway)

**Impact**: UDP packets may be dropped by routers during overload. Not suitable for mobile controllers.

### Best Practices

✅ **DO:**
- Use queue mode (Q=1) for turnout switching (FW 1.24+)
- Subscribe to specific broadcasts you need
- Maintain keep-alive (at least once per minute)
- Use native Z21 commands over LocoNet when possible
- Handle asynchronous broadcasts properly
- Validate firmware version before using newer commands

❌ **DON'T:**
- Enable unnecessary broadcast flags
- Mix Q=0 and Q=1 switching modes
- Forget to deactivate MM turnouts (no automatic shut-off)
- Assume address encoding is always little-endian
- Use high-traffic flags on mobile controllers
- Poll excessively (causes network congestion)

### Common Pitfalls

1. **Address Encoding Confusion**
   - Most commands: little-endian
   - Loco/turnout settings: **big-endian**
   - Loco addresses >= 128: need high bits set (0xC0 | MSB)

2. **Turnout Address "Shift by 4"**
   - ESU: Display #1 = DCC Addr 1 Port 0
   - Roco/Z21: Display #1 = DCC Addr 0 Port 0
   - Same physical turnout, different numbers!

3. **Speed Step Encoding**
   - DCC 14: Special encoding with E-Stop at 0x01
   - DCC 28: V5 bit interleaved in bit 4
   - DCC 128: Straightforward 1-126 encoding

4. **Function Refresh Behavior**
   - F0-F12: Sent periodically
   - F13+: Sent 3 times, then stopped until next change (RCN-212)

5. **LocoNet Virtual Stack**
   - z21/z21start: No physical LocoNet, but virtual stack exists
   - Can work with GBM16XN via XPN interface
   - High network traffic on gateway mode

## Implementation Checklist

### Minimum Required Implementation

- [ ] UDP socket on port 21105
- [ ] Packet parsing (DataLen, Header, Data)
- [ ] Keep-alive mechanism (60-second timer)
- [ ] X-BUS checksum calculation
- [ ] Basic broadcast handling (0x00000001 minimum)
- [ ] Little-endian serialization/deserialization
- [ ] Loco address encoding (short/long)
- [ ] Speed encoding (at least 128-step mode)

### Recommended Implementation

- [ ] All basic commands (power, status, version)
- [ ] Locomotive control (drive, functions F0-F28)
- [ ] Turnout control with queue mode
- [ ] System state monitoring
- [ ] Proper logoff on disconnect
- [ ] Error handling for unknown commands
- [ ] Firmware version detection
- [ ] Device capability detection (Capabilities bitmask)

### Advanced Implementation

- [ ] LocoNet gateway support
- [ ] Track occupancy detectors
- [ ] RailCom support
- [ ] CV programming (service mode + POM)
- [ ] Extended accessory decoders
- [ ] Binary state control (F29-F32767)
- [ ] Fast clock support
- [ ] CAN-Bus integration
- [ ] Multi-client synchronization

## Firmware Version Dependencies

| Feature | Minimum FW Version |
|---------|-------------------|
| Basic commands | 1.00 (all devices) |
| LocoNet gateway | 1.20 |
| LocoNet detectors | 1.22 |
| LISSY support | 1.23 |
| Queue mode (Q=1) | 1.24 |
| Binary state control | 1.42 |
| Extended functions F29-F31 | 1.42 |
| Emergency stop per loco | 1.43 |
| Purge loco | 1.43 |
| MM format indication | 1.43 |

## References

### Official Documentation
- **Source**: Z21 LAN Protocol Specification Version 1.13 (English)
- **Date**: November 6, 2023
- **Publisher**: Roewe Modelleisenbahn GmbH, Plainbachstrasse 4, A-5101 Bergheim, Austria

### Standards Referenced
- **RP-9.2.1**: NMRA DCC Accessory Decoder specifications
- **RCN-212**: Function refresh behavior (F13+ sent 3 times)
- **RCN-213**: Extended accessory decoder format, binary state addressing
- **NMRA S-9.2.1**: Feature Expansion Instruction (Binary State Control)

### Trademarks
- RailCom, XpressNet: Lenz Elektronik GmbH
- LocoNet: Digitrax, Inc.
- Motorola: Motorola Inc., Tempe-Phoenix, USA

## File Organization

```
Specifications/
├── README.md (this file)
├── Z21-Legal-Notices.md
├── Z21-Basics.md
├── Z21-System-Status.md
├── Z21-Settings.md
├── Z21-Driving.md
├── Z21-Switching.md
├── Z21-LocoNet.md
└── Z21-Command-Reference.md
```

## Contributing

When updating this documentation:
1. Maintain consistent C# code style (modern .NET 10.0 features)
2. Use markdown tables for structured data
3. Include practical examples with complete, runnable code
4. Cross-reference related sections
5. Note firmware version requirements
6. Update the command reference for new commands
7. Preserve the detail level (standard, not minimal or exhaustive)

## Version History

- **Version 1.13** (November 6, 2023): Latest official specification
- **Markdown conversion**: Created comprehensive C# documentation with examples

---

**Document processed**: All 8 PDF files successfully converted to markdown
**Total commands documented**: 109 (67 requests + 42 responses)
**Code examples**: Valid C# with .NET 10.0 features
**Cross-references**: Extensive linking between sections
**Device compatibility**: Complete matrix for all Z21/zLink devices
