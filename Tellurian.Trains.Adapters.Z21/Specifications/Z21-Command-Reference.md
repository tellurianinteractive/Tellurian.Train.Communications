# Z21 LAN Protocol - Command Reference

**Document Version:** 1.13 (English)
**Date:** November 6, 2023

This appendix provides a complete overview of all Z21 LAN protocol commands and their device support.

## Device Compatibility

### LAN Devices (Z21 Family)

| Device | Description | Identifier |
|--------|-------------|------------|
| **Z21** | Black Z21 (original) | Full command support |
| **Z21 XL** | Z21 XL Series (from 2020) | Full command support |
| **z21** | White z21 starter set | Most commands (see notes) |
| **z21 start** | z21 start starter set (2016) | Limited (needs activation code for full functionality) |

### zLink Devices

| Device | Model Number | Type |
|--------|--------------|------|
| **Single Booster** | 10806 | Z21 Single Booster |
| **Dual Booster** | 10807 | Z21 Dual Booster |
| **XL Booster** | 10869 | Z21 XL Booster (2021) |
| **Switch Decoder** | 10836 | Z21 SwitchDecoder |
| **Signal Decoder** | 10837 | Z21 SignalDecoder |

---

## Table 1: Commands from Client to Z21

These messages can be sent from a client to a Z21 or zLink device.

### System & Information Commands

| Header | X-Header | DB0 | Parameter | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|-----------|--------------|--------|-----------|---------|---------|
| 0x10 | - | - | - | LAN_GET_SERIAL_NUMBER | ✓ | ✓ | ✓ | ✓ |
| 0x18 | - | - | - | LAN_GET_CODE | ✓ | ✓ | ✓ | ✓ |
| 0x1A | - | - | - | LAN_GET_HWINFO | ✓ | ✓ | ✓ | ✓ |
| 0x30 | - | - | - | LAN_LOGOFF | ✓ | ✓ | ✓ | ✓ |

### Track Power & Status Commands

| Header | X-Header | DB0 | Parameter | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|-----------|--------------|--------|-----------|---------|---------|
| 0x40 | 0x21 | 0x21 | - | LAN_X_GET_VERSION | ✓ | ✓ | ✓ | ✓ |
| 0x40 | 0x21 | 0x24 | - | LAN_X_GET_STATUS | ✓ | ✓ | ✓ | ✓ |
| 0x40 | 0x21 | 0x80 | - | LAN_X_SET_TRACK_POWER_OFF | ✓ | ✓ | ✓ | ✓ |
| 0x40 | 0x21 | 0x81 | - | LAN_X_SET_TRACK_POWER_ON | ✓ | ✓ | ✓ | ✓ ⁽⁴⁾ |
| 0x40 | 0x80 | - | - | LAN_X_SET_STOP | ✓ | ✓ | ✓ | ✓ ⁽⁵⁾ |
| 0x40 | 0xF1 | 0x0A | - | LAN_X_GET_FIRMWARE_VERSION | ✓ | ✓ | ✓ | ✓ |

### CV Programming Commands

| Header | X-Header | DB0 | Parameter | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|-----------|--------------|--------|-----------|---------|---------|
| 0x40 | 0x22 | 0x11 | Register | LAN_X_DCC_READ_REGISTER | ✓ | ✓ | ✗ | ✗ |
| 0x40 | 0x23 | 0x11 | CV-Address | LAN_X_CV_READ | ✓ | ✓ | ✗ | ✗ |
| 0x40 | 0x23 | 0x12 | Register, Value | LAN_X_DCC_WRITE_REGISTER | ✓ | ✓ | ✗ | ✗ |
| 0x40 | 0x24 | 0x12 | CV-Address, Value | LAN_X_CV_WRITE | ✓ | ✓ | ✗ | ✗ |
| 0x40 | 0x24 | 0xFF | Register, Value | LAN_X_MM_WRITE_BYTE | ✓ | ✓ | ✗ | ✗ |

### POM (Programming on Main) Commands

