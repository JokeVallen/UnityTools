/// <summary>
/// 可堆叠 Buff 标记接口
/// </summary>
public interface IStackableBuff : IBuff
{
    /// <summary>
    /// 堆叠 Buff
    /// </summary>
    /// <param name="context">Buff 上下文</param>
    /// <param name="extraContext">自定义上下文</param>
    void Stack(BuffContext context, ITypedContext extraContext);
}