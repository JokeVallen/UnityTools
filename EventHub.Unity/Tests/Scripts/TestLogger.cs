public class TestLogger : EventHub.Unity.ILogger
{
    public bool Enabled { get; set; } = true;
    public System.Action<System.Type, System.Delegate, System.Exception> OnLogError;
    public System.Action<System.Exception> OnLogError2;
    public System.Action<string> OnLogError3;
    public System.Action<string> OnLogWarning;
    public System.Action<string> OnLogInfo;

    public void LogError(System.Type eventType, System.Delegate handler, System.Exception exception)
        => OnLogError?.Invoke(eventType, handler, exception);
    public void LogWarning(string message) => OnLogWarning?.Invoke(message);
    public void LogInfo(string message) => OnLogInfo?.Invoke(message);

    public void LogError(System.Exception exception)
    {
        OnLogError2?.Invoke(exception);
    }

    public void LogError(string message)
    {
        OnLogError3?.Invoke(message);
    }
}