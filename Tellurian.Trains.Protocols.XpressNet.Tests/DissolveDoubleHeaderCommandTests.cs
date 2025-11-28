using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class DissolveDoubleHeaderCommandTests
{
    [TestMethod]
    public void DissolveDoubleHeader_ReturnsCorrectBytes_ForShortAddress()
    {
        var target = new DissolveDoubleHeaderCommand(new LocoAddress(3));
        var data = target.GetData();

        Assert.AreEqual(0xE5, data[0]);
        Assert.AreEqual(0x43, data[1]);
        Assert.AreEqual(0x00, data[2]);
        Assert.AreEqual(0x03, data[3]);
        Assert.AreEqual(0x00, data[4]);
        Assert.AreEqual(0x00, data[5]);
    }

    [TestMethod]
    public void DissolveDoubleHeader_ReturnsCorrectBytes_ForLongAddress()
    {
        var target = new DissolveDoubleHeaderCommand(new LocoAddress(1234));
        var data = target.GetData();

        Assert.AreEqual(0xE5, data[0]);
        Assert.AreEqual(0x43, data[1]);
        Assert.AreEqual(0xC4, data[2]);
        Assert.AreEqual(0xD2, data[3]);
        Assert.AreEqual(0x00, data[4]);
        Assert.AreEqual(0x00, data[5]);
    }
}
