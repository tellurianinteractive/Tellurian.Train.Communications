using Microsoft.Extensions.Logging;
using Tellurian.Trains.Communications.Channels;
using Tellurian.Trains.Communications.Interfaces.Decoder;
using Tellurian.Trains.Protocols.XpressNet;
using Tellurian.Trains.Protocols.XpressNet.Commands;
using XpressNet = Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Adapters.Z21;

public sealed partial class Adapter : IDisposable, IObservable<Tellurian.Trains.Communications.Interfaces.Notification>
{
    private readonly ILogger Logger;
    private readonly ICommunicationsChannel Channel;
    private readonly ActionObserver<CommunicationResult> ReceivingObserver;
    private readonly Observers<Tellurian.Trains.Communications.Interfaces.Notification> Observers = new();
    private volatile bool IsReceiving;

    /// <summary>
    /// The broadcast subjects currently subscribed on the Z21 (as far as this adapter knows).
    /// Updated by <see cref="SubscribeAsync"/>, <see cref="AddSubscriptionsAsync"/>, and
    /// <see cref="RemoveSubscriptionsAsync"/>. Re-applied automatically on every
    /// <see cref="StartReceiveAsync"/> call (Z21 forgets broadcast flags across reconnects).
    /// </summary>
    public BroadcastSubjects CurrentSubscriptions { get; private set; }

    /// <summary>
    /// When true, the <see cref="Communications.Interfaces.Accessories.IAccessory"/> and
    /// <see cref="Communications.Interfaces.Accessories.ITurnout"/> implementations send their
    /// commands as wrapped LocoNet messages via <c>LAN_LOCONET_FROM_LAN</c> (0xA2) instead of
    /// native XpressNet. Feedback then arrives through the Z21's LocoNet-forwarding stream
    /// (subscribe <see cref="BroadcastSubjects.LocoNetTurnouts"/>), which is already mapped to
    /// the protocol-agnostic <see cref="Communications.Interfaces.Accessories.AccessoryNotification"/>.
    /// <para>
    /// Defaults to <c>true</c>. This path reaches accessory decoders on the Z21's LocoNet bus —
    /// the common case in the Tellurian ecosystem. Set to <c>false</c> if your accessory decoders
    /// are DCC-only (not on a LocoNet bus); XpressNet <c>LAN_X_SET_TURNOUT</c> will be used instead,
    /// at the cost of currently-unmapped feedback (a future release will map XpressNet
    /// <c>LAN_X_TURNOUT_INFO</c> to <see cref="Communications.Interfaces.Accessories.AccessoryNotification"/>).
    /// </para>
    /// </summary>
    public bool UseLocoNetForAccessories { get; }

    /// <summary>
    /// Time in milliseconds the native-XpressNet accessory path holds the activate between
    /// sending <c>A=1</c> and the paired <c>A=0</c> deactivate. Applies only when
    /// <see cref="UseLocoNetForAccessories"/> is <c>false</c>. Pick based on decoder type:
    /// twin-coil turnouts move in ~100–300 ms; stall motors (e.g. Möllehem) need the full
    /// travel time, typically 500–2000 ms. Defaults to 200 ms (twin-coil-friendly). A zero or
    /// negative value skips the deactivate — use only if the decoder self-deactivates AND you
    /// don't mind the Z21 suppressing further broadcasts for that address.
    /// </summary>
    public int AccessoryActivationDurationMs { get; }

    public Adapter(ICommunicationsChannel? channel, ILogger<Adapter> logger)
        : this(channel, logger, BroadcastSubjects.None, useLocoNetForAccessories: true, accessoryActivationDurationMs: 200) { }

    public Adapter(ICommunicationsChannel? channel, ILogger<Adapter> logger, BroadcastSubjects subscriptions)
        : this(channel, logger, subscriptions, useLocoNetForAccessories: true, accessoryActivationDurationMs: 200) { }

    /// <summary>
    /// Creates a new Z21 adapter.
    /// </summary>
    /// <param name="channel">The UDP communication channel to the Z21.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="subscriptions">
    /// Initial broadcast subjects to subscribe to when <see cref="StartReceiveAsync"/> is called.
    /// Use <see cref="BroadcastSubjects.None"/> to skip subscription. For accessory/turnout feedback
    /// via wrapped LocoNet notifications (the default accessory path, see <paramref name="useLocoNetForAccessories"/>),
    /// include <see cref="BroadcastSubjects.LocoNetTurnouts"/>. Can be changed at runtime via
    /// <see cref="SubscribeAsync"/>.
    /// </param>
    /// <param name="useLocoNetForAccessories">
    /// See <see cref="UseLocoNetForAccessories"/>. Defaults to <c>true</c>.
    /// </param>
    /// <param name="accessoryActivationDurationMs">
    /// See <see cref="AccessoryActivationDurationMs"/>. Defaults to 200.
    /// </param>
    public Adapter(
        ICommunicationsChannel? channel,
        ILogger<Adapter> logger,
        BroadcastSubjects subscriptions,
        bool useLocoNetForAccessories,
        int accessoryActivationDurationMs = 200)
    {
        Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ReceivingObserver = new ActionObserver<CommunicationResult>(ReceiveData, ReceiveError, ReceiveCompleted);
        CurrentSubscriptions = subscriptions;
        UseLocoNetForAccessories = useLocoNetForAccessories;
        AccessoryActivationDurationMs = accessoryActivationDurationMs;
    }

    public IDisposable Subscribe(IObserver<Tellurian.Trains.Communications.Interfaces.Notification> observer)
    {
        return Observers.Subscribe(observer);
    }

    private void Close()
    {
        (Channel as IDisposable)?.Dispose();
        Logger.LogInformation(new EventId(2109, nameof(Close)), "Adapter is closed.");
    }

