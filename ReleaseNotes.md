# General Release Notes
This document provides an overview of the 
hanges, improvements, and fixes included in releases of **Tellurian.Trains.Control** series of NuGet-packages.
Each NuGet-package may have its own specific release notes, which can be found in their respective repositories and on NuGet.

## Releases

### Version 1.7.8 - XpressNet Turnout Info Length Fix
Release date 2026-04-15

**Bug Fixes**
- **Factory disambiguation for 0x43 now accounts for the XOR-trailing production wire format.** 1.7.7 only routed 4-byte 0x43 buffers (test-style, no checksum) to `TurnoutInfoNotification`; real frames coming through `Packet` include the trailing XOR byte and are 5 bytes long, so they fell through to `FeedbackBroadcast` and were then silently dropped by the unmapped-notification fallback. Added a Packet-level end-to-end test so this regression path is covered.

### Version 1.7.7 - Z21 XpressNet Turnout Feedback
Release date 2026-04-15

**Bug Fixes**
- **XpressNet `LAN_X_TURNOUT_INFO` now maps to `AccessoryNotification`.** When a turnout is switched by another client on the Z21 (Z21 App, WLANMaus, etc.), the Z21 broadcasts `LAN_X_TURNOUT_INFO` (X-Header 0x43) to subscribers of the `RunningAndSwitching` (0x00000001) flag but does not mirror those changes onto the LocoNet-over-UDP stream. Previously the XpressNet notification mapper had no entry for this message and would throw `InvalidOperationException`, killing the receive loop if an application tried to subscribe. A new `TurnoutInfoNotification` is emitted by the factory for 4-byte 0x43 frames and mapped to `AccessoryNotification` (Output1 → `ClosedOrGreen`, Output2 → `ThrownOrRed`; `NotSwitched`/`Invalid` surface as unmapped). Applications that want feedback for third-party-initiated turnout changes can now subscribe `BroadcastSubjects.LocoNetTurnouts | BroadcastSubjects.RunningAndSwitching`.
- **XpressNet mapper no longer throws on unknown notification types.** `NotificationExtensions.Map` previously threw `InvalidOperationException` for any XpressNet notification not in its dispatch table; it now falls back to the `MessageNotification` unmapped path, matching the Z21 adapter's behaviour for the outer notification layer.

### Version 1.7.6 - Z21 Accessory Send Self-Echo
Release date 2026-04-15

