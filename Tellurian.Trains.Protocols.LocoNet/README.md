# Tellurian.Trains.Protocols.LocoNet

A comprehensive C# implementation of the LocoNet protocol for model railroad control.

## Overview

This library provides a complete, type-safe implementation of the LocoNet Personal Use Edition 1.0 protocol, enabling full control of DCC model railroad systems through LocoNet-compatible command stations (DCS100, DCS200, etc.) and interfaces.

LocoNet is a peer-to-peer, multi-drop network protocol developed by Digitrax for model railroad control, supporting locomotive operation, turnout control, sensor feedback, decoder programming, and more.

## Features

### ✅ Implemented (~80% Coverage)

#### Core Infrastructure
- ✅ Message length calculation from opcode
- ✅ Checksum validation and generation
- ✅ Message factory for parsing incoming messages
- ✅ Complete slot data structure (14-byte messages)

#### Power & System Control
- ✅ Global power ON/OFF
- ✅ Emergency stop (IDLE state)
- ✅ Master busy notifications

#### Locomotive Control
- ✅ Request locomotive by address (1-9999, short and long)
- ✅ Slot management (request, activate, dispatch)
- ✅ Speed control (0-127 speed steps)
- ✅ Direction control
- ✅ Functions F0-F8 (standard LocoNet)
- ✅ Functions F9-F19 (extended, non-standard)

#### Accessory Control
- ✅ Throw/close accessories (with and without acknowledge)
- ✅ Accessory state requests
- ✅ Accessory feedback reports (input and output status)
- ✅ User addressing 1-2048 (maps to wire addresses 0-2047)

#### Sensor & Feedback
- ✅ General sensor input reports (OPC_INPUT_REP)
- ✅ Occupancy detection
- ✅ Block detection
- ✅ 11-bit sensor addressing
- ✅ Transponding reports (OPC_MULTI_SENSE) - present/absent events with locomotive address
- ✅ LISSY/RailCom identification (OPC_LISSY_UPDATE) - locomotive address, direction, category

#### Programming Track
- ✅ Service mode CV read/write (paged, direct, register)
- ✅ Operations mode (POM) programming
- ✅ CV addressing (1-1024)
- ✅ Programming status and error reporting
- ✅ All programming modes supported

#### Consisting
- ✅ Link slots for multi-locomotive consists
- ✅ Unlink consist members
- ✅ Individual consist member control
- ✅ Consist status tracking

#### LNCV Programming (Uhlenbrock Extension)
- ✅ Start/end programming sessions
- ✅ Read 16-bit CVs (0-65535) with 16-bit values (0-65535)
- ✅ Write 16-bit CVs with error reporting (unsupported CV, read-only, out of range)
- ✅ Broadcast device discovery
- ✅ PXCT1 high-bit encoding/decoding for peer transfer messages

### ⏳ Not Implemented

- ⏳ Fast Clock (slot 123) - specialized feature
- ⏳ OPC_IMM_PACKET for raw DCC packets (opcode 0xED is used for LNCV programming, but not for general raw DCC packets)

## Installation

```bash
dotnet add package Tellurian.Trains.Protocols.LocoNet
```

Or add to your `.csproj`:

```xml
<PackageReference Include="Tellurian.Trains.Protocols.LocoNet" />
```

## Quick Start

### Basic Locomotive Control

```csharp
using Tellurian.Trains.Protocols.LocoNet;
using Tellurian.Trains.Protocols.LocoNet.Commands;
using Tellurian.Trains.Protocols.LocoNet.Notifications;

// 1. Request locomotive address
var requestLoco = new GetLocoAddressCommand(new LocoAddress(3));
byte[] message = requestLoco.GetBytesWithChecksum();
// Send message to LocoNet...

// 2. Parse response
var response = LocoNetMessageFactory.Create(receivedBytes);
if (response is SlotNotification slot)
{
    Console.WriteLine($"Locomotive 3 assigned to slot {slot.SlotNumber}");

    // 3. Activate slot if needed
    if (slot.Status != SlotStatus.InUse)
    {
        var activate = MoveSlotCommand.Activate(slot.SlotNumber);
        // Send activation...
    }

    // 4. Control the locomotive
    var setSpeed = new SetLocoSpeedCommand(slot.SlotNumber, 64); // Half speed
    var setDirection = new SetLocoDirectionAndFunctionF0toF4Command(
        slot.SlotNumber,
        forward: true,
        F0: true,  // Headlight on
        F1: false, F2: false, F3: false, F4: false
    );
    // Send commands...
}
```

