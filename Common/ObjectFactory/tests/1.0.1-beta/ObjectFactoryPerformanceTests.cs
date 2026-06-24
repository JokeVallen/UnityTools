using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;

namespace EditModeTests.PerformanceTests
{
    [TestFixture]
    public class ObjectFactoryPerformanceTests
    {
        private GameObjectFactory _goFactory;
        private ComponentFactory _compFactory;

        [SetUp]
        public void SetUp()
        {
            ObjectFactory.ClearCreators();
            _goFactory = new GameObjectFactory(false);
            _compFactory = new ComponentFactory(false);
        }

        [TearDown]
        public void TearDown()
        {
            ObjectFactory.ClearCreators();
        }

        // ---------- 静态初始化方法（零闭包） ----------
        private static void InitGameObject(GameObject obj)
        {
            obj.transform.position = Vector3.one;
            obj.name = "Init";
        }

        private static void InitGameObjectWithArg(GameObject obj, int arg)
        {
            obj.name = $"Arg_{arg}";
        }

        private static void InitRigidbody(Rigidbody rb)
        {
            rb.mass = 5f;
            rb.useGravity = false;
        }

        private static void InitRigidbodyWithArg(Rigidbody rb, float mass)
        {
            rb.mass = mass;
        }

        private static void InitComponent(Component c)
        {
            if (c is Rigidbody rb) rb.mass = 3f;
        }

        // ==================== 1. 注册性能测试 ====================

