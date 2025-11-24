#pragma warning disable CA1028 // Enum Storage should be Int32

namespace Tellurian.Trains.Protocols.XpressNet;

[Flags]
public enum BroadcastSubjects : uint
{
    None = 0,
    TrackEvents = 0x0000_0001,
    FreebackBus = 0x0000_0002,
    RailcomSelectedLocos = 0x0000_0004,
    System = 0x0000_0100,
    LocoNetAllLocos = 0x0001_0000,
    RailcomAllLocos = 0x0004_0000,
    CanBusTrackOccupancy = 0x0008_0000,
    LocoNetExceptLocosAndTurnouts = 0x0100_0000,
    LocoNetLocos = 0x0200_0000,
    LocoNetTurnouts = 0x0400_0000,
    LocoNetFeedback = 0x0800_0000
}