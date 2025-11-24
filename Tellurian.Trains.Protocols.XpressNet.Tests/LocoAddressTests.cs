namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class LocoAddressTests {

    [TestMethod]
    public void ZeroNumberThrows() {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LocoAddress(0));
    }

    [TestMethod]
    public void NegativeNumberThrows() {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LocoAddress(-1));
    }

    [TestMethod]
    public void NumberAbove9999Throws() {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LocoAddress(10000));
    }

    [TestMethod]
    public void ShortAddressPropertiesAreCorrect() {
        var target = new LocoAddress(1);
        Assert.IsTrue(target.IsShort);
        Assert.IsFalse(target.IsLong);
        Assert.IsFalse(target.IsShortThreeDigit);
    }

    [TestMethod]
    public void LongAddressPropertiesAreCorrect() {
        var target = new LocoAddress(128);
        Assert.IsFalse(target.IsShort);
        Assert.IsTrue(target.IsLong);
        Assert.IsFalse(target.IsShortThreeDigit);
    }

    [TestMethod]
    public void ShortAddressGetBytesIsCorrect() {
        var target = new LocoAddress(127);
        var actual = target.GetBytesAccordingToXpressNet();
        Assert.IsNotNull(actual);
        Assert.HasCount(2, actual);
        Assert.AreEqual(127, ((actual[0] & 0x3F) << 8) + actual[1]);
        Assert.AreEqual(0x00, (actual[0] & 0xC0));
        Assert.IsTrue(target.IsShortThreeDigit);
    }

    [TestMethod]
    public void LongAddressGetBytesIsCorrect() {
        var target = new LocoAddress(128);
        var actual = target.GetBytesAccordingToXpressNet();
        Assert.IsNotNull(actual);
        Assert.HasCount(2, actual);
        Assert.AreEqual(128, ((actual[0] & 0x3F) << 8) + actual[1]);
        Assert.AreEqual(0xC0, (actual[0] & 0xC0));
    }

    [TestMethod]
    public void FromBytesWorks() {
        var target = new LocoAddress(241);
        var actual = new LocoAddress(target.GetBytesAccordingToXpressNet());
        Assert.AreEqual(241, actual.Number);
     }
}
