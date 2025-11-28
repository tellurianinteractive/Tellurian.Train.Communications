namespace Tellurian.Trains.Communications.Channels;

public sealed class ActionObserver<T> : IObserver<T>
{
    private readonly Action<T> _OnNextAction;
    private readonly Action<Exception> _ErrorAction;
    private readonly Action _CompleteAction;

    public ActionObserver(Action<T> onNextAction, Action<Exception> errorAction, Action completeAction)
    {
        _OnNextAction = onNextAction;
        _ErrorAction = errorAction;
        _CompleteAction = completeAction;
    }

    public void OnCompleted()
    {
        _CompleteAction?.Invoke();
    }

    public void OnError(Exception error)
    {
        _ErrorAction?.Invoke(error);
    }

    public void OnNext(T value)
    {
        _OnNextAction?.Invoke(value);
    }
}
