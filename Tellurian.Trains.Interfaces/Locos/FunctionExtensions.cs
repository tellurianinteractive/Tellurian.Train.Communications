namespace Tellurian.Trains.Interfaces.Locos;

public static class FunctionExtensions
{
    public static Function[] Map(this (int F, bool on)[] me)
    {
        return me.Select(m => Function.Set((Functions)m.F, m.on)).ToArray();
    }

}
