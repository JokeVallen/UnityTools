using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// 测试默认游戏对象工厂的创建行为与错误处理。
/// </summary>
[TestFixture]
public class GameObjectFactoryTests
{
    private GameObjectFactory factory;

    [SetUp]
    public void SetUp()
    {
        factory = new GameObjectFactory();
    }

    [TearDown]
    public void TearDown()
    {
        // 清理场景中所有测试生成的对象
        foreach (var go in GameObject.FindObjectsOfType<GameObject>())
        {
            if (go.name.Contains("Test") || go.scene.IsValid())
            {
                if (Application.isPlaying)
                    GameObject.Destroy(go);
                else
                    GameObject.DestroyImmediate(go);
            }
        }
    }

    // ------------------------------
    // 基础创建
    // ------------------------------
    [Test]
    public void Create_Default_ReturnsNotNull()
    {
        var go = factory.Create();
        Assert.That(go, Is.Not.Null);
    }

    [Test]
    public void Create_WithName_ReturnsObjectWithCorrectName()
    {
        var go = factory.Create("TestName");
        Assert.That(go.name, Is.EqualTo("TestName"));
    }

    [Test]
    public void Create_WithComponents_AddsAllComponents()
    {
        var go = factory.Create("CompTest", null, typeof(Rigidbody), typeof(BoxCollider));
        Assert.That(go.GetComponent<Rigidbody>(), Is.Not.Null);
        Assert.That(go.GetComponent<BoxCollider>(), Is.Not.Null);
    }

    [Test]
    public void Create_WithInitializeCallback_ExecutesCallback()
    {
        bool called = false;
        var go = factory.Create(go => called = true);
        Assert.That(called, Is.True);
    }

    // ------------------------------
    // 参数验证
    // ------------------------------
    [Test]
    public void Create_InvalidComponentType_ReturnsNullAndLogsError()
    {
        // string 的别名是 String
        LogAssert.Expect(LogType.Error, "Invalid component type: String");
        var go = factory.Create("BadComponents", null, typeof(string));
        Assert.That(go, Is.Null);
    }

    [Test]
    public void Create_NullComponentInArray_ReturnsNullAndLogsError()
    {
        LogAssert.Expect(LogType.Error, "Invalid component type: null");
        var go = factory.Create("NullComp", null, new Type[] { null });
        Assert.That(go, Is.Null);
    }

    // ------------------------------
    // 错误处理与清理
    // ------------------------------
    [Test]
    public void Create_ThrowOnErrorFalse_InitializeThrows_ReturnsNullAndDestroysObject()
    {
        factory.ThrowOnError = false;
        LogAssert.Expect(LogType.Error, "Failed to initialize: Init failure");
        var go = factory.Create("DestroyTest", go =>
        {
            throw new Exception("Init failure");
        });

        Assert.That(go, Is.Null);
        var found = GameObject.Find("DestroyTest");
        Assert.That(found, Is.Null);
    }

    [Test]
    public void Create_ThrowOnErrorTrue_InitializeThrows_RethrowsException()
    {
        factory.ThrowOnError = true;
        Assert.Throws<Exception>(() =>
        {
            factory.Create("RethrowTest", go => throw new Exception("Boom"));
        });
    }

    [Test]
    public void Create_ThrowOnErrorFalse_LogsErrorOnFailure()
    {
        factory.ThrowOnError = false;
        LogAssert.Expect(LogType.Error, "Failed to initialize: Init error");
        factory.Create("LogTest", go => throw new Exception("Init error"));
    }

    // ------------------------------
    // 重载组合
    // ------------------------------
    [Test]
    public void Create_NameAndComponentsOnly_Works()
    {
        var go = factory.Create("NameOnly", null, typeof(AudioSource));
        Assert.That(go.GetComponent<AudioSource>(), Is.Not.Null);
    }

    [Test]
    public void Create_EmptyName_StillCreatesObject()
    {
        var go = factory.Create("");
        Assert.That(go, Is.Not.Null);
        // 默认名称是 "New Game Object" 或类似
        Assert.That(go.name, Is.Not.Empty);
    }
}