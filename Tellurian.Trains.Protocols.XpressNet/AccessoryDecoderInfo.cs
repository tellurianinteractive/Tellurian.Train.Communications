namespace Tellurian.Trains.Protocols.XpressNet;

public class AccessoryDecoderInfo
{
    internal AccessoryDecoderInfo(byte address, byte info)
    {
        Address = address;
        Info = info;
    }

    public byte Address { get; }
    public byte Info { get; }
    public bool IsCompleted => (Info & 0x80) == 0;
    public bool IsWithFeedback => (Info & 0x60) > 0;
}