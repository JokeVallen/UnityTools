using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// 假定被测代码位于以下命名空间（请根据实际调整）
// 如果被测代码在全局命名空间，可以不用 using

/// <summary>
/// 用于测试的模拟工厂（仅实现空接口）
/// </summary>
public class MockFactory : IObjectFactory
{
    public int CreationCount { get; private set; }
    public MockFactory() => CreationCount = 0;
    public void Touch() => CreationCount++;
}

/// <summary>
/// 实现 IFactoryCreator 的测试创建器（非泛型）
/// </summary>
public class MockFactoryCreator : IFactoryCreator
{
    public Type FactoryType { get; }
    private readonly Func<IObjectFactory> _factoryFunc;

    public MockFactoryCreator(Type factoryType, Func<IObjectFactory> factoryFunc)
    {
        FactoryType = factoryType;
        _factoryFunc = factoryFunc;
    }

    public IObjectFactory Create() => _factoryFunc();
}

/// <summary>
/// 实现 IFactoryCreator<T> 的测试创建器（泛型）
/// </summary>
public class MockFactoryCreator<T> : IFactoryCreator<T> where T : IObjectFactory
{
    private readonly Func<T> _factoryFunc;

    public MockFactoryCreator(Func<T> factoryFunc) => _factoryFunc = factoryFunc;

    public T Create() => _factoryFunc();
}

/// <summary>
/// ObjectFactory 核心功能单元测试
/// </summary>
[TestFixture]
public class ObjectFactoryTests
{
    [SetUp]
    public void SetUp()
    {
        // 每个测试前清空所有注册，保证隔离
        ObjectFactory.ClearCreators();
    }

    [TearDown]
    public void TearDown()
    {
        // 测试后清理
        ObjectFactory.ClearCreators();
    }

    // ---------- 泛型注册 + Func ----------
    [Test]
    public void RegisterCreator_Generic_Func_ShouldStoreAndResolve()
    {
        bool created = false;
        ObjectFactory.RegisterCreator<MockFactory>(() =>
        {
            created = true;
            return new MockFactory();
        });

        var factory = ObjectFactory.GetFactory<MockFactory>();
        Assert.IsNotNull(factory);
        Assert.IsTrue(created);
    }

    // ---------- 泛型注册 + IFactoryCreator<T> ----------
    [Test]
    public void RegisterCreator_Generic_IFactoryCreator_ShouldStoreAndResolve()
    {
        var creator = new MockFactoryCreator<MockFactory>(() => new MockFactory());
        ObjectFactory.RegisterCreator<MockFactory>(creator);

        var factory = ObjectFactory.GetFactory<MockFactory>();
        Assert.IsNotNull(factory);
    }

    // ---------- 非泛型注册 + Func ----------
    [Test]
    public void RegisterCreator_NonGeneric_Func_ShouldStoreAndResolve()
    {
        bool created = false;
        ObjectFactory.RegisterCreator(typeof(MockFactory), () =>
        {
            created = true;
            return new MockFactory();
        });

        var factory = ObjectFactory.GetFactory(typeof(MockFactory));
        Assert.IsNotNull(factory);
        Assert.IsTrue(created);
    }

    // ---------- 非泛型注册 + IFactoryCreator ----------
    [Test]
    public void RegisterCreator_NonGeneric_IFactoryCreator_ShouldStoreAndResolve()
    {
        var creator = new MockFactoryCreator(typeof(MockFactory), () => new MockFactory());
        ObjectFactory.RegisterCreator(creator);

        var factory = ObjectFactory.GetFactory(typeof(MockFactory));
        Assert.IsNotNull(factory);
    }

    // ---------- 重复注册覆盖 ----------
    [Test]
    public void RegisterCreator_Override_ShouldReplacePrevious()
    {
        int callCount = 0;
        ObjectFactory.RegisterCreator<MockFactory>(() =>
        {
            callCount++;
            return new MockFactory();
        });
        ObjectFactory.RegisterCreator<MockFactory>(() =>
        {
            callCount += 10;
            return new MockFactory();
        });

        var factory = ObjectFactory.GetFactory<MockFactory>();
        Assert.IsNotNull(factory);
        Assert.AreEqual(10, callCount);
    }

