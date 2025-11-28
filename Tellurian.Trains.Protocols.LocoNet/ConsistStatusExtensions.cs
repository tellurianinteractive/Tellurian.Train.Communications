namespace Tellurian.Trains.Protocols.LocoNet;

/// <summary>
/// Extension methods for managing LocoNet consists (multiple locomotives operating together).
/// </summary>
public static class ConsistStatusExtensions
{
    extension(ConsistStatus consistStatus)
    {
        /// <summary>
        /// Checks if a slot is in a consist based on its consist status.
        /// </summary>
        /// <returns>True if slot is part of a consist</returns>
        public bool IsInConsist()
        {
            return consistStatus != ConsistStatus.NotInConsist;
        }

        /// <summary>
        /// Checks if a slot is the lead (top) locomotive in a consist.
        /// </summary>
        /// <returns>True if this is the consist lead locomotive</returns>
        public bool IsConsistLead()
        {
            return consistStatus == ConsistStatus.ConsistTop;
        }

        /// <summary>
        /// Checks if a slot is a consist member (not the lead).
        /// </summary>
        /// <returns>True if this is a consist member (sub or mid)</returns>
        public bool IsConsistMember()
        {
            return consistStatus == ConsistStatus.SubMember ||
                   consistStatus == ConsistStatus.MidConsist;
        }

        /// <summary>
        /// Gets a description of the consist role based on consist status.
        /// </summary>
        /// <returns>Human-readable description</returns>
        public string GetConsistRoleDescription()
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
        /// Modifies STAT1 byte to set consist status.
        /// </summary>
        /// <param name="consistStatus">New consist status to set</param>
        /// <returns>Modified STAT1 byte</returns>
        public byte GetConsistStatus(byte leadSlot)
        {
            byte result = (byte)(leadSlot & ~0x48);

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
}



