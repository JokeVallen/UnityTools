/// <summary>
/// 对象工厂接口
/// </summary>
/// <remarks>
/// <para>所有对象工厂的基础抽象，提供初始化异常处理策略的配置入口。</para>
/// </remarks>
public interface IObjectFactory
{
    /// <summary>
    /// 是否抛出错误
    /// </summary>
    /// <remarks>
    /// <para>获取或设置一个值，指示在初始化回调抛出异常时是否直接向上抛出。</para>
    /// <para>默认值为 <see langword="false"/>，此时异常会被捕获并记录，同时已创建的资源会被安全销毁；若为 <see langword="true"/>，异常会立刻重新抛出。</para>
    /// </remarks>
    public bool ThrowOnError { get; set; }
}