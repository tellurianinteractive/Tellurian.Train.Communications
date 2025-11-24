namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

internal class NotSupportedNotification : Notification
{
    private readonly byte[] Buffer;
    private readonly string SourceBusName;

    public NotSupportedNotification(byte[] buffer, string sourceBusName)  : base(0x00)
    {
        Buffer = buffer;
        SourceBusName = sourceBusName;
    }
    public override string ToString() => $"{SourceBusName} {BitConverter.ToString(Buffer)}";
}