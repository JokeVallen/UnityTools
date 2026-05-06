using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyBinder;

[TestFixture]
public class ComparerUtilityTests
{
    [SetUp]
    public void SetUp()
    {
        ComparerUtility.ClearEqualityComparers();
        ComparerUtility.ClearComparers();
    }

    #region EqualityComparer Tests

    [Test]
    public void GetEqualityComparer_NoCustom_ReturnsDefault()
    {
        var comparer = ComparerUtility.GetEqualityComparer<string>();
        Assert.That(comparer, Is.EqualTo(EqualityComparer<string>.Default));
    }

    [Test]
    public void GetEqualityComparer_NonGeneric_NoCustom_ReturnsDefault()
    {
        var comparer = ComparerUtility.GetEqualityComparer(typeof(int));
        Assert.That(comparer, Is.EqualTo(EqualityComparer<int>.Default));
    }

    [Test]
    public void SetEqualityComparer_Generic_AndGet_ReturnsSameInstance()
    {
        var custom = new CustomEqualityComparer<string>();
        ComparerUtility.SetEqualityComparer<string>(custom);

        var retrieved = ComparerUtility.GetEqualityComparer<string>();
        Assert.That(retrieved, Is.SameAs(custom));
    }

    [Test]
    public void SetEqualityComparer_Generic_OnlyNonGeneric_AndGet_ReturnsSameInstance()
    {
        // 只实现 IEqualityComparer（非泛型）
        var custom = new NonGenericEqualityComparer();
        ComparerUtility.SetEqualityComparer(typeof(int), custom);

        // 非泛型 Get 直接返回原对象
        var retrievedNonGeneric = ComparerUtility.GetEqualityComparer(typeof(int));
        Assert.That(retrievedNonGeneric, Is.SameAs(custom));

        // 泛型 Get 应返回一个可用的比较器（通过适配器，但不影响引用测试）
        var retrievedGeneric = ComparerUtility.GetEqualityComparer<int>();
        Assert.That(retrievedGeneric, Is.Not.Null);
        // 功能验证：比较行为应一致
        Assert.That(retrievedGeneric.Equals(1, 1), Is.True);
        Assert.That(retrievedGeneric.GetHashCode(1), Is.EqualTo(1)); // 自定义的返回自身
    }

    [Test]
    public void SetEqualityComparer_Generic_OnlyGenericInterface_AndGet_ReturnsOriginal()
    {
        var custom = new OnlyGenericEqualityComparer<int>();
        ComparerUtility.SetEqualityComparer<int>(custom);

        var retrieved = ComparerUtility.GetEqualityComparer<int>();
        Assert.That(retrieved, Is.SameAs(custom));
    }

    [Test]
    public void GetEqualityComparer_NonGeneric_AfterGenericSet_ReturnsOriginalNonGeneric()
    {
        // 设置一个同时实现 IEqualityComparer<T> 和 IEqualityComparer 的比较器
        var custom = new FullEqualityComparer<int>();
        ComparerUtility.SetEqualityComparer<int>(custom);

        var nonGeneric = ComparerUtility.GetEqualityComparer(typeof(int));
        Assert.That(nonGeneric, Is.SameAs(custom));
    }

    [Test]
    public void TryRemoveEqualityComparer_RemovesCustomAndDefaultCache()
    {
        ComparerUtility.SetEqualityComparer(typeof(int), new NonGenericEqualityComparer());
        bool removed = ComparerUtility.TryRemoveEqualityComparer(typeof(int));
        Assert.That(removed, Is.True);

        // 再次移除应返回 false
        removed = ComparerUtility.TryRemoveEqualityComparer(typeof(int));
        Assert.That(removed, Is.False);
    }

    [Test]
    public void TryRemoveEqualityComparer_RemovesDefaultCacheIfExists()
    {
        // 触发默认缓存创建
        var _ = ComparerUtility.GetEqualityComparer(typeof(double));
        bool removed = ComparerUtility.TryRemoveEqualityComparer(typeof(double));
        Assert.That(removed, Is.True);

        // 再次获取会重新创建默认缓存
        var comparerAfter = ComparerUtility.GetEqualityComparer(typeof(double));
        Assert.That(comparerAfter, Is.Not.Null);
    }

