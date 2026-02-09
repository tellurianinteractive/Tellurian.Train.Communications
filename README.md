# Tellurian.Trains.Communications

A .NET 10 library for communication with model train layouts using 
TOCO Z21, Digitrax LocoNet, and LENZ XpressNet protocols. 
Program against protocol-agnostic interfaces while the library handles 
protocol-specific communication.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Your Application                         │
├─────────────────────────────────────────────────────────────┤
│        Tellurian.Trains.Communications.Interfaces (ILoco, IDecoder...)     │
├────────────────────────┬────────────────────────────────────┤
│  Adapters.Z21 (UDP)    │   Adapters.LocoNet (Serial)        │
├────────────────────────┼────────────────────────────────────┤
│  Protocols.XpressNet   │   Protocols.LocoNet                │
├────────────────────────┴────────────────────────────────────┤
│             Communications.Channels (UDP/Serial)            │
└─────────────────────────────────────────────────────────────┘
```

## Projects

| Project | Description | Status |
|---------|-------------|--------|
| **Tellurian.Trains.Communications.Interfaces** | Protocol-agnostic interfaces (`ILoco`, `IAccessory`, `ITurnout`, `IDecoder`) and data types | ✅ Complete |
| **Tellurian.Trains.Communications.Channels** | UDP and serial port transport layer with async operations and observer pattern | ✅ Complete |
| **Tellurian.Trains.Protocols.XpressNet** | XpressNet protocol encoding/decoding (Lenz, Z21) | ✅ Complete |
| **Tellurian.Trains.Protocols.LocoNet** | LocoNet protocol encoding/decoding (Digitrax) | ✅ ~75% |
| **Tellurian.Trains.Adapters.Z21** | Z21 command station adapter via UDP | ✅ Complete |
| **Tellurian.Trains.Adapters.LocoNet** | LocoNet command station adapter via serial port | ✅ Core features |

## Key Features

- **Locomotive control**: Speed (14/27/28/126 steps), direction, functions F0-F28
- **Accessory/turnout control**: Addresses 1-1024 with state queries
- **Decoder programming**: Service mode and operations mode (POM), CVs 1-1024
- **Consisting**: Multi-unit operations (LocoNet) and double headers (XpressNet)
- **Async-first**: All operations are non-blocking
- **Multiple observers**: Thread-safe notification distribution
- **JSON serializable data types**: Easy integration with web API, storage and custom protocols

## Building

Release builds of the NuGet packages should be copied to `C:/Nugets` after a successful build.

## Not Implemented (LocoNet)

- Fast clock synchronization (slot 123)
- Raw DCC packet operations (OPC_IMM_PACKET)
- Advanced peer transfer operations

## Release Notes

See [ReleaseNotes.md](ReleaseNotes.md) for version history and changes.

## License

See LICENSE file.