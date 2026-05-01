using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using FNV1A.x64;

[TestFixture]
[Category("Performance")]
public class FNV1AUtilityPerformanceTests
{
    private const int MeasurementCount = 10;
    private const int IterationsPerMeasurement = 10000;
    private const int WarmupCount = 5;

    #region 基础类型单项性能

    [Test, Performance]
    public void AppendInt32_Performance()
    {
        int value = 0x12345678;
        Measure.Method(() =>
        {
            ulong hash = FNV1AUtility.Start();
            for (int i = 0; i < IterationsPerMeasurement; i++)
            {
                hash = FNV1AUtility.AppendInt32(hash, value + i);
            }
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(1)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void AppendFloat_Performance()
    {
        float value = 3.14159f;
        Measure.Method(() =>
        {
            ulong hash = FNV1AUtility.Start();
            for (int i = 0; i < IterationsPerMeasurement; i++)
            {
                hash = FNV1AUtility.AppendFloat(hash, value + i * 0.001f);
            }
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(1)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void AppendString_Performance()
    {
        string s = "The quick brown fox jumps over the lazy dog";
        Measure.Method(() =>
        {
            ulong hash = FNV1AUtility.Start();
            for (int i = 0; i < IterationsPerMeasurement; i++)
            {
                hash = FNV1AUtility.AppendString(hash, s);
            }
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(1)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void AppendGuid_Performance()
    {
        Guid g = Guid.NewGuid();
        Measure.Method(() =>
        {
            ulong hash = FNV1AUtility.Start();
            for (int i = 0; i < IterationsPerMeasurement; i++)
            {
                hash = FNV1AUtility.AppendGuid(hash, g);
            }
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(1)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void AppendVector3_Performance()
    {
        Vector3 v = new Vector3(1.1f, 2.2f, 3.3f);
        Measure.Method(() =>
        {
            ulong hash = FNV1AUtility.Start();
            for (int i = 0; i < IterationsPerMeasurement; i++)
            {
                hash = FNV1AUtility.AppendVector3(hash, v);
            }
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(1)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void AppendColor_Performance()
    {
        Color c = new Color(0.1f, 0.2f, 0.3f, 0.4f);
        Measure.Method(() =>
        {
            ulong hash = FNV1AUtility.Start();
            for (int i = 0; i < IterationsPerMeasurement; i++)
            {
                hash = FNV1AUtility.AppendColor(hash, c);
            }
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(1)
        .GC()
        .Run();
    }

    #endregion

    #region 泛型缓存 vs 直接调用（拆分为独立测试）

    [Test, Performance]
    public void AppendInt32_DirectCall_Performance()
    {
        int value = 12345;
        Measure.Method(() =>
        {
            ulong hash = FNV1AUtility.Start();
            for (int i = 0; i < IterationsPerMeasurement; i++)
            {
                hash = FNV1AUtility.AppendInt32(hash, value + i);
            }
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(1)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void AppendForNET_Generic_Int_Performance()
    {
        int value = 12345;
        Measure.Method(() =>
        {
            ulong hash = FNV1AUtility.Start();
            for (int i = 0; i < IterationsPerMeasurement; i++)
            {
                hash = FNV1AUtility.AppendForNET(hash, value + i);
            }
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(1)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void AppendForNET_ObjectOverload_BoxedInt_Performance()
    {
        object boxedInt = 12345;
        Measure.Method(() =>
        {
            ulong hash = FNV1AUtility.Start();
            for (int i = 0; i < IterationsPerMeasurement; i++)
            {
                // 注意：这里使用 object 重载，每次循环会装箱 i 偏移量
                hash = FNV1AUtility.AppendForNET(hash, boxedInt);
            }
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(1)
        .GC()
        .Run();
    }

    #endregion

    #region 集合性能（拆分为独立测试）

    private const int CollectionSize = 1000;
    private const int CollectionIterations = 100; // 外层循环次数，总迭代 = CollectionIterations * MeasurementCount

    [Test, Performance]
    public void AppendArray_IntArray_Performance()
    {
        int[] arr = new int[CollectionSize];
        for (int i = 0; i < arr.Length; i++) arr[i] = i;

        Measure.Method(() =>
        {
            FNV1AUtility.AppendArray(FNV1AUtility.Start(), arr, FNV1AUtility.AppendInt32);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(CollectionIterations)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void ManualArrayLoop_Performance()
    {
        int[] arr = new int[CollectionSize];
        for (int i = 0; i < arr.Length; i++) arr[i] = i;

        Measure.Method(() =>
        {
            ulong hash = FNV1AUtility.Start();
            hash = FNV1AUtility.AppendInt32(hash, arr.Length);
            for (int i = 0; i < arr.Length; i++)
                hash = FNV1AUtility.AppendInt32(hash, arr[i]);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(CollectionIterations)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void AppendList_IntList_Performance()
    {
        List<int> list = new List<int>(CollectionSize);
        for (int i = 0; i < CollectionSize; i++) list.Add(i);

        Measure.Method(() =>
        {
            FNV1AUtility.AppendList(FNV1AUtility.Start(), list, FNV1AUtility.AppendInt32);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(CollectionIterations)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void ManualListLoop_Performance()
    {
        List<int> list = new List<int>(CollectionSize);
        for (int i = 0; i < CollectionSize; i++) list.Add(i);

        Measure.Method(() =>
        {
            ulong hash = FNV1AUtility.Start();
            hash = FNV1AUtility.AppendInt32(hash, list.Count);
            for (int i = 0; i < list.Count; i++)
                hash = FNV1AUtility.AppendInt32(hash, list[i]);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(CollectionIterations)
        .GC()
        .Run();
    }

    #endregion

    #region 自定义哈希接口性能（拆分为独立测试）

    class HashableObject : IFNVHashable
    {
        public int X;
        public float Y;
        public ulong AppendHash(ulong hash)
        {
            hash = FNV1AUtility.AppendInt32(hash, X);
            hash = FNV1AUtility.AppendFloat(hash, Y);
            return hash;
        }
    }

    [Test, Performance]
    public void IFNVHashable_AppendHash_Performance()
    {
        var obj = new HashableObject { X = 42, Y = 3.14f };

        Measure.Method(() =>
        {
            obj.AppendHash(FNV1AUtility.Start());
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(IterationsPerMeasurement)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void DirectCalls_EquivalentToIFNVHashable_Performance()
    {
        var obj = new HashableObject { X = 42, Y = 3.14f };

        Measure.Method(() =>
        {
            ulong h = FNV1AUtility.Start();
            h = FNV1AUtility.AppendInt32(h, obj.X);
            h = FNV1AUtility.AppendFloat(h, obj.Y);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(IterationsPerMeasurement)
        .GC()
        .Run();
    }

    #endregion

    #region 缓存冷启动开销（拆分为独立测试）

    // 注意：冷启动测试只执行一次 Measurement，因为后续调用会走热缓存。

    [Test, Performance]
    public void CollectionCache_ColdStart_FirstCall()
    {
        Measure.Method(() =>
        {
            FNV1AUtility.AppendForCollection<double[], double>(FNV1AUtility.Start(), null, null);
        })
        .WarmupCount(0)
        .MeasurementCount(1)
        .IterationsPerMeasurement(1)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void CollectionCache_Warm_AfterInitialization()
    {
        // 确保缓存已初始化
        FNV1AUtility.AppendForCollection<double[], double>(FNV1AUtility.Start(), new double[0], FNV1AUtility.AppendDouble);

        Measure.Method(() =>
        {
            FNV1AUtility.AppendForCollection<double[], double>(FNV1AUtility.Start(), new double[0], FNV1AUtility.AppendDouble);
        })
        .WarmupCount(1)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(100)
        .GC()
        .Run();
    }

    #endregion

    #region 新增：Unsafe API 性能测试（条件编译）

#if ENABLE_UNSAFE
    [Test, Performance]
    public void AppendGuidUnsafe_Performance()
    {
        Guid g = Guid.NewGuid();
        Measure.Method(() =>
        {
            ulong hash = FNV1AUtility.Start();
            for (int i = 0; i < IterationsPerMeasurement; i++)
            {
                hash = FNV1AUtility.AppendGuidUnsafe(hash, g);
            }
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(1)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void AppendGuidFastUnsafe_Performance()
    {
        Guid g = Guid.NewGuid();
        Measure.Method(() =>
        {
            ulong hash = FNV1AUtility.Start();
            for (int i = 0; i < IterationsPerMeasurement; i++)
            {
                hash = FNV1AUtility.AppendGuidFastUnsafe(hash, g);
            }
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(1)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void AppendForUnsafe_Guid_Performance()
    {
        Guid g = Guid.NewGuid();
        Measure.Method(() =>
        {
            ulong hash = FNV1AUtility.Start();
            for (int i = 0; i < IterationsPerMeasurement; i++)
            {
                hash = FNV1AUtility.AppendForUnsafe(hash, g);
            }
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(1)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void AppendForUnsafe_Object_Guid_Performance()
    {
        object obj = Guid.NewGuid();
        Measure.Method(() =>
        {
            ulong hash = FNV1AUtility.Start();
            for (int i = 0; i < IterationsPerMeasurement; i++)
            {
                hash = FNV1AUtility.AppendForUnsafe(hash, obj);
            }
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(1)
        .GC()
        .Run();
    }
#endif

    #endregion

    #region 新增：空集合性能测试

    [Test, Performance]
    public void AppendArray_Empty_Performance()
    {
        int[] arr = new int[0];
        Measure.Method(() =>
        {
            FNV1AUtility.AppendArray(FNV1AUtility.Start(), arr, FNV1AUtility.AppendInt32);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(CollectionIterations)
        .GC()
        .Run();
    }

    [Test, Performance]
    public void AppendList_Empty_Performance()
    {
        List<int> list = new List<int>();
        Measure.Method(() =>
        {
            FNV1AUtility.AppendList(FNV1AUtility.Start(), list, FNV1AUtility.AppendInt32);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(CollectionIterations)
        .GC()
        .Run();
    }

    #endregion

    #region 新增：组合类型哈希性能

    [Test, Performance]
    public void Combined_Hash_TypicalUseCase_Performance()
    {
        // 模拟一个典型的复合对象哈希：int + string + Vector3
        int id = 12345;
        string name = "Player";
        Vector3 pos = new Vector3(10, 20, 30);

        Measure.Method(() =>
        {
            ulong hash = FNV1AUtility.Start();
            hash = FNV1AUtility.AppendInt32(hash, id);
            hash = FNV1AUtility.AppendString(hash, name);
            hash = FNV1AUtility.AppendVector3(hash, pos);
        })
        .WarmupCount(WarmupCount)
        .MeasurementCount(MeasurementCount)
        .IterationsPerMeasurement(IterationsPerMeasurement)
        .GC()
        .Run();
    }

    #endregion

    #region 零分配断言测试（非性能测试，仅验证 GC 行为）

    [Test]
    public void AppendInt32_ProducesZeroGCAlloc()
    {
        int value = 12345;
        ulong hash = FNV1AUtility.Start();

        // 预热
        for (int i = 0; i < 100; i++)
            hash = FNV1AUtility.AppendInt32(hash, value + i);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        long memBefore = GC.GetTotalMemory(false);

        for (int i = 0; i < 10000; i++)
            hash = FNV1AUtility.AppendInt32(hash, value + i);

        long memAfter = GC.GetTotalMemory(false);
        long allocated = memAfter - memBefore;

        Assert.AreEqual(0, allocated, "AppendInt32 应零分配");
        Debug.Log($"Final hash: {hash}");
    }

    [Test]
    public void AppendArray_ProducesZeroExtraAlloc()
    {
        int[] arr = new int[1000];
        for (int i = 0; i < arr.Length; i++) arr[i] = i;

        // 预热
        FNV1AUtility.AppendArray(FNV1AUtility.Start(), arr, FNV1AUtility.AppendInt32);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        long memBefore = GC.GetTotalMemory(false);

        for (int i = 0; i < 100; i++)
            FNV1AUtility.AppendArray(FNV1AUtility.Start(), arr, FNV1AUtility.AppendInt32);

        long memAfter = GC.GetTotalMemory(false);
        long allocated = memAfter - memBefore;

        Assert.AreEqual(0, allocated, "AppendArray 迭代过程中应零分配");
    }

    [Test]
    public void AppendForNET_Generic_NoAlloc()
    {
        int value = 42;

        // 预热缓存
        FNV1AUtility.AppendForNET(FNV1AUtility.Start(), value);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        long memBefore = GC.GetTotalMemory(false);

        ulong hash = FNV1AUtility.Start();
        for (int i = 0; i < 10000; i++)
            hash = FNV1AUtility.AppendForNET(hash, value + i);

        long memAfter = GC.GetTotalMemory(false);
        long allocated = memAfter - memBefore;

        Assert.AreEqual(0, allocated, "泛型 AppendForNET 应零分配");
    }

    #endregion
}