using System;
using System.Collections.Generic;

/// <summary>
/// 可被组件挂载的扩展能力接口
/// </summary>
public interface IComponentAttachable
{
    /// <summary>
    /// 添加组件
    /// </summary>
    /// <param name="component">组件</param>
    void AddComponent(IComponent component);

    /// <summary>
    /// 移除指定组件
    /// </summary>
    /// <param name="component">组件</param>
    /// <returns>移除成功返回 true，否则返回 false</returns>
    bool RemoveComponent(IComponent component);

    /// <summary>
    /// 移除第一个符合指定类型的组件
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <returns>移除成功返回 true，否则返回 false</returns>
    bool RemoveComponent<T>() where T : IComponent;

    /// <summary>
    /// 移除第一个符合指定类型的组件
    /// </summary>
    /// <param name="type">组件类型</param>
    /// <returns>移除成功返回 true，否则返回 false</returns>
    bool RemoveComponent(Type type);

    /// <summary>
    /// 获取第一个符合指定类型的组件
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <returns>组件包装器</returns>
    Optional<T> GetComponent<T>() where T : IComponent;

    /// <summary>
    /// 获取第一个符合指定类型的组件
    /// </summary>
    /// <param name="type">组件类型</param>
    /// <returns>组件包装器</returns>
    Optional<IComponent> GetComponent(Type type);

    /// <summary>
    /// 获取指定类型的组件集合
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <returns>组件集合</returns>
    IEnumerable<T> GetComponents<T>() where T : IComponent;

    /// <summary>
    /// 获取指定类型的组件集合
    /// </summary>
    /// <param name="type">组件类型</param>
    /// <returns>组件集合</returns>
    IEnumerable<IComponent> GetComponents(Type type);

    /// <summary>
    /// 获取组件集合
    /// </summary>
    /// <returns>组件集合</returns>
    IEnumerable<IComponent> GetComponents();
}