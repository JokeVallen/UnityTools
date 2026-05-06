using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// 比较器缓存工具类
/// </summary>
public static class ComparerUtility
{
    private interface IEqualityComparerAdapter { IEqualityComparer NongenericOriginal { get; } }
    private interface IEqualityComparerAdapter<T> { IEqualityComparer<T> Original { get; } }

    private class EqualityComparerAdapter<T> : IEqualityComparer, IEqualityComparerAdapter, IEqualityComparer<T>, IEqualityComparerAdapter<T>
    {
        public IEqualityComparer<T> Original => comparer ?? this;
        public IEqualityComparer NongenericOriginal => nongenericComparer ?? this;
        private readonly IEqualityComparer<T> comparer;
        private readonly IEqualityComparer nongenericComparer;
        public EqualityComparerAdapter(IEqualityComparer<T> comparer) => this.comparer = comparer;
        public EqualityComparerAdapter(IEqualityComparer nongenericComparer) => this.nongenericComparer = nongenericComparer;
        public bool Equals(T x, T y) => comparer != null ? comparer.Equals(x, y) : nongenericComparer.Equals(x, y);
        public int GetHashCode(T obj) => comparer != null ? comparer.GetHashCode(obj) : nongenericComparer.GetHashCode(obj);
        bool IEqualityComparer.Equals(object x, object y) => comparer != null ? comparer.Equals((T)x, (T)y) : nongenericComparer.Equals(x, y);
        int IEqualityComparer.GetHashCode(object obj) => comparer != null ? comparer.GetHashCode((T)obj) : nongenericComparer.GetHashCode(obj);
    }

    private interface IComparerAdapter { IComparer NongenericOriginal { get; } }
    private interface IComparerAdapter<T> { IComparer<T> Original { get; } }

    private class ComparerAdapter<T> : IComparer, IComparerAdapter, IComparer<T>, IComparerAdapter<T>
    {
        public IComparer<T> Original => comparer ?? this;
        public IComparer NongenericOriginal => nongenericComparer ?? this;
        private readonly IComparer<T> comparer;
        private readonly IComparer nongenericComparer;
        public ComparerAdapter(IComparer<T> comparer) => this.comparer = comparer;
        public ComparerAdapter(IComparer nongenericComparer) => this.nongenericComparer = nongenericComparer;
        public int Compare(T x, T y) => comparer != null ? comparer.Compare(x, y) : nongenericComparer.Compare(x, y);
        int IComparer.Compare(object x, object y) => comparer != null ? comparer.Compare((T)x, (T)y) : nongenericComparer.Compare(x, y);
    }

    private static readonly ConcurrentDictionary<Type, IEqualityComparer> equalityComparers = new ConcurrentDictionary<Type, IEqualityComparer>();
    private static readonly ConcurrentDictionary<Type, IComparer> comparers = new ConcurrentDictionary<Type, IComparer>();

    #region EqualityComparer

    /// <summary>
    /// 获取指定数据类型的相等性比较器
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <returns>相等性比较器</returns>
    public static IEqualityComparer<T> GetEqualityComparer<T>()
    {
        if (!equalityComparers.TryGetValue(typeof(T), out var comparer))
            return EqualityComparer<T>.Default;

        if (comparer is IEqualityComparerAdapter<T> adapter)
            return adapter.Original;

        if (comparer is IEqualityComparer<T> typed)
            return typed;

        var wrapper = new EqualityComparerAdapter<T>(comparer);
        equalityComparers.TryUpdate(typeof(T), wrapper, comparer);
        return wrapper;
    }

    /// <summary>
    /// 获取指定数据类型的相等性比较器
    /// </summary>
    /// <param name="type">数据类型</param>
    /// <returns>相等性比较器</returns>
    /// <exception cref="ArgumentNullException">参数 <paramref name="type"/> 为 null</exception>
    /// <remarks>
    /// <para>非泛型方法若未命中缓存则通过反射获取默认相等性比较器。</para>
    /// <para>若要避免反射，请提前为该类型设置相等性比较器。</para>
    /// </remarks>
    public static IEqualityComparer GetEqualityComparer(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        if (!equalityComparers.TryGetValue(type, out var comparer))
            return DefaultEqualityComparerCache.GetDefaultComparer(type);

        if (comparer is IEqualityComparerAdapter adapter)
            return adapter.NongenericOriginal;

        return comparer;
    }

