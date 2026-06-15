using System;
using System.Collections.Generic;

namespace Orchestrator
{
    /// <summary>
    /// 类型安全的上下文
    /// </summary>
    public sealed class TypedPipelineContext : ITypedPipelineContext
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

        private Storage<TKey, TValue> GetOrAddStorage<TKey, TValue>()
        {
            var typeKey = typeof(Storage<TKey, TValue>);
            if (!storages.TryGetValue(typeKey, out var storage))
            {
                storage = new Storage<TKey, TValue>();
                storages[typeKey] = storage;
            }
            return (Storage<TKey, TValue>)storage;
        }

        /// <inheritdoc/>
        public void Set<TKey, TValue>(TKey key, TValue value)
        {
            var storage = GetOrAddStorage<TKey, TValue>();
            storage.dict[key] = value;
        }

        /// <inheritdoc/>
        public Optional<TValue> Get<TKey, TValue>(TKey key)
        {
            var typeKey = typeof(Storage<TKey, TValue>);
            if (!storages.TryGetValue(typeKey, out var rawStorage))
                return Optional<TValue>.None;

            var storage = (Storage<TKey, TValue>)rawStorage;
            if (storage.dict.TryGetValue(key, out var value))
                return value;
            return Optional<TValue>.None;
        }

        /// <inheritdoc/>
        public bool Remove<TKey, TValue>(TKey key)
        {
            var typeKey = typeof(Storage<TKey, TValue>);
            if (!storages.TryGetValue(typeKey, out var rawStorage))
                return false;

            var storage = (Storage<TKey, TValue>)rawStorage;
            return storage.dict.Remove(key);
        }

        /// <inheritdoc/>
        public bool ContainsKey<TKey, TValue>(TKey key)
        {
            var typeKey = typeof(Storage<TKey, TValue>);
            if (!storages.TryGetValue(typeKey, out var rawStorage))
                return false;

            var storage = (Storage<TKey, TValue>)rawStorage;
            return storage.dict.ContainsKey(key);
        }

        /// <inheritdoc/>
        public void AddStepExecutionResult<TStepKey>(StepExecutionResult<TStepKey> stepExecutionResult)
        {
            if (!stepExecutionResult.StepKey.HasValue)
                throw new ArgumentException($"The step key for this step does not exist.");
            var storage = GetOrAddStorage<TStepKey, StepExecutionResult<TStepKey>>();
            storage.dict[stepExecutionResult.StepKey.Value] = stepExecutionResult;
        }

        /// <inheritdoc/>
        public Optional<StepExecutionResult<TStepKey>> GetStepExecutionResult<TStepKey>(TStepKey key)
        {
            var typeKey = typeof(Storage<TStepKey, StepExecutionResult<TStepKey>>);
            if (!storages.TryGetValue(typeKey, out var rawStorage)) return Optional<StepExecutionResult<TStepKey>>.None;

            var storage = (Storage<TStepKey, StepExecutionResult<TStepKey>>)rawStorage;
            if(!storage.dict.TryGetValue(key,out var result)) return Optional<StepExecutionResult<TStepKey>>.None;
            return result;
        }

        /// <inheritdoc/>
        public IEnumerable<StepExecutionResult<TStepKey>> GetAllStepExecutionResults<TStepKey>()
        {
            var typeKey = typeof(Storage<TStepKey, StepExecutionResult<TStepKey>>);
            if (!storages.TryGetValue(typeKey, out var rawStorage))
                return Array.Empty<StepExecutionResult<TStepKey>>();

            var storage = (Storage<TStepKey, StepExecutionResult<TStepKey>>)rawStorage;
            return storage.dict.Values;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            foreach (var storage in storages.Values)
            {
                storage.Clear();
            }
            storages.Clear();
        }
    }
}
