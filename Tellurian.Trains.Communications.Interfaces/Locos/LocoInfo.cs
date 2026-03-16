namespace Tellurian.Trains.Communications.Interfaces.Locos;

/// <summary>
/// Represents the current state of a locomotive as reported by the command station.
/// </summary>
public sealed class LocoInfo
{
    public required Address Address { get; init; }
    public required Direction Direction { get; init; }
    public required Speed Speed { get; init; }
    public required bool[] FunctionStates { get; init; }
}
