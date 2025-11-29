## System Architecture

The library follows a **layered architecture** with clear separation of concerns:

### 1. Transport Layer
**Project**: `Tellurian.Communications.Channels`

Protocol-agnostic communication infrastructure.

See `Tellurian.Communications.Channels/CLAUDE.md` for details.

### 2. Interface Layer
**Project**: `Tellurian.Trains.Interfaces`

Protocol-agnostic abstractions for client applications.

See `Tellurian.Trains.Interfaces/CLAUDE.md` for details.

### 3. Protocol Layer
**Projects**: `Tellurian.Trains.Protocols.XpressNet`, `Tellurian.Protocols.LocoNet`

Protocol implementations that translate high-level commands to protocol-specific byte sequences.

- See `Tellurian.Trains.Protocols.XpressNet/CLAUDE.md` for XpressNet details.
- See `Tellurian.Protocols.LocoNet/CLAUDE.md` for LocoNet details.

### 4. Adapter Layer
**Project**: `Tellurian.Trains.Adapters.Z21`

Adapter for Z21 command stations.

See `Tellurian.Trains.Adapters.Z21/CLAUDE.md` for details.

### Data Flow

```
Application
    ↓ (uses ILocoControl, ILocoDecoder)
Adapter (Z21/LocoNet)
    ↓ (translates to)
Protocol Layer (XpressNet/LocoNet Commands)
    ↓ (sends via)
Transport Layer (UdpDataChannel or SerialDataChannel)
    ↓
Physical Hardware (Z21, Digitrax, etc.)

(Notifications flow in reverse direction)
```