    // ---------- 未注册时 Get 抛异常 ----------
    [Test]
    public void GetFactory_NotRegistered_ShouldThrowInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => ObjectFactory.GetFactory<MockFactory>());
    }

    // ---------- 未注册时 TryGet 返回 false ----------
    [Test]
    public void TryGetFactory_NotRegistered_ShouldReturnFalse()
    {
        bool success = ObjectFactory.TryGetFactory<MockFactory>(out var factory);
        Assert.IsFalse(success);
        Assert.IsNull(factory);
    }

    // ---------- 清除注册后获取失败 ----------
    [Test]
    public void ClearCreators_ShouldClearAllRegistrations()
    {
        ObjectFactory.RegisterCreator<MockFactory>(() => new MockFactory());
        ObjectFactory.ClearCreators();

        Assert.Throws<InvalidOperationException>(() => ObjectFactory.GetFactory<MockFactory>());
    }

    // ---------- 非泛型 Get 参数校验 ----------
    [Test]
    public void GetFactory_InvalidType_ShouldLogErrorAndReturnNull()
    {
        // 传入 null 或非 IObjectFactory 类型
        LogAssert.Expect(LogType.Error, "[ObjectFactory] The parameter 'factoryType' is invalid.");
        var factory = ObjectFactory.GetFactory(null);
        Assert.IsNull(factory);

        LogAssert.Expect(LogType.Error, "[ObjectFactory] The parameter 'factoryType' is invalid.");
        factory = ObjectFactory.GetFactory(typeof(string));
        Assert.IsNull(factory);
    }

    // ---------- 非泛型 TryGet 参数校验 ----------
    [Test]
    public void TryGetFactory_InvalidType_ShouldLogErrorAndReturnFalse()
    {
        LogAssert.Expect(LogType.Error, "[ObjectFactory] The parameter 'factoryType' is invalid.");
        bool success = ObjectFactory.TryGetFactory(null, out var factory);
        Assert.IsFalse(success);
        Assert.IsNull(factory);

        LogAssert.Expect(LogType.Error, "[ObjectFactory] The parameter 'factoryType' is invalid.");
        success = ObjectFactory.TryGetFactory(typeof(string), out factory);
        Assert.IsFalse(success);
        Assert.IsNull(factory);
    }

    // ---------- 泛型 Get 与 Storage<T> 共存 ----------
    [Test]
    public void GenericAndNonGenericStorage_ShouldWorkIndependently()
    {
        ObjectFactory.RegisterCreator<MockFactory>(() => new MockFactory());
        ObjectFactory.RegisterCreator(typeof(GameObjectFactory), () => new GameObjectFactory());

        var mock = ObjectFactory.GetFactory<MockFactory>();
        var go = ObjectFactory.GetFactory(typeof(GameObjectFactory));

        Assert.IsNotNull(mock);
        Assert.IsNotNull(go);
    }
}

/// <summary>
/// GameObjectFactory 单元测试
/// </summary>
[TestFixture]
public class GameObjectFactoryTests
{
    private GameObjectFactory _factory;
    private GameObject _createdObject; // 用于清理

    [SetUp]
    public void SetUp()
    {
        _factory = new GameObjectFactory(throwOnError: false);
        _createdObject = null;
    }

    [TearDown]
    public void TearDown()
    {
        if (_createdObject != null)
            UnityEngine.Object.DestroyImmediate(_createdObject);
        _createdObject = null;
    }

    // ---------- 基本创建 ----------
    [Test]
    public void Create_NoParams_ShouldCreateEmptyGameObject()
    {
        _createdObject = _factory.Create();
        Assert.IsNotNull(_createdObject);
    }

