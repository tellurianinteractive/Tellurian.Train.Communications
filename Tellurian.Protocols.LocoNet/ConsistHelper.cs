using Tellurian.Trains.Protocols.LocoNet.Commands;

namespace Tellurian.Trains.Protocols.LocoNet;

/// <summary>
/// Helper utilities for managing LocoNet consists (multiple locomotives operating together).
/// </summary>
public static class ConsistHelper
{
    /// <summary>
    /// Checks if a slot is in a consist based on its consist status.
    /// </summary>
    /// <param name="consistStatus">Consist status from slot data</param>
    /// <returns>True if slot is part of a consist</returns>
    public static bool IsInConsist(ConsistStatus consistStatus)
    {
        return consistStatus != ConsistStatus.NotInConsist;
    }

    /// <summary>
    /// Checks if a slot is the lead (top) locomotive in a consist.
    /// </summary>
    /// <param name="consistStatus">Consist status from slot data</param>
    /// <returns>True if this is the consist lead locomotive</returns>
    public static bool IsConsistLead(ConsistStatus consistStatus)
    {
        return consistStatus == ConsistStatus.ConsistTop;
    }

    /// <summary>
    /// Checks if a slot is a consist member (not the lead).
    /// </summary>
    /// <param name="consistStatus">Consist status from slot data</param>
    /// <returns>True if this is a consist member (sub or mid)</returns>
    public static bool IsConsistMember(ConsistStatus consistStatus)
    {
        return consistStatus == ConsistStatus.SubMember ||
               consistStatus == ConsistStatus.MidConsist;
    }

    /// <summary>
    /// Checks if a slot data represents a slot in a consist.
    /// </summary>
    /// <param name="slotData">Slot data to check</param>
    /// <returns>True if slot is in a consist</returns>
    public static bool IsInConsist(SlotData slotData)
    {
        return slotData != null && IsInConsist(slotData.Consist);
    }

    /// <summary>
    /// Checks if a slot can be linked to another slot.
    /// A slot can be linked if it's not already in a consist or if it's the consist top.
    /// </summary>
    /// <param name="slotData">Slot data to check</param>
    /// <returns>True if slot can be linked</returns>
    public static bool CanBeLinked(SlotData slotData)
    {
        if (slotData == null)
            return false;

        // Can link if not in consist, or if it's the consist top (can add more members)
        return slotData.Consist == ConsistStatus.NotInConsist ||
               slotData.Consist == ConsistStatus.ConsistTop;
    }

    /// <summary>
    /// Creates commands to build a consist from multiple locomotives.
    /// Returns a sequence of LinkSlotsCommand that should be executed in order.
    /// </summary>
    /// <param name="leadSlot">Lead locomotive slot (will control the consist)</param>
    /// <param name="memberSlots">Member locomotive slots (will follow the lead)</param>
    /// <returns>Array of link commands to execute in sequence</returns>
    public static LinkSlotsCommand[] BuildConsist(byte leadSlot, params byte[] memberSlots)
    {
        if (memberSlots == null || memberSlots.Length == 0)
            throw new ArgumentException("At least one member slot is required", nameof(memberSlots));

        var commands = new LinkSlotsCommand[memberSlots.Length];

        for (int i = 0; i < memberSlots.Length; i++)
        {
            commands[i] = new LinkSlotsCommand(memberSlots[i], leadSlot);
        }

        return commands;
    }

    /// <summary>
    /// Creates commands to unlink all members from a consist.
    /// Note: You need to track which slots are linked together.
    /// </summary>
    /// <param name="leadSlot">Lead locomotive slot</param>
    /// <param name="memberSlots">Member slots to unlink</param>
    /// <returns>Array of unlink commands</returns>
    public static UnlinkSlotsCommand[] BreakConsist(byte leadSlot, params byte[] memberSlots)
    {
        if (memberSlots == null || memberSlots.Length == 0)
            throw new ArgumentException("At least one member slot is required", nameof(memberSlots));

        var commands = new UnlinkSlotsCommand[memberSlots.Length];

        for (int i = 0; i < memberSlots.Length; i++)
        {
            commands[i] = new UnlinkSlotsCommand(memberSlots[i], leadSlot);
        }

        return commands;
    }

    /// <summary>
    /// Gets a description of the consist role based on consist status.
    /// </summary>
    /// <param name="consistStatus">Consist status from slot data</param>
    /// <returns>Human-readable description</returns>
    public static string GetConsistRoleDescription(ConsistStatus consistStatus)
    {
        return consistStatus switch
        {
            ConsistStatus.NotInConsist => "Not in consist",
            ConsistStatus.SubMember => "Consist member (sub)",
            ConsistStatus.ConsistTop => "Consist lead",
            ConsistStatus.MidConsist => "Consist member (mid)",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Extracts consist flags from STAT1 byte.
    /// </summary>
    /// <param name="stat1">STAT1 byte from slot data</param>
    /// <returns>Consist status</returns>
    public static ConsistStatus GetConsistStatus(byte stat1)
    {
        // Extract SL_CONUP (bit 6) and SL_CONDN (bit 3)
        bool conUp = (stat1 & 0x40) != 0;
        bool conDn = (stat1 & 0x08) != 0;

        return (conUp, conDn) switch
        {
            (false, false) => ConsistStatus.NotInConsist,
            (false, true) => ConsistStatus.SubMember,
            (true, false) => ConsistStatus.ConsistTop,
            (true, true) => ConsistStatus.MidConsist
        };
    }

    /// <summary>
    /// Modifies STAT1 byte to set consist status.
    /// </summary>
    /// <param name="stat1">Original STAT1 byte</param>
    /// <param name="consistStatus">New consist status to set</param>
    /// <returns>Modified STAT1 byte</returns>
    public static byte SetConsistStatus(byte stat1, ConsistStatus consistStatus)
    {
        // Clear consist bits (bit 6 and bit 3)
        byte result = (byte)(stat1 & ~0x48);

        // Set new consist bits based on status
        return consistStatus switch
        {
            ConsistStatus.SubMember => (byte)(result | 0x08),   // SL_CONDN
            ConsistStatus.ConsistTop => (byte)(result | 0x40),  // SL_CONUP
            ConsistStatus.MidConsist => (byte)(result | 0x48),  // Both SL_CONUP and SL_CONDN
            ConsistStatus.NotInConsist => result,                // Bits already cleared
            _ => result
        };
    }
}
