using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

/// <summary>
/// 比较器策略工具类
/// </summary>
/// <remarks>
/// <para>定位：无状态自定义比较器、不可变状态自定义比较器</para>
/// <para>开发价值：缓存和运行时装配能力</para>
/// </remarks>
public static class ComparerUtility
{
    private interface IStorage
    {
        void Clear();
    }

    private class Storage<TKey, TValue> : IStorage
    {
        public readonly ConcurrentDictionary<TKey, TValue> dict = new ConcurrentDictionary<TKey, TValue>();
        public void Clear() => dict.Clear();
    }

    private static readonly ConcurrentDictionary<Type, IStorage> storages = new ConcurrentDictionary<Type, IStorage>();
    private static readonly ConcurrentDictionary<Type, IEqualityComparer> defaultEqualityComparerCache = new ConcurrentDictionary<Type, IEqualityComparer>();
    private static readonly ConcurrentDictionary<Type, IComparer> defaultComparerCache = new ConcurrentDictionary<Type, IComparer>();

    #region EqualityComparer

    /// <summary>
    /// 获取指定的相等性比较器
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <returns>相等性比较器；若未注册则返回 <c>null</c></returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> 为 null</exception>
    public static IEqualityComparer<T> GetEqualityComparer<T, TKey>(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (!TryGetStorage<TKey, IEqualityComparer<T>>(out var storage)) return null;
        return storage.dict.TryGetValue(key, out var comparer) ? comparer : null;
    }

    /// <summary>
    /// 获取指定的相等性比较器
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <returns>相等性比较器</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> 为 null</exception>
    public static IEqualityComparer<T> GetEqualityComparerOrDefault<T, TKey>(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (!TryGetStorage<TKey, IEqualityComparer<T>>(out var storage)) return EqualityComparer<T>.Default;
        return storage.dict.TryGetValue(key, out var comparer) ? comparer : EqualityComparer<T>.Default;
    }

    /// <summary>
    /// 获取指定的相等性比较器
    /// </summary>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <returns>相等性比较器；若未注册则返回 <c>null</c></returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> 为 null</exception>
    public static IEqualityComparer GetEqualityComparer<TKey>(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (!TryGetStorage<TKey, IEqualityComparer>(out var storage)) return null;
        return storage.dict.TryGetValue(key, out var comparer) ? comparer : null;
    }

