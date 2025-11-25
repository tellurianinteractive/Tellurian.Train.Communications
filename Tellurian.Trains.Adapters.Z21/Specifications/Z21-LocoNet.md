# Z21 LAN Protocol - LocoNet Gateway

**Document Version:** 1.13 (English)
**Date:** November 6, 2023

**From Z21 FW Version 1.20**

This section describes the Z21's Ethernet/LocoNet gateway functionality, where the Z21 acts as the LocoNet master, refreshing slots and generating DCC packets.

## Overview

### Gateway Functionality

The Z21 can function as an **Ethernet/LocoNet gateway** with the following characteristics:

- **Z21 as LocoNet Master**: Manages refresh slots and generates DCC packets
- **Message Forwarding**: Bidirectional forwarding between LocoNet bus and LAN clients
- **Subscription Required**: Clients must enable LocoNet broadcast flags to receive messages

### Broadcast Subscription

Enable LocoNet message forwarding via `LAN_SET_BROADCASTFLAGS`:

| Flag | Description |
|------|-------------|
| **0x01000000** | Forward general LocoNet messages (without locos/switches) |
| **0x02000000** | Forward locomotive-specific messages (OPC_LOCO_SPD, OPC_LOCO_DIRF, etc.) |
| **0x04000000** | Forward switch-specific messages (OPC_SW_REQ, OPC_SW_REP, etc.) |
| **0x08000000** | LocoNet track occupancy detector status changes (FW 1.22+) |

### Important Notes

⚠️ **Network Traffic Warning**: LocoNet gateway functionality can generate **considerable network traffic** even for trivial LocoNet processes. Use with caution on Wi-Fi networks.

⚠️ **Use Cases**: Primarily designed for **PC control software** as a tool for communicating with LocoNet feedback devices. For conventional driving and switching, prefer the native Z21 LAN commands (Chapters 4, 5, 6).

⚠️ **Broadcast Consideration**: Carefully evaluate whether flags `0x02000000` (locomotives) and `0x04000000` (switches) are really necessary for your application.

### Message Flow

- **Received from LocoNet**: Z21 forwards to LAN clients with header `LAN_LOCONET_Z21_RX` (0xA0)
- **Sent to LocoNet**: Z21 forwards to LAN clients with header `LAN_LOCONET_Z21_TX` (0xA1)
- **From LAN Client**: Clients write to LocoNet via `LAN_LOCONET_FROM_LAN` (0xA2)
  - Other subscribed clients are notified
  - Original sender is **not** notified

---

## Message Forwarding Commands

### 9.1 LAN_LOCONET_Z21_RX

**From FW 1.20**

Asynchronous broadcast from Z21 to LAN clients when a message is **received** from the LocoNet bus.

**Requirements:**
- Client activated broadcast flags 0x01000000, 0x02000000, or 0x04000000
- Message received by Z21 from LocoNet bus

**Z21 to Client:**

| DataLen | Header | Data |
|---------|--------|------|
| 0x04+*n* 0x00 | 0xA0 0x00 | LocoNet message incl. CKSUM (*n* bytes) |

```csharp
public class LocoNetRxMessage
{
    public byte[] LocoNetData { get; set; }

    public static LocoNetRxMessage Parse(byte[] z21Packet)
    {
        // Skip DataLen(2) + Header(2) = 4 bytes
        int locoNetDataLength = z21Packet.Length - 4;
        byte[] locoNetData = new byte[locoNetDataLength];
        Array.Copy(z21Packet, 4, locoNetData, 0, locoNetDataLength);

        return new LocoNetRxMessage { LocoNetData = locoNetData };
    }

    public override string ToString()
    {
        return $"LocoNet RX: {BitConverter.ToString(LocoNetData)}";
    }
}
```

---

### 9.2 LAN_LOCONET_Z21_TX

**From FW 1.20**

Asynchronous broadcast from Z21 to LAN clients when a message is **sent** to the LocoNet bus by the Z21.

