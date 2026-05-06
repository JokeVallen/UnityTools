using System;
using System.Collections.Concurrent;
using UnityEngine;

/// <summary>
/// 对象工厂入口
/// </summary>
/// <remarks>
/// <para>全局的工厂注册与解析中心。可通过 <see cref="RegisterCreator{T}"/> 注入自定义工厂，再用 <see cref="GetFactory{T}"/> 或 <see cref="TryGetFactory{T}"/> 获取实例。</para>
/// <para>内置对 <see cref="IGameObjectFactory"/> 和 <see cref="IComponentFactory"/> 的默认实现，无需注册即可使用。</para>
/// <para>线程安全，支持多线程注册和获取。</para>
/// </remarks>
public static class ObjectFactory
{
    private static readonly ConcurrentDictionary<Type, Func<IObjectFactory>> creators = new ConcurrentDictionary<Type, Func<IObjectFactory>>();

    /// <summary>
    /// 注册工厂创建者
    /// </summary>
    /// <typeparam name="T">工厂接口类型</typeparam>
    /// <param name="creator">工厂创建委托</param>
    /// <remarks>
    /// <para>注册一个返回 <typeparamref name="T"/> 实例的委托，之后通过 <see cref="GetFactory{T}"/> 即可获取该实例。</para>
    /// <para>如果 <paramref name="creator"/> 为 <see langword="null"/>，会输出错误日志并忽略注册。</para>
    /// <para>重复注册同一接口会覆盖旧的创建者。</para>
    /// <para>示例：</para>
    /// <code>
    /// ObjectFactory.RegisterCreator&lt;IGameObjectFactory&gt;(() => new MyCustomGameObjectFactory());
    /// </code>
    /// </remarks>
    public static void RegisterCreator<T>(Func<T> creator) where T : class, IObjectFactory
    {
        if (creator == null)
        {
            Debug.LogError($"The parameter '{nameof(creator)}' cannot be null.");
            return;
        }
        creators[typeof(T)] = () => creator();
    }

    /// <summary>
    /// 获取工厂（泛型）
    /// </summary>
    /// <typeparam name="T">工厂接口类型</typeparam>
    /// <returns>工厂实例，若无注册且无默认实现则返回 <see langword="null"/></returns>
    /// <remarks>
    /// <para>先尝试从已注册的自定义工厂中解析，若未注册则回退到内置默认工厂（仅支持 <see cref="IGameObjectFactory"/> 和 <see cref="IComponentFactory"/>）。</para>
    /// <para>如果请求的类型没有任何工厂可用，返回 <see langword="null"/>。</para>
    /// <para>示例：</para>
    /// <code>
    /// IGameObjectFactory goFactory = ObjectFactory.GetFactory&lt;IGameObjectFactory&gt;();
    /// </code>
    /// </remarks>
    public static T GetFactory<T>() where T : class, IObjectFactory
    {
        var factory = Resolve<T>();
        if (factory == null) return GetDefaultFactory(typeof(T)) as T;
        return factory;
    }

    /// <summary>
    /// 获取工厂（非泛型）
    /// </summary>
    /// <param name="factoryType">工厂接口类型</param>
    /// <returns>工厂实例，若无则返回 <see langword="null"/></returns>
    /// <remarks>
    /// <para>用于运行时动态获取工厂，行为与泛型版本一致。</para>
    /// </remarks>
    public static IObjectFactory GetFactory(Type factoryType)
    {
        var factory = Resolve(factoryType);
        if (factory == null) return GetDefaultFactory(factoryType);
        return factory;
    }

    /// <summary>
    /// 尝试获取工厂（泛型）
    /// </summary>
    /// <typeparam name="T">工厂接口类型</typeparam>
    /// <param name="factory">获取到的工厂实例</param>
    /// <returns>成功获取到非空工厂则返回 <see langword="true"/>，否则 <see langword="false"/></returns>
    /// <remarks>
    /// <para>与 <see cref="GetFactory{T}"/> 逻辑相同，但通过 <see langword="out"/> 参数和布尔返回值明确表示是否成功。</para>
    /// <para>推荐在不确定工厂是否已注册的场景下使用。</para>
    /// <para>示例：</para>
    /// <code>
    /// if (ObjectFactory.TryGetFactory&lt;IGameObjectFactory&gt;(out var factory))
    /// {
    ///     factory.Create("Enemy");
    /// }
    /// </code>
    /// </remarks>
    public static bool TryGetFactory<T>(out T factory) where T : class, IObjectFactory
    {
        factory = Resolve<T>();
        if (factory == null) factory = GetDefaultFactory(typeof(T)) as T;
        return factory != null;
    }

    /// <summary>
    /// 尝试获取工厂（非泛型）
    /// </summary>
    /// <param name="factoryType">工厂接口类型</param>
    /// <param name="factory">获取到的工厂实例</param>
    /// <returns>成功获取到非空工厂则返回 <see langword="true"/>，否则 <see langword="false"/></returns>
    /// <remarks>
    /// <para>非泛型版本，适用于运行时动态获取。</para>
    /// </remarks>
    public static bool TryGetFactory(Type factoryType, out IObjectFactory factory)
    {
        factory = Resolve(factoryType);
        if (factory == null) factory = GetDefaultFactory(factoryType);
        return factory != null;
    }

    /// <summary>
    /// 清除所有注册
    /// </summary>
    /// <remarks>
    /// <para>移除所有通过 <see cref="RegisterCreator{T}"/> 注册的自定义工厂，主要供单元测试或编辑器重置状态使用。</para>
    /// <para>调用后，<see cref="GetFactory{T}"/> 将仅返回内置默认实现（若有）。</para>
    /// </remarks>
    public static void ClearCreators() { creators.Clear(); }

    private static T Resolve<T>() where T : class, IObjectFactory => Resolve(typeof(T)) as T;
    private static IObjectFactory Resolve(Type type)
    {
        if (type == null)
        {
            Debug.LogError($"The parameter '{nameof(type)}' cannot be null.");
            return null;
        }

        if (!typeof(IObjectFactory).IsAssignableFrom(type))
        {
            Debug.LogError($"The type '{type}' doesn't implement the interface '{typeof(IObjectFactory)}'.");
            return null;
        }

        if (!creators.TryGetValue(type, out var creator)) return null;
        try { return creator(); }
        catch (Exception ex) { Debug.LogException(ex); }
        return null;
    }

    private static IObjectFactory GetDefaultFactory(Type factoryType)
    {
        if (factoryType == null) return null;
        if (typeof(IGameObjectFactory).IsAssignableFrom(factoryType))
            return GameObjectFactoryHandle.instance;
        if (typeof(IComponentFactory).IsAssignableFrom(factoryType))
            return ComponentFactoryHandle.instance;
        return null;
    }

    private class GameObjectFactoryHandle
    {
        public static readonly GameObjectFactory instance = new GameObjectFactory();
    }

    private class ComponentFactoryHandle
    {
        public static readonly ComponentFactory instance = new ComponentFactory();
    }
}