// 文件: Tests/TestStress.cs
using NUnit.Framework;
using PoolKit.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoolKit.Tests
{
    /// <summary>
    /// 压力测试 - 运行于 EditMode
    /// </summary>
    public class TestStress : TestBase
    {
        private const int ITERATIONS = 10000;
        private const int CONCURRENT_TASKS = 10;

        [Test]
        public void ClassPool_Stress_GetAndRelease()
        {
            var pool = new ClassPool<TestObject>(50);

            for (int i = 0; i < ITERATIONS; i++)
            {
                var obj = pool.Get();
                pool.Release(obj);
            }

            Assert.AreEqual(1, pool.TotalCount); // 只创建了一个
            Assert.AreEqual(1, pool.FreeCount);
        }

        [Test]
        public void ClassPool_Stress_MultipleObjects()
        {
            var pool = new ClassPool<TestObject>(50);
            var objects = new List<TestObject>();

            for (int i = 0; i < 50; i++)
            {
                objects.Add(pool.Get());
            }

            Assert.AreEqual(50, pool.TotalCount);
            Assert.AreEqual(0, pool.FreeCount);

            foreach (var obj in objects)
            {
                pool.Release(obj);
            }

            Assert.AreEqual(50, pool.FreeCount);
        }

        [Test]
        public void ListPool_Stress_ConcurrentRent()
        {
            var tasks = new Task[CONCURRENT_TASKS];
            var lists = new List<List<int>>[CONCURRENT_TASKS];

            for (int t = 0; t < CONCURRENT_TASKS; t++)
            {
                int taskId = t;
                tasks[taskId] = Task.Run(() =>
                {
                    lists[taskId] = new List<List<int>>();
                    for (int i = 0; i < 100; i++)
                    {
                        var list = ListPool.Rent<int>();
                        list.Add(taskId);
                        list.Add(i);
                        lists[taskId].Add(list);
                    }
                });
            }

            Task.WaitAll(tasks);

            // 归还所有列表
            for (int t = 0; t < CONCURRENT_TASKS; t++)
            {
                foreach (var list in lists[t])
                {
                    ListPool.Return(list);
                }
            }

            // 验证可以再次租借
            var testList = ListPool.Rent<int>();
            Assert.NotNull(testList);
            ListPool.Return(testList);
        }

        [Test]
        public void ArrayPool_Stress_DifferentSizes()
        {
            var arrays = new List<int[]>();
            var sizes = new[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024 };

            for (int i = 0; i < 100; i++)
            {
                foreach (var size in sizes)
                {
                    var array = ArrayPool.Rent<int>(size);
                    arrays.Add(array);
                }
            }

            foreach (var array in arrays)
            {
                ArrayPool.Return(array);
            }

            // 验证分桶工作正常
            var sameSizeArray = ArrayPool.Rent<int>(64);
            Assert.GreaterOrEqual(sameSizeArray.Length, 64);
            ArrayPool.Return(sameSizeArray);
        }
    }
}