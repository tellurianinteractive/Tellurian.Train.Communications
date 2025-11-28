using Tellurian.Trains.Interfaces.Locos;

namespace Tellurian.Trains.Protocols.XpressNet;

public static class LocoAddressExtensions
{
    extension(LocoAddress locoAddress)
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

    extension(LocoAddress)
    {
        /// <summary>
        /// Creates a LocoAddress from XpressNet-encoded bytes.
        /// </summary>
        /// <param name="data">Two bytes in XpressNet format: [AH, AL] where AH has bits 6-7 set for long addresses.</param>
        public static LocoAddress From(byte[] data)
        {
            var span = data.AsSpan();
            span.Reverse();
            var buffer = span.ToArray();
            buffer[1] &= 0x3F;
            return LocoAddress.From(BitConverter.ToInt16(buffer, 0));
        }
    }
}
