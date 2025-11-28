using Tellurian.Trains.Protocols.XpressNet.Decoder;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class ServiceModeWriteDirectCommandTests
{
    [TestMethod]
    public void DirectModeWrite_ReturnsCorrectBytes_ForCV1()
    {
        var target = new ServiceModeWriteDirectCommand(1, 0x03);
        var data = target.GetData();
        Assert.AreEqual(0x23, data[0]); // Header with length 3
        Assert.AreEqual(0x16, data[1]); // Identification for Direct Mode
        Assert.AreEqual(0x01, data[2]); // CV 1
        Assert.AreEqual(0x03, data[3]); // Value
    }

    [TestMethod]
    public void DirectModeWrite_ReturnsCorrectBytes_ForCV256()
    {
        var target = new ServiceModeWriteDirectCommand(256, 0xFF);
        var data = target.GetData();
        Assert.AreEqual(0x23, data[0]);
        Assert.AreEqual(0x16, data[1]);
        Assert.AreEqual(0x00, data[2]); // CV 256 is encoded as 0x00
        Assert.AreEqual(0xFF, data[3]);
    }

    [TestMethod]
    public void DirectModeWrite_Throws_WhenCVIsZero()
    {
        try
        {
            _ = new ServiceModeWriteDirectCommand(0, 0x00);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void DirectModeWrite_Throws_WhenCVIsTooHigh()
    {
        try
        {
            _ = new ServiceModeWriteDirectCommand(257, 0x00);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }
}
