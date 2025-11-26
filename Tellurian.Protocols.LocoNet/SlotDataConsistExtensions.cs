using Tellurian.Trains.Protocols.LocoNet.Commands;

namespace Tellurian.Trains.Protocols.LocoNet;

public static class SlotDataConsistExtensions
{
    // Extensions for SlotData
    extension(SlotData slotData)
    {
        /// <summary>
        /// Checks if a slot data represents a slot in a consist.
        /// </summary>
        /// <returns>True if slot is in a consist</returns>
        public bool IsInConsist()
        {
            return slotData != null && slotData.Consist.IsInConsist();
        }

        /// <summary>
        /// Checks if a slot can be linked to another slot.
        /// A slot can be linked if it's not already in a consist or if it's the consist top.
        /// </summary>
        /// <returns>True if slot can be linked</returns>
        public bool CanBeLinked()
        {
            if (slotData == null)
                return false;

            // Can link if not in consist, or if it's the consist top (can add more members)
            return slotData.Consist == ConsistStatus.NotInConsist ||
                   slotData.Consist == ConsistStatus.ConsistTop;
        }
    }

    // Extensions for byte (slot operations)
    extension(byte leadSlot)
    {
        /// <summary>
        /// Creates commands to build a consist from multiple locomotives.
        /// Returns a sequence of LinkSlotsCommand that should be executed in order.
        /// </summary>
        /// <param name="memberSlots">Member locomotive slots (will follow the lead)</param>
        /// <returns>Array of link commands to execute in sequence</returns>
        public LinkSlotsCommand[] BuildConsist(params byte[] memberSlots)
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
        /// Extracts consist flags from STAT1 byte.
        /// </summary>
        /// <returns>Consist status</returns>
        public ConsistStatus GetConsistStatus()
        {
            // Extract SL_CONUP (bit 6) and SL_CONDN (bit 3)
            bool conUp = (leadSlot & 0x40) != 0;
            bool conDn = (leadSlot & 0x08) != 0;

            return (conUp, conDn) switch
            {
                (false, false) => ConsistStatus.NotInConsist,
                (false, true) => ConsistStatus.SubMember,
                (true, false) => ConsistStatus.ConsistTop,
                (true, true) => ConsistStatus.MidConsist
            };
        }

        /// <summary>
        /// Creates commands to unlink all members from a consist.
        /// Note: You need to track which slots are linked together.
        /// </summary>
        /// <param name="memberSlots">Member slots to unlink</param>
        /// <returns>Array of unlink commands</returns>
        public UnlinkSlotsCommand[] BreakConsist(byte[]? memberSlots)
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
    }
}
