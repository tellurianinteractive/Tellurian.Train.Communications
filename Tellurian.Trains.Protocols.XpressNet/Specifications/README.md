# XpressNet Protocol Specification

This folder contains the complete XpressNet Protocol Description documentation from **Lenz Elektronik GmbH**.

## Chapters

1. [**Chapter 1: General**](XpressNet%20-%2001%20General.md)
   - XpressNet protocol overview
   - XpressNet architecture and topology
   - LI100/LI100F/LI101F computer interface specifications
   - Timing constraints and wiring requirements
   - Version determination and future expansion

2. [**Chapter 2: Communications between the DCC Command Station and XpressNet Devices**](XpressNet%20-%2002%20Communication%20betwen%20the%20DCC%20Command%20Station%20and%20XpressNet%20Devices.md)
   - Communication protocol fundamentals
   - Call bytes and transmission windows
   - Packet structure and XOR error detection

3. [**Chapter 3: Data responses from Command Station to XpressNet Devices**](XpressNet%20-%2003%20Data%20responses%20from%20Command%20Station%20to%20XpressNet%20Devices.md)
   - Normal inquiry and acknowledgements
   - Broadcast messages (power, emergency stop, service mode)
   - Service mode programming responses (Register, Paged, Direct CV modes)
   - Software version and status responses
   - Locomotive information responses (X-Bus V1, V2, XpressNet)
   - Accessory decoder responses
   - Double Header and Multi-unit responses
   - Error messages

4. [**Chapter 4: Data messages from XpressNet Device to Command Station**](XpressNet%20-%2004%20Data%20messages%20from%20XpressNet%20Device%20to%20Command%20Station.md)
   - Acknowledgement and operation control requests
   - Emergency stop commands (global and specific locomotives)
   - Service mode programming requests (Register, Paged, Direct CV modes)
   - Command station status and version requests
   - Locomotive information and operation requests (speed, direction, functions)
   - Accessory decoder operations
   - Double Header operations (establish/dissolve)
   - Multi-unit operations (add/remove locomotives)
   - Operations Mode programming (Programming on Main)
   - Address search operations
   - Command station stack management

5. [**Chapter 5: Messages Overview**](XpressNet%20-%2005%20Messages%20Overview.md)
   - Complete command reference tables
   - Responses sent to XpressNet devices (Chapter 3 summary)
   - Requests sent from XpressNet devices (Chapter 4 summary)
   - Quick reference for all message formats

## Protocol Versions

The documentation covers three protocol generations:

- **X-Bus V1**: Original protocol with 14 speed steps, addresses 0-99
- **X-Bus V2**: Extended protocol with 14/27/28 speed steps, addresses 0-99
- **XpressNet**: Modern protocol with 14/27/28/128 speed steps, addresses 0-9999, enhanced features

## Key Features

- **Speed Step Modes**: 14, 27, 28, and 128 speed steps
- **Locomotive Addressing**: Short (1-127) and long (128-9999) addresses
- **Function Control**: F0-F12 (with potential for F13-F28)
- **Service Mode Programming**: Register, Paged, and Direct CV modes (CV1-CV1024)
- **Operations Mode Programming**: Programming on Main track (CV1-CV1024)
- **Consist Support**: Double Headers and Multi-unit consists
- **Accessory Control**: Turnouts and accessories (addresses 1-1024)
- **Feedback**: Feedback module support
- **Error Detection**: XOR checksum on all packets

## Implementation Notes

- All packets require XOR checksum (handled automatically by `Packet` class in the implementation)
- Only one request can be processed per "Normal inquiry" call byte
- Command station manages bandwidth allocation among XpressNet devices
- Not all command stations support all instructions
- Version 3.x command stations required for full XpressNet feature support

## Publisher

**Lenz Elektronik GmbH**

---

*Note: This documentation has been converted from the original PDF specifications for easier reference and integration with the Tellurian.Trains.Protocols.XpressNet library.*
