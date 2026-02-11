using Tellurian.Trains.Protocols.LocoNet.Lncv;
using Tellurian.Trains.Protocols.LocoNet.Notifications;

namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class LncvNotificationTests
{
    /// <summary>
    /// Creates a valid LNCV notification byte array with correct PXCT1 encoding and checksum.
    /// </summary>
    private static byte[] CreateLncvNotificationBytes(
        ushort articleNumber, ushort cvOrModuleLow, ushort valueOrModuleHigh, byte cmdData)
    {
        byte[] dataBytes =
        [
            (byte)(articleNumber & 0xFF),
            (byte)(articleNumber >> 8),
            (byte)(cvOrModuleLow & 0xFF),
            (byte)(cvOrModuleLow >> 8),
            (byte)(valueOrModuleHigh & 0xFF),
            (byte)(valueOrModuleHigh >> 8),
            cmdData
        ];

        var pxct1 = Pxct1Encoding.Encode(dataBytes);

        byte[] message =
        [
            0xE5, 0x0F, 0x01, 0x49, 0x4B,
            0x1F, pxct1,
            dataBytes[0], dataBytes[1], dataBytes[2], dataBytes[3],
            dataBytes[4], dataBytes[5], dataBytes[6]
        ];

        return Message.AppendChecksum(message);
    }

    [TestMethod]
    public void ReadReply_ExtractsArticleNumber()
    {
        var bytes = CreateLncvNotificationBytes(6341, cvOrModuleLow: 42, valueOrModuleHigh: 100, cmdData: 0x00);

        var notification = new LncvNotification(bytes);

        Assert.AreEqual(LncvMessageType.ReadReply, notification.LncvType);
        Assert.AreEqual((ushort)6341, notification.ArticleNumber);
    }

    [TestMethod]
    public void ReadReply_ExtractsCvNumberAndValue()
    {
        var bytes = CreateLncvNotificationBytes(6341, cvOrModuleLow: 42, valueOrModuleHigh: 100, cmdData: 0x00);

        var notification = new LncvNotification(bytes);

        Assert.AreEqual((ushort)42, notification.CvNumber);
        Assert.AreEqual((ushort)100, notification.CvValue);
    }

    [TestMethod]
    public void ReadReply_HandlesLargeValues()
    {
        // Test with values that have bit 7 set in both low and high bytes
        var bytes = CreateLncvNotificationBytes(6341, cvOrModuleLow: 1000, valueOrModuleHigh: 50000, cmdData: 0x00);

        var notification = new LncvNotification(bytes);

        Assert.AreEqual(LncvMessageType.ReadReply, notification.LncvType);
        Assert.AreEqual((ushort)6341, notification.ArticleNumber);
        Assert.AreEqual((ushort)1000, notification.CvNumber);
        Assert.AreEqual((ushort)50000, notification.CvValue);
    }

    [TestMethod]
    public void SessionAcknowledgment_ExtractsArticleAndModule()
    {
        var bytes = CreateLncvNotificationBytes(6341, cvOrModuleLow: 0, valueOrModuleHigh: 5, cmdData: 0x80);

        var notification = new LncvNotification(bytes);

        Assert.AreEqual(LncvMessageType.SessionAcknowledgment, notification.LncvType);
        Assert.AreEqual((ushort)6341, notification.ArticleNumber);
        Assert.AreEqual((ushort)5, notification.ModuleAddress);
    }

    [TestMethod]
    public void Pxct1Decoding_RestoresCorrect16BitValues()
    {
        // Article 6341 = 0x18C5: low byte has bit 7 set
        // CV 256 = 0x0100: high byte has bit 0 set but no bit 7
        // Value 32768 = 0x8000: high byte has bit 7 set
        var bytes = CreateLncvNotificationBytes(6341, cvOrModuleLow: 256, valueOrModuleHigh: 32768, cmdData: 0x00);

        var notification = new LncvNotification(bytes);

        Assert.AreEqual((ushort)6341, notification.ArticleNumber);
        Assert.AreEqual((ushort)256, notification.CvNumber);
        Assert.AreEqual((ushort)32768, notification.CvValue);
    }

    [TestMethod]
    public void MessageFactory_RoutesOpcodeE5_ToLncvNotification()
    {
        var bytes = CreateLncvNotificationBytes(6341, cvOrModuleLow: 42, valueOrModuleHigh: 100, cmdData: 0x00);

        var message = LocoNetMessageFactory.Create(bytes);

        Assert.IsInstanceOfType<LncvNotification>(message);
    }

    [TestMethod]
    public void MessageFactory_RoutesNonLncvOpcodeE5_ToUnsupportedNotification()
    {
        // Create a 0xE5 message with different DST bytes (not LNCV)
        byte[] message = [0xE5, 0x0F, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
        var bytes = Message.AppendChecksum(message);

        var result = LocoNetMessageFactory.Create(bytes);

        Assert.IsInstanceOfType<UnsupportedNotification>(result);
    }

    [TestMethod]
    public void LncvNotification_ThrowsOnInvalidLength()
    {
        Assert.Throws<ArgumentException>(() => new LncvNotification([0xE5, 0x0F, 0x01]));
    }

    [TestMethod]
    public void ReadReply_ToString_ContainsRelevantInfo()
    {
        var bytes = CreateLncvNotificationBytes(6341, cvOrModuleLow: 42, valueOrModuleHigh: 100, cmdData: 0x00);
        var notification = new LncvNotification(bytes);

        var result = notification.ToString();

        Assert.Contains("6341", result);
        Assert.Contains("42", result);
        Assert.Contains("100", result);
    }

    [TestMethod]
    public void SessionAcknowledgment_ToString_ContainsRelevantInfo()
    {
        var bytes = CreateLncvNotificationBytes(6341, cvOrModuleLow: 0, valueOrModuleHigh: 5, cmdData: 0x80);
        var notification = new LncvNotification(bytes);

        var result = notification.ToString();

        Assert.Contains("6341", result);
        Assert.Contains("5", result);
    }
}
