# Z21 LAN Protocol - Basics

**Document Version:** 1.13 (English)
**Date:** November 6, 2023

## 1.1 Communication

### Protocol Overview

Communication with the Z21 command station uses **UDP** (User Datagram Protocol) on ports:
- **Port 21105**: Primary port for control applications (PC, mobile apps, etc.)
- **Port 21106**: Alternative port

### Asynchronous Communication

Communication is **always asynchronous**, meaning broadcast messages can occur between a request and its response. Clients must be prepared to handle out-of-order messages.

#### Example Communication Sequence

```
Client1                    Z21                    Client2
   |                        |                        |
   |--Request serial number-|                        |
   |                        |------Loco #3 F0 on-----|
   |<---System status-------|                        |
   |  (broadcast)           |----System status------>|
   |                        |    (broadcast)         |
   |<-Info Locomotive #3----|                        |
   |  (broadcast)           |--Info Locomotive #3--->|
   |                        |    (broadcast)         |
   |<---Serial number-------|                        |
   |                        |                        |
```

### Client Keep-Alive Requirements

- **Minimum Activity**: Each client must communicate with the Z21 **at least once per minute**
- **Timeout**: Clients that don't communicate within this interval are removed from the active participants list
- **Logoff**: Clients should properly disconnect using the `LAN_LOGOFF` command when possible

```csharp
// Example: Client should send periodic keep-alive messages
const int KeepAliveIntervalMs = 50000; // 50 seconds (well under 60-second timeout)

// Send any command to maintain connection, e.g., get system status
```

## 1.2 Z21 Dataset

### 1.2.1 Structure

Every Z21 data record (request or response) follows this structure:

| Field | Size | Description |
|-------|------|-------------|
| **DataLen** | 2 bytes (little-endian) | Total length of entire dataset including DataLen, Header, and Data |
| **Header** | 2 bytes (little-endian) | Command identifier and protocol group |
| **Data** | n bytes | Command-specific data (variable length) |

**Formula**: `DataLen = 2 + 2 + n` (where n = number of data bytes)

#### Byte Order

Unless otherwise specified, all multi-byte values use **little-endian** byte order:
- Low byte first, then high byte
- Example: `0x1234` is transmitted as `[0x34, 0x12]`

#### C# Representation

```csharp
public struct Z21Packet
{
    public ushort DataLen { get; set; }  // Total length (2 + 2 + Data.Length)
    public ushort Header { get; set; }   // Command identifier
    public byte[] Data { get; set; }     // Variable-length data
}

// Example: Creating a packet
var packet = new Z21Packet
{
    Header = 0x0040,  // LAN_X_xxx command
    Data = [0x21, 0x24, 0x05]
};
packet.DataLen = (ushort)(4 + packet.Data.Length); // 2 + 2 + 3 = 7

// Serialize to bytes (little-endian)
byte[] Serialize(Z21Packet packet)
{
    var buffer = new byte[packet.DataLen];
    buffer[0] = (byte)(packet.DataLen & 0xFF);         // DataLen low byte
    buffer[1] = (byte)((packet.DataLen >> 8) & 0xFF);  // DataLen high byte
    buffer[2] = (byte)(packet.Header & 0xFF);          // Header low byte
    buffer[3] = (byte)((packet.Header >> 8) & 0xFF);   // Header high byte
    Array.Copy(packet.Data, 0, buffer, 4, packet.Data.Length);
    return buffer;
}
```

### 1.2.2 X-BUS Protocol Tunneling

X-BUS protocol commands are encapsulated within Z21 LAN packets using header **0x40** (`LAN_X_xxx`).

**Important**: This refers to the X-BUS protocol only, not the physical X-BUS connector on the Z21. These commands are exclusively for LAN communication.

#### Structure

The X-BUS command is placed in the Data field, with the last byte being an **XOR checksum**:

| DataLen | Header | Data |||||
|---------|--------|------|-----|-----|----------|
|         |        | **X-Header** | **DB0** | **DB1** | **XOR-Byte** |

**XOR Checksum Calculation**: `XOR = X-Header ⊕ DB0 ⊕ DB1 ⊕ ... ⊕ DBn`

#### Example

