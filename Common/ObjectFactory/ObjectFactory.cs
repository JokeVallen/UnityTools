using System;
using System.Collections.Concurrent;
using UnityEngine;

/// <summary>
/// 对象工厂
/// </summary>
/// <remarks>
/// <para>提供全局获取 <see cref="IGameObjectFactory"/>、<see cref="IComponentFactory"/> 和 <see cref="IComponentFactory{T}"/> 实例的统一入口。</para>
/// <para>通过 <see cref="RegisterCreator{TFactory}"/> 可以注册自定义工厂实现，从而在不修改业务代码的前提下替换默认创建行为。</para>
/// </remarks>
public static class ObjectFactory
{
    private static readonly ConcurrentDictionary<Type, Func<IObjectFactory>> creators = new ConcurrentDictionary<Type, Func<IObjectFactory>>();

    /// <summary>
    /// 注册工厂创建者
    /// </summary>
    /// <typeparam name="TFactory">工厂接口类型</typeparam>
    /// <param name="creator">创建者委托</param>
    /// <remarks>
    /// <para><paramref name="creator"/> 是一个返回具体工厂实例的委托，不能为 <see langword="null"/>。</para>
    /// <para>注册后，当调用 <see cref="GetGameObjectFactory"/>、<see cref="GetComponentFactory"/> 等方法时，会优先使用这里注册的自定义实现。</para>
    /// <para>如果对同一接口重复注册，会直接覆盖之前的注册。</para>
    /// <para>示例：</para>
    /// <code>
    /// // 注册自定义的游戏对象工厂
    /// ObjectFactory.RegisterCreator&lt;IGameObjectFactory&gt;(() => new MyPooledGameObjectFactory());
    /// </code>
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="creator"/> 为 <see langword="null"/> 时抛出。</exception>
    public static void RegisterCreator<TFactory>(Func<TFactory> creator) where TFactory : IObjectFactory
    {
        if (creator == null) throw new ArgumentNullException(nameof(creator));
        creators[typeof(TFactory)] = () => creator();
    }

    /// <summary>
    /// 获取游戏对象工厂
    /// </summary>
    /// <returns>当前可用的 <see cref="IGameObjectFactory"/> 实例</returns>
    /// <remarks>
    /// <para>优先返回通过 <see cref="RegisterCreator{TFactory}"/> 注册的自定义实现；若未注册则返回默认的 <see cref="GameObjectFactory"/> 单例。</para>
    /// </remarks>
    public static IGameObjectFactory GetGameObjectFactory()
    {
        IGameObjectFactory factory = Resolve<IGameObjectFactory>();
        return factory ?? GameObjectFactoryHandle.instance;
    }

    /// <summary>
    /// 获取泛型组件工厂
    /// </summary>
    /// <typeparam name="T">要创建的组件类型</typeparam>
    /// <returns>针对 <typeparamref name="T"/> 的 <see cref="IComponentFactory{T}"/> 实例</returns>
    /// <remarks>
    /// <para>优先返回注册的自定义实现，否则返回默认的 <see cref="ComponentFactory{T}"/> 单例。</para>
    /// </remarks>
    public static IComponentFactory<T> GetComponentFactory<T>() where T : Component
    {
        IComponentFactory<T> factory = Resolve<IComponentFactory<T>>();
        return factory ?? ComponentFactoryHandle<T>.instance;
    }

    /// <summary>
    /// 获取组件工厂
    /// </summary>
    /// <returns>当前可用的 <see cref="IComponentFactory"/> 实例</returns>
    /// <remarks>
    /// <para>优先返回注册的自定义实现，否则返回默认的 <see cref="ComponentFactory"/> 单例。</para>
    /// </remarks>
    public static IComponentFactory GetComponentFactory()
    {
        IComponentFactory factory = Resolve<IComponentFactory>();
        return factory ?? ComponentFactoryHandle.instance;
    }

    private static T Resolve<T>() where T : class, IObjectFactory
    {
        if (!creators.TryGetValue(typeof(T), out var creator)) return null;
        try { return creator() as T; }
        catch (Exception ex) { Debug.LogException(ex); }
        return null;
    }

    private class GameObjectFactoryHandle
    {
        public static readonly GameObjectFactory instance = new GameObjectFactory();
    }

    private class ComponentFactoryHandle<T> where T : Component
    {
        public static readonly ComponentFactory<T> instance = new ComponentFactory<T>();
    }

    private class ComponentFactoryHandle
    {
        public static readonly ComponentFactory instance = new ComponentFactory();
    }
}