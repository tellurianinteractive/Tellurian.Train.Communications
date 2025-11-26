namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class LocoAddressTests
{
    [TestMethod]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenAddressIsZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LocoAddress(0));
    }

    [TestMethod]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenAddressOver9999()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LocoAddress(10000));
    }

    [TestMethod]
    public void ShortAddress_HasCorrectProperties_WhenMinimum()
    {
        var target = new LocoAddress(1);
        Assert.AreEqual(1, target.Low);
        Assert.AreEqual(0, target.High);
        Assert.IsFalse(target.IsLong);
        Assert.IsTrue(target.IsShort);
    }

    [TestMethod]
    public void ShortAddress_HasCorrectProperties_WhenMaximum()
    {
        var target = new LocoAddress(127);
        Assert.AreEqual(127, target.Low);
        Assert.AreEqual(0, target.High);
        Assert.IsFalse(target.IsLong);
        Assert.IsTrue(target.IsShort);
    }

    [TestMethod]
    public void LongAddress_HasCorrectProperties_WhenMinimum()
    {
        var target = new LocoAddress(128);
        Assert.AreEqual(0, target.Low);
        Assert.AreEqual(1, target.High);
        Assert.IsTrue(target.IsLong);
        Assert.IsFalse(target.IsShort);
    }

    [TestMethod]
    public void LongAddress_HasCorrectProperties_WhenMaximum()
    {
        var target = new LocoAddress(9999);
        Assert.AreEqual(15, target.Low);
        Assert.AreEqual(78, target.High);
        Assert.IsTrue(target.IsLong);
        Assert.IsFalse(target.IsShort);
    }
}