    [Test]
    public void ClearEqualityComparers_ClearsAll()
    {
        ComparerUtility.SetEqualityComparer(typeof(int), new NonGenericEqualityComparer());
        var _ = ComparerUtility.GetEqualityComparer(typeof(string)); // 触发默认缓存

        ComparerUtility.ClearEqualityComparers();
        // 重新获取应该再次创建默认而不是使用缓存
        var comparer = ComparerUtility.GetEqualityComparer(typeof(int));
        Assert.That(comparer, Is.EqualTo(EqualityComparer<int>.Default));
    }

    [Test]
    public void GetEqualityComparer_UnsupportedType_ThrowsInvalidOperationException()
    {
        // 指针类型无法创建默认比较器
        Type pointerType = typeof(int*);
        Assert.That(() => ComparerUtility.GetEqualityComparer(pointerType),
            Throws.InvalidOperationException.With.Message.Contains("Cannot create default equality comparer"));
    }

    [Test]
    public void SetEqualityComparer_NullArguments_ThrowsArgumentNullException()
    {
        Assert.That(() => ComparerUtility.SetEqualityComparer<string>(null),
            Throws.ArgumentNullException);
        Assert.That(() => ComparerUtility.SetEqualityComparer(null, EqualityComparer<int>.Default),
            Throws.ArgumentNullException);
        Assert.That(() => ComparerUtility.SetEqualityComparer(typeof(int), null),
            Throws.ArgumentNullException);
    }

    [Test]
    public void GetEqualityComparer_NullType_ThrowsArgumentNullException()
    {
        Assert.That(() => ComparerUtility.GetEqualityComparer(null),
            Throws.ArgumentNullException);
        Assert.That(() => ComparerUtility.TryRemoveEqualityComparer(null),
            Throws.ArgumentNullException);
    }

    [Test]
    public void EqualityComparer_Adapter_PreservesCorrectness()
    {
        // 只实现非泛型接口的比较器，通过泛型 Get 后应能正确比较
        var custom = new NonGenericEqualityComparer();
        ComparerUtility.SetEqualityComparer(typeof(int), custom);

        var genericComparer = ComparerUtility.GetEqualityComparer<int>();
        Assert.That(genericComparer.Equals(5, 5), Is.True);
        Assert.That(genericComparer.Equals(5, 10), Is.False);
        Assert.That(genericComparer.GetHashCode(5), Is.EqualTo(5)); // 自定义实现返回自身
    }

    #endregion

    #region Comparer Tests

    [Test]
    public void GetComparer_NoCustom_ReturnsDefault()
    {
        var comparer = ComparerUtility.GetComparer<int>();
        Assert.That(comparer, Is.EqualTo(Comparer<int>.Default));
    }

    [Test]
    public void SetComparer_Generic_AndGet_ReturnsSameInstance()
    {
        var custom = new CustomComparer<int>();
        ComparerUtility.SetComparer<int>(custom);
        Assert.That(ComparerUtility.GetComparer<int>(), Is.SameAs(custom));
    }

    [Test]
    public void SetComparer_NonGeneric_GenericGet_Works()
    {
        var custom = new NonGenericComparer();
        ComparerUtility.SetComparer(typeof(int), custom);

        var genericComparer = ComparerUtility.GetComparer<int>();
        Assert.That(genericComparer, Is.Not.Null);
        Assert.That(genericComparer.Compare(5, 10), Is.LessThan(0)); // 自定义返回 x - y
    }

    [Test]
    public void TryRemoveComparer_ClearsCache()
    {
        ComparerUtility.SetComparer<int>(new CustomComparer<int>());
        Assert.That(ComparerUtility.TryRemoveComparer<int>(), Is.True);
        Assert.That(ComparerUtility.TryRemoveComparer<int>(), Is.False);
    }

    [Test]
    public void ClearComparers_ClearsAll()
    {
        ComparerUtility.SetComparer(typeof(int), new NonGenericComparer());
        var _ = ComparerUtility.GetComparer(typeof(string)); // 触发默认缓存
        ComparerUtility.ClearComparers();

        var comparer = ComparerUtility.GetComparer(typeof(int));
        Assert.That(comparer, Is.EqualTo(Comparer<int>.Default));
    }

