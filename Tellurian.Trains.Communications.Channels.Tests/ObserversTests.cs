using System.Net;
using System.Net.Sockets;

namespace Tellurian.Trains.Communications.Channels.Tests;

[TestClass]
public class ObserversTests
{
    [TestMethod]
    public void NotifyCallsOnNextForAllObservers()
    {
        var observers = new Observers<CommunicationResult>();
        var observer1 = new TestObserver();
        var observer2 = new TestObserver();
        var observer3 = new TestObserver();

        observers.Subscribe(observer1);
        observers.Subscribe(observer2);
        observers.Subscribe(observer3);

        var data = CommunicationResult.Success(new byte[] { 1, 2, 3 }, "test", "UDP");
        observers.Notify(data);

        Assert.HasCount(1, observer1.ReceivedNotifications);
        Assert.HasCount(1, observer2.ReceivedNotifications);
        Assert.HasCount(1, observer3.ReceivedNotifications);
        Assert.AreSame(data, observer1.ReceivedNotifications[0]);
    }

    [TestMethod]
    public void NotifyArrayCallsOnNextForEachItem()
    {
        var observers = new Observers<CommunicationResult>();
        var observer = new TestObserver();
        observers.Subscribe(observer);

        var data1 = CommunicationResult.Success(new byte[] { 1 }, "test", "UDP");
        var data2 = CommunicationResult.Success(new byte[] { 2 }, "test", "UDP");
        var data3 = CommunicationResult.Success(new byte[] { 3 }, "test", "UDP");

        observers.Notify(new[] { data1, data2, data3 });

        Assert.HasCount(3, observer.ReceivedNotifications);
        Assert.AreSame(data1, observer.ReceivedNotifications[0]);
        Assert.AreSame(data2, observer.ReceivedNotifications[1]);
        Assert.AreSame(data3, observer.ReceivedNotifications[2]);
    }

    [TestMethod]
    public void NotifyWithNullArrayDoesNothing()
    {
        var observers = new Observers<CommunicationResult>();
        var observer = new TestObserver();
        observers.Subscribe(observer);

        observers.Notify((CommunicationResult[])null!);

        Assert.HasCount(0, observer.ReceivedNotifications);
    }

    [TestMethod]
    public void UnsubscribeStopsNotifications()
    {
        var observers = new Observers<CommunicationResult>();
        var observer = new TestObserver();

        var subscription = observers.Subscribe(observer);
        var data1 = CommunicationResult.Success(new byte[] { 1 }, "test", "UDP");
        observers.Notify(data1);

        Assert.HasCount(1, observer.ReceivedNotifications);

        // Unsubscribe
        subscription.Dispose();

        var data2 = CommunicationResult.Success(new byte[] { 2 }, "test", "UDP");
        observers.Notify(data2);

        // Should still be 1 (no new notification)
        Assert.HasCount(1, observer.ReceivedNotifications);
    }

    [TestMethod]
    public void SubscribingSameObserverTwiceOnlyAddsOnce()
    {
        var observers = new Observers<CommunicationResult>();
        var observer = new TestObserver();

        observers.Subscribe(observer);
        observers.Subscribe(observer); // Subscribe again

        var data = CommunicationResult.Success(new byte[] { 1 }, "test", "UDP");
        observers.Notify(data);

        // Should only receive notification once
        Assert.HasCount(1, observer.ReceivedNotifications);
    }

    [TestMethod]
    public void ErrorPropagatesToAllObservers()
    {
        var observers = new Observers<CommunicationResult>();
        var observer1 = new TestObserver();
        var observer2 = new TestObserver();
        var observer3 = new TestObserver();

        observers.Subscribe(observer1);
        observers.Subscribe(observer2);
        observers.Subscribe(observer3);

        var exception = new InvalidOperationException("Test error");
        observers.Error(exception);

        Assert.IsNotNull(observer1.LastError);
        Assert.IsNotNull(observer2.LastError);
        Assert.IsNotNull(observer3.LastError);
        Assert.AreSame(exception, observer1.LastError);
        Assert.AreSame(exception, observer2.LastError);
        Assert.AreSame(exception, observer3.LastError);
    }

    [TestMethod]
    public void CompletedPropagatesToAllObservers()
    {
        var observers = new Observers<CommunicationResult>();
        var observer1 = new TestObserver();
        var observer2 = new TestObserver();
        var observer3 = new TestObserver();

        observers.Subscribe(observer1);
        observers.Subscribe(observer2);
        observers.Subscribe(observer3);

        observers.Completed();

        Assert.IsTrue(observer1.IsCompleted);
        Assert.IsTrue(observer2.IsCompleted);
        Assert.IsTrue(observer3.IsCompleted);
    }

    [TestMethod]
    public void UnsubscribeDoesNotAffectOtherObservers()
    {
        var observers = new Observers<CommunicationResult>();
        var observer1 = new TestObserver();
        var observer2 = new TestObserver();
        var observer3 = new TestObserver();

        var sub1 = observers.Subscribe(observer1);
        var sub2 = observers.Subscribe(observer2);
        var sub3 = observers.Subscribe(observer3);

        // Unsubscribe observer2
        sub2.Dispose();

        var data = CommunicationResult.Success(new byte[] { 1 }, "test", "UDP");
        observers.Notify(data);

        Assert.HasCount(1, observer1.ReceivedNotifications);
        Assert.HasCount(0, observer2.ReceivedNotifications); // Unsubscribed
        Assert.HasCount(1, observer3.ReceivedNotifications);
    }

    [TestMethod]
    public void UnsubscribeTwiceIsIdempotent()
    {
        var observers = new Observers<CommunicationResult>();
        var observer = new TestObserver();

        var subscription = observers.Subscribe(observer);
        subscription.Dispose();
        subscription.Dispose(); // Dispose again

        // Should not throw
        var data = CommunicationResult.Success(new byte[] { 1 }, "test", "UDP");
        observers.Notify(data);

        Assert.HasCount(0, observer.ReceivedNotifications);
    }

    [TestMethod]
    public void CountReflectsNumberOfObservers()
    {
        var observers = new Observers<CommunicationResult>();
        Assert.AreEqual(0, observers.Count);

        var sub1 = observers.Subscribe(new TestObserver());
        Assert.AreEqual(1, observers.Count);

        var sub2 = observers.Subscribe(new TestObserver());
        Assert.AreEqual(2, observers.Count);

        var sub3 = observers.Subscribe(new TestObserver());
        Assert.AreEqual(3, observers.Count);

        sub2.Dispose();
        Assert.AreEqual(2, observers.Count);

        sub1.Dispose();
        Assert.AreEqual(1, observers.Count);

        sub3.Dispose();
        Assert.AreEqual(0, observers.Count);
    }
}

internal class TestObserver : IObserver<CommunicationResult>
{
    public readonly List<CommunicationResult> ReceivedNotifications = new List<CommunicationResult>();
    public Exception? LastError { get; private set; }
    public bool IsCompleted { get; private set; }

    public void OnCompleted()
    {
        IsCompleted = true;
    }

    public void OnError(Exception error)
    {
        LastError = error;
    }

    public void OnNext(CommunicationResult value)
    {
        ReceivedNotifications.Add(value);
    }
}
