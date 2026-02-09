using Tellurian.Trains.Communications.Interfaces.Accessories;
using Tellurian.Trains.Protocols.LocoNet.Commands;

namespace Tellurian.Trains.Adapters.LocoNet;

public sealed partial class Adapter : IAccessory, ITurnout
{
    public Task<bool> SetAccessoryAsync(Address address, AccessoryCommand command, CancellationToken cancellationToken = default)
    {
        var position = command.Function;
        var output = command.Output;
        return SendAsync(new SetAccessoryCommand(address, position, output), cancellationToken);
    }

    public Task<bool> QueryAccessoryStateAsync(Address address, CancellationToken cancellationToken = default)
    {
        return SendAsync(new RequestAccessoryStateCommand(address), cancellationToken);
    }

    public Task<bool> SetThrownAsync(Address address, bool activate = true, CancellationToken cancellationToken = default)
    {
        return SendAsync(SetAccessoryCommand.Throw(address, activate), cancellationToken);
    }

    public Task<bool> SetClosedAsync(Address address, bool activate = true, CancellationToken cancellationToken = default)
    {
        return SendAsync(SetAccessoryCommand.Close(address, activate), cancellationToken);
    }

    public Task<bool> TurnOffAsync(Address address, CancellationToken cancellationToken = default)
    {
        return SendAsync(SetAccessoryCommand.TurnOff(address), cancellationToken);
    }
}
