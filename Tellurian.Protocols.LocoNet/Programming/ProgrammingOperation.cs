namespace Tellurian.Trains.Protocols.LocoNet.Programming;

/// <summary>
/// Programming operation type (read or write).
/// </summary>
public enum ProgrammingOperation
{
    /// <summary>
    /// Read CV value from decoder.
    /// </summary>
    Read,

    /// <summary>
    /// Write CV value to decoder.
    /// </summary>
    Write
}
