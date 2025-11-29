using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class AddLocoToMultiUnitCommandTests
{
    [TestMethod]
    public void AddLocoToMultiUnit_ReturnsCorrectBytes_SameDirection()
    {
        var target = new AddLocoToMultiUnitCommand(Address.From(3), 10, reversed: false);
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);
        Assert.AreEqual(0x40, data[1]);
        Assert.AreEqual(0x00, data[2]);
        Assert.AreEqual(0x03, data[3]);
        Assert.AreEqual(10, data[4]);
    }

    [TestMethod]
    public void AddLocoToMultiUnit_ReturnsCorrectBytes_ReversedDirection()
    {
        var target = new AddLocoToMultiUnitCommand(Address.From(3), 10, reversed: true);
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);
        Assert.AreEqual(0x41, data[1]);
        Assert.AreEqual(0x00, data[2]);
        Assert.AreEqual(0x03, data[3]);
        Assert.AreEqual(10, data[4]);
    }

    [TestMethod]
    public void AddLocoToMultiUnit_ReturnsCorrectBytes_ForLongAddress()
    {
        var target = new AddLocoToMultiUnitCommand(Address.From(1234), 99);
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);
        Assert.AreEqual(0x40, data[1]);
        Assert.AreEqual(0xC4, data[2]);
        Assert.AreEqual(0xD2, data[3]);
        Assert.AreEqual(99, data[4]);
    }

    [TestMethod]
    public void AddLocoToMultiUnit_Throws_WhenMTRAddressIsZero()
    {
        try
        {
            _ = new AddLocoToMultiUnitCommand(Address.From(3), 0);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void AddLocoToMultiUnit_Throws_WhenMTRAddressTooHigh()
    {
        try
        {
            _ = new AddLocoToMultiUnitCommand(Address.From(3), 100);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }
}
