namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Transfer Error notification (spec section 3.8).
/// Sent when the command station receives a message with an incorrect XOR checksum.
/// </summary>
/// <remarks>
/// Format: Header=0x61, Data=[0x80]
///
/// This error occurs when:
/// - The XOR byte is computed incorrectly
/// - The hardware handshake isn't taken into consideration
/// - Buffer overflow in the PC UART hardware (caused by driver software not processing FIFO)
///
/// A transfer error often entails additional error messages (e.g., timeout between PC and LI100F).
/// </remarks>
public sealed class TransferErrorNotification : Notification
{
    internal TransferErrorNotification() : base(0x61, 0x80) { }

    /// <summary>
    /// Gets a description of the error.
    /// </summary>
    public string Description => "Transfer error - XOR checksum validation failed";
}

/// <summary>
/// Command Station Busy notification (spec section 3.9).
/// Sent when the command station cannot process a request at the current time.
/// </summary>
/// <remarks>
/// Format: Header=0x61, Data=[0x81]
///
/// This response is sent when:
/// - The request cannot be answered at the present time
/// - The command cannot be placed on the track at the present time
///
/// This is normally rare but can occur when, for example, trying to switch
/// a large number of turnouts as fast as possible on a very busy layout.
///
/// XpressNet devices should look for this response and retry the request if received.
/// </remarks>
public sealed class CommandStationBusyNotification : Notification
{
    internal CommandStationBusyNotification() : base(0x61, 0x81) { }

    /// <summary>
    /// Gets a description of the response.
    /// </summary>
    public string Description => "Command station busy - retry the request";
}
