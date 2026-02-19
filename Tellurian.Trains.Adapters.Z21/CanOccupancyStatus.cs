namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Occupancy status values for CAN detector type 0x01.
/// </summary>
public enum CanOccupancyStatus : ushort
{
    /// <summary>Track section is free, no voltage detected.</summary>
    Free = 0x0000,
    /// <summary>Track section is free but voltage is present.</summary>
    FreeWithVoltage = 0x0100,
    /// <summary>Track section is occupied, no voltage detected.</summary>
    Occupied = 0x1000,
    /// <summary>Track section is occupied with voltage detected.</summary>
    OccupiedWithVoltage = 0x1100,
    /// <summary>Overload level 1.</summary>
    Overload1 = 0x2000,
    /// <summary>Overload level 2.</summary>
    Overload2 = 0x2100,
    /// <summary>Overload level 3.</summary>
    Overload3 = 0x2200
}
