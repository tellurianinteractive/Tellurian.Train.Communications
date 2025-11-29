using Tellurian.Trains.Protocols.LocoNet.Commands;

namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class SlotDataConsistExtensionsTests
{
    // ===== IsInConsist (SlotData) Tests =====

    [TestMethod]
    public void IsInConsist_ReturnsFalse_WhenSlotNotInConsist()
    {
        var slotData = CreateSlotData(ConsistStatus.NotInConsist);
        Assert.IsFalse(slotData.IsInConsist());
    }

    [TestMethod]
    public void IsInConsist_ReturnsTrue_WhenSlotIsSubMember()
    {
        var slotData = CreateSlotData(ConsistStatus.SubMember);
        Assert.IsTrue(slotData.IsInConsist());
    }

    [TestMethod]
    public void IsInConsist_ReturnsTrue_WhenSlotIsConsistTop()
    {
        var slotData = CreateSlotData(ConsistStatus.ConsistTop);
        Assert.IsTrue(slotData.IsInConsist());
    }

    [TestMethod]
    public void IsInConsist_ReturnsTrue_WhenSlotIsMidConsist()
    {
        var slotData = CreateSlotData(ConsistStatus.MidConsist);
        Assert.IsTrue(slotData.IsInConsist());
    }

    // ===== CanBeLinked Tests =====

    [TestMethod]
    public void CanBeLinked_ReturnsTrue_WhenNotInConsist()
    {
        var slotData = CreateSlotData(ConsistStatus.NotInConsist);
        Assert.IsTrue(slotData.CanBeLinked());
    }

    [TestMethod]
    public void CanBeLinked_ReturnsTrue_WhenConsistTop()
    {
        // Consist top can accept more members
        var slotData = CreateSlotData(ConsistStatus.ConsistTop);
        Assert.IsTrue(slotData.CanBeLinked());
    }

    [TestMethod]
    public void CanBeLinked_ReturnsFalse_WhenSubMember()
    {
        // Members cannot have additional locomotives linked to them
        var slotData = CreateSlotData(ConsistStatus.SubMember);
        Assert.IsFalse(slotData.CanBeLinked());
    }

    [TestMethod]
    public void CanBeLinked_ReturnsFalse_WhenMidConsist()
    {
        // Mid-consist members cannot have additional locomotives linked
        var slotData = CreateSlotData(ConsistStatus.MidConsist);
        Assert.IsFalse(slotData.CanBeLinked());
    }

    // ===== BuildConsist Tests =====

    [TestMethod]
    public void BuildConsist_CreatesSingleCommand_ForOneMember()
    {
        byte leadSlot = 5;
        byte memberSlot = 10;

        var commands = leadSlot.BuildConsist([memberSlot]);

        Assert.HasCount(1, commands);
        Assert.AreEqual(memberSlot, commands[0].SlaveSlot);
        Assert.AreEqual(leadSlot, commands[0].MasterSlot);
    }

    [TestMethod]
    public void BuildConsist_CreatesMultipleCommands_ForMultipleMembers()
    {
        byte leadSlot = 3;
        byte[] memberSlots = [7, 12, 15];

        var commands = leadSlot.BuildConsist(memberSlots);

        Assert.HasCount(3, commands);

        // All commands should link to the same lead slot
        for (int i = 0; i < commands.Length; i++)
        {
            Assert.AreEqual(memberSlots[i], commands[i].SlaveSlot);
            Assert.AreEqual(leadSlot, commands[i].MasterSlot);
        }
    }

    [TestMethod]
    public void BuildConsist_ThrowsArgumentException_WhenNoMembers()
    {
        byte leadSlot = 5;
        Assert.Throws<ArgumentException>(() => leadSlot.BuildConsist([]));
    }

    [TestMethod]
    public void BuildConsist_ThrowsArgumentException_WhenMembersIsNull()
    {
        byte leadSlot = 5;
        Assert.Throws<ArgumentException>(() => leadSlot.BuildConsist(null!));
    }

    [TestMethod]
    public void BuildConsist_CreatesLinkSlotsCommands_WithCorrectOpcode()
    {
        byte leadSlot = 1;
        var commands = leadSlot.BuildConsist(new byte[2]);

        // Verify the command generates correct bytes
        var bytes = commands[0].GetBytesWithChecksum();
        Assert.AreEqual(LinkSlotsCommand.OperationCode, bytes[0]);
    }

    // ===== BreakConsist Tests =====

    [TestMethod]
    public void BreakConsist_CreatesSingleCommand_ForOneMember()
    {
        byte leadSlot = 5;
        byte[] memberSlots = [10];

        var commands = leadSlot.BreakConsist(memberSlots);

        Assert.HasCount(1, commands);
        Assert.AreEqual(10, commands[0].SlaveSlot);
        Assert.AreEqual(leadSlot, commands[0].MasterSlot);
    }

    [TestMethod]
    public void BreakConsist_CreatesMultipleCommands_ForMultipleMembers()
    {
        byte leadSlot = 3;
        byte[] memberSlots = [7, 12, 15];

        var commands = leadSlot.BreakConsist(memberSlots);

        Assert.HasCount(3, commands);

        // All commands should unlink from the same lead slot
        for (int i = 0; i < commands.Length; i++)
        {
            Assert.AreEqual(memberSlots[i], commands[i].SlaveSlot);
            Assert.AreEqual(leadSlot, commands[i].MasterSlot);
        }
    }

    [TestMethod]
    public void BreakConsist_ThrowsArgumentException_WhenNoMembers()
    {
        byte leadSlot = 5;
        Assert.Throws<ArgumentException>(() => leadSlot.BreakConsist([]));
    }

    [TestMethod]
    public void BreakConsist_ThrowsArgumentException_WhenMembersIsNull()
    {
        byte leadSlot = 5;
        Assert.Throws<ArgumentException>(() => leadSlot.BreakConsist(null));
    }

    [TestMethod]
    public void BreakConsist_CreatesUnlinkSlotsCommands_WithCorrectOpcode()
    {
        byte leadSlot = 1;
        var commands = leadSlot.BreakConsist([2]);

        // Verify the command generates correct bytes
        var bytes = commands[0].GetBytesWithChecksum();
        Assert.AreEqual(UnlinkSlotsCommand.OperationCode, bytes[0]);
    }

    // ===== Helper Methods =====

    private static SlotData CreateSlotData(ConsistStatus consist, byte slotNumber = 1)
    {
        return new SlotData
        {
            SlotNumber = slotNumber,
            Address = 100,
            Speed = 0,
            Direction = true,
            Status = SlotStatus.InUse,
            Consist = consist,
            DecoderType = DecoderType.Steps128,
            TrackStatus = TrackStatus.PowerOn,
            F0 = false,
            F1 = false,
            F2 = false,
            F3 = false,
            F4 = false,
            F5 = false,
            F6 = false,
            F7 = false,
            F8 = false,
            DeviceId = 0,
            Status2 = 0
        };
    }
}
