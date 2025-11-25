# Z21 LAN Protocol - Driving (Locomotive Control)

**Document Version:** 1.13 (English)
**Date:** November 6, 2023

This section describes commands for controlling locomotive decoders, including speed, direction, and functions (F0-F68).

## Overview

### Locomotive Subscription

Clients can **subscribe** to locomotive information using `LAN_X_GET_LOCO_INFO` to receive automatic updates when the locomotive is changed by other clients or handsets.

**Requirements for automatic updates:**
1. Subscribe to the locomotive address via `LAN_X_GET_LOCO_INFO`
2. Enable broadcast flag `0x00000001` via `LAN_SET_BROADCASTFLAGS`

### Subscription Limits

- **Maximum 16 locomotive addresses** per client can be subscribed (FIFO queue)
- Consider network load when polling locomotives
- UDP packets may be dropped by routers during overload

### Locomotive Address Encoding

For addresses **>= 128**, the two highest bits in the MSB byte must be set to 1:

```csharp
// Formula for address encoding
ushort locoAddress = (ushort)(((adr_MSB & 0x3F) << 8) + adr_LSB);

// For addresses >= 128, set high bits
byte adr_MSB_encoded = (byte)(0xC0 | (locoAddress >> 8));
byte adr_LSB = (byte)(locoAddress & 0xFF);

// For addresses < 128, the two highest bits have no meaning
```

**Helper Methods:**

```csharp
public static class LocoAddressHelper
{
    public static (byte MSB, byte LSB) EncodeAddress(ushort address)
    {
        byte msb = (byte)((address >> 8) & 0xFF);
        byte lsb = (byte)(address & 0xFF);

        // For addresses >= 128, set the two highest bits
        if (address >= 128)
        {
            msb |= 0xC0;
        }

        return (msb, lsb);
    }

    public static ushort DecodeAddress(byte msb, byte lsb)
    {
        // Mask out the two highest bits
        return (ushort)(((msb & 0x3F) << 8) + lsb);
    }
}

// Example usage
var (msb, lsb) = LocoAddressHelper.EncodeAddress(3);     // Short address
var (msb2, lsb2) = LocoAddressHelper.EncodeAddress(1234); // Long address (>= 128)
```

---

## 4.1 LAN_X_GET_LOCO_INFO

Polls the status of a locomotive and subscribes the client to receive automatic updates for this address.

**Request to Z21:**

| DataLen | Header | Data |||||||
|---------|--------|------|-----|---------|---------|----------|
| 0x09 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **DB1** | **DB2** | **XOR-Byte** |
|           |           | 0xE3 | 0xF0 | Adr_MSB | Adr_LSB | XOR-Byte |

