using Tellurian.Communications.Channels;

namespace Tellurian.Trains.Adapters.Z21.Tests;

[TestClass]
public class FrameFactoryTests
{
    const FrameHeader Header = FrameHeader.Test;

    [TestMethod]
    public void CreatesManyFramesFromBuffer()
    {
        var buffer = TestData.CreateManyTestBuffer(Header);
        var result = CommunicationResult.Success(buffer, "Test", "Test");
        var actual = Frame.CreateMany(result);
        Assert.AreEqual(3, actual.Count());
    }
}
