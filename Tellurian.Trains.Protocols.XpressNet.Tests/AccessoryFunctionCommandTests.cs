using Tellurian.Trains.Communications.Interfaces.Accessories;
using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class AccessoryFunctionCommandTests
{

    [TestMethod]
    public void GetData_ReturnsCorrectBytes_ForStandardCommand()
    {
        var target = new AccessoryFunctionCommand(Tellurian.Trains.Communications.Interfaces.Accessories.Address.From(225), AccessoryOutput.Port1, AccessoryOutputState.On);
        var data = target.GetData();
        Assert.AreEqual(0x52, data[0]);
        Assert.AreEqual(225 / 4, data[1]);
        Assert.AreEqual(0x82, data[2]);
    }

    [TestMethod]
    public void GetData_ReturnsCorrectBytes_ForZ21QueuedMode()
    {
        var target = new AccessoryFunctionCommand(Tellurian.Trains.Communications.Interfaces.Accessories.Address.From(225), AccessoryOutput.Port1, AccessoryOutputState.On, AccessoryZ21Mode.Queued);
        var data = target.GetData();
        Assert.AreEqual(0x53, data[0]);
        Assert.AreEqual(0x00, data[1]);
        Assert.AreEqual(224, data[2]);
        Assert.AreEqual(0xA8, data[3]);
    }
}
