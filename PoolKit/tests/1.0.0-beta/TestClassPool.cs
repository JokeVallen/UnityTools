// 文件: Tests/TestClassPool.cs
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace PoolKit.Tests
{
    /// <summary>
    /// ClassPool 单元测试 - 运行于 EditMode
    /// </summary>
    public class TestClassPool : TestBase
    {
        [Test]
        public void ClassPool_Get_ShouldCreateNewObjectWhenPoolEmpty()
        {
            var pool = new ClassPool<TestObject>(10);
            var obj = pool.Get();

            Assert.NotNull(obj);
            Assert.AreEqual(1, pool.TotalCount);
            Assert.AreEqual(0, pool.FreeCount);
        }

        [Test]
        public void ClassPool_Get_ShouldReuseObjectWhenPoolHasFree()
        {
            var pool = new ClassPool<TestObject>(10);
            var obj1 = pool.Get();
            pool.Release(obj1);
            var obj2 = pool.Get();

            Assert.AreSame(obj1, obj2);
            Assert.AreEqual(1, pool.TotalCount);
            Assert.AreEqual(0, pool.FreeCount);
        }

        [Test]
        public void ClassPool_Release_ShouldReturnObjectToPool()
        {
            var pool = new ClassPool<TestObject>(10);
            var obj = pool.Get();
            pool.Release(obj);

            Assert.AreEqual(1, pool.FreeCount);
            Assert.AreEqual(1, pool.TotalCount);
        }

        [Test]
        public void ClassPool_Release_Null_ShouldDoNothing()
        {
            var pool = new ClassPool<TestObject>(10);
            pool.Release(null);

            Assert.AreEqual(0, pool.FreeCount);
            Assert.AreEqual(0, pool.TotalCount);
        }

        [Test]
        public void ClassPool_Reset_ShouldCallOverrideReset()
        {
            var pool = new ClassPool<TestObject>(10);
            bool resetCalled = false;
            pool.OverrideReset = (obj) => { resetCalled = true; obj.Reset(); };

            var obj = pool.Get();
            pool.Release(obj);

            Assert.True(resetCalled);
            Assert.True(obj.IsReset);
        }

        [Test]
        public void ClassPool_Create_ShouldCallOverrideCreate()
        {
            var pool = new ClassPool<TestObject>(10);
            bool createCalled = false;
            pool.OverrideCreate = () => { createCalled = true; return new TestObject(); };

            var obj = pool.Get();

            Assert.True(createCalled);
            Assert.NotNull(obj);
        }

        [Test]
        public void ClassPool_Clear_ShouldDestroyAllFreeObjects()
        {
            var pool = new ClassPool<TestObject>(10);
            var obj1 = pool.Get();
            var obj2 = pool.Get();
            pool.Release(obj1);

            pool.Clear();

            Assert.AreEqual(0, pool.FreeCount);
            Assert.AreEqual(1, pool.TotalCount); // obj2 仍在外部
        }

        [Test]
        public void ClassPool_FixedCapacity_ShouldThrowWhenExceeded()
        {
            var pool = new ClassPool<TestObject>(2, true);
            pool.Get();
            pool.Get();

            Assert.Throws<InvalidOperationException>(() => pool.Get());
        }

        [Test]
        public void ClassPool_FixedCapacity_ShouldAllowReuseAfterRelease()
        {
            var pool = new ClassPool<TestObject>(2, true);
            var obj1 = pool.Get();
            var obj2 = pool.Get();
            pool.Release(obj1);

            var obj3 = pool.Get();

            Assert.AreSame(obj1, obj3);
            Assert.AreEqual(2, pool.TotalCount);
            Assert.AreEqual(0, pool.FreeCount);
        }
    }
}