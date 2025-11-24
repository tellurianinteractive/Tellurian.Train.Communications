namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Response from Z21 on a <see cref="Commands.GetBroadcastSubjects"/>
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.17
/// </remarks>
public sealed class BroadcastSubjectNotification : Notification
{
    internal BroadcastSubjectNotification(byte[] buffer) : base(0x51, buffer) { }

    public BroadcastSubjects SubscribedSubjects => (BroadcastSubjects)BitConverter.ToInt32(Data, 2);
}
