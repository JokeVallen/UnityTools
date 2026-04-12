using EventHub.Unity;

public class InterruptibleEvent : IInterruptableEvent
{
    public bool Interrupted { get; private set; }
    public void Interrupt() => Interrupted = true;
}