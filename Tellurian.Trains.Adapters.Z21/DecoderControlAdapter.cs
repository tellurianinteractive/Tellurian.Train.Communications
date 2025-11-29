using Tellurian.Trains.Interfaces.Decoder;
using Tellurian.Trains.Protocols.XpressNet.Decoder;

namespace Tellurian.Trains.Adapters.Z21;

public sealed partial class Adapter : IDecoder
{
    private TaskCompletionSource<DecoderResponse>? _pendingDecoderResponse;
    private readonly object _decoderLock = new();

    public async Task<byte> ReadCVAsync(ushort number, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<DecoderResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

        lock (_decoderLock)
        {
            _pendingDecoderResponse = tcs;
        }

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var sent = await SendAsync(new ReadCVCommand(number), cancellationToken).ConfigureAwait(false);
            if (!sent)
            {
                throw new InvalidOperationException("Failed to send ReadCV command");
            }

            var response = await tcs.Task.WaitAsync(cts.Token).ConfigureAwait(false);

            if (!response.IsSuccess)
            {
                throw new InvalidOperationException($"CV read failed: {response.ErrorReason}");
            }

            return response.CV.Value;
        }
        finally
        {
            lock (_decoderLock)
            {
                _pendingDecoderResponse = null;
            }
        }
    }

    public async Task WriteCVAsync(ushort number, byte value, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<DecoderResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

        lock (_decoderLock)
        {
            _pendingDecoderResponse = tcs;
        }

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var cv = new CV(number, value);
            var sent = await SendAsync(new WriteCVCommand(cv), cancellationToken).ConfigureAwait(false);
            if (!sent)
            {
                throw new InvalidOperationException("Failed to send WriteCV command");
            }

            var response = await tcs.Task.WaitAsync(cts.Token).ConfigureAwait(false);

            if (!response.IsSuccess)
            {
                throw new InvalidOperationException($"CV write failed: {response.ErrorReason}");
            }
        }
        finally
        {
            lock (_decoderLock)
            {
                _pendingDecoderResponse = null;
            }
        }
    }

    private void HandleDecoderResponse(DecoderResponse response)
    {
        TaskCompletionSource<DecoderResponse>? tcs;
        lock (_decoderLock)
        {
            tcs = _pendingDecoderResponse;
        }

        tcs?.TrySetResult(response);
    }
}
