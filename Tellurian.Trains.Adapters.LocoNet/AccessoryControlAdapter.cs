using Tellurian.Trains.Interfaces.Accessories;
using Tellurian.Trains.Protocols.LocoNet.Commands;

namespace Tellurian.Trains.Adapters.LocoNet;

public sealed partial class Adapter : IAccessory, ISwitch
{
    public Task<bool> SetAccessoryAsync(Address address, AccessoryCommand command, CancellationToken cancellationToken = default)
    {
        var position = command.Function;
        var output = command.Output;
        return SendAsync(new SetTurnoutCommand(address, position, output), cancellationToken);
    }

    public Task<bool> QueryAccessoryStateAsync(Address address, CancellationToken cancellationToken = default)
    {
        return SendAsync(new RequestSwitchStateCommand(address), cancellationToken);
    }

    public Task<bool> SetThrownAsync(Address address, bool activate = true, CancellationToken cancellationToken = default)
    {
        return SendAsync(SetTurnoutCommand.Throw(address, activate), cancellationToken);
    }

    public Task<bool> SetClosedAsync(Address address, bool activate = true, CancellationToken cancellationToken = default)
    {
        return SendAsync(SetTurnoutCommand.Close(address, activate), cancellationToken);
    }

    public Task<bool> TurnOffAsync(Address address, CancellationToken cancellationToken = default)
    {
        return SendAsync(SetTurnoutCommand.TurnOff(address), cancellationToken);
    }
}