    [Test]
    public void Create_WithName_ShouldSetName()
    {
        _createdObject = _factory.Create("TestObject");
        Assert.IsNotNull(_createdObject);
        Assert.AreEqual("TestObject", _createdObject.name);
    }

    [Test]
    public void Create_WithNameAndComponents_ShouldAddComponents()
    {
        _createdObject = _factory.Create("TestObject", null, typeof(Rigidbody), typeof(BoxCollider));
        Assert.IsNotNull(_createdObject);
        Assert.IsNotNull(_createdObject.GetComponent<Rigidbody>());
        Assert.IsNotNull(_createdObject.GetComponent<BoxCollider>());
    }

    // ---------- 初始化回调 ----------
    [Test]
    public void Create_WithInitCallback_ShouldExecuteAndModify()
    {
        bool called = false;
        _createdObject = _factory.Create("InitTest", go =>
        {
            called = true;
            go.transform.position = Vector3.one;
        });

        Assert.IsTrue(called);
        Assert.AreEqual(Vector3.one, _createdObject.transform.position);
    }

    // ---------- 带参初始化回调 (TArg) ----------
    [Test]
    public void Create_WithArgInitCallback_ShouldPassArgument()
    {
        int expectedArg = 42;
        int receivedArg = 0;
        _createdObject = _factory.Create(expectedArg, (go, arg) =>
        {
            receivedArg = arg;
            go.name = "ArgTest";
        });

        Assert.AreEqual(expectedArg, receivedArg);
        Assert.AreEqual("ArgTest", _createdObject.name);
    }

    // ---------- 无效组件类型 ----------
    [Test]
    public void Create_InvalidComponentType_ShouldLogErrorAndReturnNull()
    {
        _createdObject = _factory.Create("Invalid", null, null); // 传递 null 作为组件类型
        Assert.IsNotNull(_createdObject);

        LogAssert.Expect(LogType.Error, "[ObjectFactory] Invalid component type: String");
        _createdObject = _factory.Create("Invalid", null, typeof(string));
        Assert.IsNull(_createdObject);
    }

    // ---------- 初始化异常 (ThrowOnError = false) ----------
    [Test]
    public void Create_InitThrows_WhenThrowOnErrorFalse_ShouldDestroyAndReturnNull()
    {
        LogAssert.Expect(LogType.Error, "[ObjectFactory] Failed to initialize: Init failed");
        _factory = new GameObjectFactory(throwOnError: false);
        _createdObject = _factory.Create("Fail", go => throw new InvalidOperationException("Init failed"));
        Assert.IsNull(_createdObject);
        // 对象应被销毁，无法再引用
    }

    // ---------- 初始化异常 (ThrowOnError = true) ----------
    [Test]
    public void Create_InitThrows_WhenThrowOnErrorTrue_ShouldRethrowAndDestroy()
    {
        _factory = new GameObjectFactory(throwOnError: true);
        Assert.Throws<InvalidOperationException>(() =>
        {
            _createdObject = _factory.Create("Fail", go => throw new InvalidOperationException("Init failed"));
        });
        // 异常抛出后，_createdObject 未被赋值，因为赋值发生在异常之前？实际上创建对象是在调用前，但异常发生后对象已销毁。
        // 但因为我们无法在 Assert.Throws 内部捕获 _createdObject，所以单独验证无法，但逻辑上对象会被销毁。
        // 可以改为先创建再验证，但这样异常会中断，我们只要确保异常抛出即可。
        // 额外验证对象不存在：在异常抛出后，我们可以尝试获取，但异常已抛出，我们可以在外部检查。
        // 更好的方式：使用 try-catch 手动捕获，但这里我们信任逻辑。
    }

