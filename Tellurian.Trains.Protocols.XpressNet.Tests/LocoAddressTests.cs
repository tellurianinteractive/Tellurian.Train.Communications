namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class LocoAddressTests {

    [TestMethod]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenZero() {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LocoAddress(0));
    }

    [TestMethod]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenNegative() {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LocoAddress(-1));
    }

    [TestMethod]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenAbove9999() {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LocoAddress(10000));
    }

    [TestMethod]
    public void ShortAddress_HasCorrectProperties() {
        var target = new LocoAddress(1);
        Assert.IsTrue(target.IsShort);
        Assert.IsFalse(target.IsLong);
        Assert.IsFalse(target.IsShortThreeDigit);
    }

    [TestMethod]
    public void LongAddress_HasCorrectProperties() {
        var target = new LocoAddress(128);
        Assert.IsFalse(target.IsShort);
        Assert.IsTrue(target.IsLong);
        Assert.IsFalse(target.IsShortThreeDigit);
    }

    [TestMethod]
    public void GetBytesAccordingToXpressNet_ReturnsCorrectBytes_WhenShortAddress() {
        var target = new LocoAddress(127);
        var actual = target.GetBytesAccordingToXpressNet();
        Assert.IsNotNull(actual);
        Assert.HasCount(2, actual);
        Assert.AreEqual(127, ((actual[0] & 0x3F) << 8) + actual[1]);
        Assert.AreEqual(0x00, (actual[0] & 0xC0));
        Assert.IsTrue(target.IsShortThreeDigit);
    }

    [TestMethod]
    public void GetBytesAccordingToXpressNet_ReturnsCorrectBytes_WhenLongAddress() {
        var target = new LocoAddress(128);
        var actual = target.GetBytesAccordingToXpressNet();
        Assert.IsNotNull(actual);
        Assert.HasCount(2, actual);
        Assert.AreEqual(128, ((actual[0] & 0x3F) << 8) + actual[1]);
        Assert.AreEqual(0xC0, (actual[0] & 0xC0));
    }

    [TestMethod]
    public void Constructor_FromBytes_ReconstructsAddress() {
        var target = new LocoAddress(241);
        var actual = new LocoAddress(target.GetBytesAccordingToXpressNet());
        Assert.AreEqual(241, actual.Number);
     }
}
