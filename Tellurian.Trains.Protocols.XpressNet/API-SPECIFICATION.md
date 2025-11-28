# XpressNet API Quick Reference

## Table of Contents
1. [Message Parsing](#message-parsing)
2. [Track Power Control](#track-power-control)
3. [Locomotive Control](#locomotive-control)
4. [Function Control](#function-control)
5. [Accessory Control](#accessory-control)
6. [Programming - Service Mode](#programming---service-mode)
7. [Programming - Operations Mode](#programming---operations-mode)
8. [Double Header Operations](#double-header-operations)
9. [Multi-Unit Operations](#multi-unit-operations)
10. [Address Search](#address-search)
11. [System Operations](#system-operations)
12. [Data Types](#data-types)
13. [Error Handling](#error-handling)

---

## Message Parsing

### Parse Incoming Messages

```csharp
using Tellurian.Trains.Protocols.XpressNet.Notifications;

byte[] receivedBytes = ...; // from XpressNet
Notification notification = NotificationFactory.Create(receivedBytes);

// Type checking
if (notification is LocoInfoNotification locoInfo) { /* ... */ }
if (notification is TrackPowerOnBroadcast) { /* power is on */ }
if (notification is ServiceModeDirectCVNotification cvResult) { /* ... */ }
```

### Generate Outgoing Commands

```csharp
using Tellurian.Trains.Protocols.XpressNet.Commands;

Command cmd = new TrackPowerOnCommand();
byte[] bytes = cmd.GetBytesWithChecksum();
// Send bytes to command station
```

### Validate Packets

```csharp
// Check checksum validity
var packet = new Packet(receivedBytes);
if (packet.IsValid)
{
    // Process message
}
```

---

## Track Power Control

### Power ON/OFF

```csharp
using Tellurian.Trains.Protocols.XpressNet.Commands;

// Turn track power ON
var powerOn = new TrackPowerOnCommand();
byte[] onBytes = powerOn.GetBytesWithChecksum();
// Sends: [0x21, 0x81, checksum]

// Turn track power OFF
var powerOff = new TrackPowerOffCommand();
byte[] offBytes = powerOff.GetBytesWithChecksum();
// Sends: [0x21, 0x80, checksum]
```

### Emergency Stop

```csharp
// Stop all locomotives immediately
var emergencyStop = new EmergencyStopCommand();
byte[] stopBytes = emergencyStop.GetBytesWithChecksum();
// Sends: [0x80, checksum]

// Stop specific locomotive
var address = new LocoAddress(3);
var locoStop = new LocoEmergencyStopCommand(address);
```

### Handle Power Notifications

```csharp
switch (notification)
{
    case TrackPowerOnBroadcast:
        Console.WriteLine("Track power is ON");
        break;
    case TrackPowerOffBroadcast:
        Console.WriteLine("Track power is OFF");
        break;
    case EmergencyStopBroadcast:
        Console.WriteLine("Emergency stop activated");
        break;
    case TrackShortCircuitNotification:
        Console.WriteLine("Short circuit detected!");
        break;
}
```

---

## Locomotive Control

### Request Locomotive Information

```csharp
var address = new LocoAddress(3);
var request = new GetLocoInfoCommand(address);
byte[] requestBytes = request.GetBytesWithChecksum();
```

**Response:** `LocoInfoNotification`

```csharp
if (notification is LocoInfoNotification locoInfo)
{
    ushort address = locoInfo.Address.Number;
    LocoDirection direction = locoInfo.Direction;
    LocoSpeed speed = locoInfo.Speed;
    bool controlledByOther = locoInfo.IsControlledByOtherDevice;

    // Get all function states (F0-F28)
    var functions = locoInfo.Functions();
    foreach (var (funcNum, isOn) in functions)
    {
        Console.WriteLine($"F{funcNum} = {isOn}");
    }
}
```

### Set Speed and Direction

```csharp
var address = new LocoAddress(1234);

// Create speed with specific step mode
var speed = new LocoSpeed(64, SpeedSteps.S126);

// Create drive command
var driveCmd = new LocoDriveCommand(address, speed, LocoDirection.Forward);
byte[] driveBytes = driveCmd.GetBytesWithChecksum();
```

### Speed Step Modes

```csharp
// 14 speed steps (legacy)
var speed14 = new LocoSpeed(10, SpeedSteps.S14);

// 27 speed steps
var speed27 = new LocoSpeed(20, SpeedSteps.S27);

// 28 speed steps (common)
var speed28 = new LocoSpeed(25, SpeedSteps.S28);

// 126 speed steps (high resolution)
var speed126 = new LocoSpeed(100, SpeedSteps.S126);
```

### Address Encoding

```csharp
// Short address (1-127)
var shortAddr = new LocoAddress(3);
// Encoded as: AH=0x00, AL=0x03

// Long address (128-9999)
var longAddr = new LocoAddress(1234);
// Encoded as: AH=0xC4, AL=0xD2 (with 0xC0 flag in high byte)

// Get XpressNet-encoded bytes
byte[] addrBytes = longAddr.GetBytesAccordingToXpressNet();
```

---

## Function Control

### Set Functions (Grouped)

```csharp
var address = new LocoAddress(3);

// Functions F0-F4 (Group 1)
var funcGroup1 = new LocoFunctionCommand(address, FunctionGroup.Group1,
    f0: true,   // Headlight
    f1: true,   // Bell
    f2: false,
    f3: false,
    f4: false);
SendCommand(funcGroup1);

// Functions F5-F8 (Group 2)
var funcGroup2 = new LocoFunctionCommand(address, FunctionGroup.Group2,
    f5: true,   // Horn
    f6: false,
    f7: false,
    f8: false);
SendCommand(funcGroup2);

// Functions F9-F12 (Group 3)
var funcGroup3 = new LocoFunctionCommand(address, FunctionGroup.Group3,
    f9: false,
    f10: false,
    f11: false,
    f12: false);
SendCommand(funcGroup3);

// Functions F13-F20 (Group 4)
var funcGroup4 = new LocoFunctionCommand(address, FunctionGroup.Group4,
    f13: false, f14: false, f15: false, f16: false,
    f17: false, f18: false, f19: false, f20: false);
SendCommand(funcGroup4);

// Functions F21-F28 (Group 5)
var funcGroup5 = new LocoFunctionCommand(address, FunctionGroup.Group5,
    f21: false, f22: false, f23: false, f24: false,
    f25: false, f26: false, f27: false, f28: false);
SendCommand(funcGroup5);
```

### Query Function Status (Momentary vs On/Off)

```csharp
// Query function status
var statusQuery = new GetFunctionStatusCommand(address);
SendCommand(statusQuery);

// Handle response
if (notification is FunctionStatusNotification funcStatus)
{
    // Check if functions are momentary (true) or on/off (false)
    bool f0Momentary = funcStatus.F0Momentary;
    bool f1Momentary = funcStatus.F1Momentary;
    // ... up to F12
}
```

### Set Function State (Momentary/On-Off)

```csharp
// Set function states for F0-F4
var setState1 = new SetFunctionStateGroup1Command(address,
    f0Momentary: false,  // F0 is on/off type
    f1Momentary: true,   // F1 is momentary
    f2Momentary: false,
    f3Momentary: false,
    f4Momentary: false);
SendCommand(setState1);
```

---

## Accessory Control

### Control Turnouts

```csharp
using Tellurian.Trains.Protocols.XpressNet.Commands;

// Switch turnout 100 to diverging (Output 1)
var turnoutCmd = new AccessoryFunctionCommand(
    AccessoryAddress.From(100),
    AccessoryOutput.Output1,
    activate: true);
SendCommand(turnoutCmd);

// Switch turnout 100 to straight (Output 2)
var straightCmd = new AccessoryFunctionCommand(
    AccessoryAddress.From(100),
    AccessoryOutput.Output2,
    activate: true);
SendCommand(straightCmd);
```

### Query Accessory Status

```csharp
var query = new AccessoryInfoRequestCommand(AccessoryAddress.From(100));
SendCommand(query);

// Handle response
if (notification is AccessoryDecoderInfoNotification info)
{
    TurnoutStatus status1 = info.FirstTurnoutStatus;
    TurnoutStatus status2 = info.SecondTurnoutStatus;
    AccessoryDecoderType decoderType = info.DecoderType;

    Console.WriteLine($"Turnout 1: {status1}");
    // Possible values: NotDefined, ThrownLeft, ClosedRight
}
```

### Handle Feedback Broadcasts

```csharp
if (notification is FeedbackBroadcast feedback)
{
    // Multiple feedback pairs can be in one broadcast
    foreach (var pair in feedback.Pairs)
    {
        ushort address = pair.Address;
        byte status = pair.Status;
        // Process feedback
    }
}
```

---

## Programming - Service Mode

### Read CV (Direct Mode)

```csharp
using Tellurian.Trains.Protocols.XpressNet.Decoder;

// Read CV29 using Direct Mode
var readCmd = new ServiceModeReadDirectCommand(29);
SendCommand(readCmd);

// Poll for results
var resultsCmd = new ServiceModeResultsCommand();
SendCommand(resultsCmd);

// Handle response
if (notification is ServiceModeDirectCVNotification cvResponse)
{
    // Use the CV property for convenience
    Console.WriteLine($"{cvResponse.CV}");  // "CV29=value"

    // Or access individual properties
    ushort cvNumber = cvResponse.CvNumber;
    byte cvValue = cvResponse.Value;
}
```

### Read CV (Register Mode)

```csharp
// Read register 1-8
var readReg = new ServiceModeReadRegisterCommand(1);
SendCommand(readReg);

// Handle response
if (notification is ServiceModeRegisterPagedNotification regResponse)
{
    byte value = regResponse.Value;
}
```

### Read CV (Paged Mode)

```csharp
// Read CV using Paged Mode (CV 1-256)
var readPaged = new ServiceModeReadPagedCommand(29);
SendCommand(readPaged);
```

### Write CV (Direct Mode)

```csharp
// Write CV3 = 10 using Direct Mode
var writeCmd = new ServiceModeWriteDirectCommand(3, 10);
SendCommand(writeCmd);
```

### Write CV (Register Mode)

```csharp
// Write register 1 = 3 (primary address)
var writeReg = new ServiceModeWriteRegisterCommand(1, 3);
SendCommand(writeReg);
```

### Write CV (Paged Mode)

```csharp
// Write CV using Paged Mode
var writePaged = new ServiceModeWritePagedCommand(29, 6);
SendCommand(writePaged);
```

### Handle Programming Status

```csharp
switch (notification)
{
    case ProgrammingStationReadyBroadcast:
        Console.WriteLine("Programming station ready");
        break;
    case ProgrammingStationBusyBroadcast:
        Console.WriteLine("Programming station busy - retry later");
        break;
    case ProgrammingModeEnteredBroadcast:
        Console.WriteLine("Entered programming mode");
        break;
    case WriteCVShortCircuitResponse:
        Console.WriteLine("Short circuit on programming track!");
        break;
    case WriteCVTimeoutResponse:
        Console.WriteLine("No decoder acknowledgment received");
        break;
    case CVOkResponse:
        Console.WriteLine("CV operation successful");
        break;
}
```

---

## Programming - Operations Mode

### POM Byte Write

```csharp
using Tellurian.Trains.Interfaces.Decoder;
using Tellurian.Trains.Protocols.XpressNet.Decoder;

var address = new LocoAddress(1234);

// Write CV29 = 6 on main track using CV struct
var pomWrite = new ProgramOnMainWriteByteCommand(address, new CV(29, 6));
SendCommand(pomWrite);
```

### POM Bit Write

```csharp
var address = new LocoAddress(1234);

// Write bit 5 of CV29 = 1
var writeBit = new ProgramOnMainWriteBitCommand(address, 29, bitPosition: 5, bitValue: true);
SendCommand(writeBit);

// Bit position 0-7 (0 = LSB, 7 = MSB)
```

### CV Range for POM

```csharp
// ProgramOnMain supports CV 1-1024
var cvLow = new ProgramOnMainWriteByteCommand(address, new CV(1, 3));
var cvHigh = new ProgramOnMainWriteByteCommand(address, new CV(1024, 0));
```

---

## Double Header Operations

### Establish Double Header

```csharp
using Tellurian.Trains.Protocols.XpressNet.Commands;

// Join locomotive 100 and 200 into a Double Header
var establish = new EstablishDoubleHeaderCommand(
    new LocoAddress(100),
    new LocoAddress(200));
SendCommand(establish);

// Once established, speed/direction commands to either address
// are sent to both locomotives by the command station
```

### Dissolve Double Header

```csharp
// Remove locomotive 100 from its Double Header
var dissolve = new DissolveDoubleHeaderCommand(new LocoAddress(100));
SendCommand(dissolve);
```

### Handle Double Header Errors

```csharp
if (notification is MUDHErrorNotification error)
{
    MUDHErrorCode errorCode = error.ErrorCode;
    string message = error.ErrorMessage;

    switch (errorCode)
    {
        case MUDHErrorCode.NotOperatedByDevice:
            Console.WriteLine("Locomotive not controlled by this device");
            break;
        case MUDHErrorCode.OperatedByAnotherDevice:
            Console.WriteLine("Locomotive controlled by another device");
            break;
        case MUDHErrorCode.AlreadyInMUDH:
            Console.WriteLine("Locomotive already in MU or DH");
            break;
        case MUDHErrorCode.SpeedNotZero:
            Console.WriteLine("Locomotive speed must be zero");
            break;
        case MUDHErrorCode.NotInMultiUnit:
            Console.WriteLine("Locomotive not in Multi-Unit");
            break;
        case MUDHErrorCode.NotInDoubleHeader:
            Console.WriteLine("Locomotive not in Double Header");
            break;
        case MUDHErrorCode.CannotDeleteLoco:
            Console.WriteLine("Cannot delete locomotive");
            break;
        case MUDHErrorCode.StackFull:
            Console.WriteLine("Command station stack is full");
            break;
    }
}
```

---

## Multi-Unit Operations

### Add Locomotive to Multi-Unit

```csharp
using Tellurian.Trains.Protocols.XpressNet.Commands;

// Add locomotive 100 to Multi-Unit address 10 (same direction)
var addLoco = new AddLocoToMultiUnitCommand(
    new LocoAddress(100),
    multiUnitAddress: 10,
    reversed: false);
SendCommand(addLoco);

// Add locomotive 200 to Multi-Unit 10 (reversed direction)
var addReversed = new AddLocoToMultiUnitCommand(
    new LocoAddress(200),
    multiUnitAddress: 10,
    reversed: true);
SendCommand(addReversed);
```

### Remove Locomotive from Multi-Unit

```csharp
// Remove locomotive 100 from Multi-Unit 10
var removeLoco = new RemoveLocoFromMultiUnitCommand(
    new LocoAddress(100),
    multiUnitAddress: 10);
SendCommand(removeLoco);
```

### Multi-Unit Address Range

```csharp
// Multi-Unit addresses are 1-99
var valid = new AddLocoToMultiUnitCommand(
    new LocoAddress(100),
    multiUnitAddress: 99,  // Maximum valid
    reversed: false);
```

---

## Address Search

### Search Command Station Stack

```csharp
using Tellurian.Trains.Protocols.XpressNet.Commands;

// Get first address in stack
var getFirst = new AddressInquiryStackCommand();
SendCommand(getFirst);

// Search forward from address 1234
var searchForward = new AddressInquiryStackCommand(
    new LocoAddress(1234),
    SearchDirection.Forward);
SendCommand(searchForward);

// Search backward
var searchBackward = new AddressInquiryStackCommand(
    new LocoAddress(1234),
    SearchDirection.Backward);
SendCommand(searchBackward);
```

### Search Multi-Unit Members

```csharp
// Get first member of Multi-Unit 10
var getMember = new AddressInquiryMultiUnitMemberCommand(
    multiUnitAddress: 10);
SendCommand(getMember);

// Search for next member after address 100
var searchMember = new AddressInquiryMultiUnitMemberCommand(
    multiUnitAddress: 10,
    startAddress: new LocoAddress(100),
    direction: SearchDirection.Forward);
SendCommand(searchMember);
```

### Search Multi-Unit Addresses

```csharp
// Get first Multi-Unit address in use
var getFirstMU = new AddressInquiryMultiUnitCommand(0);
SendCommand(getFirstMU);

// Search for next Multi-Unit after address 10
var searchMU = new AddressInquiryMultiUnitCommand(
    startAddress: 10,
    direction: SearchDirection.Forward);
SendCommand(searchMU);
```

### Delete Locomotive from Stack

```csharp
// Remove locomotive 1234 from command station stack
var delete = new DeleteLocoFromStackCommand(new LocoAddress(1234));
SendCommand(delete);
```

### Handle Address Retrieval Response

```csharp
if (notification is AddressRetrievalNotification result)
{
    if (result.AddressFound)
    {
        LocoAddress? address = result.LocoAddress;
        AddressType type = result.AddressType;

        switch (type)
        {
            case AddressType.NormalLoco:
                Console.WriteLine($"Normal locomotive: {address?.Number}");
                break;
            case AddressType.InDoubleHeader:
                Console.WriteLine($"In Double Header: {address?.Number}");
                break;
            case AddressType.MultiUnitBase:
                Console.WriteLine($"Multi-Unit base address: {address?.Number}");
                break;
            case AddressType.InMultiUnit:
                Console.WriteLine($"In Multi-Unit: {address?.Number}");
                break;
        }
    }
    else
    {
        Console.WriteLine("No address found");
    }
}
```

### Handle "Operated by Another Device"

```csharp
if (notification is LocoOperatedByAnotherDeviceNotification lostControl)
{
    LocoAddress address = lostControl.LocoAddress;
    Console.WriteLine($"Lost control of locomotive {address.Number}");
}
```

---

## System Operations

### Get Command Station Version

```csharp
var versionCmd = new GetVersionCommand();
SendCommand(versionCmd);

// Handle response
if (notification is VersionNotification version)
{
    byte majorVersion = version.XpressNetVersion;
    byte commandStationType = version.CommandStationType;
}
```

### Get Command Station Status

```csharp
var statusCmd = new GetStatusCommand();
SendCommand(statusCmd);

// Handle response
if (notification is StatusChangedNotification status)
{
    // Check status flags
}
```

### Get Firmware Version

```csharp
var firmwareCmd = new GetFirmwareVersionCommand();
SendCommand(firmwareCmd);

// Handle response
if (notification is FirmwareNotification firmware)
{
    byte majorVersion = firmware.MajorVersion;
    byte minorVersion = firmware.MinorVersion;
}
```

### Configure Power-Up Mode

```csharp
using Tellurian.Trains.Protocols.XpressNet.Commands;

// Set automatic power-up (track power on at startup)
var autoMode = new SetPowerUpModeCommand(PowerUpMode.Automatic);
SendCommand(autoMode);

// Set manual power-up (track power off at startup)
var manualMode = new SetPowerUpModeCommand(PowerUpMode.Manual);
SendCommand(manualMode);
```

### Configure Broadcast Subjects

```csharp
// Query current broadcast settings
var getBroadcasts = new GetBroadcastSubjectsCommand();
SendCommand(getBroadcasts);

// Handle response
if (notification is BroadcastSubjectNotification subjects)
{
    // Check which broadcasts are enabled
}
```

---

## Data Types

### LocoAddress

```csharp
// Short address (1-127)
var shortAddr = new LocoAddress(3);
bool isShort = shortAddr.Number <= 127;

// Long address (128-9999)
var longAddr = new LocoAddress(1234);
bool isLong = longAddr.Number > 127;

// Properties
ushort number = longAddr.Number;

// Get protocol-encoded bytes
byte[] encoded = longAddr.GetBytesAccordingToXpressNet();
// For 1234: [0xC4, 0xD2] - with 0xC0 flag

// Create from bytes
var parsed = new LocoAddress([0xC4, 0xD2]);
```

### AccessoryAddress

```csharp
// Accessory addresses (1-1024)
var turnout = AccessoryAddress.From(100);
ushort value = turnout.Value;
```

### LocoSpeed

```csharp
// Create with specific step mode
var speed = new LocoSpeed(64, SpeedSteps.S126);

byte current = speed.Current;      // Current speed value
byte code = speed.Code;            // Step mode code
SpeedSteps steps = speed.Steps;    // Step mode enum

// From code byte
var decoded = LocoSpeed.FromCode(0x04); // S126 mode
```

### CV (Configuration Variable)

The `CV` struct from `Tellurian.Trains.Interfaces.Decoder` holds both the CV number and value:

```csharp
using Tellurian.Trains.Interfaces.Decoder;

// Create CV using constructor
var cv = new CV(29, 6);  // CV29 = 6

// Create CV using extension method
var cv2 = 29.CV(6);  // CV29 = 6

// Properties
int number = cv.Number;   // 1-1024
byte value = cv.Value;    // 0-255

// Display
Console.WriteLine(cv.ToString());  // "CV29=6"
```

### Enums

```csharp
// Speed step modes
SpeedSteps.S14     // 14 speed steps
SpeedSteps.S27     // 27 speed steps
SpeedSteps.S28     // 28 speed steps
SpeedSteps.S126    // 126 speed steps

// Direction
LocoDirection.Forward
LocoDirection.Backward

// Accessory output
AccessoryOutput.Output1  // Diverging/Thrown
AccessoryOutput.Output2  // Straight/Closed

// Function groups
FunctionGroup.Group1  // F0-F4
FunctionGroup.Group2  // F5-F8
FunctionGroup.Group3  // F9-F12
FunctionGroup.Group4  // F13-F20
FunctionGroup.Group5  // F21-F28

// Power-up mode
PowerUpMode.Manual     // Track power off at startup
PowerUpMode.Automatic  // Track power on at startup

// Search direction
SearchDirection.Forward
SearchDirection.Backward

// Address type (from search results)
AddressType.NormalLoco
AddressType.InDoubleHeader
AddressType.MultiUnitBase
AddressType.InMultiUnit
AddressType.NotFound

// Turnout status
TurnoutStatus.NotDefined
TurnoutStatus.ThrownLeft
TurnoutStatus.ClosedRight

// MU/DH error codes
MUDHErrorCode.NotOperatedByDevice
MUDHErrorCode.OperatedByAnotherDevice
MUDHErrorCode.AlreadyInMUDH
MUDHErrorCode.SpeedNotZero
MUDHErrorCode.NotInMultiUnit
MUDHErrorCode.NotInDoubleHeader
MUDHErrorCode.CannotDeleteLoco
MUDHErrorCode.StackFull
```

---

## Error Handling

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

### Programming Errors

```csharp
switch (notification)
{
    case WriteCVShortCircuitResponse:
        Console.WriteLine("Short circuit on programming track!");
        break;
    case WriteCVTimeoutResponse:
        Console.WriteLine("No decoder acknowledgment");
        break;
    case ProgrammingStationBusyBroadcast:
        Console.WriteLine("Programming station busy");
        break;
    case ProgrammingCommandNotAcknowledgedBroadcast:
        Console.WriteLine("Programming command not acknowledged");
        break;
}
```

### Unsupported Notifications

```csharp
if (notification is NotSupportedNotification unsupported)
{
    string source = unsupported.SourceBus;
    byte header = unsupported.Header;
    // Log or ignore
}
```

---

## Common Patterns

### Complete Locomotive Control

```csharp
async Task ControlLoco(ushort address, byte targetSpeed)
{
    var locoAddr = new LocoAddress(address);

    // 1. Request current state
    SendCommand(new GetLocoInfoCommand(locoAddr));
    var info = (LocoInfoNotification)await ReceiveNotification();

    // 2. Set speed and direction
    var speed = new LocoSpeed(targetSpeed, SpeedSteps.S126);
    SendCommand(new LocoDriveCommand(locoAddr, speed, LocoDirection.Forward));

    // 3. Turn on headlight (F0)
    SendCommand(new LocoFunctionCommand(locoAddr, FunctionGroup.Group1,
        f0: true, f1: false, f2: false, f3: false, f4: false));
}
```

### CV Programming with Error Handling

```csharp
async Task<byte?> ReadCVWithRetry(ushort cv, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        SendCommand(new ServiceModeReadDirectCommand(cv));

        // Wait for response
        var notification = await ReceiveNotification();

        switch (notification)
        {
            case ServiceModeDirectCVNotification result:
                return result.Value;
            case ProgrammingStationBusyBroadcast:
                await Task.Delay(1000);
                continue;
            case WriteCVTimeoutResponse:
                Console.WriteLine("No decoder response - check decoder connection");
                return null;
            case WriteCVShortCircuitResponse:
                Console.WriteLine("Short circuit detected!");
                return null;
        }
    }
    return null;
}
```

### Monitor All Activity

```csharp
void ProcessAllNotifications()
{
    while (true)
    {
        var notification = NotificationFactory.Create(ReceiveBytes());

        switch (notification)
        {
            case LocoInfoNotification loco:
                HandleLocoInfo(loco);
                break;
            case TrackPowerOnBroadcast:
                HandlePowerOn();
                break;
            case TrackPowerOffBroadcast:
                HandlePowerOff();
                break;
            case EmergencyStopBroadcast:
                HandleEmergencyStop();
                break;
            case FeedbackBroadcast feedback:
                HandleFeedback(feedback);
                break;
            case MUDHErrorNotification error:
                HandleMUDHError(error);
                break;
        }
    }
}
```

---

For more examples and detailed documentation, see [README.md](README.md).
