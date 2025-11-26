namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Multi-Unit and Double Header error message response (spec section 3.21).
/// This notification is sent when a MU or DH operation fails.
/// </summary>
/// <remarks>
/// Format: Header=0xE1, Data=[0x80+F]
/// Where F is the error code (1-8).
///
/// This response is sent when any of the following operations fail:
/// - Establish Double Header
/// - Dissolve Double Header
/// - Add locomotive to Multi-Unit
/// - Remove locomotive from Multi-Unit
/// </remarks>
public sealed class MUDHErrorNotification : Notification
{
    internal MUDHErrorNotification(byte[] buffer) : base(0xE1, GetData(buffer)) { }

    private static new byte[] GetData(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (buffer.Length < 2) throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer must contain at least 2 bytes");
        return [buffer[1]];
    }

    /// <summary>
    /// Gets the raw identification byte (0x80 + error code).
    /// </summary>
    public byte IdentificationByte => Data[0];

    /// <summary>
    /// Gets the error code (1-8).
    /// </summary>
    public MUDHErrorCode ErrorCode => (MUDHErrorCode)(IdentificationByte & 0x0F);

    /// <summary>
    /// Gets a human-readable error message.
    /// </summary>
    public string ErrorMessage => ErrorCode switch
    {
        MUDHErrorCode.NotOperatedByDevice => "One of the locomotives has not been operated by the XpressNet device or locomotive 0 was selected",
        MUDHErrorCode.OperatedByAnotherDevice => "One of the locomotives is being operated by another XpressNet device",
        MUDHErrorCode.AlreadyInConsist => "One of the locomotives is already in another Multi-Unit or Double Header",
        MUDHErrorCode.SpeedNotZero => "The speed of one of the locomotives is not zero",
        MUDHErrorCode.NotInMultiUnit => "The locomotive is not in a Multi-Unit",
        MUDHErrorCode.NotMultiUnitBaseAddress => "The locomotive address is not a Multi-Unit base address",
        MUDHErrorCode.CannotDelete => "It is not possible to delete the locomotive",
        MUDHErrorCode.StackFull => "The command station stack is full",
        _ => $"Unknown error code: {(int)ErrorCode}"
    };
}

/// <summary>
/// Error codes for Multi-Unit and Double Header operations.
/// </summary>
public enum MUDHErrorCode : byte
{
    /// <summary>
    /// One of the locomotives has not been operated by the XpressNet device
    /// assembling the Double Header/Multi-Unit, or locomotive 0 was selected.
    /// </summary>
    NotOperatedByDevice = 1,

    /// <summary>
    /// One of the locomotives of the Double Header/Multi-Unit is being operated
    /// by another XpressNet device.
    /// </summary>
    OperatedByAnotherDevice = 2,

    /// <summary>
    /// One of the locomotives is already in another Multi-Unit or Double Header.
    /// </summary>
    AlreadyInConsist = 3,

    /// <summary>
    /// The speed of one of the locomotives of the Double Header/Multi-Unit is not zero.
    /// </summary>
    SpeedNotZero = 4,

    /// <summary>
    /// The locomotive is not in a Multi-Unit.
    /// </summary>
    NotInMultiUnit = 5,

    /// <summary>
    /// The locomotive address is not a Multi-Unit base address.
    /// </summary>
    NotMultiUnitBaseAddress = 6,

    /// <summary>
    /// It is not possible to delete the locomotive.
    /// </summary>
    CannotDelete = 7,

    /// <summary>
    /// The command station stack is full.
    /// </summary>
    StackFull = 8
}
