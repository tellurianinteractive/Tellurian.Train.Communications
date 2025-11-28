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
        // Buffer with only 2 bytes (less than minimum 4 for frame header)
        var buffer = new byte[] { 2, 0 };
        var result = CommunicationResult.Success(buffer, "Test", "Test");

        Assert.Throws<ArgumentException>(() => Frame.CreateMany(result).ToList());
    }

    [TestMethod]
    public void CreateManyWithMalformedLengthThrows()
    {
        // Frame claims to be 100 bytes but buffer only has 8 bytes
        var buffer = new byte[] { 100, 0, 0x10, 0x00, 1, 2, 3, 4 };
        var result = CommunicationResult.Success(buffer, "Test", "Test");

        Assert.Throws<ArgumentException>(() => Frame.CreateMany(result).ToList());
    }

    [TestMethod]
    public void CreateManyWithMultipleFramesDifferentSizes()
    {
        // Create 3 frames: 4 bytes, 6 bytes, 8 bytes
        var frame1 = new byte[] { 4, 0, 0x10, 0x00 }; // Minimal frame (4 bytes)
        var frame2 = new byte[] { 6, 0, 0x10, 0x00, 0xAA, 0xBB }; // 6 bytes (4 + 2 data)
        var frame3 = new byte[] { 8, 0, 0x10, 0x00, 0xCC, 0xDD, 0xEE, 0xFF }; // 8 bytes (4 + 4 data)

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
        // Frame with length 0 should cause issues
        var buffer = new byte[] { 0, 0, 0x10, 0x00 };
        var result = CommunicationResult.Success(buffer, "Test", "Test");

        // Zero length causes OverflowException when creating negative-sized data array
        Assert.Throws<OverflowException>(() => Frame.CreateMany(result).ToList());
    }
}
