/// <summary>
/// 属性集合的读写接口
/// </summary>
public interface IAttributeCollection : IReadOnlyAttributeCollection
{
    /// <summary>
    /// 设置指定键所指示的属性值
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">属性值的类型</typeparam>
    /// <param name="key">键</param>
    /// <param name="value">属性值</param>
    void Set<TKey, TValue>(TKey key, TValue value);

    /// <summary>
    /// 移除指定键所指示的属性键值对
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">属性值的类型</typeparam>
    /// <param name="key">键</param>
    /// <returns>移除成功则返回 true，否则返回 false</returns>
    bool Remove<TKey, TValue>(TKey key);

    /// <summary>
    /// 移除指定键值类型的所有属性键值对
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">属性值的类型</typeparam>
    /// <returns>移除的属性键值对数量</returns>
    int RemoveAll<TKey, TValue>();

    /// <summary>
    /// 清空属性集合
    /// </summary>
    void Clear();
}