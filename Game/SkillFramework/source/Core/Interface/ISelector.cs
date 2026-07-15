using System.Collections.Generic;

/// <summary>
/// 目标对象集选择器接口
/// </summary>
public interface ISelector
{
    /// <summary>
    /// 选择目标对象集
    /// </summary>
    /// <param name="caster">技能释放者</param>
    /// <param name="skillInfoSearcher">技能信息查询接口</param>
    /// <returns>目标对象集</returns>
    IEnumerable<IEntity> Select(IEntity caster, ISkillInfoSearcher skillInfoSearcher);
}