## Core Concepts

### Slots

LocoNet uses a **slot-based** system for locomotive control. Think of slots as "file handles" for trains:

- **120 slots** (0-119) for locomotives
- **Special slots**: 123=fast clock, 124=programming track
- Each slot contains: address, speed, direction, functions, status, decoder type

**Slot Status:**
- `FREE` - Empty, no locomotive assigned
- `COMMON` - Address assigned, being refreshed, but not controlled
- `IDLE` - Address assigned, NOT being refreshed
- `IN_USE` - Address assigned, being refreshed and controlled

### Message Structure

All LocoNet messages follow a consistent format:

```
[OPCODE | ARG1 | ARG2 | ... | CHECKSUM]
  MSB=1   MSB=0  MSB=0        MSB=0
```

- **Opcode** encodes both operation and message length
- **Length** determined by bits 6-5 of opcode
- **Checksum** is 1's complement of XOR of all bytes

### Address Encoding

**Locomotive Addresses:**
- Short (1-127): `adr_hi=0x00, adr_lo=address`
- Long (128-9999): 14-bit split across two 7-bit bytes

**Switch Addresses:**
- User addresses: 1-2048 (what you configure in your software)
- Wire addresses: 0-2047 (what is sent on LocoNet)
- Encoded in SW1 (low 7 bits) and SW2 (high 4 bits + control)
- The library automatically converts user → wire (-1) and wire → user (+1)

## Usage Examples

### Power Control

```csharp
// Turn track power ON
var powerOn = new PowerOnCommand();
SendCommand(powerOn);

// Turn track power OFF
var powerOff = new PowerOffCommand();
SendCommand(powerOff);

// Emergency stop all locomotives
var emergencyStop = new ForceIdleCommand();
SendCommand(emergencyStop);
```

### Locomotive Control Workflow

```csharp
// Complete workflow to control a locomotive
async Task ControlLocomotive(ushort address)
{
    // 1. Request the address
    var requestCmd = new GetLocoAddressCommand(new LocoAddress(address));
    SendCommand(requestCmd);

    // 2. Wait for slot notification
    var slotMsg = await ReceiveMessage();
    if (slotMsg is SlotNotification slot)
    {
        // 3. Activate if needed
        if (slot.Status != SlotStatus.InUse)
        {
            SendCommand(MoveSlotCommand.Activate(slot.SlotNumber));
            await ReceiveMessage(); // Wait for confirmation
        }

        // 4. Set speed
        SendCommand(new SetLocoSpeedCommand(slot.SlotNumber, 80));

        // 5. Set direction and functions
        SendCommand(new SetLocoDirectionAndFunctionF0toF4Command(
            slot.SlotNumber,
            forward: true,
            F0: true, // Headlight
            F1: false, F2: false, F3: false, F4: false
        ));

        // 6. Keep slot alive (send command every 100 seconds)
        // to prevent automatic purge
    }
}
```

### Switch Control

```csharp
// Throw switch 100
var throwSwitch = SetAccessoryCommand.Throw(AccessoryAddress.From(100));
SendCommand(throwSwitch);

// Wait for turnout motor
await Task.Delay(1000);

// Turn off output to prevent overheating
var turnOff = SetAccessoryCommand.TurnOff(AccessoryAddress.From(100));
SendCommand(turnOff);

// Close switch with acknowledge
var closeSwitch = AccessoryAcknowledgeCommand.Close(AccessoryAddress.From(100));
SendCommand(closeSwitch);

// Wait for acknowledgment
var ack = await ReceiveMessage();
if (ack is LongAcknowledge longAck && longAck.IsSuccess)
{
    Console.WriteLine("Switch command accepted");
}
```

