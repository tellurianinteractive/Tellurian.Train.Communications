using Tellurian.Trains.Communications.Channels;

namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class LocoNetFramerTests
{
    [TestMethod]
    public async Task ReadMessageAsync_TwoByteMessage_ReturnsCompleteMessage()
    {
        // Arrange: Power ON command (0x83) - 2 byte message
        using var mockPort = new MockSerialPortAdapter();
        mockPort.Open();
        mockPort.EnqueueMessageWithChecksum(0x83); // Power ON
        var framer = new LocoNetFramer();

        // Act
        var result = await framer.ReadMessageAsync(mockPort.ReadByteAsync, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.AreEqual(0x83, result[0]);
    }

    [TestMethod]
    public async Task ReadMessageAsync_FourByteMessage_ReturnsCompleteMessage()
    {
        // Arrange: Switch request (0xB0) - 4 byte message
        using var mockPort = new MockSerialPortAdapter();
        mockPort.Open();
        mockPort.EnqueueMessageWithChecksum(0xB0, 0x01, 0x10); // Switch 1, thrown
        var framer = new LocoNetFramer();

        // Act
        var result = await framer.ReadMessageAsync(mockPort.ReadByteAsync, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(4, result);
        Assert.AreEqual(0xB0, result[0]);
    }

    [TestMethod]
    public async Task ReadMessageAsync_SixByteMessage_ReturnsCompleteMessage()
    {
        // Arrange: 6 byte message - opcode 0xD0 has bits 6-5 = 10 → 6 bytes
        // (0xD0 = 1101 0000, bits 6-5 = 10)
        using var mockPort = new MockSerialPortAdapter();
        mockPort.Open();
        mockPort.EnqueueMessageWithChecksum(0xD0, 0x01, 0x02, 0x03, 0x04);
        var framer = new LocoNetFramer();

        // Act
        var result = await framer.ReadMessageAsync(mockPort.ReadByteAsync, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(6, result);
        Assert.AreEqual(0xD0, result[0]);
    }

    [TestMethod]
    public async Task ReadMessageAsync_VariableLengthMessage_ReturnsCompleteMessage()
    {
        // Arrange: Slot read data (0xE7) - 14 byte variable length message
        using var mockPort = new MockSerialPortAdapter();
        mockPort.Open();
        // 0xE7, count=0x0E (14), then 11 more bytes + checksum
        var messageWithoutChecksum = new byte[] { 0xE7, 0x0E, 0x01, 0x03, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00 };
        mockPort.EnqueueMessageWithChecksum(messageWithoutChecksum);
        var framer = new LocoNetFramer();

        // Act
        var result = await framer.ReadMessageAsync(mockPort.ReadByteAsync, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(14, result);
        Assert.AreEqual(0xE7, result[0]);
        Assert.AreEqual(0x0E, result[1]);
    }

    [TestMethod]
    public async Task ReadMessageAsync_SkipsNonOpcodeBytes_FindsMessage()
    {
        // Arrange: Some garbage bytes followed by valid message
        using var mockPort = new MockSerialPortAdapter();
        mockPort.Open();
        // Non-opcode bytes (MSB=0) followed by Power OFF (0x82)
        mockPort.EnqueueBytes(0x00, 0x12, 0x34, 0x56);
        mockPort.EnqueueMessageWithChecksum(0x82);
        var framer = new LocoNetFramer();

        // Act
        var result = await framer.ReadMessageAsync(mockPort.ReadByteAsync, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.AreEqual(0x82, result[0]); // Power OFF
    }

    [TestMethod]
    public async Task ReadMessageAsync_InvalidChecksum_ReturnsNull()
    {
        // Arrange: Message with wrong checksum
        using var mockPort = new MockSerialPortAdapter();
        mockPort.Open();
        mockPort.EnqueueBytes(0x83, 0x00); // Power ON with wrong checksum (should be 0x7C)
        mockPort.ReadTimeout = 50;
        var framer = new LocoNetFramer(interByteTimeoutMs: 50);

        // Act
        var result = await framer.ReadMessageAsync(mockPort.ReadByteAsync, TestContext.CancellationToken);

        // Assert
        Assert.IsNull(result); // Should return null due to checksum failure
    }

    [TestMethod]
    public async Task ReadMessageAsync_ChecksumValidationDisabled_ReturnsMessageWithBadChecksum()
    {
        // Arrange: Message with wrong checksum, validation disabled
        using var mockPort = new MockSerialPortAdapter();
        mockPort.Open();
        mockPort.EnqueueBytes(0x83, 0x00); // Power ON with wrong checksum
        var framer = new LocoNetFramer(validateChecksum: false);

        // Act
        var result = await framer.ReadMessageAsync(mockPort.ReadByteAsync, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(result); // Should return message despite bad checksum
        Assert.HasCount(2, result);
    }

    [TestMethod]
    public async Task ReadMessageAsync_InterByteTimeout_ReturnsNull()
    {
        // Arrange: Only opcode, no more bytes
        using var mockPort = new MockSerialPortAdapter();
        mockPort.Open();
        mockPort.ReadTimeout = 50;
        mockPort.EnqueueBytes(0xB0); // 4-byte message opcode, but only 1 byte available
        var framer = new LocoNetFramer(interByteTimeoutMs: 50);

        // Act
        var result = await framer.ReadMessageAsync(mockPort.ReadByteAsync, TestContext.CancellationToken);

        // Assert
        Assert.IsNull(result); // Should timeout waiting for remaining bytes
    }

    [TestMethod]
    public async Task ReadMessageAsync_Cancellation_ReturnsNull()
    {
        // Arrange
        using var mockPort = new MockSerialPortAdapter();
        mockPort.Open();
        // Don't enqueue any bytes
        var framer = new LocoNetFramer();
        using var cts = new CancellationTokenSource(50);

        // Act
        var result = await framer.ReadMessageAsync(mockPort.ReadByteAsync, cts.Token);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ReadMessageAsync_MultipleMessages_ReturnsOneAtATime()
    {
        // Arrange: Two messages in queue
        using var mockPort = new MockSerialPortAdapter();
        mockPort.Open();
        mockPort.EnqueueMessageWithChecksum(0x82); // Power OFF
        mockPort.EnqueueMessageWithChecksum(0x83); // Power ON
        var framer = new LocoNetFramer();

        // Act & Assert
        var first = await framer.ReadMessageAsync(mockPort.ReadByteAsync, TestContext.CancellationToken);
        Assert.IsNotNull(first);
        Assert.AreEqual(0x82, first[0]);

        var second = await framer.ReadMessageAsync(mockPort.ReadByteAsync, TestContext.CancellationToken);
        Assert.IsNotNull(second);
        Assert.AreEqual(0x83, second[0]);
    }

    [TestMethod]
    public void ProtocolName_ReturnsLocoNet()
    {
        var framer = new LocoNetFramer();
        Assert.AreEqual("LocoNet", framer.ProtocolName);
    }

    [TestMethod]
    public async Task ReadMessageAsync_MasterBusy_ReturnsCorrectMessage()
    {
        // Arrange: Master busy notification (0x81) - 2 byte message
        using var mockPort = new MockSerialPortAdapter();
        mockPort.Open();
        mockPort.EnqueueMessageWithChecksum(0x81);
        var framer = new LocoNetFramer();

        // Act
        var result = await framer.ReadMessageAsync(mockPort.ReadByteAsync, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(2, result);
        Assert.AreEqual(0x81, result[0]);
    }

    [TestMethod]
    public async Task ReadMessageAsync_SensorInput_ReturnsCorrectMessage()
    {
        // Arrange: Sensor input notification (0xB2) - 4 byte message
        using var mockPort = new MockSerialPortAdapter();
        mockPort.Open();
        mockPort.EnqueueMessageWithChecksum(0xB2, 0x01, 0x50); // Sensor with address info
        var framer = new LocoNetFramer();

        // Act
        var result = await framer.ReadMessageAsync(mockPort.ReadByteAsync, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(4, result);
        Assert.AreEqual(0xB2, result[0]);
    }

    [TestMethod]
    public async Task ReadMessageAsync_LongAcknowledge_ReturnsCorrectMessage()
    {
        // Arrange: Long acknowledge (0xB4) - 4 byte message
        using var mockPort = new MockSerialPortAdapter();
        mockPort.Open();
        mockPort.EnqueueMessageWithChecksum(0xB4, 0x3F, 0x00); // LACK for slot request
        var framer = new LocoNetFramer();

        // Act
        var result = await framer.ReadMessageAsync(mockPort.ReadByteAsync, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(4, result);
        Assert.AreEqual(0xB4, result[0]);
    }

    public TestContext TestContext { get; set; }
}
