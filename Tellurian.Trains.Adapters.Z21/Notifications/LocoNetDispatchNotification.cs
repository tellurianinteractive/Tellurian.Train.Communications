using Tellurian.Trains.Interfaces.Locos;
using Tellurian.Trains.Protocols.XpressNet;

namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Sent from Z21 as a response to <see cref="LocoNetDispatchCommand"/>.
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 9.4
/// </remarks>
public sealed class LocoNetDispatchNotification : Notification
{
    internal LocoNetDispatchNotification(Frame frame) : base(frame)
    {
        Address = Address.From(BitConverter.ToInt16(frame.Data, 0));
        Slot = frame.Data[2];
    }
    public Address Address { get; }
    public byte Slot { get; }
    public bool IsSuccess => Slot > 0;
}
