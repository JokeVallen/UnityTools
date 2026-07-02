// 文件: Tests/TestUnityObjectPool.cs
using NUnit.Framework;
using UnityEngine;

namespace PoolKit.Unity.Tests
{
    /// <summary>
    /// UnityObjectPool 单元测试 - 运行于 PlayMode
    /// </summary>
    public class TestUnityObjectPool
    {
        private class TestComponent : MonoBehaviour
        {
            public int Value { get; set; }
        }

        [Test]
        public void GameObjectPool_Get_ShouldCreateNewGameObject()
        {
            var pool = new GameObjectPool(10);
            var go = pool.Get();

            Assert.NotNull(go);
            Assert.AreEqual(1, pool.TotalCount);
            Assert.AreEqual(0, pool.FreeCount);
        }

        [Test]
        public void GameObjectPool_Get_ShouldActivateObject()
        {
            var pool = new GameObjectPool(10);
            var go = pool.Get();

            Assert.True(go.activeSelf);
        }

        [Test]
        public void GameObjectPool_Release_ShouldDeactivateObject()
        {
            var pool = new GameObjectPool(10);
            var go = pool.Get();
            pool.Release(go);

            Assert.False(go.activeSelf);
            Assert.AreEqual(1, pool.FreeCount);
        }

        [Test]
        public void GameObjectPool_Get_ShouldReuseDeactivatedObject()
        {
            var pool = new GameObjectPool(10);
            var go1 = pool.Get();
            pool.Release(go1);
            var go2 = pool.Get();

            Assert.AreSame(go1, go2);
            Assert.True(go2.activeSelf);
        }

        [Test]
        public void GameObjectPool_Clear_ShouldDestroyAllFreeObjects()
        {
            var pool = new GameObjectPool(10);
            var go1 = pool.Get();
            var go2 = pool.Get();
            pool.Release(go1);

            pool.Clear();

            Assert.AreEqual(0, pool.FreeCount);
            // go1 已被销毁，go2 仍在外部
        }

        [Test]
        public void GameObjectPool_WithSettings_ShouldUseContainer()
        {
            var container = new GameObject("TestContainer");
            var settings = new UnityObjectPoolSettings<GameObject>
            {
                container = container,
                capacity = 5,
                activeWhenGet = false
            };

            var pool = new GameObjectPool(settings);
            var go = pool.Get();

            Assert.AreEqual(container.transform, go.transform.parent);
            Assert.False(go.activeSelf);
        }

        [Test]
        public void ComponentPool_Get_ShouldEnableBehaviour()
        {
            var pool = new ComponentPool<TestComponent>(10);
            var comp = pool.Get();

            Assert.True(comp.enabled);
        }

        [Test]
        public void ComponentPool_Release_ShouldDisableBehaviour()
        {
            var pool = new ComponentPool<TestComponent>(10);
            var comp = pool.Get();
            pool.Release(comp);

            Assert.False(comp.enabled);
        }
    }
}