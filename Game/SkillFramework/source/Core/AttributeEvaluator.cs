using System;

/// <summary>
/// 属性计算器
/// </summary>
public readonly struct AttributeEvaluator
{
    private readonly IReadOnlyAttributeCollection collection;
    internal AttributeEvaluator(IReadOnlyAttributeCollection collection)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        this.collection = collection;
    }

    /// <summary>
    /// 计算属性
    /// </summary>
    /// <typeparam name="TFormula">公式类型</typeparam>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <param name="formula">计算公式</param>
    /// <returns>计算结果</returns>
    public TResult Compute<TFormula, TResult>(in TFormula formula) where TFormula : struct, IAttributeFormula<TResult>
    {
        return formula.Execute(new AttributeReader(collection));
    }
}