using Tellurian.Trains.Protocols.LocoNet.Lncv;

namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class Pxct1EncodingTests
{
    [TestMethod]
    public void Encode_AllZeros_ReturnsPxct1Zero()
    {
        byte[] data = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
        var pxct1 = Pxct1Encoding.Encode(data);

        Assert.AreEqual(0x00, pxct1);
        Assert.IsTrue(data.All(b => b == 0x00));
    }

    [TestMethod]
    public void Encode_AllBit7Set_ReturnsPxct1AllBits()
    {
        byte[] data = [0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80];
        var pxct1 = Pxct1Encoding.Encode(data);

        Assert.AreEqual(0x7F, pxct1); // bits 0-6 all set
        Assert.IsTrue(data.All(b => b == 0x00)); // all bit 7 cleared
    }

    [TestMethod]
    public void Encode_MixedValues_ExtractsBit7Correctly()
    {
        byte[] data = [0x85, 0x30, 0xFF, 0x00, 0x80, 0x7F, 0x01];
        var pxct1 = Pxct1Encoding.Encode(data);

        // bit 7 set in indices: 0 (0x85), 2 (0xFF), 4 (0x80)
        Assert.AreEqual(0b00010101, pxct1);
        // Data should have bit 7 cleared
        Assert.AreEqual(0x05, data[0]); // 0x85 & 0x7F
        Assert.AreEqual(0x30, data[1]); // unchanged
        Assert.AreEqual(0x7F, data[2]); // 0xFF & 0x7F
        Assert.AreEqual(0x00, data[3]); // unchanged
        Assert.AreEqual(0x00, data[4]); // 0x80 & 0x7F
        Assert.AreEqual(0x7F, data[5]); // unchanged
        Assert.AreEqual(0x01, data[6]); // unchanged
    }

    [TestMethod]
    public void Decode_RestoresBit7FromPxct1()
    {
        byte[] data = [0x05, 0x30, 0x7F, 0x00, 0x00, 0x7F, 0x01];
        byte pxct1 = 0b00010101; // bits 0, 2, 4

        Pxct1Encoding.Decode(pxct1, data);

        Assert.AreEqual(0x85, data[0]);
        Assert.AreEqual(0x30, data[1]);
        Assert.AreEqual(0xFF, data[2]);
        Assert.AreEqual(0x00, data[3]);
        Assert.AreEqual(0x80, data[4]);
        Assert.AreEqual(0x7F, data[5]);
        Assert.AreEqual(0x01, data[6]);
    }

    [TestMethod]
    public void RoundTrip_EncodeThenDecode_RestoresOriginalData()
    {
        byte[] original = [0x85, 0x30, 0xFF, 0x00, 0x80, 0x7F, 0x01];
        byte[] data = [.. original];

        var pxct1 = Pxct1Encoding.Encode(data);
        Pxct1Encoding.Decode(pxct1, data);

        for (var i = 0; i < original.Length; i++)
        {
            Assert.AreEqual(original[i], data[i], $"Byte {i} mismatch after round-trip");
        }
    }

    [TestMethod]
    public void RoundTrip_AllFF_RestoresOriginalData()
    {
        byte[] original = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF];
        byte[] data = [.. original];

        var pxct1 = Pxct1Encoding.Encode(data);
        Pxct1Encoding.Decode(pxct1, data);

        for (var i = 0; i < original.Length; i++)
        {
            Assert.AreEqual(original[i], data[i], $"Byte {i} mismatch after round-trip");
        }
    }

    [TestMethod]
    public void Encode_FewerThan7Bytes_WorksCorrectly()
    {
        byte[] data = [0x85, 0x80];
        var pxct1 = Pxct1Encoding.Encode(data);

        Assert.AreEqual(0b00000011, pxct1);
        Assert.AreEqual(0x05, data[0]);
        Assert.AreEqual(0x00, data[1]);
    }
}
