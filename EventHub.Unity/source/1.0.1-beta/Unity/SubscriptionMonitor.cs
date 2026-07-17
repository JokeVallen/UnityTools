#if !EVENTHUB_EXTENSION_ENABLE && !EVENTHUB_UNITY_EXTENSION_ENABLE

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal sealed class SubscriptionMonitor : ScriptableObject, ISubscriptionMonitor
    {
        private class Item : IDisposable
        {
            public WeakReference<Component> Component { get; }
            public List<WeakReference<ISubscription>> Subscriptions { get; }
            public bool IsValid => !disposed && Component != null && Subscriptions != null;
            private bool disposed;

            public Item(Component component) 
            {
                Component = new WeakReference<Component>(component);
                Subscriptions = new List<WeakReference<ISubscription>>();
            }

            public bool DisposeIfMissingReference() 
            {
                if (!IsValid) return false;

                if (!Component.TryGetTarget(out var com) || com == null)
                {
                    Dispose();
                    return true;
                }
                else
                {
                    Subscriptions.RemoveAll(wt => !wt.TryGetTarget(out _));
                }

                return false;
            }

            public void Dispose()
            {
                if (!IsValid) return;

                int count = Subscriptions.Count;
                for (int i = 0; i < count; i++)
                {
                    var wk = Subscriptions[i];
                    if (wk.TryGetTarget(out var subscription))
                    {
                        subscription.Dispose();
                    }
                }

                Subscriptions.Clear();
                disposed = true;
            }
        }

        public static SubscriptionMonitor Instance
        {
            get 
            {
                ThrowErrorIfDisposed();
                return instance;
            }
        }

        private static SubscriptionMonitor instance;
        private static bool disposed;
        private readonly List<Item> items = new List<Item>();
        private CancellationTokenSource cts;
        private CancellationTokenSource linkedCts;
        private bool timerRunning;
        private CancellationTokenRegistration registration;

        public void StartTimer(CancellationToken cancellationToken = default) 
        {
            ThrowErrorIfDisposed();
            StartTimerInternal(cancellationToken);
        }

        public void StopTimer() 
        {
            ThrowErrorIfDisposed();
            StopTimerInternal();
        }

        public void Register(Component component, ISubscription subscription) 
        {
            ThrowErrorIfDisposed();
            RegisterInternal(component, subscription);
        }

        public void Register(Component component, ISubscription subscription1, ISubscription subscription2)
        {
            ThrowErrorIfDisposed();
            RegisterInternal(component, subscription1, subscription2);
        }

        public void Register(Component component, params ISubscription[] subscriptions) 
        {
            ThrowErrorIfDisposed();
            RegisterInternal(component, subscriptions);
        }

        public void UnsubscribeAll(Component component)
        {
            ThrowErrorIfDisposed();
            UnsubscribeAllInternal(component);
        }

        public void Dispose()
        {
            DisposeInternal();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            try
            {
                if (instance == null) 
                { 
                    instance = CreateInstance<SubscriptionMonitor>();
                    instance.hideFlags = HideFlags.HideInInspector | HideFlags.NotEditable | HideFlags.DontSave;
                }

                if (HasConfig(true) && SubscriptionMonitorConfig.Instance.StartTimerOnInitialize)
                    instance.StartTimerInternal();

                Application.quitting -= OnQuit;
                Application.quitting += OnQuit;
            }
            catch (Exception ex)
            {
                EventDispatcherUtility.CatchError($"The method '{nameof(Initialize)}' triggered an exception: {ex.Message}");
                return;
            }
        }

        private void OnDestroy()
        {
            if (!disposed)
            {
                DisposeInternal();
            }
        }

        private void StartTimerInternal(CancellationToken cancellationToken = default) 
        {
            if (timerRunning || !HasConfig(true)) return;

            cts = new CancellationTokenSource();
            CancellationToken linkedToken;
            if (cancellationToken == default)
            {
                linkedToken = cts.Token;
            }
            else 
            {
                linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
                linkedToken = linkedCts.Token;
            }
                
            registration = linkedToken.Register(OnCancel);
            Timing(linkedToken).Forget();
            timerRunning = true;
        }

        private void StopTimerInternal() 
        {
            if (!timerRunning) return;
            cts?.Cancel(false);
        }

        private void RegisterInternal(Component component, ISubscription subscription) 
        {
            if (component == null || subscription == null)
                return;

            var item = GetOrCreateItem(component);
            item.Subscriptions.Add(new WeakReference<ISubscription>(subscription));
        }

        private void RegisterInternal(Component component, ISubscription subscription1, ISubscription subscription2)
        {
            if (component == null || (subscription1 == null && subscription2 == null))
                return;

            var item = GetOrCreateItem(component);
            var wk1 = subscription1 == null ? null : new WeakReference<ISubscription>(subscription1);
            var wk2 = subscription2 == null ? null : new WeakReference<ISubscription>(subscription2);
            if (wk1 != null) item.Subscriptions.Add(wk1);
            if (wk2 != null) item.Subscriptions.Add(wk2);
        }

        private void RegisterInternal(Component component, params ISubscription[] subscriptions) 
        {
            if (component == null || subscriptions == null || subscriptions.Length == 0)
                return;

            var item = GetOrCreateItem(component);
            item.Subscriptions.AddRange(subscriptions.Where(sub => sub != null).Select(sub => new WeakReference<ISubscription>(sub)));
        }

        private void UnsubscribeAllInternal(Component component)
        {
            if (component == null) return;

            var item = items.Find(it => it.Component.TryGetTarget(out var com) && com == component);
            if (item == null) return;

            item.Subscriptions.ForEach(wk => { if (wk.TryGetTarget(out var sub)) sub.Dispose(); });
            item.Subscriptions.Clear();
        }

        private void OnCancel() 
        {
            registration.Dispose();
            linkedCts?.Dispose();
            cts?.Dispose();
            cts = null;
            linkedCts = null;
            timerRunning = false;
        }

        private async UniTask Timing(CancellationToken cancellationToken) 
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await UniTask.Delay(SubscriptionMonitorConfig.Instance.MilliSecondsDelay, cancellationToken: cancellationToken);
                    await UniTask.SwitchToMainThread();
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) 
                {
                    EventDispatcherLog.LogInfo($"The async method '{nameof(Timing)}' has been cancelled.");
                    return;
                }

                instance?.CleanInternal();
            }
        }

        private static void OnQuit()
        {
            Application.quitting -= OnQuit;
            instance?.DisposeInternal();
        }

        private void CleanInternal() 
        {
            int count = items.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var item = items[i];
                if (!item.IsValid)
                {
                    items.RemoveAt(i);
                    continue;
                }

                if (item.DisposeIfMissingReference())
                    items.RemoveAt(i);
            }
        }

        private void DisposeInternal() 
        {
            if (disposed) return;
            disposed = true;

            cts?.Cancel(false);
            items.ForEach(item => item.Dispose());
            items.Clear();

            if (Application.isPlaying)
                Destroy(this);
            else
                DestroyImmediate(this);
        }

        private static bool HasConfig(bool catchError = false) 
        {
            if (SubscriptionMonitorConfig.Instance == null)
            {
                if(catchError)
                    EventDispatcherUtility.CatchError($"The '{nameof(SubscriptionMonitorConfig.Instance)}' typed {nameof(SubscriptionMonitorConfig)} cannot be null: { new ArgumentNullException(nameof(SubscriptionMonitorConfig.Instance)) }");
                return false;
            }

            return true;
        }

        private Item GetOrCreateItem(Component component)
        {
            var item = items.Find(it => it.Component.TryGetTarget(out var em) && em == component);
            if (item == null)
            {
                item = new Item(component);
                items.Add(item);
            }
            return item;
        }

        private static void ThrowErrorIfDisposed() 
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(SubscriptionMonitor));
        }

#if EVENTHUB_TESTS
        /// <summary>
        /// 测试用：强制立即执行清理，无需等待定时器
        /// </summary>
        internal void ForceCleanup() => CleanInternal();

        /// <summary>
        /// 测试用：获取当前定时器运行状态
        /// </summary>
        internal bool IsTimerRunning => timerRunning;

        /// <summary>
        /// 测试用：获取内部 Item 数量
        /// </summary>
        internal int GetItemCount() => items.Count;
#endif
    }
}

#endif