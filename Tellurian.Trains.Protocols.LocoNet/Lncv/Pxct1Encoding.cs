namespace Tellurian.Trains.Protocols.LocoNet.Lncv;

/// <summary>
/// PXCT1 high-bit encoding/decoding for LocoNet peer transfer messages.
/// LocoNet data bytes carry only 7 bits (MSB must be 0). PXCT1 stores the 8th bit of each data byte.
/// </summary>
internal static class Pxct1Encoding
{
    /// <summary>
    /// Encodes data bytes by extracting bit 7 from each byte into a PXCT1 byte.
    /// Modifies the data bytes in-place by clearing their bit 7.
    /// </summary>
    /// <param name="dataBytes">Up to 7 data bytes to encode. Modified in-place.</param>
    /// <returns>The PXCT1 byte containing the extracted high bits.</returns>
    public static byte Encode(Span<byte> dataBytes)
    {
        byte pxct1 = 0;
        for (var i = 0; i < dataBytes.Length && i < 7; i++)
        {
            if ((dataBytes[i] & 0x80) != 0)
            {
                pxct1 |= (byte)(1 << i);
                dataBytes[i] &= 0x7F;
            }
        }
        return pxct1;
    }

    /// <summary>
    /// Decodes data bytes by restoring bit 7 from the PXCT1 byte.
    /// Modifies the data bytes in-place.
    /// </summary>
    /// <param name="pxct1">The PXCT1 byte containing the high bits.</param>
    /// <param name="dataBytes">Up to 7 data bytes to decode. Modified in-place.</param>
    public static void Decode(byte pxct1, Span<byte> dataBytes)
    {
        for (var i = 0; i < dataBytes.Length && i < 7; i++)
        {
            if ((pxct1 & (1 << i)) != 0)
            {
                dataBytes[i] |= 0x80;
            }
        }
    }
}
