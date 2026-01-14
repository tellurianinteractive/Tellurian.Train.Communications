using Microsoft.Extensions.Logging;
using Tellurian.Trains.Communications.Interfaces.Locos;
using Tellurian.Trains.Protocols.LocoNet;
using Tellurian.Trains.Protocols.LocoNet.Commands;

namespace Tellurian.Trains.Adapters.LocoNet;

public sealed partial class Adapter : ILoco
{
    private TaskCompletionSource<SlotData>? _pendingSlotRequest;
    private readonly object _slotLock = new();

    public async Task<bool> DriveAsync(Address address, Drive drive, CancellationToken cancellationToken = default)
    {
        var slot = await GetOrRequestSlotAsync(address, cancellationToken).ConfigureAwait(false);
        if (slot is null) return false;

        var speedStep = (byte)(drive.Speed.CurrentStep + (drive.Speed.CurrentStep > 0 ? 1 : 0));
        var sent = await SendAsync(new SetLocoSpeedCommand(slot.SlotNumber, speedStep), cancellationToken).ConfigureAwait(false);
        if (!sent) return false;

        sent = await SendAsync(new SetLocoDirectionAndFunctionF0toF4Command(
            slot.SlotNumber,
            drive.Direction == Direction.Forward,
            slot.F0, slot.F1, slot.F2, slot.F3, slot.F4), cancellationToken).ConfigureAwait(false);

        return sent;
    }

    public async Task<bool> EmergencyStopAsync(Address address, CancellationToken cancellationToken = default)
    {
        var slot = await GetOrRequestSlotAsync(address, cancellationToken).ConfigureAwait(false);
        if (slot is null) return false;

        return await SendAsync(SetLocoSpeedCommand.Stop(slot.SlotNumber), cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> SetFunctionAsync(Address address, Function locoFunction, CancellationToken cancellationToken = default)
    {
        var slot = await GetOrRequestSlotAsync(address, cancellationToken).ConfigureAwait(false);
        if (slot is null) return false;

        var functionNumber = (int)locoFunction.Number;
        var isOn = locoFunction.IsOn;

        return functionNumber switch
        {
            <= 4 => await SetFunctionF0toF4Async(slot, functionNumber, isOn, cancellationToken).ConfigureAwait(false),
            <= 8 => await SetFunctionF5toF8Async(slot, functionNumber, isOn, cancellationToken).ConfigureAwait(false),
            <= 12 => await SetFunctionF9toF12Async(slot, functionNumber, isOn, cancellationToken).ConfigureAwait(false),
            _ => false
        };
    }

    private async Task<bool> SetFunctionF0toF4Async(SlotData slot, int function, bool isOn, CancellationToken cancellationToken)
    {
        var f0 = function == 0 ? isOn : slot.F0;
        var f1 = function == 1 ? isOn : slot.F1;
        var f2 = function == 2 ? isOn : slot.F2;
        var f3 = function == 3 ? isOn : slot.F3;
        var f4 = function == 4 ? isOn : slot.F4;

        return await SendAsync(new SetLocoDirectionAndFunctionF0toF4Command(
            slot.SlotNumber, slot.Direction, f0, f1, f2, f3, f4), cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> SetFunctionF5toF8Async(SlotData slot, int function, bool isOn, CancellationToken cancellationToken)
    {
        var f5 = function == 5 ? isOn : slot.F5;
        var f6 = function == 6 ? isOn : slot.F6;
        var f7 = function == 7 ? isOn : slot.F7;
        var f8 = function == 8 ? isOn : slot.F8;

        return await SendAsync(new SetLocoFunctionF5toF8Command(
            slot.SlotNumber, f5, f6, f7, f8), cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> SetFunctionF9toF12Async(SlotData slot, int function, bool isOn, CancellationToken cancellationToken)
    {
        var f9 = function == 9 && isOn;
        var f10 = function == 10 && isOn;
        var f11 = function == 11 && isOn;
        var f12 = function == 12 && isOn;

        return await SendAsync(new SetLocoFunctionF9toF12(
            slot.SlotNumber, f9, f10, f11, f12), cancellationToken).ConfigureAwait(false);
    }

    private async Task<SlotData?> GetOrRequestSlotAsync(Address address, CancellationToken cancellationToken)
    {
        if (_addressToSlotCache.TryGetValue((ushort)address.Number, out var slotNumber) &&
            _slotDataCache.TryGetValue(slotNumber, out var cachedSlot))
        {
            return cachedSlot;
        }

        return await RequestSlotAsync(address, cancellationToken).ConfigureAwait(false);
    }

    private async Task<SlotData?> RequestSlotAsync(Address address, CancellationToken cancellationToken)
    {
        await _slotRequestSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var tcs = new TaskCompletionSource<SlotData>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_slotLock)
            {
                _pendingSlotRequest = tcs;
            }

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(3));

                var sent = await SendAsync(new GetLocoAddressCommand(address), cancellationToken).ConfigureAwait(false);
                if (!sent)
                {
                    _logger.LogWarning("Failed to send GetLocoAddressCommand for address {Address}", address.Number);
                    return null;
                }

                var slotData = await tcs.Task.WaitAsync(cts.Token).ConfigureAwait(false);
                return slotData;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Timeout waiting for slot data for address {Address}", address.Number);
                return null;
            }
            finally
            {
                lock (_slotLock)
                {
                    _pendingSlotRequest = null;
                }
            }
        }
        finally
        {
            _slotRequestSemaphore.Release();
        }
    }
}
