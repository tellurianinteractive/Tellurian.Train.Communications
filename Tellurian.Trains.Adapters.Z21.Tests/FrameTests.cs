namespace Tellurian.Trains.Adapters.Z21.Tests;

[TestClass]
public class FrameTests
{
    [TestMethod]
    public void Constructor_SetsEmptyData_WhenNoDataProvided()
    {
        var target = new Frame(FrameHeader.Test);
        Assert.AreEqual(FrameHeader.Test, target.Header);
        Assert.IsFalse(target.Data.Length != 0);
    }

    [TestMethod]
    public void Constructor_ThrowsArgumentNullException_WhenDataIsNull()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new Frame(FrameHeader.Test, null));
    }

    [TestMethod]
    public void Length_ReturnsFour_WhenNoData()
    {
        var target = new Frame(FrameHeader.Test);
        Assert.AreEqual(4, target.Length);
    }
}
