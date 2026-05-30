// 回调执行阶段与状态推进阶段分离：
// - 回调中 Cancel 当前 handle：安全，本帧不再触发其他副作用
// - 回调中 Register 新 Timer：安全，新 Timer 最早下帧触发
// - 回调中 Cancel 其他 handle：安全
// - 回调中 Reset 当前 handle：安全，但会丢失本次的累计误差补偿
// - 回调中 CancelGroup：安全，影响下帧遍历范围

using System;
using System.Collections;
using UnityEngine;

namespace Timer
{
    internal sealed class InnerTimer
    {
        public static InnerTimer Instance
        {
            get
            {
                if (Application.isEditor && !Application.isPlaying)
                    throw new InvalidOperationException($"The '{nameof(InnerTimer)}' cannot support in editor mode.");
                return Handler.instance;
            }
        }

        private struct TimerJob
        {
            public bool IsActive;
            public bool IsPaused;
            public bool SkipCurrentFrame;
            public TimeSource TimeSource;
            public bool IsLoop;
            public float TimeRemaining;
            public float Interval;
            public int Generation;
            public int NextFreeIndex;
            public int GroupId;
            public bool HasGroup;
            public Action Callback;
        }

        private static bool instantiated;
        private readonly TimerJob[] slots;
        private readonly Action[] pendingCallbacks;
        private int nextFreeSlotIndex;
        private bool disposed;
        private bool ticking;
        private int activeCount;
        private int highWaterMark;
        private int pendingCount;

