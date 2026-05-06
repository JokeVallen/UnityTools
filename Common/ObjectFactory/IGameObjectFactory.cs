using System;
using UnityEngine;

/// <summary>
/// 游戏对象工厂接口
/// </summary>
/// <remarks>
/// <para>定义创建 <see cref="GameObject"/> 的标准行为，允许指定名称、初始化回调及预添加组件。</para>
/// </remarks>
public interface IGameObjectFactory : IObjectFactory
{
    /// <summary>
    /// 创建游戏对象
    /// </summary>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建的 <see cref="GameObject"/> 实例</returns>
    /// <remarks>
    /// <para>创建一个未命名的空 <see cref="GameObject"/>。</para>
    /// <para><paramref name="initialize"/> 为可选委托，创建完成后立即执行以完成初始设置。</para>
    /// </remarks>
    GameObject Create(Action<GameObject> initialize = null);

    /// <summary>
    /// 使用名称创建游戏对象
    /// </summary>
    /// <param name="name">游戏对象名称</param>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建的 <see cref="GameObject"/> 实例</returns>
    /// <remarks>
    /// <para>创建具有指定名称的空 <see cref="GameObject"/>。</para>
    /// </remarks>
    GameObject Create(string name, Action<GameObject> initialize = null);

    /// <summary>
    /// 使用名称和组件类型创建游戏对象
    /// </summary>
    /// <param name="name">游戏对象名称</param>
    /// <param name="initialize">初始化回调</param>
    /// <param name="components">要添加的组件类型数组</param>
    /// <returns>创建的 <see cref="GameObject"/> 实例</returns>
    /// <remarks>
    /// <para>创建指定名称的 <see cref="GameObject"/>，并立即添加 <paramref name="components"/> 中给出的所有组件。</para>
    /// <para>组件类型参数必须是有效的 <see cref="Component"/> 派生类，否则创建会失败。</para>
    /// </remarks>
    GameObject Create(string name, Action<GameObject> initialize = null, params Type[] components);
}