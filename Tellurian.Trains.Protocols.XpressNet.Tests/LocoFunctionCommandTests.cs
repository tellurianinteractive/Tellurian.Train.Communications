using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class LocoFunctionCommandTests
{
    [TestMethod]
    public void GetData_ReturnsCorrectBytes_WhenShortAddress()
    {
        var target = new LocoFunctionCommand(new LocoAddress(99), 1, LocoFunctionStates.On);
        var data = target.GetData();
        Assert.AreEqual(0xE0 + 4, data[0]);
        Assert.AreEqual(0xF8, data[1]);
        Assert.AreEqual(99, ((data[2] & 0x3F) << 8) + data[3]);
        Assert.AreEqual((byte)(LocoFunctionStates.On + 1), data[4]);
    }

    [TestMethod]
    public void GetData_ReturnsCorrectBytes_WhenLongAddress()
    {
        var target = new LocoFunctionCommand(new LocoAddress(9999), 1, LocoFunctionStates.On);
        var data = target.GetData();
        Assert.AreEqual(0xE0 + 4, data[0]);
        Assert.AreEqual(0xF8, data[1]);
        Assert.AreEqual(9999, ((data[2] & 0x3F) << 8) + data[3]);
        Assert.AreEqual(0xC0, data[2] & 0xC0);
        Assert.AreEqual((byte)(LocoFunctionStates.On + 1), data[4]);
    }

    [TestMethod]
    public void LocoEmergencyStopCommand_GetData_ReturnsCorrectBytes()
    {
        var target = new LocoEmergencyStopCommand(new LocoAddress(99));
        var data = target.GetData();
        Assert.AreEqual(0x92, data[0]);
        Assert.AreEqual(0x00, data[1]);
        Assert.AreEqual(99, data[2]);
    }
}

