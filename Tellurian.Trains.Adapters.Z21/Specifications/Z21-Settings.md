# Z21 LAN Protocol - Settings

**Document Version:** 1.13 (English)
**Date:** November 6, 2023

This section covers persistent settings stored in the Z21 for locomotive and accessory decoder configurations.

## Overview

### Persistent Storage

All settings described here are **stored persistently** in the Z21 and survive power cycles.

### Factory Reset

Settings can be reset to factory defaults by:
1. Press and hold the **STOP** button on the Z21
2. Keep holding until the LEDs **flash violet**
3. Release the button

### Storage Limits

- **Maximum 256 different locomotive addresses** can have custom settings
- **Maximum 256 different accessory decoder addresses** can have custom settings
- Addresses **>= 256** are automatically **DCC format** and cannot be changed

---

## Locomotive Format Settings

### 3.1 LAN_GET_LOCOMODE

Reads the output format (DCC or MM/Motorola) for a specific locomotive address.

The Z21 persistently stores the output format for each locomotive address. Each address >= 256 is automatically DCC.

**Request to Z21:**

| DataLen | Header | Data |
|---------|--------|------|
| 0x06 0x00 | 0x60 0x00 | 16-bit Loco Address (**big-endian**) |

**Reply from Z21:**

| DataLen | Header | Data ||
|---------|--------|------|------|
| 0x07 0x00 | 0x60 0x00 | 16-bit Loco Address (**big-endian**) | Mode (8-bit) |

**Mode Values:**

| Value | Format |
|-------|--------|
| 0 | DCC Format |
| 1 | MM Format (Motorola) |

**Important:** Locomotive addresses use **big-endian** byte order (high byte first, then low byte), which is different from most other Z21 commands.

#### C# Implementation

```csharp
public enum LocoFormat : byte
{
    DCC = 0,
    MM = 1  // Motorola
}

public async Task<LocoFormat> GetLocoModeAsync(ushort locoAddress)
{
    // Big-endian: high byte first!
    byte[] request =
    [
        0x06, 0x00, 0x60, 0x00,
        (byte)((locoAddress >> 8) & 0xFF),  // High byte
        (byte)(locoAddress & 0xFF)          // Low byte
    ];

    byte[] response = await SendAndReceiveAsync(request);

    // Response format: [DataLen(2)] [Header(2)] [Address(2)] [Mode(1)]
    return (LocoFormat)response[6];
}

// Example usage
var format = await GetLocoModeAsync(3);  // Get format for loco address 3
if (format == LocoFormat.MM)
{
    Console.WriteLine("Locomotive 3 uses Motorola format");
}
```

---

### 3.2 LAN_SET_LOCOMODE

Sets the output format for a specific locomotive address. The format is stored persistently in the Z21.

**Request to Z21:**

| DataLen | Header | Data ||
|---------|--------|------|------|
| 0x07 0x00 | 0x61 0x00 | 16-bit Loco Address (**big-endian**) | Mode (8-bit) |

**Reply:** None

**Important Notes:**

- Each locomotive address **>= 256** is and remains **DCC format** automatically (cannot be changed)
- **Speed steps** (14, 28, 128) are also stored persistently, but this happens automatically when sending loco driving commands (see `LAN_X_SET_LOCO_DRIVE`)

#### C# Implementation

```csharp
public async Task SetLocoModeAsync(ushort locoAddress, LocoFormat format)
{
    if (locoAddress >= 256)
    {
        throw new ArgumentException(
            "Addresses >= 256 are always DCC and cannot be changed",
            nameof(locoAddress));
    }

    // Big-endian: high byte first!
    byte[] request =
    [
        0x07, 0x00, 0x61, 0x00,
        (byte)((locoAddress >> 8) & 0xFF),  // High byte
        (byte)(locoAddress & 0xFF),         // Low byte
        (byte)format
    ];

    await SendAsync(request);
    // No response expected
}

// Example usage
await SetLocoModeAsync(3, LocoFormat.MM);   // Set loco 3 to Motorola
await SetLocoModeAsync(42, LocoFormat.DCC); // Set loco 42 to DCC
```

---

## Accessory Decoder Format Settings

### 3.3 LAN_GET_TURNOUTMODE

Reads the output format settings for a specific accessory decoder address (per RP-9.2.1 "Accessory Decoder" standard).

The Z21 persistently stores the output format for each accessory decoder address. Each address >= 256 is automatically DCC.

**Request to Z21:**

| DataLen | Header | Data |
|---------|--------|------|
| 0x06 0x00 | 0x70 0x00 | 16-bit Accessory Decoder Address (**big-endian**) |

**Reply from Z21:**

