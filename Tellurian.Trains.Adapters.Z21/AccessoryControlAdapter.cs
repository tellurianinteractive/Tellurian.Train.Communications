using Tellurian.Trains.Communications.Interfaces.Accessories;
using Tellurian.Trains.Protocols.XpressNet;
using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Adapters.Z21;

public sealed partial class Adapter : IAccessory, ITurnout
{
    public Task<bool> SetAccessoryAsync(Address address, AccessoryCommand command, CancellationToken cancellationToken = default)
    {
        var output = command.Function == Position.ClosedOrGreen ? AccessoryOutput.Port1 : AccessoryOutput.Port2;
        var state = command.Output == MotorState.On ? AccessoryOutputState.On : AccessoryOutputState.Off;
        return SendAsync(new AccessoryFunctionCommand(address, output, state, AccessoryZ21Mode.Direct), cancellationToken);
    }

    public Task<bool> QueryAccessoryStateAsync(Address address, CancellationToken cancellationToken = default)
    {
        return SendAsync(new AccessoryInfoRequestCommand(address), cancellationToken);
    }

    public Task<bool> SetThrownAsync(Address address, bool activate = true, CancellationToken cancellationToken = default)
    {
        return SetAccessoryAsync(address, AccessoryCommand.Throw(activate), cancellationToken);
    }

    public Task<bool> SetClosedAsync(Address address, bool activate = true, CancellationToken cancellationToken = default)
    {
        return SetAccessoryAsync(address, AccessoryCommand.Close(activate), cancellationToken);
    }

    public Task<bool> TurnOffAsync(Address address, CancellationToken cancellationToken = default)
    {
        return SetAccessoryAsync(address, AccessoryCommand.TurnOff(), cancellationToken);
    }
}
