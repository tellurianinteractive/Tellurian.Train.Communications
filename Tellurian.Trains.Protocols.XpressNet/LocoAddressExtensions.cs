using Tellurian.Trains.Interfaces.Locos;

namespace Tellurian.Trains.Protocols.XpressNet;

public static class LocoAddressExtensions
{
    extension(Address locoAddress)
    {
        /// <summary>
        /// Gets two-byte loco address according to XpressNet specification.
        /// </summary>
        /// <remarks>
        /// Z21 ignores the two high bits required for long addresses.
        /// </remarks>
        public byte[] GetBytesAccordingToXpressNet()
        {
            var result = new byte[2];
            var a = BitConverter.GetBytes(locoAddress.Number);
            result[0] = a[1];
            if (locoAddress.IsLong) result[0] += 192;
            result[1] = a[0];
            return result;
        }
    }

    /// <summary>
    /// Creates a LocoAddress from XpressNet-encoded bytes.
    /// </summary>
    /// <param name="high">High byte in XpressNet format (bits 6-7 set for long addresses, bits 0-5 are high address bits).</param>
    /// <param name="low">Low byte (bits 0-7 of address).</param>
    /// <returns>Decoded locomotive address.</returns>
    public static Address FromXpressNet(byte high, byte low)
    {
        return Address.From(((high & 0x3F) << 8) | low);
    }

    /// <summary>
    /// Creates a LocoAddress from XpressNet-encoded byte array.
    /// </summary>
    /// <param name="data">Two bytes in XpressNet format: [AH, AL].</param>
    /// <returns>Decoded locomotive address.</returns>
    public static Address FromXpressNet(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length != 2) throw new ArgumentOutOfRangeException(nameof(data), "Data must contain 2 bytes.");
        return FromXpressNet(data[0], data[1]);
    }
}
