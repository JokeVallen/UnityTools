using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoroutineRunner
{
    /// <summary>
    /// 全局协程运行器
    /// </summary>
    /// <remarks>
    /// <para>为全局协程调用提供切片入口或者代理，或者希望在不实现自定义 MonoBehaviour 的前提下使用协程，通过该类可以便捷和快速地启动任何位于主线程的协程代码。</para>
    /// </remarks>
    internal sealed class InternalCoroutineRunner : IGlobalCoroutineRunner
    {
        public static IGlobalCoroutineRunner Instance
        {
            get
            {
                if (Application.isEditor && !Application.isPlaying)
                    throw new InvalidOperationException($"The '{nameof(InternalCoroutineRunner)}' cannot support in editor mode.");
                return Handler.instance;
            }
        }

        private bool disposed;
        private static bool instantiated;
        private static int nextId = 0;
        private readonly Dictionary<int, CoroutineHandle> activeHandles = new Dictionary<int, CoroutineHandle>();
        private readonly Dictionary<string, CoroutineChannel> channels = new Dictionary<string, CoroutineChannel>();
        private readonly CoroutineChannel defaultChannel = new CoroutineChannel(0);

        static InternalCoroutineRunner()
        {
            Application.quitting += OnApplicationQuit;
        }

        private InternalCoroutineRunner()
        {
            var go = new GameObject(nameof(InternalCoroutineRunner));
            GameObject.DontDestroyOnLoad(go);
            go.AddComponent<Proxy>();
            go.hideFlags = HideFlags.HideAndDontSave;
            instantiated = true;
        }

        public Coroutine StartCoroutine(IEnumerator routine)
        {
            ThrowErrorIfDisposed();
            return Proxy.Instance.StartCoroutine(routine);
        }

        public Coroutine StartCoroutine(string methodName, object value)
        {
            ThrowErrorIfDisposed();
            return Proxy.Instance.StartCoroutine(methodName, value);
        }

        public void StopCoroutine(Coroutine coroutine)
        {
            ThrowErrorIfDisposed();
            Proxy.Instance.StopCoroutine(coroutine);
        }

        public void StopCoroutine(IEnumerator routine)
        {
            ThrowErrorIfDisposed();
            Proxy.Instance.StopCoroutine(routine);
        }

        public void StopCoroutine(string methodName)
        {
            ThrowErrorIfDisposed();
            Proxy.Instance.StopCoroutine(methodName);
        }

        public void StopAllCoroutines()
        {
            ThrowErrorIfDisposed();
            Proxy.Instance.StopAllCoroutines();
        }

        public void ConfigureChannel(string channelName, int maxConcurrent)
        {
            ThrowErrorIfDisposed();
            if (channels.ContainsKey(channelName))
                throw new InvalidOperationException($"Channel '{channelName}' is already configured.");
            channels[channelName] = new CoroutineChannel(maxConcurrent);
        }

        public CoroutineHandleToken Run(IEnumerator routine)
        {
            ThrowErrorIfDisposed();
            int id = ++nextId;
            var handle = defaultChannel.Enqueue(id, routine, this);
            if (handle == null) return CoroutineHandleToken.NullToken;
            activeHandles[id] = handle;

            handle.OnFinished += h => activeHandles.Remove(h.Id);
            return new CoroutineHandleToken(handle.Id, handle.Version);
        }

        public CoroutineHandleToken RunQueued(IEnumerator routine, string channelName)
        {
            ThrowErrorIfDisposed();
            if (!channels.TryGetValue(channelName, out var channel))
            {
                channel = new CoroutineChannel(1);
                channels[channelName] = channel;
            }

            int id = ++nextId;
            var handle = channel.Enqueue(id, routine, this);
            if (handle == null) return CoroutineHandleToken.NullToken;
            activeHandles[id] = handle;

            handle.OnFinished += h => activeHandles.Remove(h.Id);
            return new CoroutineHandleToken(handle.Id, handle.Version);
        }

        public void Dispose() { DisposeInternal(); }

        internal void Cancel(in CoroutineHandleToken token)
        {
            ThrowErrorIfDisposed();
            if (activeHandles.TryGetValue(token.Id, out var handle))
            {
                if (handle.Version == token.Version)
                {
                    handle.Cancel();
                }
            }
        }

        internal void Pause(in CoroutineHandleToken token)
        {
            ThrowErrorIfDisposed();
            if (activeHandles.TryGetValue(token.Id, out var handle) && handle.Version == token.Version)
                handle.Pause();
        }

        internal void Resume(in CoroutineHandleToken token)
        {
            ThrowErrorIfDisposed();
            if (activeHandles.TryGetValue(token.Id, out var handle) && handle.Version == token.Version)
                handle.Resume();
        }

        internal bool TryGetState(in CoroutineHandleToken token, out CoroutineState state)
        {
            ThrowErrorIfDisposed();
            state = CoroutineState.Completed;
            if (activeHandles.TryGetValue(token.Id, out var handle) && handle.Version == token.Version)
            {
                state = handle.State;
                return true;
            }
            return false;
        }

        internal bool TryGetHandle(int id, out ICoroutineHandle handle)
        {
            handle = null;
            if (activeHandles.TryGetValue(id, out var inner)) 
            {
                handle = inner;
                return true;
            }
            return false;
        }

        private void ThrowErrorIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(InternalCoroutineRunner));
        }

        private void DisposeInternal()
        {
            if (disposed) return;
            disposed = true;

            if (instantiated)
            {
                if (Application.isPlaying) GameObject.Destroy(((Proxy)Proxy.Instance).gameObject);
                else GameObject.DestroyImmediate(((Proxy)Proxy.Instance).gameObject);
            }

            channels.Clear();
            activeHandles.Clear();
        }

        private static void OnApplicationQuit()
        {
            Application.quitting -= OnApplicationQuit;
            if (!instantiated) return;
            Handler.instance.DisposeInternal();
        }

        private static class Handler
        {
            public static readonly InternalCoroutineRunner instance = new InternalCoroutineRunner();
        }

        [AddComponentMenu("")]
        [DisallowMultipleComponent]
        private class Proxy : MonoBehaviour
        {
            public static Proxy Instance => instance;
            private static Proxy instance;

            private void Awake()
            {
                if (ReferenceEquals(instance, null))
                {
                    instance = this;
                    DontDestroyOnLoad(gameObject);
                }
                else 
                {
                    Destroy(gameObject);
                }
            }

            private void OnDestroy()
            {
                if (ReferenceEquals(this, instance))
                    instance = null;
            }
        }

        private class CoroutineHandle : ICoroutineHandle
        {
            public CoroutineState State { get; private set; }
            public int Id { get; private set; }
            public long Version { get; private set; }
            public bool IsDone => State == CoroutineState.Completed || State == CoroutineState.Canceled;

            public event Action<CoroutineHandle> OnFinished;
            public event Action OnAwaiterComplete;
            private static readonly Queue<CoroutineHandle> pool = new Queue<CoroutineHandle>();
            private IEnumerator routine;

            public static CoroutineHandle Allocate(int id, IEnumerator routine)
            {
                CoroutineHandle handle;
                if (pool.Count > 0) handle = pool.Dequeue();
                else handle = new CoroutineHandle();

                handle.Id = id;
                handle.routine = routine;
                handle.State = CoroutineState.Running;
                return handle;
            }

            public IEnumerator Run()
            {
                while (State == CoroutineState.Running || State == CoroutineState.Paused)
                {
                    if (State == CoroutineState.Paused)
                    {
                        yield return null;
                        continue;
                    }

                    object previousCurrent = routine.Current;
                    var currentInstruction = previousCurrent as CustomYieldInstructionBase;
                    if (currentInstruction != null) currentInstruction.Handle = this;

                    bool hasNext;
                    try
                    {
                        hasNext = routine.MoveNext();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                        State = CoroutineState.Canceled;
                        break;
                    }

                    if (!hasNext)
                    {
                        if (currentInstruction != null) CustomYield.Release(currentInstruction);
                        break;
                    }

                    if (currentInstruction != null && !ReferenceEquals(routine.Current, previousCurrent))
                        CustomYield.Release(currentInstruction);

                    yield return routine.Current;
                }

                if (State == CoroutineState.Running)
                    State = CoroutineState.Completed;

                if (State == CoroutineState.Canceled && routine?.Current is CustomYieldInstructionBase canceledInstruction)
                    CustomYield.Release(canceledInstruction);

                OnFinished?.Invoke(this);
                OnAwaiterComplete?.Invoke();

                yield return null;

                Recycle(this);
            }

            public void Pause()
            {
                if (IsDone) return;
                State = CoroutineState.Paused;
            }

            public void Resume()
            {
                if (State != CoroutineState.Paused) return;
                State = CoroutineState.Running;
            }

            public void Cancel()
            {
                if (IsDone) return;
                State = CoroutineState.Canceled;
                OnAwaiterComplete?.Invoke();
                OnAwaiterComplete = null;
            }

            internal void Release()
            {
                Recycle(this);
            }

            private static void Recycle(CoroutineHandle handle)
            {
                handle.Clear();
                pool.Enqueue(handle);
            }

            private void Clear()
            {
                Id = 0;
                Version++;
                routine = null;
                OnFinished = null;
                OnAwaiterComplete = null;
            }
        }

        private class CoroutineChannel
        {
            private readonly Queue<CoroutineHandle> waitingQueue = new Queue<CoroutineHandle>();
            private readonly List<CoroutineHandle> runningHandles = new List<CoroutineHandle>();
            private readonly int maxConcurrent;
            private readonly int maxQueueCapacity;

            public CoroutineChannel(int maxConcurrent, int maxQueueCapacity = 2048) 
            {
                this.maxConcurrent = maxConcurrent;
                this.maxQueueCapacity = maxQueueCapacity;
            }

            public CoroutineHandle Enqueue(int id, IEnumerator routine, InternalCoroutineRunner runner)
            {
                if (waitingQueue.Count >= maxQueueCapacity)
                {
                    Debug.LogError($"The coroutine channel reached the limit of capacity ({maxQueueCapacity}), the new managed coroutine cannot enqueue！");
                    return null;
                }

                var handle = CoroutineHandle.Allocate(id, routine);
                waitingQueue.Enqueue(handle);

                handle.OnFinished += OnHandleFinished;

                CheckQueue(runner);
                return handle;
            }

            private void CheckQueue(InternalCoroutineRunner runner)
            {
                while (waitingQueue.Count > 0 && (maxConcurrent <= 0 || runningHandles.Count < maxConcurrent))
                {
                    var handle = waitingQueue.Dequeue();
                    if (handle.State == CoroutineState.Canceled)
                    {
                        handle.Release();
                        continue;
                    }

                    runningHandles.Add(handle);
                    runner.StartCoroutine(handle.Run());
                }
            }

            private void OnHandleFinished(CoroutineHandle h)
            {
                runningHandles.Remove(h);
                CheckQueue((InternalCoroutineRunner)Instance);
            }
        }
    }
}