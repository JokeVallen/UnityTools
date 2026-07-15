/// <summary>
/// 技能执行器接口
/// </summary>
public interface ISkillExecutor
{
    /// <summary>
    /// 执行技能
    /// </summary>
    /// <param name="context">执行器上下文</param>
    /// <param name="extraContext">自定义上下文</param>
    void Execute(SkillExecutorContext context, ITypedContext extraContext);
}