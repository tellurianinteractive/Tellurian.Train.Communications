using Tellurian.Trains.Protocols.XpressNet.Decoder;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class ServiceModeReadRegisterCommandTests
{
    [TestMethod]
    public void RegisterModeRead_ReturnsCorrectBytes_ForRegister1()
    {
        var target = new ServiceModeReadRegisterCommand(1);
        var data = target.GetData();
        Assert.AreEqual(0x22, data[0]); // Header with length 2
        Assert.AreEqual(0x11, data[1]); // Identification
        Assert.AreEqual(0x01, data[2]); // Register 1
    }

    [TestMethod]
    public void RegisterModeRead_ReturnsCorrectBytes_ForRegister8()
    {
        var target = new ServiceModeReadRegisterCommand(8);
        var data = target.GetData();
        Assert.AreEqual(0x22, data[0]);
        Assert.AreEqual(0x11, data[1]);
        Assert.AreEqual(0x08, data[2]);
    }

    [TestMethod]
    public void RegisterModeRead_Throws_WhenRegisterIsZero()
    {
        try
        {
            _ = new ServiceModeReadRegisterCommand(0);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void RegisterModeRead_Throws_WhenRegisterIsTooHigh()
    {
        try
        {
            _ = new ServiceModeReadRegisterCommand(9);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }
}
