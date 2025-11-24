namespace Tellurian.Trains.Protocols.XpressNet;

#pragma warning disable CA1028 // Enum Storage should be Int32
[Flags]
public enum CentralStates: byte
{
    None = 0x00,
    EmergencyStop = 0x01,
    TrackVoltageOff = 0x02,
    ShortCircuit = 0x04,
    ProgrammingMode = 0x20
}
[Flags]
public enum ExtendedCentralStates : byte
{
    None = 0x00,
    HighTemperature = 0x01,
    PowerLost = 0x02,
    ShortCircuitBooster = 0x04,
    ShortCircuitMainOrProgramming = 0x08
}