        [Test, Performance]
        public void Register_Generic_Func()
        {
            Measure.Method(() =>
            {
                ObjectFactory.RegisterCreator<MockFactory>(() => new MockFactory());
            })
            .WarmupCount(1)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void Register_Generic_IFactoryCreator()
        {
            var creator = new MockFactoryCreator<MockFactory>(() => new MockFactory());
            Measure.Method(() =>
            {
                ObjectFactory.RegisterCreator<MockFactory>(creator);
            })
            .WarmupCount(1)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void Register_NonGeneric_Func()
        {
            Measure.Method(() =>
            {
                ObjectFactory.RegisterCreator(typeof(MockFactory), () => new MockFactory());
            })
            .WarmupCount(1)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void Register_NonGeneric_IFactoryCreator()
        {
            var creator = new MockFactoryCreator(typeof(MockFactory), () => new MockFactory());
            Measure.Method(() =>
            {
                ObjectFactory.RegisterCreator(creator);
            })
            .WarmupCount(1)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        // ==================== 2. 获取工厂性能测试 ====================

        [Test, Performance]
        public void GetFactory_Generic()
        {
            ObjectFactory.RegisterCreator<MockFactory>(() => new MockFactory());
            Measure.Method(() =>
            {
                var factory = ObjectFactory.GetFactory<MockFactory>();
            })
            .WarmupCount(1)
            .MeasurementCount(100)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void GetFactory_NonGeneric()
        {
            ObjectFactory.RegisterCreator(typeof(MockFactory), () => new MockFactory());
            Measure.Method(() =>
            {
                var factory = ObjectFactory.GetFactory(typeof(MockFactory));
            })
            .WarmupCount(1)
            .MeasurementCount(100)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void TryGetFactory_Generic()
        {
            ObjectFactory.RegisterCreator<MockFactory>(() => new MockFactory());
            Measure.Method(() =>
            {
                bool success = ObjectFactory.TryGetFactory<MockFactory>(out var factory);
            })
            .WarmupCount(1)
            .MeasurementCount(100)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void TryGetFactory_NonGeneric()
        {
            ObjectFactory.RegisterCreator(typeof(MockFactory), () => new MockFactory());
            Measure.Method(() =>
            {
                bool success = ObjectFactory.TryGetFactory(typeof(MockFactory), out var factory);
            })
            .WarmupCount(1)
            .MeasurementCount(100)
            .GC()
            .Run();
        }

        // ==================== 3. GameObjectFactory 创建性能测试 ====================

        [Test, Performance]
        public void GameObjectFactory_Create_NoInit()
        {
            GameObject go = null;
            Measure.Method(() =>
            {
                go = _goFactory.Create();
            })
            .WarmupCount(1)
            .MeasurementCount(10)
            .GC()
            .Run();
            if (go != null) UnityEngine.Object.DestroyImmediate(go);
        }

        [Test, Performance]
        public void GameObjectFactory_Create_WithInit()
        {
            GameObject go = null;
            Measure.Method(() =>
            {
                go = _goFactory.Create(InitGameObject);
            })
            .WarmupCount(1)
            .MeasurementCount(10)
            .GC()
            .Run();
            if (go != null) UnityEngine.Object.DestroyImmediate(go);
        }

        [Test, Performance]
        public void GameObjectFactory_Create_WithArgInit()
        {
            GameObject go = null;
            int arg = 42;
            Measure.Method(() =>
            {
                go = _goFactory.Create(arg, InitGameObjectWithArg);
            })
            .WarmupCount(1)
            .MeasurementCount(10)
            .GC()
            .Run();
            if (go != null) UnityEngine.Object.DestroyImmediate(go);
        }

        [Test, Performance]
        public void GameObjectFactory_Create_WithComponents()
        {
            GameObject go = null;
            var componentTypes = new Type[] { typeof(Rigidbody), typeof(BoxCollider) };
            Measure.Method(() =>
            {
                go = _goFactory.Create("WithComponents", null, componentTypes);
            })
            .WarmupCount(1)
            .MeasurementCount(10)
            .GC()
            .Run();
            if (go != null) UnityEngine.Object.DestroyImmediate(go);
        }

        // ==================== 4. ComponentFactory 创建性能测试（修改：每次迭代新建 GameObject） ====================

        [Test, Performance]
        public void ComponentFactory_Create_Generic_NoInit()
        {
            Measure.Method(() =>
            {
                var go = new GameObject("Temp");
                var comp = _compFactory.Create<Rigidbody>(go);
                Assert.IsNotNull(comp);
                UnityEngine.Object.DestroyImmediate(go);
            })
            .WarmupCount(1)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void ComponentFactory_Create_Generic_WithInit()
        {
            Measure.Method(() =>
            {
                var go = new GameObject("Temp");
                var comp = _compFactory.Create<Rigidbody>(go, InitRigidbody);
                Assert.IsNotNull(comp);
                UnityEngine.Object.DestroyImmediate(go);
            })
            .WarmupCount(1)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void ComponentFactory_Create_Generic_WithArgInit()
        {
            float mass = 2.5f;
            Measure.Method(() =>
            {
                var go = new GameObject("Temp");
                var comp = _compFactory.Create<Rigidbody, float>(go, mass, InitRigidbodyWithArg);
                Assert.IsNotNull(comp);
                UnityEngine.Object.DestroyImmediate(go);
            })
            .WarmupCount(1)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void ComponentFactory_Create_NonGeneric_NoInit()
        {
            Measure.Method(() =>
            {
                var go = new GameObject("Temp");
                var comp = _compFactory.Create(go, typeof(Rigidbody));
                Assert.IsNotNull(comp);
                UnityEngine.Object.DestroyImmediate(go);
            })
            .WarmupCount(1)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void ComponentFactory_Create_NonGeneric_WithInit()
        {
            Measure.Method(() =>
            {
                var go = new GameObject("Temp");
                var comp = _compFactory.Create(go, typeof(Rigidbody), InitComponent);
                Assert.IsNotNull(comp);
                UnityEngine.Object.DestroyImmediate(go);
            })
            .WarmupCount(1)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        // ==================== 5. 压力测试（不同数量级） ====================

        [Test, Performance]
        public void GameObjectFactory_Stress_Create_100()
        {
            var list = new List<GameObject>(100);
            Measure.Method(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    list.Add(_goFactory.Create());
                }
            })
            .WarmupCount(1)
            .MeasurementCount(5)
            .GC()
            .Run();

            foreach (var go in list)
                if (go != null) UnityEngine.Object.DestroyImmediate(go);
            list.Clear();
        }

        [Test, Performance]
        public void GameObjectFactory_Stress_Create_1000()
        {
            var list = new List<GameObject>(1000);
            Measure.Method(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    list.Add(_goFactory.Create());
                }
            })
            .WarmupCount(1)
            .MeasurementCount(3)  // 减少次数避免超时
            .GC()
            .Run();

            foreach (var go in list)
                if (go != null) UnityEngine.Object.DestroyImmediate(go);
            list.Clear();
        }

        [Test, Performance]
        public void GameObjectFactory_Stress_Create_WithInit_100()
        {
            var list = new List<GameObject>(100);
            Measure.Method(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    list.Add(_goFactory.Create(InitGameObject));
                }
            })
            .WarmupCount(1)
            .MeasurementCount(5)
            .GC()
            .Run();

            foreach (var go in list)
                if (go != null) UnityEngine.Object.DestroyImmediate(go);
            list.Clear();
        }

        [Test, Performance]
        public void GameObjectFactory_Stress_Create_WithArgInit_100()
        {
            var list = new List<GameObject>(100);
            int arg = 0;
            Measure.Method(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    list.Add(_goFactory.Create(arg, InitGameObjectWithArg));
                    arg++;
                }
            })
            .WarmupCount(1)
            .MeasurementCount(5)
            .GC()
            .Run();

            foreach (var go in list)
                if (go != null) UnityEngine.Object.DestroyImmediate(go);
            list.Clear();
        }

        [Test, Performance]
        public void ComponentFactory_Stress_Create_100()
        {
            var comps = new List<Component>(100);
            // 每次测量迭代使用同一个 GameObject，但只添加组件
            var target = new GameObject("StressTarget");
            Measure.Method(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    comps.Add(_compFactory.Create<Rigidbody>(target));
                }
            })
            .WarmupCount(1)
            .MeasurementCount(5)
            .GC()
            .Run();

            foreach (var c in comps)
                if (c != null) UnityEngine.Object.DestroyImmediate(c);
            comps.Clear();
            UnityEngine.Object.DestroyImmediate(target);
        }
    }

    // ---------- 辅助测试类 ----------
    public class MockFactory : IObjectFactory { }
    public class MockFactoryCreator : IFactoryCreator
    {
        public Type FactoryType { get; }
        private readonly Func<IObjectFactory> _factoryFunc;
        public MockFactoryCreator(Type type, Func<IObjectFactory> func) { FactoryType = type; _factoryFunc = func; }
        public IObjectFactory Create() => _factoryFunc();
    }
    public class MockFactoryCreator<T> : IFactoryCreator<T> where T : IObjectFactory
    {
        private readonly Func<T> _factoryFunc;
        public MockFactoryCreator(Func<T> func) => _factoryFunc = func;
        public T Create() => _factoryFunc();
    }
}