    /// <summary>
    /// 获取指定的相等性比较器
    /// </summary>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <param name="equalityComparerType">比较器类型</param>
    /// <returns>相等性比较器；若未注册则返回 <c>null</c></returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> 或 <paramref name="equalityComparerType"/> 为 null</exception>
    public static IEqualityComparer GetEqualityComparer<TKey>(TKey key, Type equalityComparerType)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (equalityComparerType == null) throw new ArgumentNullException(nameof(equalityComparerType));
        if (!TryGetStorage<TKey, IEqualityComparer>(out var storage)) return null;
        if (!storage.dict.TryGetValue(key, out var comparer)) return null;
        var type1 = comparer.GetType();
        if (equalityComparerType != type1)
            throw new InvalidCastException($"[ComparerUtility] The actual type of the equality comparer obtained, '{type1}', differs from the expected type '{equalityComparerType}'.");
        return comparer;
    }

    /// <summary>
    /// 获取指定的相等性比较器
    /// </summary>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <param name="type">数据类型</param>
    /// <returns>相等性比较器</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> 或 <paramref name="type"/> 为 null</exception>
    /// <remarks>
    /// <para>该方法在获取默认值时涉及反射开销，但反射获取的默认值实例会进行缓存。</para>
    /// </remarks>
    public static IEqualityComparer GetEqualityComparerOrDefault<TKey>(TKey key, Type type)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (!TryGetStorage<TKey, IEqualityComparer>(out var storage)) return GetDefaultEqualityComparer(type);
        return storage.dict.TryGetValue(key, out var comparer) ? comparer : GetDefaultEqualityComparer(type);
    }

    /// <summary>
    /// 获取指定的相等性比较器
    /// </summary>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <param name="type">数据类型</param>
    /// <param name="equalityComparerType">比较器的类型</param>
    /// <returns>相等性比较器</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> 或 <paramref name="type"/> 或 <paramref name="equalityComparerType"/> 为 null</exception>
    /// <remarks>
    /// <para>该方法在获取默认值时涉及反射开销，但反射获取的默认值实例会进行缓存。</para>
    /// </remarks>
    public static IEqualityComparer GetEqualityComparerOrDefault<TKey>(TKey key, Type type, Type equalityComparerType)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (equalityComparerType == null) throw new ArgumentNullException(nameof(equalityComparerType));
        if (!TryGetStorage<TKey, IEqualityComparer>(out var storage)) return GetDefaultEqualityComparer(type);
        if (!storage.dict.TryGetValue(key, out var comparer)) return GetDefaultEqualityComparer(type);
        var type1 = comparer.GetType();
        if (equalityComparerType != type1)
            throw new InvalidCastException($"[ComparerUtility] The actual type of the equality comparer obtained, '{type1}', differs from the expected type '{equalityComparerType}'.");
        return comparer;
    }

    /// <summary>
    /// 设置指定的相等性比较器
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <param name="comparer">相等性比较器</param>
    /// <exception cref="ArgumentNullException">参数 <paramref name="key"/> 或 <paramref name="comparer"/> 为 null</exception>
    public static void SetEqualityComparer<T, TKey>(TKey key, IEqualityComparer<T> comparer)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (comparer == null) throw new ArgumentNullException(nameof(comparer));
        var storage = GetOrAddStorage<TKey, IEqualityComparer<T>>();
        storage.dict[key] = comparer;
        if (comparer is IEqualityComparer)
        {
            var storage2 = GetOrAddStorage<TKey, IEqualityComparer>();
            storage2.dict[key] = (IEqualityComparer)comparer;
        }
    }

    /// <summary>
    /// 设置指定的相等性比较器
    /// </summary>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <param name="comparer">相等性比较器</param>
    /// <exception cref="ArgumentNullException">参数 <paramref name="key"/> 或 <paramref name="comparer"/> 为 null</exception>
    public static void SetEqualityComparer<TKey>(TKey key, IEqualityComparer comparer)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (comparer == null) throw new ArgumentNullException(nameof(comparer));
        var storage = GetOrAddStorage<TKey, IEqualityComparer>();
        storage.dict[key] = comparer;
    }

    /// <summary>
    /// 移除指定的相等性比较器
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <returns>移除成功返回 true，否则返回 false。</returns>
    /// <exception cref="ArgumentNullException">参数 <paramref name="key"/> 为 null</exception>
    public static bool RemoveEqualityComparer<T, TKey>(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (!TryGetStorage<TKey, IEqualityComparer<T>>(out var storage)) return false;
        bool removed1 = storage.dict.TryRemove(key, out _);
        if (!TryGetStorage<TKey, IEqualityComparer>(out var storage2)) return removed1;
        return removed1 || storage2.dict.TryRemove(key, out _);
    }

    /// <summary>
    /// 移除指定的相等性比较器
    /// </summary>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <returns>移除成功返回 true，否则返回 false。</returns>
    /// <exception cref="ArgumentNullException">参数 <paramref name="key"/> 为 null</exception>
    public static bool RemoveEqualityComparer<TKey>(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (!TryGetStorage<TKey, IEqualityComparer>(out var storage)) return false;
        return storage.dict.TryRemove(key, out _);
    }

    #endregion

    #region Comparer

    /// <summary>
    /// 获取指定的比较器
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <returns>比较器；若未注册则返回 <c>null</c></returns>
    /// <exception cref="ArgumentNullException">参数 <paramref name="key"/> 为 null</exception>
    public static IComparer<T> GetComparer<T, TKey>(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (!TryGetStorage<TKey, IComparer<T>>(out var storage)) return null;
        return storage.dict.TryGetValue(key, out var comparer) ? comparer : null;
    }

    /// <summary>
    /// 获取指定的比较器
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <returns>比较器</returns>
    /// <exception cref="ArgumentNullException">参数 <paramref name="key"/> 为 null</exception>
    public static IComparer<T> GetComparerOrDefault<T, TKey>(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (!TryGetStorage<TKey, IComparer<T>>(out var storage)) return Comparer<T>.Default;
        return storage.dict.TryGetValue(key, out var comparer) ? comparer : Comparer<T>.Default;
    }

    /// <summary>
    /// 获取指定的比较器
    /// </summary>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <returns>比较器；若未注册则返回 <c>null</c></returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> 为 null</exception>
    public static IComparer GetComparer<TKey>(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (!TryGetStorage<TKey, IComparer>(out var storage)) return null;
        return storage.dict.TryGetValue(key, out var comparer) ? comparer : null;
    }

    /// <summary>
    /// 获取指定的比较器
    /// </summary>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <param name="comparerType">比较器类型</param>
    /// <returns>比较器；若未注册则返回 <c>null</c></returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> 或 <paramref name="comparerType"/> 为 null</exception>
    public static IComparer GetComparer<TKey>(TKey key, Type comparerType)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (comparerType == null) throw new ArgumentNullException(nameof(comparerType));
        if (!TryGetStorage<TKey, IComparer>(out var storage)) return null;
        if (!storage.dict.TryGetValue(key, out var comparer)) return null;
        var type1 = comparer.GetType();
        if (comparerType != type1)
            throw new InvalidCastException($"[ComparerUtility] The actual type of the comparer obtained, '{type1}', differs from the expected type '{comparerType}'.");
        return comparer;
    }

    /// <summary>
    /// 获取指定的比较器
    /// </summary>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <param name="type">数据类型</param>
    /// <returns>比较器</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> 或 <paramref name="type"/> 为 null</exception>
    /// <remarks>
    /// <para>该方法在获取默认值时涉及反射开销，但反射获取的默认值实例会进行缓存。</para>
    /// </remarks>
    public static IComparer GetComparerOrDefault<TKey>(TKey key, Type type)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (!TryGetStorage<TKey, IComparer>(out var storage)) return GetDefaultComparer(type);
        return storage.dict.TryGetValue(key, out var comparer) ? comparer : GetDefaultComparer(type);
    }

    /// <summary>
    /// 获取指定的比较器
    /// </summary>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <param name="type">数据类型</param>
    /// <param name="comparerType">比较器类型</param>
    /// <returns>比较器</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> 或 <paramref name="type"/> 或 <paramref name="comparerType"/> 为 null</exception>
    /// <remarks>
    /// <para>该方法在获取默认值时涉及反射开销，但反射获取的默认值实例会进行缓存。</para>
    /// </remarks>
    public static IComparer GetComparerOrDefault<TKey>(TKey key, Type type, Type comparerType)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (comparerType == null) throw new ArgumentNullException(nameof(comparerType));
        if (!TryGetStorage<TKey, IComparer>(out var storage)) return GetDefaultComparer(type);
        if (!storage.dict.TryGetValue(key, out var comparer)) return GetDefaultComparer(type);
        var type1 = comparer.GetType();
        if (comparerType != type1)
            throw new InvalidCastException($"[ComparerUtility] The actual type of the comparer obtained, '{type1}', differs from the expected type '{comparerType}'.");
        return comparer;
    }

    /// <summary>
    /// 设置指定数据类型的比较器
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <param name="comparer">比较器</param>
    /// <exception cref="ArgumentNullException">参数 <paramref name="key"/> 或 <paramref name="comparer"/> 为 null</exception>
    public static void SetComparer<T, TKey>(TKey key, IComparer<T> comparer)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (comparer == null) throw new ArgumentNullException(nameof(comparer));
        var storage = GetOrAddStorage<TKey, IComparer<T>>();
        storage.dict[key] = comparer;
        if (comparer is IComparer)
        {
            var storage2 = GetOrAddStorage<TKey, IComparer>();
            storage2.dict[key] = (IComparer)comparer;
        }
    }

    /// <summary>
    /// 设置指定数据类型的比较器
    /// </summary>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <param name="comparer">比较器</param>
    /// <exception cref="ArgumentNullException">参数 <paramref name="key"/> 或 <paramref name="comparer"/> 为 null</exception>
    public static void SetComparer<TKey>(TKey key, IComparer comparer)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (comparer == null) throw new ArgumentNullException(nameof(comparer));
        var storage = GetOrAddStorage<TKey, IComparer>();
        storage.dict[key] = comparer;
    }

    /// <summary>
    /// 移除指定类型的比较器
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <returns>移除成功返回 true，否则返回 false。</returns>
    public static bool RemoveComparer<T, TKey>(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (!TryGetStorage<TKey, IComparer<T>>(out var storage)) return false;
        bool removed1 = storage.dict.TryRemove(key, out _);
        if (!TryGetStorage<TKey, IComparer>(out var storage2)) return removed1;
        return removed1 || storage2.dict.TryRemove(key, out _);
    }

    /// <summary>
    /// 移除指定类型的比较器
    /// </summary>
    /// <typeparam name="TKey">比较器实例唯一标识类型</typeparam>
    /// <param name="key">比较器实例唯一标识</param>
    /// <returns>移除成功返回 true，否则返回 false。</returns>
    public static bool RemoveComparer<TKey>(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (!TryGetStorage<TKey, IComparer>(out var storage)) return false;
        return storage.dict.TryRemove(key, out _);
    }

    #endregion

    /// <summary>
    /// 清空所有比较器
    /// </summary>
    public static void ClearAll()
    {
        foreach (var storage in storages.Values)
            storage.Clear();
        storages.Clear();
        defaultEqualityComparerCache.Clear();
        defaultComparerCache.Clear();
    }

    private static Storage<TKey, TValue> GetOrAddStorage<TKey, TValue>()
    {
        var typeKey = typeof(Storage<TKey, TValue>);
        return (Storage<TKey, TValue>)storages.GetOrAdd(typeKey, _ => new Storage<TKey, TValue>());
    }

    private static bool TryGetStorage<TKey, TValue>(out Storage<TKey, TValue> storage)
    {
        storage = null;
        var typeKey = typeof(Storage<TKey, TValue>);
        if (!storages.TryGetValue(typeKey, out var rawStorage)) return false;
        storage = (Storage<TKey, TValue>)rawStorage;
        return true;
    }

    private static IEqualityComparer GetDefaultEqualityComparer(Type type)
    {
        if (defaultEqualityComparerCache.TryGetValue(type, out var cache)) return cache;
        var comparer = (IEqualityComparer)typeof(EqualityComparer<>)
        .MakeGenericType(type)
        .GetProperty("Default", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)!
        .GetValue(null)!;
        defaultEqualityComparerCache[type] = comparer;
        return comparer;
    }

    private static IComparer GetDefaultComparer(Type type)
    {
        if (defaultComparerCache.TryGetValue(type, out var cache)) return cache;
        var comparer = (IComparer)typeof(Comparer<>)
        .MakeGenericType(type)
        .GetProperty("Default", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)!
        .GetValue(null)!;
        defaultComparerCache[type] = comparer;
        return comparer;
    }
}