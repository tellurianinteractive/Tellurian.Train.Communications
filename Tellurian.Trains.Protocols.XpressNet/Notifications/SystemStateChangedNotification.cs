using System.Globalization;

namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Response from Z21 on a <see cref="Commands.GetSystemStatusCommand"/>
/// or when subscribed on with <see cref="Commands.SetBroadcastSubjects"/>.
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.18
/// </remarks>
public class SystemStateChangedNotification : Notification
{
    internal SystemStateChangedNotification(byte[] data) : base(0x84, data) { }

    public short MainCurrent => BitConverter.ToInt16(Data, 0);
    public short ProgrammingCurrent => BitConverter.ToInt16(Data, 2);
    public short FilteredMainCurrent => BitConverter.ToInt16(Data, 4);
    public short Temperature => BitConverter.ToInt16(Data, 6);
    public int SupplyVoltage => BitConverter.ToUInt16(Data, 8);
    public int TrackVoltage => BitConverter.ToUInt16(Data, 10);
    public CentralStates CentralStates => (CentralStates)Data[12];
    public ExtendedCentralStates ExtendedCentralStates => (ExtendedCentralStates)Data[13];

    public override string ToString() =>
        string.Format(CultureInfo.CurrentCulture, Resources.Strings.SystemStateFormat, MainCurrent, TrackVoltage, ProgrammingCurrent, Temperature);
}
