namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class ConsistStatusTests
{
    // ===== IsInConsist Tests =====

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

    // ===== IsConsistLead Tests =====

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

    // ===== IsConsistMember Tests =====

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

    // ===== GetConsistRoleDescription Tests =====

    [TestMethod]
    public void GetConsistRoleDescription_ReturnsExpectedText_ForAllStatuses()
    {
        Assert.AreEqual("Not in consist", ConsistStatus.NotInConsist.GetConsistRoleDescription());
        Assert.AreEqual("Consist member (sub)", ConsistStatus.SubMember.GetConsistRoleDescription());
        Assert.AreEqual("Consist lead", ConsistStatus.ConsistTop.GetConsistRoleDescription());
        Assert.AreEqual("Consist member (mid)", ConsistStatus.MidConsist.GetConsistRoleDescription());
    }

    // ===== GetConsistStatus (byte encoding) Tests =====

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

[TestClass]
public class ConsistStatusByteDecodingTests
{
    // ===== GetConsistStatus (byte decoding) Tests =====

    [TestMethod]
    public void GetConsistStatus_FromByte_ReturnsNotInConsist_WhenNoBitsSet()
    {
        byte stat1 = 0x00;
        Assert.AreEqual(ConsistStatus.NotInConsist, stat1.GetConsistStatus());
    }

    [TestMethod]
    public void GetConsistStatus_FromByte_ReturnsSubMember_WhenOnlyBit3Set()
    {
        byte stat1 = 0x08; // Bit 3 (SL_CONDN)
        Assert.AreEqual(ConsistStatus.SubMember, stat1.GetConsistStatus());
    }

    [TestMethod]
    public void GetConsistStatus_FromByte_ReturnsConsistTop_WhenOnlyBit6Set()
    {
        byte stat1 = 0x40; // Bit 6 (SL_CONUP)
        Assert.AreEqual(ConsistStatus.ConsistTop, stat1.GetConsistStatus());
    }

    [TestMethod]
    public void GetConsistStatus_FromByte_ReturnsMidConsist_WhenBothBitsSet()
    {
        byte stat1 = 0x48; // Both bits 6 and 3
        Assert.AreEqual(ConsistStatus.MidConsist, stat1.GetConsistStatus());
    }

    [TestMethod]
    public void GetConsistStatus_FromByte_IgnoresOtherBits()
    {
        // Full byte with other status/decoder bits, but no consist bits
        byte stat1 = 0x37; // 0b00110111
        Assert.AreEqual(ConsistStatus.NotInConsist, stat1.GetConsistStatus());

        // Same but with consist bits set
        byte stat1WithConsist = 0x7F; // All bits except bit 7
        Assert.AreEqual(ConsistStatus.MidConsist, stat1WithConsist.GetConsistStatus());
    }

    // ===== Round-trip Tests =====

    [TestMethod]
    public void ConsistStatus_RoundTrip_PreservesValue_NotInConsist()
    {
        byte original = 0x00;
        byte encoded = ConsistStatus.NotInConsist.GetConsistStatus(original);
        ConsistStatus decoded = encoded.GetConsistStatus();
        Assert.AreEqual(ConsistStatus.NotInConsist, decoded);
    }

    [TestMethod]
    public void ConsistStatus_RoundTrip_PreservesValue_SubMember()
    {
        byte original = 0x00;
        byte encoded = ConsistStatus.SubMember.GetConsistStatus(original);
        ConsistStatus decoded = encoded.GetConsistStatus();
        Assert.AreEqual(ConsistStatus.SubMember, decoded);
    }

    [TestMethod]
    public void ConsistStatus_RoundTrip_PreservesValue_ConsistTop()
    {
        byte original = 0x00;
        byte encoded = ConsistStatus.ConsistTop.GetConsistStatus(original);
        ConsistStatus decoded = encoded.GetConsistStatus();
        Assert.AreEqual(ConsistStatus.ConsistTop, decoded);
    }

    [TestMethod]
    public void ConsistStatus_RoundTrip_PreservesValue_MidConsist()
    {
        byte original = 0x00;
        byte encoded = ConsistStatus.MidConsist.GetConsistStatus(original);
        ConsistStatus decoded = encoded.GetConsistStatus();
        Assert.AreEqual(ConsistStatus.MidConsist, decoded);
    }

    [TestMethod]
    public void ConsistStatus_RoundTrip_PreservesOtherBits()
    {
        byte original = 0x37; // Other bits set
        byte encoded = ConsistStatus.ConsistTop.GetConsistStatus(original);
        ConsistStatus decoded = encoded.GetConsistStatus();

        Assert.AreEqual(ConsistStatus.ConsistTop, decoded);
        Assert.AreEqual(0x37, encoded & ~0x48, "Other bits should be preserved");
    }
}