### Sensor Monitoring

```csharp
// Monitor all incoming messages
while (true)
{
    var msg = LocoNetMessageFactory.Create(await ReceiveBytes());

    if (msg is SensorInputNotification sensor)
    {
        Console.WriteLine($"Sensor {sensor.Address}: {(sensor.IsOccupied ? "OCCUPIED" : "CLEAR")}");

        // React to occupancy
        if (sensor.IsOccupied)
        {
            OnBlockOccupied(sensor.Address);
        }
    }
    else if (msg is MultiSenseNotification multiSense && multiSense.IsTransponding)
    {
        Console.WriteLine($"Section {multiSense.Section} Zone {multiSense.Zone}: " +
            $"Loco {multiSense.LocoAddress} {(multiSense.IsPresent ? "PRESENT" : "ABSENT")}");
    }
    else if (msg is LissyNotification lissy && lissy.IsValid)
    {
        Console.WriteLine($"LISSY Section {lissy.SectionAddress}: " +
            $"Loco {lissy.LocoAddress} {(lissy.IsForward ? "Fwd" : "Rev")} Cat {lissy.Category}");
    }
    else if (msg is AccessoryReportNotification switchReport)
    {
        if (switchReport.IsOutputStatus)
        {
            Console.WriteLine($"Switch {switchReport.Address.Value}: {switchReport.CurrentDirection}");
        }
    }
}
```

### Programming Track (CV Operations)

```csharp
// Read CV29 on programming track
var readCV = ProgrammingCommand.ReadCvService(29);
SendCommand(readCV);

// Wait for Long Acknowledge
var ack = await ReceiveMessage();
if (ack is LongAcknowledge longAck && longAck.ResponseCode == 0x01)
{
    Console.WriteLine("Programming started, waiting for result...");

    // Wait for programming response (slot 124)
    var result = await ReceiveMessage();
    if (result is SlotNotification slot && slot.IsProgrammingSlot)
    {
        var progResult = slot.ProgrammingResult;
        if (progResult?.IsSuccess == true)
        {
            Console.WriteLine($"{progResult.CV}");  // "CV29=value"
        }
        else
        {
            Console.WriteLine($"Programming failed: {progResult?.StatusMessage}");
        }
    }
}

// Write CV3 = 10
var writeCV = ProgrammingCommand.WriteCvService(3, 10);
SendCommand(writeCV);

// Operations Mode (POM) - write to running locomotive
var pomWrite = ProgrammingCommand.WriteCvOperations(
    locomotiveAddress: 3,
    cvNumber: 2,
    value: 5,
    withFeedback: false
);
SendCommand(pomWrite);
```

### Consisting (Multi-Locomotive Control)

```csharp
// Create a consist with 3 locomotives
var leadSlot = 5;   // Locomotive address 100
var memberSlots = new byte[] { 8, 12 }; // Locomotives address 200, 300

// Link them together
var linkCommands = leadSlot.BuildConsist(memberSlots);
foreach (var cmd in linkCommands)
{
    SendCommand(cmd);
    await ReceiveMessage(); // Wait for confirmation
}

// Control entire consist via lead locomotive
SendCommand(new SetLocoSpeedCommand(leadSlot, 64));
SendCommand(new SetLocoDirectionAndFunctionF0toF4Command(
    leadSlot, true, true, false, false, false, false));
// All consist members follow automatically!

// Control individual member (e.g., rear headlight)
var memberFunc = ConsistFunctionCommand.Headlight(12, true, true);
SendCommand(memberFunc);

// Break consist when done
var unlinkCommands = leadSlot.BreakConsist(memberSlots);
foreach (var cmd in unlinkCommands)
{
    SendCommand(cmd);
}
```

