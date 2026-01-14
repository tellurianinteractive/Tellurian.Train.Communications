using Tellurian.Trains.Communications.Interfaces.Accessories;
using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class AccessoryInfoRequestCommandTests
{
    [TestMethod]
    public void AccessoryInfoRequest_ReturnsCorrectBytes_ForTurnout1()
    {
        var target = new AccessoryInfoRequestCommand(Tellurian.Trains.Communications.Interfaces.Accessories.Address.From(1));
        var data = target.GetData();

        Assert.AreEqual(0x42, data[0]);
        Assert.AreEqual(0x00, data[1]);
        Assert.AreEqual(0x80, data[2]);
    }

    [TestMethod]
    public void AccessoryInfoRequest_ReturnsCorrectBytes_ForTurnout3()
    {
        var target = new AccessoryInfoRequestCommand(Tellurian.Trains.Communications.Interfaces.Accessories.Address.From(3));
        var data = target.GetData();

        Assert.AreEqual(0x42, data[0]);
        Assert.AreEqual(0x00, data[1]);
        Assert.AreEqual(0x81, data[2]);
    }

    [TestMethod]
    public void AccessoryInfoRequest_ReturnsCorrectBytes_ForTurnout5()
    {
        var target = new AccessoryInfoRequestCommand(Tellurian.Trains.Communications.Interfaces.Accessories.Address.From(5));
        var data = target.GetData();

        Assert.AreEqual(0x42, data[0]);
        Assert.AreEqual(0x01, data[1]);
        Assert.AreEqual(0x80, data[2]);
    }

    [TestMethod]
    public void AccessoryInfoRequest_ReturnsCorrectBytes_ForTurnout100()
    {
        var target = new AccessoryInfoRequestCommand(Tellurian.Trains.Communications.Interfaces.Accessories.Address.From(100));
        var data = target.GetData();

        Assert.AreEqual(0x42, data[0]);
        Assert.AreEqual(24, data[1]);
        Assert.AreEqual(0x80, data[2]);
    }

    [TestMethod]
    public void AccessoryInfoRequest_ReturnsCorrectBytes_ForTurnout1024()
    {
        var target = new AccessoryInfoRequestCommand(Tellurian.Trains.Communications.Interfaces.Accessories.Address.From(1024));
        var data = target.GetData();

        Assert.AreEqual(0x42, data[0]);
        Assert.AreEqual(255, data[1]);
        Assert.AreEqual(0x80, data[2]);
    }

    [TestMethod]
    public void AccessoryInfoRequest_WithGroupAndNibble_ReturnsCorrectBytes()
    {
        var target = new AccessoryInfoRequestCommand(10, true);
        var data = target.GetData();

        Assert.AreEqual(0x42, data[0]);
        Assert.AreEqual(10, data[1]);
        Assert.AreEqual(0x81, data[2]);
    }
}