| Header | X-Header | DB0 | Parameter | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|-----------|--------------|--------|-----------|---------|---------|
| 0x40 | 0xE6 | 0x30 | POM-Param, Option 0xEC | LAN_X_CV_POM_WRITE_BYTE | ✓ | ✓ | ✗ | ✗ |
| 0x40 | 0xE6 | 0x30 | POM-Param, Option 0xE8 | LAN_X_CV_POM_WRITE_BIT | ✓ | ✓ | ✗ | ✗ |
| 0x40 | 0xE6 | 0x30 | POM-Param, Option 0xE4 | LAN_X_CV_POM_READ_BYTE | ✓ | ✓ | ✗ | ✗ |
| 0x40 | 0xE6 | 0x31 | POM-Param, Option 0xEC | LAN_X_CV_POM_ACCESSORY_WRITE_BYTE | ✓ | ✓ | ✗ | ✗ |
| 0x40 | 0xE6 | 0x31 | POM-Param, Option 0xE8 | LAN_X_CV_POM_ACCESSORY_WRITE_BIT | ✓ | ✓ | ✗ | ✗ |
| 0x40 | 0xE6 | 0x31 | POM-Param, Option 0xE4 | LAN_X_CV_POM_ACCESSORY_READ_BYTE | ✓ | ✓ | ✗ | ✗ |

### Locomotive Control Commands

| Header | X-Header | DB0 | Parameter | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|-----------|--------------|--------|-----------|---------|---------|
| 0x40 | 0x92 | - | Loco address | LAN_X_SET_LOCO_E_STOP | ✓ | ✓ | ✗ | ✗ |
| 0x40 | 0xE3 | 0x44 | Loco address | LAN_X_PURGE_LOCO | ✓ | ✓ | ✗ | ✗ |
| 0x40 | 0xE3 | 0xF0 | Loco address | LAN_X_GET_LOCO_INFO | ✓ | ✓ | ✗ | ✗ |
| 0x40 | 0xE4 | 0x1s | Loco address, Speed | LAN_X_SET_LOCO_DRIVE | ✓ | ✓ ⁽¹⁾ | ✗ | ✗ |
| 0x40 | 0xE4 | 0xF8 | Loco address, Function | LAN_X_SET_LOCO_FUNCTION | ✓ | ✓ ⁽¹⁾ | ✗ | ✗ |
| 0x40 | 0xE4 | Group | Loco address, Function group | LAN_X_SET_LOCO_FUNCTION_GROUP | ✓ | ✓ ⁽¹⁾ | ✗ | ✗ |
| 0x40 | 0xE4 | 0x5F | Loco address, Binary state | LAN_X_SET_LOCO_BINARY_STATE | ✓ | ✓ | ✗ | ✗ |

### Accessory/Turnout Control Commands

| Header | X-Header | DB0 | Parameter | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|-----------|--------------|--------|-----------|---------|---------|
| 0x40 | 0x43 | - | Turnout address | LAN_X_GET_TURNOUT_INFO | ✓ | ✓ ⁽¹⁾ | ✗ | ✗ |
| 0x40 | 0x44 | - | Accessory decoder address | LAN_X_GET_EXT_ACCESSORY_INFO | ✓ | ✓ | ✗ | ✓ ⁽³⁾ |
| 0x40 | 0x53 | - | Turnout address, command | LAN_X_SET_TURNOUT | ✓ | ✓ ⁽¹⁾ | ✗ | ✗ |
| 0x40 | 0x54 | - | Accessory decoder address, State | LAN_X_SET_EXT_ACCESSORY | ✓ | ✓ ⁽¹⁾ | ✗ | ✗ |

### Broadcast & Configuration Commands

