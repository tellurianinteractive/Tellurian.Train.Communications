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

        // NMRA S-9.2.1: C=0 (Output 1/Port1) = Diverging/Thrown, C=1 (Output 2/Port2) = Normal/Straight.
        var output = command.Function == Position.ClosedOrGreen ? AccessoryOutput.Port2 : AccessoryOutput.Port1;

        // Z21 tracks an in-flight activate per address and suppresses new commands until it's
        // cleared with a deactivate. Instead of scheduling a background deactivate after a delay
        // (which races with subsequent commands and whose broadcast can revert model state),
        // pre-clear the opposite output before activating the desired one. This ensures Z21's
        // in-flight tracking is clean regardless of what other clients (Z21 App, WLANMaus) did
        // previously. Self-deactivating decoders (e.g. Möllehem stall-motor drives) handle motor
        // timing internally so no post-activate deactivate is needed.
        if (command.Output == MotorState.On)
        {
            var oppositeOutput = output == AccessoryOutput.Port1 ? AccessoryOutput.Port2 : AccessoryOutput.Port1;
            await SendAsync(new AccessoryFunctionCommand(address, oppositeOutput, AccessoryOutputState.Off, AccessoryZ21Mode.Direct), cancellationToken).ConfigureAwait(false);
            return await SendAsync(new AccessoryFunctionCommand(address, output, AccessoryOutputState.On, AccessoryZ21Mode.Direct), cancellationToken).ConfigureAwait(false);
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
