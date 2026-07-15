using System;
using System.Collections.Generic;

/// <summary>
/// 强类型上下文
/// </summary>
public sealed class TypedContext : ITypedContext, IResettable
{
    private interface IStorage : IResettable
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

        public void Reset()
        {
            Clear();
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

    /// <inheritdoc/>
    public void Set<TKey, TValue>(TKey key, TValue value)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        var storage = GetStorage<TKey, TValue>();
        storage.dict[key] = value;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public void Clear()
    {
        foreach (var storage in storages.Values)
            storage.Clear();
    }

    /// <inheritdoc/>
    void IResettable.Reset()
    {
        foreach (var storage in storages.Values)
            storage.Reset();
        storages.Clear();
    }
}
