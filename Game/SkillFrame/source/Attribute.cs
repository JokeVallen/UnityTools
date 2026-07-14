/// <summary>
/// 属性值包装器
/// </summary>
/// <typeparam name="T">属性值的类型</typeparam>
public readonly struct Attribute<T>
{
    /// <summary>
    /// 属性值
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// 是否有值
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// 缺省值
    /// </summary>
    public static readonly Attribute<T> None = new Attribute<T>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value">属性值</param>
    public Attribute(T value)
    {
        Value = value;
        HasValue = true;
    }

    public static implicit operator Attribute<T>(T value)
    {
        return new Attribute<T>(value);
    }

    public static explicit operator T(Attribute<T> attribute)
    {
        return attribute.HasValue ? attribute.Value : default;
    }
}