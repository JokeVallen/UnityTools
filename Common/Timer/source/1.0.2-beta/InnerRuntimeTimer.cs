using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace Timer
{
    internal sealed class InnerRuntimeTimer
    {
        public static InnerRuntimeTimer Instance
        {
            get
            {
                if (Application.isEditor && !Application.isPlaying)
                    throw new InvalidOperationException($"[GlobalTimer] The '{nameof(InnerRuntimeTimer)}' cannot support in editor mode.");
                return Handler.instance;
            }
        }

        private struct TimerJob
        {
            public bool isActive;
            public bool isPaused;
            public bool skipCurrentFrame;
            public TimeSource timeSource;
            public bool isLoop;
            public float timeRemaining;
            public float interval;
            public int generation;
            public int nextFreeIndex;
            public Optional<int> groupId;
            public Action callback;
        }

        private static bool instantiated;
        private readonly TimerJob[] slots;
        private readonly Action[] pendingCallbacks;
        private readonly Stopwatch stopwatch = Stopwatch.StartNew();
        
        private bool ticking;
        private int nextFreeSlotIndex;
        private int activeCount;
        private int highWaterMark;
        private int pendingCount;
        private double lastWallClockSeconds;
        private float wallClockDelta;
        private float manualDelta;
        private bool disposed;

        private InnerRuntimeTimer(int capacity = 2048)
        {
            slots = new TimerJob[capacity];
            pendingCallbacks = new Action[capacity];
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].generation = 1;
                slots[i].isActive = false;
                slots[i].nextFreeIndex = i + 1;
            }
            slots[slots.Length - 1].nextFreeIndex = -1;
            nextFreeSlotIndex = 0;
            lastWallClockSeconds = stopwatch.Elapsed.TotalSeconds;

            var go = new GameObject(nameof(InnerRuntimeTimer));
            GameObject.DontDestroyOnLoad(go);
            go.AddComponent<Proxy>().owner = this;
            go.hideFlags = HideFlags.HideAndDontSave;
            instantiated = true;
            Application.quitting += OnApplicationQuitting;
        }

        public TimerHandle Register(float interval, Action callback, TimeSource source, bool loop, Optional<int> groupId)
        {
            ThrowErrorIfDisposed();
            if (interval < 0f) throw new ArgumentOutOfRangeException($"[GlobalTimer] The parameter '{nameof(interval)}' cannot be less than zero.");
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            if (nextFreeSlotIndex == -1)
            {
                UnityEngine.Debug.LogWarning("[GlobalTimer] The number of concurrently active timer tasks has reached the limit.");
                return TimerHandle.Null;
            }

            int index = nextFreeSlotIndex;
            ref var slot = ref slots[index];

            nextFreeSlotIndex = slot.nextFreeIndex;
            activeCount++;
            if (index + 1 > highWaterMark) highWaterMark = index + 1;

            slot.timeSource = source;
            slot.interval = interval;
            slot.timeRemaining = interval;
            slot.callback = callback;
            slot.isLoop = loop;
            slot.isPaused = false;
            slot.isActive = true;
            slot.groupId = groupId;
            if(ticking) slot.skipCurrentFrame = true;

            return new TimerHandle(index, slot.generation);
        }

        public void Cancel(in TimerHandle handle)
        {
            ThrowErrorIfDisposed();
            if (ValidateHandle(handle, out int index))
                InternalReleaseSlot(index);
        }

        public void CancelAll()
        {
            ThrowErrorIfDisposed();

            for (int i = 0; i < pendingCount; i++)
                pendingCallbacks[i] = null;
            pendingCount = 0;

            int lastIndex = slots.Length - 1;
            for (int i = 0; i < lastIndex; i++)
            {
                ref var job = ref slots[i];
                if (job.isActive)
                {
                    job.callback = null;
                    job.isActive = false;
                    unchecked { job.generation++; }
                }
                job.nextFreeIndex = i + 1;
            }

            ref var lastJob = ref slots[lastIndex];
            if (lastJob.isActive)
            {
                lastJob.callback = null;
                lastJob.isActive = false;
                unchecked { lastJob.generation++; }
            }
            lastJob.nextFreeIndex = -1;

            nextFreeSlotIndex = 0;
            activeCount = 0;
            highWaterMark = 0;
        }

        public void SetPaused(in TimerHandle handle, bool paused)
        {
            ThrowErrorIfDisposed();
            if (ValidateHandle(handle, out int index))
                slots[index].isPaused = paused;
        }

        public bool IsActive(in TimerHandle handle)
        {
            ThrowErrorIfDisposed();
            return ValidateHandle(handle, out _);
        }

        public bool TryGetTimeRemaining(in TimerHandle handle, out float result)
        {
            ThrowErrorIfDisposed();
            result = default;
            if (!ValidateHandle(handle, out int index)) return false;
            result = slots[index].timeRemaining;
            return true;
        }

        public bool TryGetProgress(in TimerHandle handle, out float result)
        {
            ThrowErrorIfDisposed();
            result = default;
            if (!ValidateHandle(handle, out int index)) return false;
            ref var slot = ref slots[index];
            if (slot.interval <= 0f)
            {
                result = 1f;
                return true;
            }

            result = 1f - (slot.timeRemaining / slot.interval);
            return true;
        }

        public bool Reset(in TimerHandle handle)
        {
            ThrowErrorIfDisposed();
            if (!ValidateHandle(handle, out int index)) return false;
            slots[index].timeRemaining = slots[index].interval;
            return true;
        }

        public bool SetInterval(in TimerHandle handle, float interval)
        {
            ThrowErrorIfDisposed();
            if (!ValidateHandle(handle, out int index)) return false;
            if (interval < 0f) interval = 0f;
            ref var slot = ref slots[index];
            slot.interval = interval;
            if (slot.timeRemaining > interval) slot.timeRemaining = interval;
            return true;
        }

        public bool SetLoop(in TimerHandle handle, bool loop)
        {
            ThrowErrorIfDisposed();
            if (!ValidateHandle(handle, out int index)) return false;
            slots[index].isLoop = loop;
            return true;
        }

        public void CancelGroup(Optional<int> groupId)
        {
            ThrowErrorIfDisposed();
            if (!groupId.HasValue) return;
            int limit = highWaterMark;
            for (int i = 0; i < limit; i++)
            {
                ref var slot = ref slots[i];
                if (slot.groupId.HasValue && slot.isActive && slot.groupId == groupId.Value)
                    InternalReleaseSlot(i);
            }
        }

        public void SetGroupPaused(Optional<int> groupId, bool isPaused)
        {
            ThrowErrorIfDisposed();
            if (!groupId.HasValue) return;
            int limit = highWaterMark;
            for (int i = 0; i < limit; i++)
            {
                ref var slot = ref slots[i];
                if (slot.groupId.HasValue && slot.isActive && slot.groupId == groupId.Value)
                    slot.isPaused = isPaused;
            }
        }

        public bool TryGetGroupId(in TimerHandle handle, out int groupId)
        {
            ThrowErrorIfDisposed();
            groupId = 0;
            if (!ValidateHandle(handle, out int index)) return false;
            ref var slot = ref slots[index];
            groupId = slot.groupId.HasValue ? slot.groupId.Value : 0;
            return true;
        }

        public bool TryGetInterval(in TimerHandle handle, out float interval)
        {
            ThrowErrorIfDisposed();
            interval = 0f;
            if (!ValidateHandle(handle, out int index)) return false;
            interval = slots[index].interval;
            return true;
        }

        public bool TryGetIsLoop(in TimerHandle handle, out bool isLoop)
        {
            ThrowErrorIfDisposed();
            isLoop = false;
            if (!ValidateHandle(handle, out int index)) return false;
            isLoop = slots[index].isLoop;
            return true;
        }

        public bool TryGetFramesRemaining(in TimerHandle handle, out float framesRemaining)
        {
            ThrowErrorIfDisposed();
            framesRemaining = 0f;
            if (!ValidateHandle(handle, out int index)) return false;
            ref var job = ref slots[index];
            if (job.timeSource.Delta != TimeDelta.Frame) return false;
            framesRemaining = job.timeRemaining;
            return true;
        }

        public void ManualUpdate(float deltaTime)
        {
            ThrowErrorIfDisposed();
            if (deltaTime < 0f) deltaTime = 0f;
            manualDelta = deltaTime;
            TickTimerSlots(TimeSchedule.Manual);
        }

        public void Dispose()
        {
            DisposeInternal();
        }

        private void ThrowErrorIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(InnerRuntimeTimer));
        }

        private void DisposeInternal()
        {
            if (disposed) return;
            disposed = true;

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].isActive = false;
                slots[i].callback = null;
            }

            if (instantiated && Proxy.Instance != null)
            {
                if (Application.isPlaying) GameObject.Destroy(Proxy.Instance.gameObject);
                else GameObject.DestroyImmediate(Proxy.Instance.gameObject);
            }
        }

        private void OnApplicationQuitting()
        {
            DisposeInternal();
        }

        private void TickTimerSlots(TimeSchedule currentSchedule)
        {
            if (activeCount == 0) return;

            double currentWallSec = stopwatch.Elapsed.TotalSeconds;
            wallClockDelta = (float)(currentWallSec - lastWallClockSeconds);
            lastWallClockSeconds = currentWallSec;
            if (wallClockDelta < 0) wallClockDelta = 0;

            float scaledDelta = Time.deltaTime;
            float unscaledDelta = Time.unscaledDeltaTime;

            int limit = highWaterMark;
            ticking = limit > 0;
            for (int i = 0; i < limit; i++)
            {
                ref var job = ref slots[i];
                if (!job.isActive || job.isPaused) continue;
                if (job.skipCurrentFrame)
                {
                    job.skipCurrentFrame = false;
                    continue;
                }
                if (job.timeSource.Schedule != currentSchedule) continue;

                float delta = 0f;
                switch (job.timeSource.Delta)
                {
                    case TimeDelta.Scaled: 
                        delta = scaledDelta * job.timeSource.Scale; 
                        break;
                    case TimeDelta.Unscaled: 
                        delta = unscaledDelta * job.timeSource.Scale; 
                        break;
                    case TimeDelta.WallClock: 
                        delta = wallClockDelta * job.timeSource.Scale; 
                        break;
                    case TimeDelta.Frame: 
                        delta = 1f * job.timeSource.Scale; 
                        break;
                    case TimeDelta.Manual: 
                        delta = manualDelta * job.timeSource.Scale; 
                        break;
                }

                job.timeRemaining -= delta;

                if (job.timeRemaining <= 0f)
                {
                    if (pendingCount < pendingCallbacks.Length)
                        pendingCallbacks[pendingCount++] = job.callback;

                    if (job.isLoop)
                    {
                        if (job.timeSource.Delta == TimeDelta.Frame)
                            job.timeRemaining = job.interval;
                        else
                            job.timeRemaining = job.interval + job.timeRemaining;
                    }
                    else
                    {
                        InternalReleaseSlot(i);
                    }
                }
            }

            ticking = false;

            try
            {
                for (int i = 0; i < pendingCount; i++)
                {
                    try { if (pendingCallbacks[i] != null) pendingCallbacks[i].Invoke(); }
                    catch (Exception ex) { UnityEngine.Debug.LogError(ex); }
                    pendingCallbacks[i] = null;
                }
            }
            finally
            {
                pendingCount = 0;
            }
        }

        private void InternalReleaseSlot(int index)
        {
            ref var slot = ref slots[index];
            if (!slot.isActive) return;

            slot.isActive = false;
            slot.callback = null;
            unchecked { slot.generation++; }

            slot.nextFreeIndex = nextFreeSlotIndex;
            nextFreeSlotIndex = index;
            activeCount--;
            if (activeCount == 0) highWaterMark = 0;
        }

        private bool ValidateHandle(in TimerHandle handle, out int index)
        {
            index = handle.SlotIndex;
            if (index < 0 || index >= slots.Length) return false;
            return slots[index].isActive && slots[index].generation == handle.Generation;
        }

        private class Handler { public static readonly InnerRuntimeTimer instance = new InnerRuntimeTimer(); }

        [AddComponentMenu("")]
        [DisallowMultipleComponent]
        private class Proxy : MonoBehaviour
        {
            public static Proxy Instance => instance;
            private static Proxy instance;
            public InnerRuntimeTimer owner;

            private void Awake()
            {
                if (ReferenceEquals(instance, null)) instance = this;
                else Destroy(gameObject);
                if (ReferenceEquals(this, instance))
                {
                    DontDestroyOnLoad(gameObject);
                    StartCoroutine(PermanentCoroutineLoop());
                }
            }

            private void OnDestroy()
            {
                if (ReferenceEquals(this, instance))
                    instance = null;
            }

            private void Update()
            {
                if(owner != null)
                    owner.TickTimerSlots(TimeSchedule.Update);
            }

            private void FixedUpdate()
            {
                if (owner != null)
                    owner.TickTimerSlots(TimeSchedule.FixedUpdate);
            }

            private void LateUpdate()
            {
                if (owner != null)
                    owner.TickTimerSlots(TimeSchedule.LateUpdate);
            }

            private IEnumerator PermanentCoroutineLoop()
            {
                var waitForEndOfFrame = new WaitForEndOfFrame();
                var waitForFixedUpdate = new WaitForFixedUpdate();
                while (true)
                {
                    yield return null;
                    if (owner != null)
                        owner.TickTimerSlots(TimeSchedule.Coroutine);

                    yield return waitForFixedUpdate;
                    if (owner != null) 
                        owner.TickTimerSlots(TimeSchedule.WaitForFixedUpdate);

                    yield return waitForEndOfFrame;
                    if (owner != null)
                        owner.TickTimerSlots(TimeSchedule.EndOfFrame);
                }
            }
        }
    }
}