### LNCV Programming (Stationary Decoder Configuration)

LNCV (LocoNet Configuration Variables) is an Uhlenbrock extension for configuring stationary LocoNet devices such as accessory decoders, feedback modules, and signal decoders from manufacturers like Uhlenbrock, Digikeijs, etc.

Unlike DCC CV programming (8-bit, targets locomotive decoders on the track), LNCV uses 16-bit CV numbers and values, and communicates over the LocoNet bus.

```csharp
using Tellurian.Trains.Protocols.LocoNet.Commands;
using Tellurian.Trains.Protocols.LocoNet.Lncv;
using Tellurian.Trains.Protocols.LocoNet.Notifications;

// Devices are identified by article number (product code) + module address.
// Article number is the manufacturer's product code (e.g. 6341 for Uhlenbrock 63410).
// Module address is stored in LNCV 0 and defaults to 1.

// 1. Start a programming session (required before read/write)
var startSession = LncvCommand.StartSession(articleNumber: 6341, moduleAddress: 1);
SendCommand(startSession);
// Wait for session acknowledgment (LncvNotification with SessionAcknowledgment type)

// 2. Read an LNCV
var readCmd = LncvCommand.Read(articleNumber: 6341, cvNumber: 5, moduleAddress: 1);
SendCommand(readCmd);
// Wait for LncvNotification with ReadReply type
// notification.CvNumber and notification.CvValue contain the result

// 3. Write an LNCV
var writeCmd = LncvCommand.Write(articleNumber: 6341, cvNumber: 5, value: 42);
SendCommand(writeCmd);
// Wait for LongAcknowledge (0xB4) with ForOperationCode 0xED
// ack.IsSuccess indicates if the write was accepted

// 4. End the programming session
var endSession = new LncvEndSessionCommand(articleNumber: 6341, moduleAddress: 1);
SendCommand(endSession);

// Device discovery: find all devices of a specific type
var discover = LncvCommand.StartSession(articleNumber: 6341, moduleAddress: 0xFFFF);
SendCommand(discover);
// Collect multiple SessionAcknowledgment notifications over a timeout period
// Each response contains the article number and module address of a found device
```

When using the adapter layer (`Tellurian.Trains.Adapters.LocoNet` or `Tellurian.Trains.Adapters.Z21`), the async request/response correlation is handled automatically:

```csharp
// Using the adapter (LocoNet or Z21)
var device = await adapter.StartLncvSessionAsync(6341, moduleAddress: 1);
var lncv = await adapter.ReadLncvAsync(6341, cvNumber: 5, moduleAddress: 1);
Console.WriteLine(lncv); // "LNCV5=42"
var success = await adapter.WriteLncvAsync(6341, cvNumber: 5, value: 100);
await adapter.EndLncvSessionAsync(6341, moduleAddress: 1);

// Discover all devices of a specific type
var devices = await adapter.DiscoverLncvDevicesAsync(6341);
foreach (var d in devices)
    Console.WriteLine(d); // "Article 6341, Module 1"
```

### Error Handling

```csharp
// Parse any message safely
var msg = LocoNetMessageFactory.Create(receivedBytes);

// Check for errors
if (msg is LongAcknowledge ack)
{
    if (ack.IsSuccess)
    {
        Console.WriteLine($"Command accepted: {ack.Message}");
    }
    else if (ack.IsFailure)
    {
        Console.WriteLine($"Command failed: {ack.Message}");
    }
}

// Check slot status
if (msg is SlotNotification slot)
{
    if (slot.Status == SlotStatus.Free)
    {
        Console.WriteLine("No slots available!");
    }
    else if (slot.Data.IsInConsist())
    {
        Console.WriteLine($"Slot is in consist: {slot.Data.Consist.GetConsistRoleDescription()}");
    }
}

// Handle unsupported messages
if (msg is UnsupportedNotification unsupported)
{
    Console.WriteLine($"Unsupported message: {unsupported}");
}
```