| Field | Value | Description |
|-------|-------|-------------|
| DataLen | 0x0008 | 8 bytes total |
| Header | 0x0040 | X-BUS tunnel command |
| X-Header | h | X-BUS command header |
| DB0 | x | First data byte |
| DB1 | y | Second data byte |
| XOR-Byte | h ⊕ x ⊕ y | Checksum |

#### C# Implementation

```csharp
public struct XBusCommand
{
    public byte XHeader { get; set; }
    public byte[] DataBytes { get; set; }

    public byte CalculateChecksum()
    {
        byte xor = XHeader;
        foreach (byte b in DataBytes)
        {
            xor ^= b;
        }
        return xor;
    }

    public byte[] ToZ21Data()
    {
        var data = new byte[DataBytes.Length + 2];
        data[0] = XHeader;
        Array.Copy(DataBytes, 0, data, 1, DataBytes.Length);
        data[^1] = CalculateChecksum(); // Last byte is checksum
        return data;
    }
}

// Example usage
var xbusCmd = new XBusCommand
{
    XHeader = 0x21,
    DataBytes = [0x24, 0x05]
};

var z21Packet = new Z21Packet
{
    Header = 0x0040,
    Data = xbusCmd.ToZ21Data() // [0x21, 0x24, 0x05, 0x00] where 0x00 = 0x21 ^ 0x24 ^ 0x05
};
z21Packet.DataLen = (ushort)(4 + z21Packet.Data.Length);
```

### 1.2.3 LocoNet Tunneling

**Available from Z21 Firmware Version 1.20+**

LocoNet messages can be tunneled through the Z21 LAN interface, allowing the Z21 to function as an Ethernet/LocoNet gateway.

#### LocoNet Headers

| Header | Direction | Description |
|--------|-----------|-------------|
| **0xA0** | `LAN_LOCONET_Z21_RX` | Messages received by Z21 from LocoNet bus → LAN client |
| **0xA1** | `LAN_LOCONET_Z21_TX` | Messages sent by Z21 to LocoNet bus → LAN client |
| **0xA2** | `LAN_LOCONET_FROM_LAN` | Messages from LAN client → Z21 → LocoNet bus |

#### Subscription

Clients must subscribe to LocoNet messages using the `LAN_SET_BROADCASTFLAGS` command (see section 2.16).

#### Gateway Functionality

The Z21 acts as:
- **LocoNet Master**: Manages refresh slots and generates DCC packets
- **Gateway**: Forwards messages between Ethernet and LocoNet bus

#### Example: LocoNet Message

LocoNet `OPC_MOVE_SLOTS` command sent by Z21:

| Field | Value | Description |
|-------|-------|-------------|
| DataLen | 0x0008 | 8 bytes total |
| Header | 0x00A0 | LAN_LOCONET_Z21_RX |
| OPC | 0xBA | OPC_MOVE_SLOTS operation code |
| ARG1 | 0x00 | Argument 1 |
| ARG2 | 0x00 | Argument 2 |
| CKSUM | 0x45 | LocoNet checksum |

#### C# Implementation

```csharp
public struct LocoNetMessage
{
    public byte OpCode { get; set; }
    public byte[] Arguments { get; set; }
    public byte Checksum { get; set; }

    public byte[] ToZ21Data()
    {
        var data = new byte[Arguments.Length + 2];
        data[0] = OpCode;
        Array.Copy(Arguments, 0, data, 1, Arguments.Length);
        data[^1] = Checksum;
        return data;
    }
}

// Example: OPC_MOVE_SLOTS
var locoNetMsg = new LocoNetMessage
{
    OpCode = 0xBA,
    Arguments = [0x00, 0x00],
    Checksum = 0x45
};

var z21Packet = new Z21Packet
{
    Header = 0x00A0, // LAN_LOCONET_Z21_RX
    Data = locoNetMsg.ToZ21Data()
};
z21Packet.DataLen = (ushort)(4 + z21Packet.Data.Length);
```

See **Section 9 (LocoNet)** for detailed information about LocoNet gateway functionality.

## 1.3 Combining Datasets in One UDP Packet

Multiple independent Z21 datasets can be combined in a single UDP packet payload. All recipients must be able to parse these combined packets.

