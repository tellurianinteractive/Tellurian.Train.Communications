# LocoNet Library Comparison
*Updated 2026-01-11*

A technical comparison of .NET and Java libraries for LocoNet communication in model railroad control systems.

## Libraries Compared

| Aspect | **Tellurian.Trains** | **LocoNetSharp** | **JMRI** |
|--------|----------------------|-------------------------------|----------|
| NuGet/Package | `Tellurian.Trains.*` 1.5.0 | `ParkSquare.LocoNetSharp` 8.0.0 | N/A (application) |
| Language | C# (.NET 10, latest C#) | C# (.NET Standard 2.0 / .NET 6 / .NET 8) | Java |
| Source | [Open source (GitHub)](https://github.com/tellurianinteractive/Tellurian.Trains.Communications) | Closed source | [Open source (GitHub)](https://github.com/JMRI/JMRI) |
| Type | Embeddable NuGet library | Embeddable NuGet library | Full application framework |
| Origin | Original implementation based on availabe specifications | Port of LocoNet Toolbox (Chris Sharp / Modelspoorgroep Venlo / Ewout Prangsma) | Community project (est. 2000) |
| Scope | LocoNet + XpressNet + Z21 (serial, TCP, UDP multicast) | LocoNet only | 20+ hardware systems |
| Compatible with | LocoBuffer-USB, PR3/PR4, LbServer, JMRI, Rocrail, loconetd, GCA101, Roco Z21 | DR5000, serial LocoNet interfaces | Extensive hardware list |
| Latest update | 2026-02-11 (v1.5.0) | 2024-06-22 (v8.0.0) | 2026-01-21 (v5.15.3) |
| License | MIT | Not stated | GPL v2 |

## Transport / Connection

| Transport | **Tellurian.Trains** | **LocoNetSharp** | **JMRI** |
|-----------|----------------------|------------------|----------|
| Serial (COM port) | `SerialDataChannel` + `LocoNetFramer` | `SerialPortLocoBuffer` | Yes (LocoBuffer, PR3/PR4, DCS52, DCS240) |
| TCP | `TcpLocoNetChannel` (LoconetOverTcp ASCII) | `TcpLocoBuffer` (binary) | Yes (LoconetOverTcp, also acts as server) |
| UDP unicast | `UdpDataChannel` (Z21) | `UdpLocoBuffer` (binary) | Yes (binary, DR5000-style) |
| UDP multicast | `UdpLocoNetChannel` (loconetd + GCA101) | Not supported | Not supported |
| HexFile simulator | No | No | Yes |
| Abstraction | `ICommunicationsChannel` + adapter interfaces | `LocoBuffer` base class | `LnPortController` / `LnNetworkPortController` |
| Testability | Mock adapters for all transports | Not documented | Limited |

**Notes:**
- LocoNetSharp uses a binary TCP/UDP protocol (compatible with DR5000 and similar).

### Tellurian.Trains Compatible Software and Hardware

By supporting LocoNet over serial, TCP ([LoconetOverTcp](http://loconetovertcp.sourceforge.net/)), and UDP multicast, Tellurian.Trains can communicate with a wide range of software and hardware:

**Serial (direct connection):**
- Digitrax LocoBuffer-USB, PR3, PR4
- Any LocoNet interface with a serial/USB port

**TCP (LoconetOverTcp protocol, port 1234):**
- **LbServer** — dedicated LoconetOverTcp server for LocoBuffer hardware
- **JMRI** — acts as a LoconetOverTcp server, enabling remote LocoNet access
- **Rocrail** — model railroad automation software with LoconetOverTcp server support

**UDP multicast (raw binary LocoNet datagrams):**
- **loconetd** (Glenn Butcher) — Unix daemon that bridges a LocoBuffer serial device to multicast UDP (group 225.0.0.2, listen port 4501, send to gateway on port 4500)
- **GCA101 LocoBuffer-UDP** (Rocrail/Peter Giling) — hardware LocoNet-to-UDP bridge (group 224.0.0.1, port 1235)

This means a single Tellurian.Trains application can connect to a LocoNet layout through a local USB cable, across the network via JMRI or Rocrail, or via multicast UDP — all using the same `Adapter` class with different channel implementations.

## LocoNet Opcodes / Commands

| Opcode | Description | **Tellurian.Trains** | **LocoNetSharp** | **JMRI** |
|--------|-------------|----------------------|------------------|----------|
| 0x81 (OPC_GPBUSY) | Master busy | `MasterBusyNotification` | Unknown | Yes |
| 0x82 (OPC_GPOFF) | Power off | `PowerOffCommand` | `GlobalPowerOff` | Yes |
| 0x83 (OPC_GPON) | Power on | `PowerOnCommand` | `GlobalPowerOn` | Yes |
| 0x85 (OPC_IDLE) | Emergency stop all | `ForceIdleCommand` | Unknown | Yes |
| 0xA0 (OPC_LOCO_SPD) | Locomotive speed | `SetLocoSpeedCommand` | Unknown | Yes |
| 0xA1 (OPC_LOCO_DIRF) | Direction + F0–F4 | `SetLocoDirectionAndFunctionF0toF4Command` | Unknown | Yes |
| 0xA2 (OPC_LOCO_SND) | Functions F5–F8 | `SetLocoFunctionF5toF8Command` | Unknown | Yes |
| 0xA3 | Functions F9–F12 | `SetLocoFunctionF9toF12` | Unknown | Yes |
| 0xB0 (OPC_SW_REQ) | Set accessory | `SetAccessoryCommand` / `SetAccessoryNotification` | Unknown | Yes |
| 0xB1 (OPC_SW_REP) | Accessory feedback | `AccessoryReportNotification` | Unknown | Yes |
| 0xB2 (OPC_INPUT_REP) | Sensor input | `SensorInputNotification` | Unknown | Yes |
| 0xB4 (OPC_LONG_ACK) | Long acknowledge | `LongAcknowledge` (incl. LNCV errors) | Unknown | Yes |
| 0xB5 (OPC_SLOT_STAT1) | Slot status write | `WriteSlotStatus1Command` | Unknown | Yes |
| 0xB6 (OPC_CONSIST_FUNC) | Consist function | `ConsistFunctionCommand` | Not supported | Yes |
| 0xB8 (OPC_UNLINK_SLOTS) | Unlink consist | `UnlinkSlotsCommand` | Unknown | Yes |
| 0xB9 (OPC_LINK_SLOTS) | Link consist | `LinkSlotsCommand` | Unknown | Yes |
| 0xBA (OPC_MOVE_SLOTS) | Move/dispatch slot | `MoveSlotCommand` | Unknown | Yes |
| 0xBB (OPC_RQ_SL_DATA) | Request slot data | `RequestSlotDataCommand` | Unknown | Yes |
| 0xBC (OPC_SW_STATE) | Query switch state | `RequestAccessoryStateCommand` | Unknown | Yes |
| 0xBD (OPC_SW_ACK) | Accessory with ack | `AccessoryAcknowledgeCommand` | Unknown | Yes |
| 0xBF (OPC_LOCO_ADR) | Request loco slot | `GetLocoAddressCommand` | Unknown | Yes |
| 0xD4 | Functions F13–F19 | `SetLocoFunctionF13toF19` | Unknown | Yes (expanded) |
| 0xE5 (OPC_PEER_XFER) | Peer transfer | `LncvNotification` (LNCV replies) | Not supported | Yes |
| 0xE7 (OPC_SL_RD_DATA) | Slot data response | `SlotNotification` | Unknown | Yes |
| 0xED (OPC_IMM_PACKET) | Immediate packet | `LncvCommand` (LNCV only) | Not supported | Yes (full raw DCC) |
| 0xEF (OPC_WR_SL_DATA) | Write slot data | `WriteSlotDataCommand` / `ProgrammingCommand` | Unknown | Yes |

### Opcodes Only in JMRI

| Opcode | Description |
|--------|-------------|
| OPC_MULTI_SENSE | Transponding, power manager, BDL16x status |
| OPC_MULTI_SENSE_LONG | Extended multi-sense messages |
| OPC_LISSY_UPDATE | LISSY detection system |
| OPC_PANEL_QUERY/RESPONSE | Panel information exchange |
| OPC_ALM_READ/WRITE | Alarm memory operations |
| OPC_EXP_* (expanded slots) | Newer command stations (DCS210+) |
| OPC_EXP_SEND_FUNCTION_GROUP_* | Functions F0–F28 via expanded protocol |
| OPC_RE_LOCORESET_BUTTON | Locomotive reset button |
| Fast clock (slot 123) | Layout time synchronization |

## High-Level API

| Feature | **Tellurian.Trains** | **LocoNetSharp** | **JMRI** |
|---------|----------------------|------------------|----------|
| Locomotive control | `ILoco` — `DriveAsync`, `EmergencyStopAsync`, `SetFunctionAsync` | Raw messages | `LocoNetThrottle`, `SlotManager` |
| Accessory control | `IAccessory` — `SetAccessoryAsync`, `QueryAccessoryStateAsync` | Raw messages | `LnTurnout`, `LnTurnoutManager` |
| Turnout control | `ITurnout` — `SetThrownAsync`, `SetClosedAsync`, `TurnOffAsync` | Raw messages | `LnTurnout` |
| CV programming | `IDecoder` — `ReadCVAsync`, `WriteCVAsync` | Raw messages | `SlotManager` implements `Programmer` |
| LNCV programming | `StartLncvSessionAsync`, `ReadLncvAsync`, `WriteLncvAsync`, discovery | Not supported | `LncvDevicesManager` |
| Consisting | `LinkSlotsCommand`, `UnlinkSlotsCommand`, `ConsistFunctionCommand` | Not supported | `LocoNetConsist`, `LocoNetConsistManager` |
| Slot management | Automatic allocation + caching | `LocoNet` state management | `SlotManager` + `LocoNetSlot` |
| Sensor monitoring | `SensorInputNotification` (occupancy, switch input) | Raw messages | `LnSensor`, `LnSensorManager` |
| Signaling | Not supported | Not supported | `SE8cSignalHead`, `LNCPSignalMast`, `LnCabSignal` |
| Transponding | Not supported | Not supported | `TranspondingTag`, `TranspondingTagManager` |
| Fast clock | Not supported | Not supported | `LnClockControl` |
| Throttle messaging | Not supported | Not supported | `LnMessageManager` |
| Multi-protocol | LocoNet + XpressNet + Z21 behind same interfaces | LocoNet only | 20+ hardware systems (different packages) |

## Notifications / Events

| Aspect | **Tellurian.Trains** | **LocoNetSharp** | **JMRI** |
|--------|----------------------|------------------|----------|
| Pattern | `IObservable<T>` (standard .NET) | Custom events (`SendMessage`, `PreviewMessage`, `StateChanged`, `Idle`) | `LocoNetListener` interface |
| Typed notifications | Yes — `SlotNotification`, `SensorInputNotification`, `AccessoryReportNotification`, `LncvNotification`, etc. | Message decode for "some well-known messages" | Yes — full typed managers per feature |
| Thread safety | `Observers<T>` thread-safe distribution | Not documented | Async listener dispatch (not GUI thread) |

## Design & Architecture

| Aspect | **Tellurian.Trains** | **LocoNetSharp** | **JMRI** |
|--------|----------------------|------------------|----------|
| API style | async/await, typed commands, result types | Event-driven, raw messages | Listener/callback, managers |
| Async model | `Task`-based with `CancellationToken` | Not documented | Thread-based |
| Error handling | `CommunicationResult` (Success/Failure/NoOperation) | Exceptions (implied) | Exceptions |
| Testability | Interface abstractions + mock implementations | Not documented | Limited mocking |
| JSON serialization | AOT-compatible for all data types | Not supported | Not applicable |
| Embeddable | Yes (NuGet packages) | Yes (NuGet package) | Difficult (monolithic application) |
| Package size | Multiple focused packages | 76.81 KB single package | Full application (~100 MB+) |

## Addressing

| Aspect | **Tellurian.Trains** | **JMRI** |
|--------|----------------------|----------|
| Locomotive addresses | 1–9999 (short 1–127, long 128–9999) | 1–9999 |
| Accessory addresses | 1–2048 user (0–2047 wire) | 1–2048 |
| Sensor addresses | 0–2047 (11-bit) | 0–4095 |
| CV numbers | 1–1024 (DCC), 0–65535 (LNCV) | 1–1024 (DCC), LNCV supported |
| CV values | 0–255 (DCC), 0–65535 (LNCV) | 0–255 (DCC), LNCV supported |
| Slots | 0–119 (locomotives), 124 (programming) | 0–119 + expanded slots |

## Summary

**JMRI** is the most complete LocoNet implementation — it covers virtually every opcode including expanded slots, transponding, fast clock, signaling, and board configuration. However, it is a monolithic Java application framework, not an embeddable library.

**Tellurian.Trains** is designed as an embeddable .NET library with modern async patterns, covering ~75–80% of LocoNet opcodes (the production-critical ones). It adds unique value with UDP multicast transport and multi-protocol support (LocoNet + XpressNet + Z21 behind the same `ILoco`/`IAccessory`/`ITurnout`/`IDecoder` interfaces).

**LocoNetSharp** is a simpler transport-focused library — suitable for raw LocoNet bus access with basic message decode and DR5000-style binary TCP/UDP. Its closed source makes exact opcode coverage difficult to determine.

### Choosing a Library

| Use Case | Recommendation |
|----------|----------------|
| Full model railroad control application (Java) | JMRI |
| Embedded .NET application with multi-protocol support | Tellurian.Trains |
| Simple LocoNet monitoring/raw access (.NET) | LocoNetSharp |
| UDP multicast LocoNet (loconetd, GCA101) | Tellurian.Trains (unique) |
| LNCV programming (.NET) | Tellurian.Trains |
| Transponding, signaling, fast clock | JMRI |

## References

- [Tellurian.Trains.Communications (this repository)](README.md)
- [LocoNetSharp product page](https://www.parksq.co.uk/dotnet-core/loconet-sharp)
- [LocoNetSharp on NuGet](https://www.nuget.org/packages/ParkSquare.LocoNetSharp/)
- [JMRI: Java Model Railroad Interface](https://www.jmri.org/)
- [JMRI LocoNet Classes](https://www.jmri.org/help/en/html/hardware/loconet/LocoNetClasses.shtml)
- [JMRI on GitHub](https://github.com/JMRI/JMRI)
- [LocoNet Personal Use Edition specification](https://www.digitrax.com/support/loconet/home/)
