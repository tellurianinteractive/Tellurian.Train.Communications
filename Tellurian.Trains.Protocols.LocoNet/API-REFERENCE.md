# LocoNet API Quick Reference

## Table of Contents
1. [Message Factory](#message-factory)
2. [Power Control](#power-control)
3. [Locomotive Control](#locomotive-control)
4. [Switch/Turnout Control](#switchturnout-control)
5. [Sensor Monitoring](#sensor-monitoring)
6. [Programming](#programming)
7. [Consisting](#consisting)
8. [Data Types](#data-types)
9. [Helper Utilities](#helper-utilities)

---

## Message Factory

### Parse Incoming Messages

```csharp
using Tellurian.Trains.Protocols.LocoNet;

byte[] receivedBytes = ...; // from LocoNet
Message msg = LocoNetMessageFactory.Create(receivedBytes);

// Type checking
if (msg is SlotNotification slot) { /* ... */ }
if (msg is SensorInputNotification sensor) { /* ... */ }
if (msg is LongAcknowledge ack) { /* ... */ }
```

### Generate Outgoing Commands

```csharp
Command cmd = new SomeCommand(...);
byte[] bytes = cmd.GetBytesWithChecksum();
// Send bytes to LocoNet
```

---

## Power Control

### Power ON/OFF

```csharp
using Tellurian.Trains.Protocols.LocoNet.Commands;

// Turn power ON
var powerOn = new PowerOnCommand();
byte[] onBytes = powerOn.GetBytesWithChecksum();
// Sends: [0x83, 0x7C]

// Turn power OFF
var powerOff = new PowerOffCommand();
byte[] offBytes = powerOff.GetBytesWithChecksum();
// Sends: [0x82, 0x7D]
```

### Emergency Stop

```csharp
// Stop all locomotives immediately
var emergencyStop = new ForceIdleCommand();
byte[] stopBytes = emergencyStop.GetBytesWithChecksum();
// Sends: [0x85, 0x7A]
```

---

## Locomotive Control

### Request Locomotive Address

```csharp
// Short address (1-127)
var locoAddr = new LocoAddress(3);
var request = new GetLocoAddressCommand(locoAddr);
byte[] requestBytes = request.GetBytesWithChecksum();
// Sends: [0xBF, 0x00, 0x03, checksum]

// Long address (128-9999)
var longAddr = new LocoAddress(1234);
var requestLong = new GetLocoAddressCommand(longAddr);
// Automatically encodes 14-bit address
```

**Response:** `SlotNotification` (OPC_SL_RD_DATA)

```csharp
var response = LocoNetMessageFactory.Create(receivedBytes);
if (response is SlotNotification slot)
{
    byte slotNumber = slot.SlotNumber;
    ushort address = slot.Address;
    SlotStatus status = slot.Status;
    byte speed = slot.Speed;
    bool direction = slot.Direction;
}
```

### Request Slot Data

```csharp
// Request data for specific slot number
var requestSlot = new RequestSlotDataCommand(5);
byte[] bytes = requestSlot.GetBytesWithChecksum();
// Sends: [0xBB, 0x05, 0x00, checksum]
```

**Response:** `SlotNotification`

### Activate Slot

```csharp
// NULL move - marks slot as IN_USE
var activate = MoveSlotCommand.Activate(5);
byte[] activateBytes = activate.GetBytesWithChecksum();
// Sends: [0xBA, 0x05, 0x05, checksum]
```

### Dispatch Slot

```csharp
// Release slot to dispatch stack
var dispatch = MoveSlotCommand.DispatchPut(5);
// Sends: [0xBA, 0x05, 0x00, checksum]

// Get dispatched locomotive
var getDispatched = MoveSlotCommand.DispatchGet();
// Sends: [0xBA, 0x00, 0x00, checksum]
```

### Set Speed

```csharp
// Speed values: 0=stop, 1=emergency stop, 2-127=speed steps
var setSpeed = new SetLocoSpeedCommand(slotNumber: 5, speedSteps: 64);
byte[] speedBytes = setSpeed.GetBytesWithChecksum();
// Sends: [0xA0, 0x05, 0x40, checksum]

// Stop with momentum
var stop = SetLocoSpeedCommand.Zero(slotNumber: 5);

// Emergency stop
var eStop = SetLocoSpeedCommand.Stop(slotNumber: 5);
```

### Set Direction and Functions F0-F4

```csharp
var setDirF = new SetLocoDirectionAndFunctionF0toF4Command(
    slot: 5,
    forward: true,
    F0: true,   // Headlight
    F1: false,
    F2: true,
    F3: false,
    F4: false
);
byte[] dirBytes = setDirF.GetBytesWithChecksum();
// Sends: [0xA1, 0x05, 0x34, checksum]
```

### Set Functions F5-F8

```csharp
var setSnd = new SetLocoFunctionF5toF8Command(
    slot: 5,
    F5: true,
    F6: false,
    F7: true,
    F8: false
);
byte[] sndBytes = setSnd.GetBytesWithChecksum();
// Sends: [0xA2, 0x05, 0x05, checksum]
```

### Write Slot Data (Advanced)

```csharp
// Modify multiple slot parameters at once
var modifiedSlot = WriteSlotDataCommand.ModifySlot(
    existingData: currentSlotData,
    speed: 80,
    direction: true,
    status: SlotStatus.InUse
);
byte[] writeBytes = modifiedSlot.GetBytesWithChecksum();
// Sends: 14-byte OPC_WR_SL_DATA message
```

---

## Accessory Control

### Without Acknowledge

```csharp
using Tellurian.Trains.Protocols.LocoNet.Commands;

// Throw accessory 100 (Thrown/Red position)
var throwCmd = SetAccessoryCommand.Throw(AccessoryAddress.From(100), activate: true);
byte[] throwBytes = throwCmd.GetBytesWithChecksum();
// Sends: [0xB0, sw1, sw2, checksum]

// Close accessory 100 (Closed/Green position)
var closeCmd = SetAccessoryCommand.Close(AccessoryAddress.From(100), activate: true);

// Turn off output
var turnOff = SetAccessoryCommand.TurnOff(AccessoryAddress.From(100));
```

### With Acknowledge

```csharp
// Request acknowledgment
var switchAck = AccessoryAcknowledgeCommand.Throw(AccessoryAddress.From(100));
byte[] ackBytes = switchAck.GetBytesWithChecksum();
// Sends: [0xBD, sw1, sw2, checksum]
```

**Response:** `LongAcknowledge`

```csharp
if (response is LongAcknowledge ack)
{
    if (ack.IsSuccess)
        Console.WriteLine("Switch command accepted");
    else
        Console.WriteLine($"Failed: {ack.Message}");
}
```

### Request Switch State

```csharp
var requestState = new RequestAccessoryStateCommand(AccessoryAddress.From(100));
byte[] stateBytes = requestState.GetBytesWithChecksum();
// Sends: [0xBC, sw1, sw2, checksum]
```

**Response:** `AccessoryReportNotification` (OPC_SW_REP)

### Parse Switch Reports

```csharp
using Tellurian.Trains.Protocols.LocoNet.Notifications;

if (msg is AccessoryReportNotification switchReport)
{
    ushort address = switchReport.Address.Value;

    if (switchReport.IsOutputStatus)
    {
        bool closedOn = switchReport.ClosedOutputOn;
        bool thrownOn = switchReport.ThrownOutputOn;
        AccessoryFunction? direction = switchReport.CurrentDirection;
        Console.WriteLine($"Switch {address}: {direction}");
    }
    else if (switchReport.IsInputFeedback)
    {
        bool isHigh = switchReport.IsInputHigh;
        bool isSwitchInput = switchReport.IsSwitchInput;
        Console.WriteLine($"Switch {address} sensor: {(isHigh ? "HIGH" : "LOW")}");
    }
}
```

---

## Sensor Monitoring

### Parse Sensor Input Reports

```csharp
using Tellurian.Trains.Protocols.LocoNet.Notifications;

if (msg is SensorInputNotification sensor)
{
    ushort address = sensor.Address;
    bool isHigh = sensor.IsHigh;
    bool isLow = sensor.IsLow;
    bool isOccupied = sensor.IsOccupied;  // Alias for isHigh
    bool isClear = sensor.IsClear;        // Alias for isLow
    bool isSwitchInput = sensor.IsSwitchInput;

    Console.WriteLine($"Sensor {address}: {(isOccupied ? "OCCUPIED" : "CLEAR")}");
}
```

### Parse Transponding Reports

```csharp
using Tellurian.Trains.Protocols.LocoNet.Notifications;

if (msg is MultiSenseNotification multiSense)
{
    if (multiSense.IsTransponding)
    {
        ushort section = multiSense.Section;
        char zone = multiSense.Zone;           // A-H
        ushort locoAddress = multiSense.LocoAddress;
        bool isPresent = multiSense.IsPresent; // true=entering, false=leaving

        Console.WriteLine($"Section {section} Zone {zone}: Loco {locoAddress} {(isPresent ? "PRESENT" : "ABSENT")}");
    }
    else if (multiSense.IsPowerMessage)
    {
        Console.WriteLine($"Section {multiSense.Section}: Power management event");
    }
}
```

### Parse LISSY/RailCom Reports

```csharp
using Tellurian.Trains.Protocols.LocoNet.Notifications;

if (msg is LissyNotification lissy && lissy.IsValid)
{
    byte sectionAddress = lissy.SectionAddress;
    ushort locoAddress = lissy.LocoAddress;
    bool isForward = lissy.IsForward;
    byte category = lissy.Category;

    Console.WriteLine($"LISSY Section {sectionAddress}: Loco {locoAddress} {(isForward ? "Fwd" : "Rev")} Category {category}");
}
```

---

## Programming

### CV Read (Service Mode)

```csharp
using Tellurian.Trains.Protocols.LocoNet.Commands;
using Tellurian.Trains.Protocols.LocoNet.Programming;

// Read CV29 on programming track
var readCV = ProgrammingCommand.ReadCvService(
    cvNumber: 29,
    mode: ProgrammingMode.DirectModeByteService  // Default
);
byte[] readBytes = readCV.GetBytesWithChecksum();
// Sends: 14-byte message to slot 124
```

**Response:** `LongAcknowledge`, then `SlotNotification` (slot 124)

```csharp
// 1. Wait for LACK
if (ack is LongAcknowledge longAck)
{
    if (longAck.ResponseCode == 0x01)
        Console.WriteLine("Programming started...");
    else if (longAck.ResponseCode == 0x00)
        Console.WriteLine("Programmer busy");
}

// 2. Wait for slot 124 response
if (slot is SlotNotification slotMsg && slotMsg.IsProgrammingSlot)
{
    var result = slotMsg.ProgrammingResult;
    if (result?.IsSuccess == true)
    {
        Console.WriteLine($"{result.CV}");  // "CV29=value"
        // Or access individual properties:
        // result.CV.Number, result.CV.Value
    }
    else
    {
        Console.WriteLine($"Failed: {result?.StatusMessage}");
    }
}
```

### CV Write (Service Mode)

```csharp
// Write CV3 = 10
var writeCV = ProgrammingCommand.WriteCvService(
    cvNumber: 3,
    value: 10,
    mode: ProgrammingMode.DirectModeByteService
);
byte[] writeBytes = writeCV.GetBytesWithChecksum();
```

### Operations Mode Programming (POM)

```csharp
// Write CV2 = 5 to locomotive 3 on main track
var pomWrite = ProgrammingCommand.WriteCvOperations(
    locomotiveAddress: 3,
    cvNumber: 2,
    value: 5,
    withFeedback: false  // Blind write
);
byte[] pomBytes = pomWrite.GetBytesWithChecksum();
```

### Programming Modes

```csharp
// Available modes
ProgrammingMode.PagedModeService            // Paged mode
ProgrammingMode.DirectModeByteService       // Direct byte (most common)
ProgrammingMode.DirectModeBitService        // Direct bit
ProgrammingMode.RegisterModeService         // Register mode
ProgrammingMode.OperationsModeByte          // POM byte, no feedback
ProgrammingMode.OperationsModeByteWithFeedback  // POM byte with feedback
```

### Parse Programming Status

```csharp
using Tellurian.Trains.Protocols.LocoNet.Programming;

// From ProgrammingResult
ProgrammingStatus status = result.Status;

// Check flags
bool success = status == ProgrammingStatus.Success;
bool noDecoder = (status & ProgrammingStatus.NoDecoder) != 0;
bool writeAckFail = (status & ProgrammingStatus.WriteAckFail) != 0;
bool readAckFail = (status & ProgrammingStatus.ReadAckFail) != 0;
bool userAborted = (status & ProgrammingStatus.UserAborted) != 0;

// Get message
string message = ProgrammingHelper.GetStatusMessage(status);
```

---

## Consisting

### Link Slots (Create Consist)

```csharp
using Tellurian.Trains.Protocols.LocoNet.Commands;

// Link slot 8 to follow slot 5
var link = new LinkSlotsCommand(slaveSlot: 8, masterSlot: 5);
byte[] linkBytes = link.GetBytesWithChecksum();
// Sends: [0xB9, 0x08, 0x05, checksum]

// Build multi-loco consist
byte leadSlot = 5;
var commands = leadSlot.BuildConsist(8, 12, 15);
foreach (var cmd in commands)
{
    SendCommand(cmd);
    // Wait for confirmation
}
```

**After linking:**
- Slave slot gets `SL_CONUP` flag
- Master slot gets `SL_CONDN` flag

### Unlink Slots

```csharp
// Unlink slot 8 from slot 5
var unlink = new UnlinkSlotsCommand(slaveSlot: 8, masterSlot: 5);
byte[] unlinkBytes = unlink.GetBytesWithChecksum();
// Sends: [0xB8, 0x08, 0x05, checksum]

// Break entire consist
byte leadSlot = 5;
var unlinkCommands = leadSlot.BreakConsist(8, 12, 15);
```

### Control Consist Member

```csharp
// Set direction/functions on specific member (not entire consist)
var memberFunc = new ConsistFunctionCommand(
    slotNumber: 8,
    forward: true,
    f0: true,  // Turn on rear headlight
    f1: false, f2: false, f3: false, f4: false
);
byte[] funcBytes = memberFunc.GetBytesWithChecksum();
// Sends: [0xB6, 0x08, dirf, checksum]

// Helper methods
var dirOnly = ConsistFunctionCommand.DirectionOnly(slot: 8, forward: false);
var headlight = ConsistFunctionCommand.Headlight(slot: 8, forward: true, headlightOn: true);
```

### Check Consist Status

```csharp
// From slot data
if (slotData.IsInConsist())
{
    if (slotData.Consist.IsConsistLead())
        Console.WriteLine("This is the lead locomotive");
    else if (slotData.Consist.IsConsistMember())
        Console.WriteLine("This is a consist member");

    string role = slotData.Consist.GetConsistRoleDescription();
    Console.WriteLine(role);
}
```

---

## Data Types

### SlotData

```csharp
// Parse from 14-byte message
byte[] slotBytes = ...; // OPC_SL_RD_DATA
SlotData slot = SlotData.FromBytes(slotBytes);

// Properties
byte slotNumber = slot.SlotNumber;
ushort address = slot.Address;
byte speed = slot.Speed;
bool direction = slot.Direction;
SlotStatus status = slot.Status;
ConsistStatus consist = slot.Consist;
DecoderType decoderType = slot.DecoderType;
TrackStatus trackStatus = slot.TrackStatus;
bool f0 = slot.F0;
bool f1 = slot.F1;
// ... F2-F8
ushort deviceId = slot.DeviceId;

// Convert back to bytes
byte[] bytes = slot.ToBytes(opcode: 0xEF);
```

### LocoAddress

```csharp
// Create address
var shortAddr = new LocoAddress(3);       // 1-127
var longAddr = new LocoAddress(1234);     // 128-9999

// Properties
ushort address = locoAddr.Address;
byte high = locoAddr.High;  // High 7 bits
byte low = locoAddr.Low;    // Low 7 bits
bool isShort = locoAddr.IsShort;
bool isLong = locoAddr.IsLong;
```

### AccessoryAddress

```csharp
// Create address
var switchAddr = AccessoryAddress.From(100);
var withInput = AccessoryAddress.From(100, AccessoryInput.Port1);

// Encode for commands
var (sw1, sw2) = switchAddr.EncodeAccessoryBytes(
    direction: AccessoryFunction.ClosedOrGreen,
    output: OutputState.On
);

// Decode from message
var decoded = AccessoryAddress.DecodeAccessoryBytes(
    sw1, sw2,
    out AccessoryFunction direction,
    out OutputState output
);
```

### CV (Configuration Variable)

The `CV` struct is defined in `Tellurian.Trains.Communications.Interfaces.Decoder` and can be used across all protocol implementations.

```csharp
using Tellurian.Trains.Communications.Interfaces.Decoder;
using Tellurian.Trains.Protocols.LocoNet.Programming;

// Create CV using constructor
var cv = new CV(29, 6);  // CV29 = 6

// Create CV using extension method
var cv2 = 29.CV(6);  // CV29 = 6

// Properties
int number = cv.Number;   // 1-1024
byte value = cv.Value;    // 0-255

// Display
Console.WriteLine(cv.ToString());  // "CV29=6"

// Equality
var cv1 = new CV(29, 6);
var cv2 = new CV(29, 6);
bool equal = cv1 == cv2;  // true

// Encode for LocoNet protocol transmission (extension method)
var (cvh, cvl, data7) = cv.EncodeToBytes();

// Decode from LocoNet protocol bytes (extension method)
CV decoded = CV.DecodeFromBytes(cvh, cvl, data7);
```

### Enums

```csharp
// Slot Status
SlotStatus.Free      // Empty
SlotStatus.Common    // Being refreshed, not controlled
SlotStatus.Idle      // Not being refreshed
SlotStatus.InUse     // Active control

// Consist Status
ConsistStatus.NotInConsist
ConsistStatus.SubMember      // Uplinked only
ConsistStatus.ConsistTop     // Lead (downlinked)
ConsistStatus.MidConsist     // Up and down linked

// Decoder Type
DecoderType.Steps14
DecoderType.Steps28
DecoderType.Steps128         // Most common
DecoderType.Steps28Trinary
DecoderType.Steps28AdvancedConsist
DecoderType.Steps128AdvancedConsist

// Track Status (flags)
TrackStatus.PowerOn
TrackStatus.NotIdle
TrackStatus.LocoNet11
TrackStatus.ProgrammingBusy
```

---

## Helper Utilities

### Message Helpers

```csharp
// Get message length from opcode
int length = Message.GetMessageLength(opcode);
// Returns: 2, 4, 6, or -1 (variable)

// Check if opcode expects response
bool expects = Message.ExpectsResponse(opcode);
// Checks bit 3 (follow-on bit)

// Calculate checksum
byte checksum = Message.Checksum(dataBytes);

// Append checksum
byte[] withChecksum = Message.AppendChecksum(dataWithoutChecksum);
```

### Programming Helpers

```csharp
using Tellurian.Trains.Communications.Interfaces.Decoder;
using Tellurian.Trains.Protocols.LocoNet.Programming;

// Build PCMD byte (extension method on ProgrammingMode)
byte pcmd = ProgrammingMode.DirectModeByteService.BuildProgrammingCommandByte(
    operation: ProgrammingOperation.Write
);

// Encode CV and data for LocoNet transmission (extension method on CV)
var cv = new CV(29, 6);
var (cvh, cvl, data7) = cv.EncodeToBytes();

// Decode CV and data from LocoNet bytes (extension method on CV)
CV decoded = CV.DecodeFromBytes(cvh, cvl, data7);

// Check status (extension methods on ProgrammingStatus)
bool success = status.IsSuccess();
string message = status.GetMessage();
```

### Consist Helpers

```csharp
// Check consist status
bool inConsist = consistStatus.IsInConsist();
bool isLead = consistStatus.IsConsistLead();
bool isMember = consistStatus.IsConsistMember();
bool canLink = slotData.CanBeLinked();

// Build/break consists
LinkSlotsCommand[] links = lead.BuildConsist(members);
UnlinkSlotsCommand[] unlinks = lead.BreakConsist(members);

// STAT1 manipulation
ConsistStatus status = stat1Byte.GetConsistStatus();
byte newStat1 = stat1Byte.SetConsistStatus(newStatus);
string description = status.GetConsistRoleDescription();
```

---

## Error Handling

### Long Acknowledge Codes

```csharp
if (msg is LongAcknowledge ack)
{
    byte forOpcode = ack.ForOperationCode;  // Which command
    byte responseCode = ack.ResponseCode;    // Result code

    // Common response codes
    // 0x00 - FAIL/Rejected
    // 0x01 - Accepted, will send follow-on
    // 0x7F - Accepted/Success
    // 0x40 - Accepted, blind operation

    if (ack.IsSuccess)
        Console.WriteLine($"Success: {ack.Message}");
    else if (ack.IsFailure)
        Console.WriteLine($"Failed: {ack.Message}");
    else
        Console.WriteLine($"Undecided: {ack.Message}");
}
```

### Unsupported Messages

```csharp
if (msg is UnsupportedNotification unsupported)
{
    byte[] rawData = unsupported.RawData;
    // Log or ignore
}
```

---

## Common Patterns

### Complete Locomotive Control

```csharp
async Task ControlLoco(ushort address, byte targetSpeed)
{
    // 1. Request
    SendCommand(new GetLocoAddressCommand(new LocoAddress(address)));
    var slot = (SlotNotification)await ReceiveMessage();

    // 2. Activate
    if (slot.Status != SlotStatus.InUse)
    {
        SendCommand(MoveSlotCommand.Activate(slot.SlotNumber));
        await ReceiveMessage();
    }

    // 3. Control
    SendCommand(new SetLocoSpeedCommand(slot.SlotNumber, targetSpeed));
    SendCommand(new SetLocoDirectionAndFunctionF0toF4Command(
        slot.SlotNumber, true, true, false, false, false, false));
}
```

### Switch Control with Timing

```csharp
async Task ThrowSwitch(AccessoryAddress address)
{
    // Activate
    SendCommand(SetAccessoryCommand.Throw(address));

    // Wait for motor
    await Task.Delay(1000);

    // Turn off
    SendCommand(SetAccessoryCommand.TurnOff(address));
}
```

### Monitor All Activity

```csharp
void ProcessAllMessages()
{
    while (true)
    {
        var msg = LocoNetMessageFactory.Create(ReceiveBytes());

        switch (msg)
        {
            case SlotNotification slot:
                HandleSlot(slot);
                break;
            case SensorInputNotification sensor:
                HandleSensor(sensor);
                break;
            case AccessoryReportNotification switchReport:
                HandleSwitch(switchReport);
                break;
            case MultiSenseNotification multiSense when multiSense.IsTransponding:
                HandleTransponding(multiSense);
                break;
            case LissyNotification lissy when lissy.IsValid:
                HandleLissy(lissy);
                break;
            case LongAcknowledge ack:
                HandleAck(ack);
                break;
        }
    }
}
```

---

For more examples, see [README.md](README.md).
