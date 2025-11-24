using System.Globalization;

namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Sent from Z21 as a response on <see cref="GetSubscribedNotificationsCommand"/>.
/// </summary>
/// <remarks>
///  Reference: Z21 LAN Protokoll Spezifikation 2.17
/// </remarks>
public sealed class BroadcastSubjectsNotification : Notification
{
    internal BroadcastSubjectsNotification(Frame frame) : base(frame)
    {
        Subjects = (BroadcastSubjects)BitConverter.ToInt32(frame.Data, 0);
    }

    public BroadcastSubjects Subjects { get; }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}: Subjects={1}", base.ToString(), Subjects);
    }
}
