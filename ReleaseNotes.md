# General Release Notes
This document provides an overview of the 
hanges, improvements, and fixes included in releases of **Tellurian.Trains.Control** series of NuGet-packages.
Each NuGet-package may have its own specific release notes, which can be found in their respective repositories and on NuGet.

## Releases

### Version 1.4.0 - LNCV Programming Support
Release date 2026-02-11

**New Features**
- **LNCV programming**: Full support for LocoNet Configuration Variables (LNCV), the Uhlenbrock proprietary extension for configuring stationary LocoNet devices (accessory decoders, feedback modules, signal decoders) from manufacturers like Uhlenbrock, Digikeijs, etc.
  - Session-based programming: start/end programming sessions with specific devices
  - Read and write 16-bit CVs (0-65535) with 16-bit values (0-65535)
  - Device discovery: broadcast scan to find all connected LNCV devices of a given type
  - Available on both LocoNet (serial) and Z21 (UDP) adapters
- **PXCT1 encoding**: Internal helper for LocoNet 7-bit data byte encoding/decoding used by peer transfer messages
- **LNCV error reporting**: `LongAcknowledge` now reports LNCV-specific write errors (unsupported CV, read-only CV, value out of range)
- **LoconetOverTcp transport** (`TcpLocoNetChannel`): New TCP channel implementing the LoconetOverTcp protocol (Stefan Bormann), enabling LocoNet communication over IP networks via servers such as LbServer, JMRI, or Rocrail on port 1234. The existing LocoNet adapter works unchanged with this new channel — just swap `SerialDataChannel` for `TcpLocoNetChannel`.

**Bug Fixes**
- **Z21 LocoNet notification parsing**: Fixed `LocoNetNotification.Message` property which was always null. LocoNet messages received through Z21 are now properly parsed, enabling LocoNet notification mapping to work correctly.

### Version 1.3.0 - Naming Improvements and LocoNet Enhancements
Release date 2026-02-09

**Breaking Changes**
- **`ISwitch` renamed to `ITurnout`**: The `ISwitch` interface has been renamed to `ITurnout` to use proper railroad terminology. `ISwitch` is retained as an obsolete wrapper inheriting from `ITurnout` for backwards compatibility.

**Improvements**
- **Accessory naming cleanup**: Renamed LocoNet command classes from "Switch"/"Turnout" terminology to "Accessory" (e.g., `SetTurnoutCommand` → `SetAccessoryCommand`, `RequestTurnoutStateCommand` → `RequestAccessoryStateCommand`) for consistency with the LocoNet specification.
- **Added `SetAccessoryNotification`**: New notification class for LocoNet OPCODE 0xB0 (OPC_SW_REQ). When a device on the LocoNet bus sends a `SetAccessoryCommand`, other devices now receive it as a `SetAccessoryNotification`, enabling proper bus monitoring and state tracking.

### Version 1.2.1 - Bug fix
Release date 2026-02-09
**Bug fix**
- Observer<T> made thread safe.

### Version 1.2.0 - Bug Fixes and Improvements
Release Date: 2026-01-14

**Bug Fixes**
- **Accessory addressing corrected**: Fixed accessory/switch addresses to use 1-based user addressing (1-2048) consistently across all protocols. Previously, addresses were incorrectly 0-based in some contexts, causing off-by-one errors when controlling turnouts. The `Address` struct now properly converts between user-facing 1-based addresses and protocol-specific wire addresses internally.

**Improvements**
- **AOT compatibility**: Rewrote JSON serialization converters to be fully AOT-compatible by eliminating reflection-based property enumeration. Custom converters now use explicit type handling instead of `JsonSerializer.Serialize` with runtime types.
- **Enum serialization**: Added AOT-safe `JsonStringEnumConverter<T>` implementations for all enum types (`Position`, `MotorState`, `Direction`, `Functions`, `LocoSpeedSteps`).
- **Project renamed**: `Tellurian.Trains.Interfaces` renamed to `Tellurian.Trains.Communications.Interfaces` for clarity.

### Version 1.1.0 - Initial Release
Release Date: 2025-11-29
- **Inital Release** for Tellurian.Trains.Control NuGet-packages. 
Consult the README files in each package repository for detailed information on features and usage.

### Version 1.1.0 - Feature Update
Release Date: 2025-11-30
- Added JSON serialization support for LocoNet and XpressNet messages..