    #region Commands

    public Task<bool> GetLocoInfoAsync(Tellurian.Trains.Communications.Interfaces.Locos.Address address, CancellationToken cancellationToken = default)
    {
        return SendAsync(new XpressNet.GetLocoInfoCommand(address), cancellationToken);
    }

    public Task<bool> GetSerialNumberAsync(CancellationToken cancellationToken = default)
    {
        return SendAsync(new GetSerialNumberCommand(), cancellationToken);
    }

    #endregion

    #region Send and receive

    public Task<bool> SendAsync(XpressNet.Command command, CancellationToken cancellationToken = default)
    {
        return SendAsync(new XpressNetCommand(command), cancellationToken);
    }

    public async Task<bool> SendAsync(Command command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        Logger.LogInformation(new EventId(2101, nameof(SendAsync)), "Command: {0}", command);
        var data = command.ToFrame().GetBytes();
        var result = await Channel.SendAsync(data, cancellationToken).ConfigureAwait(false);
        Logger.LogInformation(new EventId(2102, nameof(SendAsync)), "Result: {0}", result);
        return result.IsSuccess;
    }

    public async Task StartReceiveAsync(CancellationToken cancellationToken = default)
    {
        Channel.Subscribe(ReceivingObserver);
        await Channel.StartReceiveAsync(cancellationToken).ConfigureAwait(false);
        IsReceiving = true;
        Logger.LogInformation(new EventId(2103, nameof(StartReceiveAsync)), "Started receiving notifications.");
        if (CurrentSubscriptions != BroadcastSubjects.None)
        {
            await SendAsync(new SubscribeNotificationsCommand(CurrentSubscriptions), cancellationToken).ConfigureAwait(false);
            Logger.LogInformation(new EventId(2110, nameof(StartReceiveAsync)), "Subscribed to Z21 broadcast subjects: {Subjects}", CurrentSubscriptions);
        }
    }

    /// <summary>
    /// Sets the Z21 broadcast subscription to the given set of subjects (absolute replace).
    /// The Z21 protocol only supports a "set absolute" operation — any subjects not included
    /// here will no longer be delivered. If the adapter is already receiving, the command is
    /// sent immediately; otherwise the value is stored and applied on the next
    /// <see cref="StartReceiveAsync"/> call.
    /// </summary>
    public Task<bool> SubscribeAsync(BroadcastSubjects subjects, CancellationToken cancellationToken = default)
    {
        CurrentSubscriptions = subjects;
        if (!IsReceiving) return Task.FromResult(true);
        return SendAsync(new SubscribeNotificationsCommand(subjects), cancellationToken);
    }

    /// <summary>
    /// Adds the given subjects to the active subscription set. Convenience over
    /// <see cref="SubscribeAsync"/> that preserves the currently-subscribed subjects.
    /// </summary>
    public Task<bool> AddSubscriptionsAsync(BroadcastSubjects subjects, CancellationToken cancellationToken = default)
        => SubscribeAsync(CurrentSubscriptions | subjects, cancellationToken);

    /// <summary>
    /// Removes the given subjects from the active subscription set. Convenience over
    /// <see cref="SubscribeAsync"/> that preserves other currently-subscribed subjects.
    /// </summary>
    public Task<bool> RemoveSubscriptionsAsync(BroadcastSubjects subjects, CancellationToken cancellationToken = default)
        => SubscribeAsync(CurrentSubscriptions & ~subjects, cancellationToken);

    private void ReceiveData(CommunicationResult result)
    {
        Logger.LogInformation(new EventId(2103, nameof(ReceiveData)), "Result: {0}", result);
        foreach (var frame in Frame.CreateMany(result))
        {
            try
            {
                var n = frame.Notification();

                // Intercept XpressNet loco info notifications for request/response correlation
                if (n is XpressNetNotification xpressNetNotification &&
                    xpressNetNotification.Notification is Protocols.XpressNet.Notifications.LocoInfoNotification locoInfo)
                {
                    HandleLocoInfoNotification(locoInfo);
                }

                // Intercept LocoNet notifications for LNCV handling
                if (n is LocoNetNotification locoNetNotification)
                {
                    ProcessLocoNetMessage(locoNetNotification);
                }

                var notification = n.Map();
                if (notification.Length == 1 && notification[0] is DecoderResponse decoderResponse)
                {
                    HandleDecoderResponse(decoderResponse);
                    _ = SendAsync(new TrackPowerOnCommand());
                }
                Observers.Notify(notification);
            }
            catch (Exception ex)
            {
                Logger.LogError(new EventId(2104, nameof(ReceiveData)), ex, "Failed to process received data.");
                throw;
            }
        }
    }

    private void ProcessLocoNetMessage(LocoNetNotification locoNetNotification)
    {
        switch (locoNetNotification.Message)
        {
            case Protocols.LocoNet.Notifications.LncvNotification lncvNotification:
                HandleLncvNotification(lncvNotification);
                break;
            case Protocols.LocoNet.Notifications.LongAcknowledge ack when ack.ForOperationCode == Protocols.LocoNet.Commands.LncvCommand.OperationCode:
                HandleLncvWriteAcknowledge(ack);
                break;
        }
    }

    private void ReceiveError(Exception ex)
    {
        Logger.LogError(new EventId(2104, nameof(ReceiveError)), ex, "Failed to process received data.");
        Observers.Error(ex);
    }

    private void ReceiveCompleted()
    {
        Observers.Completed();
    }

    #endregion

    #region IDisposable Support
    private bool disposedValue = false;

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _lncvSemaphore.Dispose();
                Close();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
    #endregion
}
