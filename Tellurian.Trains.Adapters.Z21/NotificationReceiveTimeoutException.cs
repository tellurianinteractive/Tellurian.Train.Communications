namespace Tellurian.Trains.Adapters.Z21;

[Serializable]
public class NotificationReceiveTimeoutException : Exception
{
    public NotificationReceiveTimeoutException() : base()
    {
    }

    public NotificationReceiveTimeoutException(string message) : base(message)
    {
    }

    public NotificationReceiveTimeoutException(string message, Exception innerException) : base(message, innerException)
    {
    }


}
