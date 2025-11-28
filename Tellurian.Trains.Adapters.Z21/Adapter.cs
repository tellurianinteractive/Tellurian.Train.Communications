using Microsoft.Extensions.Logging;
using Tellurian.Trains.Communications.Channels;
using Tellurian.Trains.Interfaces.Decoder;
using Tellurian.Trains.Interfaces.Locos;
using Tellurian.Trains.Protocols.XpressNet;
using Tellurian.Trains.Protocols.XpressNet.Commands;
using XpressNet = Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Adapters.Z21;

public sealed partial class Adapter : IDisposable, IObservable<Interfaces.Notification>
{
    private readonly ILogger Logger;
    private readonly ICommunicationsChannel Channel;
    private readonly ActionObserver<CommunicationResult> ReceivingObserver;
    private readonly Observers<Interfaces.Notification> Observers = new();

    public Adapter(ICommunicationsChannel? channel, ILogger<Adapter> logger)
    {
        Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ReceivingObserver = new ActionObserver<CommunicationResult>(ReceiveData, ReceiveError, ReceiveCompleted);
    }

    public IDisposable Subscribe(IObserver<Interfaces.Notification> observer)
    {
        return Observers.Subscribe(observer);
    }

    private void Close()
    {
        (Channel as IDisposable)?.Dispose();
        Logger.LogInformation(new EventId(2109, nameof(Close)), "Adapter is closed.");
    }

    #region Commands

    public Task<bool> GetLocoInfoAsync(Interfaces.Locos.LocoAddress address, CancellationToken cancellationToken = default)
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
        if (command is null) throw new ArgumentNullException(nameof(command));
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
        Logger.LogInformation(new EventId(2103, nameof(StartReceiveAsync)), "Started receiving notifications.");
    }

    private void ReceiveData(CommunicationResult result)
    {
        Logger.LogInformation(new EventId(2103, nameof(ReceiveData)), "Result: {0}", result);
        foreach (var frame in Frame.CreateMany(result))
        {
            try
            {
                var n = frame.Notification();
                var notification = n.Map();
                if (notification.Length == 1 && notification[0] is DecoderResponse)
                {
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
