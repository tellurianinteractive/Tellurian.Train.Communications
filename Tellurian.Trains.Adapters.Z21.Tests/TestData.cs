namespace Tellurian.Trains.Adapters.Z21.Tests;

internal static class TestData
{
    public static byte[] CreateTestBuffer(FrameHeader header, int value)
    {
        const ushort length = 8;
        var result = new List<byte>();
        result.AddRange(BitConverter.GetBytes(length));
        result.AddRange(BitConverter.GetBytes((ushort)header));
        result.AddRange(BitConverter.GetBytes(value));
        return result.ToArray();
    }

    public static byte[] CreateManyTestBuffer(FrameHeader header)
    {
        return Enumerable.Range(1, 3).Select(i => CreateTestBuffer(header, i)).SelectMany(b => b).ToArray();
    }
}
