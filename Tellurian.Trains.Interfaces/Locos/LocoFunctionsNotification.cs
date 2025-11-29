namespace Tellurian.Trains.Interfaces.Locos;

public sealed class LocoFunctionsNotification(Address address, Function[] activeFunctions, DateTimeOffset timestamp) : LocoNotification(address, timestamp)
{
    private readonly Function[] _ActiveFunctions = activeFunctions ?? [];

    public LocoFunctionsNotification(Address address, Function[] activeFunctions) : this(address, activeFunctions, DateTimeOffset.Now) { }

    public IEnumerable<Function> ActiveFunctions => _ActiveFunctions;
}