| Header | X-Header | DB0 | Parameter | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|-----------|--------------|--------|-----------|---------|---------|
| 0x50 | - | - | Broadcast-Flags | LAN_SET_BROADCASTFLAGS | ✓ | ✓ | ✓ | ✓ |
| 0x51 | - | - | - | LAN_GET_BROADCASTFLAGS | ✓ | ✓ | ✓ | ✓ |
| 0x60 | - | - | Loco address | LAN_GET_LOCOMODE | ✓ | ✓ | ✗ | ✗ |
| 0x61 | - | - | Loco address, Mode | LAN_SET_LOCOMODE | ✓ | ✓ | ✗ | ✗ |
| 0x70 | - | - | Accessory decoder address | LAN_GET_TURNOUTMODE | ✓ | ✓ | ✗ | ✗ |
| 0x71 | - | - | Accessory decoder address, Mode | LAN_SET_TURNOUTMODE | ✓ | ✓ | ✗ | ✗ |

### System State Commands

| Header | X-Header | DB0 | Parameter | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|-----------|--------------|--------|-----------|---------|---------|
| 0x85 | - | - | - | LAN_SYSTEMSTATE_GETDATA | ✓ | ✓ | ✓ | ✓ |

### R-Bus Feedback Commands

| Header | X-Header | DB0 | Parameter | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|-----------|--------------|--------|-----------|---------|---------|
| 0x81 | - | - | Group index | LAN_RMBUS_GETDATA | ✓ | ✓ | ✗ | ✗ |
| 0x82 | - | - | Address | LAN_RMBUS_PROGRAMMODULE | ✓ | ✓ | ✗ | ✗ |

### RailCom Commands

| Header | X-Header | DB0 | Parameter | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|-----------|--------------|--------|-----------|---------|---------|
| 0x89 | - | - | Address | LAN_RAILCOM_GETDATA | ✓ | ✓ | ✗ | ✗ |

### LocoNet Commands

| Header | X-Header | DB0 | Parameter | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|-----------|--------------|--------|-----------|---------|---------|
| 0xA2 | - | - | LocoNet message | LAN_LOCONET_FROM_LAN | ✓ | ✓ ⁽¹⁾⁽²⁾ | ✗ | ✗ |
| 0xA3 | - | - | Loco address | LAN_LOCONET_DISPATCH_ADDR | ✓ | ✗ | ✗ | ✗ |
| 0xA4 | - | - | Type, Report address | LAN_LOCONET_DETECTOR | ✓ | ✓ ⁽²⁾ | ✗ | ✗ |

### CAN-Bus Commands

| Header | X-Header | DB0 | Parameter | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|-----------|--------------|--------|-----------|---------|---------|
| 0xC4 | - | - | Type, NId | LAN_CAN_DETECTOR | ✓ | ✗ | ✗ | ✗ |
| 0xC8 | - | - | NetID | LAN_CAN_DEVICE_GET_DESCRIPTION | ✓ | ✗ | ✗ | ✗ |
| 0xC9 | - | - | NetID, Name | LAN_CAN_DEVICE_SET_DESCRIPTION | ✓ | ✗ | ✗ | ✗ |
| 0xCB | - | - | NetID, PowerState | LAN_CAN_BOOSTER_SET_TRACKPOWER | ✓ | ✗ | ✗ | ✗ |

### Fast Clock Commands

| Header | X-Header | DB0 | Parameter | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|-----------|--------------|--------|-----------|---------|---------|
| 0xCC | - | - | Fastclock Start/Stop/Get/Set Command | LAN_FAST_CLOCK_CONTROL | ✓ | ✓ | ✗ | ✗ |
| 0xCE | - | - | Len | LAN_FAST_CLOCK_SETTINGS_GET | ✓ | ✓ | ✗ | ✗ |
| 0xCF | - | - | Fastclock Settings | LAN_FAST_CLOCK_SETTINGS_SET | ✓ | ✓ | ✗ | ✗ |

### Booster Commands

| Header | X-Header | DB0 | Parameter | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|-----------|--------------|--------|-----------|---------|---------|
| 0xB2 | - | - | BoosterPort, BoosterPowerState | LAN_BOOSTER_SET_POWER | ✗ | ✗ | ✓ | ✗ |
| 0xB8 | - | - | - | LAN_BOOSTER_GET_DESCRIPTION | ✗ | ✗ | ✓ | ✗ |
| 0xB9 | - | - | String | LAN_BOOSTER_SET_DESCRIPTION | ✗ | ✗ | ✓ | ✗ |
| 0xBB | - | - | - | LAN_BOOSTER_SYSTEMSTATE_GETDATA | ✗ | ✗ | ✓ | ✗ |

