// 文件: Tests/BenchmarkCollectionPools.cs
using NUnit.Framework;
using PoolKit.Collections;
using System.Collections.Generic;
using Unity.PerformanceTesting;

namespace PoolKit.Tests.Benchmarks
{
    /// <summary>
    /// 集合池基准测试 - 运行于 PlayMode
    /// 使用 Measure.Method() 测量方法执行时间
    /// </summary>
    public class BenchmarkCollectionPools : BenchmarkBase
    {
        private const int IterationsPerMeasurement = 10000;

        #region ListPool

        [Test, Performance]
        public void ListPool_GetAndReturn()
        {
            Measure.Method(() =>
            {
                var list = ListPool.Rent<int>();
                ListPool.Return(list);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void ListPool_GetAndReturn_WithData()
        {
            Measure.Method(() =>
            {
                var list = ListPool.Rent<int>();
                for (int i = 0; i < 10; i++)
                    list.Add(i);
                ListPool.Return(list);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement / 10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void ListPool_RentWithScope()
        {
            Measure.Method(() =>
            {
                using (var scope = ListPool.RentWithScope<int>())
                {
                    scope.List.Add(1);
                }
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void ListPool_NewVsPooled()
        {
            // 新建 List
            Measure.Method(() =>
            {
                var list = new List<int>();
                list.Add(1);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .SampleGroup(new SampleGroup("New List<int>", SampleUnit.Microsecond))
            .Run();

            // 池化 List
            Measure.Method(() =>
            {
                var list = ListPool.Rent<int>();
                list.Add(1);
                ListPool.Return(list);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .SampleGroup(new SampleGroup("Pooled List<int>", SampleUnit.Microsecond))
            .Run();
        }

        [Test, Performance]
        public void ListPool_DifferentSizes(
            [Values(1, 10, 100, 1000)] int size)
        {
            Measure.Method(() =>
            {
                var list = ListPool.Rent<int>();
                for (int i = 0; i < size; i++)
                    list.Add(i);
                ListPool.Return(list);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement / 10)
            .GC()
            .SampleGroup(new SampleGroup($"ListPool.Size{size}", SampleUnit.Microsecond))
            .Run();
        }

        #endregion

        #region DictionaryPool

        [Test, Performance]
        public void DictionaryPool_GetAndReturn()
        {
            Measure.Method(() =>
            {
                var dict = DictionaryPool.Rent<string, int>();
                DictionaryPool.Return(dict);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void DictionaryPool_GetAndReturn_WithData()
        {
            Measure.Method(() =>
            {
                var dict = DictionaryPool.Rent<string, int>();
                for (int i = 0; i < 10; i++)
                    dict[$"key{i}"] = i;
                DictionaryPool.Return(dict);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement / 10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void DictionaryPool_NewVsPooled()
        {
            Measure.Method(() =>
            {
                var dict = new Dictionary<string, int>();
                dict["test"] = 1;
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement / 10)
            .GC()
            .SampleGroup(new SampleGroup("New Dictionary", SampleUnit.Microsecond))
            .Run();

            Measure.Method(() =>
            {
                var dict = DictionaryPool.Rent<string, int>();
                dict["test"] = 1;
                DictionaryPool.Return(dict);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement / 10)
            .GC()
            .SampleGroup(new SampleGroup("Pooled Dictionary", SampleUnit.Microsecond))
            .Run();
        }

        #endregion

        #region QueuePool

        [Test, Performance]
        public void QueuePool_GetAndReturn()
        {
            Measure.Method(() =>
            {
                var queue = QueuePool.Rent<int>();
                QueuePool.Return(queue);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void QueuePool_GetAndReturn_WithData()
        {
            Measure.Method(() =>
            {
                var queue = QueuePool.Rent<int>();
                for (int i = 0; i < 100; i++)
                    queue.Enqueue(i);
                QueuePool.Return(queue);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement / 10)
            .GC()
            .Run();
        }

        #endregion

        #region StackPool

        [Test, Performance]
        public void StackPool_GetAndReturn()
        {
            Measure.Method(() =>
            {
                var stack = StackPool.Rent<int>();
                StackPool.Return(stack);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void StackPool_GetAndReturn_WithData()
        {
            Measure.Method(() =>
            {
                var stack = StackPool.Rent<int>();
                for (int i = 0; i < 100; i++)
                    stack.Push(i);
                StackPool.Return(stack);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement / 10)
            .GC()
            .Run();
        }

        #endregion

        #region HashSetPool

        [Test, Performance]
        public void HashSetPool_GetAndReturn()
        {
            Measure.Method(() =>
            {
                var set = HashSetPool.Rent<int>();
                HashSetPool.Return(set);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void HashSetPool_GetAndReturn_WithData()
        {
            Measure.Method(() =>
            {
                var set = HashSetPool.Rent<int>();
                for (int i = 0; i < 100; i++)
                    set.Add(i);
                HashSetPool.Return(set);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement / 10)
            .GC()
            .Run();
        }

        #endregion

        #region ArrayPool

        [Test, Performance]
        public void ArrayPool_RentAndReturn(
            [Values(16, 64, 256, 1024)] int size)
        {
            Measure.Method(() =>
            {
                var array = ArrayPool.Rent<int>(size);
                for (int i = 0; i < array.Length; i++)
                    array[i] = i;
                ArrayPool.Return(array);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement / 10)
            .GC()
            .SampleGroup(new SampleGroup($"ArrayPool.Size{size}", SampleUnit.Microsecond))
            .Run();
        }

        [Test, Performance]
        public void ArrayPool_NewVsPooled()
        {
            Measure.Method(() =>
            {
                var array = new int[64];
                array[0] = 1;
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .SampleGroup(new SampleGroup("New int[64]", SampleUnit.Microsecond))
            .Run();

            Measure.Method(() =>
            {
                var array = ArrayPool.Rent<int>(64);
                array[0] = 1;
                ArrayPool.Return(array);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .SampleGroup(new SampleGroup("Pooled int[64]", SampleUnit.Microsecond))
            .Run();
        }

        #endregion
    }
}