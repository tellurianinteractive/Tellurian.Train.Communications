using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class RemoveLocoFromMultiUnitCommandTests
{
    [TestMethod]
    public void RemoveLocoFromMultiUnit_ReturnsCorrectBytes()
    {
        var target = new RemoveLocoFromMultiUnitCommand(new LocoAddress(3), 10);
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);
        Assert.AreEqual(0x42, data[1]);
        Assert.AreEqual(0x00, data[2]);
        Assert.AreEqual(0x03, data[3]);
        Assert.AreEqual(10, data[4]);
    }

    [TestMethod]
    public void RemoveLocoFromMultiUnit_ReturnsCorrectBytes_ForLongAddress()
    {
        var target = new RemoveLocoFromMultiUnitCommand(new LocoAddress(1234), 50);
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);
        Assert.AreEqual(0x42, data[1]);
        Assert.AreEqual(0xC4, data[2]);
        Assert.AreEqual(0xD2, data[3]);
        Assert.AreEqual(50, data[4]);
    }

    [TestMethod]
    public void RemoveLocoFromMultiUnit_Throws_WhenMTRAddressIsZero()
    {
        try
        {
            _ = new RemoveLocoFromMultiUnitCommand(new LocoAddress(3), 0);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }
}
