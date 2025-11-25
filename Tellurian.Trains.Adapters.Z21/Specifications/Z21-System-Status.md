# Z21 LAN Protocol - System, Status, and Versions

**Document Version:** 1.13 (English)
**Date:** November 6, 2023

This section covers commands for system information, status monitoring, track power control, and broadcast configuration.

## Table of Contents

- [Serial Number and Login](#serial-number-and-login)
- [Version Information](#version-information)
- [Track Power Control](#track-power-control)
- [Broadcast Messages](#broadcast-messages)
- [System Status](#system-status)
- [Hardware Information](#hardware-information)

---

## Serial Number and Login

### 2.1 LAN_GET_SERIAL_NUMBER

Retrieves the unique serial number of the Z21 device.

**Request to Z21:**

| DataLen | Header | Data |
|---------|--------|------|
| 0x04 0x00 | 0x10 0x00 | - |

**Reply from Z21:**

| DataLen | Header | Data |
|---------|--------|------|
| 0x08 0x00 | 0x10 0x00 | 32-bit Serial Number (little-endian) |

```csharp
public async Task<uint> GetSerialNumberAsync()
{
    byte[] request = [0x04, 0x00, 0x10, 0x00];
    byte[] response = await SendAndReceiveAsync(request);

    // Extract serial number (bytes 4-7)
    uint serialNumber = BitConverter.ToUInt32(response, 4);
    return serialNumber;
}
```

---

### 2.2 LAN_LOGOFF

Logs off the client from the Z21. Use the same port number for logoff as for login.

**Request to Z21:**

| DataLen | Header | Data |
|---------|--------|------|
| 0x04 0x00 | 0x30 0x00 | - |

**Reply:** None

**Important Notes:**
- Login is **implicitly** done with the first command sent by the client (e.g., `LAN_SYSTEM_STATE_GETDATA`)
- Use the same UDP source port for logoff that was used for communication
- Proper logoff helps the Z21 manage client connections efficiently

```csharp
public async Task LogoffAsync()
{
    byte[] request = [0x04, 0x00, 0x30, 0x00];
    await SendAsync(request);
    // No response expected
}
```

---

## Version Information

### 2.3 LAN_X_GET_VERSION

Reads the X-Bus protocol version and command station ID.

**Request to Z21:**

| DataLen | Header | Data ||||
|---------|--------|------|-----|-----|----------|
| 0x07 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **XOR-Byte** |
|           |           | 0x21 | 0x21 | 0x00 |

**Reply from Z21:**

| DataLen | Header | Data ||||||
|---------|--------|------|-----|-----|----------|-----|----------|
| 0x09 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **DB1** | **DB2** | **XOR-Byte** |
|           |           | 0x63 | 0x21 | XBUS_VER | CMDST_ID | 0x60 |

**Field Descriptions:**

| Field | Description |
|-------|-------------|
| **XBUS_VER** | X-Bus protocol version (0x30 = V3.0, 0x36 = V3.6, 0x40 = V4.0) |
| **CMDST_ID** | Command station ID (0x12 = Z21 device family) |

```csharp
public struct XBusVersion
{
    public byte ProtocolVersion { get; set; }  // e.g., 0x40 = V4.0
    public byte CommandStationId { get; set; } // 0x12 = Z21

    public string GetVersionString()
    {
        int major = (ProtocolVersion >> 4) & 0x0F;
        int minor = ProtocolVersion & 0x0F;
        return $"V{major}.{minor}";
    }
}

public async Task<XBusVersion> GetXBusVersionAsync()
{
    byte[] request = [0x07, 0x00, 0x40, 0x00, 0x21, 0x21, 0x00];
    byte[] response = await SendAndReceiveAsync(request);

    return new XBusVersion
    {
        ProtocolVersion = response[6], // DB1
        CommandStationId = response[7]  // DB2
    };
}
```

---

### 2.15 LAN_X_GET_FIRMWARE_VERSION

Reads the firmware version of the Z21.

**Request to Z21:**

| DataLen | Header | Data ||||
|---------|--------|------|-----|----------|
| 0x07 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **XOR-Byte** |
|           |           | 0xF1 | 0x0A | 0xFB |

**Reply from Z21:**

| DataLen | Header | Data ||||||
|---------|--------|------|-----|-----|-----|----------|
| 0x09 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **DB1** | **DB2** | **XOR-Byte** |
|           |           | 0xF3 | 0x0A | V_MSB | V_LSB | XOR-Byte |

**Version Format:** BCD (Binary-Coded Decimal)

**Example:**
- Raw bytes: `0x09 0x00 0x40 0x00 0xF3 0x0A 0x01 0x23 0xDB`
- Version: **1.23**

```csharp
public struct FirmwareVersion
{
    public byte MajorMinor { get; set; }  // BCD format
    public byte Revision { get; set; }     // BCD format

    public override string ToString()
    {
        int major = (MajorMinor >> 4) & 0x0F;
        int minor = MajorMinor & 0x0F;
        int rev1 = (Revision >> 4) & 0x0F;
        int rev2 = Revision & 0x0F;
        return $"{major}.{minor}{rev1}{rev2}";
    }
}

public async Task<FirmwareVersion> GetFirmwareVersionAsync()
{
    byte[] request = [0x07, 0x00, 0x40, 0x00, 0xF1, 0x0A, 0xFB];
    byte[] response = await SendAndReceiveAsync(request);

    return new FirmwareVersion
    {
        MajorMinor = response[6], // DB1 (e.g., 0x01)
        Revision = response[7]    // DB2 (e.g., 0x23)
    };
}
```

---

## Track Power Control

### 2.5 LAN_X_SET_TRACK_POWER_OFF

Switches off the track voltage.

**Request to Z21:**

| DataLen | Header | Data ||||
|---------|--------|------|-----|----------|
| 0x07 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **XOR-Byte** |
|           |           | 0x21 | 0x80 | 0xA1 |

**Reply:** See [2.7 LAN_X_BC_TRACK_POWER_OFF](#27-lan_x_bc_track_power_off)

```csharp
public async Task SetTrackPowerOffAsync()
{
    byte[] request = [0x07, 0x00, 0x40, 0x00, 0x21, 0x80, 0xA1];
    await SendAsync(request);
}
```

---

### 2.6 LAN_X_SET_TRACK_POWER_ON

Switches on the track voltage, or terminates emergency stop or programming mode.

**Request to Z21:**

| DataLen | Header | Data ||||
|---------|--------|------|-----|----------|
| 0x07 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **XOR-Byte** |
|           |           | 0x21 | 0x81 | 0xA0 |

**Reply:** See [2.8 LAN_X_BC_TRACK_POWER_ON](#28-lan_x_bc_track_power_on)

```csharp
public async Task SetTrackPowerOnAsync()
{
    byte[] request = [0x07, 0x00, 0x40, 0x00, 0x21, 0x81, 0xA0];
    await SendAsync(request);
}
```

---

### 2.13 LAN_X_SET_STOP

Activates emergency stop. Locomotives are stopped but track voltage remains on.

**Request to Z21:**

| DataLen | Header | Data |||
|---------|--------|------|----------|
| 0x06 0x00 | 0x40 0x00 | **X-Header** | **XOR-Byte** |
|           |           | 0x80 | 0x80 |

**Reply:** See [2.14 LAN_X_BC_STOPPED](#214-lan_x_bc_stopped)

```csharp
public async Task SetEmergencyStopAsync()
{
    byte[] request = [0x06, 0x00, 0x40, 0x00, 0x80, 0x80];
    await SendAsync(request);
}
```

---

## Broadcast Messages

These messages are sent by the Z21 to all registered clients when specific events occur and the client has subscribed to the corresponding broadcast flag.

### 2.7 LAN_X_BC_TRACK_POWER_OFF

Broadcast sent when track power is turned off.

**Trigger Conditions:**
- Client sent `LAN_X_SET_TRACK_POWER_OFF` command
- Track voltage switched off by input device (e.g., multiMaus)
- Client has broadcast flag `0x00000001` enabled

**Z21 to Client:**

| DataLen | Header | Data ||||
|---------|--------|------|-----|----------|
| 0x07 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **XOR-Byte** |
|           |           | 0x61 | 0x00 | 0x61 |

---

### 2.8 LAN_X_BC_TRACK_POWER_ON

Broadcast sent when track power is turned on.

**Z21 to Client:**

| DataLen | Header | Data ||||
|---------|--------|------|-----|----------|
| 0x07 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **XOR-Byte** |
|           |           | 0x61 | 0x01 | 0x60 |

---

### 2.9 LAN_X_BC_PROGRAMMING_MODE

Broadcast sent when Z21 enters CV programming mode (via `LAN_X_CV_READ` or `LAN_X_CV_WRITE`).

**Z21 to Client:**

| DataLen | Header | Data ||||
|---------|--------|------|-----|----------|
| 0x07 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **XOR-Byte** |
|           |           | 0x61 | 0x02 | 0x63 |

---

### 2.10 LAN_X_BC_TRACK_SHORT_CIRCUIT

Broadcast sent when a short circuit is detected.

**Z21 to Client:**

| DataLen | Header | Data ||||
|---------|--------|------|-----|----------|
| 0x07 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **XOR-Byte** |
|           |           | 0x61 | 0x08 | 0x69 |

---

### 2.14 LAN_X_BC_STOPPED

Broadcast sent when emergency stop is activated.

**Z21 to Client:**

| DataLen | Header | Data ||||
|---------|--------|------|-----|----------|
| 0x07 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **XOR-Byte** |
|           |           | 0x81 | 0x00 | 0x81 |

---

### 2.11 LAN_X_UNKNOWN_COMMAND

Sent by Z21 in response to an invalid or unknown request.

**Z21 to Client:**

| DataLen | Header | Data ||||
|---------|--------|------|-----|----------|
| 0x07 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **XOR-Byte** |
|           |           | 0x61 | 0x82 | 0xE3 |

---

## System Status

### 2.4 LAN_X_GET_STATUS

Requests the current Z21 status.

**Request to Z21:**

| DataLen | Header | Data ||||
|---------|--------|------|-----|----------|
| 0x07 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **XOR-Byte** |
|           |           | 0x21 | 0x24 | 0x05 |

**Reply:** See [2.12 LAN_X_STATUS_CHANGED](#212-lan_x_status_changed)

**Note:** This command station status is identical to `CentralState` in `LAN_SYSTEMSTATE_DATACHANGED`.

---

### 2.12 LAN_X_STATUS_CHANGED

Returns the command station status.

**Z21 to Client:**

| DataLen | Header | Data |||||
|---------|--------|------|-----|--------|----------|
| 0x08 0x00 | 0x40 0x00 | **X-Header** | **DB0** | **DB1** | **XOR-Byte** |
|           |           | 0x62 | 0x22 | Status | XOR-Byte |

**Status Bitmask:**

```csharp
[Flags]
public enum CentralState : byte
{
    EmergencyStop = 0x01,         // Emergency stop is active
    TrackVoltageOff = 0x02,       // Track voltage is off
    ShortCircuit = 0x04,          // Short circuit detected
    ProgrammingModeActive = 0x20  // Programming mode is active
}
```

**Example Usage:**

```csharp
public async Task<CentralState> GetStatusAsync()
{
    byte[] request = [0x07, 0x00, 0x40, 0x00, 0x21, 0x24, 0x05];
    byte[] response = await SendAndReceiveAsync(request);

    return (CentralState)response[6]; // DB1
}

public bool IsTrackPowerOn(CentralState status)
{
    return !status.HasFlag(CentralState.TrackVoltageOff);
}
```

---

### 2.16 LAN_SET_BROADCASTFLAGS

Configures which broadcast messages the client wants to receive. Flags are set per client (IP + port) and must be set again after reconnecting.

**Request to Z21:**

| DataLen | Header | Data |
|---------|--------|------|
| 0x08 0x00 | 0x50 0x00 | 32-bit Broadcast Flags (little-endian) |

**Broadcast Flags:**

```csharp
[Flags]
public enum BroadcastFlags : uint
{
    // Basic driving and switching (0x00000001)
    DrivingSwitching = 0x00000001,

    // R-Bus feedback changes (0x00000002)
    RBusFeedback = 0x00000002,

    // RailCom changes for subscribed locos (0x00000004)
    RailComSubscribed = 0x00000004,

    // System status changes (0x00000100)
    SystemStatus = 0x00000100,

    // From FW 1.20: All loco info without subscription (0x00010000)
    // WARNING: High network traffic - PC automation only, NOT for mobile controllers
    AllLocoInfo = 0x00010000,

    // From FW 1.29: All RailCom data without subscription (0x00040000)
    // WARNING: High network traffic - PC automation only
    AllRailCom = 0x00040000,

    // From FW 1.30: CAN detector changes (0x00080000)
    CanDetector = 0x00080000,

    // From FW 1.41: CAN booster status (0x00020000)
    CanBoosterStatus = 0x00020000,

    // From FW 1.43: Fast clock time (0x00000010)
    FastClock = 0x00000010,

    // LocoNet forwarding (from FW 1.20)
    LocoNetGeneral = 0x01000000,           // General LocoNet messages
    LocoNetLocos = 0x02000000,             // Loco-specific messages
    LocoNetSwitches = 0x04000000,          // Switch-specific messages
    LocoNetDetector = 0x08000000           // From FW 1.22: Detector messages
}
```

**Flag 0x00000001 includes:**
- `LAN_X_BC_TRACK_POWER_OFF`
- `LAN_X_BC_TRACK_POWER_ON`
- `LAN_X_BC_PROGRAMMING_MODE`
- `LAN_X_BC_TRACK_SHORT_CIRCUIT`
- `LAN_X_BC_STOPPED`
- `LAN_X_LOCO_INFO` (for subscribed locomotives)
- `LAN_X_TURNOUT_INFO`

**Important Performance Notes:**

⚠️ **Network Load Consideration:** Flags `0x00010000`, `0x00040000`, `0x02000000`, and `0x04000000` generate high network traffic. UDP packets may be dropped by routers during overload. These flags should **only** be used by PC railroad automation software, **never** by mobile controllers.

Consider using `0x00000001` with specific broadcast messages instead of `0x00000100` if detailed voltage, current, and temperature updates are not needed.

```csharp
public async Task SetBroadcastFlagsAsync(BroadcastFlags flags)
{
    uint flagValue = (uint)flags;
    byte[] request =
    [
        0x08, 0x00, 0x50, 0x00,
        (byte)(flagValue & 0xFF),
        (byte)((flagValue >> 8) & 0xFF),
        (byte)((flagValue >> 16) & 0xFF),
        (byte)((flagValue >> 24) & 0xFF)
    ];
    await SendAsync(request);
}

// Example: Subscribe to basic events and system status
await SetBroadcastFlagsAsync(
    BroadcastFlags.DrivingSwitching |
    BroadcastFlags.SystemStatus
);
```

---

### 2.17 LAN_GET_BROADCASTFLAGS

Reads the current broadcast flags for this client.

**Request to Z21:**

| DataLen | Header | Data |
|---------|--------|------|
| 0x04 0x00 | 0x51 0x00 | - |

**Reply from Z21:**

| DataLen | Header | Data |
|---------|--------|------|
| 0x08 0x00 | 0x51 0x00 | Broadcast Flags 32-bit (little-endian) |

---

### 2.18 LAN_SYSTEMSTATE_DATACHANGED

Reports detailed system status including current, voltage, and temperature.

**Trigger Conditions:**
- Client has broadcast flag `0x00000100` enabled (asynchronous)
- Client explicitly requested via `LAN_SYSTEMSTATE_GETDATA`

**Z21 to Client:**

| DataLen | Header | Data |
|---------|--------|------|
| 0x14 0x00 | 0x84 0x00 | SystemState (16 bytes) |

**SystemState Structure:**

| Offset | Type | Name | Unit | Description |
|--------|------|------|------|-------------|
| 0 | INT16 | MainCurrent | mA | Current on main track |
| 2 | INT16 | ProgCurrent | mA | Current on programming track |
| 4 | INT16 | FilteredMainCurrent | mA | Smoothed current on main track |
| 6 | INT16 | Temperature | °C | Internal temperature |
| 8 | UINT16 | SupplyVoltage | mV | Supply voltage |
| 10 | UINT16 | VCCVoltage | mV | Internal voltage (= track voltage) |
| 12 | UINT8 | CentralState | bitmask | Command station status |
| 13 | UINT8 | CentralStateEx | bitmask | Extended status |
| 14 | UINT8 | Reserved | - | Reserved for future use |
| 15 | UINT8 | Capabilities | bitmask | From FW 1.42+ |

**CentralState Bitmask:**

```csharp
[Flags]
public enum CentralState : byte
{
    EmergencyStop = 0x01,         // Emergency stop active
    TrackVoltageOff = 0x02,       // Track voltage off
    ShortCircuit = 0x04,          // Short circuit
    ProgrammingModeActive = 0x20  // Programming mode active
}
```

**CentralStateEx Bitmask:**

```csharp
[Flags]
public enum CentralStateEx : byte
{
    HighTemperature = 0x01,        // Temperature too high
    PowerLost = 0x02,              // Input voltage too low
    ShortCircuitExternal = 0x04,   // Short circuit at external booster
    ShortCircuitInternal = 0x08,   // Short circuit at main/prog track
    RCN213 = 0x20                  // Turnout addresses per RCN-213 (from FW 1.42)
}
```

**Capabilities Bitmask (from FW 1.42):**

```csharp
[Flags]
public enum Capabilities : byte
{
    DCC = 0x01,                    // Capable of DCC
    MM = 0x02,                     // Capable of Motorola (MM)
    // Reserved = 0x04,
    RailCom = 0x08,                // RailCom is activated
    LocoCmds = 0x10,               // Accepts loco decoder commands
    AccessoryCmds = 0x20,          // Accepts accessory decoder commands
    DetectorCmds = 0x40,           // Accepts detector commands
    NeedsUnlockCode = 0x80         // Needs unlock code (z21start)
}
```

**C# Structure:**

```csharp
public struct SystemState
{
    public short MainCurrent { get; set; }           // mA
    public short ProgCurrent { get; set; }           // mA
    public short FilteredMainCurrent { get; set; }   // mA
    public short Temperature { get; set; }           // °C
    public ushort SupplyVoltage { get; set; }        // mV
    public ushort VCCVoltage { get; set; }           // mV
    public CentralState CentralState { get; set; }
    public CentralStateEx CentralStateEx { get; set; }
    public byte Reserved { get; set; }
    public Capabilities Capabilities { get; set; }    // FW 1.42+

    public static SystemState Parse(byte[] data, int offset = 4)
    {
        return new SystemState
        {
            MainCurrent = BitConverter.ToInt16(data, offset),
            ProgCurrent = BitConverter.ToInt16(data, offset + 2),
            FilteredMainCurrent = BitConverter.ToInt16(data, offset + 4),
            Temperature = BitConverter.ToInt16(data, offset + 6),
            SupplyVoltage = BitConverter.ToUInt16(data, offset + 8),
            VCCVoltage = BitConverter.ToUInt16(data, offset + 10),
            CentralState = (CentralState)data[offset + 12],
            CentralStateEx = (CentralStateEx)data[offset + 13],
            Reserved = data[offset + 14],
            Capabilities = (Capabilities)data[offset + 15]
        };
    }
}
```

**Important:** If `Capabilities == 0`, assume older firmware version and do not evaluate the Capabilities field.

---

### 2.19 LAN_SYSTEMSTATE_GETDATA

Explicitly requests the current system status.

**Request to Z21:**

| DataLen | Header | Data |
|---------|--------|------|
| 0x04 0x00 | 0x85 0x00 | - |

**Reply:** See [2.18 LAN_SYSTEMSTATE_DATACHANGED](#218-lan_systemstate_datachanged)

```csharp
public async Task<SystemState> GetSystemStateAsync()
{
    byte[] request = [0x04, 0x00, 0x85, 0x00];
    byte[] response = await SendAndReceiveAsync(request);

    return SystemState.Parse(response);
}
```

---

## Hardware Information

### 2.20 LAN_GET_HWINFO

**Available from Z21 FW Version 1.20 and SmartRail FW Version 1.13**

Reads the hardware type and firmware version.

**Request to Z21:**

| DataLen | Header | Data |
|---------|--------|------|
| 0x04 0x00 | 0x1A 0x00 | - |

**Reply from Z21:**

| DataLen | Header | Data ||
|---------|--------|------|------|
| 0x0C 0x00 | 0x1A 0x00 | HwType 32-bit (little-endian) | FW Version 32-bit (little-endian) |

**Hardware Types:**

```csharp
public enum HardwareType : uint
{
    Z21_OLD = 0x00000200,           // Black Z21 (2012 variant)
    Z21_NEW = 0x00000201,           // Black Z21 (2013+ variant)
    SMARTRAIL = 0x00000202,         // SmartRail (2012)
    z21_SMALL = 0x00000203,         // White z21 starter set (2013)
    z21_START = 0x00000204,         // z21 start starter set (2016)
    SINGLE_BOOSTER = 0x00000205,    // 10806 Z21 Single Booster (zLink)
    DUAL_BOOSTER = 0x00000206,      // 10807 Z21 Dual Booster (zLink)
    Z21_XL = 0x00000211,            // 10870 Z21 XL Series (2020+)
    XL_BOOSTER = 0x00000212,        // 10869 Z21 XL Booster (2021, zLink)
    Z21_SWITCH_DECODER = 0x00000301,// 10836 Z21 SwitchDecoder (zLink)
    Z21_SIGNAL_DECODER = 0x00000302 // 10836 Z21 SignalDecoder (zLink)
}
```

**Firmware Version:** BCD format

**Example:**
- Raw: `0x0C 0x00 0x1A 0x00 0x00 0x02 0x00 0x00 0x20 0x01 0x00 0x00`
- Hardware: `0x200` (Z21_OLD)
- Firmware: `1.20`

**Fallback for Older Firmware:**

For devices that don't support `LAN_GET_HWINFO`, use `LAN_X_GET_FIRMWARE_VERSION` and apply these rules:
- V1.10 → Z21 (2012 variant)
- V1.11 → Z21 (2012 variant)
- V1.12 → SmartRail (2012)

```csharp
public struct HardwareInfo
{
    public HardwareType HardwareType { get; set; }
    public uint FirmwareVersionRaw { get; set; }

    public string GetFirmwareVersion()
    {
        // BCD format: 0x00000120 = 1.20
        int major = (int)((FirmwareVersionRaw >> 8) & 0xFF);
        int minor = (int)(FirmwareVersionRaw & 0xFF);
        return $"{major / 16}.{major % 16}{minor / 16}{minor % 16}";
    }
}

public async Task<HardwareInfo> GetHardwareInfoAsync()
{
    byte[] request = [0x04, 0x00, 0x1A, 0x00];
    byte[] response = await SendAndReceiveAsync(request);

    return new HardwareInfo
    {
        HardwareType = (HardwareType)BitConverter.ToUInt32(response, 4),
        FirmwareVersionRaw = BitConverter.ToUInt32(response, 8)
    };
}
```

---

### 2.21 LAN_GET_CODE

Reads the software feature scope (unlock status) of the Z21/z21/z21start.

This is particularly important for the "z21 start" variant to check if driving and switching via LAN is permitted.

**Request to Z21:**

| DataLen | Header | Data |
|---------|--------|------|
| 0x04 0x00 | 0x18 0x00 | - |

**Reply from Z21:**

| DataLen | Header | Data |
|---------|--------|------|
| 0x05 0x00 | 0x18 0x00 | Code (8-bit) |

**Code Values:**

```csharp
public enum UnlockCode : byte
{
    Z21_NO_LOCK = 0x00,        // All features permitted
    z21_START_LOCKED = 0x01,   // Driving and switching blocked
    z21_START_UNLOCKED = 0x02  // Driving and switching permitted
}
```

```csharp
public async Task<UnlockCode> GetUnlockCodeAsync()
{
    byte[] request = [0x04, 0x00, 0x18, 0x00];
    byte[] response = await SendAndReceiveAsync(request);

    return (UnlockCode)response[4];
}

public async Task<bool> IsDrivingPermittedAsync()
{
    var code = await GetUnlockCodeAsync();
    return code != UnlockCode.z21_START_LOCKED;
}
```

---

## Summary

This section covers essential Z21 system commands:

- **Serial Number & Login**: Identify devices and manage connections
- **Version Info**: Query X-Bus version, firmware version, and hardware type
- **Track Power**: Control track voltage and emergency stop
- **Broadcast Configuration**: Subscribe to events with fine-grained control
- **System Status**: Monitor current, voltage, temperature, and operational state
- **Hardware Info**: Identify device capabilities and unlock status

**Key Concepts:**

1. **Implicit Login**: First command from client automatically registers it
2. **Keep-Alive**: Communicate at least once per minute or be disconnected
3. **Broadcast Flags**: Must be set per client session after each connection
4. **Network Load**: Be cautious with high-traffic broadcast flags
5. **Little-Endian**: All multi-byte values use little-endian byte order
