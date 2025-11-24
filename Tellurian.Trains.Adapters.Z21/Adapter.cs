using Microsoft.Extensions.Logging;
using Tellurian.Communications.Channels;
using Tellurian.Trains.Interfaces.Decoder;
using Tellurian.Trains.Interfaces.Locos;
using Tellurian.Trains.Protocols.XpressNet;
using Tellurian.Trains.Protocols.XpressNet.Commands;
using XpressNet = Tellurian.Trains.Protocols.XpressNet.Commands;

#pragma warning disable CA1303 // Do not pass literals as localized parameters

namespace Tellurian.Trains.Adapters.Z21;

public sealed partial class Adapter : IDisposable, IObservable<Interfaces.Notification>
{
    private readonly ILogger Logger;
    private readonly ICommunicationsChannel Channel;
    private readonly ActionObserver<CommunicationResult> ReceivingObserver;
    private readonly Observers<Interfaces.Notification> Observers = new Observers<Interfaces.Notification>();

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

    private void Close() // Called by Dispose
    {
        (Channel as IDisposable)?.Dispose();
        Logger.LogInformation(new EventId(2109, nameof(Close)), "Adapter is closed.");
    }

    #region Commands

    public bool GetLocoInfo(Interfaces.Locos.LocoAddress address)
    {
        return Send(new XpressNet.GetLocoInfoCommand(address.Map()));
    }

    public bool GetSerialNumber()
    {
        return Send(new GetSerialNumberCommand());
    }

    #endregion

    #region Send and receive

    public bool Send(XpressNet.Command command)
    {
        return Send(new XpressNetCommand(command));
    }

    public bool Send(Command command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));
        Logger.LogInformation(new EventId(2101, nameof(Send)), "Command: {0}", command);
        var data = command.ToFrame().GetBytes();
        var result = Channel.Send(data);
        Logger.LogInformation(new EventId(2102, nameof(Send)), "Result: {0}", result);
        return result.IsSuccess;
    }

    public void StartReceive()
    {
        Channel.Subscribe(ReceivingObserver);
        Channel.StartReceive();
        Logger.LogInformation(new EventId(2103, nameof(StartReceive)), "Started receiving notifications.");
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
                    Send(new TrackPowerOnCommand());
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
    private bool disposedValue = false; // To detect redundant calls

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
    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        // GC.SuppressFinalize(this);
    }
    #endregion
}
