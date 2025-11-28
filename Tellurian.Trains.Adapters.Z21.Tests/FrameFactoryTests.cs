using Tellurian.Trains.Communications.Channels;

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

    [TestMethod]
    public void CreateManyWithEmptyBufferReturnsEmpty()
    {
        var buffer = Array.Empty<byte>();
        var result = CommunicationResult.Success(buffer, "Test", "Test");
        var actual = Frame.CreateMany(result);
        Assert.AreEqual(0, actual.Count());
    }

    [TestMethod]
    public void CreateManyWithFailureResultReturnsEmpty()
    {
        var result = CommunicationResult.Failure(new Exception("Test exception"));
        var actual = Frame.CreateMany(result);
        Assert.AreEqual(0, actual.Count());
    }

    [TestMethod]
    public void CreateManyWithNoOperationResultReturnsEmpty()
    {
        var result = CommunicationResult.NoOperation();
        var actual = Frame.CreateMany(result);
        Assert.AreEqual(0, actual.Count());
    }

    [TestMethod]
    public void CreateManyWithBufferTooShortThrows()
    {
        var buffer = new byte[] { 2, 0 };
        var result = CommunicationResult.Success(buffer, "Test", "Test");

        Assert.Throws<ArgumentException>(() => Frame.CreateMany(result).ToList());
    }

    [TestMethod]
    public void CreateManyWithMalformedLengthThrows()
    {
        var buffer = new byte[] { 100, 0, 0x10, 0x00, 1, 2, 3, 4 };
        var result = CommunicationResult.Success(buffer, "Test", "Test");

        Assert.Throws<ArgumentException>(() => Frame.CreateMany(result).ToList());
    }

    [TestMethod]
    public void CreateManyWithMultipleFramesDifferentSizes()
    {
        var frame1 = new byte[] { 4, 0, 0x10, 0x00 };
        var frame2 = new byte[] { 6, 0, 0x10, 0x00, 0xAA, 0xBB };
        var frame3 = new byte[] { 8, 0, 0x10, 0x00, 0xCC, 0xDD, 0xEE, 0xFF };

        var buffer = new byte[frame1.Length + frame2.Length + frame3.Length];
        Buffer.BlockCopy(frame1, 0, buffer, 0, frame1.Length);
        Buffer.BlockCopy(frame2, 0, buffer, frame1.Length, frame2.Length);
        Buffer.BlockCopy(frame3, 0, buffer, frame1.Length + frame2.Length, frame3.Length);

        var result = CommunicationResult.Success(buffer, "Test", "Test");
        var actual = Frame.CreateMany(result).ToList();

        Assert.HasCount(3, actual);
        Assert.AreEqual(4, actual[0].Length);
        Assert.AreEqual(6, actual[1].Length);
        Assert.AreEqual(8, actual[2].Length);
    }

    [TestMethod]
    public void CreateManyWithZeroLengthFrameThrows()
    {
        var buffer = new byte[] { 0, 0, 0x10, 0x00 };
        var result = CommunicationResult.Success(buffer, "Test", "Test");

        Assert.Throws<OverflowException>(() => Frame.CreateMany(result).ToList());
    }
}
