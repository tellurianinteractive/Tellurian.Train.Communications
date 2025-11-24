using Tellurian.Trains.Protocols.XpressNet;

namespace Tellurian.Trains.Adapters.Z21;

public abstract class Command : Message
{
    internal abstract Frame ToFrame();
}

public sealed class XpressNetCommand : Command
{
    private readonly Protocols.XpressNet.Commands.Command Command;
    public XpressNetCommand(Protocols.XpressNet.Commands.Command command)
    {
        Command = command;
    }
    internal override Frame ToFrame()
    {
        return new Frame(FrameHeader.Xbus, Command.GetBytes());
    }
}
