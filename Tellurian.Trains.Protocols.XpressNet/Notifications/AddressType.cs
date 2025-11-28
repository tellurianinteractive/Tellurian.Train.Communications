namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Type of address returned in an address retrieval response.
/// </summary>
public enum AddressType : byte
{
    /// <summary>
    /// Normal locomotive address.
    /// </summary>
    NormalLoco = 0,

    /// <summary>
    /// Locomotive is in a Double Header.
    /// </summary>
    InDoubleHeader = 1,

    /// <summary>
    /// Multi-Unit base address.
    /// </summary>
    MultiUnitBase = 2,

    /// <summary>
    /// Locomotive is in a Multi-Unit.
    /// </summary>
    InMultiUnit = 3,

    /// <summary>
    /// Zero address found as a result of the request.
    /// </summary>
    Zero = 4
}
