using System;
using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    internal static class SnapshotCacheInternal
    {
        private interface ITypedCache
        {
            void Remove(Guid key);
            void Clear();
        }

        private class TypedCache<TSnapshot> : ITypedCache
        {
            public readonly Dictionary<Guid, TSnapshot> storage = new Dictionary<Guid, TSnapshot>();
            public void Remove(Guid key) => storage.Remove(key);
            public void Clear() => storage.Clear();
        }

        private static readonly Dictionary<Type, ITypedCache> caches = new Dictionary<Type, ITypedCache>();

        public static void Store<TSnapshot>(Guid key, in TSnapshot snapshot)
        {
            var type = typeof(TSnapshot);
            if (!caches.TryGetValue(type, out var cache))
            {
                cache = new TypedCache<TSnapshot>();
                caches[type] = cache;
            }
            ((TypedCache<TSnapshot>)cache).storage[key] = snapshot;
        }

        public static bool TryGet<TSnapshot>(Guid key, out TSnapshot snapshot)
        {
            var type = typeof(TSnapshot);
            if (caches.TryGetValue(type, out var cache))
                return ((TypedCache<TSnapshot>)cache).storage.TryGetValue(key, out snapshot);
            snapshot = default;
            return false;
        }

        public static bool Exists<TSnapshot>(Guid key)
        {
            var type = typeof(TSnapshot);
            if (!caches.TryGetValue(type, out var cache))
                return false;
            return ((TypedCache<TSnapshot>)cache).storage.ContainsKey(key);
        }

        public static void Remove<TSnapshot>(Guid key)
        {
            var type = typeof(TSnapshot);
            if (!caches.TryGetValue(type, out var cache)) return;
            cache.Remove(key);
        }

        public static void RemoveAll(Guid key)
        {
            foreach (var cache in caches.Values)
            {
                cache.Remove(key);
            }
        }

        public static IEnumerable<(Guid Key, TSnapshot Snapshot)> GetAll<TSnapshot>()
        {
            var type = typeof(TSnapshot);
            if (caches.TryGetValue(type, out var cache))
            {
                var typedCache = (TypedCache<TSnapshot>)cache;
                foreach (var kvp in typedCache.storage)
                {
                    yield return (kvp.Key, kvp.Value);
                }
            }
        }

        public static void Clear()
        {
            foreach (var cache in caches.Values)
                cache.Clear();
            caches.Clear();
        }
    }

    internal static class SnapshotCacheInternal<TTag>
    {
        private interface ITypedCache
        {
            void Remove(Guid key, TTag tag);
            void RemoveAll(Guid key);
            void Clear();
        }

        private class TypedCache<TSnapshot> : ITypedCache
        {
            public readonly Dictionary<(Guid, TTag), TSnapshot> storage = new Dictionary<(Guid, TTag), TSnapshot>();
            private readonly List<(Guid, TTag)> temp = new List<(Guid, TTag)>();
            public void Clear() => storage.Clear();
            public void Remove(Guid key, TTag tag) => storage.Remove((key, tag));

            public void RemoveAll(Guid key)
            {
                foreach (var k in storage.Keys)
                {
                    if (k.Item1 == key)
                        temp.Add(k);
                }
                foreach (var k in temp)
                    storage.Remove(k);
                temp.Clear();
            }
        }

        private static readonly Dictionary<Type, ITypedCache> caches = new Dictionary<Type, ITypedCache>();

        public static void Store<TSnapshot>(Guid key, in TSnapshot snapshot, TTag tag)
        {
            var type = typeof(TSnapshot);
            if (!caches.TryGetValue(type, out var cache))
            {
                cache = new TypedCache<TSnapshot>();
                caches[type] = cache;
            }
            ((TypedCache<TSnapshot>)cache).storage[(key, tag)] = snapshot;
        }

        public static bool TryGet<TSnapshot>(Guid key, out TSnapshot snapshot, TTag tag)
        {
            var type = typeof(TSnapshot);
            if (caches.TryGetValue(type, out var cache))
            {
                var typedCache = (TypedCache<TSnapshot>)cache;
                return typedCache.storage.TryGetValue((key, tag), out snapshot);
            }
            snapshot = default;
            return false;
        }

        public static bool Exists<TSnapshot>(Guid key, TTag tag)
        {
            var type = typeof(TSnapshot);
            if (!caches.TryGetValue(type, out var cache))
                return false;
            var typedCache = (TypedCache<TSnapshot>)cache;
            return typedCache.storage.ContainsKey((key, tag));
        }

        public static void Remove<TSnapshot>(Guid key, TTag tag)
        {
            var type = typeof(TSnapshot);
            if (!caches.TryGetValue(type, out var cache)) return;
            cache.Remove(key, tag);
        }

        public static void Remove<TSnapshot>(Guid key)
        {
            var type = typeof(TSnapshot);
            if (!caches.TryGetValue(type, out var cache)) return;
            cache.RemoveAll(key);
        }

        public static void RemoveAll(Guid key)
        {
            foreach (var cache in caches.Values)
                cache.RemoveAll(key);
        }

        public static IEnumerable<(Guid Key, TTag Tag, TSnapshot Snapshot)> GetAll<TSnapshot>()
        {
            var type = typeof(TSnapshot);
            if (caches.TryGetValue(type, out var cache))
            {
                var typedCache = (TypedCache<TSnapshot>)cache;
                foreach (var kvp in typedCache.storage)
                {
                    yield return (kvp.Key.Item1, kvp.Key.Item2, kvp.Value);
                }
            }
        }

        public static void Clear()
        {
            foreach (var cache in caches.Values)
                cache.Clear();
            caches.Clear();
        }
    }
}
