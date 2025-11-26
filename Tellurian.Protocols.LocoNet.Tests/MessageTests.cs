using Tellurian.Trains.Protocols.LocoNet;

namespace ins.Tellurian.Protocols.LocoNet.Tests;

[TestClass]
public class MessageTests
{
    [TestMethod]
    public void Checksum_CalculatesCorrectValue()
    {
        Assert.AreEqual(0x45, Message.Checksum(TestNotification.Data));
    }

    [TestMethod]
    public void AppendChecksum_AppendsCorrectValue()
    {
        var data = new byte[] { 0xBA, 0x00, 0x00 };
        var actual = Message.AppendChecksum(data);
        Assert.HasCount(4, actual);
        Assert.AreEqual(0x45, actual.Last());
    }
}

internal static class TestNotification
{
    public const byte OperationCode = 0xBA;
    public static byte[] Data => new byte[] { 0xBA, 0x00, 0x00, 0x45 };
}
