using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class GetFunctionStatusCommandTests
{
    [TestMethod]
    public void GetFunctionStatus_ReturnsCorrectBytes_ForShortAddress()
    {
        var target = new GetFunctionStatusCommand(new LocoAddress(3));
        var data = target.GetData();

        Assert.AreEqual(0xE3, data[0]);
        Assert.AreEqual(0x07, data[1]);
        Assert.AreEqual(0x00, data[2]);
        Assert.AreEqual(0x03, data[3]);
    }

    [TestMethod]
    public void GetFunctionStatus_ReturnsCorrectBytes_ForLongAddress()
    {
        var target = new GetFunctionStatusCommand(new LocoAddress(1234));
        var data = target.GetData();

        Assert.AreEqual(0xE3, data[0]);
        Assert.AreEqual(0x07, data[1]);
        Assert.AreEqual(0xC4, data[2]);
        Assert.AreEqual(0xD2, data[3]);
    }
}
