using System.Text.Json.Serialization;

namespace Tellurian.Trains.Communications.Interfaces.Locos;

public sealed class LocoFunctionsNotification(Address address, Function[] activeFunctions, DateTimeOffset timestamp) : LocoNotification(address, timestamp)
{
    [JsonConstructor]
    public LocoFunctionsNotification(Address address, Function[] activeFunctions) : this(address, activeFunctions, DateTimeOffset.Now) { }

    public Function[] ActiveFunctions { get; } = activeFunctions ?? [];
}
