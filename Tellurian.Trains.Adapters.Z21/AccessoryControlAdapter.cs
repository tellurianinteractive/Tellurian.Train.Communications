using Tellurian.Trains.Communications.Interfaces.Accessories;
using Tellurian.Trains.Protocols.XpressNet;
using Tellurian.Trains.Protocols.XpressNet.Commands;
using LocoNetCommands = Tellurian.Trains.Protocols.LocoNet.Commands;

namespace Tellurian.Trains.Adapters.Z21;

public sealed partial class Adapter : IAccessory, ITurnout
{
    public async Task<bool> SetAccessoryAsync(Address address, AccessoryCommand command, CancellationToken cancellationToken = default)
    {
        if (UseLocoNetForAccessories)
        {
            var bytes = new LocoNetCommands.SetAccessoryCommand(address, command.Function, command.Output).GetBytesWithChecksum();
            var result = await SendAsync(new LocoNetRawCommand(bytes), cancellationToken).ConfigureAwait(false);
            if (result)
            {
                // Z21 does not echo LAN_LOCONET_FROM_LAN back to the original sender (spec §9.3).
                // On LocoNet serial the sender hears its own write via bus loopback; mirror that
                // here so UIs see commanded-state feedback immediately. A real decoder reply
                // (OPC_SW_REP via LAN_LOCONET_Z21_RX) will arrive as its own AccessoryNotification
                // and override/confirm this value.
                Observers.Notify([new AccessoryNotification(address, command.Function, DateTimeOffset.Now)]);
            }
            return result;
        }

        var output = command.Function == Position.ClosedOrGreen ? AccessoryOutput.Port1 : AccessoryOutput.Port2;
        var state = command.Output == MotorState.On ? AccessoryOutputState.On : AccessoryOutputState.Off;
        return await SendAsync(new AccessoryFunctionCommand(address, output, state, AccessoryZ21Mode.Direct), cancellationToken).ConfigureAwait(false);
    }

    public Task<bool> QueryAccessoryStateAsync(Address address, CancellationToken cancellationToken = default)
    {
        if (UseLocoNetForAccessories)
        {
            var bytes = new LocoNetCommands.RequestAccessoryStateCommand(address).GetBytesWithChecksum();
            return SendAsync(new LocoNetRawCommand(bytes), cancellationToken);
        }

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
