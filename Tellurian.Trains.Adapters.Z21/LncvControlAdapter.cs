using Microsoft.Extensions.Logging;
using Tellurian.Trains.Protocols.LocoNet.Commands;
using Tellurian.Trains.Protocols.LocoNet.Lncv;
using Tellurian.Trains.Protocols.LocoNet.Notifications;

namespace Tellurian.Trains.Adapters.Z21;

public sealed partial class Adapter
{
    private TaskCompletionSource<LncvNotification>? _pendingLncvReadRequest;
    private TaskCompletionSource<LongAcknowledge>? _pendingLncvWriteRequest;
    private List<LncvDeviceInfo>? _discoveryResults;
    private readonly object _lncvLock = new();
    private readonly SemaphoreSlim _lncvSemaphore = new(1, 1);

    /// <summary>
    /// Starts an LNCV programming session for a specific device.
    /// </summary>
    /// <returns>Device info on success, null on timeout.</returns>
    public async Task<LncvDeviceInfo?> StartLncvSessionAsync(ushort articleNumber, ushort moduleAddress, CancellationToken cancellationToken = default)
    {
        await _lncvSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var tcs = new TaskCompletionSource<LncvNotification>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_lncvLock)
            {
                _pendingLncvReadRequest = tcs;
            }

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                var sent = await SendLncvAsync(LncvCommand.StartSession(articleNumber, moduleAddress), cancellationToken).ConfigureAwait(false);
                if (!sent)
                {
                    Logger.LogWarning("Failed to send LNCV start session for article {Article}, module {Module}", articleNumber, moduleAddress);
                    return null;
                }

