using Tellurian.Trains.Communications.Interfaces.Locos;
using Tellurian.Trains.Protocols.XpressNet;
using Tellurian.Trains.Protocols.XpressNet.Commands;
using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Adapters.Z21;

public sealed partial class Adapter : ILoco
{
    private TaskCompletionSource<LocoInfoNotification>? _pendingLocoInfoRequest;
    private readonly object _locoInfoLock = new();

    public Task<bool> SetFunctionAsync(Tellurian.Trains.Communications.Interfaces.Locos.Address address, Function function, CancellationToken cancellationToken = default)
    {
        return SendAsync(new LocoFunctionCommand(address, (byte)function.Number, function.IsOn ? LocoFunctionStates.On : LocoFunctionStates.Off), cancellationToken);
    }

    public Task<bool> EmergencyStopAsync(Tellurian.Trains.Communications.Interfaces.Locos.Address address, CancellationToken cancellationToken = default)
    {
        return SendAsync(new LocoEmergencyStopCommand(address), cancellationToken);
    }

    public Task<bool> DriveAsync(Tellurian.Trains.Communications.Interfaces.Locos.Address address, Drive drive, CancellationToken cancellationToken = default)
    {
        return SendAsync(new LocoDriveCommand(address, drive.Speed.Map(), drive.Direction.Map()), cancellationToken);
    }

    async Task<LocoInfo?> ILoco.GetLocoInfoAsync(Tellurian.Trains.Communications.Interfaces.Locos.Address address, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<LocoInfoNotification>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_locoInfoLock)
        {
            _pendingLocoInfoRequest = tcs;
        }

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            var sent = await SendAsync(new GetLocoInfoCommand(address), cancellationToken).ConfigureAwait(false);
            if (!sent) return null;

            var notification = await tcs.Task.WaitAsync(cts.Token).ConfigureAwait(false);

            var functions = notification.Functions();
            var functionStates = new bool[29];
            foreach (var (number, isOn) in functions)
            {
                functionStates[number] = isOn;
            }

            return new LocoInfo
            {
                Address = notification.Address,
                Direction = notification.Direction.Map(),
                Speed = notification.Speed.Map(),
                FunctionStates = functionStates
            };
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        finally
        {
            lock (_locoInfoLock)
            {
                _pendingLocoInfoRequest = null;
            }
        }
    }

    internal void HandleLocoInfoNotification(LocoInfoNotification notification)
    {
        lock (_locoInfoLock)
        {
            _pendingLocoInfoRequest?.TrySetResult(notification);
        }
    }
}
