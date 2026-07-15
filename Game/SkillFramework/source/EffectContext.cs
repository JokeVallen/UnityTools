/// <summary>
/// Effect 上下文
/// </summary>
public readonly struct EffectContext
{
    /// <summary>
    /// 技能释放者
    /// </summary>
    public IEntity Caster { get; }

    /// <summary>
    /// 目标
    /// </summary>
    public Optional<IEntity> Target { get; }

    /// <summary>
    /// 技能信息查询接口
    /// </summary>
    public ISkillInfoSearcher SkillInfoSearcher { get; }

    private EffectContext(IEntity caster, Optional<IEntity> target, ISkillInfoSearcher skillInfoSearcher)
    {
        Caster = caster;
        Target = target;
        SkillInfoSearcher = skillInfoSearcher;
    }

    /// <summary>
    /// 创建 Effect 上下文
    /// </summary>
    /// <param name="caster">技能释放者</param>
    /// <param name="target">目标</param>
    /// <param name="skillInfoSearcher">技能信息查询接口</param>
    /// <returns>Effect 上下文</returns>
    public static EffectContext Create(IEntity caster, Optional<IEntity> target, ISkillInfoSearcher skillInfoSearcher)
    {
        return new EffectContext(caster, target, skillInfoSearcher);
    }
}