namespace Tellurian.Trains.Adapters.Z21;

public class ResponseContext
{
    public ResponseContext(IEnumerable<CommandHandler> commandHandlers)
    {
        CommandHandlers = commandHandlers;
    }
    
    private readonly IEnumerable<CommandHandler> CommandHandlers;


    public void OnNotification(Notification notification)
    {

    }


}

public sealed class CommandHandler
{
    public  CommandHandler(Type commandType, Type awaitedNotificationResponse )
    {
        CommandType = commandType;
        AwaitedNotificationResponse = awaitedNotificationResponse;
    }
    private readonly Type CommandType;
    private readonly Type AwaitedNotificationResponse;
}
