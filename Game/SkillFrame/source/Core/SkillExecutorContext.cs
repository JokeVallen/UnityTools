using System.Collections.Generic;

/// <summary>
/// 技能执行上下文
/// </summary>
public readonly struct SkillExecutorContext
{
    /// <summary>
    /// 技能释放者
    /// </summary>
    public IEntity Caster { get; }

    /// <summary>
    /// 技能目标对象集
    /// </summary>
    public IEnumerable<IEntity> Targets { get; }

    /// <summary>
    /// 技能信息查询接口
    /// </summary>
    public ISkillInfoSearcher SkillInfoSearcher { get; }

    private SkillExecutorContext(IEntity caster, IEnumerable<IEntity> targets, ISkillInfoSearcher skillInfoSearcher)
    {
        Caster = caster;
        Targets = targets;
        SkillInfoSearcher = skillInfoSearcher;
    }

    /// <summary>
    /// 创建技能执行上下文实例
    /// </summary>
    /// <param name="caster">技能释放者</param>
    /// <param name="targets">技能目标对象集</param>
    /// <param name="skillInfoSearcher">技能信息查询接口</param>
    /// <returns>技能执行上下文实例</returns>
    public static SkillExecutorContext Create(IEntity caster, IEnumerable<IEntity> targets, ISkillInfoSearcher skillInfoSearcher)
    {
        return new SkillExecutorContext(caster, targets, skillInfoSearcher);
    }
}