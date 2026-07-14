using System;

/// <summary>
/// 属性读取器
/// </summary>
public readonly struct AttributeReader
{
    private readonly IReadOnlyAttributeCollection collection;
    internal AttributeReader(IReadOnlyAttributeCollection collection)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        this.collection = collection;
    }

    /// <summary>
    /// 读取属性值
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">属性值的类型</typeparam>
    /// <param name="key">键</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>属性值</returns>
    public TValue Read<TKey, TValue>(TKey key, TValue fallback = default)
    {
        var attr = collection.Get<TKey, TValue>(key);
        return attr.HasValue ? attr.Value : fallback;
    }

    /// <summary>
    /// 读取属性值包装器
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">属性值的类型</typeparam>
    /// <param name="key">键</param>
    /// <returns>属性值包装器</returns>
    public Attribute<TValue> ReadRaw<TKey, TValue>(TKey key)
    {
        return collection.Get<TKey, TValue>(key);
    }
}