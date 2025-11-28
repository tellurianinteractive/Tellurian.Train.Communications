namespace Tellurian.Trains.Protocols.LocoNet;

/// <summary>
/// Consist status derived from SL_CONUP (bit 6) and SL_CONDN (bit 3) of STAT1 byte.
/// </summary>
public enum ConsistStatus : byte
{
    /// <summary>
    /// Not in a consist (both bits = 0).
    /// </summary>
    NotInConsist = 0b00,

    /// <summary>
    /// Sub-member of consist, linked upward only (CONDN=1, CONUP=0).
    /// </summary>
    SubMember = 0b01,

    /// <summary>
    /// Consist top/lead locomotive, linked downward only (CONUP=1, CONDN=0).
    /// </summary>
    ConsistTop = 0b10,

    /// <summary>
    /// Mid-consist member, linked both up and down (both bits = 1).
    /// </summary>
    MidConsist = 0b11
}



