using XpressNet = Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Adapters.Z21;

[Flags]
public enum BroadcastSubjects
{
    /// <summary>
    /// Subscribes on no subjects at all.
    /// </summary>
    None = 0x00000000,
    /// <summary>
    /// Subscribes on LocoNet turnout notifications:
    /// </summary>
    RunningAndSwitching = 0x00000001,
    /// <summary>
    /// Suscribes on R-Bus changes.
    /// </summary>
    RbusFeedback = 0x00000002,
    /// <summary>
    /// Subscribes on following system change notifications: 
    /// <see cref="SystemStateChangeNotification"/>,
    /// <see cref="XpressNet.EmergencyStopBroadcast"/>,
    /// <see cref="XpressNet.ProgrammingModeBroadcast"/>,
    /// <see cref="XpressNet.TrackPowerOffBroadcast"/>, 
    /// <see cref="XpressNet.TrackPowerOnBroadcast"/>,
    /// <see cref="XpressNet.TrackShortCircuitNotification"/>,
    /// </summary>
    SystemStateChanges = 0x00000100,
    /// <summary>
    /// Subscribes on all locomotive change notifications.
    /// </summary>
    ChangedLocomotiveInfo = 0x00010000,
    /// <summary>
    /// Subscribes on LocoNet notifications, except for locomotives and turnouts.
    /// </summary>
    LocoNetExceptLocomotivesAndSwitches = 0x01000000,
    /// <summary>
    /// Subscribes on LocoNet notifciations for locomotives.
    /// </summary>
    LocoNetLocomotiveSpecific = 0x02000000,
    /// <summary>
    /// Subscribes on LocoNet notifications for turnouts.
    /// </summary>
    LocoNetTurnouts = 0x04000000,
    /// <summary>
    /// Subscribes on LocoNet notifications for occupied stretches.
    /// </summary>
    OccupiedStretch = 0x08000000,
    /// <summary>
    /// Subscribes on CAN detector change notifications (Z21 FW 1.30+).
    /// </summary>
    CanDetectorChanges = 0x00080000,
    /// <summary>
    /// Subscribes on all notifications from Z21.
    /// </summary>
    All =
        RunningAndSwitching |
        RbusFeedback |
        SystemStateChanges |
        ChangedLocomotiveInfo |
        CanDetectorChanges |
        LocoNetExceptLocomotivesAndSwitches |
        LocoNetLocomotiveSpecific |
        LocoNetTurnouts |
        OccupiedStretch
}
