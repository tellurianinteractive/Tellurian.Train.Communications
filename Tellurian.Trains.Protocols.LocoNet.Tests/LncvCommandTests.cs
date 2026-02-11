using Tellurian.Trains.Protocols.LocoNet.Commands;

namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class LncvCommandTests
{
    [TestMethod]
    public void StartSession_GeneratesCorrect15Bytes()
    {
        var command = LncvCommand.StartSession(6341, 1);
        var bytes = command.GetBytesWithChecksum();

        Assert.HasCount(15, bytes);
        Assert.AreEqual(0xED, bytes[0]); // opcode
        Assert.AreEqual(0x0F, bytes[1]); // length
        Assert.AreEqual(0x01, bytes[2]); // SRC
        Assert.AreEqual(0x49, bytes[3]); // DST_L
        Assert.AreEqual(0x4B, bytes[4]); // DST_H
        Assert.AreEqual(0x21, bytes[5]); // CMD (read/session)
    }

    [TestMethod]
    public void StartSession_HasPronFlag()
    {
        var command = LncvCommand.StartSession(6341, 1);
        var bytes = command.GetBytesWithChecksum();

        // CMDDATA (byte 13) should have PRON flag (0x80) set
        // After PXCT1 decoding, the CMDDATA byte should contain 0x80
        // Since CMDDATA is the 7th data byte (index 6), its bit 7 is stored in PXCT1 bit 6
        Assert.AreNotEqual(0, bytes[6] & 0x40, "PXCT1 bit 6 should be set for PRON flag");
    }

    [TestMethod]
    public void StartSession_Broadcast_UsesModuleAddressFFFF()
    {
        var command = LncvCommand.StartSession(6341, 0xFFFF);
        var bytes = command.GetBytesWithChecksum();

        Assert.HasCount(15, bytes);
        Assert.AreEqual(0xED, bytes[0]);
    }

    [TestMethod]
    public void Read_GeneratesCorrectCmdByte()
    {
        var command = LncvCommand.Read(6341, 42, 1);
        var bytes = command.GetBytesWithChecksum();

        Assert.HasCount(15, bytes);
        Assert.AreEqual(0xED, bytes[0]);
        Assert.AreEqual(0x21, bytes[5]); // CMD for read
    }

    [TestMethod]
    public void Read_HasNoCmdDataFlags()
    {
        var command = LncvCommand.Read(6341, 42, 1);
        var bytes = command.GetBytesWithChecksum();

        // CMDDATA (byte 13) should be 0x00 (no flags)
        // PXCT1 bit 6 should be clear (no bit 7 in CMDDATA)
        Assert.AreEqual(0, bytes[6] & 0x40, "PXCT1 bit 6 should be clear for no CMDDATA flags");
    }

    [TestMethod]
    public void Write_GeneratesCorrectCmdByte()
    {
        var command = LncvCommand.Write(6341, 42, 100);
        var bytes = command.GetBytesWithChecksum();

        Assert.HasCount(15, bytes);
        Assert.AreEqual(0xED, bytes[0]);
        Assert.AreEqual(0x20, bytes[5]); // CMD for write
    }

    [TestMethod]
    public void EndSession_UsesOpcodeE5()
    {
        var command = new LncvEndSessionCommand(6341, 1);
        var bytes = command.GetBytesWithChecksum();

        Assert.HasCount(15, bytes);
        Assert.AreEqual(0xE5, bytes[0]);
        Assert.AreEqual(0x0F, bytes[1]);
        Assert.AreEqual(0x01, bytes[2]); // SRC
        Assert.AreEqual(0x49, bytes[3]); // DST_L
        Assert.AreEqual(0x4B, bytes[4]); // DST_H
        Assert.AreEqual(0x21, bytes[5]); // CMD
    }

    [TestMethod]
    public void EndSession_HasProffFlag()
    {
        var command = new LncvEndSessionCommand(6341, 1);
        var bytes = command.GetBytesWithChecksum();

        // CMDDATA = 0x40 (PROFF). bit 7 is not set, so PXCT1 bit 6 should be clear.
        Assert.AreEqual(0, bytes[6] & 0x40, "PXCT1 bit 6 should be clear for PROFF (0x40)");
        // But byte 13 should have 0x40 directly (no bit 7 extraction needed)
        Assert.AreEqual(0x40, bytes[13]);
    }

    [TestMethod]
    public void Pxct1_CorrectForArticle6341()
    {
        // Article 6341 = 0x18C5: low byte 0xC5 has bit 7 set, high byte 0x18 does not
        var command = LncvCommand.StartSession(6341, 1);
        var bytes = command.GetBytesWithChecksum();

        // PXCT1 bit 0 should be set (ART_L 0xC5 has bit 7)
        Assert.AreNotEqual(0, bytes[6] & 0x01, "PXCT1 bit 0 should be set for article 6341 low byte");
        // PXCT1 bit 1 should be clear (ART_H 0x18 has no bit 7)
        Assert.AreEqual(0, bytes[6] & 0x02, "PXCT1 bit 1 should be clear for article 6341 high byte");
    }

    [TestMethod]
    public void AllCommands_HaveValidChecksum()
    {
        var commands = new Command[]
        {
            LncvCommand.StartSession(6341, 1),
            LncvCommand.StartSession(6341, 0xFFFF),
            LncvCommand.Read(6341, 42, 1),
            LncvCommand.Write(6341, 42, 100),
            new LncvEndSessionCommand(6341, 1)
        };

        foreach (var command in commands)
        {
            var bytes = command.GetBytesWithChecksum();
            var expected = Message.Checksum(bytes);
            Assert.AreEqual(expected, bytes[^1], $"Checksum mismatch for {command.GetType().Name}");
        }
    }
}
