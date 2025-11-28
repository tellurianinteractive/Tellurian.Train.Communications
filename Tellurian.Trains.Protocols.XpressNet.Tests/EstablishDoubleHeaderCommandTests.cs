using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class EstablishDoubleHeaderCommandTests
{
    [TestMethod]
    public void EstablishDoubleHeader_ReturnsCorrectBytes_ForShortAddresses()
    {
        var target = new EstablishDoubleHeaderCommand(new LocoAddress(3), new LocoAddress(5));
        var data = target.GetData();

        Assert.AreEqual(0xE5, data[0]);
        Assert.AreEqual(0x43, data[1]);
        Assert.AreEqual(0x00, data[2]);
        Assert.AreEqual(0x03, data[3]);
        Assert.AreEqual(0x00, data[4]);
        Assert.AreEqual(0x05, data[5]);
    }

    [TestMethod]
    public void EstablishDoubleHeader_ReturnsCorrectBytes_ForLongAddresses()
    {
        var target = new EstablishDoubleHeaderCommand(new LocoAddress(1234), new LocoAddress(5678));
        var data = target.GetData();

        Assert.AreEqual(0xE5, data[0]);
        Assert.AreEqual(0x43, data[1]);
        Assert.AreEqual(0xC4, data[2]);
        Assert.AreEqual(0xD2, data[3]);
        Assert.AreEqual(0xD6, data[4]);
        Assert.AreEqual(0x2E, data[5]);
    }
}
