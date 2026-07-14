/// <summary>
/// Buff 上下文
/// </summary>
public readonly struct BuffContext
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

    private BuffContext(IEntity caster, Optional<IEntity> target, ISkillInfoSearcher skillInfoSearcher)
    {
        Caster = caster;
        Target = target;
        SkillInfoSearcher = skillInfoSearcher;
    }

    /// <summary>
    /// 创建 Buff 上下文
    /// </summary>
    /// <param name="caster">技能释放者</param>
    /// <param name="target">目标</param>
    /// <param name="skillInfoSearcher">技能信息查询接口</param>
    /// <returns>Buff 上下文</returns>
    public static BuffContext Create(IEntity caster, Optional<IEntity> target, ISkillInfoSearcher skillInfoSearcher)
    {
        return new BuffContext(caster, target, skillInfoSearcher);
    }
}