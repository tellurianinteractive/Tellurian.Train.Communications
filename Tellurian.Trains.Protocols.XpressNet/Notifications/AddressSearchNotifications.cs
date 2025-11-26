namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Locomotive information response for address retrieval requests (spec section 3.17).
/// Sent as a response to address inquiry commands.
/// </summary>
/// <remarks>
/// Format: Header=0xE3, Data=[0x30+K, AH, AL]
///
/// K indicates the type of address returned:
/// - K=0: Normal locomotive address
/// - K=1: Locomotive is in a Double Header
/// - K=2: Multi-Unit base address
/// - K=3: Locomotive is in a Multi-Unit
/// - K=4: No address found (if AH/AL = 0x00)
/// </remarks>
public sealed class AddressRetrievalNotification : Notification
{
    internal AddressRetrievalNotification(byte[] buffer) : base(0xE3, GetData(buffer)) { }

    private static new byte[] GetData(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (buffer.Length < 4) throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer must contain at least 4 bytes");
        return [buffer[1], buffer[2], buffer[3]];
    }

    /// <summary>
    /// Gets the raw identification byte (0x30 + K).
    /// </summary>
    public byte IdentificationByte => Data[0];

    /// <summary>
    /// Gets the address type code (K value, 0-4).
    /// </summary>
    public AddressType AddressType => (AddressType)(IdentificationByte & 0x0F);

    /// <summary>
    /// Gets the address high byte.
    /// </summary>
    public byte AddressHigh => Data[1];

    /// <summary>
    /// Gets the address low byte.
    /// </summary>
    public byte AddressLow => Data[2];

    /// <summary>
    /// Gets whether an address was found.
    /// </summary>
    public bool AddressFound => AddressType != AddressType.NotFound;

    /// <summary>
    /// Gets the locomotive address if one was found.
    /// Returns null if no address was found.
    /// </summary>
    public LocoAddress? LocoAddress
    {
        get
        {
            if (!AddressFound || (AddressHigh == 0 && AddressLow == 0))
                return null;

            return new LocoAddress([AddressHigh, AddressLow]);
        }
    }
}

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
    /// No address found as a result of the request.
    /// </summary>
    NotFound = 4
}

/// <summary>
/// Locomotive is being operated by another device response (spec section 3.15).
/// Sent unrequested when another XpressNet device takes control of a locomotive.
/// </summary>
/// <remarks>
/// Format: Header=0xE3, Data=[0x40, AH, AL]
///
/// This notification is sent to the XpressNet device that previously had control
/// of a locomotive when another device takes over using a Locomotive operations request.
/// </remarks>
public sealed class LocoOperatedByAnotherDeviceNotification : Notification
{
    internal LocoOperatedByAnotherDeviceNotification(byte[] buffer) : base(0xE3, GetData(buffer)) { }

    private static new byte[] GetData(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (buffer.Length < 4) throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer must contain at least 4 bytes");
        return [buffer[1], buffer[2], buffer[3]];
    }

    /// <summary>
    /// Gets the address high byte.
    /// </summary>
    public byte AddressHigh => Data[1];

    /// <summary>
    /// Gets the address low byte.
    /// </summary>
    public byte AddressLow => Data[2];

    /// <summary>
    /// Gets the locomotive address that is now being operated by another device.
    /// </summary>
    public LocoAddress LocoAddress => new([AddressHigh, AddressLow]);
}
