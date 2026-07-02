// 文件: Tests/TestBase.cs
using NUnit.Framework;
using PoolKit.Collections;
using UnityEngine;

namespace PoolKit.Tests
{
    /// <summary>
    /// 单元测试基类 - 运行于 EditMode
    /// </summary>
    public abstract class TestBase
    {
        [SetUp]
        public virtual void Setup()
        {
            // 确保测试前所有静态池处于干净状态
            CleanupPools();
        }

        [TearDown]
        public virtual void Teardown()
        {
            CleanupPools();
        }

        private void CleanupPools()
        {
            // 清理集合池
            ListPool.Clear();
            DictionaryPool.Clear();
            QueuePool.Clear();
            StackPool.Clear();
            HashSetPool.Clear();
            ArrayPool.Clear();
        }
    }
}