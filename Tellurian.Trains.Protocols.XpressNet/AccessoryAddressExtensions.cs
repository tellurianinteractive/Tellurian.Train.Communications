using Tellurian.Trains.Communications.Interfaces.Accessories;

namespace Tellurian.Trains.Protocols.XpressNet;

internal static class AccessoryAddressExtensions
{
    extension(Address address)
    {
        public byte Group => (byte)(address.WireAddress / 4);

        public byte Subaddress => (byte)(address.Number % 4);

        public byte[] GetBytes()
        {
            var result = new byte[2];
            var bytes = BitConverter.GetBytes(address.WireAddress);
            result[0] = bytes[1];
            result[1] = bytes[0];
            return result;
        }
    }
}
