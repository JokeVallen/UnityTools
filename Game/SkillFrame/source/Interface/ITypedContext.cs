/// <summary>
/// 强类型上下文接口
/// </summary>
/// <remarks>
/// <para>提供类型安全的键值存储能力</para>
/// </remarks>
public interface ITypedContext
{
    /// <summary>
    /// 设置值
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">值的类型</typeparam>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    void Set<TKey, TValue>(TKey key, TValue value);

    /// <summary>
    /// 获取值
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">值的类型</typeparam>
    /// <param name="key">键</param>
    /// <returns>包含值的 <see cref="Optional{T}"/> 包装器，如果不存在则返回 <see cref="Optional{T}.None"/> </returns>
    Optional<TValue> Get<TKey, TValue>(TKey key);

    /// <summary>
    /// 移除指定类型的值
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">值的类型</typeparam>
    /// <param name="key">键</param>
    /// <returns>若移除成功则返回 true，否则返回 false。</returns>
    bool Remove<TKey, TValue>(TKey key);

    /// <summary>
    /// 是否包含指定的键
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">值的类型</typeparam>
    /// <param name="key">键</param>
    /// <returns>若包含则返回 true，否则返回 false。</returns>
    bool ContainsKey<TKey, TValue>(TKey key);

    /// <summary>
    /// 清空所有值
    /// </summary>
    void Clear();
}
