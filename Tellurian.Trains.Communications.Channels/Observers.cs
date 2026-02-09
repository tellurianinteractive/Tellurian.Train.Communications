namespace Tellurian.Trains.Communications.Channels;

public sealed class Observers<T> where T : class
{
    private readonly Lock _lock = new();
    private readonly List<IObserver<T>> _observers = [];

    public int Count { get { lock (_lock) { return _observers.Count; } } }

    public void Notify(T notification)
    {
        foreach (var observer in GetSnapshot())
        {
            observer.OnNext(notification);
        }
    }
    public void Notify(T[] notifications)
    {
        if (notifications is null) return;
        foreach (var notification in notifications) { Notify(notification); }
    }
    public void Completed()
    {
        foreach (var observer in GetSnapshot()) { observer.OnCompleted(); }
    }
    public void Error(Exception ex)
    {
        foreach (var observer in GetSnapshot()) { observer.OnError(ex); }
    }
    public IDisposable Subscribe(IObserver<T> observer)
    {
        lock (_lock)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }
        return new Unsubscriber(this, observer);
    }

    private IObserver<T>[] GetSnapshot()
    {
        lock (_lock) { return [.. _observers]; }
    }

    private void Remove(IObserver<T> observer)
    {
        lock (_lock) { _observers.Remove(observer); }
    }

    private sealed class Unsubscriber(Observers<T> parent, IObserver<T> observer) : IDisposable
    {
        public void Dispose() => parent.Remove(observer);
    }
}
