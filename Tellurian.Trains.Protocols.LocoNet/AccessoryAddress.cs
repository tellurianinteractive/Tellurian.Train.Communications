namespace Tellurian.Trains.Protocols.LocoNet;

/// <summary>
/// Represents a LocoNet accessory (switch/turnout) address with input port selection.
/// Supports 11-bit addressing (0-2047) with 4 ports per decoder (DS54).
/// </summary>
public readonly struct AccessoryAddress : IEquatable<AccessoryAddress>
{
    public AccessoryAddress(ushort address, AccessoryInput input = AccessoryInput.Port0)
    {
        if (address > 2047)
            throw new ArgumentOutOfRangeException(nameof(address), "Accessory address must be 0-2047");

        Value = address;
        Input = input;
    }

    /// <summary>
    /// The accessory address (0-2047).
    /// </summary>
    public ushort Value { get; }

    /// <summary>
    /// The input port selection (0-3) for decoders with multiple outputs.
    /// </summary>
    public AccessoryInput Input { get; }

    /// <summary>
    /// Encodes this address into SW1 and SW2 bytes for LocoNet switch commands.
    /// </summary>
    /// <param name="direction">Direction/function: Closed/Green or Thrown/Red</param>
    /// <param name="output">Output state: On or Off</param>
    /// <returns>Tuple of (sw1, sw2) bytes</returns>
    public (byte sw1, byte sw2) EncodeSwitchBytes(AccessoryFunction direction, OutputState output)
    {
        byte sw1 = (byte)(Value & 0x7F);
        byte sw2 = (byte)((Value >> 7) & 0x0F);

        if (direction == AccessoryFunction.ClosedOrGreen)
            sw2 |= 0x20;

        if (output == OutputState.On)
            sw2 |= 0x10;

        return (sw1, sw2);
    }

    /// <summary>
    /// Decodes an accessory address from SW1 and SW2 bytes.
    /// </summary>
    /// <param name="sw1">Low 7 bits of address</param>
    /// <param name="sw2">High 4 bits of address plus control bits</param>
    /// <returns>Decoded accessory address (direction and output state in separate out parameters)</returns>
    public static AccessoryAddress DecodeSwitchBytes(byte sw1, byte sw2, out AccessoryFunction direction, out OutputState output)
    {
        ushort address = (ushort)(sw1 | ((sw2 & 0x0F) << 7));
        direction = (sw2 & 0x20) != 0 ? AccessoryFunction.ClosedOrGreen : AccessoryFunction.ThrownOrRed;
        output = (sw2 & 0x10) != 0 ? OutputState.On : OutputState.Off;

        return new AccessoryAddress(address);
    }

    public bool Equals(AccessoryAddress other) => other.Value == Value && other.Input == Input;
    public override bool Equals(object? obj) => obj is AccessoryAddress other && Equals(other);
    public override int GetHashCode() => (Value.GetHashCode() / 2) + (Input.GetHashCode() / 2);
    public override string ToString() => $"{Value}:{Input}";
    public static bool operator ==(AccessoryAddress left, AccessoryAddress right) => left.Equals(right);
    public static bool operator !=(AccessoryAddress left, AccessoryAddress right) => !(left == right);
}

public enum AccessoryInput : byte
{
    Port0 = 0b00,
    Port1 = 0b01,
    Port2 = 0b10,
    Port3 = 0b11
}

public enum AccessoryFunction : byte
{
    ClosedOrGreen = 0b0,
    ThrownOrRed = 0b1
}

public enum OutputState : byte
{
    Off = 0b0,
    On = 0b1
}