## Architecture

### Layer Structure

```
┌─────────────────────────────────┐
│     Application Layer           │
│  (Your Model Railroad Control)  │
└────────────┬────────────────────┘
             │
┌────────────▼────────────────────┐
│   LocoNet Protocol Layer        │
│  • Commands (0xA0, 0xB0, etc.)  │
│  • Notifications (0xE7, 0xB2)   │
│  • Message Factory              │
└────────────┬────────────────────┘
             │
┌────────────▼────────────────────┐
│   Transport Layer               │
│  (Serial, TCP, UDP)             │
│  Tellurian.Communications.*     │
└─────────────────────────────────┘
```

### Transport Options

The LocoNet adapter works with any `ICommunicationsChannel` implementation:

```csharp
// Serial — direct connection via LocoBuffer-USB
var serialChannel = new SerialDataChannel(new SerialPortAdapter("COM3"), new LocoNetFramer(), logger);
var adapter = new Adapter(serialChannel, adapterLogger);

// TCP — LoconetOverTcp (LbServer, JMRI, Rocrail, etc.)
var tcpChannel = new TcpLocoNetChannel(new TcpStreamAdapter("192.168.1.100", 1234), logger);
var adapter = new Adapter(tcpChannel, adapterLogger);

// UDP — loconetd (Glenn Butcher, multicast 225.0.0.2)
var udpAdapter = new UdpLocoNetAdapter(
    multicastGroup: IPAddress.Parse("225.0.0.2"),
    listenPort: 4501,
    sendEndpoint: new IPEndPoint(IPAddress.Parse("192.168.1.50"), 4500));
var udpChannel = new UdpLocoNetChannel(udpAdapter, logger);
var adapter = new Adapter(udpChannel, adapterLogger);

// UDP — GCA101 / Rocrail LocoBuffer-UDP (multicast 224.0.0.1)
var gca101Adapter = new UdpLocoNetAdapter(
    multicastGroup: IPAddress.Parse("224.0.0.1"),
    listenPort: 1235,
    sendEndpoint: new IPEndPoint(IPAddress.Parse("192.168.0.200"), 1235));
var gca101Channel = new UdpLocoNetChannel(gca101Adapter, logger);
var adapter = new Adapter(gca101Channel, adapterLogger);
```

### Key Classes

#### Commands
- `PowerOnCommand`, `PowerOffCommand`, `ForceIdleCommand`
- `GetLocoAddressCommand`, `RequestSlotDataCommand`, `MoveSlotCommand`
- `SetLocoSpeedCommand`, `SetLocoDirectionAndFunctionF0toF4Command`
- `SetAccessoryCommand`, `AccessoryAcknowledgeCommand`, `AccessoryOutputStatusCommand`, `RequestAccessoryStateCommand`
- `ProgrammingCommand`
- `LinkSlotsCommand`, `UnlinkSlotsCommand`, `ConsistFunctionCommand`
- `WriteSlotDataCommand`, `WriteSlotStatus1Command`
- `LncvCommand` - LNCV read/write/start session (static factory methods)
- `LncvEndSessionCommand` - End LNCV programming session

#### Notifications
- `SlotNotification` - Slot data (14 bytes)
- `LongAcknowledge` - Command acknowledgment
- `AccessoryReportNotification` - Switch feedback
- `SensorInputNotification` - Occupancy/sensor reports
- `MultiSenseNotification` - Transponding present/absent events
- `LissyNotification` - LISSY/RailCom locomotive identification
- `MasterBusyNotification` - Network timing
- `LncvNotification` - LNCV read replies and session acknowledgments
- `UnsupportedNotification` - Unknown messages