### Example: Combined Datasets

**Single UDP packet containing three Z21 datasets:**

```
┌─────────────────────────────────────────────────────────────────────┐
│ UDP Packet                                                          │
├────────────┬────────────┬──────────────────────────────────────────┤
│ IP Header  │ UDP Header │ UDP Payload                              │
│            │            ├──────────┬───────────┬───────────────────┤
│            │            │ Dataset 1│ Dataset 2 │ Dataset 3         │
│            │            │ GET_INFO │ GET_INFO  │ RMBUS_GETDATA #0  │
│            │            │ TURNOUT#4│ TURNOUT#5 │                   │
└────────────┴────────────┴──────────┴───────────┴───────────────────┘
```

**Is equivalent to three separate UDP packets:**

```
┌──────────────────────────────────────┐
│ UDP Packet 1                         │
├───────┬────────┬─────────────────────┤
│ IP    │ UDP    │ Z21 Dataset         │
│ Header│ Header │ GET_INFO TURNOUT #4 │
└───────┴────────┴─────────────────────┘

┌──────────────────────────────────────┐
│ UDP Packet 2                         │
├───────┬────────┬─────────────────────┤
│ IP    │ UDP    │ Z21 Dataset         │
│ Header│ Header │ GET_INFO TURNOUT #5 │
└───────┴────────┴─────────────────────┘

┌──────────────────────────────────────┐
│ UDP Packet 3                         │
├───────┬────────┬─────────────────────┤
│ IP    │ UDP    │ Z21 Dataset         │
│ Header│ Header │ RMBUS_GETDATA #0    │
└───────┴────────┴─────────────────────┘
```

### Size Constraints

UDP packets must fit within the Ethernet MTU (Maximum Transmission Unit):

- **Ethernet MTU**: 1500 bytes
- **IPv4 Header**: 20 bytes
- **UDP Header**: 8 bytes
- **Maximum Payload**: 1500 - 20 - 8 = **1472 bytes**

### C# Implementation

```csharp
public class Z21PacketBuilder
{
    private const int MaxUdpPayload = 1472;
    private readonly List<byte> _buffer = [];

    public void AddPacket(Z21Packet packet)
    {
        byte[] serialized = Serialize(packet);

        if (_buffer.Count + serialized.Length > MaxUdpPayload)
        {
            throw new InvalidOperationException("Combined packets exceed UDP payload limit");
        }

        _buffer.AddRange(serialized);
    }

    public byte[] Build()
    {
        return [.._buffer];
    }

    public void Clear()
    {
        _buffer.Clear();
    }
}

// Example usage
var builder = new Z21PacketBuilder();
builder.AddPacket(createTurnoutInfoPacket(4));
builder.AddPacket(createTurnoutInfoPacket(5));
builder.AddPacket(createRmbusDataPacket(0));

byte[] combinedPayload = builder.Build();
// Send combinedPayload via UDP
```

### Parsing Combined Packets

```csharp
public static IEnumerable<Z21Packet> ParseCombinedPackets(byte[] udpPayload)
{
    int offset = 0;

    while (offset < udpPayload.Length)
    {
        if (offset + 4 > udpPayload.Length)
            break; // Not enough data for header

        ushort dataLen = (ushort)(udpPayload[offset] | (udpPayload[offset + 1] << 8));

        if (offset + dataLen > udpPayload.Length)
            break; // Incomplete packet

        ushort header = (ushort)(udpPayload[offset + 2] | (udpPayload[offset + 3] << 8));

        byte[] data = new byte[dataLen - 4];
        Array.Copy(udpPayload, offset + 4, data, 0, data.Length);

        yield return new Z21Packet
        {
            DataLen = dataLen,
            Header = header,
            Data = data
        };

        offset += dataLen;
    }
}
```

## Summary

The Z21 LAN protocol provides a flexible UDP-based communication system with:
- **Asynchronous messaging** with broadcast support
- **Simple packet structure** (Length + Header + Data)
- **Protocol tunneling** for X-BUS and LocoNet
- **Packet combining** for efficient bulk operations
- **Little-endian** byte ordering throughout

All clients must maintain periodic communication (at least once per minute) to remain active.
