# Tellurian.Trains.Communications

A .NET 10 library for communication with model train layouts using
Roco Z21, Digitrax LocoNet, and LENZ XpressNet protocols.
Program against protocol-agnostic interfaces while the library handles
protocol-specific communication.

## Architecture

```
┌────────────────────────────────────────────────────────────────────────────┐
│                    Your Application                                        │
├────────────────────────────────────────────────────────────────────────────┤
│        Tellurian.Trains.Communications.Interfaces (ILoco, IDecoder...)     │
├────────────────────────┬───────────────────────────────────────────────────┤
│  Adapters.Z21 (UDP)    │   Adapters.LocoNet (Serial/TCP/UDP Multicast)      │
├────────────────────────┼───────────────────────────────────────────────────┤
│  Protocols.XpressNet   │   Protocols.LocoNet                               │
├────────────────────────┴───────────────────────────────────────────────────┤
│             Communications.Channels (UDP/Serial/TCP)                       │
└────────────────────────────────────────────────────────────────────────────┘
```

## Projects

| Project | Description | Status |
|---------|-------------|--------|
| **Tellurian.Trains.Communications.Interfaces** | Protocol-agnostic interfaces (`ILoco`, `IAccessory`, `ITurnout`, `IDecoder`) and data types | ✅ Complete |
| **Tellurian.Trains.Communications.Channels** | UDP, serial, TCP (LoconetOverTcp), and UDP multicast (LocoNet over UDP) transport layer with async operations and observer pattern | ✅ Complete |
| **Tellurian.Trains.Protocols.XpressNet** | XpressNet protocol encoding/decoding (Lenz, Z21) | ✅ Complete |
| **Tellurian.Trains.Protocols.LocoNet** | LocoNet protocol encoding/decoding (Digitrax) | ✅ ~75% |
| **Tellurian.Trains.Adapters.Z21** | Z21 command station adapter via UDP | ✅ Complete |
| **Tellurian.Trains.Adapters.LocoNet** | LocoNet command station adapter via serial port, TCP, or UDP multicast | ✅ Core features |

## Key Features

- **Locomotive control**: Speed (14/27/28/126 steps), direction, functions F0-F28, and state query from command station
- **Accessory/turnout control**: Addresses 1-1024 with state queries
- **Decoder programming**: Service mode and operations mode (POM), CVs 1-1024
- **LNCV programming**: Read/write LocoNet Configuration Variables for stationary decoders (Uhlenbrock, YaMoRC, etc.)
- **Detector/occupancy support**: Occupancy, transponder, and RailCom notifications from LocoNet, Z21 LocoNet detector API, and Z21 CAN detectors (10808)
- **Consisting**: Multi-unit operations (LocoNet) and double headers (XpressNet)
- **Async-first**: All operations are non-blocking
- **Multiple observers**: Thread-safe notification distribution
- **JSON serializable data types**: Easy integration with web API, storage and custom protocols

## Compatibility

This library implements open, widely adopted model railroad protocols.
Any command station, interface, or software that speaks **XpressNet**, **LocoNet**, or the **Z21 UDP protocol** can potentially be used with this library.

### Protocols

| Protocol | Origin | Used by |
|----------|--------|---------|
| **XpressNet** | Lenz Elektronik | Lenz, Roco/Fleischmann, Atlas, YaMoRC, TAMS, ZTC, OpenDCC, and others |
| **LocoNet** | Digitrax | Digitrax, Uhlenbrock, Fleischmann, TAMS, YaMoRC, and others |
| **Z21 UDP** | Roco/Fleischmann | Roco Z21 family (Z21, z21, Z21 XL), TAMS mc², YaMoRC YD7010/YD7001 (Z21-compatible mode) |

### Command Stations

The following are examples of command stations that use protocols supported by this library:

| Manufacturer | Model(s) | Protocol | Connection |
|---|---|---|---|
| **Roco/Fleischmann** | Z21, z21 start, Z21 XL | Z21 UDP (encapsulates XpressNet) | Ethernet (UDP) |
| **Lenz** | LZ100, LZV100, LZV200 | XpressNet | Via LI-USB or similar interface |
| **Digitrax** | DCS100, DCS200, DCS210, DCS240 | LocoNet | Serial (LocoBuffer-USB, PR3, PR4) or TCP/UDP |
| **Uhlenbrock** | Intellibox, Intellibox II, IB-Com | LocoNet | Serial, USB, or LocoNet network |
| **TAMS** | MasterControl (mc²) | LocoNet, XpressNet, Z21-compatible | Ethernet (UDP), LocoNet, or XpressNet bus |
| **YaMoRC** | YD7010, YD7001 | LocoNet, XpressNet, Z21-compatible | Ethernet, USB, WiFi |

This list is not exhaustive. Any command station with a LocoNet or XpressNet bus may work.

### LocoNet Interfaces and Gateways

| Connection | Hardware / Software | Details |
|---|---|---|
| **Serial** | LocoBuffer-USB, PR3, PR4 | Direct serial/USB connection to LocoNet bus |
| **TCP** (LoconetOverTcp, port 1234) | LbServer, JMRI, Rocrail | Network access to LocoNet via ASCII hex-encoded TCP |
| **UDP multicast** | loconetd (Glenn Butcher), GCA101 LocoBuffer-UDP (Rocrail/Peter Giling) | Raw binary LocoNet over multicast UDP |

### Compatible Software

| Software | Compatibility |
|---|---|
| **JMRI** | Can act as a LoconetOverTcp server; this library connects as a TCP client |
| **Rocrail** | Supports LoconetOverTcp server mode |
| **LbServer** | Dedicated LoconetOverTcp server for LocoBuffer hardware |

### Detector Hardware

| Detector Type | Supported via |
|---|---|
| Z21 CAN detectors (Roco 10808) | Z21 CAN detector API |
| Z21 LocoNet detectors (Digitrax BDL, Uhlenbrock, LISSY) | Z21 LocoNet detector API |
| LocoNet occupancy/sensor modules | LocoNet sensor input reports |

## Not Implemented (LocoNet)

- Fast clock synchronization (slot 123)
- Raw DCC packet operations (OPC_IMM_PACKET is used for LNCV programming, but not for general raw DCC packets)

## Release Notes

See [ReleaseNotes.md](ReleaseNotes.md) for version history and changes.

## License

See LICENSE file.