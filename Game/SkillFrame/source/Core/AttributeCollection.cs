using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 属性集合
/// </summary>
public sealed class AttributeCollection : IAttributeCollection, INotifiableAttributeCollection, IResettable
{
    private interface IStorage : IResettable
    {
        int Count { get; }
        void Clear();
        void UnregisterAll();
    }

    private class Storage<TKey, TValue> : IStorage
    {
        public readonly Dictionary<TKey, Attribute<TValue>> dict = new Dictionary<TKey, Attribute<TValue>>();
        public event Action<TKey, Attribute<TValue>> OnChanged;
        public int Count => dict.Count;
        public IEqualityComparer<TValue> equalityComparer;

        public void Clear()
        {
            if (OnChanged != null)
            {
                foreach (var key in dict.Keys)
                    OnChanged(key, default);
            }
            dict.Clear();
        }

        public void UnregisterAll()
        {
            OnChanged = null;
        }

        public bool Remove(in TKey key)
        {
            bool removed = dict.Remove(key);
            if (removed && OnChanged != null)
                OnChanged(key, default);
            return removed;
        }

        public void Set(in TKey key, in TValue value)
        {
            var comparer = equalityComparer == null ? EqualityComparer<TValue>.Default : equalityComparer;
            if (dict.TryGetValue(key, out var old) && old.HasValue && comparer.Equals(old.Value, value))
                return;
            dict[key] = value;
            if (OnChanged != null) OnChanged(key, value);
        }

        public void Reset()
        {
            Clear();
            UnregisterAll();
            equalityComparer = null;
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
        storage.Set(key, value);
    }

    /// <inheritdoc/>
    public Attribute<TValue> Get<TKey, TValue>(TKey key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        var typeKey = typeof(Storage<TKey, TValue>);
        if (!storages.TryGetValue(typeKey, out var rawStorage))
            return Attribute<TValue>.None;

        var storage = (Storage<TKey, TValue>)rawStorage;
        if (storage.dict.TryGetValue(key, out var value))
            return value;
        return Attribute<TValue>.None;
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
        return storage.Remove(key);
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
    public int RemoveAll<TKey, TValue>()
    {
        var typeKey = typeof(Storage<TKey, TValue>);
        if (!storages.TryGetValue(typeKey, out var rawStorage))
            return 0;

        var count = rawStorage.Count;
        rawStorage.Clear();
        return count;
    }

    /// <inheritdoc/>
    public IEnumerable<TKey> GetKeys<TKey, TValue>()
    {
        var typeKey = typeof(Storage<TKey, TValue>);
        if (!storages.TryGetValue(typeKey, out var rawStorage))
            return Enumerable.Empty<TKey>();

        var storage = (Storage<TKey, TValue>)rawStorage;
        return storage.dict.Keys;
    }

    /// <inheritdoc/>
    public IEnumerable<Attribute<TValue>> GetValues<TKey, TValue>()
    {
        var typeKey = typeof(Storage<TKey, TValue>);
        if (!storages.TryGetValue(typeKey, out var rawStorage))
            return Enumerable.Empty<Attribute<TValue>>();

        var storage = (Storage<TKey, TValue>)rawStorage;
        return storage.dict.Values;
    }

    /// <inheritdoc/>
    public void Register<TKey, TValue>(Action<TKey, Attribute<TValue>> callback)
    {
        if (callback == null) throw new ArgumentNullException(nameof(callback));
        var storage = GetStorage<TKey, TValue>();
        storage.OnChanged += callback;
    }

    /// <inheritdoc/>
    public void Unregister<TKey, TValue>(Action<TKey, Attribute<TValue>> callback)
    {
        if (callback == null) throw new ArgumentNullException(nameof(callback));
        var typeKey = typeof(Storage<TKey, TValue>);
        if (!storages.TryGetValue(typeKey, out var rawStorage)) return;

        var storage = (Storage<TKey, TValue>)rawStorage;
        storage.OnChanged -= callback;
    }

    /// <inheritdoc/>
    public void UnregisterAll<TKey, TValue>()
    {
        var typeKey = typeof(Storage<TKey, TValue>);
        if (!storages.TryGetValue(typeKey, out var rawStorage)) return;
        rawStorage.UnregisterAll();
    }

    /// <inheritdoc/>
    public void UnregisterAll()
    {
        foreach (var storage in storages.Values)
            storage.UnregisterAll();
    }

    /// <inheritdoc/>
    public void SetEqualityComparer<TKey, TValue>(IEqualityComparer<TValue> equalityComparer)
    {
        var storage = GetStorage<TKey, TValue>();
        storage.equalityComparer = equalityComparer;
    }

    /// <inheritdoc/>
    void IResettable.Reset()
    {
        foreach (var storage in storages.Values)
            storage.Reset();
        storages.Clear();
    }
}
