namespace Tellurian.Trains.Adapters.Z21;

public class ActionObserver<T> : IObserver<T>
{
    private readonly Action<T> OnNextAction;
    private readonly Action<Exception> ErrorAction;
    private readonly Action CompleteAction;

    public ActionObserver(Action<T> onNextAction, Action<Exception> errorAction, Action completeAction)
    {
        OnNextAction = onNextAction;
        ErrorAction = errorAction;
        CompleteAction = completeAction;
    }

    public void OnCompleted()
    {
        CompleteAction?.Invoke();
    }

    public void OnError(Exception error)
    {
        ErrorAction?.Invoke(error);
    }

    public void OnNext(T value)
    {
        OnNextAction?.Invoke(value);
    }
}
