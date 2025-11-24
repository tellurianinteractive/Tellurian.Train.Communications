namespace Tellurian.Trains.Protocols.XpressNet.Tests; 
[TestClass]
public class AccessoryAddressTests {

    [TestMethod]
    public void ZeroNumberThrows() {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AccessoryAddress(0));
    }

    [TestMethod]
    public void NegativeNumberThrows() {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AccessoryAddress(-1));
    }

    [TestMethod]
    public void NumberAbove9999Throws() {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AccessoryAddress(10000));
    }

    [TestMethod]
    public void AccessoryAddressPropertiesAreCorrect() {
        var target = new AccessoryAddress(777);
        Assert.AreEqual(194, target.Group);
        Assert.AreEqual(1, target.Subaddress);
     }

    [TestMethod]
    public void AccessoryAddressBytesAreCorrect() {
        var target = new AccessoryAddress(777);
        var data = target.GetBytes();
        var actual = (data[0] << 8) + data[1] + 1;
        Assert.AreEqual(777, actual);
    }


}
