using Tellurian.Trains.Protocols.XpressNet.Decoder;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class ServiceModeWriteRegisterCommandTests
{
    [TestMethod]
    public void RegisterModeWrite_ReturnsCorrectBytes_ForRegister1()
    {
        var target = new ServiceModeWriteRegisterCommand(1, 0x55);
        var data = target.GetData();
        Assert.AreEqual(0x23, data[0]); // Header with length 3
        Assert.AreEqual(0x12, data[1]); // Identification
        Assert.AreEqual(0x01, data[2]); // Register 1
        Assert.AreEqual(0x55, data[3]); // Value
    }

    [TestMethod]
    public void RegisterModeWrite_ReturnsCorrectBytes_ForRegister8()
    {
        var target = new ServiceModeWriteRegisterCommand(8, 0xAA);
        var data = target.GetData();
        Assert.AreEqual(0x23, data[0]);
        Assert.AreEqual(0x12, data[1]);
        Assert.AreEqual(0x08, data[2]);
        Assert.AreEqual(0xAA, data[3]);
    }

    [TestMethod]
    public void RegisterModeWrite_Throws_WhenRegisterIsZero()
    {
        try
        {
            _ = new ServiceModeWriteRegisterCommand(0, 0x00);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void RegisterModeWrite_Throws_WhenRegisterIsTooHigh()
    {
        try
        {
            _ = new ServiceModeWriteRegisterCommand(9, 0x00);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }
}
