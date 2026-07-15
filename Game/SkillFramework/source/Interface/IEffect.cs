/// <summary>
/// Effect 接口
/// </summary>
public interface IEffect
{
    /// <summary>
    /// 执行 Effect
    /// </summary>
    /// <param name="context">Effe 上下文</param>
    /// <param name="extraContext">自定义上下文</param>
    void Execute(EffectContext context, ITypedContext extraContext);
}