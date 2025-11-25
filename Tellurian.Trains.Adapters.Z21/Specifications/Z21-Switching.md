# Z21 LAN Protocol - Switching (Accessory Decoders)

**Document Version:** 1.13 (English)
**Date:** November 6, 2023

This section covers commands for controlling accessory decoders (turnouts, signals, etc.) according to RP-9.2.1.

## Table of Contents

- [Address Visualization Overview](#address-visualization-overview)
- [DCC Address Encoding](#dcc-address-encoding)
- [Basic Turnout Commands](#basic-turnout-commands)
- [Queue Mode](#queue-mode)
- [Extended Accessory Decoders](#extended-accessory-decoders)

---

## Address Visualization Overview

### The Problem

Different DCC systems visualize turnout numbers differently, which can cause confusion. According to DCC (RP-9.2.1):
- Each accessory decoder address has **4 ports**
- Each port has **2 outputs**
- One turnout connects to one port

### Common Visualization Schemes

| System | Numbering Start | DCC Address Start | Example |
|--------|----------------|-------------------|---------|
| **ESU, Uhlenbrock** | Switch #1 | DCC-Addr=1 | Switch #1: Addr=1 Port=0<br>Switch #5: Addr=2 Port=0 |
| **Roco (Z21)** | Switch #1 | DCC-Addr=0 | Switch #1: Addr=0 Port=0<br>Switch #5: Addr=1 Port=0 |
| **Twin Center** | Virtual | Freely configurable | User-defined mapping |
| **Zimo** | N/A | Shows raw DCC address + port | Direct display |

**Important:** The same physical turnout may have different numbers on different systems. For example:
- ESU control panel: Switch #1
- Roco multiMaus/Z21: Switch #5
- **"Shift by 4"** difference between systems

### Terminology

This documentation uses **"Output 1"** and **"Output 2"** instead of "straight" and "branching" because:
- Actual position depends on physical wiring
- Command station cannot know the physical configuration
- Application software determines the meaning

---

## DCC Address Encoding

### Z21 Function Address (FAdr)

The Z21 uses a **Function Address (FAdr)** parameter that combines the DCC address and port:

```csharp
// Z21 Function Address formula
ushort FAdr = (ushort)((FAdr_MSB << 8) + FAdr_LSB);

// Extract DCC components
ushort Dcc_Addr = (ushort)(FAdr >> 2);        // DCC decoder address
byte Port = (byte)(FAdr & 0x03);              // Port (0-3)
```

### DCC Packet Format

DCC basic accessory decoder packet: `{preamble} 0 10AAAAAA 0 1aaaCDDd 0 EEEEEEEE 1`

**Encoding:**

```csharp
public static class DccAccessoryEncoder
{
    public static byte[] EncodeDccAddress(ushort dccAddr, byte port, bool activate, bool output)
    {
        // aaaAAAAAA = (~Dcc_Addr & 0x1C0) | (Dcc_Addr & 0x003F)
        ushort aaaAAAAAA = (ushort)((~dccAddr & 0x1C0) | (dccAddr & 0x003F));

        // C = Activate/Deactivate
        // DD = Port (0-3)
        // d = Output selection (0 or 1)
        byte c = activate ? (byte)1 : (byte)0;
        byte dd = (byte)(port & 0x03);
        byte d = output ? (byte)1 : (byte)0;

        // Build DCC packet bytes (simplified)
        return new[]
        {
            (byte)(0x80 | (aaaAAAAAA & 0x3F)),              // 10AAAAAA
            (byte)(0x80 | (c << 3) | (dd << 1) | d)         // 1aaaCDDd
        };
    }
}
```

### Function Address Examples

| FAdr | DCC Address | Port |
|------|-------------|------|
| 0 | 0 | 0 |
| 1 | 0 | 1 |
| 2 | 0 | 2 |
| 3 | 0 | 3 |
| 4 | 1 | 0 |
| 5 | 1 | 1 |
| ... | ... | ... |

**Formula:**
```csharp
ushort CalculateFAdr(ushort dccAddr, byte port)
{
    return (ushort)((dccAddr << 2) | (port & 0x03));
}

(ushort DccAddr, byte Port) DecodeFAdr(ushort fAdr)
{
    return ((ushort)(fAdr >> 2), (byte)(fAdr & 0x03));
}
```

### MM (Motorola) Format

For MM format, **FAdr starts with 0**:
- FAdr=0 → MM-Addr=1
- FAdr=1 → MM-Addr=2
- etc.

### Subscription

Clients can subscribe to accessory info to receive automatic updates when accessories are changed by other clients or handsets. Enable broadcast flag `0x00000001` via `LAN_SET_BROADCASTFLAGS`.

---

## Basic Turnout Commands

### 5.1 LAN_X_GET_TURNOUT_INFO

Polls the status of a turnout or accessory function.

**Request to Z21:**

| DataLen | Header | Data |||||
|---------|--------|------|-----|---------|---------|----------|
| 0x08 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **DB1** | **XOR-Byte** |
|           |           | 0x43 | FAdr_MSB | FAdr_LSB | XOR-Byte |

**Reply:** See [5.3 LAN_X_TURNOUT_INFO](#53-lan_x_turnout_info)

```csharp
public async Task<TurnoutInfo> GetTurnoutInfoAsync(ushort fAddr)
{
    byte msb = (byte)((fAddr >> 8) & 0xFF);
    byte lsb = (byte)(fAddr & 0xFF);
    byte xor = (byte)(0x43 ^ msb ^ lsb);

    byte[] request =
    [
        0x08, 0x00, 0x40, 0x00,
        0x43, msb, lsb, xor
    ];

    byte[] response = await SendAndReceiveAsync(request);
    return TurnoutInfo.Parse(response);
}
```

---

### 5.2 LAN_X_SET_TURNOUT

Switches a turnout or accessory function.

**Request to Z21:**

| DataLen | Header | Data |||||||
|---------|--------|------|-----|---------|---------|------------|----------|
| 0x09 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **DB1** | **DB2** | **XOR-Byte** |
|           |           | 0x53 | FAdr_MSB | FAdr_LSB | 10Q0A00P | XOR-Byte |

**DB2 (10Q0A00P):**

| Bit | Name | Description |
|-----|------|-------------|
| 7 | Fixed | Always 1 |
| 6 | Fixed | Always 0 |
| 5 | **Q** | Queue mode (FW 1.24+)<br>0 = Execute immediately<br>1 = Add to queue |
| 4 | Fixed | Always 0 |
| 3 | **A** | Activate/Deactivate<br>0 = Deactivate turnout output<br>1 = Activate turnout output |
| 2-1 | Fixed | Always 00 |
| 0 | **P** | Output selection<br>0 = Select output 1<br>1 = Select output 2 |

**Reply:** Subscribed clients receive `LAN_X_TURNOUT_INFO` broadcast.

```csharp
public enum TurnoutOutput : byte
{
    Output1 = 0,
    Output2 = 1
}

public enum QueueMode : byte
{
    Immediate = 0,  // Execute immediately (Q=0)
    Queue = 1       // Add to queue (Q=1, FW 1.24+)
}

public async Task SetTurnoutAsync(ushort fAddr, TurnoutOutput output,
    bool activate, QueueMode queueMode = QueueMode.Queue)
{
    byte msb = (byte)((fAddr >> 8) & 0xFF);
    byte lsb = (byte)(fAddr & 0xFF);

    // Build DB2: 10Q0A00P
    byte db2 = 0x80;  // Bit 7 = 1
    if (queueMode == QueueMode.Queue)
        db2 |= 0x20;  // Set Q bit
    if (activate)
        db2 |= 0x08;  // Set A bit
    if (output == TurnoutOutput.Output2)
        db2 |= 0x01;  // Set P bit

    byte xor = (byte)(0x53 ^ msb ^ lsb ^ db2);

    byte[] request =
    [
        0x09, 0x00, 0x40, 0x00,
        0x53, msb, lsb, db2, xor
    ];

    await SendAsync(request);
}

// Helper method for complete turnout switching cycle
public async Task SwitchTurnoutAsync(ushort fAddr, TurnoutOutput output,
    int activationTimeMs = 150)
{
    // Activate
    await SetTurnoutAsync(fAddr, output, activate: true, QueueMode.Queue);

    // Wait
    await Task.Delay(activationTimeMs);

    // Deactivate
    await SetTurnoutAsync(fAddr, output, activate: false, QueueMode.Queue);
}
```

---

## Queue Mode

**From Z21 FW V1.24**, the **Q flag** ("Queue") was introduced, fundamentally changing switching behavior.

### 5.2.1 Q=0 (Immediate Mode) - Legacy Behavior

**Behavior:**
- Switching command sent **immediately** on track, mixed into loco commands
- **Activate (A=1)** is output until client sends corresponding **Deactivate (A=0)**
- **Only ONE switching command** may be active at a time
- Similar to pressing and releasing a multiMaus key

**Client Responsibilities:**
- ✅ Correct sequence: Activate → Deactivate
- ✅ Proper timing of switching duration
- ✅ Strict serialization (wait for each to complete)

**Correct Sequence:**

```csharp
// CORRECT with Q=0
async Task SwitchMultipleTurnoutsImmediate(params (ushort Addr, TurnoutOutput Output)[] turnouts)
{
    foreach (var (addr, output) in turnouts)
    {
        await SetTurnoutAsync(addr, output, activate: true, QueueMode.Immediate);
        await Task.Delay(100);  // Wait for activation
        await SetTurnoutAsync(addr, output, activate: false, QueueMode.Immediate);
        await Task.Delay(50);   // Wait before next
    }
}

// Example
await SwitchMultipleTurnoutsImmediate(
    (4, TurnoutOutput.Output2),   // Turnout #5/A2
    (5, TurnoutOutput.Output2),   // Turnout #6/A2
    (2, TurnoutOutput.Output1)    // Turnout #3/A1
);
```

**WRONG Sequence (Do NOT do this):**

```csharp
// WRONG - activating multiple turnouts before deactivating
await SetTurnoutAsync(4, TurnoutOutput.Output2, true, QueueMode.Immediate);
await SetTurnoutAsync(5, TurnoutOutput.Output2, true, QueueMode.Immediate);
await SetTurnoutAsync(2, TurnoutOutput.Output1, true, QueueMode.Immediate);
await SetTurnoutAsync(2, TurnoutOutput.Output1, false, QueueMode.Immediate);
await SetTurnoutAsync(4, TurnoutOutput.Output2, false, QueueMode.Immediate);
await SetTurnoutAsync(5, TurnoutOutput.Output2, false, QueueMode.Immediate);
// Result: undefined end positions depending on decoder!
```

---

### 5.2.2 Q=1 (Queue Mode) - Recommended

**Behavior (FW 1.24+):**
- Switching command added to **internal FIFO queue** in Z21
- Z21 continuously checks queue for pending commands
- Command sent **four times** onto track, then removed from queue
- **No strict serialization required** by client
- Client only needs to manage **Deactivate timing**

**Advantages:**
- ✅ Simplified client logic
- ✅ Perfect for **routes** (multiple turnouts)
- ✅ No serialization needed
- ✅ More reliable

**Deactivate Timing:**
- **DCC decoders**: Deactivate may be **omitted** (decoder has automatic shut-off)
- **MM decoders**: **Always send Deactivate** (k83 and older decoders lack automatic shut-off)

```csharp
// RECOMMENDED: Queue mode for routes
async Task SwitchRoute(params (ushort Addr, TurnoutOutput Output)[] turnouts)
{
    // Activate all turnouts (queued automatically)
    foreach (var (addr, output) in turnouts)
    {
        await SetTurnoutAsync(addr, output, activate: true, QueueMode.Queue);
    }

    // Wait for all activations
    await Task.Delay(150);

    // Deactivate all (especially important for MM decoders)
    foreach (var (addr, output) in turnouts)
    {
        await SetTurnoutAsync(addr, output, activate: false, QueueMode.Queue);
    }
}

// Example: Set a route
await SwitchRoute(
    (24, TurnoutOutput.Output2),  // Turnout #25/A2
    (4, TurnoutOutput.Output2)    // Turnout #5/A2
);
```

**Track Signal (Q=1):** Commands sent 4 times each, properly spaced by Z21.

### Important Warning

⚠️ **Never mix Q=0 and Q=1 in the same application!** Choose one mode and stick with it.

---

### 5.3 LAN_X_TURNOUT_INFO

Reports turnout status.

**Sent when:**
- Response to `LAN_X_GET_TURNOUT_INFO` request
- Broadcast when turnout changed by other clients/handsets (requires broadcast flag `0x00000001`)

**Z21 to Client:**

| DataLen | Header | Data |||||||
|---------|--------|------|-----|---------|---------|----------|----------|
| 0x09 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **DB1** | **DB2** | **XOR-Byte** |
|           |           | 0x43 | FAdr_MSB | FAdr_LSB | 000000ZZ | XOR-Byte |

**DB2 (000000ZZ):**

| Value | Meaning |
|-------|---------|
| 0x00 | Turnout not switched yet |
| 0x01 | Turnout in position from command P=0 (Output 1) |
| 0x02 | Turnout in position from command P=1 (Output 2) |
| 0x03 | Invalid combination |

```csharp
public enum TurnoutPosition : byte
{
    NotSwitched = 0x00,
    Output1 = 0x01,
    Output2 = 0x02,
    Invalid = 0x03
}

public class TurnoutInfo
{
    public ushort FunctionAddress { get; set; }
    public TurnoutPosition Position { get; set; }

    public static TurnoutInfo Parse(byte[] data)
    {
        if (data.Length < 9)
            throw new ArgumentException("Invalid turnout info packet");

        int offset = 5; // Skip DataLen(2), Header(2), X-Header(1)

        byte fAdr_msb = data[offset];
        byte fAdr_lsb = data[offset + 1];
        byte positionByte = data[offset + 2];

        return new TurnoutInfo
        {
            FunctionAddress = (ushort)((fAdr_msb << 8) + fAdr_lsb),
            Position = (TurnoutPosition)(positionByte & 0x03)
        };
    }

    public override string ToString()
    {
        var (dccAddr, port) = DecodeFAdr(FunctionAddress);
        return $"Turnout FAdr={FunctionAddress} (DCC Addr={dccAddr} Port={port}): {Position}";
    }

    private static (ushort DccAddr, byte Port) DecodeFAdr(ushort fAddr)
    {
        return ((ushort)(fAddr >> 2), (byte)(fAddr & 0x03));
    }
}
```

---

## Extended Accessory Decoders

**From Z21 FW V1.40**, extended accessory decoder commands (DCC ext) allow sending:
- Switching times for turnouts
- Complex signal aspects
- 256 different states per address

Compliant with **RCN-213 Section 2.3**.

### 5.4 LAN_X_SET_EXT_ACCESSORY

Sends a command to an extended accessory decoder.

**Request to Z21:**

| DataLen | Header | Data ||||||||
|---------|--------|------|-----|---------|---------|----------|----------|----------|
| 0x0A 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **DB1** | **DB2** | **DB3** | **XOR-Byte** |
|           |           | 0x54 | Adr_MSB | Adr_LSB | DDDDDDDD | 0x00 | XOR-Byte |

**RawAddress:** `(Adr_MSB << 8) + Adr_LSB`

- **First extended decoder**: RawAddress = 4 (displayed as "Address 1" in UI)
- **Address calculation**: Strictly compliant with RCN-213 (no system-specific offsets)

**DDDDDDDD (DB2):** 256 different states (0-255)

### Z21 Switch Decoder (10836) Interpretation

Format: **RZZZZZZZ**

| Field | Description |
|-------|-------------|
| **R** (bit 7) | Output selection<br>R=1: "green" (straight)<br>R=0: "red" (branched) |
| **ZZZZZZZ** (bits 0-6) | Power-on time (100ms resolution)<br>0: Output off<br>1-126: On for Z × 100ms<br>127: On permanently (until next command) |

**Examples:**

```csharp
// Turn output "red" on for 500ms (RZZZZZZZ = 0b00000101 = 0x05)
byte state_red_500ms = 0x05;

// Turn output "green" on for 1000ms (RZZZZZZZ = 0b10001010 = 0x8A)
byte state_green_1000ms = 0x8A;

// Turn output off (RZZZZZZZ = 0x00)
byte state_off = 0x00;

// Turn output "green" on permanently (RZZZZZZZ = 0b11111111 = 0xFF)
byte state_green_permanent = 0xFF;
```

### Z21 Signal Decoder (10837) Interpretation

**DDDDDDDD** = One of 256 signal aspects (actual range depends on signal type configured).

**Common values:**

| Value | Aspect | Description |
|-------|--------|-------------|
| 0 | Stop | Red signal |
| 4 | Clear (40 km/h) | Speed limit 40 km/h |
| 16 | Clear | Green signal |
| 65 (0x41) | Shunting allowed | White signal |
| 66 (0x42) | All lights off | For distant signals |
| 69 (0x45) | Substitution | Permission to pass defect signal |

Find complete signal aspect tables at: [https://www.z21.eu/en/products/z21-signal-decoder/signaltypen](https://www.z21.eu/en/products/z21-signal-decoder/signaltypen)

### C# Implementation

```csharp
public async Task SetExtAccessoryAsync(ushort rawAddress, byte state)
{
    byte msb = (byte)((rawAddress >> 8) & 0xFF);
    byte lsb = (byte)(rawAddress & 0xFF);
    byte xor = (byte)(0x54 ^ msb ^ lsb ^ state ^ 0x00);

    byte[] request =
    [
        0x0A, 0x00, 0x40, 0x00,
        0x54, msb, lsb, state, 0x00, xor
    ];

    await SendAsync(request);
}

// Z21 Switch Decoder helpers
public static class Z21SwitchDecoder
{
    public static byte EncodeState(bool isGreen, byte timeIn100ms)
    {
        if (timeIn100ms > 127)
            throw new ArgumentOutOfRangeException(nameof(timeIn100ms), "Max 127 (12.7s)");

        byte state = timeIn100ms;
        if (isGreen)
            state |= 0x80;  // Set R bit

        return state;
    }

    public static byte Off => 0x00;
    public static byte PermanentGreen => 0xFF;
    public static byte PermanentRed => 0x7F;
}

// Z21 Signal Decoder helpers
public static class Z21SignalDecoder
{
    public const byte Stop = 0;
    public const byte Clear40 = 4;
    public const byte Clear = 16;
    public const byte ShuntingAllowed = 65;
    public const byte AllLightsOff = 66;
    public const byte Substitution = 69;
}

// Examples
await SetExtAccessoryAsync(4, Z21SwitchDecoder.EncodeState(isGreen: false, timeIn100ms: 5));
// Result: RawAddress=4 (display addr 1), output 1 "red", 500ms

await SetExtAccessoryAsync(10, Z21SignalDecoder.Clear);
// Result: Signal at RawAddress=10 shows "Clear" (green)
```

### Emergency Stop for Extended Accessory Decoders

**RCN-213 Section 2.4:** Send value 0 ("Stop") to RawAddress=2047:

```csharp
public async Task SendExtAccessoryEmergencyStopAsync()
{
    // RawAddress = 2047 (0x07FF), State = 0x00
    await SetExtAccessoryAsync(2047, 0x00);
}

// Raw bytes: 0x0A 0x00 0x40 0x00 0x54 0x07 0xFF 0x00 0x00 0xAC
```

**Reply:** Subscribed clients receive `LAN_X_EXT_ACCESSORY_INFO` broadcast.

---

### 5.5 LAN_X_GET_EXT_ACCESSORY_INFO

**From FW 1.40**, polls the last command sent to an extended accessory decoder.

**Request to Z21:**

| DataLen | Header | Data ||||||
|---------|--------|------|-----|---------|---------|----------|----------|
| 0x09 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **DB1** | **DB2** | **XOR-Byte** |
|           |           | 0x44 | Adr_MSB | Adr_LSB | 0x00 | XOR-Byte |

**DB2:** Reserved (set to 0x00)

**Reply:** See [5.6 LAN_X_EXT_ACCESSORY_INFO](#56-lan_x_ext_accessory_info)

```csharp
public async Task<ExtAccessoryInfo> GetExtAccessoryInfoAsync(ushort rawAddress)
{
    byte msb = (byte)((rawAddress >> 8) & 0xFF);
    byte lsb = (byte)(rawAddress & 0xFF);
    byte xor = (byte)(0x44 ^ msb ^ lsb ^ 0x00);

    byte[] request =
    [
        0x09, 0x00, 0x40, 0x00,
        0x44, msb, lsb, 0x00, xor
    ];

    byte[] response = await SendAndReceiveAsync(request);
    return ExtAccessoryInfo.Parse(response);
}
```

---

### 5.6 LAN_X_EXT_ACCESSORY_INFO

Reports extended accessory decoder status.

**Sent when:**
- Response to `LAN_X_GET_EXT_ACCESSORY_INFO` request
- Broadcast when accessory changed by other clients/handsets (requires broadcast flag `0x00000001`)

**Z21 to Client:**

| DataLen | Header | Data ||||||||
|---------|--------|------|-----|---------|---------|----------|----------|----------|
| 0x0A 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **DB1** | **DB2** | **DB3** | **XOR-Byte** |
|           |           | 0x44 | Adr_MSB | Adr_LSB | DDDDDDDD | Status | XOR-Byte |

**Status (DB3):**

| Value | Meaning |
|-------|---------|
| 0x00 | Data Valid |
| 0xFF | Data Unknown |

```csharp
public class ExtAccessoryInfo
{
    public ushort RawAddress { get; set; }
    public byte State { get; set; }
    public bool IsDataValid { get; set; }

    public static ExtAccessoryInfo Parse(byte[] data)
    {
        if (data.Length < 10)
            throw new ArgumentException("Invalid ext accessory info packet");

        int offset = 5; // Skip DataLen(2), Header(2), X-Header(1)

        byte adr_msb = data[offset];
        byte adr_lsb = data[offset + 1];
        byte state = data[offset + 2];
        byte status = data[offset + 3];

        return new ExtAccessoryInfo
        {
            RawAddress = (ushort)((adr_msb << 8) + adr_lsb),
            State = state,
            IsDataValid = (status == 0x00)
        };
    }

    public override string ToString()
    {
        string validStr = IsDataValid ? "Valid" : "Unknown";
        return $"ExtAccessory RawAddr={RawAddress}: State={State:X2} ({validStr})";
    }
}
```

---

## Summary

### Key Concepts

1. **Address Confusion**: Different systems number turnouts differently (be aware of "shift by 4")
2. **Function Address (FAdr)**: Combines DCC address and port
3. **Queue Mode**: Use Q=1 (queue) for modern applications; Q=0 (immediate) is legacy
4. **Extended Decoders**: Support switching times and complex signal aspects

### Command Overview

| Command | Purpose | FW Version |
|---------|---------|------------|
| `LAN_X_GET_TURNOUT_INFO` | Poll turnout status | All |
| `LAN_X_SET_TURNOUT` | Switch turnout (Q flag from 1.24) | All |
| `LAN_X_TURNOUT_INFO` | Turnout status broadcast | All |
| `LAN_X_SET_EXT_ACCESSORY` | Extended accessory control | 1.40+ |
| `LAN_X_GET_EXT_ACCESSORY_INFO` | Poll extended accessory | 1.40+ |
| `LAN_X_EXT_ACCESSORY_INFO` | Extended accessory broadcast | 1.40+ |

### Best Practices

✅ **DO:**
- Use **Queue Mode (Q=1)** for new applications
- Always deactivate MM turnouts
- Subscribe to accessory broadcasts for multi-client environments
- Use extended accessory commands for complex signal aspects

❌ **DON'T:**
- Mix Q=0 and Q=1 in the same application
- Activate multiple turnouts simultaneously with Q=0
- Forget to deactivate MM turnouts (they lack automatic shut-off)

### Address Conversion Helper

```csharp
public static class AccessoryAddressHelper
{
    // Function Address ↔ DCC Address + Port
    public static ushort ToFAdr(ushort dccAddr, byte port)
    {
        return (ushort)((dccAddr << 2) | (port & 0x03));
    }

    public static (ushort DccAddr, byte Port) FromFAdr(ushort fAddr)
    {
        return ((ushort)(fAddr >> 2), (byte)(fAddr & 0x03));
    }

    // Display Address ↔ Function Address (Roco/Z21 style)
    public static ushort DisplayToFAdr_Roco(ushort displayAddr)
    {
        // Roco: Display #1 = DCC Addr 0 Port 0 = FAdr 0
        return (ushort)(displayAddr - 1);
    }

    public static ushort FAddrToDisplay_Roco(ushort fAddr)
    {
        return (ushort)(fAddr + 1);
    }

    // Display Address ↔ Function Address (ESU style)
    public static ushort DisplayToFAdr_ESU(ushort displayAddr)
    {
        // ESU: Display #1 = DCC Addr 1 Port 0 = FAdr 4
        return (ushort)(displayAddr + 3);
    }

    public static ushort FAddrToDisplay_ESU(ushort fAddr)
    {
        return (ushort)(fAddr - 3);
    }

    // Extended Accessory: RawAddress ↔ Display Address
    public static ushort RawToDisplay(ushort rawAddress)
    {
        // RawAddress 4 = Display Address 1 (per RCN-213)
        return (ushort)(rawAddress - 3);
    }

    public static ushort DisplayToRaw(ushort displayAddress)
    {
        return (ushort)(displayAddress + 3);
    }
}
```