#### Data Types
- `SlotData` - Complete slot information structure
- `LocoAddress` - Locomotive address (1-9999)
- `AccessoryAddress` - Switch/turnout address (1-2048 user addresses)
- `CV` - Configuration Variable with number (1-1024) and value (0-255)
- `SlotStatus`, `ConsistStatus`, `DecoderType`, `TrackStatus` - Enums
- `ProgrammingMode`, `ProgrammingOperation`, `ProgrammingStatus` - Programming types
- `ProgrammingResult` - Programming response parser
- `Lncv` - LNCV number (0-65535) and value (0-65535)
- `LncvDeviceInfo` - Discovered device article number and module address
- `LncvMessageType` - Enum: `ReadReply`, `SessionAcknowledgment`

#### Utilities
- `LocoNetMessageFactory` - Parse byte arrays into messages
- `Message` - Base class with checksum helpers
- `ProgrammingHelper` - CV encoding/decoding
- `ConsistExtensions` - Consist management extension methods

## API Reference

### Common Patterns

#### Command Generation
All commands inherit from `Command` base class:

```csharp
Command cmd = new SomeCommand(...);
byte[] bytes = cmd.GetBytesWithChecksum();
// Send bytes to LocoNet
```

#### Message Parsing
All received messages parsed by factory:

```csharp
byte[] receivedBytes = ... // from network
Message msg = LocoNetMessageFactory.Create(receivedBytes);

// Pattern match on message type
switch (msg)
{
    case SlotNotification slot:
        // Handle slot
        break;
    case SensorInputNotification sensor:
        // Handle sensor
        break;
    // ... etc
}
```

#### Slot Workflow
Standard pattern for slot operations:

```csharp
// 1. Request
var request = new GetLocoAddressCommand(address);

// 2. Receive
var slot = (SlotNotification)ReceiveMessage();

// 3. Activate if needed
if (slot.Status != SlotStatus.InUse)
{
    SendCommand(MoveSlotCommand.Activate(slot.SlotNumber));
}

// 4. Control
SendCommand(new SetLocoSpeedCommand(slot.SlotNumber, speed));
```

### Message Length Reference

| Length | Bits 6-5 | Example Opcodes |
|--------|----------|-----------------|
| 2 bytes | 00 | 0x82 (POWER OFF), 0x83 (POWER ON), 0x85 (IDLE) |
| 4 bytes | 01 | 0xA0 (SPEED), 0xB0 (SWITCH), 0xB4 (LACK) |
| 6 bytes | 10 | 0xD0 (MULTI_SENSE) |
| Variable | 11 | 0xE5 (LNCV reply - 15 bytes), 0xE7 (SLOT READ - 14 bytes), 0xED (LNCV cmd - 15 bytes), 0xEF (SLOT WRITE - 14 bytes) |

### Opcode Quick Reference

| Opcode | Name | Command/Notification | Description |
|--------|------|----------------------|-------------|
| 0x81 | OPC_BUSY | Notification | Master busy |
| 0x82 | OPC_GPOFF | Command | Global power OFF |
| 0x83 | OPC_GPON | Command | Global power ON |
| 0x85 | OPC_IDLE | Command | Emergency stop |
| 0xA0 | OPC_LOCO_SPD | Command | Set locomotive speed |
| 0xA1 | OPC_LOCO_DIRF | Command | Set direction/F0-F4 |
| 0xA2 | OPC_LOCO_SND | Command | Set functions F5-F8 |
| 0xB0 | OPC_SW_REQ | Command | Switch request |
| 0xB1 | OPC_SW_REP | Command/Notification | Switch output status / report |
| 0xB2 | OPC_INPUT_REP | Notification | Sensor input |
| 0xB4 | OPC_LONG_ACK | Notification | Long acknowledge |
| 0xB5 | OPC_SLOT_STAT1 | Command | Write slot status |
| 0xB6 | OPC_CONSIST_FUNC | Command | Consist function |
| 0xB8 | OPC_UNLINK_SLOTS | Command | Unlink consist |
| 0xB9 | OPC_LINK_SLOTS | Command | Link consist |
| 0xBA | OPC_MOVE_SLOTS | Command | Move/activate slot |
| 0xBB | OPC_RQ_SL_DATA | Command | Request slot data |
| 0xBC | OPC_SW_STATE | Command | Request switch state |
| 0xBD | OPC_SW_ACK | Command | Switch with acknowledge |
| 0xBF | OPC_LOCO_ADR | Command | Request loco address |
| 0xD0 | OPC_MULTI_SENSE | Notification | Transponding present/absent, power management |
| 0xE4 | OPC_LISSY_UPDATE | Notification | LISSY/RailCom loco identification |
| 0xE5 | OPC_PEER_XFER | Notification | Peer transfer / LNCV reply (15 bytes) |
| 0xE7 | OPC_SL_RD_DATA | Notification | Slot data (14 bytes) |
| 0xED | OPC_IMM_PACKET | Command | LNCV command (15 bytes) |
| 0xEF | OPC_WR_SL_DATA | Command | Write slot data (14 bytes) |

