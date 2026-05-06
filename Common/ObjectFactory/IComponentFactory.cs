using System;
using UnityEngine;

/// <summary>
/// 泛型组件工厂接口
/// </summary>
/// <typeparam name="T">要创建的组件类型，必须派生自 <see cref="Component"/></typeparam>
/// <remarks>
/// <para>定义针对特定组件类型的创建行为，允许在添加组件后执行额外的初始化逻辑。</para>
/// </remarks>
public interface IComponentFactory<T> : IObjectFactory<T>, IObjectFactory where T : Component
{
    /// <summary>
    /// 创建组件
    /// </summary>
    /// <param name="gameObject">游戏对象</param>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建并初始化成功的组件实例</returns>
    /// <remarks>
    /// <para>在 <paramref name="gameObject"/> 上添加 <typeparamref name="T"/> 组件，并可选地执行 <paramref name="initialize"/> 回调。</para>
    /// <para>具体错误处理策略由实现决定（可参考 <see cref="ComponentFactory{T}"/> 的默认行为）。</para>
    /// </remarks>
    T Create(GameObject gameObject, Action<T> initialize = null);
}

/// <summary>
/// 组件工厂接口
/// </summary>
/// <remarks>
/// <para>定义运行时通过 <see cref="Type"/> 动态创建组件的工厂行为。</para>
/// </remarks>
public interface IComponentFactory : IObjectFactory<Component>, IObjectFactory
{
    /// <summary>
    /// 创建组件
    /// </summary>
    /// <param name="gameObject">游戏对象</param>
    /// <param name="type">组件类型</param>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建并初始化成功的组件实例</returns>
    /// <remarks>
    /// <para>在 <paramref name="gameObject"/> 上添加指定 <paramref name="type"/> 的组件，并可选地执行 <paramref name="initialize"/> 回调。</para>
    /// <para><paramref name="type"/> 必须派生自 <see cref="Component"/>。</para>
    /// </remarks>
    Component Create(GameObject gameObject, Type type, Action<Component> initialize = null);
}