### Decoder Commands

| Header | X-Header | DB0 | Parameter | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|-----------|--------------|--------|-----------|---------|---------|
| 0xD8 | - | - | - | LAN_DECODER_GET_DESCRIPTION | ✗ | ✗ | ✗ | ✓ |
| 0xD9 | - | - | String | LAN_DECODER_SET_DESCRIPTION | ✗ | ✗ | ✗ | ✓ |
| 0xDB | - | - | - | LAN_DECODER_SYSTEMSTATE_GETDATA | ✗ | ✗ | ✗ | ✓ |

### zLink Commands

| Header | X-Header | DB0 | Parameter | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|-----------|--------------|--------|-----------|---------|---------|
| 0xE8 | 0x06 | - | - | LAN_ZLINK_GET_HWINFO | ✗ | ✗ | ✓ ⁽⁶⁾ | ✓ ⁽⁶⁾ |

---

## Table 2: Commands from Z21 to Client

These messages can be sent to a client from a Z21 or zLink device.

### System & Information Replies

| Header | X-Header | DB0 | Data | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|------|--------------|--------|-----------|---------|---------|
| 0x10 | - | - | Serialnumber | Reply to LAN_GET_SERIAL_NUMBER | ✓ | ✓ | ✓ | ✓ |
| 0x18 | - | - | Code | Reply to LAN_GET_CODE | ✓ | ✓ | ✓ | ✓ |
| 0x1A | - | - | HWType, FW Version (BCD) | Reply to LAN_GET_HWINFO | ✓ | ✓ | ✓ | ✓ |

### Track Power & Status Broadcasts

| Header | X-Header | DB0 | Data | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|------|--------------|--------|-----------|---------|---------|
| 0x40 | 0x61 | 0x00 | - | LAN_X_BC_TRACK_POWER_OFF | ✓ | ✓ | ✓ | ✓ |
| 0x40 | 0x61 | 0x01 | - | LAN_X_BC_TRACK_POWER_ON | ✓ | ✓ | ✓ | ✓ |
| 0x40 | 0x61 | 0x02 | - | LAN_X_BC_PROGRAMMING_MODE | ✓ | ✓ | ✗ | ✗ |
| 0x40 | 0x61 | 0x08 | - | LAN_X_BC_TRACK_SHORT_CIRCUIT | ✓ | ✓ | ✓ ⁽⁴⁾ | ✓ ⁽⁴⁾ |
| 0x40 | 0x61 | 0x12 | - | LAN_X_CV_NACK_SC | ✓ | ✓ | ✗ | ✗ |
| 0x40 | 0x61 | 0x13 | - | LAN_X_CV_NACK | ✓ | ✓ | ✗ | ✗ |
| 0x40 | 0x61 | 0x82 | - | LAN_X_UNKNOWN_COMMAND | ✓ | ✓ | ✗ | ✗ |
| 0x40 | 0x81 | - | - | LAN_X_BC_STOPPED | ✓ | ✓ | ✓ | ✓ |

### Version & Status Replies

| Header | X-Header | DB0 | Data | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|------|--------------|--------|-----------|---------|---------|
| 0x40 | 0x62 | 0x22 | State | LAN_X_STATUS_CHANGED | ✓ | ✓ | ✓ | ✓ |
| 0x40 | 0x63 | 0x21 | XBus Version, ID | Reply to LAN_X_GET_VERSION | ✓ | ✓ | ✓ | ✓ |
| 0x40 | 0xF3 | 0x0A | Version (BCD) | Reply to LAN_X_GET_FIRMWARE_VERSION | ✓ | ✓ | ✓ | ✓ |

### CV Programming Replies

