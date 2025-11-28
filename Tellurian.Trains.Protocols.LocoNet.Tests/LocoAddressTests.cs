using Tellurian.Trains.Interfaces.Locos;

namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class LocoAddressTests
{
    [TestMethod]
    public void Constructor_AllowsZero_AsSentinelValue()
    {
        var address = LocoAddress.Zero;
        Assert.AreEqual(0, address.Number);
    }

    [TestMethod]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenAddressOver9999()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LocoAddress.From(10000));
    }

    [TestMethod]
    public void ShortAddress_HasCorrectProperties_WhenMinimum()
    {
        var target = LocoAddress.From(1);
        Assert.AreEqual(1, target.Low);
        Assert.AreEqual(0, target.High);
        Assert.IsFalse(target.IsLong);
        Assert.IsTrue(target.IsShort);
    }

    [TestMethod]
    public void ShortAddress_HasCorrectProperties_WhenMaximum()
    {
        var target = LocoAddress.From(127);
        Assert.AreEqual(127, target.Low);
        Assert.AreEqual(0, target.High);
        Assert.IsFalse(target.IsLong);
        Assert.IsTrue(target.IsShort);
    }

    [TestMethod]
    public void LongAddress_HasCorrectProperties_WhenMinimum()
    {
        var target = LocoAddress.From(128);
        Assert.AreEqual(0, target.Low);
        Assert.AreEqual(1, target.High);
        Assert.IsTrue(target.IsLong);
        Assert.IsFalse(target.IsShort);
    }

    [TestMethod]
    public void LongAddress_HasCorrectProperties_WhenMaximum()
    {
        var target = LocoAddress.From(9999);
        Assert.AreEqual(15, target.Low);
        Assert.AreEqual(78, target.High);
        Assert.IsTrue(target.IsLong);
        Assert.IsFalse(target.IsShort);
    }
}
