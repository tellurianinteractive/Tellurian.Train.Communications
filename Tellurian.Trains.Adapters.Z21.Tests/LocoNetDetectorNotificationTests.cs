namespace Tellurian.Trains.Adapters.Z21.Tests;

[TestClass]
public class LocoNetDetectorNotificationTests
{
    private static LocoNetDetectorNotification CreateNotification(params byte[] data) =>
        new(new Frame(FrameHeader.LocoNetDetector, data));

    [TestMethod]
    public void ParsesOccupancyOccupied()
    {
        // Type=0x01, Address=5 (LE), Occupied=1
        var n = CreateNotification(0x01, 0x05, 0x00, 0x01);
        Assert.AreEqual((byte)0x01, n.DetectorType);
        Assert.AreEqual((ushort)5, n.FeedbackAddress);
        Assert.IsTrue(n.IsOccupied);
    }

    [TestMethod]
    public void ParsesOccupancyFree()
    {
        var n = CreateNotification(0x01, 0x05, 0x00, 0x00);
        Assert.IsFalse(n.IsOccupied);
    }

    [TestMethod]
    public void ParsesTransponderEntering()
    {
        // Type=0x02, Address=10 (LE), Transponder address=300 (LE, 14-bit)
        byte addrLo = (byte)(300 & 0xFF);
        byte addrHi = (byte)((300 >> 8) & 0x3F);
        var n = CreateNotification(0x02, 0x0A, 0x00, addrLo, addrHi);
        Assert.IsTrue(n.IsTransponder);
        Assert.IsTrue(n.IsEntering);
        Assert.AreEqual((ushort)300, n.TransponderAddress);
    }

    [TestMethod]
    public void ParsesTransponderLeaving()
    {
        var n = CreateNotification(0x03, 0x0A, 0x00, 0x05, 0x00);
        Assert.IsTrue(n.IsTransponder);
        Assert.IsFalse(n.IsEntering);
        Assert.AreEqual((ushort)5, n.TransponderAddress);
    }

    [TestMethod]
    public void ParsesLissyLocoAddress()
    {
        // Type=0x10, Address=1, LocoAddr=42 (LE, 14-bit), direction bits in high byte
        byte locoLo = 42;
        byte locoHi = 0xC0; // HasDirection=1 (bit 7), IsForward=1 (bit 6)
        var n = CreateNotification(0x10, 0x01, 0x00, locoLo, locoHi);
        Assert.IsTrue(n.IsLissy);
        Assert.AreEqual((ushort)42, n.LocoAddress);
        Assert.IsTrue(n.HasDirection);
        Assert.IsTrue(n.IsForward);
    }

    [TestMethod]
    public void ParsesLissyBlockStatus()
    {
        var n = CreateNotification(0x11, 0x01, 0x00, 0x01);
        Assert.AreEqual((byte)0x11, n.DetectorType);
        Assert.IsTrue(n.IsOccupied);
    }

    [TestMethod]
    public void ParsesLissySpeed()
    {
        // Type=0x12, Address=1, Speed=120 (LE)
        var n = CreateNotification(0x12, 0x01, 0x00, 0x78, 0x00);
        Assert.AreEqual((ushort)120, n.Speed);
    }
}
