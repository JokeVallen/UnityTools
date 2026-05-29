using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// 核心工厂注册与解析功能的测试。
/// </summary>
[TestFixture]
public class ObjectFactoryTests
{
    [SetUp]
    public void SetUp()
    {
        // 保证每次测试前清空所有注册，避免相互干扰
        ObjectFactory.ClearCreators();
    }

    [TearDown]
    public void TearDown()
    {
        ObjectFactory.ClearCreators();
    }

    // --------------------------------------------------
    // RegisterCreator 测试
    // --------------------------------------------------
    [Test]
    public void RegisterCreator_NullCreator_ShouldLogErrorAndNotThrow()
    {
        // Arrange
        LogAssert.Expect(LogType.Error, "The parameter 'creator' cannot be null.");

        // Act & Assert (不抛异常)
        Assert.DoesNotThrow(() => ObjectFactory.RegisterCreator<IGameObjectFactory>(null));
    }

    [Test]
    public void RegisterCreator_ValidCreator_ShouldRegisterSuccessfully()
    {
        // Arrange
        var mockFactory = new MockGameObjectFactory();
        ObjectFactory.RegisterCreator<IGameObjectFactory>(() => mockFactory);

        // Act
        var result = ObjectFactory.GetFactory<IGameObjectFactory>();

        // Assert
        Assert.That(result, Is.SameAs(mockFactory));
    }

    [Test]
    public void RegisterCreator_OverwriteExistingRegistration_ShouldUseLatest()
    {
        // Arrange
        var first = new MockGameObjectFactory();
        var second = new MockGameObjectFactory();
        ObjectFactory.RegisterCreator<IGameObjectFactory>(() => first);
        ObjectFactory.RegisterCreator<IGameObjectFactory>(() => second);

        // Act
        var result = ObjectFactory.GetFactory<IGameObjectFactory>();

        // Assert
        Assert.That(result, Is.SameAs(second));
    }

    // --------------------------------------------------
    // GetFactory<T> 测试
    // --------------------------------------------------
    [Test]
    public void GetFactory_NotRegistered_ReturnsDefaultGameObjectFactory()
    {
        // Act
        var factory = ObjectFactory.GetFactory<IGameObjectFactory>();

        // Assert
        Assert.That(factory, Is.Not.Null);
        Assert.That(factory, Is.TypeOf<GameObjectFactory>());
    }

    [Test]
    public void GetFactory_NotRegistered_ReturnsDefaultComponentFactory()
    {
        // Act
        var factory = ObjectFactory.GetFactory<IComponentFactory>();

        // Assert
        Assert.That(factory, Is.Not.Null);
        Assert.That(factory, Is.TypeOf<ComponentFactory>());
    }

    [Test]
    public void GetFactory_CustomInterfaceWithoutDefault_ReturnsNull()
    {
        // Act
        var factory = ObjectFactory.GetFactory<ICustomFactory>();

        // Assert
        Assert.That(factory, Is.Null);
    }

    [Test]
    public void GetFactory_TypeParameter_ReturnsSameAsGeneric()
    {
        // Act
        var byType = ObjectFactory.GetFactory(typeof(IGameObjectFactory));
        var byGeneric = ObjectFactory.GetFactory<IGameObjectFactory>();

        // Assert
        Assert.That(byType, Is.SameAs(byGeneric));
    }

    // --------------------------------------------------
    // TryGetFactory 测试
    // --------------------------------------------------
    [Test]
    public void TryGetFactory_Registered_ReturnsTrueAndFactory()
    {
        // Arrange
        var mock = new MockGameObjectFactory();
        ObjectFactory.RegisterCreator<IGameObjectFactory>(() => mock);

        // Act
        bool success = ObjectFactory.TryGetFactory<IGameObjectFactory>(out var factory);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(factory, Is.SameAs(mock));
    }

    [Test]
    public void TryGetFactory_NoRegistrationOrDefault_ReturnsFalse()
    {
        // Act
        bool success = ObjectFactory.TryGetFactory<ICustomFactory>(out var factory);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(factory, Is.Null);
    }

    [Test]
    public void TryGetFactory_DefaultAvailable_ReturnsTrue()
    {
        // Act
        bool success = ObjectFactory.TryGetFactory<IGameObjectFactory>(out var factory);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(factory, Is.Not.Null);
    }

    [Test]
    public void TryGetFactory_NonGeneric_WorksCorrectly()
    {
        // Arrange
        ObjectFactory.RegisterCreator<IGameObjectFactory>(() => new MockGameObjectFactory());

        // Act
        bool success = ObjectFactory.TryGetFactory(typeof(IGameObjectFactory), out var factory);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(factory, Is.InstanceOf<IGameObjectFactory>());
    }

    // --------------------------------------------------
    // ClearCreators 测试
    // --------------------------------------------------
    [Test]
    public void ClearCreators_AfterRegister_ShouldFallBackToDefault()
    {
        // Arrange
        ObjectFactory.RegisterCreator<IGameObjectFactory>(() => new MockGameObjectFactory());
        ObjectFactory.ClearCreators();

        // Act
        var factory = ObjectFactory.GetFactory<IGameObjectFactory>();

        // Assert
        Assert.That(factory, Is.TypeOf<GameObjectFactory>());
    }

    // --------------------------------------------------
    // 辅助接口和类
    // --------------------------------------------------
    internal interface ICustomFactory : IObjectFactory { }

    private class MockGameObjectFactory : IGameObjectFactory
    {
        public bool ThrowOnError { get; set; }
        public GameObject Create(Action<GameObject> initialize = null) => new GameObject();
        public GameObject Create(string name, Action<GameObject> initialize = null) => new GameObject(name);
        public GameObject Create(string name, Action<GameObject> initialize = null, params Type[] components) => new GameObject(name, components);
    }
}