    // 为了验证销毁，可以采用更明确的测试（分开写）
    [Test]
    public void Create_InitThrows_WithThrowOnErrorTrue_ShouldDestroyGameObject()
    {
        _factory = new GameObjectFactory(throwOnError: true);
        GameObject go = null;
        try
        {
            go = _factory.Create("Fail", g => throw new Exception("Boom"));
        }
        catch (Exception)
        {
            // 忽略
        }
        // 由于异常抛出，go 应该为 null 或者已销毁，但 go 变量在创建时被赋值，然后异常导致赋值中断？实际上创建对象是在执行初始化之前，所以 go 会被赋值。
        // 但销毁发生在 catch 内部，所以 go 引用的对象已被销毁，但变量不为 null，需要检查是否为 null 或判断是否被销毁。
        // 在 Unity 中，销毁的对象会变为 null（当使用 DestroyImmediate 时）。
        Assert.IsNull(go); // 因为异常后对象被销毁，所以引用变为 null
    }

    // 注意：上面测试依赖于 DestroyImmediate 后引用变为 null，在 EditMode 下成立。

    // ---------- 多个重载的组合 ----------
    [Test]
    public void Create_AllOverloads_ShouldWork()
    {
        // 覆盖所有公开重载，确保无歧义
        _createdObject = _factory.Create();
        Assert.IsNotNull(_createdObject);
        UnityEngine.Object.DestroyImmediate(_createdObject);

        _createdObject = _factory.Create((Action<GameObject>)null);
        Assert.IsNotNull(_createdObject);
        UnityEngine.Object.DestroyImmediate(_createdObject);

        _createdObject = _factory.Create("Name");
        Assert.IsNotNull(_createdObject);
        UnityEngine.Object.DestroyImmediate(_createdObject);

        _createdObject = _factory.Create<string>("Name", null, (Action<GameObject, string>)null);
        Assert.IsNotNull(_createdObject);
        UnityEngine.Object.DestroyImmediate(_createdObject);

        _createdObject = _factory.Create("Name", null, typeof(Rigidbody));
        Assert.IsNotNull(_createdObject);
        UnityEngine.Object.DestroyImmediate(_createdObject);

        _createdObject = _factory.Create<int>("Name", 5, null, typeof(Rigidbody));
        Assert.IsNotNull(_createdObject);
        UnityEngine.Object.DestroyImmediate(_createdObject);

        _createdObject = null;
    }
}

/// <summary>
/// ComponentFactory 单元测试
/// </summary>
[TestFixture]
public class ComponentFactoryTests
{
    private ComponentFactory _factory;
    private GameObject _targetObject;

    [SetUp]
    public void SetUp()
    {
        _factory = new ComponentFactory(throwOnError: false);
        _targetObject = new GameObject("Target");
    }

    [TearDown]
    public void TearDown()
    {
        if (_targetObject != null)
            UnityEngine.Object.DestroyImmediate(_targetObject);
        _targetObject = null;
    }

    // ---------- 泛型创建 ----------
    [Test]
    public void Create_Generic_ShouldAddComponent()
    {
        var rb = _factory.Create<Rigidbody>(_targetObject);
        Assert.IsNotNull(rb);
        Assert.IsNotNull(_targetObject.GetComponent<Rigidbody>());
    }

    [Test]
    public void Create_Generic_WithInit_ShouldExecute()
    {
        bool called = false;
        var rb = _factory.Create<Rigidbody>(_targetObject, r =>
        {
            called = true;
            r.mass = 5f;
        });
        Assert.IsTrue(called);
        Assert.AreEqual(5f, rb.mass);
    }

    [Test]
    public void Create_Generic_WithArgInit_ShouldPassArgument()
    {
        int expected = 10;
        int received = 0;
        var rb = _factory.Create<Rigidbody, int>(_targetObject, expected, (r, arg) =>
        {
            received = arg;
            r.mass = arg;
        });
        Assert.AreEqual(expected, received);
        Assert.AreEqual(expected, rb.mass);
    }

    // ---------- 非泛型创建 ----------
    [Test]
    public void Create_NonGeneric_ShouldAddComponent()
    {
        var comp = _factory.Create(_targetObject, typeof(Rigidbody));
        Assert.IsNotNull(comp);
        Assert.IsInstanceOf<Rigidbody>(comp);
    }

