# LocoNet Protocol Specification v1.0 - Developer Guide

> **Source:** LocoNet Personal Use Edition 1.0 by Digitrax Inc., October 16, 1997
> **Copyright:** Digitrax Inc. - For non-commercial private use only
> **Document Purpose:** Clarified specification for C# API implementation

---

## Table of Contents
1. [Overview & Core Concepts](#overview--core-concepts)
2. [Physical Connection (Minimal)](#physical-connection-minimal)
3. [Network Timing & Access](#network-timing--access)
4. [Message Format & Structure](#message-format--structure)
5. [Opcode Reference](#opcode-reference)
6. [Locomotive Control (Slots)](#locomotive-control-slots)
7. [Switch/Turnout Control](#switchturnout-control)
8. [Sensor & Feedback](#sensor--feedback)
9. [Programming Track](#programming-track)
10. [Fast Clock](#fast-clock)
11. [Common Workflows & Examples](#common-workflows--examples)

---

## Overview & Core Concepts

### What is LocoNet?

LocoNet is a **peer-to-peer distributed network** for model railroad control. Unlike centralized systems where a master polls slave devices, LocoNet allows all devices to communicate freely on the network.

**Key Characteristics:**
- **Peer-to-peer:** No single master controller (though one device maintains the DCC refresh stack)
- **Event-driven:** Devices send messages when they have something to say, not when polled
- **CSMA/CD:** Carrier Sense Multiple Access with Collision Detection (like Ethernet)
- **Scalable:** Can add devices without reconfiguring the entire system
- **Multi-drop:** All devices see all messages

### Network Topology

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Throttle   │     │   Command   │     │   Sensor    │
│   (DT100)   │     │  Station    │     │   Module    │
└──────┬──────┘     └──────┬──────┘     └──────┬──────┘
       │                   │                   │
       └───────────────────┴───────────────────┘
                    LocoNet Bus
                   (All hear all)
```

### The "Master" Device

While LocoNet is peer-to-peer, one device is designated as the **Master** (typically a command station like DCS100):
- **Maintains the DCC refresh stack** (keeps locomotives running)
- **Manages locomotive slots** (allocates slot numbers)
- **Has priority network access** (lowest delay before transmitting)
- **Other devices can function without understanding everything the Master does**

### The Slot System

Think of slots as "locomotive handles" or "file descriptors" for trains:
- **Up to 120 slots** (addresses 0-119) for active locomotives
- **Slots 120-127** are reserved for system functions
- **Slot 124 ($7C):** Programming track operations
- **Slot 123 ($7B):** Fast clock data
- **Slot 0:** Special - used for dispatching

Each slot contains:
- Locomotive address (7-bit or 14-bit DCC address)
- Current speed and direction
- Function states (F0-F8)
- Status flags (in-use, consist info, decoder type)
- Device ID (which throttle is using it)

---

## Physical Connection (Minimal)

Since you're using existing hardware, here's what you need to know:

### Connector Pinout (RJ12 6-pin)
```
Pin 1: RAIL_SYNC-  (white) - DCC track signal copy
Pin 2: SIGNAL GROUND
Pin 3: LOCONET-    (data signal)
Pin 4: LOCONET+    (data signal)
Pin 5: SIGNAL GROUND
Pin 6: RAIL_SYNC+  (blue) - DCC track signal copy
```

**For PC/Software Interface:**
- Only need **pins 2, 3, 4, 5** (ground and LocoNet data)
- Pins 3 & 4 are paralleled in single-ended mode (polarity insensitive)
- Connect to UART or USB-serial adapter

### Signal Levels (Single-Ended Implementation)
- **Logic HIGH (1):** +4.0V or above (nominal +12V)
- **Logic LOW (0):** Below +4.0V
- **Idle state:** HIGH (MARK)
- **Data format:** Asynchronous serial (8N1)

### Baud Rate
- **16.66 KBaud** (±1.5%)
- For PC: Use **16.457 KBaud** (divisor 07 for NS8250 UART)
- **60 microseconds per bit**

---

## Network Timing & Access

### Basic Timing Parameters

| Parameter | Value | Description |
|-----------|-------|-------------|
| Bit time | 60 µs | Time for one bit |
| Baud rate | 16,666 bps | Serial data rate |
| CD Backoff | 1.2 ms | 20 bit times - carrier detect timeout |
| Master delay | 360 µs | 6 bit times - minimum delay for non-Master devices |
| Priority delay | 0-20 bit times | Additional delay for prioritization (0-1200 µs) |
| Break duration | 15 bit times | 900 µs - collision notification |
| Disconnect timeout | 100 ms | Bus low this long = disconnected |
| Startup backoff | 250 ms | Wait after reconnect before transmitting |

### Network Access Protocol (CSMA/CD)

LocoNet uses collision detection and backoff to share the bus:

```
Device wants to transmit:
  │
  ├─> Wait for bus IDLE (HIGH)
  │
  ├─> Start CD Backoff timer (1.2 ms)
  │
  ├─> If NOT Master: Add 360 µs (Master delay)
  │
  ├─> On first attempt: Add 0-20 bit priority delay
  │   (Decrements on each retry)
  │
  ├─> Bus still free?
  │   ├─> YES: Seize within 2 µs
  │   └─> NO: Decrement priority, try again after current message
  │
  ├─> Monitor transmit echo
  │   ├─> Mismatch detected?
  │   │   └─> COLLISION! Send 15-bit BREAK
  │   └─> Success: Message sent
  │
  └─> Done
```

**Example Timeline:**
```
Time    Event
0 µs    Bus goes IDLE (last message finished)
0 µs    All devices start CD Backoff (1200 µs)
1200    Master can seize now
1560    Other devices can seize (if Master didn't)
1560+   Priority delay 0-1200 µs (decrements on retry)
```

### Collision Handling

When a device detects a collision (transmit echo doesn't match sent data):
1. **Immediately send a 15-bit BREAK** (900 µs LOW pulse)
2. **All receivers see framing error** and discard partial message
3. **Network is free to re-arbitrate**
4. **Transmitter decrements its priority delay** and tries again

### PC "Smart" Access

PCs can bypass the CD backoff if they understand the protocol:
- **Monitor messages to know when network is truly free**
- **Seize immediately** before hardware CD backoff expires
- **Allows PC to control network if needed**
- **Multiple PCs subdivide the 20-bit CD backoff window**

---

## Message Format & Structure

### Basic Message Structure

All LocoNet messages follow this format:
```
┌──────────┬──────────┬──────────┬─────┬──────────┐
│  OPCODE  │  ARG 1   │  ARG 2   │ ... │ CHECKSUM │
│  (MSB=1) │ (MSB=0)  │ (MSB=0)  │     │ (MSB=0)  │
└──────────┴──────────┴──────────┴─────┴──────────┘
```

**Rules:**
- **First byte:** OPCODE with MSB (bit 7) = 1
- **All other bytes:** MSB = 0 (bits 0-6 contain data)
- **Last byte:** Checksum
- **Invalid format?** Discard entire message

### Opcode Structure

The opcode byte encodes both the operation AND the message length:

```
Bit:  7   6   5   4   3   2   1   0
     ┌───┬───┬───┬───┬───┬───┬───┬───┐
     │ 1 │Length │ F │  Operation Code  │
     └───┴───┴───┴───┴───┴───┴───┴───┘
           │   │   │
           │   │   └─> Follow-on bit (1 = expect response)
           │   │
           └───┴─────> Message length encoding
```

**Length Encoding (bits 6-5):**

| Bits 6-5 | Length | Description |
|----------|--------|-------------|
| 0 0 | 2 bytes | Opcode + Checksum |
| 0 1 | 4 bytes | Opcode + 2 args + Checksum |
| 1 0 | 6 bytes | Opcode + 4 args + Checksum |
| 1 1 | Variable | Next byte is byte count (N bytes total) |

**Bit 3 (F):**
- **1 = Follow-on message expected** (request/response pattern)
- **0 = No response** (fire-and-forget)

**Example:**
```
0xBF = 10111111
       │ │││││││
       │ ││││└┴┴─> Operation bits (31)
       │ │││└────> Follow-on = 1 (expects response)
       │ └┴┴─────> Length = 01 = 4 bytes
       └─────────> Opcode flag = 1

This is OPC_LOCO_ADR: Request locomotive address (4 bytes, expects slot read response)
```

### Checksum Calculation

The checksum is a **1's complement of XOR** of all bytes except the checksum itself.

**Algorithm:**
```csharp
byte CalculateChecksum(byte[] message)
{
    byte xor = 0;
    for (int i = 0; i < message.Length - 1; i++)  // All except checksum
    {
        xor ^= message[i];
    }
    return (byte)(~xor & 0x7F);  // 1's complement, keep MSB=0
}

bool ValidateChecksum(byte[] message)
{
    byte xor = 0;
    for (int i = 0; i < message.Length; i++)  // All bytes including checksum
    {
        xor ^= message[i];
    }
    return xor == 0xFF;  // Valid if XOR of all bytes = 0xFF
}
```

**Example:**
```
Message: [0xBF, 0x00, 0x03, ??]
XOR: 0xBF ^ 0x00 ^ 0x03 = 0xBC
Checksum: ~0xBC & 0x7F = 0x43

Complete message: [0xBF, 0x00, 0x03, 0x43]
Validation: 0xBF ^ 0x00 ^ 0x03 ^ 0x43 = 0xFF ✓
```

---

## Opcode Reference

### Quick Lookup Table

| Opcode | Name | Length | Response | Description |
|--------|------|--------|----------|-------------|
| **Power & System** |
| 0x82 | OPC_GPOFF | 2 | No | Global power OFF |
| 0x83 | OPC_GPON | 2 | No | Global power ON |
| 0x85 | OPC_IDLE | 2 | No | Force IDLE state, emergency stop |
| 0x81 | OPC_BUSY | 2 | No | Master busy (NOP/time filler) |
| **Locomotive Control** |
| 0xBF | OPC_LOCO_ADR | 4 | Yes | Request locomotive address |
| 0xBB | OPC_RQ_SL_DATA | 4 | Yes | Request slot data |
| 0xBA | OPC_MOVE_SLOTS | 4 | Yes | Move/dispatch/activate slot |
| 0xB9 | OPC_LINK_SLOTS | 4 | Yes | Link slots for consisting |
| 0xB8 | OPC_UNLINK_SLOTS | 4 | Yes | Unlink consist slots |
| 0xA0 | OPC_LOCO_SPD | 4 | No | Set locomotive speed |
| 0xA1 | OPC_LOCO_DIRF | 4 | No | Set direction and F0-F4 |
| 0xA2 | OPC_LOCO_SND | 4 | No | Set sound/functions F5-F8 |
| 0xB6 | OPC_CONSIST_FUNC | 4 | No | Set functions in consist |
| 0xB5 | OPC_SLOT_STAT1 | 4 | No | Write slot status1 byte |
| 0xE7 | OPC_SL_RD_DATA | 14 | No | Slot data read (response) |
| 0xEF | OPC_WR_SL_DATA | 14 | Yes | Write slot data |
| **Switches/Turnouts** |
| 0xB0 | OPC_SW_REQ | 4 | No | Request switch function (no ack) |
| 0xBD | OPC_SW_ACK | 4 | Yes | Request switch with acknowledge |
| 0xBC | OPC_SW_STATE | 4 | Yes | Request switch state |
| 0xB1 | OPC_SW_REP | 4 | No | Turnout sensor state report |
| **Sensors/Input** |
| 0xB2 | OPC_INPUT_REP | 4 | No | General sensor input report |
| **Programming** |
| 0xED | OPC_IMM_PACKET | Variable | Yes | Send immediate DCC packet |
| 0xEF | OPC_WR_SL_DATA | 14 | Yes | Write slot 124 = program |
| 0xE7 | OPC_SL_RD_DATA | 14 | No | Read slot 124 = prog result |
| **Acknowledgment** |
| 0xB4 | OPC_LONG_ACK | 4 | No | Long acknowledgment (response) |
| **Advanced** |
| 0xE5 | OPC_PEER_XFER | Variable | No | Peer-to-peer transfer (8 bytes) |

### 2-Byte Messages (System Control)

#### OPC_GPOFF (0x82) - Global Power OFF
```
Format: [0x82, 0x7D]
Effect: Turn off track power globally
```

#### OPC_GPON (0x83) - Global Power ON
```
Format: [0x83, 0x7C]
Effect: Turn on track power globally
```

#### OPC_IDLE (0x85) - Emergency Stop
```
Format: [0x85, 0x7A]
Effect: Force idle state, broadcast emergency stop to all locomotives
```

#### OPC_BUSY (0x81) - Master Busy
```
Format: [0x81, 0x7E]
Effect: NOP/time filler, restarts CD backoff timers
Note: Strip and ignore this opcode - it's for hardware timing
```

---

## Locomotive Control (Slots)

### Understanding Slots

A **slot** is the Master's internal representation of a locomotive. Think of it as a database record:

```
Slot #42:
  ├─ Address: 3 (short DCC address)
  ├─ Speed: 64 (half speed)
  ├─ Direction: Forward
  ├─ Functions: F0=ON, F1=OFF, F2=ON, F3=OFF, F4=OFF
  ├─ Status: IN_USE
  ├─ Decoder Type: 128-step mode
  ├─ Consist: Not consisted
  └─ Device ID: Throttle #5
```

### Slot Status States

Slots can be in various states:

| Status | Bits 5-4 | Description |
|--------|----------|-------------|
| **FREE** | 00 | Empty slot, no locomotive assigned |
| **COMMON** | 01 | Loco address assigned, being refreshed, but not controlled |
| **IDLE** | 10 | Loco address assigned, NOT being refreshed |
| **IN_USE** | 11 | Loco address assigned, being refreshed and controlled |

**Consist Status (bits 6-3):**

| SL_CONUP (6) | SL_CONDN (3) | Meaning |
|--------------|--------------|---------|
| 0 | 0 | **FREE** - Not in consist |
| 0 | 1 | **SUB-MEMBER** - Linked upward only |
| 1 | 0 | **CONSIST TOP** - Linked downward only (this is the "lead" loco) |
| 1 | 1 | **MID-CONSIST** - Linked both up and down |

### Requesting a Locomotive

**Workflow to control a locomotive:**

```
Step 1: Request address
  → Send: OPC_LOCO_ADR

Step 2: Master responds
  ← Receive: OPC_SL_RD_DATA (with slot number and current state)
     OR
  ← Receive: OPC_LONG_ACK (fail code if no slots available)

Step 3: Check status
  → If COMMON/IDLE: Need to activate slot
  → If IN_USE by someone else: Don't use it!
  → If UP_CONSISTED: Need to unlink first

Step 4: Activate slot (if needed)
  → Send: OPC_MOVE_SLOTS (NULL move: source=dest)

Step 5: Control locomotive
  → Send: OPC_LOCO_SPD, OPC_LOCO_DIRF, OPC_LOCO_SND
```

### OPC_LOCO_ADR (0xBF) - Request Locomotive Address

**Purpose:** Ask the Master for a slot containing a specific locomotive address.

**Format:**
```
Byte 0: 0xBF                    Opcode
Byte 1: <loco_adr_hi>          High 7 bits of address (0 for short address)
Byte 2: <loco_adr_lo>          Low 7 bits of address
Byte 3: <checksum>             Checksum
```

**Address Encoding:**
- **Short address (1-127):** `adr_hi = 0x00`, `adr_lo = address`
- **Long address (128-9999):** Both bytes used (14-bit address)
- **Analog loco (address 0):** `adr_hi = 0x00`, `adr_lo = 0x00`

**Response:**
- **Success:** `OPC_SL_RD_DATA` (14 bytes) with slot information
- **Failure:** `OPC_LONG_ACK` with fail code `[0xB4, 0x3F, 0x00, CHK]`

**Example - Request locomotive #3:**
```csharp
byte[] request = { 0xBF, 0x00, 0x03, 0x43 };
// 0xBF = Request address
// 0x00 = High byte (0 for short address)
// 0x03 = Address 3
// 0x43 = Checksum
```

**Example - Request long address 1234:**
```csharp
// Long address 1234 = 0x04D2
// adr_hi gets top 6 bits: (1234 >> 8) = 0x04
// adr_lo gets bottom 7 bits with top bit of low byte:
//   (1234 & 0xFF) = 0xD2, but must fit in 7 bits
// Actually: adr_hi = 0x04, adr_lo = 0x52

byte[] request = { 0xBF, 0x04, 0x52, 0xXX };  // Calculate checksum
```

### OPC_SL_RD_DATA (0xE7) - Slot Data Read

**Purpose:** Master's response containing all slot information (10 data bytes + overhead = 14 bytes total).

**Format:**
```
Byte  0: 0xE7               Opcode
Byte  1: 0x0E               Byte count (14)
Byte  2: <slot>             Slot number (0-127)
Byte  3: <stat1>            Status byte 1
Byte  4: <adr>              Locomotive address (low 7 bits)
Byte  5: <spd>              Speed
Byte  6: <dirf>             Direction and F0-F4
Byte  7: <trk>              Track status (global)
Byte  8: <ss2>              Status byte 2
Byte  9: <adr2>             Locomotive address (high 7 bits)
Byte 10: <snd>              Sound/Functions F5-F8
Byte 11: <id1>              Device ID (low 7 bits)
Byte 12: <id2>              Device ID (high 7 bits)
Byte 13: <checksum>         Checksum
```

**STAT1 Byte (byte 3):** Slot status and decoder type
```
Bit 7: Reserved (purge enable)
Bit 6: SL_CONUP    - Consist up-link
Bit 5: SL_BUSY     - Busy/Active status (high bit)
Bit 4: SL_ACTIVE   - Busy/Active status (low bit)
Bit 3: SL_CONDN    - Consist down-link
Bit 2: SL_SPDEX    - Decoder type (bit 2)
Bit 1: SL_SPD14    - Decoder type (bit 1)
Bit 0: SL_SPD28    - Decoder type (bit 0)
```

**Decoder Type (bits 2-0):**

| Bits 2-0 | Mode | Description |
|----------|------|-------------|
| 000 | 28-step | 3-byte packet, regular mode |
| 001 | 28-step | Trinary packets |
| 010 | 14-step | 14 speed steps |
| 011 | 128-step | 128 speed steps (most common) |
| 100 | 28-step | Advanced DCC consisting |
| 111 | 128-step | Advanced DCC consisting |

**SPD Byte (byte 5):** Speed value
```
0x00 = Stop (with inertia/momentum)
0x01 = Emergency stop (immediate)
0x02-0x7F = Speed steps 2-127 (0x7F = maximum speed)
```

**DIRF Byte (byte 6):** Direction and functions
```
Bit 7: Always 0
Bit 6: Reserved
Bit 5: Direction (1=Forward, 0=Reverse)
Bit 4: F0 (headlight)
Bit 3: F4
Bit 2: F3
Bit 1: F2
Bit 0: F1
```

**TRK Byte (byte 7):** Global track status
```
Bit 7-4: Reserved
Bit 3: GTRK_PROG_BUSY (1=Programming track busy)
Bit 2: GTRK_MLOK1 (1=LocoNet 1.1, 0=DT200)
Bit 1: GTRK_IDLE (0=Track paused/emergency stop)
Bit 0: GTRK_POWER (1=Track power ON)
```

**SND Byte (byte 10):** Sound functions F5-F8
```
Bit 7-4: Reserved
Bit 3: F8
Bit 2: F7
Bit 1: F6
Bit 0: F5
```

**Example - Parsing a slot read:**
```csharp
byte[] slotRead = {
    0xE7,        // Opcode
    0x0E,        // 14 bytes
    0x05,        // Slot 5
    0x33,        // STAT1: IN_USE, 128-step mode
    0x03,        // Address low = 3
    0x40,        // Speed = 64
    0x28,        // DIRF: Forward, F0 ON, F3 ON
    0x03,        // TRK: Power ON, not programming
    0x00,        // SS2
    0x00,        // Address high = 0 (short address)
    0x00,        // SND: No sound functions
    0x05,        // ID1
    0x01,        // ID2
    0xXX         // Checksum
};

// Parse:
int slot = slotRead[2];                    // 5
int address = slotRead[4];                 // 3 (short)
int speed = slotRead[5];                   // 64
bool forward = (slotRead[6] & 0x20) != 0;  // true
bool f0 = (slotRead[6] & 0x10) != 0;       // true
bool f3 = (slotRead[6] & 0x08) != 0;       // true
bool powerOn = (slotRead[7] & 0x01) != 0;  // true
```

### OPC_MOVE_SLOTS (0xBA) - Move/Activate Slot

**Purpose:** Multi-function command for slot management.

**Format:**
```
Byte 0: 0xBA           Opcode
Byte 1: <src>          Source slot number
Byte 2: <dest>         Destination slot number
Byte 3: <checksum>     Checksum
```

**Special Cases:**

**1. NULL Move (Activate Slot):**
```csharp
// Activate slot 5 (mark as IN_USE)
byte[] activate = { 0xBA, 0x05, 0x05, 0xXX };  // SRC=DEST
```

**2. Dispatch PUT:**
```csharp
// Put slot 5 into dispatch stack
byte[] dispatchPut = { 0xBA, 0x05, 0x00, 0xXX };  // DEST=0
```

**3. Dispatch GET:**
```csharp
// Get dispatched locomotive
byte[] dispatchGet = { 0xBA, 0x00, 0x00, 0xXX };  // SRC=0, DEST=don't care
// Response: OPC_SL_RD_DATA if dispatch slot exists
// or OPC_LONG_ACK with fail if no dispatch slot
```

**4. Move Slot:**
```csharp
// Move slot 5 to slot 8 (if slot 5 is not IN_USE)
byte[] move = { 0xBA, 0x05, 0x08, 0xXX };
// Source slot is cleared, destination gets the data
```

**Response:**
- **Success:** `OPC_SL_RD_DATA` with destination slot info
- **Failure:** `OPC_LONG_ACK` `[0xB4, 0x3A, 0x00, CHK]`

### OPC_LOCO_SPD (0xA0) - Set Locomotive Speed

**Purpose:** Change the speed of a locomotive in a slot.

**Format:**
```
Byte 0: 0xA0           Opcode
Byte 1: <slot>         Slot number
Byte 2: <spd>          Speed value (0-127)
Byte 3: <checksum>     Checksum
```

**Speed Values:**
- `0x00` = Stop (with momentum/inertia)
- `0x01` = Emergency stop (immediate halt)
- `0x02-0x7F` = Speed steps 2-127

**Example:**
```csharp
// Set slot 5 to half speed (64)
byte[] setSpeed = { 0xA0, 0x05, 0x40, 0xXX };

// Emergency stop slot 5
byte[] eStop = { 0xA0, 0x05, 0x01, 0xXX };
```

### OPC_LOCO_DIRF (0xA1) - Set Direction and F0-F4

**Purpose:** Control direction and functions F0-F4.

**Format:**
```
Byte 0: 0xA1           Opcode
Byte 1: <slot>         Slot number
Byte 2: <dirf>         Direction and function bits
Byte 3: <checksum>     Checksum
```

**DIRF Byte:**
```
Bit 7: Always 0
Bit 6: Reserved (set to 0)
Bit 5: Direction (1=Forward, 0=Reverse)
Bit 4: F0 (typically headlight)
Bit 3: F4
Bit 2: F3
Bit 1: F2
Bit 0: F1
```

**Example:**
```csharp
// Slot 5: Forward, F0 ON, F2 ON
// Bit pattern: 0 0 1 1 0 1 0 0 = 0x34
byte[] setDirF = { 0xA1, 0x05, 0x34, 0xXX };

// Reverse with all functions off
byte[] reverse = { 0xA1, 0x05, 0x00, 0xXX };
```

### OPC_LOCO_SND (0xA2) - Set Sound Functions F5-F8

**Purpose:** Control functions F5-F8.

**Format:**
```
Byte 0: 0xA2           Opcode
Byte 1: <slot>         Slot number
Byte 2: <snd>          Function bits F5-F8
Byte 3: <checksum>     Checksum
```

**SND Byte:**
```
Bit 7-4: Reserved (set to 0)
Bit 3: F8
Bit 2: F7
Bit 1: F6
Bit 0: F5
```

**Example:**
```csharp
// Slot 5: F5 ON, F6 ON
byte[] setSnd = { 0xA2, 0x05, 0x03, 0xXX };
```

### OPC_WR_SL_DATA (0xEF) - Write Slot Data

**Purpose:** Write complete slot data (advanced control, programming, fast clock).

**Format:** Same as `OPC_SL_RD_DATA` but with opcode `0xEF`.
```
[0xEF, 0x0E, slot, stat1, adr, spd, dirf, trk, ss2, adr2, snd, id1, id2, chk]
```

**Response:** `OPC_LONG_ACK`

**Example - Change multiple parameters at once:**
```csharp
// Read current slot data first
// Modify desired bytes
// Send OPC_WR_SL_DATA with modified data
```

### Consisting (Linking Slots)

**Purpose:** Run multiple locomotives together as a consist/lash-up.

#### OPC_LINK_SLOTS (0xB9) - Link Two Slots

```
Format: [0xB9, <slot1>, <slot2>, <chk>]
Effect: Slave slot1 to slot2 (slot1 follows slot2)
Response: OPC_SL_RD_DATA or OPC_LONG_ACK (fail)
```

**Consist Structure:**
```
      TOP (Lead locomotive)
       │
       ├─> MID (if more than 2)
       │
       └─> SUB (Last locomotive)
```

#### OPC_UNLINK_SLOTS (0xB8) - Unlink Slots

```
Format: [0xB8, <slot1>, <slot2>, <chk>]
Effect: Unlink slot1 from slot2
Response: OPC_SL_RD_DATA with new status
```

---

## Switch/Turnout Control

### Switch Addressing

Switches/turnouts use an 11-bit address (0-2047) with additional control bits.

**Address Encoding:**
- **Bits A10-A0:** Switch address (0-2047)
- **Bits A1-A0:** Select 1 of 4 pairs in a DS54 (Digitrax stationary decoder)

**Two message bytes encode the address:**

**SW1 Byte:**
```
Bit 7: Always 0
Bit 6-0: Address bits A6-A0
```

**SW2 Byte:**
```
Bit 7: Always 0
Bit 6: Reserved (usually 0)
Bit 5: DIR (1=Closed/GREEN, 0=Thrown/RED)
Bit 4: ON (1=Output ON, 0=Output OFF)
Bit 3-0: Address bits A10-A7
```

### OPC_SW_REQ (0xB0) - Switch Request (No Acknowledge)

**Purpose:** Request switch/turnout change without feedback.

**Format:**
```
Byte 0: 0xB0           Opcode
Byte 1: <sw1>          Address low bits
Byte 2: <sw2>          Address high bits + DIR + ON
Byte 3: <checksum>     Checksum
```

**Example - Throw switch 100:**
```csharp
// Address 100 = 0x64
// A6-A0 = 0x64 & 0x7F = 0x64
// A10-A7 = (0x64 >> 7) = 0
// DIR = 0 (Thrown)
// ON = 1 (Activate output)

byte sw1 = 0x64;              // Low 7 bits of address
byte sw2 = 0x10;              // High 4 bits + DIR=0 + ON=1
byte[] throwSwitch = { 0xB0, sw1, sw2, 0xXX };
```

**Example - Close switch 100:**
```csharp
byte sw1 = 0x64;
byte sw2 = 0x30;              // DIR=1 (Closed), ON=1
byte[] closeSwitch = { 0xB0, sw1, sw2, 0xXX };
```

### OPC_SW_ACK (0xBD) - Switch Request With Acknowledge

**Purpose:** Request switch change with DCS100 acknowledgment (not supported by DT200).

**Format:** Same as `OPC_SW_REQ`
```
[0xBD, <sw1>, <sw2>, <checksum>]
```

**Response:**
- **FIFO full (rejected):** `[0xB4, 0x3D, 0x00, CHK]`
- **Accepted:** `[0xB4, 0x3D, 0x7F, CHK]`

### OPC_SW_REP (0xB1) - Switch Sensor Report

**Purpose:** Broadcast from DS54 reporting turnout feedback or output status.

**Format:**
```
Byte 0: 0xB1           Opcode
Byte 1: <sn1>          Address low bits
Byte 2: <sn2>          Address high bits + status
Byte 3: <checksum>     Checksum
```

**Two interpretations of SN2:**

**Input Feedback (from switch sensors):**
```
Bit 7: Always 0
Bit 6: Always 1
Bit 5: I (0=aux input, 1=switch input for feedback)
Bit 4: L (0=input LOW/0V, 1=input HIGH/>6V)
Bit 3-0: Address A10-A7
```

**Output Status (current state):**
```
Bit 7: Always 0
Bit 6: Always 0
Bit 5: C (1=Closed output ON, 0=OFF)
Bit 4: T (1=Thrown output ON, 0=OFF)
Bit 3-0: Address A10-A7
```

**Example - Parsing switch report:**
```csharp
byte[] switchReport = { 0xB1, 0x64, 0x50, 0xXX };

int address = switchReport[1] | ((switchReport[2] & 0x0F) << 7);  // 100

if ((switchReport[2] & 0x40) != 0)  // Bit 6 = 1
{
    // Input feedback
    bool isSwitchInput = (switchReport[2] & 0x20) != 0;
    bool isHigh = (switchReport[2] & 0x10) != 0;
}
else  // Bit 6 = 0
{
    // Output status
    bool closedOn = (switchReport[2] & 0x20) != 0;
    bool thrownOn = (switchReport[2] & 0x10) != 0;
}
```

### Broadcast Commands

**Stationary Decoder Broadcast:**
```
Switch address with SW2=0x7F, SW1=0x78 triggers broadcast
This is DCC packet: 10111111 1000-Dcba
```

**Interrogate (Query All DS54 States):**
```
Switch address with SW2=0x78, SW1=0x78
Generated by DCS100 at power-up to scan all DS54 inputs
Results in OPC_SW_REP messages from all DS54s
```

---

## Sensor & Feedback

### OPC_INPUT_REP (0xB2) - General Sensor Input

**Purpose:** Report sensor state changes (occupancy detectors, block detectors, etc.).

**Format:**
```
Byte 0: 0xB2           Opcode
Byte 1: <in1>          Address low bits
Byte 2: <in2>          Address high bits + status
Byte 3: <checksum>     Checksum
```

**IN1 Byte:**
```
Bit 7: Always 0
Bit 6-0: Address bits A6-A0
```

**IN2 Byte:**
```
Bit 7: Always 0
Bit 6: X (must be 1, 0 reserved)
Bit 5: I (0=DS54 aux inputs, 1=switch inputs in 4K sensor space)
Bit 4: L (0=input LOW/0V, 1=input HIGH/>6V)
Bit 3-0: Address bits A10-A7
```

**Addressing:**
- Sensors have 11-bit addresses (0-2047, expandable to 4096 with I bit)
- **Bits A1-A0** select 1 of 4 input pairs in DS54

**Example - Sensor 50 goes HIGH:**
```csharp
// Address 50 = 0x32
byte in1 = 0x32;              // A6-A0
byte in2 = 0x50;              // X=1, I=0, L=1 (HIGH), A10-A7=0
byte[] sensorHigh = { 0xB2, in1, in2, 0xXX };
```

**Example - Parsing sensor report:**
```csharp
void ParseSensorReport(byte[] msg)
{
    int address = msg[1] | ((msg[2] & 0x0F) << 7);
    bool isHigh = (msg[2] & 0x10) != 0;
    bool isSwitchInput = (msg[2] & 0x20) != 0;

    Console.WriteLine($"Sensor {address}: {(isHigh ? "HIGH" : "LOW")}");
}
```

**Use Cases:**
- **Block occupancy:** Sensor goes HIGH when train enters, LOW when exits
- **Switch position confirmation:** Feedback from turnout actuators
- **Button presses:** Panel buttons for manual control

---

## Programming Track

### Overview

The programming track is accessed through **Slot 124 (0x7C)**, which is a special shared resource.

**Programming Modes:**
1. **Service Mode** (on programming track):
   - Paged mode (byte read/write)
   - Direct mode (byte read/write, bit read/write)
   - Register mode (byte read/write)

2. **Operations Mode** (on main track - "Programming on Main" or POM):
   - Byte program (with/without feedback)
   - Bit program (with/without feedback)

### Programming Workflow

```
Step 1: Write to Slot 124
  → Send: OPC_WR_SL_DATA with slot 124 and programming command

Step 2: Immediate acknowledge
  ← Receive: OPC_LONG_ACK
     - 0x7F 0x7F = Function not implemented
     - 0x7F 0x00 = Busy, try again later
     - 0x7F 0x01 = Accepted, will send E7 when done
     - 0x7F 0x40 = Accepted, no E7 reply (blind operation)

Step 3: Wait for completion (if LACK was 0x01)
  ← Receive: OPC_SL_RD_DATA (slot 124) with result

Step 4: Check PSTAT byte for success/error flags
```

### OPC_WR_SL_DATA (Slot 124) - Program Command

**Format:**
```
Byte  0: 0xEF               Opcode
Byte  1: 0x0E               Byte count (14)
Byte  2: 0x7C               Slot 124
Byte  3: <pcmd>             Programming command
Byte  4: 0x00               Reserved
Byte  5: <hopsa>            Ops mode: High address byte (0 if service mode)
Byte  6: <lopsa>            Ops mode: Low address byte (0 if service mode)
Byte  7: <trk>              Track status
Byte  8: <cvh>              CV high bits and data bit 7
Byte  9: <cvl>              CV low 7 bits
Byte 10: <data7>            Data bits 0-6
Byte 11: 0x00               Reserved
Byte 12: 0x00               Reserved
Byte 13: <checksum>         Checksum
```

### PCMD Byte - Programming Command

```
Bit 7: Always 0
Bit 6: Write/Read (1=Write, 0=Read)
Bit 5: Byte mode (1=Byte operation, 0=Bit operation if possible)
Bit 4: TY1 (Programming type select)
Bit 3: TY0 (Programming type select)
Bit 2: Ops mode (1=Operations mode/POM, 0=Service mode)
Bit 1-0: Reserved (set to 0)
```

**Type Codes:**

| Byte | Ops | TY1 | TY0 | Mode |
|------|-----|-----|-----|------|
| 1 | 0 | 0 | 0 | Paged mode byte R/W on service track |
| 1 | 0 | 0 | 1 | Direct mode byte R/W on service track |
| 0 | 0 | 0 | 1 | Direct mode bit R/W on service track |
| x | 0 | 1 | 0 | Physical register byte R/W on service track |
| 1 | 1 | 0 | 0 | Ops mode byte, no feedback |
| 1 | 1 | 0 | 1 | Ops mode byte, with feedback |
| 0 | 1 | 0 | 0 | Ops mode bit, no feedback |
| 0 | 1 | 0 | 1 | Ops mode bit, with feedback |

### CVH and CVL Bytes - CV Address

CVs (Configuration Variables) are addressed 1-1024 in DCC:
```
CVH Byte:
  Bit 7: Always 0
  Bit 6-5: Reserved (0)
  Bit 4: CV bit 9
  Bit 3: CV bit 8
  Bit 2-1: Reserved (0)
  Bit 1: Data bit 7 (MSB of data value)
  Bit 0: CV bit 7

CVL Byte:
  Bit 7: Always 0
  Bit 6-0: CV bits 6-0
```

**Encoding CV Number:**
```csharp
int cv = 29;  // Example: CV29
byte cvh = (byte)(((cv - 1) >> 7) & 0x01);      // Bit 0 = CV bit 7
byte cvl = (byte)((cv - 1) & 0x7F);             // Bits 6-0

// For CV 29:
// cvh = 0x00
// cvl = 0x1C  (28 decimal, since CV numbers are 1-indexed but we send 0-indexed)
```

### DATA7 and CVH Bit 1 - Data Value

Data is split across two bytes:
```
CVH bit 1: Data bit 7 (MSB)
DATA7: Data bits 6-0
```

**Encoding Data Value:**
```csharp
byte dataValue = 0xC3;  // Example data
byte cvh |= (byte)((dataValue & 0x80) >> 6);   // Bit 7 goes to CVH bit 1
byte data7 = (byte)(dataValue & 0x7F);          // Bits 6-0
```

### PSTAT Byte - Programming Status (in response)

When programming completes, `OPC_SL_RD_DATA` is sent with slot 124:

```
PSTAT Byte (byte 4 of response):
  Bit 7-4: Reserved
  Bit 3: User aborted (1=aborted)
  Bit 2: Read compare acknowledge fail (1=no read acknowledge)
  Bit 1: Write acknowledge fail (1=no write acknowledge)
  Bit 0: No decoder detected (1=programming track empty)
```

**Success:** All error bits = 0 (PSTAT = 0x00)

### Example - Read CV29 (Service Mode, Direct Byte)

```csharp
// CV29, Direct mode byte read, Service mode
byte pcmd = 0x28;  // Bit 5=1 (byte), Bits 4-3=01 (direct), Bit 2=0 (service)
byte cvh = 0x00;   // CV29-1 = 28, bit 7 = 0
byte cvl = 0x1C;   // CV29-1 = 28 = 0x1C

byte[] readCV29 = {
    0xEF,        // Opcode
    0x0E,        // Count
    0x7C,        // Slot 124
    pcmd,        // Command
    0x00,        // Reserved
    0x00,        // HOPSA (service mode)
    0x00,        // LOPSA (service mode)
    0x03,        // TRK (power on, not programming yet)
    cvh,         // CVH
    cvl,         // CVL
    0x00,        // DATA7 (don't care for read)
    0x00,        // Reserved
    0x00,        // Reserved
    0xXX         // Checksum
};

// Wait for LACK response:
// [0xB4, 0x7F, 0x01, CHK] = Accepted

// Wait for completion:
// [0xE7, 0x0E, 0x7C, <pstat>, 0x00, 0x00, 0x00, <trk>, <cvh>, <cvl>, <data7>, 0, 0, CHK]
// If pstat=0x00, read data from <data7> and bit 7 from <cvh> bit 1
```

### Example - Write CV3 = 10 (Service Mode, Direct Byte)

```csharp
// CV3 = acceleration rate
byte pcmd = 0x68;  // Bit 6=1 (write), Bit 5=1 (byte), Bits 4-3=01 (direct), Bit 2=0 (service)
byte cvh = 0x00;   // CV3-1 = 2, bit 7 = 0
byte cvl = 0x02;   // CV3-1 = 2
byte data7 = 0x0A; // Value 10

byte[] writeCV3 = {
    0xEF, 0x0E, 0x7C,
    pcmd,
    0x00,
    0x00, 0x00,  // Service mode
    0x03,        // TRK
    cvh,
    cvl,
    data7,
    0x00, 0x00,
    0xXX         // Checksum
};

// Wait for LACK [0xB4, 0x7F, 0x01, CHK]
// Wait for E7 response, check pstat for success
```

### Example - Write CV2 = 5 on Address 3 (Ops Mode POM)

```csharp
// Operations mode (on main), write byte, no feedback
byte pcmd = 0x6C;  // Bit 6=1 (write), Bit 5=1 (byte), Bits 4-3=00, Bit 2=1 (ops mode)
byte hopsa = 0x00; // Loco address 3 high byte (short address)
byte lopsa = 0x03; // Loco address 3 low byte
byte cvh = 0x00;   // CV2-1 = 1, bit 7 = 0, data bit 7 = 0
byte cvl = 0x01;   // CV2-1 = 1
byte data7 = 0x05; // Value 5

byte[] pomWriteCV2 = {
    0xEF, 0x0E, 0x7C,
    pcmd,
    0x00,
    hopsa, lopsa,  // Loco address for ops mode
    0x03,          // TRK
    cvh,
    cvl,
    data7,
    0x00, 0x00,
    0xXX           // Checksum
};

// Ops mode write with no feedback = blind write
// LACK response will be [0xB4, 0x7F, 0x40, CHK] (accepted, no E7 reply)
```

### Abort Programming

To cancel a running programming operation:
```csharp
// Send PCMD = 0x00 to abort
byte[] abort = {
    0xEF, 0x0E, 0x7C,
    0x00,  // PCMD = abort
    // ... rest zeros ...
    0xXX   // Checksum
};
```

---

## Fast Clock

### Overview

The system fast clock is stored in **Slot 123 (0x7B)**. All devices maintain their own clock calculation and periodically sync to this slot.

**Design Philosophy:**
- Devices calculate time locally (don't continuously poll)
- One device "pings" slot 123 every 70-100 seconds to resync all devices
- When any device sees a slot 123 read, it resets its local sub-minute counter

### Clock Slot Format

**Reading the clock:** `[0xBB, 0x7B, 0x00, CHK]` (OPC_RQ_SL_DATA slot 123)

**Clock response/set:** `OPC_SL_RD_DATA` or `OPC_WR_SL_DATA` with slot 123:

```
Byte  0: 0xE7/0xEF          Opcode (E7=read, EF=write)
Byte  1: 0x0E               Byte count
Byte  2: 0x7B               Slot 123
Byte  3: <clk_rate>         Clock rate multiplier
Byte  4: <frac_minsl>       Fractional minutes (low byte)
Byte  5: <frac_minsh>       Fractional minutes (high byte)
Byte  6: <256-mins_60>      256 - (minutes mod 60)
Byte  7: <trk>              Track status (same as other slots)
Byte  8: <256-hrs_24>       256 - (hours mod 24)
Byte  9: <days>             Day counter (number of 24hr rollovers)
Byte 10: <clk_cntrl>        Clock control byte
Byte 11: <id1>              Device ID that last set clock (low)
Byte 12: <id2>              Device ID that last set clock (high)
Byte 13: <checksum>         Checksum
```

### Clock Fields

**CLK_RATE (byte 3):**
```
0 = Freeze clock (stopped)
1 = Normal 1:1 real-time
2-127 = Fast clock rate (2:1 up to 127:1)

Example: clk_rate = 10 means clock runs 10x real-time
```

**FRAC_MINS (bytes 4-5):**
```
Internal sub-minute counter (generator-dependent)
Reset when valid sync message (E7 slot 123 with valid flag) is seen
User code typically doesn't need to set this
```

**MINS (byte 6):**
```
Minutes: 256 - (current_minute % 60)

Example: Current time is 14:35
  byte 6 = 256 - 35 = 221 = 0xDD
```

**HRS (byte 8):**
```
Hours: 256 - (current_hour % 24)

Example: Current time is 14:35
  byte 8 = 256 - 14 = 242 = 0xF2
```

**DAYS (byte 9):**
```
Number of times clock has rolled over 24 hours
Positive counter, starts at 0
```

**CLK_CNTRL (byte 10):**
```
Bit 7: Reserved
Bit 6: Valid (1=This is valid clock data, 0=Ignore this sync message)
Bit 5-0: Reserved
```

**ID1/ID2 (bytes 11-12):**
```
Device ID of last device to set the clock
0x00 0x00 = Never set
0x7F 0x7x = Reserved for PC access
```

### Example - Read Current Time

```csharp
// Request clock
byte[] readClock = { 0xBB, 0x7B, 0x00, 0xXX };
// Wait for response...

void ParseClock(byte[] msg)
{
    if (msg[0] != 0xE7 || msg[2] != 0x7B) return;

    byte clkRate = msg[3];
    byte mins = (byte)(256 - msg[6]);  // Decode minutes
    byte hrs = (byte)(256 - msg[8]);   // Decode hours
    byte days = msg[9];
    bool valid = (msg[10] & 0x40) != 0;

    if (!valid)
    {
        Console.WriteLine("Clock sync ignored (invalid flag)");
        return;
    }

    Console.WriteLine($"Fast Clock: {hrs:D2}:{mins:D2} (Day {days})");
    Console.WriteLine($"Rate: {clkRate}:1");

    // Reset local sub-minute counter here
}
```

### Example - Set Clock to 08:30, Rate 10:1

```csharp
byte clkRate = 10;               // 10:1 fast clock
byte mins = (byte)(256 - 30);    // 30 minutes: 256-30 = 226
byte hrs = (byte)(256 - 8);      // 8 hours: 256-8 = 248
byte days = 0;                   // Day 0
byte clkCntrl = 0x40;            // Valid flag set

byte[] setClock = {
    0xEF,        // Write slot data
    0x0E,        // Count
    0x7B,        // Slot 123
    clkRate,     // Rate
    0x00,        // Frac mins low
    0x00,        // Frac mins high
    mins,        // Minutes (256-30)
    0x03,        // TRK (power on)
    hrs,         // Hours (256-8)
    days,        // Days
    clkCntrl,    // Valid flag
    0x7F,        // ID1 (PC)
    0x70,        // ID2 (PC)
    0xXX         // Checksum
};

// All devices on network will sync to this time
```

### Local Clock Calculation

**Best Practice:** Don't poll the clock slot continuously!

```csharp
class FastClock
{
    private int hours;
    private int minutes;
    private double seconds;
    private int rate;
    private DateTime lastSync;

    public void Sync(byte[] slotData)
    {
        // Parse from OPC_SL_RD_DATA slot 123
        rate = slotData[3];
        minutes = 256 - slotData[6];
        hours = 256 - slotData[8];
        seconds = 0;  // Reset sub-minute
        lastSync = DateTime.Now;
    }

    public string GetCurrentTime()
    {
        if (rate == 0) return $"{hours:D2}:{minutes:D2}";  // Frozen

        // Calculate elapsed time since sync
        double elapsed = (DateTime.Now - lastSync).TotalSeconds;
        double fastElapsed = elapsed * rate;  // Apply rate multiplier

        // Add to synced time
        int totalSeconds = (int)(minutes * 60 + seconds + fastElapsed);
        int currentMinutes = (totalSeconds / 60) % 60;
        int currentHours = (hours + (totalSeconds / 3600)) % 24;
        int currentSeconds = totalSeconds % 60;

        return $"{currentHours:D2}:{currentMinutes:D2}:{currentSeconds:D2}";
    }

    public bool ShouldResync()
    {
        // Ping every 70-100 seconds to keep all devices in sync
        return (DateTime.Now - lastSync).TotalSeconds > 80;
    }
}
```

---

## Common Workflows & Examples

### Complete Example: Control a Locomotive

```csharp
public class LocoNetController
{
    // 1. Request locomotive address
    public byte[] RequestLocoAddress(int address)
    {
        byte adrHi = (address > 127) ? (byte)(address >> 8) : (byte)0x00;
        byte adrLo = (byte)(address & 0x7F);

        byte[] msg = { 0xBF, adrHi, adrLo, 0x00 };
        msg[3] = CalculateChecksum(msg);
        return msg;
    }

    // 2. Parse slot read response
    public SlotData ParseSlotRead(byte[] msg)
    {
        if (msg[0] != 0xE7 || msg[1] != 0x0E)
            throw new Exception("Not a slot read message");

        var slot = new SlotData
        {
            SlotNumber = msg[2],
            Status = msg[3],
            Address = msg[4] | (msg[9] << 7),
            Speed = msg[5],
            Direction = (msg[6] & 0x20) != 0,
            F0 = (msg[6] & 0x10) != 0,
            F1 = (msg[6] & 0x01) != 0,
            F2 = (msg[6] & 0x02) != 0,
            F3 = (msg[6] & 0x04) != 0,
            F4 = (msg[6] & 0x08) != 0,
            F5 = (msg[10] & 0x01) != 0,
            F6 = (msg[10] & 0x02) != 0,
            F7 = (msg[10] & 0x04) != 0,
            F8 = (msg[10] & 0x08) != 0
        };

        return slot;
    }

    // 3. Check if slot needs activation
    public bool SlotNeedsActivation(byte status1)
    {
        byte busyActive = (byte)((status1 >> 4) & 0x03);
        // 00=FREE, 01=COMMON, 10=IDLE, 11=IN_USE
        return busyActive != 0x03;  // Not IN_USE
    }

    // 4. Activate slot (NULL move)
    public byte[] ActivateSlot(int slotNum)
    {
        byte[] msg = { 0xBA, (byte)slotNum, (byte)slotNum, 0x00 };
        msg[3] = CalculateChecksum(msg);
        return msg;
    }

    // 5. Set speed
    public byte[] SetSpeed(int slotNum, int speed)
    {
        byte[] msg = { 0xA0, (byte)slotNum, (byte)speed, 0x00 };
        msg[3] = CalculateChecksum(msg);
        return msg;
    }

    // 6. Set direction and functions
    public byte[] SetDirF(int slotNum, bool forward, bool f0, bool f1, bool f2, bool f3, bool f4)
    {
        byte dirf = 0x00;
        if (forward) dirf |= 0x20;
        if (f0) dirf |= 0x10;
        if (f4) dirf |= 0x08;
        if (f3) dirf |= 0x04;
        if (f2) dirf |= 0x02;
        if (f1) dirf |= 0x01;

        byte[] msg = { 0xA1, (byte)slotNum, dirf, 0x00 };
        msg[3] = CalculateChecksum(msg);
        return msg;
    }

    // Checksum helper
    private byte CalculateChecksum(byte[] msg)
    {
        byte xor = 0;
        for (int i = 0; i < msg.Length - 1; i++)
            xor ^= msg[i];
        return (byte)(~xor & 0x7F);
    }
}

// Usage workflow:
async Task ControlLocomotive()
{
    var controller = new LocoNetController();

    // Step 1: Request address 3
    Send(controller.RequestLocoAddress(3));
    var response = await ReceiveMessage();

    // Step 2: Parse response
    if (response[0] == 0xE7)  // Slot read
    {
        var slot = controller.ParseSlotRead(response);
        Console.WriteLine($"Got slot {slot.SlotNumber} for address {slot.Address}");

        // Step 3: Activate if needed
        if (controller.SlotNeedsActivation(slot.Status))
        {
            Send(controller.ActivateSlot(slot.SlotNumber));
            await ReceiveMessage();  // Wait for slot read response
        }

        // Step 4: Control the loco!
        Send(controller.SetSpeed(slot.SlotNumber, 64));      // Half speed
        Send(controller.SetDirF(slot.SlotNumber, true, true, false, false, false, false));  // Forward, F0 on
    }
}
```

### Example: Monitor Sensors

```csharp
public class SensorMonitor
{
    public Dictionary<int, bool> SensorStates = new Dictionary<int, bool>();

    public void ProcessMessage(byte[] msg)
    {
        if (msg[0] == 0xB2)  // OPC_INPUT_REP
        {
            int address = msg[1] | ((msg[2] & 0x0F) << 7);
            bool isHigh = (msg[2] & 0x10) != 0;

            SensorStates[address] = isHigh;
            OnSensorChanged(address, isHigh);
        }
    }

    public event Action<int, bool> SensorChanged;

    private void OnSensorChanged(int address, bool isHigh)
    {
        Console.WriteLine($"Sensor {address}: {(isHigh ? "OCCUPIED" : "CLEAR")}");
        SensorChanged?.Invoke(address, isHigh);
    }
}

// Usage:
var monitor = new SensorMonitor();
monitor.SensorChanged += (addr, high) =>
{
    if (high)
        Console.WriteLine($"Block {addr} is now occupied!");
};

// In message loop:
while (true)
{
    byte[] msg = await ReceiveMessage();
    monitor.ProcessMessage(msg);
}
```

### Example: Throw Switches

```csharp
public class SwitchController
{
    public byte[] ThrowSwitch(int address)
    {
        return BuildSwitchCommand(address, thrown: true, on: true);
    }

    public byte[] CloseSwitch(int address)
    {
        return BuildSwitchCommand(address, thrown: false, on: true);
    }

    public byte[] TurnOffSwitch(int address)
    {
        return BuildSwitchCommand(address, thrown: false, on: false);
    }

    private byte[] BuildSwitchCommand(int address, bool thrown, bool on)
    {
        byte sw1 = (byte)(address & 0x7F);           // A6-A0
        byte sw2 = (byte)((address >> 7) & 0x0F);    // A10-A7

        if (!thrown) sw2 |= 0x20;  // DIR bit (1=Closed)
        if (on) sw2 |= 0x10;       // ON bit

        byte[] msg = { 0xB0, sw1, sw2, 0x00 };
        msg[3] = CalculateChecksum(msg);
        return msg;
    }

    private byte CalculateChecksum(byte[] msg)
    {
        byte xor = 0;
        for (int i = 0; i < msg.Length - 1; i++)
            xor ^= msg[i];
        return (byte)(~xor & 0x7F);
    }
}

// Usage:
var switches = new SwitchController();

// Throw turnout 100
Send(switches.ThrowSwitch(100));
await Task.Delay(1000);  // Wait for turnout motor

// Turn off power to avoid overheating
Send(switches.TurnOffSwitch(100));
```

### Example: Complete Message Parser

```csharp
public class LocoNetMessageParser
{
    public void ParseMessage(byte[] msg)
    {
        if (msg.Length < 2) return;

        byte opcode = msg[0];

        switch (opcode)
        {
            case 0x82:
                Console.WriteLine("POWER OFF");
                break;

            case 0x83:
                Console.WriteLine("POWER ON");
                break;

            case 0x85:
                Console.WriteLine("EMERGENCY STOP");
                break;

            case 0x81:
                // Ignore BUSY opcode
                break;

            case 0xB2:  // Sensor report
                ParseSensorReport(msg);
                break;

            case 0xB1:  // Switch report
                ParseSwitchReport(msg);
                break;

            case 0xE7:  // Slot read
                ParseSlotRead(msg);
                break;

            case 0xB4:  // Long ACK
                ParseLongAck(msg);
                break;

            case 0xA0:  // Speed command
                Console.WriteLine($"Speed: Slot {msg[1]}, Speed {msg[2]}");
                break;

            case 0xA1:  // DIRF command
                ParseDirf(msg);
                break;

            default:
                Console.WriteLine($"Unknown opcode: 0x{opcode:X2}");
                break;
        }
    }

    private void ParseSensorReport(byte[] msg)
    {
        int addr = msg[1] | ((msg[2] & 0x0F) << 7);
        bool high = (msg[2] & 0x10) != 0;
        Console.WriteLine($"Sensor {addr}: {(high ? "HIGH" : "LOW")}");
    }

    private void ParseSwitchReport(byte[] msg)
    {
        int addr = msg[1] | ((msg[2] & 0x0F) << 7);

        if ((msg[2] & 0x40) != 0)  // Input feedback
        {
            bool isSwitchInput = (msg[2] & 0x20) != 0;
            bool isHigh = (msg[2] & 0x10) != 0;
            Console.WriteLine($"Switch {addr} Input: {(isHigh ? "HIGH" : "LOW")}");
        }
        else  // Output status
        {
            bool closed = (msg[2] & 0x20) != 0;
            bool thrown = (msg[2] & 0x10) != 0;
            Console.WriteLine($"Switch {addr} Output: C={closed}, T={thrown}");
        }
    }

    private void ParseSlotRead(byte[] msg)
    {
        if (msg[1] != 0x0E) return;

        int slot = msg[2];
        int addr = msg[4] | (msg[9] << 7);
        int speed = msg[5];
        bool fwd = (msg[6] & 0x20) != 0;

        Console.WriteLine($"Slot {slot}: Addr {addr}, Speed {speed}, Dir {(fwd ? "FWD" : "REV")}");
    }

    private void ParseLongAck(byte[] msg)
    {
        byte lopc = msg[1];
        byte ack1 = msg[2];

        string result = ack1 switch
        {
            0x00 => "FAIL",
            0x01 => "ACCEPTED (will reply)",
            0x40 => "ACCEPTED (blind, no reply)",
            0x7F => "ACCEPTED",
            _ => $"Code {ack1:X2}"
        };

        Console.WriteLine($"Long ACK for 0x{lopc:X2}: {result}");
    }

    private void ParseDirf(byte[] msg)
    {
        int slot = msg[1];
        bool fwd = (msg[2] & 0x20) != 0;
        bool f0 = (msg[2] & 0x10) != 0;
        bool f1 = (msg[2] & 0x01) != 0;

        Console.WriteLine($"Slot {slot}: {(fwd ? "FWD" : "REV")}, F0={f0}, F1={f1}");
    }
}
```

### Slot Purge and Keep-Alive

**Important:** Slots are automatically purged if not accessed within ~200 seconds!

```csharp
public class SlotKeepAlive
{
    private Dictionary<int, DateTime> slotLastAccess = new Dictionary<int, DateTime>();

    public void MonitorSlot(int slot)
    {
        slotLastAccess[slot] = DateTime.Now;
    }

    public List<int> GetSlotsNeedingPing()
    {
        var needPing = new List<int>();
        var now = DateTime.Now;

        foreach (var kvp in slotLastAccess)
        {
            // Ping every 100 seconds (purge timeout is ~200 seconds)
            if ((now - kvp.Value).TotalSeconds > 100)
            {
                needPing.Add(kvp.Key);
            }
        }

        return needPing;
    }

    public byte[] PingSlot(int slot)
    {
        // Send current speed again (no change)
        // This resets the purge timer
        return new LocoNetController().SetSpeed(slot, lastSpeed[slot]);
    }
}

// Background task:
async Task KeepAliveTask()
{
    var keepAlive = new SlotKeepAlive();

    while (true)
    {
        await Task.Delay(10000);  // Check every 10 seconds

        foreach (int slot in keepAlive.GetSlotsNeedingPing())
        {
            Send(keepAlive.PingSlot(slot));
            keepAlive.MonitorSlot(slot);
        }
    }
}
```

---

## OPC_LONG_ACK (0xB4) - Long Acknowledgment

**Purpose:** Response message indicating success/failure of a command.

**Format:**
```
Byte 0: 0xB4           Opcode
Byte 1: <lopc>         Opcode being acknowledged (with MSB cleared)
Byte 2: <ack1>         Acknowledgment code
Byte 3: <checksum>     Checksum
```

**Common ACK1 Codes:**

| ACK1 | Meaning |
|------|---------|
| 0x00 | FAIL (command rejected or failed) |
| 0x01 | ACCEPTED - will send follow-on response |
| 0x7F | ACCEPTED - command completed successfully |
| 0x40 | ACCEPTED - blind operation, no follow-on response |

**Opcode-Specific Codes:**

| LOPC | ACK1 | Meaning |
|------|------|---------|
| 0x3F (BF) | 0x00 | Loco address request failed - no free slots |
| 0x3A (BA) | 0x00 | Move slots failed - illegal move |
| 0x39 (B9) | 0x00 | Link slots failed - invalid link |
| 0x3D (BD) | 0x00 | Switch ACK - DCS100 FIFO full, rejected |
| 0x3D (BD) | 0x7F | Switch ACK - DCS100 accepted |
| 0x30 (B0) | 0x00 | Switch request failed immediately |
| 0x7F (Programming) | 0x7F | Function not implemented |
| 0x7F (Programming) | 0x00 | Programmer busy |
| 0x7F (Programming) | 0x01 | Accepted, will send E7 when done |
| 0x7F (Programming) | 0x40 | Accepted blind, no E7 reply |

---

## Additional Opcodes

### OPC_RQ_SL_DATA (0xBB) - Request Slot Data

**Purpose:** Explicitly request the current data for a specific slot.

**Format:**
```
Byte 0: 0xBB           Opcode
Byte 1: <slot>         Slot number (0-127)
Byte 2: 0x00           Reserved
Byte 3: <checksum>     Checksum
```

**Response:** `OPC_SL_RD_DATA` (0xE7) with slot data

**Example:**
```csharp
// Request data for slot 10
byte[] reqSlot = { 0xBB, 0x0A, 0x00, 0xXX };
```

### OPC_SLOT_STAT1 (0xB5) - Write Slot Status1

**Purpose:** Change only the STATUS1 byte of a slot (faster than writing entire slot).

**Format:**
```
Byte 0: 0xB5           Opcode
Byte 1: <slot>         Slot number
Byte 2: <stat1>        New STATUS1 value
Byte 3: <checksum>     Checksum
```

**Use Cases:**
- Mark slot as COMMON vs IN_USE
- Change decoder type
- Modify consist flags

### OPC_CONSIST_FUNC (0xB6) - Consist Function Control

**Purpose:** Set functions on an uplinked consist member without affecting the consist top.

**Format:**
```
Byte 0: 0xB6           Opcode
Byte 1: <slot>         Slot number (in consist, uplinked)
Byte 2: <dirf>         Direction and F0-F4 bits
Byte 3: <checksum>     Checksum
```

**Note:** Only affects the specific locomotive, not the whole consist.

### OPC_IMM_PACKET (0xED) - Immediate Packet

**Purpose:** Send a raw DCC packet immediately (not refreshed). For advanced users.

**Format:**
```
Byte 0: 0xED              Opcode
Byte 1: 0x0B              Byte count (11)
Byte 2: 0x7F              Reserved
Byte 3: <reps>            Repeat count and IM byte count
Byte 4: <dhi>             High bits of IM bytes
Byte 5: <im1>             Immediate byte 1
Byte 6: <im2>             Immediate byte 2
Byte 7: <im3>             Immediate byte 3
Byte 8: <im4>             Immediate byte 4
Byte 9: <im5>             Immediate byte 5
Byte 10: <checksum>       Checksum
```

**REPS Byte:**
```
Bits 6-4: Number of IM bytes (1-5)
Bit 3: Reserved (0)
Bits 2-0: Repeat count (0-7)
```

**DHI Byte:** High bits (bit 7) of each IM byte
```
Bit 6-5: Reserved (11 for this format)
Bit 4: IM5 bit 7
Bit 3: IM4 bit 7
Bit 2: IM3 bit 7
Bit 1: IM2 bit 7
Bit 0: IM1 bit 7
```

**Response:** `OPC_LONG_ACK`

**Note:** Not implemented on DT200. Requires DCS100 or similar.

---

## Protocol Summary for Implementation

### Essential Message Flow Patterns

**1. Startup Sequence:**
```
Power on
  → Wait 250ms (startup backoff)
  → Listen for traffic
  → Optionally send OPC_RQ_SL_DATA(0) to get Master config
```

**2. Control a Loco:**
```
OPC_LOCO_ADR (request address)
  → Receive OPC_SL_RD_DATA (slot info)
  → If needed: OPC_MOVE_SLOTS (NULL move to activate)
    → Receive OPC_SL_RD_DATA (confirmation)
  → OPC_LOCO_SPD (set speed)
  → OPC_LOCO_DIRF (set direction/functions)
  → OPC_LOCO_SND (set sound functions)
  → Repeat speed/dirf/snd as needed
  → Every 100s: Send any command to prevent purge
```

**3. Throw a Switch:**
```
OPC_SW_REQ (throw switch)
  → Wait ~1 second for motor
  → OPC_SW_REQ (turn off output)
```

**4. Monitor Sensors:**
```
Listen for OPC_INPUT_REP messages
  → Parse sensor address and state
  → React to changes
```

**5. Program a Decoder:**
```
OPC_WR_SL_DATA (slot 124 with prog command)
  → Receive OPC_LONG_ACK (accepted or rejected)
  → If accepted (0x01): Wait for completion
    → Receive OPC_SL_RD_DATA (slot 124 with result)
    → Check PSTAT for success
```

**6. Monitor Fast Clock:**
```
Every 80 seconds: OPC_RQ_SL_DATA(123) to resync
  → Receive OPC_SL_RD_DATA (slot 123)
  → Update local clock calculation
```

### Bit Manipulation Helpers

```csharp
public static class LocoNetHelpers
{
    // Extract address from SW1/SW2
    public static int GetSwitchAddress(byte sw1, byte sw2)
    {
        return sw1 | ((sw2 & 0x0F) << 7);
    }

    // Build SW1/SW2 from address
    public static (byte sw1, byte sw2) EncodeSwitchAddress(int address, bool closed, bool on)
    {
        byte sw1 = (byte)(address & 0x7F);
        byte sw2 = (byte)((address >> 7) & 0x0F);
        if (closed) sw2 |= 0x20;
        if (on) sw2 |= 0x10;
        return (sw1, sw2);
    }

    // Extract address from IN1/IN2
    public static int GetSensorAddress(byte in1, byte in2)
    {
        return in1 | ((in2 & 0x0F) << 7);
    }

    // Get message length from opcode
    public static int GetMessageLength(byte opcode)
    {
        if ((opcode & 0x80) == 0) return 0;  // Not an opcode

        int lengthBits = (opcode >> 5) & 0x03;
        return lengthBits switch
        {
            0 => 2,
            1 => 4,
            2 => 6,
            3 => -1,  // Variable, read next byte
            _ => 0
        };
    }

    // Check if opcode expects response
    public static bool ExpectsResponse(byte opcode)
    {
        return (opcode & 0x08) != 0;
    }

    // Decode locomotive address from slot data
    public static int GetLocoAddress(byte adrLo, byte adrHi)
    {
        if (adrHi == 0) return adrLo;  // Short address

        // Long address (14-bit)
        // Combine according to NMRA DCC addressing
        return (adrHi << 7) | adrLo;
    }

    // Encode locomotive address for request
    public static (byte adrHi, byte adrLo) EncodeLocoAddress(int address)
    {
        if (address <= 127)
            return (0x00, (byte)address);
        else
            return ((byte)(address >> 7), (byte)(address & 0x7F));
    }
}
```

---

## Troubleshooting Guide

### Common Issues

**1. No response to commands:**
- Check physical connection (pins 2-5 for data)
- Verify baud rate (16.666 KBaud or 16.457 KBaud for PC)
- Ensure proper voltage levels (+12V idle)
- Check checksum calculation

**2. Commands are rejected (LACK fail):**
- Check if Master is busy (programming track active?)
- Verify slot is in correct state (IN_USE for commands)
- Check if consist needs unlinking
- Ensure slot hasn't been purged (send keep-alive)

**3. Collision detection issues:**
- Don't seize network too quickly
- Respect CD backoff (1.2ms) and Master delay (360µs)
- Monitor transmit echo properly
- On collision, send 15-bit BREAK immediately

**4. Programming fails:**
- Check programming track is not busy (TRK bit 3)
- Verify decoder is on programming track (PSTAT bit 0)
- For Ops Mode, ensure locomotive address is correct
- Some decoders don't support all programming modes

**5. Clock drifts:**
- Reset local counter when seeing OPC_SL_RD_DATA slot 123
- Don't continuously poll - calculate locally
- Resync every 70-100 seconds with one "ping"

**6. Sensors not reporting:**
- Check sensor address calculation
- Verify X bit is set (bit 6 of IN2)
- Check wiring (sensors need power)
- DS54 may need configuration

---

## Quick Reference: Message Templates

### Power Control
```csharp
byte[] powerOn =  { 0x83, 0x7C };
byte[] powerOff = { 0x82, 0x7D };
byte[] eStop =    { 0x85, 0x7A };
```

### Locomotive Control
```csharp
// Request address 3
byte[] reqAddr3 = { 0xBF, 0x00, 0x03, 0x43 };

// Request slot 5 data
byte[] reqSlot5 = { 0xBB, 0x05, 0x00, 0xXX };

// Activate slot 5
byte[] activateSlot5 = { 0xBA, 0x05, 0x05, 0xXX };

// Slot 5: Speed 64
byte[] speed64 = { 0xA0, 0x05, 0x40, 0xXX };

// Slot 5: Forward, F0 ON
byte[] fwdF0 = { 0xA1, 0x05, 0x30, 0xXX };

// Slot 5: Emergency stop
byte[] eStopSlot5 = { 0xA0, 0x05, 0x01, 0xXX };
```

### Switch Control
```csharp
// Throw switch 1 (closed=0, on=1)
byte[] throwSw1 = { 0xB0, 0x01, 0x10, 0xXX };

// Close switch 1 (closed=1, on=1)
byte[] closeSw1 = { 0xB0, 0x01, 0x30, 0xXX };

// Turn off switch 1 output
byte[] offSw1 = { 0xB0, 0x01, 0x00, 0xXX };
```

### Programming
```csharp
// Read CV29, service mode, direct byte
byte[] readCV29 = {
    0xEF, 0x0E, 0x7C, 0x28, 0x00, 0x00, 0x00, 0x03,
    0x00, 0x1C, 0x00, 0x00, 0x00, 0xXX
};

// Write CV3=10, service mode, direct byte
byte[] writeCV3 = {
    0xEF, 0x0E, 0x7C, 0x68, 0x00, 0x00, 0x00, 0x03,
    0x00, 0x02, 0x0A, 0x00, 0x00, 0xXX
};
```

### Fast Clock
```csharp
// Request clock
byte[] reqClock = { 0xBB, 0x7B, 0x00, 0xXX };

// Set clock 08:30, rate 10:1
byte[] setClock = {
    0xEF, 0x0E, 0x7B, 0x0A, 0x00, 0x00, 0xE2, 0x03,
    0xF8, 0x00, 0x40, 0x7F, 0x70, 0xXX
};
```

---

## Implementation Checklist

### Phase 1: Basic Setup
- [ ] Serial port connection (16.457 KBaud, 8N1)
- [ ] Message framing (detect opcode, calculate length)
- [ ] Checksum validation
- [ ] Parse basic messages (power, speed, direction)

### Phase 2: Locomotive Control
- [ ] Request locomotive address (OPC_LOCO_ADR)
- [ ] Parse slot data (OPC_SL_RD_DATA)
- [ ] Activate slot (OPC_MOVE_SLOTS NULL move)
- [ ] Set speed (OPC_LOCO_SPD)
- [ ] Set direction/functions (OPC_LOCO_DIRF, OPC_LOCO_SND)
- [ ] Slot keep-alive mechanism

### Phase 3: Feedback
- [ ] Monitor sensor inputs (OPC_INPUT_REP)
- [ ] Monitor switch reports (OPC_SW_REP)
- [ ] Parse and expose sensor states

### Phase 4: Switch Control
- [ ] Throw/close switches (OPC_SW_REQ)
- [ ] Handle switch output timing (on/off cycle)
- [ ] Parse switch feedback

### Phase 5: Programming
- [ ] Service mode CV read/write (slot 124)
- [ ] Operations mode POM (slot 124)
- [ ] Handle programming responses (LACK, slot read)
- [ ] Parse PSTAT error codes

### Phase 6: Advanced Features
- [ ] Fast clock (slot 123 read/write/sync)
- [ ] Consisting (OPC_LINK_SLOTS, OPC_UNLINK_SLOTS)
- [ ] Dispatching (OPC_MOVE_SLOTS special cases)
- [ ] Full slot management

### Phase 7: Network Management
- [ ] CSMA/CD timing (optional for PC interface)
- [ ] Collision handling
- [ ] Message prioritization
- [ ] Multiple PC coordination

---

## Glossary

**Consist:** Multiple locomotives linked together to operate as one unit (lash-up)

**CSMA/CD:** Carrier Sense Multiple Access with Collision Detection - network arbitration method

**CV:** Configuration Variable - DCC decoder parameter (address, speed curve, etc.)

**DCC:** Digital Command Control - NMRA standard for digital model railroad control

**DCS100:** Digitrax Command Station (also called "Chief") - implements Master functions

**Dispatcher:** System mode for simplified throttle operation (dispatch put/get)

**DS54:** Digitrax Stationary Decoder for switches/turnouts (4 outputs)

**Master:** Device maintaining DCC refresh stack (typically command station)

**Ops Mode:** Operations Mode - programming decoders on the main track (POM)

**Opcode:** Operation Code - first byte of message identifying command type

**Purge:** Automatic freeing of unused slots after timeout (~200 seconds)

**Refresh Stack:** Master's list of active locomotives being continuously sent DCC packets

**Service Mode:** Programming on dedicated programming track (isolated from main)

**Slot:** Master's internal record for a locomotive (like a file handle)

**Stationary Decoder:** Decoder for accessories (switches, lights) not locomotives

**Throttle:** Handheld controller for operating locomotives

---

## Conclusion

This specification covers LocoNet Personal Use Edition 1.0, a robust peer-to-peer network for model railroad control. The protocol balances simplicity with power, allowing:

- **Distributed control** without central bottlenecks
- **Hot-plugging** of devices without configuration
- **Multi-user** operation with automatic arbitration
- **Extensibility** through peer-to-peer messages

**Key Takeaways for Implementation:**

1. **Messages are self-describing** (opcode encodes length and type)
2. **Checksum everything** (XOR with 1's complement)
3. **Slots are the key abstraction** for locomotive control
4. **Respect timing** (CD backoff, Master delay, priority)
5. **Keep slots alive** (ping every 100 seconds)
6. **All devices see all messages** (design for monitoring)
7. **No polling required** (event-driven architecture)

**For C# Implementation:**
- Use async/await for message handling
- Implement state machines for request/response patterns
- Cache slot data to minimize network traffic
- Handle reconnection gracefully (startup backoff)
- Monitor all messages even if not directly relevant (situational awareness)

Good luck with your LocoNet implementation! 🚂
