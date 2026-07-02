// 文件: Tests/BenchmarkStress.cs
using NUnit.Framework;
using PoolKit.Collections;
using Unity.PerformanceTesting;

namespace PoolKit.Tests.Benchmarks
{
    /// <summary>
    /// 综合压力测试 - 运行于 PlayMode
    /// </summary>
    public class BenchmarkStress : BenchmarkBase
    {
        private const int IterationsPerMeasurement = 50;

        [Test, Performance]
        public void Stress_AllPools()
        {
            Measure.Method(() =>
            {
                var list = ListPool.Rent<int>();
                for (int i = 0; i < 100; i++) list.Add(i);
                ListPool.Return(list);

                var dict = DictionaryPool.Rent<string, int>();
                for (int i = 0; i < 50; i++) dict[$"k{i}"] = i;
                DictionaryPool.Return(dict);

                var queue = QueuePool.Rent<int>();
                for (int i = 0; i < 100; i++) queue.Enqueue(i);
                QueuePool.Return(queue);

                var stack = StackPool.Rent<int>();
                for (int i = 0; i < 100; i++) stack.Push(i);
                StackPool.Return(stack);

                var set = HashSetPool.Rent<int>();
                for (int i = 0; i < 100; i++) set.Add(i);
                HashSetPool.Return(set);

                var array = ArrayPool.Rent<int>(256);
                for (int i = 0; i < array.Length; i++) array[i] = i;
                ArrayPool.Return(array);
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void Stress_ClassPool_1000Objects()
        {
            var pool = new ClassPool<TestObject>(1000);

            Measure.Method(() =>
            {
                var objects = new TestObject[1000];
                for (int i = 0; i < 1000; i++)
                    objects[i] = pool.Get();
                for (int i = 0; i < 1000; i++)
                    pool.Release(objects[i]);
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .IterationsPerMeasurement(20)
            .GC()
            .Run();
        }

        private class TestObject
        {
            public int[] Data { get; set; } = new int[10];
        }
    }
}