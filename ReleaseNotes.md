# General Release Notes
This document provides an overview of the 
hanges, improvements, and fixes included in releases of **Tellurian.Trains.Control** series of NuGet-packages.
Each NuGet-package may have its own specific release notes, which can be found in their respective repositories and on NuGet.

## Releases

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