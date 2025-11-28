using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class DeleteLocoFromStackCommandTests
{
    [TestMethod]
    public void DeleteLocoFromStack_ReturnsCorrectBytes_ShortAddress()
    {
        var target = new DeleteLocoFromStackCommand(new LocoAddress(3));
        var data = target.GetData();

        Assert.AreEqual(0xE3, data[0]);
        Assert.AreEqual(0x44, data[1]);
        Assert.AreEqual(0x00, data[2]);
        Assert.AreEqual(0x03, data[3]);
    }

    [TestMethod]
    public void DeleteLocoFromStack_ReturnsCorrectBytes_LongAddress()
    {
        var target = new DeleteLocoFromStackCommand(new LocoAddress(5000));
        var data = target.GetData();

        Assert.AreEqual(0xE3, data[0]);
        Assert.AreEqual(0x44, data[1]);
        Assert.AreEqual(0xD3, data[2]);
        Assert.AreEqual(0x88, data[3]);
    }
}
