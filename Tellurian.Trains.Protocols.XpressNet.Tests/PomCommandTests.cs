using Tellurian.Trains.Protocols.XpressNet.Decoder;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class PomCommandTests
{
    #region POM Byte Write

    [TestMethod]
    public void PomWriteByte_ReturnsCorrectBytes_ForShortAddressAndCV1()
    {
        // Loco address 3, CV 1, value 0x55
        var target = new PomWriteByteCommand(new LocoAddress(3), 1, 0x55);
        var data = target.GetData();

        Assert.AreEqual(0xE6, data[0]);     // Header with length 6
        Assert.AreEqual(0x30, data[1]);     // Identification
        Assert.AreEqual(0x00, data[2]);     // Address High (short address)
        Assert.AreEqual(0x03, data[3]);     // Address Low
        Assert.AreEqual(0xEC, data[4]);     // Mode byte (0xEC + 0 for CV 1-256)
        Assert.AreEqual(0x00, data[5]);     // CV lower 8 bits (CV1 = 0)
        Assert.AreEqual(0x55, data[6]);     // Value
    }

    [TestMethod]
    public void PomWriteByte_ReturnsCorrectBytes_ForLongAddressAndCV256()
    {
        // Loco address 9999, CV 256, value 0xAA
        var target = new PomWriteByteCommand(new LocoAddress(9999), 256, 0xAA);
        var data = target.GetData();

        Assert.AreEqual(0xE6, data[0]);     // Header with length 6
        Assert.AreEqual(0x30, data[1]);     // Identification
        // Address 9999 = 0x270F, with long address flag 0xC0 -> 0xE7, 0x0F
        Assert.AreEqual(0xE7, data[2]);     // Address High (0x27 + 0xC0)
        Assert.AreEqual(0x0F, data[3]);     // Address Low
        Assert.AreEqual(0xEC, data[4]);     // Mode byte (CV 256 = wire 255 = 0x00FF, upper 2 bits = 0)
        Assert.AreEqual(0xFF, data[5]);     // CV lower 8 bits (CV256 = wire 255 = 0xFF)
        Assert.AreEqual(0xAA, data[6]);     // Value
    }

    [TestMethod]
    public void PomWriteByte_ReturnsCorrectBytes_ForCV257()
    {
        // CV 257 requires upper bits - wire CV = 256 = 0x100
        var target = new PomWriteByteCommand(new LocoAddress(3), 257, 0x42);
        var data = target.GetData();

        Assert.AreEqual(0xED, data[4]);     // Mode byte (0xEC + 1 for CV 257-512)
        Assert.AreEqual(0x00, data[5]);     // CV lower 8 bits (257-1 = 256 = 0x100, low byte = 0x00)
        Assert.AreEqual(0x42, data[6]);     // Value
    }

    [TestMethod]
    public void PomWriteByte_ReturnsCorrectBytes_ForCV513()
    {
        // CV 513 = wire 512 = 0x200, upper 2 bits = 2
        var target = new PomWriteByteCommand(new LocoAddress(3), 513, 0x77);
        var data = target.GetData();

        Assert.AreEqual(0xEE, data[4]);     // Mode byte (0xEC + 2)
        Assert.AreEqual(0x00, data[5]);     // CV lower 8 bits
        Assert.AreEqual(0x77, data[6]);     // Value
    }

    [TestMethod]
    public void PomWriteByte_ReturnsCorrectBytes_ForCV769()
    {
        // CV 769 = wire 768 = 0x300, upper 2 bits = 3
        var target = new PomWriteByteCommand(new LocoAddress(3), 769, 0x99);
        var data = target.GetData();

        Assert.AreEqual(0xEF, data[4]);     // Mode byte (0xEC + 3)
        Assert.AreEqual(0x00, data[5]);     // CV lower 8 bits
        Assert.AreEqual(0x99, data[6]);     // Value
    }

    [TestMethod]
    public void PomWriteByte_ReturnsCorrectBytes_ForCV1024()
    {
        // CV 1024 = wire 1023 = 0x3FF, upper 2 bits = 3
        var target = new PomWriteByteCommand(new LocoAddress(3), 1024, 0xBB);
        var data = target.GetData();

        Assert.AreEqual(0xEF, data[4]);     // Mode byte (0xEC + 3)
        Assert.AreEqual(0xFF, data[5]);     // CV lower 8 bits (1023 & 0xFF = 0xFF)
        Assert.AreEqual(0xBB, data[6]);     // Value
    }

    [TestMethod]
    public void PomWriteByte_Throws_WhenCVIsZero()
    {
        try
        {
            _ = new PomWriteByteCommand(new LocoAddress(3), 0, 0x00);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void PomWriteByte_Throws_WhenCVIsTooHigh()
    {
        try
        {
            _ = new PomWriteByteCommand(new LocoAddress(3), 1025, 0x00);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    #endregion

    #region POM Bit Write

    [TestMethod]
    public void PomWriteBit_ReturnsCorrectBytes_ForBit0SetTo1()
    {
        // Loco address 3, CV 29, bit 0, value 1
        var target = new PomWriteBitCommand(new LocoAddress(3), 29, 0, true);
        var data = target.GetData();

        Assert.AreEqual(0xE6, data[0]);     // Header with length 6
        Assert.AreEqual(0x30, data[1]);     // Identification
        Assert.AreEqual(0x00, data[2]);     // Address High
        Assert.AreEqual(0x03, data[3]);     // Address Low
        Assert.AreEqual(0xE8, data[4]);     // Mode byte (0xE8 + 0 for CV 1-256)
        Assert.AreEqual(0x1C, data[5]);     // CV lower 8 bits (CV29 - 1 = 28 = 0x1C)
        Assert.AreEqual(0xF8, data[6]);     // Bit value: 0xF0 + 8 (W=1) + 0 (bit position)
    }

    [TestMethod]
    public void PomWriteBit_ReturnsCorrectBytes_ForBit7SetTo0()
    {
        // Loco address 3, CV 29, bit 7, value 0
        var target = new PomWriteBitCommand(new LocoAddress(3), 29, 7, false);
        var data = target.GetData();

        Assert.AreEqual(0xE8, data[4]);     // Mode byte
        Assert.AreEqual(0x1C, data[5]);     // CV lower 8 bits
        Assert.AreEqual(0xF7, data[6]);     // Bit value: 0xF0 + 0 (W=0) + 7 (bit position)
    }

    [TestMethod]
    public void PomWriteBit_ReturnsCorrectBytes_ForBit3SetTo1()
    {
        // Loco address 100, CV 1, bit 3, value 1
        var target = new PomWriteBitCommand(new LocoAddress(100), 1, 3, true);
        var data = target.GetData();

        Assert.AreEqual(0x00, data[2]);     // Address High (short address < 128)
        Assert.AreEqual(0x64, data[3]);     // Address Low (100 = 0x64)
        Assert.AreEqual(0xE8, data[4]);     // Mode byte
        Assert.AreEqual(0x00, data[5]);     // CV lower 8 bits (CV1 = 0)
        Assert.AreEqual(0xFB, data[6]);     // Bit value: 0xF0 + 8 (W=1) + 3 (bit position)
    }

    [TestMethod]
    public void PomWriteBit_ReturnsCorrectBytes_ForLongAddressAndHighCV()
    {
        // Loco address 1000, CV 513, bit 5, value 0
        var target = new PomWriteBitCommand(new LocoAddress(1000), 513, 5, false);
        var data = target.GetData();

        // Address 1000 = 0x03E8, with long address flag -> 0xC3, 0xE8
        Assert.AreEqual(0xC3, data[2]);     // Address High
        Assert.AreEqual(0xE8, data[3]);     // Address Low
        Assert.AreEqual(0xEA, data[4]);     // Mode byte (0xE8 + 2 for CV 513-768)
        Assert.AreEqual(0x00, data[5]);     // CV lower 8 bits (512 & 0xFF = 0)
        Assert.AreEqual(0xF5, data[6]);     // Bit value: 0xF0 + 0 (W=0) + 5 (bit position)
    }

    [TestMethod]
    public void PomWriteBit_Throws_WhenBitPositionTooHigh()
    {
        try
        {
            _ = new PomWriteBitCommand(new LocoAddress(3), 29, 8, true);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void PomWriteBit_Throws_WhenCVIsZero()
    {
        try
        {
            _ = new PomWriteBitCommand(new LocoAddress(3), 0, 0, true);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void PomWriteBit_Throws_WhenCVIsTooHigh()
    {
        try
        {
            _ = new PomWriteBitCommand(new LocoAddress(3), 1025, 0, true);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    #endregion
}