**Reply:** See [4.4 LAN_X_LOCO_INFO](#44-lan_x_loco_info)

```csharp
public async Task<LocoInfo> GetLocoInfoAsync(ushort address)
{
    var (msb, lsb) = LocoAddressHelper.EncodeAddress(address);

    byte xor = (byte)(0xE3 ^ 0xF0 ^ msb ^ lsb);

    byte[] request =
    [
        0x09, 0x00, 0x40, 0x00,
        0xE3, 0xF0, msb, lsb, xor
    ];

    byte[] response = await SendAndReceiveAsync(request);
    return LocoInfo.Parse(response);
}
```

---

## 4.2 LAN_X_SET_LOCO_DRIVE

Changes the speed and direction of a locomotive.

**Request to Z21:**

| DataLen | Header | Data |||||||
|---------|--------|------|-----|---------|---------|---------|----------|----------|
| 0x0A 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **DB1** | **DB2** | **DB3** | **XOR-Byte** |
|           |           | 0xE4 | 0x1S | Adr_MSB | Adr_LSB | RVVVVVVV | XOR-Byte |

**DB0 (0x1S):** Speed steps configuration

| Value | Description |
|-------|-------------|
| 0x10 | DCC 14 speed steps, or MMI with 14 steps and F0 |
| 0x12 | DCC 28 speed steps, or MMII with 14 real steps and F0-F4 |
| 0x13 | DCC 128 speed steps (126 real steps), or MMII with 28 real steps and F0-F4 |

**DB3 (RVVVVVVV):** Direction and speed

- **R** (bit 7): Direction (1 = forward, 0 = reverse)
- **VVVVVVV** (bits 0-6): Speed value (encoding depends on speed steps)

### Speed Encoding Tables

#### DCC 14 Speed Steps

| RVVVVVVV | Speed | RVVVVVVV | Speed | RVVVVVVV | Speed | RVVVVVVV | Speed |
|----------|-------|----------|-------|----------|-------|----------|-------|
| R000 0000 | Stop | R000 0100 | Step 3 | R000 1000 | Step 7 | R000 1100 | Step 11 |
| R000 0001 | **E-Stop** | R000 0101 | Step 4 | R000 1001 | Step 8 | R000 1101 | Step 12 |
| R000 0010 | Step 1 | R000 0110 | Step 5 | R000 1010 | Step 9 | R000 1110 | Step 13 |
| R000 0011 | Step 2 | R000 0111 | Step 6 | R000 1011 | Step 10 | R000 1111 | Step 14 (max) |

#### DCC 28 Speed Steps

Uses 5 bits: V5 in bit 4, VVVV in bits 0-3.

| R00V5 VVVV | Speed | R00V5 VVVV | Speed | R00V5 VVVV | Speed |
|------------|-------|------------|-------|------------|-------|
| R000 0000 | Stop | R000 0100 | Step 5 | R000 1000 | Step 13 |
| R001 0000 | Stop¹ | R001 0100 | Step 6 | R001 1000 | Step 14 |
| R000 0001 | **E-Stop** | R000 0101 | Step 7 | R000 1001 | Step 15 |
| R001 0001 | E-Stop¹ | R001 0101 | Step 8 | R001 1001 | Step 16 |
| R000 0010 | Step 1 | R000 0110 | Step 9 | R000 1010 | Step 17 |
| R001 0010 | Step 2 | R001 0110 | Step 10 | R001 1010 | Step 18 |
| R000 0011 | Step 3 | R000 0111 | Step 11 | R000 1011 | Step 19 |
| R001 0011 | Step 4 | R001 0111 | Step 12 | R001 1011 | Step 20 |

(continues to Step 28 max at R001 1111)

¹ Usage not recommended

#### DCC 128 Speed Steps

| RVVVVVVV | Speed |
|----------|-------|
| R000 0000 | Stop |
| R000 0001 | **E-Stop** |
| R000 0010 | Step 1 |
| R000 0011 | Step 2 |
| R000 0100 | Step 3 |
| ... | ... |
| R111 1110 | Step 125 |
| R111 1111 | Step 126 (max) |

**Note:** "Stop" = normal stop, "E-Stop" = immediate emergency stop

### C# Implementation

```csharp
public enum SpeedSteps : byte
{
    Steps14 = 0x10,
    Steps28 = 0x12,
    Steps128 = 0x13
}

public enum Direction : byte
{
    Reverse = 0,
    Forward = 1
}

public static class SpeedEncoder
{
    // Encode speed for DCC 14 steps
    public static byte EncodeSpeed14(int speed, Direction direction, bool emergencyStop = false)
    {
        if (speed < 0 || speed > 14)
            throw new ArgumentOutOfRangeException(nameof(speed), "Speed must be 0-14");

        byte dirBit = (byte)((direction == Direction.Forward) ? 0x80 : 0x00);

        if (emergencyStop)
            return (byte)(dirBit | 0x01);

        if (speed == 0)
            return dirBit;

        // Speed 1-14: add 1 to get encoding
        return (byte)(dirBit | (speed + 1));
    }

    // Encode speed for DCC 28 steps
    public static byte EncodeSpeed28(int speed, Direction direction, bool emergencyStop = false)
    {
        if (speed < 0 || speed > 28)
            throw new ArgumentOutOfRangeException(nameof(speed), "Speed must be 0-28");

        byte dirBit = (byte)((direction == Direction.Forward) ? 0x80 : 0x00);

        if (emergencyStop)
            return (byte)(dirBit | 0x01);

        if (speed == 0)
            return dirBit;

        // DCC 28: interleave V5 bit
        int v5 = (speed & 0x01) << 4;  // Bit 0 becomes bit 4
        int vvvv = (speed >> 1) & 0x0F;
        return (byte)(dirBit | v5 | vvvv);
    }

    // Encode speed for DCC 128 steps
    public static byte EncodeSpeed128(int speed, Direction direction, bool emergencyStop = false)
    {
        if (speed < 0 || speed > 126)
            throw new ArgumentOutOfRangeException(nameof(speed), "Speed must be 0-126");

        byte dirBit = (byte)((direction == Direction.Forward) ? 0x80 : 0x00);

        if (emergencyStop)
            return (byte)(dirBit | 0x01);

        if (speed == 0)
            return dirBit;

        // Speed 1-126: add 1 to get encoding
        return (byte)(dirBit | (speed + 1));
    }
}

public async Task SetLocoDriveAsync(ushort address, int speed, Direction direction,
    SpeedSteps speedSteps, bool emergencyStop = false)
{
    var (msb, lsb) = LocoAddressHelper.EncodeAddress(address);

    byte speedByte = speedSteps switch
    {
        SpeedSteps.Steps14 => SpeedEncoder.EncodeSpeed14(speed, direction, emergencyStop),
        SpeedSteps.Steps28 => SpeedEncoder.EncodeSpeed28(speed, direction, emergencyStop),
        SpeedSteps.Steps128 => SpeedEncoder.EncodeSpeed128(speed, direction, emergencyStop),
        _ => throw new ArgumentException("Invalid speed steps", nameof(speedSteps))
    };

    byte xor = (byte)(0xE4 ^ (byte)speedSteps ^ msb ^ lsb ^ speedByte);

    byte[] request =
    [
        0x0A, 0x00, 0x40, 0x00,
        0xE4, (byte)speedSteps, msb, lsb, speedByte, xor
    ];

    await SendAsync(request);
}

// Example usage
await SetLocoDriveAsync(3, 50, Direction.Forward, SpeedSteps.Steps128);
await SetLocoDriveAsync(1234, 0, Direction.Reverse, SpeedSteps.Steps28, emergencyStop: true);
```

**Important Notes:**

- Speed steps (14/28/128) are **automatically stored persistently** for each locomotive address
- For MM format locomotives, the Z21 automatically converts DCC speed steps to MM speed steps
- No standard reply; subscribed clients receive `LAN_X_LOCO_INFO` broadcasts

---

## 4.3 Functions for Locomotive Decoder

### Function Refresh Behavior

- **F0-F12**: Sent **periodically** (priority-dependent) on the main track, like speed and direction
- **F13-F68**: Sent **three times** after a change, then not sent again until the next state change (per RCN-212)

### 4.3.1 LAN_X_SET_LOCO_FUNCTION

Changes a single function of a locomotive.

**Request to Z21:**

| DataLen | Header | Data |||||||
|---------|--------|------|-----|---------|---------|------------|----------|
| 0x0A 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **DB1** | **DB2** | **DB3** | **XOR-Byte** |
|           |           | 0xE4 | 0xF8 | Adr_MSB | Adr_LSB | TTNNNNNN | XOR-Byte |

**DB3 (TTNNNNNN):**

- **TT** (bits 6-7): Switch type
  - `00` = Off
  - `01` = On
  - `10` = Toggle
  - `11` = Not allowed
- **NNNNNN** (bits 0-5): Function index
  - `0x00` = F0 (light)
  - `0x01` = F1
  - ...
  - `0x1F` = F31 (from FW 1.42)

**Function Support by Format:**

| Format | Supported Functions |
|--------|---------------------|
| **MMI** | F0 only |
| **MMII** | F0-F4 |
| **DCC** | F0-F28 (F0-F31 from FW 1.42) |

```csharp
public enum FunctionSwitch : byte
{
    Off = 0x00,
    On = 0x40,
    Toggle = 0x80
}

public async Task SetLocoFunctionAsync(ushort address, byte functionIndex,
    FunctionSwitch switchType)
{
    if (functionIndex > 31)
        throw new ArgumentOutOfRangeException(nameof(functionIndex), "Function index must be 0-31");

    var (msb, lsb) = LocoAddressHelper.EncodeAddress(address);

    byte db3 = (byte)((byte)switchType | functionIndex);
    byte xor = (byte)(0xE4 ^ 0xF8 ^ msb ^ lsb ^ db3);

    byte[] request =
    [
        0x0A, 0x00, 0x40, 0x00,
        0xE4, 0xF8, msb, lsb, db3, xor
    ];

    await SendAsync(request);
}

// Example usage
await SetLocoFunctionAsync(3, 0, FunctionSwitch.On);     // Turn on F0 (light)
await SetLocoFunctionAsync(3, 1, FunctionSwitch.Toggle); // Toggle F1
await SetLocoFunctionAsync(3, 5, FunctionSwitch.Off);    // Turn off F5
```

---

### 4.3.2 LAN_X_SET_LOCO_FUNCTION_GROUP

Sets multiple functions (up to 8) with a single command. **From FW 1.42**, DCC functions up to F31 (with feedback) and F68 (without feedback) are supported.

**Warning:** This command requires the client to track all function states to avoid accidentally turning off functions that were enabled by other clients or handsets. **Recommended for PC automation software only.**

**Request to Z21:**

| DataLen | Header | Data |||||||
|---------|--------|------|-----|---------|---------|-----------|----------|
| 0x0A 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **DB1** | **DB2** | **DB3** | **XOR-Byte** |
|           |           | 0xE4 | Group | Adr_MSB | Adr_LSB | Functions | XOR-Byte |

**Function Groups:**

| Number | Group (Hex) | Bit 7 | Bit 6 | Bit 5 | Bit 4 | Bit 3 | Bit 2 | Bit 1 | Bit 0 | Remarks |
|--------|-------------|-------|-------|-------|-------|-------|-------|-------|-------|---------|
| 1 | 0x20 | 0 | 0 | 0 | F0 | F4 | F3 | F2 | F1 | (A) |
| 2 | 0x21 | 0 | 0 | 0 | 0 | F8 | F7 | F6 | F5 | |
| 3 | 0x22 | 0 | 0 | 0 | 0 | F12 | F11 | F10 | F9 | |
| 4 | 0x23 | F20 | F19 | F18 | F17 | F16 | F15 | F14 | F13 | (B) |
| 5 | 0x28 | F28 | F27 | F26 | F25 | F24 | F23 | F22 | F21 | (B) |
| 6 | 0x29 | F36 | F35 | F34 | F33 | F32 | F31 | F30 | F29 | (C)(D)(E) |
| 7 | 0x2A | F44 | F43 | F42 | F41 | F40 | F39 | F38 | F37 | (D)(E) |
| 8 | 0x2B | F52 | F51 | F50 | F49 | F48 | F47 | F46 | F45 | (D)(E) |
| 9 | 0x50 | F60 | F59 | F58 | F57 | F56 | F55 | F54 | F53 | (D)(E) |
| 10 | 0x51 | F68 | F67 | F66 | F65 | F64 | F63 | F62 | F61 | (D)(E) |

**Remarks:**

- **(A)** MMI: F0 only; MMII: F0-F4
- **(B)** DCC F13-F28 from FW 1.24+
- **(C)** DCC F29-F31 from FW 1.42+ (with feedback to LAN clients)
- **(D)** DCC F32-F68 from FW 1.42+ (**no feedback** to LAN clients, only sent on track)
- **(E)** Not all decoders support F29+. Tested successfully with "Loksound 5" decoder.

```csharp
public enum FunctionGroup : byte
{
    Group1_F0_F4 = 0x20,
    Group2_F5_F8 = 0x21,
    Group3_F9_F12 = 0x22,
    Group4_F13_F20 = 0x23,
    Group5_F21_F28 = 0x28,
    Group6_F29_F36 = 0x29,  // FW 1.42+
    Group7_F37_F44 = 0x2A,  // FW 1.42+
    Group8_F45_F52 = 0x2B,  // FW 1.42+
    Group9_F53_F60 = 0x50,  // FW 1.42+
    Group10_F61_F68 = 0x51  // FW 1.42+
}

public async Task SetLocoFunctionGroupAsync(ushort address, FunctionGroup group,
    byte functionBits)
{
    var (msb, lsb) = LocoAddressHelper.EncodeAddress(address);

    byte xor = (byte)(0xE4 ^ (byte)group ^ msb ^ lsb ^ functionBits);

    byte[] request =
    [
        0x0A, 0x00, 0x40, 0x00,
        0xE4, (byte)group, msb, lsb, functionBits, xor
    ];

    await SendAsync(request);
}

// Example: Set F0=on, F1=off, F2=on, F3=off, F4=on
// Bits: F0=1, F4=1, F3=0, F2=1, F1=0 = 0b00011010 = 0x1A
await SetLocoFunctionGroupAsync(3, FunctionGroup.Group1_F0_F4, 0x1A);

// Example: Set all F5-F8 off
await SetLocoFunctionGroupAsync(3, FunctionGroup.Group2_F5_F8, 0x00);
```

**Reply:** For F0-F31, subscribed clients receive `LAN_X_LOCO_INFO` broadcast. For F32-F68, no feedback is provided.

---

### 4.3.3 LAN_X_SET_LOCO_BINARY_STATE

**From FW 1.42**, sends a DCC "Binary State" command to a locomotive decoder.

**Request to Z21:**

| DataLen | Header | Data ||||||||
|---------|--------|------|-----|-----|-----|------------|------------|----------|
| 0x0A 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **DB1** | **DB2** | **DB3** | **DB4** | **XOR-Byte** |
| | | 0xE5 | 0x5F | AH | AL | FLLLLLLL | HHHHHHHH | XOR-Byte |

**Field Definitions:**

- **AH, AL**: Locomotive address (standard encoding)
- **F** (bit 7 of DB3): State on/off (1 = on, 0 = off)
- **LLLLLLL** (bits 0-6 of DB3): Low 7 bits of binary state address
- **HHHHHHHH** (DB4): High 8 bits of binary state address

**Binary State Address:** 15-bit value = `(HHHHHHHH << 7) + (LLLLLLL & 0x7F)`

**Valid Range:** 29 to 32767

- Addresses 1-28: Reserved for special applications
- Address 0: Reserved as broadcast
- Addresses < 128: Sent as DCC "short form" (per RCN-212)
- Addresses >= 128: Sent as DCC "long form"

**Transmission:** Sent **three times** on main track, then not repeated (per RCN-212)

**Reply:** None (no response to caller, no notification to other clients)

```csharp
public async Task SetLocoBinaryStateAsync(ushort address, ushort binaryStateAddress, bool state)
{
    if (binaryStateAddress < 29 || binaryStateAddress > 32767)
        throw new ArgumentOutOfRangeException(nameof(binaryStateAddress),
            "Binary state address must be 29-32767");

    var (msb, lsb) = LocoAddressHelper.EncodeAddress(address);

    // Split binary state address into high and low parts
    byte stateLow = (byte)(binaryStateAddress & 0x7F);
    byte stateHigh = (byte)((binaryStateAddress >> 7) & 0xFF);

    // Set F bit for on/off
    if (state)
        stateLow |= 0x80;

    byte xor = (byte)(0xE5 ^ 0x5F ^ msb ^ lsb ^ stateLow ^ stateHigh);

    byte[] request =
    [
        0x0A, 0x00, 0x40, 0x00,
        0xE5, 0x5F, msb, lsb, stateLow, stateHigh, xor
    ];

    await SendAsync(request);
}

// Example: Set binary state 1000 to on for loco 3
await SetLocoBinaryStateAsync(3, 1000, true);
```

---

## 4.4 LAN_X_LOCO_INFO

Reports locomotive information (speed, direction, functions) to the client.

**Sent when:**
- Response to `LAN_X_GET_LOCO_INFO` request
- Broadcast to subscribed clients when locomotive state changes (requires broadcast flag `0x00000001`)

**Z21 to Client:**

| DataLen | Header | Data ||||
|---------|--------|------|----------|----------|----------|
| 7 + *n* 0x00 | 0x40 0x00 | **X-Header** | **DB0...DBn** | **XOR-Byte** |
| | | 0xEF | Locomotive Information | XOR-Byte |

**Packet Length:** 7 ≤ *n* ≤ 14 (from FW 1.42: *n* ≥ 8 for F29-F31 status)

### Locomotive Information Structure

| Position | Data | Meaning |
|----------|------|---------|
| **DB0** | Adr_MSB | High byte of address (ignore two highest bits) |
| **DB1** | Adr_LSB | Low byte of address |
| **DB2** | 0000BKKK | **M**=1: MM output format (FW 1.43+)<br>**B**=1: Controlled by another X-BUS handset ("busy")<br>**KKK**: Speed steps (0=14, 2=28, 4=128) |
| **DB3** | RVVVVVVV | **R**: Direction (1=forward)<br>**V**: Speed (encoding depends on KKK) |
| **DB4** | 0DSLFGHJ | **D**: Double traction (1=in double traction)<br>**S**: Smartsearch<br>**L**: F0 (Light)<br>**F**: F4<br>**G**: F3<br>**H**: F2<br>**J**: F1 |
| **DB5** | F5-F12 | Function F5 is bit 0 (LSB) |
| **DB6** | F13-F20 | Function F13 is bit 0 (LSB) |
| **DB7** | F21-F28 | Function F21 is bit 0 (LSB) |
| **DB8** | F29-F31 | Function F29 is bit 0 (LSB) (FW 1.42+) |
| **DBn** | | Optional, for future extensions |

**Address Decoding:** `address = (Adr_MSB & 0x3F) << 8 + Adr_LSB`

### C# Data Structure

```csharp
public class LocoInfo
{
    public ushort Address { get; set; }
    public bool IsMMFormat { get; set; }           // FW 1.43+
    public bool IsBusy { get; set; }               // Controlled by another handset
    public SpeedSteps SpeedSteps { get; set; }
    public Direction Direction { get; set; }
    public int Speed { get; set; }
    public bool IsInDoubleTraction { get; set; }
    public bool SmartSearch { get; set; }
    public bool[] Functions { get; set; } = new bool[32]; // F0-F31

    public static LocoInfo Parse(byte[] data)
    {
        if (data.Length < 11) // Minimum: DataLen(2) + Header(2) + XHeader(1) + Data(5) + XOR(1)
            throw new ArgumentException("Invalid loco info packet");

        var info = new LocoInfo();

        int offset = 5; // Skip DataLen(2), Header(2), X-Header(1)

        // DB0-DB1: Address
        byte adr_msb = data[offset];
        byte adr_lsb = data[offset + 1];
        info.Address = LocoAddressHelper.DecodeAddress(adr_msb, adr_lsb);

        // DB2: Format, Busy, Speed steps
        byte db2 = data[offset + 2];
        info.IsMMFormat = (db2 & 0x08) != 0;     // Bit 3 (FW 1.43+)
        info.IsBusy = (db2 & 0x08) != 0;         // Bit 3
        int speedStepsValue = db2 & 0x07;
        info.SpeedSteps = speedStepsValue switch
        {
            0 => SpeedSteps.Steps14,
            2 => SpeedSteps.Steps28,
            4 => SpeedSteps.Steps128,
            _ => SpeedSteps.Steps128
        };

        // DB3: Direction and speed
        byte db3 = data[offset + 3];
        info.Direction = (db3 & 0x80) != 0 ? Direction.Forward : Direction.Reverse;
        info.Speed = DecodeSpeed(db3, info.SpeedSteps);

        // DB4: F0-F4, double traction, smartsearch
        byte db4 = data[offset + 4];
        info.IsInDoubleTraction = (db4 & 0x40) != 0;
        info.SmartSearch = (db4 & 0x20) != 0;
        info.Functions[0] = (db4 & 0x10) != 0;  // F0 (Light)
        info.Functions[4] = (db4 & 0x08) != 0;  // F4
        info.Functions[3] = (db4 & 0x04) != 0;  // F3
        info.Functions[2] = (db4 & 0x02) != 0;  // F2
        info.Functions[1] = (db4 & 0x01) != 0;  // F1

        // DB5: F5-F12
        if (data.Length > offset + 5)
        {
            byte db5 = data[offset + 5];
            for (int i = 0; i < 8; i++)
            {
                info.Functions[5 + i] = (db5 & (1 << i)) != 0;
            }
        }

        // DB6: F13-F20
        if (data.Length > offset + 6)
        {
            byte db6 = data[offset + 6];
            for (int i = 0; i < 8; i++)
            {
                info.Functions[13 + i] = (db6 & (1 << i)) != 0;
            }
        }

        // DB7: F21-F28
        if (data.Length > offset + 7)
        {
            byte db7 = data[offset + 7];
            for (int i = 0; i < 8; i++)
            {
                info.Functions[21 + i] = (db7 & (1 << i)) != 0;
            }
        }

        // DB8: F29-F31 (FW 1.42+)
        if (data.Length > offset + 8)
        {
            byte db8 = data[offset + 8];
            info.Functions[29] = (db8 & 0x01) != 0;
            info.Functions[30] = (db8 & 0x02) != 0;
            info.Functions[31] = (db8 & 0x04) != 0;
        }

        return info;
    }

    private static int DecodeSpeed(byte speedByte, SpeedSteps speedSteps)
    {
        byte speed = (byte)(speedByte & 0x7F); // Remove direction bit

        if (speed == 0) return 0;      // Stop
        if (speed == 1) return -1;     // E-Stop (special value)

        return speedSteps switch
        {
            SpeedSteps.Steps14 => speed - 1,  // Steps 1-14
            SpeedSteps.Steps28 => DecodeSpeed28(speed),
            SpeedSteps.Steps128 => speed - 1, // Steps 1-126
            _ => speed - 1
        };
    }

    private static int DecodeSpeed28(byte speed)
    {
        // Extract V5 (bit 4) and VVVV (bits 0-3)
        int v5 = (speed >> 4) & 0x01;
        int vvvv = speed & 0x0F;
        return (vvvv << 1) | v5;  // Reconstruct 28-step value
    }

    public override string ToString()
    {
        var functionsOn = string.Join(", ",
            Enumerable.Range(0, 32)
                .Where(i => Functions[i])
                .Select(i => $"F{i}"));

        return $"Loco {Address}: {Direction} Speed {Speed} ({SpeedSteps}) " +
               $"Functions: {functionsOn} {(IsBusy ? "[BUSY]" : "")}";
    }
}
```

---

## 4.5 LAN_X_SET_LOCO_E_STOP

**From FW 1.43**, sends emergency stop to a specific locomotive.

- **DCC**: Sends "E-STOP" speed step (decoder should stop as quickly as possible, per RCN-212)
- **MM**: Sends speed step 0 ("Stop")

**Request to Z21:**

| DataLen | Header | Data ||||||
|---------|--------|------|-----|---------|----------|----------|
| 0x08 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **DB2** | **XOR-Byte** |
| | | 0x92 | Adr_MSB | Adr_LSB | XOR-Byte |

**Reply:** Subscribed clients receive `LAN_X_LOCO_INFO` broadcast.

```csharp
public async Task SetLocoEmergencyStopAsync(ushort address)
{
    var (msb, lsb) = LocoAddressHelper.EncodeAddress(address);

    byte xor = (byte)(0x92 ^ msb ^ lsb);

    byte[] request =
    [
        0x08, 0x00, 0x40, 0x00,
        0x92, msb, lsb, xor
    ];

    await SendAsync(request);
}

// Example: Emergency stop for loco 3
await SetLocoEmergencyStopAsync(3);
```

---

## 4.6 LAN_X_PURGE_LOCO

**From FW 1.43**, removes a locomotive from the Z21, stopping transmission of its commands on the track. Transmission resumes when a new drive or function command is sent to the same address.

**Use case:** PC automation software can manage the number of active locomotives to optimize track data throughput.

**Request to Z21:**

| DataLen | Header | Data |||||||
|---------|--------|------|-----|---------|---------|----------|
| 0x09 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **DB1** | **DB2** | **XOR-Byte** |
| | | 0xE3 | 0x44 | Adr_MSB | Adr_LSB | XOR-Byte |

**Reply:** None (no response to caller, no notification to other clients)

```csharp
public async Task PurgeLocoAsync(ushort address)
{
    var (msb, lsb) = LocoAddressHelper.EncodeAddress(address);

    byte xor = (byte)(0xE3 ^ 0x44 ^ msb ^ lsb);

    byte[] request =
    [
        0x09, 0x00, 0x40, 0x00,
        0xE3, 0x44, msb, lsb, xor
    ];

    await SendAsync(request);
}

// Example: Remove loco 42 from active refresh
await PurgeLocoAsync(42);
```

---

## Summary

### Key Concepts

1. **Subscription**: Up to 16 locomotives per client (FIFO)
2. **Address Encoding**: Addresses >= 128 need high bits set in MSB
3. **Speed Steps**: 14, 28, or 128 steps with different encoding schemes
4. **Functions**: F0-F12 refreshed periodically; F13+ sent 3 times then stopped
5. **Emergency Stop**: Immediate stop via E-Stop speed step
6. **Purge**: Remove loco from refresh to reduce track bandwidth

### Command Overview

| Command | Purpose | Reply |
|---------|---------|-------|
| `LAN_X_GET_LOCO_INFO` | Poll status & subscribe | `LAN_X_LOCO_INFO` |
| `LAN_X_SET_LOCO_DRIVE` | Set speed & direction | Broadcast to subscribers |
| `LAN_X_SET_LOCO_FUNCTION` | Set single function | Broadcast to subscribers |
| `LAN_X_SET_LOCO_FUNCTION_GROUP` | Set function group | Broadcast to subscribers |
| `LAN_X_SET_LOCO_BINARY_STATE` | DCC binary state (FW 1.42+) | None |
| `LAN_X_SET_LOCO_E_STOP` | Emergency stop (FW 1.43+) | Broadcast to subscribers |
| `LAN_X_PURGE_LOCO` | Remove from refresh (FW 1.43+) | None |

### Function Support Matrix

| Format | Functions | Periodic Refresh |
|--------|-----------|------------------|
| **MMI** | F0 | F0 |
| **MMII** | F0-F4 | F0-F4 |
| **DCC** | F0-F28 (F0-F31 from FW 1.42) | F0-F12 |

**Note:** F13+ are sent 3 times after change, then not repeated until next change (per RCN-212).