| Header | X-Header | DB0 | Data | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|------|--------------|--------|-----------|---------|---------|
| 0x40 | 0x64 | 0x14 | CV-Result | LAN_X_CV_RESULT | ✓ | ✓ | ✗ | ✗ |

### Locomotive Information

| Header | X-Header | DB0 | Data | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|------|--------------|--------|-----------|---------|---------|
| 0x40 | 0xEF | - | Loco information | LAN_X_LOCO_INFO | ✓ | ✓ ⁽¹⁾ | ✗ | ✗ |

### Accessory/Turnout Information

| Header | X-Header | DB0 | Data | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|------|--------------|--------|-----------|---------|---------|
| 0x40 | 0x43 | - | Turnout information | LAN_X_TURNOUT_INFO | ✓ | ✓ ⁽¹⁾ | ✗ | ✗ |
| 0x40 | 0x44 | - | Accessory state information | LAN_X_EXT_ACCESSORY_INFO | ✓ | ✓ ⁽¹⁾ | ✗ | ✓ ⁽³⁾ |

### Broadcast & Configuration Replies

| Header | X-Header | DB0 | Data | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|------|--------------|--------|-----------|---------|---------|
| 0x51 | - | - | Broadcast-Flags | Reply to LAN_GET_BROADCASTFLAGS | ✓ | ✓ | ✓ | ✓ |
| 0x60 | - | - | Loco address, Mode | Reply to LAN_GET_LOCOMODE | ✓ | ✓ | ✗ | ✗ |
| 0x70 | - | - | Accessory decoder address, Mode | Reply to LAN_GET_TURNOUTMODE | ✓ | ✓ | ✗ | ✗ |

### System State Broadcasts

| Header | X-Header | DB0 | Data | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|------|--------------|--------|-----------|---------|---------|
| 0x84 | - | - | SystemState | LAN_SYSTEMSTATE_DATACHANGED | ✓ | ✓ | ✓ | ✓ |

### R-Bus Feedback Broadcasts

| Header | X-Header | DB0 | Data | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|------|--------------|--------|-----------|---------|---------|
| 0x80 | - | - | Group index, Feedback status | LAN_RMBUS_DATACHANGED | ✓ | ✓ | ✗ | ✗ |

### RailCom Broadcasts

| Header | X-Header | DB0 | Data | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|------|--------------|--------|-----------|---------|---------|
| 0x88 | - | - | RailCom data | LAN_RAILCOM_DATACHANGED | ✓ | ✓ | ✗ | ✗ |

### LocoNet Broadcasts

| Header | X-Header | DB0 | Data | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|------|--------------|--------|-----------|---------|---------|
| 0xA0 | - | - | LocoNet-Message | LAN_LOCONET_Z21_RX | ✓ | ✗ | ✗ | ✗ |
| 0xA1 | - | - | LocoNet-Message | LAN_LOCONET_Z21_TX | ✓ | ✓ ⁽²⁾ | ✗ | ✗ |
| 0xA2 | - | - | LocoNet-Message | LAN_LOCONET_FROM_LAN | ✓ | ✓ ⁽²⁾ | ✗ | ✗ |
| 0xA3 | - | - | Loco address, Result | LAN_LOCONET_DISPATCH_ADDR | ✓ | ✗ | ✗ | ✗ |
| 0xA4 | - | - | Type, Feedback address, Info | LAN_LOCONET_DETECTOR | ✓ | ✓ ⁽²⁾ | ✗ | ✗ |

### CAN-Bus Broadcasts

| Header | X-Header | DB0 | Data | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|------|--------------|--------|-----------|---------|---------|
| 0xC4 | - | - | Occupancy message | LAN_CAN_DETECTOR | ✓ | ✗ | ✗ | ✗ |
| 0xC8 | - | - | NetID, Name | Reply to LAN_CAN_DEVICE_GET_DESCRIPTION | ✓ | ✗ | ✗ | ✗ |
| 0xCA | - | - | CANBoosterSystemState | LAN_CAN_BOOSTER_SYSTEMSTATE_CHGD | ✓ | ✗ | ✗ | ✗ |

