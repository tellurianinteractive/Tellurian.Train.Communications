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

        // DCC accessory protocol requires an activate → delay → deactivate pair: without the
        // deactivate the decoder coil stays energised (risking motor burnout on twin-coil drives)
        // and the Z21 treats the command as still in-flight, suppressing LAN_X_TURNOUT_INFO
        // broadcasts for the same address until it's cleared — so UIs would miss subsequent
        // state changes. The adapter pairs the two sends itself so callers don't have to reason
        // about it. Hold time is governed by AccessoryActivationDurationMs; set it to a value
        // ≥ decoder travel time (≈200 ms twin-coil, 500–2000 ms stall-motor) or ≤ 0 to opt out
        // of pairing when the decoder self-deactivates.
        if (command.Output == MotorState.On)
        {
            var activated = await SendAsync(new AccessoryFunctionCommand(address, output, AccessoryOutputState.On, AccessoryZ21Mode.Direct), cancellationToken).ConfigureAwait(false);
            if (!activated) return false;
            if (AccessoryActivationDurationMs <= 0) return true;
            await Task.Delay(AccessoryActivationDurationMs, cancellationToken).ConfigureAwait(false);
            return await SendAsync(new AccessoryFunctionCommand(address, output, AccessoryOutputState.Off, AccessoryZ21Mode.Direct), cancellationToken).ConfigureAwait(false);
        }

        // Caller explicitly requested deactivate only (TurnOffAsync or AccessoryCommand.*(activate: false)).
        return await SendAsync(new AccessoryFunctionCommand(address, output, AccessoryOutputState.Off, AccessoryZ21Mode.Direct), cancellationToken).ConfigureAwait(false);
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
