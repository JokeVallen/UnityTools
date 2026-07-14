using System;
using System.Collections.Generic;

/// <summary>
/// 属性集合的属性变更事件通知能力接口
/// </summary>
public interface INotifiableAttributeCollection
{
    /// <summary>
    /// 设置指定属性值的相等性比较器
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">属性值的类型</typeparam>
    /// <param name="equalityComparer">相等性比较器</param>
    void SetEqualityComparer<TKey, TValue>(IEqualityComparer<TValue> equalityComparer);

    /// <summary>
    /// 注册属性变更事件
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">属性值的类型</typeparam>
    /// <param name="callback">属性变更事件</param>
    void Register<TKey, TValue>(Action<TKey, Attribute<TValue>> callback);

    /// <summary>
    /// 注销属性变更事件
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">属性值的类型</typeparam>
    /// <param name="callback">属性变更事件</param>
    void Unregister<TKey, TValue>(Action<TKey, Attribute<TValue>> callback);

    /// <summary>
    /// 注销指定键值类型的所有属性变更事件
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">属性值的类型</typeparam>
    void UnregisterAll<TKey, TValue>();

    /// <summary>
    /// 注销所有属性变更事件
    /// </summary>
    void UnregisterAll();
}