### Fast Clock Broadcasts

| Header | X-Header | DB0 | Data | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|------|--------------|--------|-----------|---------|---------|
| 0xCD | - | - | Fastclock Time | LAN_FAST_CLOCK_DATA | ✓ | ✓ | ✗ | ✗ |
| 0xCE | - | - | Fastclock Settings | LAN_FAST_CLOCK_SETTINGS_GET | ✓ | ✓ | ✗ | ✗ |

### Booster Broadcasts

| Header | X-Header | DB0 | Data | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|------|--------------|--------|-----------|---------|---------|
| 0xB8 | - | - | String | Reply to LAN_BOOSTER_GET_DESCRIPTION | ✗ | ✗ | ✓ | ✗ |
| 0xBA | - | - | BoosterSystemState | LAN_BOOSTER_SYSTEMSTATE_DATACHANGED | ✗ | ✗ | ✓ | ✗ |

### Decoder Broadcasts

| Header | X-Header | DB0 | Data | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|------|--------------|--------|-----------|---------|---------|
| 0xD8 | - | - | String | Reply to LAN_DECODER_GET_DESCRIPTION | ✗ | ✗ | ✗ | ✓ |
| 0xDA | - | - | DecoderSystemState | LAN_DECODER_SYSTEMSTATE_DATACHANGED | ✗ | ✗ | ✗ | ✓ |

### zLink Replies

| Header | X-Header | DB0 | Data | Command Name | Z21/XL | z21/start | Booster | Decoder |
|--------|----------|-----|------|--------------|--------|-----------|---------|---------|
| 0xE8 | 0x06 | - | Z_Hw_Info | Reply to LAN_ZLINK_GET_HWINFO | ✗ | ✗ | ✓ ⁽⁵⁾ | ✓ ⁽⁵⁾ |

---

## Notes

### General Notes

- ✓ = Command supported
- ✗ = Command not supported
- ⁽ⁿ⁾ = See specific note below

### Specific Notes

1. **z21start: Activation Code Required**
   - z21start is **fully functional** only with activation code (order number 10814 or 10818)
   - Without activation code, driving and switching functions are limited

2. **Virtual LocoNet Stack**
   - z21, z21start: Have **virtual LocoNet stack** (no physical LocoNet interface)
   - Example use: GBM16XN with XPN interface

3. **Decoder Firmware Version**
   - Available from decoder firmware **V1.11+**

4. **Short Circuit Reporting**
   - **Decoder (10837)**: Short-circuit is reported in the corresponding booster/decoder system state
   - **Track Power On (10837)**: Command turns on signal lamps again

5. **Emergency Stop Signal Aspect**
   - **Decoder (10837 only)**: Shows stop aspect if the second bit (0x02) is set in CV38

6. **Z21 pro LINK Response**
   - Answered by the **10838 Z21 pro LINK**, not by its terminal device (booster or decoder)

---

## C# Enumerations

