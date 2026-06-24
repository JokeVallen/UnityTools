using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象工厂入口
/// </summary>
public static class ObjectFactory
{
    private static readonly ConcurrentDictionary<Type, FactoryCreator> creators = new ConcurrentDictionary<Type, FactoryCreator>();
    private static event Action OnClear;

    private readonly struct FactoryCreator : IFactoryCreator
    {
        public Type FactoryType => factoryType;
        private readonly Type factoryType;
        private readonly IFactoryCreator creator;
        private readonly Func<IObjectFactory> func;

        public static readonly FactoryCreator Null = new FactoryCreator();

        public FactoryCreator(IFactoryCreator creator)
        {
            factoryType = creator.FactoryType;
            this.creator = creator;
            func = null;
        }

        public FactoryCreator(Type factoryType, Func<IObjectFactory> func)
        {
            this.factoryType = factoryType;
            creator = null;
            this.func = func;
        }

        public IObjectFactory Create()
        {
            if (creator != null) return creator.Create();
            return func();
        }
    }

    private readonly struct FactoryCreator<T> : IFactoryCreator<T> where T : IObjectFactory
    {
        private readonly IFactoryCreator<T> creator;
        private readonly Func<T> func;

        public static readonly FactoryCreator<T> Null = new FactoryCreator<T>();

        public FactoryCreator(Func<T> func)
        {
            creator = null;
            this.func = func;
        }

        public FactoryCreator(IFactoryCreator<T> creator)
        {
            this.creator = creator;
            func = null;
        }

        public T Create()
        {
            if (creator != null) return creator.Create();
            return func();
        }
    }

    private static class Storage<T> where T : IObjectFactory
    {
        public static FactoryCreator<T> creator = FactoryCreator<T>.Null;
        static Storage() { OnClear += Clear; }
        private static void Clear() { creator = default; }
    }

    /// <summary>
    /// 注册工厂创建者
    /// </summary>
    /// <typeparam name="T">工厂类型</typeparam>
    /// <param name="callback">工厂创建委托</param>
    public static void RegisterCreator<T>(Func<T> callback) where T : class, IObjectFactory
    {
        if (callback == null)
        {
            Debug.LogError($"[ObjectFactory] The parameter '{nameof(callback)}' cannot be null.");
            return;
        }
        Storage<T>.creator = new FactoryCreator<T>(callback);
    }

    /// <summary>
    /// 注册工厂创建者
    /// </summary>
    /// <typeparam name="T">工厂类型</typeparam>
    /// <param name="creator">工厂创建器</param>
    public static void RegisterCreator<T>(IFactoryCreator<T> creator) where T : class, IObjectFactory
    {
        if (creator == null)
        {
            Debug.LogError($"[ObjectFactory] The parameter '{nameof(creator)}' cannot be null.");
            return;
        }
        Storage<T>.creator = new FactoryCreator<T>(creator);
    }

    /// <summary>
    /// 注册工厂创建者
    /// </summary>
    /// <param name="factoryType">工厂类型</param>
    /// <param name="callback">工厂创建委托</param>
    public static void RegisterCreator(Type factoryType, Func<IObjectFactory> callback)
    {
        if (factoryType == null || !typeof(IObjectFactory).IsAssignableFrom(factoryType))
        {
            Debug.LogError($"[ObjectFactory] The parameter '{nameof(factoryType)}' is invalid.");
            return;
        }

        if (callback == null)
        {
            Debug.LogError($"[ObjectFactory] The parameter '{nameof(callback)}' cannot be null.");
            return;
        }

        creators[factoryType] = new FactoryCreator(factoryType, callback);
    }

    /// <summary>
    /// 注册工厂创建者
    /// </summary>
    /// <param name="factoryType">工厂类型</param>
    /// <param name="creator">工厂创建器</param>
    public static void RegisterCreator(IFactoryCreator creator)
    {
        if (creator == null)
        {
            Debug.LogError($"[ObjectFactory] The parameter '{nameof(creator)}' cannot be null.");
            return;
        }

        if (creator.FactoryType == null || !typeof(IObjectFactory).IsAssignableFrom(creator.FactoryType))
        {
            Debug.LogError($"[ObjectFactory] The parameter '{nameof(creator)}' has no valid factory type.");
            return;
        }

        creators[creator.FactoryType] = new FactoryCreator(creator);
    }

    /// <summary>
    /// 获取工厂（泛型）
    /// </summary>
    /// <typeparam name="T">工厂接口类型</typeparam>
    /// <returns>工厂实例，若无注册且无默认实现则返回 <see langword="null"/></returns>
    public static T GetFactory<T>() where T : class, IObjectFactory
    {
        var factory = Resolve<T>();
        if (factory == null)
            throw new InvalidOperationException($"[ObjectFactory] The factory typed '{typeof(T)}' doesn't exist.");
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
        if (factoryType == null || !typeof(IObjectFactory).IsAssignableFrom(factoryType))
        {
            Debug.LogError($"[ObjectFactory] The parameter '{nameof(factoryType)}' is invalid.");
            return null;
        }

        var factory = Resolve(factoryType);
        if (factory == null)
            throw new InvalidOperationException($"[ObjectFactory] The factory typed '{factoryType}' doesn't exist.");
        return factory;
    }

    /// <summary>
    /// 尝试获取工厂（泛型）
    /// </summary>
    /// <typeparam name="T">工厂接口类型</typeparam>
    /// <param name="factory">获取到的工厂实例</param>
    /// <returns>成功获取到非空工厂则返回 <see langword="true"/>，否则 <see langword="false"/></returns>
    public static bool TryGetFactory<T>(out T factory) where T : class, IObjectFactory
    {
        factory = Resolve<T>();
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
        if (factoryType == null || !typeof(IObjectFactory).IsAssignableFrom(factoryType))
        {
            Debug.LogError($"[ObjectFactory] The parameter '{nameof(factoryType)}' is invalid.");
            factory = null;
            return false;
        }

        factory = Resolve(factoryType);
        return factory != null;
    }

    /// <summary>
    /// 清除所有注册
    /// </summary>
    /// <remarks>
    /// <para>移除所有通过 <see cref="RegisterCreator{T}"/> 注册的自定义工厂，主要供单元测试或编辑器重置状态使用。</para>
    /// <para>调用后，<see cref="GetFactory{T}"/> 将仅返回内置默认实现（若有）。</para>
    /// </remarks>
    public static void ClearCreators()
    {
        creators.Clear();
        if (OnClear != null) OnClear();
    }

    private static T Resolve<T>() where T : class, IObjectFactory
    {
        ref var creator = ref Storage<T>.creator;
        if (EqualityComparer<FactoryCreator<T>>.Default.Equals(creator, FactoryCreator<T>.Null)) return null;
        try { return creator.Create(); }
        catch (Exception ex) { Debug.LogException(ex); }
        return null;
    }

    private static IObjectFactory Resolve(Type type)
    {
        if (type == null)
        {
            Debug.LogError($"[ObjectFactory] The parameter '{nameof(type)}' cannot be null.");
            return null;
        }

        if (!typeof(IObjectFactory).IsAssignableFrom(type))
        {
            Debug.LogError($"[ObjectFactory] The type '{type}' is not derived from '{typeof(IObjectFactory)}'.");
            return null;
        }

        if (!creators.TryGetValue(type, out var creator)) return null;
        try { return creator.Create(); }
        catch (Exception ex) { Debug.LogException(ex); }
        return null;
    }
}