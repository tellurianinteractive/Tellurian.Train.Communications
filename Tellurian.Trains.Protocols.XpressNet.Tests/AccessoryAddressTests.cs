using Tellurian.Trains.Communications.Interfaces.Accessories;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class AccessoryAddressTests
{
    [TestMethod]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Tellurian.Trains.Communications.Interfaces.Accessories.Address.From(-1));
    }

    [TestMethod]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenAbove2048()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Tellurian.Trains.Communications.Interfaces.Accessories.Address.From(2049));
    }

    [TestMethod]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenZero()
    {
        // User addresses are 1-based, so 0 is invalid
        Assert.Throws<ArgumentOutOfRangeException>(() => Tellurian.Trains.Communications.Interfaces.Accessories.Address.From(0));
    }

    [TestMethod]
    public void Constructor_AcceptsOne()
    {
        var target = Tellurian.Trains.Communications.Interfaces.Accessories.Address.From(1);
        Assert.AreEqual(1, target.Number);
    }

    [TestMethod]
    public void Constructor_Accepts2048()
    {
        var target = Tellurian.Trains.Communications.Interfaces.Accessories.Address.From(2048);
        Assert.AreEqual(2048, target.Number);
    }

    [TestMethod]
    public void AccessoryAddress_HasCorrectXpressNetProperties()
    {
        var target = Tellurian.Trains.Communications.Interfaces.Accessories.Address.From(777);
        Assert.AreEqual(194, target.Group);
        Assert.AreEqual(1, target.Subaddress);
    }

    [TestMethod]
    public void GetBytes_ReturnsCorrectBytes()
    {
        var target = Tellurian.Trains.Communications.Interfaces.Accessories.Address.From(777);
        var data = target.GetBytes();
        var actual = (data[0] << 8) + data[1] + 1;
        Assert.AreEqual(777, actual);
    }
}
