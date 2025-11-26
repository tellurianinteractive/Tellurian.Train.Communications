# Tellurian.Trains.Protocols.XpressNet

A complete C# implementation of the XpressNet protocol for digital model railroad control.

## Overview

This library provides a comprehensive, type-safe implementation of the XpressNet protocol, enabling full control of DCC model railroad systems through XpressNet-compatible command stations (Lenz LZ100/LZV100, Roco Z21, etc.).

XpressNet is a serial bus protocol developed by Lenz Elektronik GmbH that provides a standardized interface between throttles, computer interfaces, and DCC command stations.

## Features

### Fully Implemented

#### Locomotive Control
- Speed control with 14, 27, 28, and 126 speed steps
- Direction control (forward/backward)
- Function control (F0-F28)
- Emergency stop (per-locomotive and global)
- Short address support (1-127)
- Long address support (128-9999)
- Locomotive state queries

#### Track Power Management
- Track power on/off
- Emergency stop (all locomotives)
- Short circuit detection and notification
- Power-up mode configuration (manual/automatic)

#### Accessory Control
- Turnout/accessory control (addresses 1-1024)
- Accessory decoder information requests
- Feedback broadcast notifications
- Turnout status queries

#### Programming - Service Mode
- Register Mode read/write (registers 1-8)
- Direct CV Mode read/write (CVs 1-256)
- Paged Mode read/write (CVs 1-256)
- Service mode result polling
- Programming status notifications (ready, busy, short circuit, timeout)

#### Programming - Operations Mode (POM)
- Byte mode write (CVs 1-1024)
- Bit mode write (CVs 1-1024)
- Full locomotive address range (1-9999)

#### Double Header Operations
- Establish Double Header (join two locomotives)
- Dissolve Double Header
- Full address range support (1-9999)

#### Multi-Unit (Consist) Operations
- Add locomotive to Multi-Unit (with direction control)
- Remove locomotive from Multi-Unit
- Multi-Unit address range (1-99)

#### Address Search & Stack
- Multi-Unit member inquiry (forward/backward search)
- Multi-Unit address inquiry
- Command station stack inquiry
- Delete locomotive from stack

#### Function Status
- Query function status (momentary vs on/off)
- Set function state for F0-F12
- Per-function momentary/on-off configuration

#### System Monitoring
- Hardware and firmware version detection
- Command station status queries
- Broadcast subject configuration
- Transfer error detection
- Command station busy notification

## Installation

```bash
dotnet add package Tellurian.Trains.Protocols.XpressNet
```

Or add to your `.csproj`:

```xml
<PackageReference Include="Tellurian.Trains.Protocols.XpressNet" Version="1.0.0" />
```

## Quick Start

### Basic Locomotive Control

```csharp
using Tellurian.Trains.Protocols.XpressNet;
using Tellurian.Trains.Protocols.XpressNet.Commands;
using Tellurian.Trains.Protocols.XpressNet.Notifications;

// Create locomotive address
var address = new LocoAddress(3);

// Create speed/direction command (126 speed steps, speed 64, forward)
var driveCmd = new LocoDriveCommand(address, new LocoSpeed(64, SpeedSteps.S126), LocoDirection.Forward);
byte[] message = driveCmd.GetBytesWithChecksum();
// Send message to command station...

// Emergency stop a specific locomotive
var stopCmd = new LocoEmergencyStopCommand(address);

// Parse incoming notifications
var notification = NotificationFactory.Create(receivedBytes);
if (notification is LocoInfoNotification locoInfo)
{
    Console.WriteLine($"Loco {locoInfo.Address}: Speed={locoInfo.Speed}, Direction={locoInfo.Direction}");
}
```

### Track Power Control

```csharp
// Turn track power on
var powerOn = new TrackPowerOnCommand();
SendCommand(powerOn);

// Turn track power off
var powerOff = new TrackPowerOffCommand();
SendCommand(powerOff);

// Emergency stop all locomotives
var emergencyStop = new EmergencyStopCommand();
SendCommand(emergencyStop);
```

### Function Control

```csharp
var address = new LocoAddress(3);

// Set functions F0-F4 (lights and sound)
var funcGroup1 = new LocoFunctionCommand(address, FunctionGroup.Group1,
    f0: true,  // Headlight
    f1: true,  // Bell
    f2: false, f3: false, f4: false);
SendCommand(funcGroup1);

// Set functions F5-F8
var funcGroup2 = new LocoFunctionCommand(address, FunctionGroup.Group2,
    f5: true,  // Horn
    f6: false, f7: false, f8: false);
SendCommand(funcGroup2);
```

### Accessory/Turnout Control

