using Tellurian.Trains.Interfaces.Accessories;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class AccessoryAddressTests
{
    [TestMethod]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AccessoryAddress.From(-1));
    }

    [TestMethod]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenAbove2047()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AccessoryAddress.From(2048));
    }

    [TestMethod]
    public void Constructor_AcceptsZero()
    {
        var target = AccessoryAddress.From(0);
        Assert.AreEqual(0, target.Number);
    }

    [TestMethod]
    public void AccessoryAddress_HasCorrectXpressNetProperties()
    {
        var target = AccessoryAddress.From(777);
        Assert.AreEqual(194, target.Group);
        Assert.AreEqual(1, target.Subaddress);
    }

    [TestMethod]
    public void GetBytes_ReturnsCorrectBytes()
    {
        var target = AccessoryAddress.From(777);
        var data = target.GetBytes();
        var actual = (data[0] << 8) + data[1] + 1;
        Assert.AreEqual(777, actual);
    }
}