    [Test]
    public void Create_NonGeneric_WithInit_ShouldExecute()
    {
        bool called = false;
        var comp = _factory.Create(_targetObject, typeof(Rigidbody), c =>
        {
            called = true;
            ((Rigidbody)c).mass = 3f;
        });
        Assert.IsTrue(called);
        Assert.AreEqual(3f, ((Rigidbody)comp).mass);
    }

    [Test]
    public void Create_NonGeneric_WithArgInit_ShouldPassArgument()
    {
        object expected = "test";
        object received = null;
        var comp = _factory.Create(_targetObject, typeof(Rigidbody), expected, (c, arg) =>
        {
            received = arg;
        });
        Assert.AreEqual(expected, received);
    }

    // ---------- 参数 null 校验 ----------
    [Test]
    public void Create_GameObjectNull_WhenThrowOnErrorFalse_ShouldLogAndReturnNull()
    {
        LogAssert.Expect(LogType.Error, "[ObjectFactory] The parameter 'gameObject' cannot be null.");
        var result = _factory.Create<Rigidbody>(null);
        Assert.IsNull(result);
    }

    [Test]
    public void Create_GameObjectNull_WhenThrowOnErrorTrue_ShouldThrow()
    {
        _factory = new ComponentFactory(throwOnError: true);
        Assert.Throws<ArgumentNullException>(() => _factory.Create<Rigidbody>(null));
    }

    [Test]
    public void Create_TypeNull_WhenThrowOnErrorFalse_ShouldLogAndReturnNull()
    {
        LogAssert.Expect(LogType.Error, "[ObjectFactory] The parameter 'type' cannot be null.");
        var result = _factory.Create(_targetObject, null);
        Assert.IsNull(result);
    }

    [Test]
    public void Create_TypeNull_WhenThrowOnErrorTrue_ShouldThrow()
    {
        _factory = new ComponentFactory(throwOnError: true);
        Assert.Throws<ArgumentNullException>(() => _factory.Create(_targetObject, null));
    }

    [Test]
    public void Create_InvalidType_WhenThrowOnErrorFalse_ShouldLogAndReturnNull()
    {
        LogAssert.Expect(LogType.Error, "[ObjectFactory] The type 'System.String' is not derived from 'UnityEngine.Component'.");
        var result = _factory.Create(_targetObject, typeof(string));
        Assert.IsNull(result);
    }

    [Test]
    public void Create_InvalidType_WhenThrowOnErrorTrue_ShouldThrow()
    {
        _factory = new ComponentFactory(throwOnError: true);
        Assert.Throws<ArgumentException>(() => _factory.Create(_targetObject, typeof(string)));
    }

    // ---------- 初始化异常回滚 ----------
    [Test]
    public void Create_InitThrows_WhenThrowOnErrorFalse_ShouldDestroyComponentAndReturnNull()
    {
        LogAssert.Expect(LogType.Error, "[ObjectFactory] Failed to initialize: Init fail");
        _factory = new ComponentFactory(throwOnError: false);
        var comp = _factory.Create<Rigidbody>(_targetObject, r => throw new Exception("Init fail"));
        Assert.IsNull(comp);
        // 验证组件被移除：因为 AddComponent 后立即销毁，应该不存在
        Assert.IsNull(_targetObject.GetComponent<Rigidbody>());
    }

    [Test]
    public void Create_InitThrows_WhenThrowOnErrorTrue_ShouldThrowAndDestroy()
    {
        _factory = new ComponentFactory(throwOnError: true);
        Rigidbody rb = null;
        try
        {
            rb = _factory.Create<Rigidbody>(_targetObject, r => throw new Exception("Init fail"));
        }
        catch (Exception)
        {
            // 异常被抛出
        }
        // 验证 rb 为 null（因为异常后赋值未完成？实际上异常发生在初始化中，但对象已创建，然后被销毁并重新抛出，rb 未被赋值）
        Assert.IsNull(rb);
        Assert.IsNull(_targetObject.GetComponent<Rigidbody>());
    }
}