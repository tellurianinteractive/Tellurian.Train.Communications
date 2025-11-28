using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class SetFunctionStateGroup3CommandTests
{
    [TestMethod]
    public void SetFunctionStateGroup3_ReturnsCorrectBytes_AllOnOff()
    {
        var target = new SetFunctionStateGroup3Command(new LocoAddress(3), true, true, true, true);
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);
        Assert.AreEqual(0x26, data[1]);
        Assert.AreEqual(0x0F, data[4]);
    }

    [TestMethod]
    public void SetFunctionStateGroup3_ReturnsCorrectBytes_F12OnlyOnOff()
    {
        var target = new SetFunctionStateGroup3Command(new LocoAddress(3), false, false, false, true);
        var data = target.GetData();

        Assert.AreEqual(0x08, data[4]);
    }
}