| DataLen | Header | Data ||
|---------|--------|------|------|
| 0x07 0x00 | 0x70 0x00 | 16-bit Accessory Decoder Address (**big-endian**) | Mode (8-bit) |

**Mode Values:**

| Value | Format |
|-------|--------|
| 0 | DCC Format |
| 1 | MM Format (Motorola) |

**Address Indexing:**

At the LAN interface and internally in the Z21, accessory decoder addresses are **indexed from 0**. However, in user interfaces (apps, multiMaus), they are **displayed starting from 1**.

**Example:**
- multiMaus shows "Switch address #3"
- Corresponds to **address 2** on LAN and in Z21
- Formula: `LAN_Address = Display_Address - 1`

#### C# Implementation

```csharp
public async Task<LocoFormat> GetTurnoutModeAsync(ushort turnoutAddress)
{
    // Big-endian: high byte first!
    byte[] request =
    [
        0x06, 0x00, 0x70, 0x00,
        (byte)((turnoutAddress >> 8) & 0xFF),  // High byte
        (byte)(turnoutAddress & 0xFF)          // Low byte
    ];

    byte[] response = await SendAndReceiveAsync(request);

    return (LocoFormat)response[6];
}

// Helper method to convert display address to LAN address
public static ushort DisplayToLanAddress(ushort displayAddress)
{
    if (displayAddress == 0)
    {
        throw new ArgumentException("Display addresses start from 1", nameof(displayAddress));
    }
    return (ushort)(displayAddress - 1);
}

// Helper method to convert LAN address to display address
public static ushort LanToDisplayAddress(ushort lanAddress)
{
    return (ushort)(lanAddress + 1);
}

// Example usage
ushort lanAddr = DisplayToLanAddress(3);  // multiMaus switch #3 = LAN address 2
var format = await GetTurnoutModeAsync(lanAddr);
Console.WriteLine($"Switch #3 (LAN addr {lanAddr}) uses {format} format");
```

---

### 3.4 LAN_SET_TURNOUTMODE

Sets the output format for a specific accessory decoder address. The format is stored persistently in the Z21.

**Request to Z21:**

| DataLen | Header | Data ||
|---------|--------|------|------|
| 0x07 0x00 | 0x71 0x00 | 16-bit Accessory Decoder Address (**big-endian**) | Mode (8-bit) |

**Reply:** None

**Firmware Requirements:**

- **MM accessory decoders** are supported by Z21 firmware **version 1.20 and higher**
- **MM accessory decoders are NOT supported** by SmartRail

**Important:**

- Each accessory decoder address **>= 256** is and remains **DCC format** automatically (cannot be changed)

#### C# Implementation

```csharp
public async Task SetTurnoutModeAsync(ushort turnoutAddress, LocoFormat format)
{
    if (turnoutAddress >= 256)
    {
        throw new ArgumentException(
            "Addresses >= 256 are always DCC and cannot be changed",
            nameof(turnoutAddress));
    }

    // Big-endian: high byte first!
    byte[] request =
    [
        0x07, 0x00, 0x71, 0x00,
        (byte)((turnoutAddress >> 8) & 0xFF),  // High byte
        (byte)(turnoutAddress & 0xFF),         // Low byte
        (byte)format
    ];

    await SendAsync(request);
    // No response expected
}

// Example usage with display addresses
async Task ConfigureTurnoutFromDisplayAddress(ushort displayAddress, LocoFormat format)
{
    ushort lanAddress = DisplayToLanAddress(displayAddress);
    await SetTurnoutModeAsync(lanAddress, format);
    Console.WriteLine($"Set switch #{displayAddress} (LAN addr {lanAddress}) to {format}");
}

await ConfigureTurnoutFromDisplayAddress(3, LocoFormat.MM);  // Set display switch #3 to MM
```

---

## Complete Helper Class

Here's a complete C# helper class for managing Z21 settings:

```csharp
public class Z21Settings
{
    private readonly IZ21Client _client;

    public Z21Settings(IZ21Client client)
    {
        _client = client;
    }

    // Locomotive format management
    public async Task<LocoFormat> GetLocoFormatAsync(ushort address)
    {
        if (address >= 256)
        {
            return LocoFormat.DCC; // Always DCC for addresses >= 256
        }

        byte[] request =
        [
            0x06, 0x00, 0x60, 0x00,
            (byte)((address >> 8) & 0xFF),
            (byte)(address & 0xFF)
        ];

        byte[] response = await _client.SendAndReceiveAsync(request);
        return (LocoFormat)response[6];
    }

    public async Task SetLocoFormatAsync(ushort address, LocoFormat format)
    {
        if (address >= 256)
        {
            throw new InvalidOperationException(
                "Cannot set format for addresses >= 256 (always DCC)");
        }

        byte[] request =
        [
            0x07, 0x00, 0x61, 0x00,
            (byte)((address >> 8) & 0xFF),
            (byte)(address & 0xFF),
            (byte)format
        ];

        await _client.SendAsync(request);
    }

    // Turnout/Accessory format management
    public async Task<LocoFormat> GetTurnoutFormatAsync(ushort address)
    {
        if (address >= 256)
        {
            return LocoFormat.DCC; // Always DCC for addresses >= 256
        }

        byte[] request =
        [
            0x06, 0x00, 0x70, 0x00,
            (byte)((address >> 8) & 0xFF),
            (byte)(address & 0xFF)
        ];

        byte[] response = await _client.SendAndReceiveAsync(request);
        return (LocoFormat)response[6];
    }

    public async Task SetTurnoutFormatAsync(ushort address, LocoFormat format)
    {
        if (address >= 256)
        {
            throw new InvalidOperationException(
                "Cannot set format for addresses >= 256 (always DCC)");
        }

        byte[] request =
        [
            0x07, 0x00, 0x71, 0x00,
            (byte)((address >> 8) & 0xFF),
            (byte)(address & 0xFF),
            (byte)format
        ];

        await _client.SendAsync(request);
    }

    // Address conversion helpers
    public static ushort TurnoutDisplayToLan(ushort displayAddress)
    {
        if (displayAddress == 0)
        {
            throw new ArgumentException(
                "Display addresses start from 1",
                nameof(displayAddress));
        }
        return (ushort)(displayAddress - 1);
    }

    public static ushort TurnoutLanToDisplay(ushort lanAddress)
    {
        return (ushort)(lanAddress + 1);
    }

    // Batch configuration
    public async Task ConfigureLocomotivesAsync(
        IEnumerable<(ushort Address, LocoFormat Format)> configs)
    {
        foreach (var (address, format) in configs)
        {
            await SetLocoFormatAsync(address, format);
            await Task.Delay(10); // Small delay between commands
        }
    }

    public async Task ConfigureTurnoutsAsync(
        IEnumerable<(ushort Address, LocoFormat Format)> configs)
    {
        foreach (var (address, format) in configs)
        {
            await SetTurnoutFormatAsync(address, format);
            await Task.Delay(10); // Small delay between commands
        }
    }
}

// Example usage
var settings = new Z21Settings(z21Client);

// Configure some locomotives
await settings.ConfigureLocomotivesAsync(
[
    (3, LocoFormat.MM),    // Old Märklin loco
    (42, LocoFormat.DCC),  // Modern DCC loco
    (78, LocoFormat.MM)    // Another Motorola loco
]);

// Configure turnouts using LAN addresses
await settings.SetTurnoutFormatAsync(0, LocoFormat.MM);   // LAN addr 0 = display #1
await settings.SetTurnoutFormatAsync(1, LocoFormat.DCC);  // LAN addr 1 = display #2

// Or using display addresses
ushort lanAddr = Z21Settings.TurnoutDisplayToLan(3);  // Display #3 → LAN addr 2
await settings.SetTurnoutFormatAsync(lanAddr, LocoFormat.MM);
```

---

## Summary

### Key Points

1. **Persistent Storage**: All settings are stored in non-volatile memory
2. **Factory Reset**: Hold STOP button until LEDs flash violet
3. **Address Limits**: Maximum 256 custom settings; addresses >= 256 are always DCC
4. **Big-Endian**: Both loco and turnout addresses use **big-endian** byte order (unusual for Z21)
5. **Address Indexing**: Turnout LAN addresses start from 0, but displays show them starting from 1

### Format Support

| Format | Locomotives | Accessories |
|--------|-------------|-------------|
| **DCC** | All Z21 models | All Z21 models |
| **MM (Motorola)** | All Z21 models | Z21 FW 1.20+, not SmartRail |

### Additional Persistent Settings

- **Speed steps** (14, 28, 128) are also stored persistently for each locomotive
- These are automatically saved when sending loco driving commands (see `LAN_X_SET_LOCO_DRIVE`)

### Comparison Table

| Command | Header | Direction | Purpose |
|---------|--------|-----------|---------|
| `LAN_GET_LOCOMODE` | 0x60 | Request | Read locomotive format |
| `LAN_SET_LOCOMODE` | 0x61 | Request | Set locomotive format |
| `LAN_GET_TURNOUTMODE` | 0x70 | Request | Read turnout/accessory format |
| `LAN_SET_TURNOUTMODE` | 0x71 | Request | Set turnout/accessory format |
