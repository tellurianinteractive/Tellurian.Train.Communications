# Tellurian Trains Control
A .NET library for controlling model trains using Z21, LocoNet and XpressNet protocols.
This library provides a comprehensive set of tools and abstractions to facilitate communication with compatible model train controllers,
allowing applications to control train operations, accessories, programming, and system monitoring.

## Features

### Core Components
- **Protocol-Agnostic Interfaces** - Abstract interfaces for locomotive control, decoder programming, and notifications
- **UDP Communication Channel** - Generic, testable communication layer with observer pattern support
- **Multi-Protocol Support** - Work with Z21, XpressNet, and LocoNet protocols through unified interfaces

### Z21 Protocol Support ✅ Fully Implemented
The Z21 adapter provides comprehensive control over Z21 digital command stations via UDP/LAN.

**Key Capabilities:**
- System information retrieval (serial number, hardware type, firmware version)
- System state monitoring (voltage, current, temperature, emergency stop, short circuit)
- Configurable broadcast subscriptions for granular event filtering
- Support for both DCC and Märklin Motorola (MM) address modes
- XpressNet and LocoNet command passthrough
- Hardware variant detection (Z21 old/new, SmartRail, z21, z21 smart/start)

### XpressNet Protocol Support ✅ Fully Implemented
Complete implementation of the XpressNet command protocol used by Lenz and Z21 command stations.

**Locomotive Control:**
- Speed control with 14, 27, 28, or 126 speed steps
- Direction control (forward/backward)
- Function control (F0-F28)
- Emergency stop (per-locomotive or global)
- Short and long address support (1-127, 128-9999)
- Locomotive state queries

**Track Power Management:**
- Track power on/off
- Emergency stop (all locomotives)
- Short circuit detection and notification
- Power-up mode configuration (manual/automatic)

**Accessory Control:**
- Turnout/accessory control (addresses 1-1024)
- Accessory decoder information requests
- Feedback broadcast notifications
- Turnout status queries

**Programming - Service Mode:**
- Register Mode read/write (registers 1-8)
- Direct CV Mode read/write (CVs 1-256)
- Paged Mode read/write (CVs 1-256)
- Service mode result polling
- Programming status notifications (ready, busy, short circuit, timeout)

**Programming - Operations Mode (POM):**
- Byte mode write (CVs 1-1024)
- Bit mode write (CVs 1-1024)
- Full locomotive address range (1-9999)

**Double Header Operations:**
- Establish Double Header (join two locomotives)
- Dissolve Double Header
- Full address range support (1-9999)

**Multi-Unit (Consist) Operations:**
- Add locomotive to Multi-Unit (with direction control)
- Remove locomotive from Multi-Unit
- Multi-Unit address range (1-99)

**Address Search & Stack:**
- Multi-Unit member inquiry (forward/backward search)
- Multi-Unit address inquiry
- Command station stack inquiry
- Delete locomotive from stack

**Function Status:**
- Query function status (momentary vs on/off)
- Set function state for F0-F12
- Per-function momentary/on-off configuration

**System Monitoring:**
- Hardware and firmware version detection
- Command station status queries
- Broadcast subject configuration
- Transfer error detection
- Command station busy notification

See `Tellurian.Trains.Protocols.XpressNet/README.md` for detailed documentation and usage examples.

### LocoNet Protocol Support ✅ Comprehensive Implementation (~75%)
Comprehensive LocoNet protocol implementation based on Digitrax LocoNet Personal Use Edition 1.0 specification.

**Power Control:**
- Global power on/off
- Emergency stop (global and per-locomotive)
- Idle force command

**Locomotive Control:**
- Full slot-based control (120 slots)
- Speed and direction control
- Function control (F0-F8)
- Slot lifecycle management (request, activate, dispatch, keep-alive)
- Long and short address support (1-9999)

**Switch/Turnout Control:**
- Switch control with direction (throw/close)
- Switch state requests and feedback
- Output state control (on/off)

**Sensor and Feedback:**
- Sensor input notifications (128 sensor blocks)
- Occupancy detection
- Switch report notifications

**Programming:**
- Service mode programming (paged, direct byte/bit, register modes)
- Operations mode programming (POM) with optional feedback
- CV read/write operations (CVs 1-1024)
- Programming track (slot 124) support

**Advanced Features:**
- Multi-locomotive consisting (link/unlink, member control)
- Consist function control
- Checksum validation and acknowledgments

**Not Implemented:**
- Fast clock synchronization (slot 123)
- Raw DCC packet operations (OPC_IMM_PACKET)
- Advanced peer transfer operations

See `Tellurian.Protocols.LocoNet/README.md` for detailed documentation and usage examples.

### Communication Features
- **Asynchronous Operations** - Non-blocking send/receive with async patterns
- **Observer Pattern** - Multiple observers per adapter with thread-safe notification distribution
- **Error Handling** - Comprehensive exception handling and error propagation
- **Frame Parsing** - Automatic protocol frame parsing from byte streams

### Data Type Support
- Multiple speed step modes (14/27/28/126)
- Short and long locomotive addresses
- 29 locomotive functions (F0-F28)
- 1024 accessory/turnout addresses
- CV addresses (1-1024)
- Big and little endian conversions

## Architecture
The library follows a clean layered architecture:
- **Transport Layer** - UDP communication channel (protocol-agnostic)
- **Protocol Layer** - Z21, XpressNet, and LocoNet protocol implementations
- **Adapter Layer** - Protocol-specific adapters (e.g., Z21 adapter)
- **Interface Layer** - Protocol-agnostic abstractions for application use

This design enables easy testing, protocol independence, and extensibility.

## Project Structure
- `Tellurian.Communications.Channels` - Generic UDP communication infrastructure
- `Tellurian.Trains.Interfaces` - Protocol-agnostic interfaces and data types
- `Tellurian.Trains.Protocols.XpressNet` - XpressNet protocol implementation
- `Tellurian.Protocols.LocoNet` - LocoNet protocol implementation
- `Tellurian.Trains.Adapters.Z21` - Z21 command station adapter

## Status Legend
- ✅ **Fully Implemented** - Production-ready with comprehensive features
- ⚠️ **Partially Implemented** - Functional but with some limitations

##