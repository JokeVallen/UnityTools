/// <summary>
/// 泛型对象工厂接口
/// </summary>
/// <typeparam name="T">工厂产物类型</typeparam>
/// <remarks>
/// <para>预留接口，当前未定义额外成员，未来扩展时可用于添加与特定产物类型相关的工厂行为。</para>
/// </remarks>
internal interface IObjectFactory<T> : IObjectFactory
{

}

/// <summary>
/// 对象工厂接口
/// </summary>
/// <remarks>
/// <para>所有对象工厂的基础接口，提供错误处理策略的配置。</para>
/// </remarks>
public interface IObjectFactory
{
    /// <summary>
    /// 是否抛出错误
    /// </summary>
    /// <remarks>
    /// <para>获取或设置一个值，指示在初始化回调发生异常时是否直接抛出该异常。</para>
    /// <para>若为 <see langword="false"/>，异常会被记录，并尽可能回滚已创建的对象；默认为 <see langword="false"/>。</para>
    /// </remarks>
    public bool ThrowOnError { get; set; }
}