```csharp
// Switch turnout 100 to diverging
var turnout = new AccessoryFunctionCommand(new AccessoryAddress(100), AccessoryOutput.Output1, true);
SendCommand(turnout);

// Query turnout status
var query = new AccessoryInfoRequestCommand(new AccessoryAddress(100));
SendCommand(query);

// Handle response
if (notification is AccessoryDecoderInfoNotification info)
{
    Console.WriteLine($"Turnout status: {info.FirstTurnoutStatus}");
}
```

### CV Programming (Service Mode)

```csharp
// Read CV29 using Direct Mode
var readCmd = new ServiceModeReadDirectCommand(29);
SendCommand(readCmd);

// Poll for results
var resultsCmd = new ServiceModeResultsCommand();
SendCommand(resultsCmd);

// Handle response
if (notification is ServiceModeDirectCVNotification cvResponse)
{
    Console.WriteLine($"CV{cvResponse.CvNumber} = {cvResponse.Value}");
}

// Write CV3 = 10 using Direct Mode
var writeCmd = new ServiceModeWriteDirectCommand(3, 10);
SendCommand(writeCmd);
```

### CV Programming (Operations Mode - POM)

```csharp
var address = new LocoAddress(1234);

// Write CV29 = 6 while locomotive is on main track
var pomWrite = new PomWriteByteCommand(address, 29, 6);
SendCommand(pomWrite);

// Write single bit (CV29 bit 5 = 1)
var pomBit = new PomWriteBitCommand(address, 29, 5, true);
SendCommand(pomBit);
```

### Double Header Operations

```csharp
// Create a double header with locomotives 100 and 200
var establish = new EstablishDoubleHeaderCommand(
    new LocoAddress(100),
    new LocoAddress(200));
SendCommand(establish);

// Dissolve the double header
var dissolve = new DissolveDoubleHeaderCommand(new LocoAddress(100));
SendCommand(dissolve);
```

### Multi-Unit (Consist) Operations

```csharp
// Add locomotive 100 to Multi-Unit 10 (same direction)
var addLoco = new AddLocoToMultiUnitCommand(
    new LocoAddress(100),
    multiUnitAddress: 10,
    reversed: false);
SendCommand(addLoco);

// Add locomotive 200 reversed
var addReversed = new AddLocoToMultiUnitCommand(
    new LocoAddress(200),
    multiUnitAddress: 10,
    reversed: true);
SendCommand(addReversed);

// Remove locomotive from Multi-Unit
var removeLoco = new RemoveLocoFromMultiUnitCommand(
    new LocoAddress(100),
    multiUnitAddress: 10);
SendCommand(removeLoco);
```

## Core Concepts

### Message Structure

All XpressNet messages follow this format:

```
[Header] [Data Byte 1] [Data Byte 2] ... [XOR Checksum]
```

- **Header**: Encodes message type and data length (upper nibble = 0x0-0xF for length)
- **Data Bytes**: Message-specific payload
- **Checksum**: XOR of all preceding bytes

### Checksum Calculation

```csharp
// Automatic checksum via GetBytesWithChecksum()
byte[] message = command.GetBytesWithChecksum();

// Manual validation via Packet class
var packet = new Packet(receivedBytes);
if (packet.IsValid)
{
    // Process message
}
```

### Address Encoding

**Locomotive Addresses:**
- Short (1-127): Single byte, AH=0x00
- Long (128-9999): Two bytes with 0xC0 flag in high byte

```csharp
var shortAddr = new LocoAddress(3);      // AH=0x00, AL=0x03
var longAddr = new LocoAddress(1234);    // AH=0xC4, AL=0xD2
byte[] bytes = longAddr.GetBytesAccordingToXpressNet();
```

**Accessory Addresses:**
- Range: 1-1024
- Encoded as group address (address/4) and subaddress (address%4)

### Speed Steps

Four speed step modes are supported:

| Mode | Steps | Encoding |
|------|-------|----------|
| 14 | 0-14 + E-Stop | 4 bits |
| 27 | 0-27 + E-Stop | 5 bits |
| 28 | 0-28 + E-Stop | 5 bits |
| 126 | 0-126 + E-Stop | 7 bits |

```csharp
var speed14 = new LocoSpeed(10, SpeedSteps.S14);
var speed126 = new LocoSpeed(64, SpeedSteps.S126);
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
│   XpressNet Protocol Layer      │
│  • Commands (outgoing)          │
│  • Notifications (incoming)     │
│  • NotificationFactory          │
└────────────┬────────────────────┘
             │
┌────────────▼────────────────────┐
│   Transport Layer               │
│  (UDP via Z21, Serial via LI)   │
│  Tellurian.Communications.*     │
└─────────────────────────────────┘
```

### Key Classes