                var notification = await tcs.Task.WaitAsync(cts.Token).ConfigureAwait(false);
                Logger.LogInformation("LNCV session started: {Device}", new LncvDeviceInfo(notification.ArticleNumber, notification.ModuleAddress));
                return new LncvDeviceInfo(notification.ArticleNumber, notification.ModuleAddress);
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("LNCV start session timed out for article {Article}, module {Module}", articleNumber, moduleAddress);
                return null;
            }
            finally
            {
                lock (_lncvLock)
                {
                    _pendingLncvReadRequest = null;
                }
            }
        }
        finally
        {
            _lncvSemaphore.Release();
        }
    }

    /// <summary>
    /// Reads an LNCV value from a device.
    /// </summary>
    public async Task<Lncv> ReadLncvAsync(ushort articleNumber, ushort cvNumber, ushort moduleAddress, CancellationToken cancellationToken = default)
    {
        await _lncvSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var tcs = new TaskCompletionSource<LncvNotification>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_lncvLock)
            {
                _pendingLncvReadRequest = tcs;
            }

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                var sent = await SendLncvAsync(LncvCommand.Read(articleNumber, cvNumber, moduleAddress), cancellationToken).ConfigureAwait(false);
                if (!sent)
                {
                    throw new InvalidOperationException("Failed to send LNCV read command");
                }

                var notification = await tcs.Task.WaitAsync(cts.Token).ConfigureAwait(false);
                return new Lncv(notification.CvNumber, notification.CvValue);
            }
            finally
            {
                lock (_lncvLock)
                {
                    _pendingLncvReadRequest = null;
                }
            }
        }
        finally
        {
            _lncvSemaphore.Release();
        }
    }

    /// <summary>
    /// Writes an LNCV value to a device.
    /// </summary>
    /// <returns>True if the write was accepted.</returns>
    public async Task<bool> WriteLncvAsync(ushort articleNumber, ushort cvNumber, ushort value, CancellationToken cancellationToken = default)
    {
        await _lncvSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var tcs = new TaskCompletionSource<LongAcknowledge>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_lncvLock)
            {
                _pendingLncvWriteRequest = tcs;
            }

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                var sent = await SendLncvAsync(LncvCommand.Write(articleNumber, cvNumber, value), cancellationToken).ConfigureAwait(false);
                if (!sent)
                {
                    Logger.LogWarning("Failed to send LNCV write command for article {Article}, LNCV{Cv}={Value}", articleNumber, cvNumber, value);
                    return false;
                }

                var ack = await tcs.Task.WaitAsync(cts.Token).ConfigureAwait(false);
                return ack.IsSuccess;
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("LNCV write timed out for article {Article}, LNCV{Cv}={Value}", articleNumber, cvNumber, value);
                return false;
            }
            finally
            {
                lock (_lncvLock)
                {
                    _pendingLncvWriteRequest = null;
                }
            }
        }
        finally
        {
            _lncvSemaphore.Release();
        }
    }

    /// <summary>
    /// Ends an LNCV programming session.
    /// </summary>
    public Task<bool> EndLncvSessionAsync(ushort articleNumber, ushort moduleAddress, CancellationToken cancellationToken = default)
    {
        if (Logger.IsEnabled(LogLevel.Debug))
            Logger.LogDebug("Ending LNCV session for article {Article}, module {Module}", articleNumber, moduleAddress);
        return SendLncvAsync(new LncvEndSessionCommand(articleNumber, moduleAddress), cancellationToken);
    }

    /// <summary>
    /// Discovers LNCV devices of a specific type on the LocoNet bus.
    /// </summary>
    /// <param name="articleNumber">Product code to discover.</param>
    /// <param name="timeout">Discovery timeout (default 3 seconds).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Array of discovered devices.</returns>
    public async Task<LncvDeviceInfo[]> DiscoverLncvDevicesAsync(ushort articleNumber, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var discoveryTimeout = timeout ?? TimeSpan.FromSeconds(3);
        await _lncvSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            lock (_lncvLock)
            {
                _discoveryResults = [];
            }

            try
            {
                var sent = await SendLncvAsync(LncvCommand.StartSession(articleNumber, 0xFFFF), cancellationToken).ConfigureAwait(false);
                if (!sent)
                {
                    Logger.LogWarning("Failed to send LNCV discovery broadcast for article {Article}", articleNumber);
                    return [];
                }

                await Task.Delay(discoveryTimeout, cancellationToken).ConfigureAwait(false);

                LncvDeviceInfo[] results;
                lock (_lncvLock)
                {
                    results = [.. _discoveryResults];
                }

                await EndLncvSessionAsync(articleNumber, 0xFFFF, cancellationToken).ConfigureAwait(false);
                Logger.LogInformation("LNCV discovery for article {Article} found {Count} device(s)", articleNumber, results.Length);
                return results;
            }
            finally
            {
                lock (_lncvLock)
                {
                    _discoveryResults = null;
                }
            }
        }
        finally
        {
            _lncvSemaphore.Release();
        }
    }

    private Task<bool> SendLncvAsync(Protocols.LocoNet.Commands.Command lncvCommand, CancellationToken cancellationToken)
    {
        return SendAsync(new LocoNetRawCommand(lncvCommand.GetBytesWithChecksum()), cancellationToken);
    }

    private void HandleLncvNotification(LncvNotification notification)
    {
        if (Logger.IsEnabled(LogLevel.Debug))
            Logger.LogDebug("LNCV notification received: {Notification}", notification);

        lock (_lncvLock)
        {
            if (_discoveryResults is not null && notification.LncvType == LncvMessageType.SessionAcknowledgment)
            {
                _discoveryResults.Add(new LncvDeviceInfo(notification.ArticleNumber, notification.ModuleAddress));
                return;
            }
        }

        if (notification.LncvType == LncvMessageType.SessionAcknowledgment || notification.LncvType == LncvMessageType.ReadReply)
        {
            TaskCompletionSource<LncvNotification>? tcs;
            lock (_lncvLock)
            {
                tcs = _pendingLncvReadRequest;
            }
            tcs?.TrySetResult(notification);
        }
    }

    private void HandleLncvWriteAcknowledge(LongAcknowledge ack)
    {
        if (ack.ForOperationCode != LncvCommand.OperationCode) return;

        if (ack.IsSuccess)
        {
            if (Logger.IsEnabled(LogLevel.Debug))
                Logger.LogDebug("LNCV write acknowledged: {Message}", ack.Message);
        }
        else
        {
            Logger.LogWarning("LNCV write rejected: {Message}", ack.Message);
        }

        TaskCompletionSource<LongAcknowledge>? tcs;
        lock (_lncvLock)
        {
            tcs = _pendingLncvWriteRequest;
        }
        tcs?.TrySetResult(ack);
    }
}
