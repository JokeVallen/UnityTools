using System;
using UnityEngine;

/// <summary>
/// 游戏对象工厂接口
/// </summary>
/// <remarks>
/// <para>定义创建 <see cref="GameObject"/> 的标准行为，支持指定名称、初始化回调和预添加组件。</para>
/// </remarks>
public interface IGameObjectFactory : IObjectFactory<GameObject>, IObjectFactory
{
    /// <summary>
    /// 创建游戏对象
    /// </summary>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建的 <see cref="GameObject"/></returns>
    /// <remarks>
    /// <para>创建一个未命名的空 <see cref="GameObject"/>，可选地在创建后执行 <paramref name="initialize"/> 回调。</para>
    /// </remarks>
    GameObject Create(Action<GameObject> initialize = null);

    /// <summary>
    /// 使用名称创建游戏对象
    /// </summary>
    /// <param name="name">游戏对象名称</param>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建的 <see cref="GameObject"/></returns>
    /// <remarks>
    /// <para>创建具有给定名称的空 <see cref="GameObject"/>。</para>
    /// </remarks>
    GameObject Create(string name, Action<GameObject> initialize = null);

    /// <summary>
    /// 使用名称和组件类型创建游戏对象
    /// </summary>
    /// <param name="name">游戏对象名称</param>
    /// <param name="initialize">初始化回调</param>
    /// <param name="components">要添加的组件类型数组</param>
    /// <returns>创建的 <see cref="GameObject"/></returns>
    /// <remarks>
    /// <para>创建具有给定名称的 <see cref="GameObject"/>，并自动添加 <paramref name="components"/> 中指定的所有组件。</para>
    /// </remarks>
    GameObject Create(string name, Action<GameObject> initialize = null, params Type[] components);
}