```csharp
/// <summary>
/// Z21/zLink device types
/// </summary>
public enum Z21DeviceType
{
    Z21,              // Black Z21 (original)
    Z21_XL,           // Z21 XL Series
    z21,              // White z21 starter set
    z21_start,        // z21 start (limited functionality)
    SingleBooster,    // 10806 Z21 Single Booster
    DualBooster,      // 10807 Z21 Dual Booster
    XLBooster,        // 10869 Z21 XL Booster
    SwitchDecoder,    // 10836 Z21 SwitchDecoder
    SignalDecoder     // 10837 Z21 SignalDecoder
}

/// <summary>
/// Z21 LAN command headers
/// </summary>
public static class Z21Headers
{
    // System
    public const ushort LAN_GET_SERIAL_NUMBER = 0x0010;
    public const ushort LAN_GET_CODE = 0x0018;
    public const ushort LAN_GET_HWINFO = 0x001A;
    public const ushort LAN_LOGOFF = 0x0030;

    // X-Bus tunnel
    public const ushort LAN_X = 0x0040;

    // Broadcast
    public const ushort LAN_SET_BROADCASTFLAGS = 0x0050;
    public const ushort LAN_GET_BROADCASTFLAGS = 0x0051;

    // Loco mode
    public const ushort LAN_GET_LOCOMODE = 0x0060;
    public const ushort LAN_SET_LOCOMODE = 0x0061;

    // Turnout mode
    public const ushort LAN_GET_TURNOUTMODE = 0x0070;
    public const ushort LAN_SET_TURNOUTMODE = 0x0071;

    // R-Bus
    public const ushort LAN_RMBUS_DATACHANGED = 0x0080;
    public const ushort LAN_RMBUS_GETDATA = 0x0081;
    public const ushort LAN_RMBUS_PROGRAMMODULE = 0x0082;

    // System state
    public const ushort LAN_SYSTEMSTATE_DATACHANGED = 0x0084;
    public const ushort LAN_SYSTEMSTATE_GETDATA = 0x0085;

    // RailCom
    public const ushort LAN_RAILCOM_DATACHANGED = 0x0088;
    public const ushort LAN_RAILCOM_GETDATA = 0x0089;

    // LocoNet
    public const ushort LAN_LOCONET_Z21_RX = 0x00A0;
    public const ushort LAN_LOCONET_Z21_TX = 0x00A1;
    public const ushort LAN_LOCONET_FROM_LAN = 0x00A2;
    public const ushort LAN_LOCONET_DISPATCH_ADDR = 0x00A3;
    public const ushort LAN_LOCONET_DETECTOR = 0x00A4;

    // Booster
    public const ushort LAN_BOOSTER_SET_POWER = 0x00B2;
    public const ushort LAN_BOOSTER_GET_DESCRIPTION = 0x00B8;
    public const ushort LAN_BOOSTER_SET_DESCRIPTION = 0x00B9;
    public const ushort LAN_BOOSTER_SYSTEMSTATE_DATACHANGED = 0x00BA;
    public const ushort LAN_BOOSTER_SYSTEMSTATE_GETDATA = 0x00BB;

    // CAN
    public const ushort LAN_CAN_DETECTOR = 0x00C4;
    public const ushort LAN_CAN_DEVICE_GET_DESCRIPTION = 0x00C8;
    public const ushort LAN_CAN_DEVICE_SET_DESCRIPTION = 0x00C9;
    public const ushort LAN_CAN_BOOSTER_SYSTEMSTATE_CHGD = 0x00CA;
    public const ushort LAN_CAN_BOOSTER_SET_TRACKPOWER = 0x00CB;

    // Fast Clock
    public const ushort LAN_FAST_CLOCK_CONTROL = 0x00CC;
    public const ushort LAN_FAST_CLOCK_DATA = 0x00CD;
    public const ushort LAN_FAST_CLOCK_SETTINGS_GET = 0x00CE;
    public const ushort LAN_FAST_CLOCK_SETTINGS_SET = 0x00CF;

    // Decoder
    public const ushort LAN_DECODER_GET_DESCRIPTION = 0x00D8;
    public const ushort LAN_DECODER_SET_DESCRIPTION = 0x00D9;
    public const ushort LAN_DECODER_SYSTEMSTATE_DATACHANGED = 0x00DA;
    public const ushort LAN_DECODER_SYSTEMSTATE_GETDATA = 0x00DB;

    // zLink
    public const ushort LAN_ZLINK_GET_HWINFO = 0x00E8;
}
```

---

## Summary

This command reference provides a complete overview of:

- **67 Client→Z21 commands** covering all aspects of train control
- **42 Z21→Client responses** including broadcasts and replies
- **Device compatibility** for 9 different Z21/zLink device types
- **Special notes** for activation codes, virtual LocoNet, and firmware versions

Use this reference to quickly determine:
- Which commands are available for your device
- The correct header and parameter structure
- Compatibility requirements and limitations
- Special behaviors for specific device types
