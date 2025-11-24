namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Sent from Z21 as response on <see cref="GetSerialNumberCommand"/>.
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.1
/// </remarks>
public sealed class SerialNumberNotification : Notification
{
    internal SerialNumberNotification(Frame frame) : base(frame)
    {
        SerialNumber = (int)BitConverter.ToUInt32(frame.Data, 0);
    }
    public int SerialNumber { get; }
}