    /// <summary>
    /// 设置指定数据类型的相等性比较器
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="comparer">相等性比较器</param>
    /// <exception cref="ArgumentNullException">参数 <paramref name="comparer"/> 为 null</exception>
    public static void SetEqualityComparer<T>(IEqualityComparer<T> comparer)
    {
        if (comparer == null) throw new ArgumentNullException(nameof(comparer));
        if (comparer is IEqualityComparer nongeneric) SetEqualityComparer(typeof(T), nongeneric);
        else SetEqualityComparer(typeof(T), new EqualityComparerAdapter<T>(comparer));
    }

    /// <summary>
    /// 设置指定数据类型的相等性比较器
    /// </summary>
    /// <param name="type">数据类型</param>
    /// <param name="comparer">相等性比较器</param>
    /// <exception cref="ArgumentNullException">参数 <paramref name="type"/> 或 <paramref name="comparer"/> 为 null</exception>
    public static void SetEqualityComparer(Type type, IEqualityComparer comparer)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (comparer == null) throw new ArgumentNullException(nameof(comparer));
        equalityComparers[type] = comparer;
    }

    /// <summary>
    /// 尝试移除指定类型的相等性比较器
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <returns>移除成功返回 true，否则返回 false。</returns>
    public static bool TryRemoveEqualityComparer<T>()
    {
        return TryRemoveEqualityComparer(typeof(T));
    }

    /// <summary>
    /// 尝试移除指定类型的相等性比较器
    /// </summary>
    /// <param name="type">数据类型</param>
    /// <returns>移除成功返回 true，否则返回 false。</returns>
    /// <exception cref="ArgumentNullException">参数 <paramref name="type"/> 为 null</exception>
    public static bool TryRemoveEqualityComparer(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        bool removeFromCustom = equalityComparers.TryRemove(type, out _);
        bool removeFromDefaultCache = DefaultEqualityComparerCache.TryRemove(type);
        return removeFromCustom || removeFromDefaultCache;
    }

    /// <summary>
    /// 清空相等性比较器缓存
    /// </summary>
    public static void ClearEqualityComparers()
    {
        equalityComparers.Clear();
        DefaultEqualityComparerCache.Clear();
    }

    #endregion

    #region Comparer

    /// <summary>
    /// 获取指定数据类型的比较器
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <returns>比较器</returns>
    public static IComparer<T> GetComparer<T>()
    {
        if (!comparers.TryGetValue(typeof(T), out var comparer))
            return Comparer<T>.Default;

        if (comparer is IComparerAdapter<T> adapter)
            return adapter.Original;

        if (comparer is IComparer<T> typed)
            return typed;

        var wrapper = new ComparerAdapter<T>(comparer);
        comparers.TryUpdate(typeof(T), wrapper, comparer);
        return wrapper;
    }

    /// <summary>
    /// 获取指定数据类型的比较器
    /// </summary>
    /// <param name="type">数据类型</param>
    /// <returns>比较器</returns>
    /// <exception cref="ArgumentNullException">参数 <paramref name="type"/> 为 null</exception>
    /// <remarks>
    /// <para>非泛型方法若未命中缓存则通过反射获取默认比较器。</para>
    /// <para>若要避免反射，请提前为该类型设置比较器。</para>
    /// </remarks>
    public static IComparer GetComparer(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        if (!comparers.TryGetValue(type, out var comparer))
            return DefaultComparerCache.GetDefaultComparer(type);

        if (comparer is IComparerAdapter adapter)
            return adapter.NongenericOriginal;

        return comparer;
    }

    /// <summary>
    /// 设置指定数据类型的比较器
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="comparer">比较器</param>
    /// <exception cref="ArgumentNullException">参数 <paramref name="comparer"/> 为 null</exception>
    public static void SetComparer<T>(IComparer<T> comparer)
    {
        if (comparer == null) throw new ArgumentNullException(nameof(comparer));
        if (comparer is IComparer nongeneric) SetComparer(typeof(T), nongeneric);
        else SetComparer(typeof(T), new ComparerAdapter<T>(comparer));
    }

    /// <summary>
    /// 设置指定数据类型的比较器
    /// </summary>
    /// <param name="type">数据类型</param>
    /// <param name="comparer">比较器</param>
    /// <exception cref="ArgumentNullException">参数 <paramref name="type"/> 或 <paramref name="comparer"/> 为 null</exception>
    public static void SetComparer(Type type, IComparer comparer)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (comparer == null) throw new ArgumentNullException(nameof(comparer));
        comparers[type] = comparer;
    }

    /// <summary>
    /// 尝试移除指定类型的比较器
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <returns>移除成功返回 true，否则返回 false。</returns>
    public static bool TryRemoveComparer<T>()
    {
        return TryRemoveComparer(typeof(T));
    }

    /// <summary>
    /// 尝试移除指定类型的比较器
    /// </summary>
    /// <param name="type">数据类型</param>
    /// <returns>移除成功返回 true，否则返回 false。</returns>
    /// <exception cref="ArgumentNullException">参数 <paramref name="type"/> 为 null</exception>
    public static bool TryRemoveComparer(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        bool removeFromCustom = comparers.TryRemove(type, out _);
        bool removeFromDefaultCache = DefaultComparerCache.TryRemove(type);
        return removeFromCustom || removeFromDefaultCache;
    }

    /// <summary>
    /// 清空比较器缓存
    /// </summary>
    public static void ClearComparers()
    {
        comparers.Clear();
        DefaultComparerCache.Clear();
    }

    #endregion

    #region Default Comparer Caching

    private static class DefaultEqualityComparerCache
    {
        private static readonly ConcurrentDictionary<Type, IEqualityComparer> cache = new ConcurrentDictionary<Type, IEqualityComparer>();

        public static IEqualityComparer GetDefaultComparer(Type type)
        {
            return cache.GetOrAdd(type, t =>
            {
                try
                {
                    var comparerType = typeof(EqualityComparer<>).MakeGenericType(type);
                    var defaultProperty = comparerType.GetProperty("Default", BindingFlags.Static | BindingFlags.Public);
                    if (defaultProperty == null)
                        throw new InvalidOperationException($"The type '{comparerType}' doesn't have a 'Default' property.");
                    return (IEqualityComparer)defaultProperty.GetValue(null);
                }
                catch (Exception ex) when (ex is ArgumentException || ex is TypeLoadException || ex is NotSupportedException)
                {
                    throw new InvalidOperationException(
                        $"Cannot create default equality comparer for type '{t}'. The type may be unsupported (e.g., pointer, ByRef, open generic).", ex);
                }
            });
        }

        public static bool TryRemove(Type type) { return cache.TryRemove(type, out _); }
        public static void Clear() { cache.Clear(); }
    }

    private static class DefaultComparerCache
    {
        private static readonly ConcurrentDictionary<Type, IComparer> cache = new ConcurrentDictionary<Type, IComparer>();

        public static IComparer GetDefaultComparer(Type type)
        {
            return cache.GetOrAdd(type, t =>
            {
                try
                {
                    var comparerType = typeof(Comparer<>).MakeGenericType(type);
                    var defaultProperty = comparerType.GetProperty("Default", BindingFlags.Static | BindingFlags.Public);
                    if (defaultProperty == null)
                        throw new InvalidOperationException($"The type '{comparerType}' doesn't have a 'Default' property.");
                    return (IComparer)defaultProperty.GetValue(null);
                }
                catch (Exception ex) when (ex is ArgumentException || ex is TypeLoadException || ex is NotSupportedException)
                {
                    throw new InvalidOperationException(
                        $"Cannot create default comparer for type '{t}'. The type may be unsupported (e.g., pointer, ByRef, open generic).", ex);
                }
            });
        }

        public static bool TryRemove(Type type) { return cache.TryRemove(type, out _); }
        public static void Clear() { cache.Clear(); }
    }

    #endregion
}