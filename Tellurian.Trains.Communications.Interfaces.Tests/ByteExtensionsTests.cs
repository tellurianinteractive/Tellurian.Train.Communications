using Tellurian.Trains.Communications.Interfaces.Extensions;

namespace Tellurian.Trains.Communications.Interfaces.Tests;

[TestClass]
public class ByteExtensionsTests
{
    [TestMethod]
    public void BitConverter_IsLittleEndian_OnOperatingSystem()
    {
        Assert.IsTrue(BitConverter.IsLittleEndian);
    }

    [TestMethod]
    public void ToInt32LittleEndian_ReadsCorrectValue()
    {
        const int value = 12345678;
        var bytes = BitConverter.GetBytes(value);
        Assert.AreEqual(value, bytes.ToInt32LittleEndian());
    }

    [TestMethod]
    public void ToInt32BigEndian_ReadsCorrectValue()
    {
        const int value = 12345678;
        var span = BitConverter.GetBytes(value).AsSpan();
        span.Reverse();
        var bytes = span.ToArray();
        Assert.AreEqual(value, bytes.ToInt32BigEndian());
    }

    [TestMethod]
    public void Bcd_ConvertsCorrectly()
    {
        Assert.AreEqual(20, ((byte)32).Bcd());
        Assert.AreEqual(33, ((byte)51).Bcd());
    }
}
