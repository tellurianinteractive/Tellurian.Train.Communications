using Tellurian.Trains.Interfaces.Accessories;

namespace Tellurian.Trains.Protocols.XpressNet;

internal static class AccessoryAddressExtensions
{
    extension(AccessoryAddress address)
    {
        public byte Group => (byte)((address.Number - 1) / 4);

        public byte Subaddress => (byte)(address.Number % 4);

        public byte[] GetBytes()
        {
            var result = new byte[2];
            var a = BitConverter.GetBytes((short)(address.Number - 1));
            result[0] = a[1];
            result[1] = a[0];
            return result;
        }
    }
}
