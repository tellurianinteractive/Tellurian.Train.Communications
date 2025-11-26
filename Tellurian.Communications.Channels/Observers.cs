namespace Tellurian.Communications.Channels;

public sealed class Observers<T> where T : class
{
    private readonly List<IObserver<T>> _Observers = [];

    public int Count => _Observers.Count;

    public void Notify(T notification)
    {
        foreach (var observer in _Observers)
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
        foreach (var observer in _Observers) { observer.OnCompleted(); }
    }
    public void Error(Exception ex)
    {
        foreach (var observer in _Observers) { observer.OnError(ex); }
    }
    public IDisposable Subscribe(IObserver<T> observer)
    {
        if (!_Observers.Contains(observer))
            _Observers.Add(observer);
        return new Unsubscriber<T>(_Observers, observer);
    }

    private class Unsubscriber<U>(List<IObserver<U>> observers, IObserver<U> observer) : IDisposable
    {
        private readonly List<IObserver<U>> _Observers = observers;
        private readonly IObserver<U> _Observer = observer;

        public void Dispose()
        {
            if (_Observer != null && _Observers.Contains(_Observer))
                _Observers.Remove(_Observer);
        }
    }
}
