using Tellurian.Trains.Interfaces.Accessories;

namespace Tellurian.Trains.Protocols.XpressNet;

internal static class AccessoryAddressExtensions
{
    extension(Address address)
    {
        public byte Group => (byte)((address.Number - 1) / 4);

        public byte Subaddress => (byte)(address.Number % 4);

        public byte[] GetBytes()
        {
            var result = new byte[2];
            var bytes = BitConverter.GetBytes((short)(address.Number - 1));
            result[0] = bytes[1];
            result[1] = bytes[0];
            return result;
        }
    }
}