**Requirements:**
- Client activated broadcast flags 0x01000000, 0x02000000, or 0x04000000
- Message written to LocoNet bus by Z21

**Z21 to Client:**

| DataLen | Header | Data |
|---------|--------|------|
| 0x04+*n* 0x00 | 0xA1 0x00 | LocoNet message incl. CKSUM (*n* bytes) |

```csharp
public class LocoNetTxMessage
{
    public byte[] LocoNetData { get; set; }

    public static LocoNetTxMessage Parse(byte[] z21Packet)
    {
        int locoNetDataLength = z21Packet.Length - 4;
        byte[] locoNetData = new byte[locoNetDataLength];
        Array.Copy(z21Packet, 4, locoNetData, 0, locoNetDataLength);

        return new LocoNetTxMessage { LocoNetData = locoNetData };
    }

    public override string ToString()
    {
        return $"LocoNet TX: {BitConverter.ToString(LocoNetData)}";
    }
}
```

---

### 9.3 LAN_LOCONET_FROM_LAN

**From FW 1.20**

Allows a LAN client to write a message to the LocoNet bus.

Also sent as broadcast to other subscribed clients when another client writes to LocoNet (original sender is not notified).

**LAN Client to Z21, or Z21 to LAN Client:**

| DataLen | Header | Data |
|---------|--------|------|
| 0x04+*n* 0x00 | 0xA2 0x00 | LocoNet message incl. CKSUM (*n* bytes) |

```csharp
public async Task SendLocoNetMessageAsync(byte[] locoNetMessage)
{
    // LocoNet message must include checksum
    ushort dataLen = (ushort)(4 + locoNetMessage.Length);

    byte[] request = new byte[dataLen];
    request[0] = (byte)(dataLen & 0xFF);
    request[1] = (byte)((dataLen >> 8) & 0xFF);
    request[2] = 0xA2;
    request[3] = 0x00;
    Array.Copy(locoNetMessage, 0, request, 4, locoNetMessage.Length);

    await SendAsync(request);
}

// Example: Send OPC_MOVE_SLOTS (0xBA 0x00 0x00 0x45)
await SendLocoNetMessageAsync([0xBA, 0x00, 0x00, 0x45]);
```

#### 9.3.1 DCC Binary State Control via LocoNet OPC_IMM_PACKET

**Note:** From FW 1.42+, the command `LAN_X_SET_LOCO_BINARY_STATE` (see section 4.3.3) is **recommended** instead of this method.

**From FW 1.25**, any DCC packets can be generated using `LAN_LOCONET_FROM_LAN` with the LocoNet `OPC_IMM_PACKET` command, including Binary State Control Instructions ("F29...F32767"). This works even on the white z21, which has no physical LocoNet interface but has a virtual LocoNet stack.

**References:**
- **OPC_IMM_PACKET structure**: See LocoNet Spec (personal edition available for learning)
- **Binary State Control Instruction**: See NMRA S-9.2.1 Section "Feature Expansion Instruction"

---

## LocoNet Dispatch

### 9.4 LAN_LOCONET_DISPATCH_ADDR

**From FW 1.20**

Prepares a locomotive address for LocoNet dispatch. Corresponds to "DISPATCH_PUT" - at the next "DISPATCH_GET" (triggered by handset), the slot for this loco address is reported. The Z21 automatically occupies a free slot if necessary.

**Request to Z21:**

| DataLen | Header | Data |
|---------|--------|------|
| 0x06 0x00 | 0xA3 0x00 | 16-bit Loco address (**little-endian**) |

**Reply from Z21:**

**FW < 1.22:** None

**FW >= 1.22:**

| DataLen | Header | Data ||
|---------|--------|------|------|
| 0x07 0x00 | 0xA3 0x00 | 16-bit Loco address (**little-endian**) | Result (8-bit) |

**Result Values:**

| Value | Meaning |
|-------|---------|
| **0** | DISPATCH_PUT failed<br>May occur if Z21 is LocoNet slave and master rejected request (loco already assigned to another handset) |
| **>0** | DISPATCH_PUT successful<br>Value = current LocoNet slot number for the loco address |