**Bug Fixes**
- **Z21 adapter now emits a local `AccessoryNotification` after a successful wrapped-LocoNet accessory send.** The Z21 filters the original sender out of `LAN_LOCONET_FROM_LAN` echoes (spec §9.3), so a client sending a turnout command via the LocoNet gateway never hears its own write back — unlike serial LocoNet, where the sender naturally hears its write via bus loopback. This left accessory UIs without commanded-state feedback whenever no decoder responded (for example during bench testing without hardware, or when the decoder doesn't report state). The Z21 adapter now synthesizes the matching `AccessoryNotification` locally after a successful send, mirroring the serial-LocoNet behaviour. Real decoder replies (`OPC_SW_REP` via `LAN_LOCONET_Z21_RX`) still arrive as independent `AccessoryNotification` events and override/confirm the commanded value.
- Only applies when `UseLocoNetForAccessories` is `true` (the default, introduced in 1.7.5). The XpressNet path is unaffected.

### Version 1.7.5 - Z21 Accessories via Wrapped LocoNet
Release date 2026-04-15

**New Features**
- **Z21 adapter routes accessory commands through the LocoNet gateway by default.** `IAccessory.SetAccessoryAsync` / `ITurnout.SetThrownAsync` / `SetClosedAsync` / `QueryAccessoryStateAsync` now wrap LocoNet `OPC_SW_REQ` / `OPC_SW_STATE` bytes in `LAN_LOCONET_FROM_LAN` (0xA2) instead of sending native XpressNet `LAN_X_SET_TURNOUT`. Feedback then arrives on the already-mapped LocoNet stream (subscribe `BroadcastSubjects.LocoNetTurnouts`), which is decoded into the protocol-agnostic `AccessoryNotification` — so applications get accessory state change events out of the box without relying on the currently-unmapped XpressNet `LAN_X_TURNOUT_INFO` path.
- **Opt out for DCC-only decoders.** New `Adapter` constructor parameter `useLocoNetForAccessories` (defaults to `true`) and public read-only property `UseLocoNetForAccessories`. Set it to `false` if your accessory decoders are DCC-only and not reachable via the Z21's LocoNet bus — XpressNet `LAN_X_SET_TURNOUT` will be used as before. A future release will add XpressNet → `AccessoryNotification` mapping so feedback works in that mode too.
- **Backward compatibility.** All existing `Adapter` constructor overloads still work; the new `useLocoNetForAccessories` parameter is added via a new constructor, while the 2- and 3-argument overloads pass-through with the new default.

### Version 1.7.4 - Z21 Dynamic Broadcast Subscription
Release date 2026-04-15

**New Features**
- **Z21 adapter auto-subscribes on connect**: New constructor overload `Adapter(ICommunicationsChannel, ILogger<Adapter>, BroadcastSubjects)` accepts an initial broadcast subscription. When `StartReceiveAsync` is called, the adapter automatically sends `LAN_SET_BROADCASTFLAGS` with the configured subjects. The set is re-applied on every (re)connect — the Z21 protocol requires flags to be resent after reconnection.
- **Dynamic subscription management**: New methods on the Z21 `Adapter` allow runtime subscription changes:
  - `SubscribeAsync(BroadcastSubjects)` — replace the active set (absolute).
  - `AddSubscriptionsAsync(BroadcastSubjects)` / `RemoveSubscriptionsAsync(BroadcastSubjects)` — convenience OR/AND-NOT over the tracked current set.
  - `CurrentSubscriptions` property exposes the last-applied set.
- **Backward compatibility**: The existing 2-argument constructor is preserved as a pass-through with `BroadcastSubjects.None`, so existing consumers are unaffected.

### Version 1.7.3 - LocoNet Slot Cache Fix
Release date 2026-03-28

**Bug Fixes**
- **LocoNet slot cache kept in sync**: The LocoNet adapter now updates its cached slot data after sending speed, direction, and function commands, preventing stale state from overwriting previously set function bits. Since LocoNet packs multiple functions into a single byte (DIRF for F0-F4, SND for F5-F8), stale cache data caused toggling one function to reset others.
- **LocoNet bus state monitoring**: Added notifications for `OPC_LOCO_SPD` (0xA0), `OPC_LOCO_DIRF` (0xA1), and `OPC_LOCO_SND` (0xA2) so that loco state changes from other throttles on the LocoNet bus are reflected in the slot cache, keeping it in sync even when multiple throttles control the same locomotive.

### Version 1.7.1 - LocoNet Function Key Fix
Release date 2026-03-27

**Bug Fixes**
- **LocoNet DIRF function bit order corrected**: Fixed `SetLocoDirectionAndFunctionF0toF4Command` (OPC_LOCO_DIRF 0xA1) where F1-F4 bit positions were reversed. F1 was mapped to bit 3 (F4's position) and F4 to bit 0 (F1's position), causing F1↔F4 and F2↔F3 to be swapped when controlling locomotives via LocoNet serial, TCP, or UDP.

### Version 1.7.0 - Locomotive State Query
Release date 2026-03-16

**New Features**
- **`ILoco.GetLocoInfoAsync`**: New method on the `ILoco` interface to query the current state of a locomotive from the command station. Returns a `LocoInfo` object with speed, direction, and function states (F0-F28).
- **`LocoInfo` type**: New class in `Tellurian.Trains.Communications.Interfaces.Locos` representing locomotive state as reported by the command station.
- **Z21 adapter**: Implements `GetLocoInfoAsync` using the XpressNet `GetLocoInfoCommand` (`LAN_X_GET_LOCO_INFO`). Returns all 29 function states (F0-F28), speed, and direction.
- **LocoNet adapter**: Implements `GetLocoInfoAsync` using LocoNet slot data. Returns speed, direction, and function states F0-F8 (F9-F28 are not available via LocoNet slot data).

### Version 1.6.1 - Accessory Output Status Command
Release date 2026-02-22

**New Features**
- **Accessory output status command** (`OPC_SW_REP` 0xB1): New `AccessoryOutputStatusCommand` for sending accessory output status reports. Encodes address the same way as `SetAccessoryCommand` but with output status bits (bit 6=0 for output, bit 5=Closed, bit 4=Thrown) instead of Direction and MotorState. Includes `Closed` and `Thrown` factory methods.
- **Z21 accessory notification mapping**: `SetAccessoryNotification` (0xB0) and `AccessoryReportNotification` (0xB1 output status) received through the Z21 adapter are now mapped to protocol-agnostic `AccessoryNotification`, matching the standalone LocoNet adapter behavior.

### Version 1.6.0 - Track Detector Support
Release date 2026-02-19

**New Features**
- **Protocol-agnostic detector notifications**: Three new interface notification types for consuming detector events regardless of protocol:
  - `OccupancyNotification` - Track section occupied/free state
  - `TransponderNotification` - Transponder/RailCom locomotive presence (entering/leaving with loco address)
  - `RailComLocomotiveNotification` - RailCom/LISSY locomotive identification with direction and classification
- **LocoNet transponding** (`OPC_MULTI_SENSE` 0xD0): Reports transponder present/absent events with locomotive address, section, and zone (A-H). Supports both short (0x7D marker) and long 14-bit address decoding.
- **LocoNet LISSY/RailCom** (`OPC_LISSY_UPDATE` 0xE4): Reports locomotive address, direction, and category from LISSY and RailCom-enabled detectors.
- **Z21 LocoNet detector API** (`LAN_LOCONET_DETECTOR` 0xA4): Native Z21 API for querying LocoNet-based detectors. Supports occupancy (type 0x01), transponder entering/leaving (types 0x02/0x03), LISSY loco identification (type 0x10), LISSY block status (type 0x11), and LISSY speed (type 0x12). Includes `LocoNetDetectorRequestCommand` with `DetectorRequestType` enum for SIC, Uhlenbrock, and LISSY detectors.
- **Z21 CAN detector API** (`LAN_CAN_DETECTOR` 0xC4): Native CAN bus detector support for Z21 10808 eight-track occupancy detectors with RailCom. Parses occupancy status with detailed state (free/occupied/voltage/overload levels), and RailCom address reports (types 0x11-0x1F) with dual locomotive addresses and direction information. Includes `CanDetectorRequestCommand` and `CanOccupancyStatus` enum.
- **CAN detector broadcast subscription** (`CanDetectorChanges` 0x00080000): New broadcast subject for receiving CAN detector change notifications (requires Z21 FW 1.30+).
- **LocoNet notification mapping**: `SensorInputNotification`, `MultiSenseNotification`, and `LissyNotification` are now mapped to the protocol-agnostic detector notification types when received through Z21 or the LocoNet adapter.

### Version 1.5.0 - LocoNet over UDP Transport
Release date 2026-02-11

**New Features**
- **LocoNet over UDP transport** (`UdpLocoNetChannel`): New multicast UDP channel for raw binary LocoNet communication. Supports both **loconetd** (Glenn Butcher, multicast group 225.0.0.2, ports 4500/4501) and **GCA101 LocoBuffer-UDP** (Rocrail/Peter Giling, multicast group 224.0.0.1, port 1235). Includes optional LocoNet checksum validation. Uses `IUdpLocoNetAdapter` abstraction for testability.

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