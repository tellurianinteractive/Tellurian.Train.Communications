using Tellurian.Trains.Interfaces.Extensions;

namespace Tellurian.Trains.Interfaces.Tests;

[TestClass]
public class ByteExtensionsTests
{
    [TestMethod]
    public void OperatingSystemIsLittleEndian()
    {
        Assert.IsTrue(BitConverter.IsLittleEndian);
    }

    [TestMethod]
    public void ReadsInt32LittleEndian()
    {
        const int value = 12345678;
        var bytes = BitConverter.GetBytes(value);
        Assert.AreEqual(value, bytes.ToInt32LittleEndian());
    }

    [TestMethod]
    public void ReadsInt32BigEndian()
    {
        const int value = 12345678;
        var span = BitConverter.GetBytes(value).AsSpan(); 
        span.Reverse();
        var bytes = span.ToArray();
        Assert.AreEqual(value, bytes.ToInt32BigEndian());
    }

    [TestMethod]
    public void Bcd()
    {
        Assert.AreEqual(20, ((byte)32).Bcd());
        Assert.AreEqual(33, ((byte)51).Bcd());
    }
}