    [Test]
    public void SetComparer_NullArguments_ThrowsArgumentNullException()
    {
        Assert.That(() => ComparerUtility.SetComparer<int>(null), Throws.ArgumentNullException);
        Assert.That(() => ComparerUtility.SetComparer(null, Comparer<int>.Default), Throws.ArgumentNullException);
        Assert.That(() => ComparerUtility.SetComparer(typeof(int), null), Throws.ArgumentNullException);
    }

    [Test]
    public void GetComparer_NullType_ThrowsArgumentNullException()
    {
        Assert.That(() => ComparerUtility.GetComparer(null), Throws.ArgumentNullException);
        Assert.That(() => ComparerUtility.TryRemoveComparer(null), Throws.ArgumentNullException);
    }

    [Test]
    public void GetComparer_UnsupportedType_ThrowsInvalidOperationException()
    {
        Type pointerType = typeof(int*);
        Assert.That(() => ComparerUtility.GetComparer(pointerType),
            Throws.InvalidOperationException.With.Message.Contains("Cannot create default comparer"));
    }

    #endregion

    #region Thread Safety (Smoke Test)

    [Test]
    public void ConcurrentAccess_EqualityComparer_DoesNotThrow()
    {
        int taskCount = 4;
        var tasks = new Task[taskCount];

        for (int i = 0; i < taskCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    var comparer = ComparerUtility.GetEqualityComparer<int>();
                    ComparerUtility.SetEqualityComparer(typeof(int), new NonGenericEqualityComparer());
                    ComparerUtility.TryRemoveEqualityComparer(typeof(int));
                }
            });
        }

        Assert.That(() => Task.WaitAll(tasks), Throws.Nothing);
    }

    [Test]
    public void ConcurrentAccess_Comparer_DoesNotThrow()
    {
        int taskCount = 4;
        var tasks = new Task[taskCount];

        for (int i = 0; i < taskCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    var comparer = ComparerUtility.GetComparer<int>();
                    ComparerUtility.SetComparer(typeof(int), new NonGenericComparer());
                    ComparerUtility.TryRemoveComparer(typeof(int));
                }
            });
        }

        Assert.That(() => Task.WaitAll(tasks), Throws.Nothing);
    }

    #endregion

    #region Custom Comparer Implementations for Testing

    // 同时实现 IEqualityComparer<T> 和 IEqualityComparer
    private class FullEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer
    {
        public bool Equals(T x, T y) => EqualityComparer<T>.Default.Equals(x, y);
        public int GetHashCode(T obj) => EqualityComparer<T>.Default.GetHashCode(obj);

        bool IEqualityComparer.Equals(object x, object y) => Equals((T)x, (T)y);
        int IEqualityComparer.GetHashCode(object obj) => GetHashCode((T)obj);
    }

    // 只实现 IEqualityComparer<T>
    private class OnlyGenericEqualityComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y) => EqualityComparer<T>.Default.Equals(x, y);
        public int GetHashCode(T obj) => EqualityComparer<T>.Default.GetHashCode(obj);
    }

    // 只实现 IEqualityComparer（非泛型）
    private class NonGenericEqualityComparer : IEqualityComparer
    {
        public new bool Equals(object x, object y) => (int)x == (int)y;
        public int GetHashCode(object obj) => (int)obj;   // 简单返回 int 值本身
    }

    // 用于字符串的自定义比较器（泛型）
    private class CustomEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer
    {
        public bool Equals(T x, T y) => EqualityComparer<T>.Default.Equals(x, y);
        public int GetHashCode(T obj) => EqualityComparer<T>.Default.GetHashCode(obj);
        bool IEqualityComparer.Equals(object x, object y) => Equals((T)x, (T)y);
        int IEqualityComparer.GetHashCode(object obj) => GetHashCode((T)obj);
    }

    // 比较器测试实现
    private class CustomComparer<T> : IComparer<T>, IComparer
    {
        public int Compare(T x, T y) => Comparer<T>.Default.Compare(x, y);
        int IComparer.Compare(object x, object y) => Compare((T)x, (T)y);
    }

    private class NonGenericComparer : IComparer
    {
        public int Compare(object x, object y) => (int)x - (int)y;
    }

    #endregion
}