#### Commands
- **Locomotive**: `LocoDriveCommand`, `LocoFunctionCommand`, `LocoEmergencyStopCommand`, `GetLocoInfoCommand`
- **Track Power**: `TrackPowerOnCommand`, `TrackPowerOffCommand`, `EmergencyStopCommand`
- **Accessories**: `AccessoryFunctionCommand`, `AccessoryInfoRequestCommand`
- **Service Mode**: `ServiceModeReadDirectCommand`, `ServiceModeWriteDirectCommand`, `ServiceModeResultsCommand`
- **Operations Mode**: `PomWriteByteCommand`, `PomWriteBitCommand`
- **Double Header**: `EstablishDoubleHeaderCommand`, `DissolveDoubleHeaderCommand`
- **Multi-Unit**: `AddLocoToMultiUnitCommand`, `RemoveLocoFromMultiUnitCommand`
- **Address Search**: `AddressInquiryStackCommand`, `AddressInquiryMultiUnitCommand`, `DeleteLocoFromStackCommand`
- **System**: `GetVersionCommand`, `GetStatusCommand`, `SetPowerUpModeCommand`

#### Notifications
- **Locomotive**: `LocoInfoNotification`, `LocoOperatedByAnotherDeviceNotification`
- **Track Power**: `TrackPowerOnBroadcast`, `TrackPowerOffBroadcast`, `EmergencyStopBroadcast`
- **Accessories**: `AccessoryDecoderInfoNotification`, `FeedbackBroadcast`
- **Programming**: `ServiceModeDirectCVNotification`, `ServiceModeRegisterPagedNotification`
- **System**: `VersionNotification`, `StatusChangedNotification`, `TransferErrorNotification`, `CommandStationBusyNotification`
- **Multi-Unit/DH**: `MUDHErrorNotification`, `AddressRetrievalNotification`
- **Function Status**: `FunctionStatusNotification`

#### Data Types
- `LocoAddress` - Locomotive address (1-9999)
- `AccessoryAddress` - Accessory/turnout address (1-1024)
- `LocoSpeed` - Speed with step mode
- `LocoDirection` - Forward/Backward/Unchanged
- `LocoFunctionStates` - F0-F28 states
- `CvAddress` - CV number (1-1024)

## Error Handling

### Programming Errors

```csharp
var notification = NotificationFactory.Create(buffer);

switch (notification)
{
    case WriteCVShortCircuitResponse:
        Console.WriteLine("Short circuit on programming track!");
        break;
    case WriteCVTimeoutResponse:
        Console.WriteLine("No decoder acknowledgment received");
        break;
    case ProgrammingStationBusyBroadcast:
        Console.WriteLine("Programming station busy, retry later");
        break;
}
```

### Multi-Unit/Double Header Errors

```csharp
if (notification is MUDHErrorNotification error)
{
    Console.WriteLine($"MU/DH Error: {error.ErrorMessage}");
    // Possible errors:
    // - NotOperatedByDevice
    // - OperatedByAnotherDevice
    // - AlreadyInConsist
    // - SpeedNotZero
    // - NotInMultiUnit
    // - StackFull
}
```

### System Errors

```csharp
switch (notification)
{
    case TransferErrorNotification:
        Console.WriteLine("Checksum error - resend command");
        break;
    case CommandStationBusyNotification:
        Console.WriteLine("Command station busy - retry");
        break;
    case UnknownCommandNotification:
        Console.WriteLine("Command not supported by this station");
        break;
}
```

## Testing

Run tests:

```bash
dotnet test Tellurian.Trains.Protocols.XpressNet.Tests
```

The test suite (156 tests) covers:
- Command byte generation
- Notification parsing
- Checksum validation
- Address encoding
- Speed step conversions
- Error handling

## Hardware Compatibility

This library is compatible with:

**Command Stations:**
- Lenz LZ100, LZV100, LZV200
- Roco Z21, z21 (via XpressNet tunneling)
- Other XpressNet 3.0+ compatible stations

**Interfaces:**
- Lenz LI100, LI100F, LI101F
- Roco Z21 (UDP/LAN)

## Protocol Specification

Based on:
- **XpressNet Protocol Specification** by Lenz Elektronik GmbH
- Covers XpressNet versions up to 3.6

## Contributing

Contributions welcome! Please:
1. Follow existing code style
2. Add XML documentation to public APIs
3. Include unit tests for new features
4. Update documentation

## License

This library is part of the Tellurian.Trains project.

## Support

For issues, questions, or contributions:
- GitHub Issues: [Tellurian.Trains.Control](https://github.com/tellurianinteractive/Tellurian.Trains.Control)

## See Also

- [API-SPECIFICATION.md](API-SPECIFICATION.md) - Detailed API reference
- [CLAUDE.md](CLAUDE.md) - Development guidelines
- [Specifications/](Specifications/) - Protocol specification documents
