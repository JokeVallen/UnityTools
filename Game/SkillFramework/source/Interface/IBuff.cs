/// <summary>
/// Buff 接口
/// </summary>
public interface IBuff
{
    /// <summary>
    /// 应用 Buff
    /// </summary>
    /// <param name="context">Buff 上下文</param>
    /// <param name="extraContext">自定义上下文</param>
    void ApplyTo(BuffContext context, ITypedContext extraContext);

    /// <summary>
    /// 帧推动 Buff
    /// </summary>
    /// <param name="context">Buff 上下文</param>
    /// <param name="extraContext">自定义上下文</param>
    /// <param name="deltaTime">时间差</param>
    void Tick(BuffContext context, ITypedContext extraContext, float deltaTime);

    /// <summary>
    /// 移除 Buff
    /// </summary>
    /// <param name="context">Buff 上下文</param>
    /// <param name="extraContext">自定义上下文</param>
    void RemoveFrom(BuffContext context, ITypedContext extraContext);
}