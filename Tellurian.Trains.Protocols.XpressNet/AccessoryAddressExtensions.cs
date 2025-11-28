using Tellurian.Trains.Interfaces.Accessories;

namespace Tellurian.Trains.Protocols.XpressNet;

/// <summary>
/// XpressNet-specific extensions for <see cref="AccessoryAddress"/>.
/// </summary>
public static class AccessoryAddressExtensions
{
    extension(AccessoryAddress address)
    {
        /// <summary>
        /// Gets the XpressNet group number for this address.
        /// </summary>
        public byte Group => (byte)((address.Number - 1) / 4);

        /// <summary>
        /// Gets the XpressNet subaddress within the group.
        /// </summary>
        public byte Subaddress => (byte)(address.Number % 4);

        /// <summary>
        /// Gets the XpressNet protocol bytes for this address.
        /// </summary>
        /// <returns>Two-byte array in XpressNet format.</returns>
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
