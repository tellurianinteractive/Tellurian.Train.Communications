using Tellurian.Trains.Interfaces.Accessories;

namespace Tellurian.Trains.Protocols.LocoNet;

/// <summary>
/// LocoNet-specific extensions for <see cref="AccessoryAddress"/>.
/// </summary>
public static class AccessoryAddressExtensions
{
    extension(AccessoryAddress address)
    {
        /// <summary>
        /// Encodes this address into SW1 and SW2 bytes for LocoNet switch commands.
        /// </summary>
        /// <param name="direction">Direction/function: Closed/Green or Thrown/Red</param>
        /// <param name="output">Output state: On or Off</param>
        /// <returns>Tuple of (sw1, sw2) bytes</returns>
        public (byte sw1, byte sw2) EncodeSwitchBytes(AccessoryFunction direction, OutputState output)
        {
            byte sw1 = (byte)(address.Number & 0x7F);
            byte sw2 = (byte)((address.Number >> 7) & 0x0F);

            if (direction == AccessoryFunction.ClosedOrGreen)
                sw2 |= 0x20;

            if (output == OutputState.On)
                sw2 |= 0x10;

            return (sw1, sw2);
        }
    }

    /// <summary>
    /// Decodes an accessory address from SW1 and SW2 bytes.
    /// </summary>
    /// <param name="lowBits">Low 7 bits of address</param>
    /// <param name="highBits">High 4 bits of address plus control bits</param>
    /// <param name="direction">Decoded direction/function</param>
    /// <param name="output">Decoded output state</param>
    /// <returns>Decoded accessory address</returns>
    public static AccessoryAddress DecodeSwitchBytes(byte lowBits, byte highBits, out AccessoryFunction direction, out OutputState output)
    {
        short address = (short)(lowBits | ((highBits & 0x0F) << 7));
        direction = (highBits & 0x20) != 0 ? AccessoryFunction.ClosedOrGreen : AccessoryFunction.ThrownOrRed;
        output = (highBits & 0x10) != 0 ? OutputState.On : OutputState.Off;

        return AccessoryAddress.From(address);
    }
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
