namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class ConsistStatusTests
{
    [TestMethod]
    public void IsInConsist_ReturnsFalse_WhenNotInConsist()
    {
        var status = ConsistStatus.NotInConsist;
        Assert.IsFalse(status.IsInConsist());
    }

    [TestMethod]
    public void IsInConsist_ReturnsTrue_WhenSubMember()
    {
        var status = ConsistStatus.SubMember;
        Assert.IsTrue(status.IsInConsist());
    }

    [TestMethod]
    public void IsInConsist_ReturnsTrue_WhenConsistTop()
    {
        var status = ConsistStatus.ConsistTop;
        Assert.IsTrue(status.IsInConsist());
    }

    [TestMethod]
    public void IsInConsist_ReturnsTrue_WhenMidConsist()
    {
        var status = ConsistStatus.MidConsist;
        Assert.IsTrue(status.IsInConsist());
    }

    [TestMethod]
    public void IsConsistLead_ReturnsFalse_WhenNotInConsist()
    {
        var status = ConsistStatus.NotInConsist;
        Assert.IsFalse(status.IsConsistLead());
    }

    [TestMethod]
    public void IsConsistLead_ReturnsFalse_WhenSubMember()
    {
        var status = ConsistStatus.SubMember;
        Assert.IsFalse(status.IsConsistLead());
    }

    [TestMethod]
    public void IsConsistLead_ReturnsTrue_WhenConsistTop()
    {
        var status = ConsistStatus.ConsistTop;
        Assert.IsTrue(status.IsConsistLead());
    }

    [TestMethod]
    public void IsConsistLead_ReturnsFalse_WhenMidConsist()
    {
        var status = ConsistStatus.MidConsist;
        Assert.IsFalse(status.IsConsistLead());
    }

    [TestMethod]
    public void IsConsistMember_ReturnsFalse_WhenNotInConsist()
    {
        var status = ConsistStatus.NotInConsist;
        Assert.IsFalse(status.IsConsistMember());
    }

    [TestMethod]
    public void IsConsistMember_ReturnsTrue_WhenSubMember()
    {
        var status = ConsistStatus.SubMember;
        Assert.IsTrue(status.IsConsistMember());
    }

    [TestMethod]
    public void IsConsistMember_ReturnsFalse_WhenConsistTop()
    {
        var status = ConsistStatus.ConsistTop;
        Assert.IsFalse(status.IsConsistMember());
    }

    [TestMethod]
    public void IsConsistMember_ReturnsTrue_WhenMidConsist()
    {
        var status = ConsistStatus.MidConsist;
        Assert.IsTrue(status.IsConsistMember());
    }

    [TestMethod]
    public void GetConsistRoleDescription_ReturnsExpectedText_ForAllStatuses()
    {
        Assert.AreEqual("Not in consist", ConsistStatus.NotInConsist.GetConsistRoleDescription());
        Assert.AreEqual("Consist member (sub)", ConsistStatus.SubMember.GetConsistRoleDescription());
        Assert.AreEqual("Consist lead", ConsistStatus.ConsistTop.GetConsistRoleDescription());
        Assert.AreEqual("Consist member (mid)", ConsistStatus.MidConsist.GetConsistRoleDescription());
    }

    [TestMethod]
    public void GetConsistStatus_SetsNoBits_ForNotInConsist()
    {
        byte stat1 = 0x00;
        byte result = ConsistStatus.NotInConsist.GetConsistStatus(stat1);

        // Bits 6 (0x40) and 3 (0x08) should both be clear
        Assert.AreEqual(0x00, result & 0x48);
    }

    [TestMethod]
    public void GetConsistStatus_SetsBit3Only_ForSubMember()
    {
        byte stat1 = 0x00;
        byte result = ConsistStatus.SubMember.GetConsistStatus(stat1);

        // Only bit 3 (SL_CONDN = 0x08) should be set
        Assert.AreEqual(0x08, result & 0x48);
    }

    [TestMethod]
    public void GetConsistStatus_SetsBit6Only_ForConsistTop()
    {
        byte stat1 = 0x00;
        byte result = ConsistStatus.ConsistTop.GetConsistStatus(stat1);

        // Only bit 6 (SL_CONUP = 0x40) should be set
        Assert.AreEqual(0x40, result & 0x48);
    }

    [TestMethod]
    public void GetConsistStatus_SetsBits6And3_ForMidConsist()
    {
        byte stat1 = 0x00;
        byte result = ConsistStatus.MidConsist.GetConsistStatus(stat1);

        // Both bits 6 and 3 (0x48) should be set
        Assert.AreEqual(0x48, result & 0x48);
    }

    [TestMethod]
    public void GetConsistStatus_PreservesOtherBits_WhenSettingStatus()
    {
        // STAT1 with other bits set: status bits (4-5), decoder type (0-2)
        byte stat1 = 0x37; // 0b00110111 - some status and decoder type bits

        byte result = ConsistStatus.SubMember.GetConsistStatus(stat1);

        // Consist bits should be 0x08, other bits preserved
        Assert.AreEqual(0x08, result & 0x48, "Consist bits");
        Assert.AreEqual(0x37, result & ~0x48, "Other bits preserved");
    }

    [TestMethod]
    public void GetConsistStatus_ClearsExistingConsistBits_WhenSettingNewStatus()
    {
        // Start with MidConsist (both bits set)
        byte stat1 = 0x48;

        // Change to SubMember
        byte result = ConsistStatus.SubMember.GetConsistStatus(stat1);

        // Should clear bit 6 and keep bit 3
        Assert.AreEqual(0x08, result & 0x48);
    }
}