        public void CancelAll()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                ref var job = ref slots[i];
                if (job.IsActive)
                {
                    job.Callback = null;
                    job.IsActive = false;
                    unchecked { job.Generation++; }
                }
                job.NextFreeIndex = i + 1;
            }
            if (slots.Length > 0)
                slots[slots.Length - 1].NextFreeIndex = -1;
            nextFreeSlotIndex = 0;
            activeCount = 0;
            highWaterMark = 0;
            pendingCount = 0;
        }

        private InnerTimer(int capacity = 2048)
        {
            slots = new TimerJob[capacity];
            pendingCallbacks = new Action[capacity];
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].Generation = 1;
                slots[i].IsActive = false;
                slots[i].NextFreeIndex = i + 1;
            }
            slots[slots.Length - 1].NextFreeIndex = -1;
            nextFreeSlotIndex = 0;

            var go = new GameObject(nameof(InnerTimer));
            GameObject.DontDestroyOnLoad(go);
            go.AddComponent<Proxy>().owner = this;
            go.hideFlags = HideFlags.HideAndDontSave;
            instantiated = true;
            Application.quitting += OnApplicationQuitting;
        }

        public TimerHandle Register(float interval, Action callback, TimeSource source, bool loop, int groupId, bool hasGroup)
        {
            ThrowErrorIfDisposed();
            if (interval < 0f) interval = 0f;
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            if (nextFreeSlotIndex == -1)
            {
                Debug.LogWarning("The slots reached the limit of capacity.");
                return TimerHandle.Null;
            }

            int index = nextFreeSlotIndex;
            ref var slot = ref slots[index];

            nextFreeSlotIndex = slot.NextFreeIndex;
            activeCount++;
            if (index + 1 > highWaterMark) highWaterMark = index + 1;

            slot.TimeSource = source;
            slot.Interval = interval;
            slot.TimeRemaining = interval;
            slot.Callback = callback;
            slot.IsLoop = loop;
            slot.IsPaused = false;
            slot.IsActive = true;
            slot.GroupId = groupId;
            slot.HasGroup = hasGroup;
            if (ticking) slot.SkipCurrentFrame = true;

            return new TimerHandle(index, slot.Generation);
        }

        public void Cancel(in TimerHandle handle)
        {
            ThrowErrorIfDisposed();
            if (ValidateHandle(handle, out int index))
                InternalReleaseSlot(index);
        }

        public void SetPaused(in TimerHandle handle, bool paused)
        {
            ThrowErrorIfDisposed();
            if (ValidateHandle(handle, out int index))
                slots[index].IsPaused = paused;
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
            result = slots[index].TimeRemaining;
            return true;
        }

        public bool TryGetProgress(in TimerHandle handle, out float result)
        {
            ThrowErrorIfDisposed();
            result = default;
            if (!ValidateHandle(handle, out int index)) return false;
            ref var slot = ref slots[index];
            if (slot.Interval <= 0f)
            {
                result = 1f;
                return true;
            }

            result = 1f - (slot.TimeRemaining / slot.Interval);
            return true;
        }

        public bool Reset(in TimerHandle handle)
        {
            ThrowErrorIfDisposed();
            if (!ValidateHandle(handle, out int index)) return false;
            slots[index].TimeRemaining = slots[index].Interval;
            return true;
        }

        public bool SetInterval(in TimerHandle handle, float interval)
        {
            ThrowErrorIfDisposed();
            if (!ValidateHandle(handle, out int index)) return false;
            if (interval < 0f) interval = 0f;
            ref var slot = ref slots[index];
            slot.Interval = interval;
            if (slot.TimeRemaining > interval) slot.TimeRemaining = interval;
            return true;
        }

        public void CancelGroup(int groupId)
        {
            if (groupId == 0) return;
            int limit = highWaterMark;
            for (int i = 0; i < limit; i++)
            {
                ref var slot = ref slots[i];
                if (slot.HasGroup && slot.IsActive && slot.GroupId == groupId)
                    InternalReleaseSlot(i);
            }
        }

        public void SetGroupPaused(int groupId, bool isPaused)
        {
            if (groupId == 0) return;
            int limit = highWaterMark;
            for (int i = 0; i < limit; i++)
            {
                ref var slot = ref slots[i];
                if (slot.HasGroup && slot.IsActive && slot.GroupId == groupId)
                    slot.IsPaused = isPaused;
            }
        }

        public bool TryGetGroupId(in TimerHandle handle, out int groupId)
        {
            groupId = 0;
            if (!ValidateHandle(handle, out int index)) return false;
            groupId = slots[index].GroupId;
            return true;
        }

        public bool TryGetInterval(in TimerHandle handle, out float interval)
        {
            interval = 0f;
            if (!ValidateHandle(handle, out int index)) return false;
            interval = slots[index].Interval;
            return true;
        }

        public bool TryGetIsLoop(in TimerHandle handle, out bool isLoop)
        {
            isLoop = false;
            if (!ValidateHandle(handle, out int index)) return false;
            isLoop = slots[index].IsLoop;
            return true;
        }

        public bool SetLoop(in TimerHandle handle, bool loop)
        {
            if (!ValidateHandle(handle, out int index)) return false;
            slots[index].IsLoop = loop;
            return true;
        }

        public bool TryGetFramesRemaining(in TimerHandle handle, out float framesRemaining)
        {
            framesRemaining = 0f;
            if (!ValidateHandle(handle, out int index)) return false;
            ref var job = ref slots[index];
            bool isFrameDriven = job.TimeSource == TimeSource.MonoUpdate ||
                                 job.TimeSource == TimeSource.MonoLateUpdate ||
                                 job.TimeSource == TimeSource.MonoFixedUpdate ||
                                 job.TimeSource == TimeSource.CoroutineUpdate ||
                                 job.TimeSource == TimeSource.CoroutineEndOfFrame;
            if (!isFrameDriven) return false;
            framesRemaining = job.TimeRemaining;
            return true;
        }

        public void Dispose()
        {
            DisposeInternal();
        }

        private void ThrowErrorIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(InnerTimer));
        }

        private void DisposeInternal()
        {
            if (disposed) return;
            disposed = true;

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].IsActive = false;
                slots[i].Callback = null;
            }

            if (instantiated && Proxy.Instance != null)
            {
                if (Application.isPlaying) GameObject.Destroy(Proxy.Instance.gameObject);
                else GameObject.DestroyImmediate(Proxy.Instance.gameObject);
            }
        }

        private void OnApplicationQuitting()
        {
            Application.quitting -= OnApplicationQuitting;
            DisposeInternal();
        }

        private void TickTimerSlots(float scaledDeltaTime, float unscaledDeltaTime, TimeSource currentPhase)
        {
            if (activeCount == 0) return;

            int limit = highWaterMark;
            ticking = limit > 0;
            for (int i = 0; i < limit; i++)
            {
                ref var job = ref slots[i];

                if (!job.IsActive || job.IsPaused) continue;
                if (job.SkipCurrentFrame)
                {
                    job.SkipCurrentFrame = false;
                    continue;
                }

                if (currentPhase == TimeSource.MonoUpdate)
                {
                    if (job.TimeSource == TimeSource.MonoLateUpdate ||
                        job.TimeSource == TimeSource.MonoFixedUpdate ||
                        job.TimeSource == TimeSource.CoroutineUpdate ||
                        job.TimeSource == TimeSource.CoroutineEndOfFrame) continue;
                }
                else
                {
                    if (job.TimeSource != currentPhase)
                        continue;
                }

                switch (job.TimeSource)
                {
                    case TimeSource.ScaledTime:
                    case TimeSource.MonoFixedUpdate:
                        job.TimeRemaining -= scaledDeltaTime;
                        break;
                    case TimeSource.UnscaledTime: job.TimeRemaining -= unscaledDeltaTime; break;
                    default: job.TimeRemaining -= 1f; break;
                }

                if (job.TimeRemaining <= 0f)
                {
                    pendingCallbacks[pendingCount++] = job.Callback;

                    if (job.IsLoop)
                    {
                        if (job.TimeSource == TimeSource.ScaledTime || job.TimeSource == TimeSource.UnscaledTime)
                            job.TimeRemaining = job.Interval + job.TimeRemaining;
                        else
                            job.TimeRemaining = job.Interval;
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
                    try { pendingCallbacks[i]?.Invoke(); }
                    catch (Exception ex) { Debug.LogError(ex); }
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
            if (!slot.IsActive) return;

            slot.IsActive = false;
            slot.Callback = null;
            unchecked { slot.Generation++; }

            slot.NextFreeIndex = nextFreeSlotIndex;
            nextFreeSlotIndex = index;
            activeCount--;
            if (activeCount == 0) highWaterMark = 0;
        }

        private bool ValidateHandle(in TimerHandle handle, out int index)
        {
            index = handle.SlotIndex;
            if (index < 0 || index >= slots.Length) return false;
            return slots[index].IsActive && slots[index].Generation == handle.Generation;
        }

        private class Handler { public static readonly InnerTimer instance = new InnerTimer(); }

        [AddComponentMenu("")]
        [DisallowMultipleComponent]
        private class Proxy : MonoBehaviour
        {
            public static Proxy Instance => instance;
            private static Proxy instance;
            public InnerTimer owner;

            private void Awake()
            {
                if (ReferenceEquals(instance, null)) instance = this;
                else Destroy(gameObject);
                if (ReferenceEquals(this, Instance))
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
                owner?.TickTimerSlots(Time.deltaTime, Time.unscaledDeltaTime, TimeSource.MonoUpdate);
            }

            private void FixedUpdate()
            {
                owner?.TickTimerSlots(Time.fixedDeltaTime, Time.fixedUnscaledDeltaTime, TimeSource.MonoFixedUpdate);
            }

            private void LateUpdate()
            {
                owner?.TickTimerSlots(0f, 0f, TimeSource.MonoLateUpdate);
            }

            private IEnumerator PermanentCoroutineLoop()
            {
                var waitForEndOfFrame = new WaitForEndOfFrame();
                while (true)
                {
                    yield return null;
                    owner?.TickTimerSlots(0f, 0f, TimeSource.CoroutineUpdate);
                    yield return waitForEndOfFrame;
                    owner?.TickTimerSlots(0f, 0f, TimeSource.CoroutineEndOfFrame);
                }
            }
        }
    }
}