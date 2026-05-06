using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// 测试默认组件工厂的创建、参数验证及异常清理机制。
/// </summary>
[TestFixture]
public class ComponentFactoryTests
{
    private ComponentFactory factory;
    private GameObject testGameObject;

    [SetUp]
    public void SetUp()
    {
        factory = new ComponentFactory();
        testGameObject = new GameObject("TestHolder");
    }

    [TearDown]
    public void TearDown()
    {
        if (testGameObject != null)
        {
            if (Application.isPlaying)
                GameObject.Destroy(testGameObject);
            else
                GameObject.DestroyImmediate(testGameObject);
        }
    }

    // -----------------------------------
    // 泛型创建 Create<T>
    // -----------------------------------
    [Test]
    public void CreateT_ValidType_ReturnsComponent()
    {
        var rb = factory.Create<Rigidbody>(testGameObject);
        Assert.That(rb, Is.Not.Null);
        Assert.That(testGameObject.GetComponent<Rigidbody>(), Is.SameAs(rb));
    }

    [Test]
    public void CreateT_WithInitialize_SetsValues()
    {
        float targetMass = 5.0f;
        var rb = factory.Create<Rigidbody>(testGameObject, r => r.mass = targetMass);
        Assert.That(rb.mass, Is.EqualTo(targetMass));
    }

    [Test]
    public void CreateT_NullGameObject_ReturnsNullAndLogsError()
    {
        LogAssert.Expect(LogType.Error, "The parameter 'gameObject' cannot be null.");
        var result = factory.Create<Rigidbody>(null);
        Assert.That(result, Is.Null);
    }

    // -----------------------------------
    // 非泛型创建 Create(Type)
    // -----------------------------------
    [Test]
    public void Create_ValidType_ReturnsComponent()
    {
        var comp = factory.Create(testGameObject, typeof(Rigidbody));
        Assert.That(comp, Is.InstanceOf<Rigidbody>());
    }

    [Test]
    public void Create_InvalidTypeNotComponent_ReturnsNullAndLogsError()
    {
        LogAssert.Expect(LogType.Error, $"The type '{typeof(string)}' doesn't inherit from '{typeof(Component)}'.");
        var comp = factory.Create(testGameObject, typeof(string));
        Assert.That(comp, Is.Null);
    }

    [Test]
    public void Create_NullType_ReturnsNullAndLogsError()
    {
        LogAssert.Expect(LogType.Error, "The parameter 'type' cannot be null.");
        var comp = factory.Create(testGameObject, null);
        Assert.That(comp, Is.Null);
    }

    [Test]
    public void Create_NullGameObject_ReturnsNullAndLogsError()
    {
        LogAssert.Expect(LogType.Error, "The parameter 'gameObject' cannot be null.");
        var comp = factory.Create(null, typeof(Rigidbody));
        Assert.That(comp, Is.Null);
    }

    // -----------------------------------
    // 初始化时的异常处理
    // -----------------------------------
    [Test]
    public void Create_ThrowOnErrorFalse_InitializeFails_ReturnsNullAndDestroysComponent()
    {
        factory.ThrowOnError = false;
        LogAssert.Expect(LogType.Error, "Failed to initialize: Init failure");
        var comp = factory.Create<Rigidbody>(testGameObject, r =>
        {
            throw new Exception("Init failure");
        });

        Assert.That(comp, Is.Null);
        Assert.That(testGameObject.GetComponent<Rigidbody>(), Is.Null);
    }

    [Test]
    public void Create_ThrowOnErrorTrue_InitializeFails_Rethrows()
    {
        factory.ThrowOnError = true;
        Assert.Throws<Exception>(() =>
        {
            factory.Create<Rigidbody>(testGameObject, r => throw new Exception("Boom"));
        });
    }

    [Test]
    public void Create_NonGeneric_ThrowOnErrorFalse_CleansUp()
    {
        factory.ThrowOnError = false;
        LogAssert.Expect(LogType.Error, "Failed to initialize: Error");
        var comp = factory.Create(testGameObject, typeof(Rigidbody), c =>
        {
            throw new Exception("Error");
        });

        Assert.That(comp, Is.Null);
        Assert.That(testGameObject.GetComponent<Rigidbody>(), Is.Null);
    }

    [Test]
    public void Create_ThrowOnErrorFalse_LogsError()
    {
        factory.ThrowOnError = false;
        LogAssert.Expect(LogType.Error, "Failed to initialize: Init error");
        factory.Create<Rigidbody>(testGameObject, r => throw new Exception("Init error"));
    }
}