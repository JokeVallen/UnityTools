using System;
using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    internal sealed class TypedPipelineContext : ITypedPipelineContext, IResettable
    {
        private interface IStorage
        {
            void Clear();
        }

        private class Storage<TKey, TValue> : IStorage
        {
            public readonly Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

            public void Clear()
            {
                dict.Clear();
            }
        }

        private readonly Dictionary<Type, IStorage> storages = new Dictionary<Type, IStorage>();

        private Storage<TKey, TValue> GetStorage<TKey, TValue>()
        {
            var typeKey = typeof(Storage<TKey, TValue>);
            if (!storages.TryGetValue(typeKey, out var storage))
            {
                storage = new Storage<TKey, TValue>();
                storages[typeKey] = storage;
            }
            return (Storage<TKey, TValue>)storage;
        }

        public void Set<TKey, TValue>(TKey key, TValue value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var storage = GetStorage<TKey, TValue>();
            storage.dict[key] = value;
        }

        public Optional<TValue> Get<TKey, TValue>(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var typeKey = typeof(Storage<TKey, TValue>);
            if (!storages.TryGetValue(typeKey, out var rawStorage))
                return Optional<TValue>.None;

            var storage = (Storage<TKey, TValue>)rawStorage;
            if (storage.dict.TryGetValue(key, out var value))
                return value;
            return Optional<TValue>.None;
        }

        public bool Remove<TKey, TValue>(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var typeKey = typeof(Storage<TKey, TValue>);
            if (!storages.TryGetValue(typeKey, out var rawStorage))
                return false;

            var storage = (Storage<TKey, TValue>)rawStorage;
            return storage.dict.Remove(key);
        }

        public bool ContainsKey<TKey, TValue>(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var typeKey = typeof(Storage<TKey, TValue>);
            if (!storages.TryGetValue(typeKey, out var rawStorage))
                return false;

            var storage = (Storage<TKey, TValue>)rawStorage;
            return storage.dict.ContainsKey(key);
        }

        public void Clear()
        {
            foreach (var storage in storages.Values)
            {
                storage.Clear();
            }
            storages.Clear();
        }

        public void Reset()
        {
            Clear();
        }
    }
}
