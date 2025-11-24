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
- Short circuit detection
- Current and voltage monitoring

**Accessory Control:**
- Turnout/accessory control (addresses 1-1024)
- Feedback notifications
- Direct or queued execution modes

**Programming:**
- CV read/write operations (CVs 1-1024)
- Programming track status monitoring
- Error detection (short circuit, timeout, no acknowledgment)

**System Monitoring:**
- Hardware and firmware version detection
- Real-time current monitoring (main track and programming track)
- Voltage monitoring (supply and track)
- Temperature monitoring
- Comprehensive system state notifications

### LocoNet Protocol Support ⚠️ Partially Implemented
Basic LocoNet command support for Digitrax-compatible systems.

**Implemented Features:**
- Track power control (on/off/idle)
- Slot-based locomotive control (speed, direction, functions F0-F19)
- Turnout control
- Address/slot management
- Checksum validation and acknowledgments

**Status Note:** Command structures are implemented, but notification mapping to interface types is incomplete.

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