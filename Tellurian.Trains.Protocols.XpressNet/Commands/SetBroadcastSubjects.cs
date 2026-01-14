using Tellurian.Trains.Communications.Interfaces.Extensions;

namespace Tellurian.Trains.Protocols.XpressNet.Commands;

/// <summary>
/// Sets the <see cref="BroadcastSubjects"/> for what Z21 should send <see cref="Notifications.Notification"/>
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.16
/// </remarks>
public sealed class SetBroadcastSubjects : Command
{
    public SetBroadcastSubjects(BroadcastSubjects subjects) : base(0x50, GetData(subjects)) { }

    private static byte[] GetData(BroadcastSubjects subjects)
    {
        var data = new byte[3];
        data[0] = 0x00;
        Array.ConstrainedCopy(((uint)subjects).ToLittleEndian(), 0, data, 1, 2);
        return data;
    }
}