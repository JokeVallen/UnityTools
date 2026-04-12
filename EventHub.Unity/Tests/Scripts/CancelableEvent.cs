using EventHub.Unity;

public class CancelableEvent : ICancelableEvent
{
    public bool Cancelled { get; private set; }
    public void Cancel() => Cancelled = true;
}