using Tellurian.Trains.Interfaces.Accessories;

namespace Tellurian.Trains.Protocols.LocoNet;

internal static class AccessoryAddressExtensions
{
    extension(AccessoryAddress address)
    {
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

    public static AccessoryAddress DecodeSwitchBytes(byte lowBits, byte highBits, out AccessoryFunction direction, out OutputState output)
    {
        short address = (short)(lowBits | ((highBits & 0x0F) << 7));
        direction = (highBits & 0x20) != 0 ? AccessoryFunction.ClosedOrGreen : AccessoryFunction.ThrownOrRed;
        output = (highBits & 0x10) != 0 ? OutputState.On : OutputState.Off;

        return AccessoryAddress.From(address);
    }
}

internal enum AccessoryInput : byte
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
