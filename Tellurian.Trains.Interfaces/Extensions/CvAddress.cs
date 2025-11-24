namespace Tellurian.Trains.Interfaces.Extensions;

public readonly struct CvAddress : IEquatable<CvAddress>
{
    public CvAddress(byte[] data, int offset = 0)
    {
        if (data is null || data.Length < 2) throw new ArgumentNullException(nameof(data));
        MSB = data[offset + 1];
        LSB = data[offset];
    }
    public CvAddress(ushort number)
    {
        if (number < 1 || number > 1024) throw new ArgumentOutOfRangeException(nameof(number));
        var data = BitConverter.GetBytes(number - 1);
        MSB = data[1];
        LSB = data[0];
    }
    public byte MSB { get; }
    public byte LSB { get; }
    public int Value => (MSB << 8) + LSB + 1;

    public bool Equals(CvAddress other) => other.MSB == MSB && other.LSB == LSB;
    public override bool Equals(object? obj) => obj is CvAddress other && Equals(other);
    public override int GetHashCode() => (MSB.GetHashCode() / 2) + (LSB.GetHashCode() / 2);
    public static bool operator ==(CvAddress left, CvAddress right) => left == right;
    public static bool operator !=(CvAddress left, CvAddress right) => !(left == right);
}

public static class CVAddressExtensions
{
    public static CvAddress CV(this int value) => new ((ushort)value);

}
