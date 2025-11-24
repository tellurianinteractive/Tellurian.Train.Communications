namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Sent by Z21 in response to a <see cref="Commands.GetStatusCommand"/>.
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.12
/// </remarks>
public class StatusChangedNotification : Notification
{
    internal StatusChangedNotification(byte[] data) : base(0x62, data) { }

    public bool IsEmergencyStop => (Data[1] & (byte)CentralStates.EmergencyStop) > 0;
    public bool IsTrackVoltageOff => (Data[1] & (byte)CentralStates.TrackVoltageOff) > 0;
    public bool IsStortCircuit => (Data[1] & (byte)CentralStates.ShortCircuit) > 0;
    public bool IsProgrammingModeActive => (Data[1] & (byte)CentralStates.ProgrammingMode) > 0;
}
