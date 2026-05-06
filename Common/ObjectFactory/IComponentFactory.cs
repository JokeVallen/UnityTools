using System;
using UnityEngine;

/// <summary>
/// 组件工厂接口
/// </summary>
/// <remarks>
/// <para>定义向现有 <see cref="GameObject"/> 添加组件的标准行为，同时支持泛型和非泛型创建方式。</para>
/// </remarks>
public interface IComponentFactory : IObjectFactory
{
    /// <summary>
    /// 创建组件（泛型）
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <param name="gameObject">目标游戏对象</param>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建并初始化成功的组件实例</returns>
    /// <remarks>
    /// <para>在 <paramref name="gameObject"/> 上添加 <typeparamref name="T"/> 组件，并可通过 <paramref name="initialize"/> 进行初始化。</para>
    /// <para>若 <paramref name="initialize"/> 为 <see langword="null"/> 则跳过初始化；否则回调接收强类型组件实例。</para>
    /// <para>示例：</para>
    /// <code>
    /// var factory = ObjectFactory.GetFactory&lt;IComponentFactory&gt;();
    /// var rb = factory.Create&lt;Rigidbody&gt;(gameObject, r => r.mass = 2f);
    /// </code>
    /// </remarks>
    T Create<T>(GameObject gameObject, Action<T> initialize = null) where T : Component;

    /// <summary>
    /// 创建组件（非泛型）
    /// </summary>
    /// <param name="gameObject">目标游戏对象</param>
    /// <param name="type">组件类型</param>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建并初始化成功的组件实例</returns>
    /// <remarks>
    /// <para>在运行时通过 <see cref="Type"/> 动态添加组件，适用于无法在编译期确定具体类型的场景。</para>
    /// <para><paramref name="type"/> 必须派生自 <see cref="Component"/>，且不能为 <see langword="null"/>。</para>
    /// <para><paramref name="initialize"/> 回调接收 <see cref="Component"/> 类型，使用时需手动类型转换。</para>
    /// <para>示例：</para>
    /// <code>
    /// var factory = ObjectFactory.GetFactory&lt;IComponentFactory&gt;();
    /// Component c = factory.Create(gameObject, typeof(Rigidbody), com => ((Rigidbody)com).mass = 5f);
    /// </code>
    /// </remarks>
    Component Create(GameObject gameObject, Type type, Action<Component> initialize = null);
}