```csharp
public class DispatchResult
{
    public ushort LocoAddress { get; set; }
    public byte SlotNumber { get; set; }
    public bool Success => SlotNumber > 0;
}

public async Task<DispatchResult> DispatchLocoAsync(ushort locoAddress)
{
    byte[] request =
    [
        0x06, 0x00, 0xA3, 0x00,
        (byte)(locoAddress & 0xFF),         // Little-endian
        (byte)((locoAddress >> 8) & 0xFF)
    ];

    byte[] response = await SendAndReceiveAsync(request);

    if (response.Length < 7)
    {
        // FW < 1.22: no response
        return new DispatchResult
        {
            LocoAddress = locoAddress,
            SlotNumber = 0  // Unknown
        };
    }

    return new DispatchResult
    {
        LocoAddress = (ushort)(response[4] | (response[5] << 8)),
        SlotNumber = response[6]
    };
}

// Example: Dispatch loco #3 to handset
var result = await DispatchLocoAsync(3);
if (result.Success)
{
    Console.WriteLine($"Loco {result.LocoAddress} dispatched to slot {result.SlotNumber}");
}
else
{
    Console.WriteLine($"Dispatch failed for loco {result.LocoAddress}");
}
```

---

## Track Occupancy Detectors

### 9.5 LAN_LOCONET_DETECTOR

**From FW 1.22**

Provides simplified access to LocoNet track occupancy detectors without requiring deep LocoNet protocol knowledge.

**Two Approaches:**

1. **Direct LocoNet Messages**: Receive via `LAN_LOCONET_Z21_RX` and process LocoNet messages directly
   - Requires exact LocoNet protocol knowledge
   - Generates high network traffic

2. **Simplified Detector API** (Recommended): Poll occupied status and receive asynchronous notifications
   - No deep LocoNet protocol knowledge required
   - Reduced network traffic

### Detector Comparison

| Device | Technology | Behavior |
|--------|------------|----------|
| **Roco 10787 (R-Bus)** | Mechanical switching contacts | Closed/reopened per train axis |
| **LocoNet Detectors** | Current measurement, transponder, infrared, RailCom | Ideally one message per occupancy state change |

### Polling Detector Status

**Request to Z21:**

| DataLen | Header | Data |||
|---------|--------|------|------|------|
| 0x07 0x00 | 0xA4 0x00 | Type (8-bit) | 16-bit report address (**little-endian**) |

**Type Values:**

| Type | Description |
|------|-------------|
| **0x80** | **Stationary Interrogate Request (SIC)** per Digitrax<br>Works with Blücher-Elektronik detectors<br>Report address parameter = 0 (not relevant) |
| **0x81** | **Report address** for Uhlenbrock occupancy detector<br>Configurable via LNCV 17 (default: 1017)<br>**Note**: Report address ≠ feedback address<br>Implemented via turnout switching commands on LocoNet (value decremented by 1) |
| **0x82** | **LISSY status request** (FW 1.23+)<br>For Uhlenbrock LISSY, report address = feedback address<br>Feedback depends on LISSY receiver operating mode |

**Important:** A single request may address multiple detectors → expect multiple responses. Depending on manufacturer, the same input status may be reported multiple times.

```csharp
public enum DetectorType : byte
{
    SIC_Digitrax = 0x80,        // Digitrax/Blücher
    Uhlenbrock = 0x81,          // Uhlenbrock occupancy detector
    LISSY = 0x82                // Uhlenbrock LISSY (FW 1.23+)
}

public async Task RequestDetectorStatusAsync(DetectorType type, ushort reportAddress = 0)
{
    byte[] request =
    [
        0x07, 0x00, 0xA4, 0x00,
        (byte)type,
        (byte)(reportAddress & 0xFF),         // Little-endian
        (byte)((reportAddress >> 8) & 0xFF)
    ];

    await SendAsync(request);
    // Expect one or more LAN_LOCONET_DETECTOR responses
}

// Examples
await RequestDetectorStatusAsync(DetectorType.SIC_Digitrax);
await RequestDetectorStatusAsync(DetectorType.Uhlenbrock, 1017);
```

