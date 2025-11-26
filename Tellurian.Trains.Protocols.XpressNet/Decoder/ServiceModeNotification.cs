using Tellurian.Trains.Interfaces.Decoder;
using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Decoder;

/// <summary>
/// Base class for service mode (programming track) result notifications.
/// </summary>
public abstract class ServiceModeNotification : Notification
{
    protected ServiceModeNotification(byte[] buffer) : base(0x63, GetData(buffer)) { }

    private static new byte[] GetData(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (buffer.Length < 4) throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer must contain at least 4 bytes");
        var result = new byte[buffer.Length - 1];
        Array.Copy(buffer, 1, result, 0, result.Length);
        return result;
    }
}

/// <summary>
/// Service Mode response for Register and Paged Mode (spec section 3.5.5).
/// This response is provided when reading a CV using Register or Paged mode.
/// </summary>
/// <remarks>
/// Format: Header=0x63, Data=[0x10, EE, DATA]
/// - EE: Register number (1-8) or CV number (1-256, with 256 as 0x00)
/// - DATA: The value read from the decoder
///
/// Note: If a Direct Mode request was sent and this response was received,
/// the command station determined that the decoder does not support Direct Mode
/// and has shifted into Register or Paged mode to obtain the result.
/// Subsequent requests to this decoder should use Register or Paged mode.
/// </remarks>
public sealed class ServiceModeRegisterPagedNotification : ServiceModeNotification
{
    internal ServiceModeRegisterPagedNotification(byte[] buffer) : base(buffer) { }

    /// <summary>
    /// Gets the register or CV number that was read.
    /// For Register mode: 1-8
    /// For Paged mode: 1-256 (256 is returned as 0)
    /// </summary>
    public byte RegisterOrCv => Data[1];

    /// <summary>
    /// Gets the CV number (1-256) as decoded from the response.
    /// </summary>
    public ushort CvNumber => RegisterOrCv == 0 ? (ushort)256 : RegisterOrCv;

    /// <summary>
    /// Gets the value read from the decoder.
    /// </summary>
    public byte Value => Data[2];

    /// <summary>
    /// Gets the CV number and value as a CV struct.
    /// </summary>
    public CV CV => new(CvNumber, Value);
}

/// <summary>
/// Service Mode response for Direct CV mode (spec section 3.5.6).
/// This response is provided when reading a CV using Direct CV mode.
/// </summary>
/// <remarks>
/// Format: Header=0x63, Data=[0x14, CV, DATA]
/// - CV: CV number (1-256, with 256 as 0x00)
/// - DATA: The value read from the decoder
///
/// If this response is received following a Direct Mode request, it can be assumed
/// that the decoder supports Direct Mode.
///
/// Note: If the decoder did not respond to the service mode request and the command
/// station was able to process the request using Paged or Register modes, a
/// ServiceModeRegisterPagedNotification will be sent instead.
/// </remarks>
public sealed class ServiceModeDirectCVNotification : ServiceModeNotification
{
    internal ServiceModeDirectCVNotification(byte[] buffer) : base(buffer) { }

    /// <summary>
    /// Gets the CV number (1-256) that was read.
    /// CV 256 is encoded as 0x00 in the response.
    /// </summary>
    public ushort CvNumber => Data[1] == 0 ? (ushort)256 : Data[1];

    /// <summary>
    /// Gets the raw CV byte as received (0x00 for CV 256).
    /// </summary>
    public byte CvByte => Data[1];

    /// <summary>
    /// Gets the value read from the decoder.
    /// </summary>
    public byte Value => Data[2];

    /// <summary>
    /// Gets the CV number and value as a CV struct.
    /// </summary>
    public CV CV => new(CvNumber, Value);
}
