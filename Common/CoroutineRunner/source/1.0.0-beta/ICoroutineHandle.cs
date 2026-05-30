using System;

namespace CoroutineRunner
{
    internal interface ICoroutineHandle
    {
        int Id { get; }
        long Version { get; }
        CoroutineState State { get; }
        event Action OnAwaiterComplete;
        bool IsDone { get; }
        void Pause();
        void Resume();
        void Cancel();
    }
}