**Example - Uhlenbrock Report Address:**

Request detectors with report address 1017:
```
0x07 0x00 0xA4 0x00 0x81 0xF8 0x03
```
- Report address = `0x03F8 + 1 = 1016 + 1 = 1017`

---

### Detector Status Response

**Z21 to Client:**

| DataLen | Header | Data ||||
|---------|--------|------|------|------|------|
| 0x07+*n* 0x00 | 0xA4 0x00 | Type (8-bit) | Feedback address 16-bit (**little-endian**) | Info[*n*] |

**Trigger Conditions:**
- Client activated broadcast flag **0x08000000**
- AND either:
  - Detector status changed on input
  - Explicit status request by LAN client

**Feedback Address:** Each detector input has a unique, user-configurable feedback address (via LNCV) that identifies the monitored block.

**Info[n]:** Byte array; content and length depend on **Type**.

---

### Detector Types

#### Type 0x01 - Simple Occupancy (Free/Occupied)

**Detectors:** Uhlenbrock 63320, Blücher GBM16XL
**Protocol:** LocoNet OPC_INPUT_REP (X=1)
**Info length:** n=1

| Info[0] | Status |
|---------|--------|
| 0 | Sensor is **LOW** (free) |
| 1 | Sensor is **HIGH** (occupied) |

```csharp
public class SimpleOccupancyInfo
{
    public ushort FeedbackAddress { get; set; }
    public bool IsOccupied { get; set; }

    public static SimpleOccupancyInfo Parse(byte[] data)
    {
        ushort feedbackAddr = (ushort)(data[5] | (data[6] << 8));
        bool occupied = data[7] == 1;

        return new SimpleOccupancyInfo
        {
            FeedbackAddress = feedbackAddr,
            IsOccupied = occupied
        };
    }
}
```

---

#### Type 0x02 / 0x03 - Transponder Enters/Exits Block

**Detectors:** Blücher GBM16XN (RailCom-based)
**Protocol:** LocoNet OPC_MULTI_SENSE transponding
**Info length:** n=2

| Type | Event |
|------|-------|
| 0x02 | Transponder **enters** block |
| 0x03 | Transponder **exits** block |

**Info[0..1]:** 16-bit transponder address (little-endian) = locomotive address determined via RailCom

**GBM16XN Quirks:**

⚠️ Due to LocoNet spec ambiguity:
- Add **+1** to feedback address to get configured GBM16XN feedback address
- Direction may be encoded in bit mask `0x1000` (not recommended - collides with long loco addresses!)

```csharp
public class TransponderInfo
{
    public ushort FeedbackAddress { get; set; }
    public ushort TransponderAddress { get; set; }  // Usually loco address
    public bool IsEntering { get; set; }            // true=entering, false=exiting

    public static TransponderInfo Parse(byte[] data, bool isEntering)
    {
        ushort feedbackAddr = (ushort)(data[5] | (data[6] << 8));
        ushort transponderAddr = (ushort)(data[7] | (data[8] << 8));

        return new TransponderInfo
        {
            FeedbackAddress = feedbackAddr,
            TransponderAddress = transponderAddr & 0x0FFF, // Mask direction bit
            IsEntering = isEntering
        };
    }

    public override string ToString()
    {
        string action = IsEntering ? "entered" : "exited";
        return $"Loco {TransponderAddress} {action} block at feedback {FeedbackAddress}";
    }
}
```

---

#### Type 0x10 - LISSY Loco Address

**From FW 1.23**

**Detectors:** Uhlenbrock LISSY receiver with "Transfer format (ÜF) Uhlenbrock" (LNCV 15=1)
**Info length:** n=3

**Info[0..1]:** 16-bit loco address (little-endian)
- Locomotives: 1-9999
- Wagons: 10000-16382

