using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class SetFunctionStateGroup1CommandTests
{
    [TestMethod]
    public void SetFunctionStateGroup1_ReturnsCorrectBytes_AllOnOff()
    {
        var target = new SetFunctionStateGroup1Command(Address.From(3), true, true, true, true, true);
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);
        Assert.AreEqual(0x24, data[1]);
        Assert.AreEqual(0x00, data[2]);
        Assert.AreEqual(0x03, data[3]);
        Assert.AreEqual(0x1F, data[4]);
    }

    [TestMethod]
    public void SetFunctionStateGroup1_ReturnsCorrectBytes_AllMomentary()
    {
        var target = new SetFunctionStateGroup1Command(Address.From(3), false, false, false, false, false);
        var data = target.GetData();

        Assert.AreEqual(0x00, data[4]);
    }

    [TestMethod]
    public void SetFunctionStateGroup1_ReturnsCorrectBytes_F0OnlyMomentary()
    {
        var target = new SetFunctionStateGroup1Command(Address.From(3), false, true, true, true, true);
        var data = target.GetData();

        Assert.AreEqual(0x0F, data[4]);
    }

    [TestMethod]
    public void SetFunctionStateGroup1_ReturnsCorrectBytes_WithStateByte()
    {
        var target = new SetFunctionStateGroup1Command(Address.From(3), 0x15);
        var data = target.GetData();

        Assert.AreEqual(0x15, data[4]);
    }
}
