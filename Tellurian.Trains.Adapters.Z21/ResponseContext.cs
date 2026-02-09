namespace Tellurian.Trains.Adapters.Z21;

internal class ResponseContext
{
    public ResponseContext(IEnumerable<CommandHandler> commandHandlers)
    {
        CommandHandlers = commandHandlers;
    }

    private readonly IEnumerable<CommandHandler> CommandHandlers;


    public static void OnNotification(Notification notification)
    {

    }


}

internal sealed class CommandHandler
{
    public CommandHandler(Type commandType, Type awaitedNotificationResponse)
    {
        CommandType = commandType;
        AwaitedNotificationResponse = awaitedNotificationResponse;
    }
    private readonly Type CommandType;
    private readonly Type AwaitedNotificationResponse;
}
