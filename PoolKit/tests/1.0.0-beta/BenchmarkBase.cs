// 文件: Tests/BenchmarkBase.cs
using NUnit.Framework;
using PoolKit.Collections;
using Unity.PerformanceTesting;
using UnityEngine;

namespace PoolKit.Tests.Benchmarks
{
    /// <summary>
    /// 基准测试基类 - 运行于 PlayMode
    /// </summary>
    public abstract class BenchmarkBase
    {
        protected const int TEST_ITERATIONS = 100000;

        /// <summary>
        /// 清理所有池
        /// </summary>
        protected void CleanupAllPools()
        {
            ListPool.Clear();
            DictionaryPool.Clear();
            QueuePool.Clear();
            StackPool.Clear();
            HashSetPool.Clear();
            ArrayPool.Clear();
        }

        [SetUp]
        public virtual void Setup() { }

        [TearDown]
        public virtual void Teardown()
        {
            CleanupAllPools();
        }
    }
}