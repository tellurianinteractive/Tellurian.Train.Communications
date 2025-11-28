namespace Tellurian.Trains.Protocols.LocoNet;

internal static class ConsistStatusExtensions
{
    extension(ConsistStatus consistStatus)
    {
        public bool IsInConsist()
        {
            return consistStatus != ConsistStatus.NotInConsist;
        }

        public bool IsConsistLead()
        {
            return consistStatus == ConsistStatus.ConsistTop;
        }

        public bool IsConsistMember()
        {
            return consistStatus == ConsistStatus.SubMember ||
                   consistStatus == ConsistStatus.MidConsist;
        }

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



