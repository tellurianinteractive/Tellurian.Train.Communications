using Tellurian.Trains.Protocols.XpressNet.Decoder;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class ServiceModeReadDirectCommandTests
{
    [TestMethod]
    public void DirectModeRead_ReturnsCorrectBytes_ForCV1()
    {
        var target = new ServiceModeReadDirectCommand(1);
        var data = target.GetData();
        Assert.AreEqual(0x22, data[0]); // Header with length 2
        Assert.AreEqual(0x15, data[1]); // Identification for Direct Mode
        Assert.AreEqual(0x01, data[2]); // CV 1
    }

    [TestMethod]
    public void DirectModeRead_ReturnsCorrectBytes_ForCV256()
    {
        var target = new ServiceModeReadDirectCommand(256);
        var data = target.GetData();
        Assert.AreEqual(0x22, data[0]);
        Assert.AreEqual(0x15, data[1]);
        Assert.AreEqual(0x00, data[2]); // CV 256 is encoded as 0x00
    }

    [TestMethod]
    public void DirectModeRead_ReturnsCorrectBytes_ForCV128()
    {
        var target = new ServiceModeReadDirectCommand(128);
        var data = target.GetData();
        Assert.AreEqual(0x22, data[0]);
        Assert.AreEqual(0x15, data[1]);
        Assert.AreEqual(0x80, data[2]);
    }

    [TestMethod]
    public void DirectModeRead_Throws_WhenCVIsZero()
    {
        try
        {
            _ = new ServiceModeReadDirectCommand(0);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void DirectModeRead_Throws_WhenCVIsTooHigh()
    {
        try
        {
            _ = new ServiceModeReadDirectCommand(257);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }
}
