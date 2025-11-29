using Tellurian.Trains.Interfaces.Accessories;

namespace Tellurian.Trains.Protocols.LocoNet;

internal static class AccessoryAddressExtensions
{
    extension(Address address)
    {
        public (byte sw1, byte sw2) EncodeSwitchBytes(Position direction, MotorState output)
        {
            byte sw1 = (byte)(address.Number & 0x7F);
            byte sw2 = (byte)((address.Number >> 7) & 0x0F);

            if (direction == Position.ClosedOrGreen)
                sw2 |= 0x20;

            if (output == MotorState.On)
                sw2 |= 0x10;

            return (sw1, sw2);
        }
    }

    public static Address DecodeSwitchBytes(byte lowBits, byte highBits, out Position direction, out MotorState output)
    {
        short address = (short)(lowBits | ((highBits & 0x0F) << 7));
        direction = (highBits & 0x20) != 0 ? Position.ClosedOrGreen : Position.ThrownOrRed;
        output = (highBits & 0x10) != 0 ? MotorState.On : MotorState.Off;

        return Address.From(address);
    }
}

/// <summary>
/// LocoNet-specific accessory input port (protocol-level detail).
/// </summary>
internal enum AccessoryInput : byte
{
    Port0 = 0b00,
    Port1 = 0b01,
    Port2 = 0b10,
    Port3 = 0b11
}
