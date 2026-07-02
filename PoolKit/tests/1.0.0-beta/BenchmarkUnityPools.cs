// 文件: Tests/BenchmarkUnityPools.cs
using NUnit.Framework;
using PoolKit.Tests.Benchmarks;
using UnityEngine;
using Unity.PerformanceTesting;

namespace PoolKit.Unity.Tests.Benchmarks
{
    /// <summary>
    /// Unity 对象池基准测试 - 运行于 PlayMode
    /// </summary>
    public class BenchmarkUnityPools : BenchmarkBase
    {
        private GameObject _prefab;
        private GameObjectPool _gameObjectPool;
        private ComponentPool<TestComponent> _componentPool;
        private const int IterationsPerMeasurement = 1000;

        private class TestComponent : MonoBehaviour
        {
            public int Value { get; set; }
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _prefab = new GameObject("TestPrefab");
            _prefab.AddComponent<TestComponent>();
            _gameObjectPool = new GameObjectPool(100);
            _componentPool = new ComponentPool<TestComponent>(100);
        }

        [TearDown]
        public override void Teardown()
        {
            base.Teardown();
            if (_prefab != null)
                Object.DestroyImmediate(_prefab);
        }

        #region GameObjectPool

        [Test, Performance]
        public void GameObjectPool_GetAndReturn()
        {
            Measure.Method(() =>
            {
                var go = _gameObjectPool.Get();
                _gameObjectPool.Release(go);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void GameObjectPool_GetAndReturn_WithParent()
        {
            var container = new GameObject("Container");
            var settings = new UnityObjectPoolSettings<GameObject>
            {
                container = container,
                capacity = 100
            };
            var pool = new GameObjectPool(settings);

            Measure.Method(() =>
            {
                var go = pool.Get();
                pool.Release(go);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .SampleGroup(new SampleGroup("GameObjectPool.WithParent", SampleUnit.Microsecond))
            .Run();

            Object.DestroyImmediate(container);
        }

        [Test, Performance]
        public void GameObjectPool_NewVsPooled()
        {
            Measure.Method(() =>
            {
                var go = new GameObject("Test");
                go.SetActive(false);
                Object.Destroy(go);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .SampleGroup(new SampleGroup("New GameObject", SampleUnit.Microsecond))
            .Run();

            Measure.Method(() =>
            {
                var go = _gameObjectPool.Get();
                _gameObjectPool.Release(go);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .SampleGroup(new SampleGroup("Pooled GameObject", SampleUnit.Microsecond))
            .Run();
        }

        #endregion

        #region ComponentPool

        [Test, Performance]
        public void ComponentPool_GetAndReturn()
        {
            Measure.Method(() =>
            {
                var comp = _componentPool.Get();
                _componentPool.Release(comp);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void ComponentPool_GetAndReturn_WithData()
        {
            Measure.Method(() =>
            {
                var comp = _componentPool.Get();
                comp.Value = 42;
                _componentPool.Release(comp);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void ComponentPool_AddComponentVsPooled()
        {
            var container = new GameObject("Container");

            Measure.Method(() =>
            {
                var comp = container.AddComponent<TestComponent>();
                comp.Value = 1;
                Object.Destroy(comp);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .SampleGroup(new SampleGroup("AddComponent", SampleUnit.Microsecond))
            .Run();

            Measure.Method(() =>
            {
                var comp = _componentPool.Get();
                comp.Value = 1;
                _componentPool.Release(comp);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .IterationsPerMeasurement(IterationsPerMeasurement)
            .GC()
            .SampleGroup(new SampleGroup("Pooled Component", SampleUnit.Microsecond))
            .Run();

            Object.DestroyImmediate(container);
        }

        #endregion

        #region 压力测试

        [Test, Performance]
        public void GameObjectPool_Stress_100Objects()
        {
            Measure.Method(() =>
            {
                var objects = new GameObject[100];
                for (int i = 0; i < 100; i++)
                    objects[i] = _gameObjectPool.Get();
                for (int i = 0; i < 100; i++)
                    _gameObjectPool.Release(objects[i]);
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .IterationsPerMeasurement(20)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void ComponentPool_Stress_100Components()
        {
            Measure.Method(() =>
            {
                var components = new TestComponent[100];
                for (int i = 0; i < 100; i++)
                    components[i] = _componentPool.Get();
                for (int i = 0; i < 100; i++)
                    _componentPool.Release(components[i]);
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .IterationsPerMeasurement(20)
            .GC()
            .Run();
        }

        #endregion
    }
}