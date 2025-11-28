namespace Tellurian.Trains.Adapters.Z21;

internal sealed class BitConverterExtensions
{
    private BitConverterExtensions() { }

    public static byte[] GetBigEndianBytes(short value)
    {
        var result = BitConverter.GetBytes(value);
        Array.Reverse(result);
        return result;
    }

    public static short GetBigEndianInt16(byte[] data, int offset)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Length - offset < 2) throw new ArgumentOutOfRangeException(nameof(data));
        Array.Reverse(data, offset, sizeof(short));
        return BitConverter.ToInt16(data, offset);
    }
}
