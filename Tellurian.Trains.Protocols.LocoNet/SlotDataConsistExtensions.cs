using Tellurian.Trains.Protocols.LocoNet.Commands;

namespace Tellurian.Trains.Protocols.LocoNet;

internal static class SlotDataConsistExtensions
{
    extension(SlotData slotData)
    {
        public bool IsInConsist()
        {
            return slotData != null && slotData.Consist.IsInConsist();
        }

        public bool CanBeLinked()
        {
            if (slotData == null)
                return false;

            return slotData.Consist == ConsistStatus.NotInConsist ||
                   slotData.Consist == ConsistStatus.ConsistTop;
        }
    }

    extension(byte leadSlot)
    {
        public LinkSlotsCommand[] BuildConsist(byte[] memberSlots)
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

        public ConsistStatus GetConsistStatus()
        {
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
