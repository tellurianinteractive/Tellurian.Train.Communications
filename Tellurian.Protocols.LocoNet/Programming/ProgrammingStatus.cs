namespace Tellurian.Trains.Protocols.LocoNet.Programming;

/// <summary>
/// Programming status flags from PSTAT byte in response.
/// </summary>
[Flags]
public enum ProgrammingStatus : byte
{
    /// <summary>
    /// No errors - programming succeeded.
    /// </summary>
    Success = 0,

    /// <summary>
    /// No decoder detected on programming track (bit 0).
    /// </summary>
    NoDecoder = 0b0001,

    /// <summary>
    /// Write acknowledge fail - decoder didn't acknowledge write (bit 1).
    /// </summary>
    WriteAckFail = 0b0010,

    /// <summary>
    /// Read compare acknowledge fail - no read acknowledgment (bit 2).
    /// </summary>
    ReadAckFail = 0b0100,

    /// <summary>
    /// User aborted operation (bit 3).
    /// </summary>
    UserAborted = 0b1000
}
