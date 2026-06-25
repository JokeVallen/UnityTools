using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;

public class ComparerUtilityPerformanceTests
{
    private const int WarmupCount = 5;
    private const int MeasurementCount = 20;

    // ---------- 通用辅助 ----------
    private static void RegisterEqualityComparers(int count, out string[] keys)
    {
        keys = new string[count];
        for (int i = 0; i < count; i++) keys[i] = $"eq_key_{i}";
        var comparer = EqualityComparer<string>.Default;
        foreach (var k in keys)
            ComparerUtility.SetEqualityComparer<string, string>(k, comparer);
    }

    private static void RegisterComparers(int count, out string[] keys)
    {
        keys = new string[count];
        for (int i = 0; i < count; i++) keys[i] = $"cmp_key_{i}";
        var comparer = Comparer<string>.Default;
        foreach (var k in keys)
            ComparerUtility.SetComparer<string, string>(k, comparer);
    }

    // ============================================================
    // EqualityComparer Performance
    // ============================================================

    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    [TestCase(10000)]
    [Performance]
    public void SetAndGetEqualityComparer_Performance(int count)
    {
        RegisterEqualityComparers(count, out var keys);
        var random = new Random(12345);

        // 命中
        Measure.Method(() =>
        {
            var idx = random.Next(0, count);
            var result = ComparerUtility.GetEqualityComparer<string, string>(keys[idx]);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();

        // 未命中（回退默认值）
        var missingKey = "eq_missing";
        Measure.Method(() =>
        {
            var result = ComparerUtility.GetEqualityComparerOrDefault<string, string>(missingKey);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();

        // 基线：直接访问默认值
        Measure.Method(() =>
        {
            var result = EqualityComparer<string>.Default;
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }

    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    [Performance]
    public void NonGenericGetEqualityComparer_Performance(int count)
    {
        RegisterEqualityComparers(count, out var keys);
        var random = new Random(12345);
        // 修复：使用比较器的实际类型，而非 typeof(string)
        var comparerType = EqualityComparer<string>.Default.GetType();

        // 命中（类型匹配）
        Measure.Method(() =>
        {
            var idx = random.Next(0, count);
            var result = ComparerUtility.GetEqualityComparer<string>(keys[idx], comparerType);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();

        // 未命中回退默认值（带类型校验）
        var missingKey = "eq_missing";
        Measure.Method(() =>
        {
            var result = ComparerUtility.GetEqualityComparerOrDefault<string>(missingKey, typeof(string), comparerType);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();

        // 未命中回退默认值（无类型校验）
        Measure.Method(() =>
        {
            var result = ComparerUtility.GetEqualityComparerOrDefault<string>(missingKey, typeof(string));
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }

    // ============================================================
    // Comparer Performance（完全对称）
    // ============================================================

    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    [TestCase(10000)]
    [Performance]
    public void SetAndGetComparer_Performance(int count)
    {
        RegisterComparers(count, out var keys);
        var random = new Random(12345);

        // 命中
        Measure.Method(() =>
        {
            var idx = random.Next(0, count);
            var result = ComparerUtility.GetComparer<string, string>(keys[idx]);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();

        // 未命中
        var missingKey = "cmp_missing";
        Measure.Method(() =>
        {
            var result = ComparerUtility.GetComparerOrDefault<string, string>(missingKey);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();

        // 基线
        Measure.Method(() =>
        {
            var result = Comparer<string>.Default;
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }

    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    [Performance]
    public void NonGenericGetComparer_Performance(int count)
    {
        RegisterComparers(count, out var keys);
        var random = new Random(12345);
        var comparerType = Comparer<string>.Default.GetType();

        Measure.Method(() =>
        {
            var idx = random.Next(0, count);
            var result = ComparerUtility.GetComparer<string>(keys[idx], comparerType);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();

        var missingKey = "cmp_missing";
        Measure.Method(() =>
        {
            var result = ComparerUtility.GetComparerOrDefault<string>(missingKey, typeof(string), comparerType);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();

        Measure.Method(() =>
        {
            var result = ComparerUtility.GetComparerOrDefault<string>(missingKey, typeof(string));
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }

    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    [Performance]
    public void NonGenericGetEqualityComparerOrDefault_WithTypeCheck_Performance(int count)
    {
        RegisterEqualityComparers(count, out var keys);
        var random = new Random(12345);
        var comparerType = EqualityComparer<string>.Default.GetType();
        var elementType = typeof(string);

        // 命中（有类型校验）
        Measure.Method(() =>
        {
            var idx = random.Next(0, count);
            var result = ComparerUtility.GetEqualityComparerOrDefault<string>(keys[idx], elementType, comparerType);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();

        // 未命中（有类型校验）
        var missingKey = "eq_missing";
        Measure.Method(() =>
        {
            var result = ComparerUtility.GetEqualityComparerOrDefault<string>(missingKey, elementType, comparerType);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }

    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    [Performance]
    public void NonGenericGetComparerOrDefault_WithTypeCheck_Performance(int count)
    {
        RegisterComparers(count, out var keys);
        var random = new Random(12345);
        var comparerType = Comparer<string>.Default.GetType();
        var elementType = typeof(string);

        Measure.Method(() =>
        {
            var idx = random.Next(0, count);
            var result = ComparerUtility.GetComparerOrDefault<string>(keys[idx], elementType, comparerType);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();

        var missingKey = "cmp_missing";
        Measure.Method(() =>
        {
            var result = ComparerUtility.GetComparerOrDefault<string>(missingKey, elementType, comparerType);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }

    // ============================================================
    // 并发压力测试（Equality 和 Comparer 各测一次）
    // ============================================================

    [Test]
    [Performance]
    public void ConcurrentGetEqualityComparer_Performance()
    {
        const int threadCount = 10;
        const int iterationsPerThread = 1000;
        var keys = new string[100];
        for (int i = 0; i < keys.Length; i++) keys[i] = $"con_eq_{i}";
        var comparer = EqualityComparer<string>.Default;
        foreach (var k in keys)
            ComparerUtility.SetEqualityComparer<string, string>(k, comparer);

        Measure.Method(() =>
        {
            System.Threading.Tasks.Parallel.For(0, threadCount, t =>
            {
                var random = new Random(t + 1);
                for (int i = 0; i < iterationsPerThread; i++)
                {
                    var idx = random.Next(0, keys.Length);
                    var result = ComparerUtility.GetEqualityComparer<string, string>(keys[idx]);
                    if (result == null) throw new Exception("Unexpected null");
                }
            });
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }

    [Test]
    [Performance]
    public void ConcurrentGetComparer_Performance()
    {
        const int threadCount = 10;
        const int iterationsPerThread = 1000;
        var keys = new string[100];
        for (int i = 0; i < keys.Length; i++) keys[i] = $"con_cmp_{i}";
        var comparer = Comparer<string>.Default;
        foreach (var k in keys)
            ComparerUtility.SetComparer<string, string>(k, comparer);

        Measure.Method(() =>
        {
            System.Threading.Tasks.Parallel.For(0, threadCount, t =>
            {
                var random = new Random(t + 1);
                for (int i = 0; i < iterationsPerThread; i++)
                {
                    var idx = random.Next(0, keys.Length);
                    var result = ComparerUtility.GetComparer<string, string>(keys[idx]);
                    if (result == null) throw new Exception("Unexpected null");
                }
            });
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }

    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    [TestCase(10000)]
    [Performance]
    public void TryGetEqualityComparer_Performance(int count)
    {
        RegisterEqualityComparers(count, out var keys);
        var random = new Random(12345);

        IEqualityComparer<string> result = null;
        Measure.Method(() =>
        {
            var idx = random.Next(0, count);
            ComparerUtility.TryGetEqualityComparer<string, string>(keys[idx], out result);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }

    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    [TestCase(10000)]
    [Performance]
    public void TryGetComparer_Performance(int count)
    {
        RegisterComparers(count, out var keys);
        var random = new Random(12345);

        IComparer<string> result = null;
        Measure.Method(() =>
        {
            var idx = random.Next(0, count);
            ComparerUtility.TryGetComparer<string, string>(keys[idx], out result);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .GC()
        .Run();
    }
}