**Info[2]:** Additional information

| Bit 7 | Bit 6 | Bit 5 | Bit 4 | Bit 3-0 |
|-------|-------|-------|-------|---------|
| 0 | DIR1 | DIR0 | 0 | K3 K2 K1 K0 |

- **DIR1=0**: Ignore DIR0
- **DIR1=1**: DIR0=0 forward, DIR0=1 backward
- **K3..K0**: 4-bit class information from LISSY sender

**Example LISSY Configuration (68610):**

| LNCV | Value | Comment |
|------|-------|---------|
| 2 | 98 | Optional: Module reset |
| 2 | 0 | Basic function: Read loco + direction (double sensor) |
| 15 | 1 | Send using Transfer format (ÜF) Uhlenbrock |

```csharp
public class LissyLocoInfo
{
    public ushort FeedbackAddress { get; set; }
    public ushort LocoAddress { get; set; }
    public bool IsWagon => LocoAddress >= 10000;
    public bool HasDirection { get; set; }
    public bool IsForward { get; set; }
    public byte ClassInfo { get; set; }  // 4-bit value

    public static LissyLocoInfo Parse(byte[] data)
    {
        ushort feedbackAddr = (ushort)(data[5] | (data[6] << 8));
        ushort locoAddr = (ushort)(data[7] | (data[8] << 8));
        byte additionalInfo = data[9];

        bool hasDir = (additionalInfo & 0x40) != 0;
        bool isForward = (additionalInfo & 0x20) == 0;
        byte classInfo = (byte)(additionalInfo & 0x0F);

        return new LissyLocoInfo
        {
            FeedbackAddress = feedbackAddr,
            LocoAddress = locoAddr,
            HasDirection = hasDir,
            IsForward = isForward,
            ClassInfo = classInfo
        };
    }
}
```

---

#### Type 0x11 - LISSY Block Status

**From FW 1.23**

**Detectors:** Uhlenbrock LISSY with block occupancy status messaging
**Info length:** n=1

| Info[0] | Status |
|---------|--------|
| 0 | Block is **free** |
| 1 | Block is **occupied** |

**Example LISSY Configuration (68610):**

| LNCV | Value | Comment |
|------|-------|---------|
| 2 | 98 | Optional: Module reset |
| 2 | 22 | Automation: Time-controlled waiting station |
| 3 | 2 | Automation active in both directions |
| 4 | 3 | Wait time 3 seconds |
| 10 | 2 | Block status change to "free" after 2 seconds |
| 15 | 1 | Send Transfer format (ÜF) Uhlenbrock |

---

#### Type 0x12 - LISSY Speed

**From FW 1.23**

**Detectors:** Uhlenbrock LISSY with speed measurement
**Info length:** n=2

**Info[0..1]:** 16-bit speed value (little-endian)

**Example LISSY Configuration (68610):**

| LNCV | Value | Comment |
|------|-------|---------|
| 2 | 98 | Optional: Module reset |
| 2 | 0 | Basic function: Read loco + direction (double sensor) |
| 14 | 15660 | Velocity Scaling = 1566 (H0) × 10mm (sensor distance) |
| 15 | 1 | Send Transfer format Uhlenbrock |

```csharp
public class LissySpeedInfo
{
    public ushort FeedbackAddress { get; set; }
    public ushort Speed { get; set; }  // Scaled speed value

    public static LissySpeedInfo Parse(byte[] data)
    {
        ushort feedbackAddr = (ushort)(data[5] | (data[6] << 8));
        ushort speed = (ushort)(data[7] | (data[8] << 8));

        return new LissySpeedInfo
        {
            FeedbackAddress = feedbackAddr,
            Speed = speed
        };
    }
}
```

---

## Complete Detector Handler

