public static partial class Extensions
{
    /// <summary>
    /// 获取值或默认值
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">属性值的类型</typeparam>
    /// <param name="collection">属性集合的只读视图</param>
    /// <param name="key">键</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>属性值</returns>
    public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyAttributeCollection collection, TKey key, TValue defaultValue = default)
    {
        var attr = collection.Get<TKey, TValue>(key);
        return attr.HasValue ? attr.Value : defaultValue;
    }

    /// <summary>
    /// 开启计算模式
    /// </summary>
    /// <param name="collection">属性集合的只读视图</param>
    /// <returns>属性计算器</returns>
    public static AttributeEvaluator Evaluate(this IReadOnlyAttributeCollection collection)
    {
        return new AttributeEvaluator(collection);
    }
}