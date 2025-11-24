namespace Tellurian.Trains.Adapters.Z21;

public sealed class LocoNetNotification : Notification
{
    internal LocoNetNotification(Frame frame) : base(frame)
    {
        Data = frame.Data;
    }
    private readonly byte[] Data;
    public Protocols.LocoNet.Message? Message { get; }
    public override string ToString() => BitConverter.ToString(Data);
}