## Testing

Run tests:

```bash
dotnet test Tellurian.Protocols.LocoNet.Tests
```

The test suite covers:
- Checksum calculation
- Message parsing
- Command byte generation
- Address encoding/decoding
- Slot data parsing

## Best Practices

### 1. Keep Slots Alive
Slots are automatically purged after ~200 seconds of inactivity:

```csharp
// Send any command every 100 seconds
await Task.Delay(100000);
SendCommand(new SetLocoSpeedCommand(slot, currentSpeed));
```

### 2. Activate Slots Before Control
Always check and activate slots:

```csharp
if (slot.Status != SlotStatus.InUse)
{
    SendCommand(MoveSlotCommand.Activate(slot.SlotNumber));
    // Wait for confirmation
}
```

### 3. Turn Off Switch Outputs
Prevent turnout motor overheating:

```csharp
var switchAddress = AccessoryAddress.From(100);
SendCommand(SetAccessoryCommand.Throw(switchAddress));
await Task.Delay(1000); // Wait for motor
SendCommand(SetAccessoryCommand.TurnOff(switchAddress));
```

### 4. Handle Errors
Always check acknowledgments:

```csharp
if (msg is LongAcknowledge ack)
{
    if (ack.IsFailure)
    {
        Console.WriteLine($"Command failed: {ack.Message}");
        // Retry or handle error
    }
}
```

### 5. Monitor All Messages
LocoNet is peer-to-peer; monitor everything:

```csharp
// Even if you didn't send a command, you'll see
// notifications from other devices
while (true)
{
    var msg = LocoNetMessageFactory.Create(await ReceiveBytes());
    ProcessMessage(msg);
}
```

## Hardware Compatibility

This library is compatible with:
- **Command Stations**: Digitrax DCS100, DCS200, DCS210, DCS240
- **Interfaces**: PR3, PR4, LNWI, RR-CirKits LCC-Buffer
- **TCP Servers**: LbServer, JMRI, Rocrail (via LoconetOverTcp protocol on port 1234)
- **UDP Gateways**: loconetd (Glenn Butcher), GCA101 LocoBuffer-UDP (Rocrail/Peter Giling)
- **Boosters**: DB150, DB200, DB210, DB220
- **Throttles**: DT400, DT500, UT4
- **Decoders**: Any NMRA DCC-compliant decoder

## Protocol Specification

Based on:
- **LocoNet Personal Use Edition 1.0** by Digitrax Inc. (October 16, 1997)
- For non-commercial private use only

## Contributing

Contributions welcome! Please:
1. Follow existing code style (see `Specifications/coding-conventions.md`)
2. Add XML documentation to public APIs
3. Include unit tests for new features
4. Update this README with new examples

## License

This library is part of the Tellurian.Trains project.

## Support

For issues, questions, or contributions:
- GitHub Issues: [Tellurian.Trains.Control](https://github.com/tellurianinteractive/Tellurian.Trains.Control)

## Acknowledgments

- Digitrax Inc. for the LocoNet specification
- NMRA for DCC standards
- The model railroad community

---

**Happy Railroading! 🚂**
