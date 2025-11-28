namespace Tellurian.Trains.Protocols.LocoNet.Tests;

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
