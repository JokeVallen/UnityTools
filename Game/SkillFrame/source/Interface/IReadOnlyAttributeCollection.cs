using System.Collections.Generic;

/// <summary>
/// 属性集合的只读视图接口
/// </summary>
public interface IReadOnlyAttributeCollection
{
    /// <summary>
    /// 获取指定键所指示的属性值
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">属性值的类型</typeparam>
    /// <param name="key">键</param>
    /// <returns>属性值包装器</returns>
    Attribute<TValue> Get<TKey, TValue>(TKey key);

    /// <summary>
    /// 是否包含指定的键
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">属性值的类型</typeparam>
    /// <param name="key">键</param>
    /// <returns>包含则返回 true，否则返回 false</returns>
    bool ContainsKey<TKey, TValue>(TKey key);

    /// <summary>
    /// 获取键集合
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">属性值的类型</typeparam>
    /// <returns>键集合</returns>
    IEnumerable<TKey> GetKeys<TKey, TValue>();

    /// <summary>
    /// 获取属性值包装器集合
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">属性值的类型</typeparam>
    /// <returns>属性值包装器集合</returns>
    IEnumerable<Attribute<TValue>> GetValues<TKey, TValue>();
}