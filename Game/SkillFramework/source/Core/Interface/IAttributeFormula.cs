/// <summary>
/// 属性计算公式接口
/// </summary>
/// <typeparam name="TResult">结果类型</typeparam>
public interface IAttributeFormula<out TResult>
{
    /// <summary>
    /// 执行公式
    /// </summary>
    /// <param name="reader">属性读取器</param>
    /// <returns>计算结果</returns>
    TResult Execute(in AttributeReader reader);
}