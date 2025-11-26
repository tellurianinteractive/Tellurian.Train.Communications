using Tellurian.Trains.Interfaces.Extensions;

namespace Tellurian.Trains.Interfaces.Tests;

[TestClass]
public class CVAddressTests
{
    [TestMethod]
    public void CvAddress_HasCorrectProperties_WhenValue1()
    {
        const int expected = 1;
        var target = new CvAddress(expected);
        Assert.AreEqual(0, target.LSB);
        Assert.AreEqual(0, target.MSB);
        Assert.AreEqual(expected, target.Value);
    }

    [TestMethod]
    public void CvAddress_HasCorrectProperties_WhenValue256()
    {
        const int expected = 256;
        var target = new CvAddress(expected);
        Assert.AreEqual(255, target.LSB);
        Assert.AreEqual(0, target.MSB);
        Assert.AreEqual(expected, target.Value);
    }
}
