using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Tellurian.Trains.Communications.Channels;
using Tellurian.Trains.Interfaces.Accessories;
using Tellurian.Trains.Interfaces.Decoder;
using Tellurian.Trains.Interfaces.Locos;
using Tellurian.Trains.Protocols.LocoNet;
using Tellurian.Trains.Protocols.LocoNet.Commands;
using Tellurian.Trains.Protocols.LocoNet.Notifications;

namespace Tellurian.Trains.Adapters.LocoNet;

/// <summary>
/// Adapter for LocoNet command stations via serial port.
/// Implements ILoco, IAccessory, ISwitch, and IDecoder interfaces using LocoNet protocol.
/// </summary>
/// <remarks>
/// LocoNet uses a slot-based architecture where each locomotive is assigned to a slot.
/// This adapter manages slot allocation and caching automatically.
/// </remarks>
public sealed partial class Adapter : IDisposable, IAsyncDisposable, IObservable<Interfaces.Notification>
{
    private readonly ILogger _logger;
    private readonly ICommunicationsChannel _channel;
    private readonly ActionObserver<CommunicationResult> _receivingObserver;
    private readonly Observers<Interfaces.Notification> _observers = new();
    private readonly ConcurrentDictionary<ushort, byte> _addressToSlotCache = new();
    private readonly ConcurrentDictionary<byte, SlotData> _slotDataCache = new();
    private readonly SemaphoreSlim _slotRequestSemaphore = new(1, 1);
    private bool _disposed;

    /// <summary>
    /// Creates a new LocoNet adapter.
    /// </summary>
    /// <param name="channel">The communication channel (typically SerialDataChannel with LocoNetFramer)</param>
    /// <param name="logger">Logger instance</param>
    public Adapter(ICommunicationsChannel channel, ILogger<Adapter> logger)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _receivingObserver = new ActionObserver<CommunicationResult>(ReceiveData, ReceiveError, ReceiveCompleted);
    }

    /// <summary>
    /// Subscribes to notifications from the adapter.
    /// </summary>
    public IDisposable Subscribe(IObserver<Interfaces.Notification> observer) => _observers.Subscribe(observer);

    /// <summary>
    /// Starts receiving messages from the LocoNet.
    /// </summary>
    public async Task StartReceiveAsync(CancellationToken cancellationToken = default)
    {
        _channel.Subscribe(_receivingObserver);
        await _channel.StartReceiveAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Started receiving LocoNet messages");
    }

    /// <summary>
    /// Sends a LocoNet command.
    /// </summary>
    public async Task<bool> SendAsync(Command command, CancellationToken cancellationToken = default)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));
        var data = command.GetBytesWithChecksum();
        _logger.LogDebug("Sending: {Data}", BitConverter.ToString(data));
        var result = await _channel.SendAsync(data, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess;
    }

    private void ReceiveData(CommunicationResult result)
    {
        if (result is not SuccessResult successResult) return;

        var data = successResult.Data();
        if (data.Length == 0) return;

        _logger.LogDebug("Received: {Data}", BitConverter.ToString(data));

        try
        {
            var message = LocoNetMessageFactory.Create(data);
            if (message is Notification notification)
            {
                ProcessNotification(notification);
                var mapped = MapToInterfaceNotification(notification);
                if (mapped is not null)
                {
                    _observers.Notify(mapped);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process received data");
        }
    }

    private void ProcessNotification(Notification notification)
    {
        switch (notification)
        {
            case SlotNotification slotNotification when slotNotification.IsProgrammingSlot:
                HandleProgrammingResponse(slotNotification);
                break;
            case SlotNotification slotNotification when slotNotification.IsLocomotiveSlot:
                HandleSlotNotification(slotNotification);
                break;
            case SwitchReportNotification switchReport:
                HandleSwitchReportNotification(switchReport);
                break;
        }
    }

    private void HandleSlotNotification(SlotNotification notification)
    {
        var slotData = notification.Data;
        _slotDataCache[slotData.SlotNumber] = slotData;

        if (slotData.Address > 0)
        {
            _addressToSlotCache[slotData.Address] = slotData.SlotNumber;
            _pendingSlotRequest?.TrySetResult(slotData);
        }

        _logger.LogDebug("Slot {Slot} updated: Address={Address}, Speed={Speed}, Direction={Direction}",
            slotData.SlotNumber, slotData.Address, slotData.Speed, slotData.Direction ? "FWD" : "REV");
    }

    private void HandleSwitchReportNotification(SwitchReportNotification notification)
    {
        _logger.LogDebug("Switch {Address}: {Direction}",
            notification.Address, notification.CurrentDirection);
    }

    private Interfaces.Notification? MapToInterfaceNotification(Notification notification)
    {
        // Map LocoNet notifications to interface notifications
        return notification switch
        {
            SlotNotification slot when slot.IsProgrammingSlot && slot.ProgrammingResult is not null =>
                slot.ProgrammingResult.IsSuccess
                    ? DecoderResponse.Success(slot.ProgrammingResult.CV)
                    : DecoderResponse.Timeout(),
            SlotNotification slot when slot.IsLocomotiveSlot =>
                CreateLocoNotification(slot),
            SwitchReportNotification switchReport =>
                CreateAccessoryNotification(switchReport),
            _ => null
        };
    }

    private static Interfaces.Locos.LocoMovementNotification CreateLocoNotification(SlotNotification slot)
    {
        return new Interfaces.Locos.LocoMovementNotification(
            Interfaces.Locos.Address.From(slot.Address),
            slot.Direction ? Interfaces.Locos.Direction.Forward : Interfaces.Locos.Direction.Backward,
            Interfaces.Locos.Speed.Set126((byte)(slot.Speed > 1 ? slot.Speed - 1 : 0)));
    }

    private static AccessoryNotification? CreateAccessoryNotification(SwitchReportNotification switchReport)
    {
        var position = switchReport.CurrentDirection;
        if (position is null) return null;
        return new AccessoryNotification(switchReport.Address, position.Value, DateTimeOffset.Now);
    }

    private void ReceiveError(Exception ex)
    {
        _logger.LogError(ex, "Receive error");
        _observers.Error(ex);
    }

    private void ReceiveCompleted()
    {
        _observers.Completed();
    }

    #region IDisposable and IAsyncDisposable

    public void Dispose()
    {
        if (_disposed) return;
        _slotRequestSemaphore.Dispose();
        _programmingSemaphore.Dispose();
        (_channel as IDisposable)?.Dispose();
        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _slotRequestSemaphore.Dispose();
        _programmingSemaphore.Dispose();
        if (_channel is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        else
            (_channel as IDisposable)?.Dispose();
        _disposed = true;
    }

    #endregion
}
