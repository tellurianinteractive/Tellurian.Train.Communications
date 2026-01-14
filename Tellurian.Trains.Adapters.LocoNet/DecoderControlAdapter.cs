using Tellurian.Trains.Communications.Interfaces.Decoder;
using Tellurian.Trains.Protocols.LocoNet.Commands;
using Tellurian.Trains.Protocols.LocoNet.Notifications;
using Tellurian.Trains.Protocols.LocoNet.Programming;

namespace Tellurian.Trains.Adapters.LocoNet;

public sealed partial class Adapter : IDecoder
{
    private TaskCompletionSource<ProgrammingResult>? _pendingProgrammingRequest;
    private readonly object _programmingLock = new();
    private readonly SemaphoreSlim _programmingSemaphore = new(1, 1);

    public async Task<byte> ReadCVAsync(ushort number, CancellationToken cancellationToken = default)
    {
        await _programmingSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var tcs = new TaskCompletionSource<ProgrammingResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_programmingLock)
            {
                _pendingProgrammingRequest = tcs;
            }

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(10));

                var sent = await SendAsync(ProgrammingCommand.ReadCvService(number), cancellationToken).ConfigureAwait(false);
                if (!sent)
                {
                    throw new InvalidOperationException("Failed to send ReadCV command");
                }

                var result = await tcs.Task.WaitAsync(cts.Token).ConfigureAwait(false);

                if (!result.IsSuccess)
                {
                    throw new InvalidOperationException($"CV read failed: {result.StatusMessage}");
                }

                return result.CV.Value;
            }
            finally
            {
                lock (_programmingLock)
                {
                    _pendingProgrammingRequest = null;
                }
            }
        }
        finally
        {
            _programmingSemaphore.Release();
        }
    }

    public async Task WriteCVAsync(ushort number, byte value, CancellationToken cancellationToken = default)
    {
        await _programmingSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var tcs = new TaskCompletionSource<ProgrammingResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_programmingLock)
            {
                _pendingProgrammingRequest = tcs;
            }

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(10));

                var sent = await SendAsync(ProgrammingCommand.WriteCvService(number, value), cancellationToken).ConfigureAwait(false);
                if (!sent)
                {
                    throw new InvalidOperationException("Failed to send WriteCV command");
                }

                var result = await tcs.Task.WaitAsync(cts.Token).ConfigureAwait(false);

                if (!result.IsSuccess)
                {
                    throw new InvalidOperationException($"CV write failed: {result.StatusMessage}");
                }
            }
            finally
            {
                lock (_programmingLock)
                {
                    _pendingProgrammingRequest = null;
                }
            }
        }
        finally
        {
            _programmingSemaphore.Release();
        }
    }

    private void HandleProgrammingResponse(SlotNotification notification)
    {
        if (!notification.IsProgrammingSlot || notification.ProgrammingResult is null)
            return;

        TaskCompletionSource<ProgrammingResult>? tcs;
        lock (_programmingLock)
        {
            tcs = _pendingProgrammingRequest;
        }

        tcs?.TrySetResult(notification.ProgrammingResult);
    }
}