```csharp
public class LocoNetDetectorHandler
{
    public event Action<SimpleOccupancyInfo>? OccupancyChanged;
    public event Action<TransponderInfo>? TransponderDetected;
    public event Action<LissyLocoInfo>? LissyLocoDetected;
    public event Action<LissySpeedInfo>? LissySpeedDetected;

    public void HandleDetectorMessage(byte[] data)
    {
        if (data.Length < 7 || data[2] != 0xA4)
            return;

        byte type = data[4];

        switch (type)
        {
            case 0x01:  // Simple occupancy
                OccupancyChanged?.Invoke(SimpleOccupancyInfo.Parse(data));
                break;

            case 0x02:  // Transponder enters
                TransponderDetected?.Invoke(TransponderInfo.Parse(data, isEntering: true));
                break;

            case 0x03:  // Transponder exits
                TransponderDetected?.Invoke(TransponderInfo.Parse(data, isEntering: false));
                break;

            case 0x10:  // LISSY loco
                LissyLocoDetected?.Invoke(LissyLocoInfo.Parse(data));
                break;

            case 0x11:  // LISSY block (reuse SimpleOccupancyInfo)
                OccupancyChanged?.Invoke(SimpleOccupancyInfo.Parse(data));
                break;

            case 0x12:  // LISSY speed
                LissySpeedDetected?.Invoke(LissySpeedInfo.Parse(data));
                break;

            default:
                Console.WriteLine($"Unknown detector type: 0x{type:X2}");
                break;
        }
    }
}

// Usage
var detectorHandler = new LocoNetDetectorHandler();

detectorHandler.OccupancyChanged += info =>
{
    Console.WriteLine($"Block {info.FeedbackAddress}: {(info.IsOccupied ? "Occupied" : "Free")}");
};

detectorHandler.TransponderDetected += info =>
{
    Console.WriteLine(info.ToString());
};

detectorHandler.LissyLocoDetected += info =>
{
    string vehicle = info.IsWagon ? "Wagon" : "Loco";
    Console.WriteLine($"{vehicle} {info.LocoAddress} detected at {info.FeedbackAddress}");
};
```

---

## Summary

### Key Concepts

1. **Gateway Mode**: Z21 acts as LocoNet master, forwarding messages bidirectionally
2. **High Traffic**: LocoNet gateway generates considerable network traffic
3. **Selective Subscription**: Choose broadcast flags carefully
4. **Dispatch**: Prepare locos for handset transfer
5. **Detectors**: Simplified API for occupancy, transponder, and LISSY support

### Command Overview

| Command | Header | Direction | Purpose |
|---------|--------|-----------|---------|
| `LAN_LOCONET_Z21_RX` | 0xA0 | Z21 → Client | Messages received from LocoNet |
| `LAN_LOCONET_Z21_TX` | 0xA1 | Z21 → Client | Messages sent to LocoNet |
| `LAN_LOCONET_FROM_LAN` | 0xA2 | Bidirectional | Write to LocoNet / notify other clients |
| `LAN_LOCONET_DISPATCH_ADDR` | 0xA3 | Request/Reply | Prepare loco for dispatch |
| `LAN_LOCONET_DETECTOR` | 0xA4 | Request/Broadcast | Detector status |

### Detector Types

| Type | Description | Info Size |
|------|-------------|-----------|
| 0x01 | Simple occupancy (free/occupied) | 1 byte |
| 0x02 | Transponder enters block | 2 bytes |
| 0x03 | Transponder exits block | 2 bytes |
| 0x10 | LISSY loco address | 3 bytes |
| 0x11 | LISSY block status | 1 byte |
| 0x12 | LISSY speed | 2 bytes |

**Note:** Type IDs will be extended in future firmware versions as needed.

### Best Practices

✅ **DO:**
- Use for PC automation software with LocoNet feedback devices
- Enable broadcast flag 0x08000000 for detector support
- Handle multiple responses from single detector requests
- Use native Z21 LAN commands for driving/switching

❌ **DON'T:**
- Use for mobile controllers (too much network traffic)
- Enable unnecessary broadcast flags (0x02000000, 0x04000000)
- Assume single response from detector polls
- Rely on GBM16XN direction bit (0x1000) for long addresses
