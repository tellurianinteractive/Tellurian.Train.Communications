namespace Tellurian.Trains.Adapters.Z21;

[Flags]
public enum ExtendedCentralStatuses
{
    None = 0x00,
    HighTemperature = 0x01,
    PowerLost = 0x02,
    ShortCircuitExternal = 0x04,
    ShortCircuitInternal = 0x08
}
