namespace Tellurian.Trains.Protocols.XpressNet.Commands;

/// <summary>
/// All locos will stop, but track voltage will be on.
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.13
/// </remarks>
public sealed class EmergencyStopCommand : Command
{
    public EmergencyStopCommand() : base(0x80) { }
}
