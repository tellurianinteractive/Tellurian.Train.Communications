using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class SetFunctionStateGroup2CommandTests
{
    [TestMethod]
    public void SetFunctionStateGroup2_ReturnsCorrectBytes_AllOnOff()
    {
        var target = new SetFunctionStateGroup2Command(Address.From(3), true, true, true, true);
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);
        Assert.AreEqual(0x25, data[1]);
        Assert.AreEqual(0x0F, data[4]);
    }

    [TestMethod]
    public void SetFunctionStateGroup2_ReturnsCorrectBytes_F5OnlyOnOff()
    {
        var target = new SetFunctionStateGroup2Command(Address.From(3), true, false, false, false);
        var data = target.GetData();

        Assert.AreEqual(0x01, data[4]);
    }
}
