using System;

/// <summary>
/// 扩展方法
/// </summary>
public static partial class Extensions
{
    #region int 算术运算

    /// <summary>
    /// 加法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<int> Add(this in Attribute<int> attr, int value, int fallback = 0)
    {
        int baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<int>(baseValue + value);
    }

    /// <summary>
    /// 加法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<int> Add(this in Attribute<int> attr, in Attribute<int> value, int fallback = 0)
    {
        int left = attr.HasValue ? attr.Value : fallback;
        int right = value.HasValue ? value.Value : fallback;
        return new Attribute<int>(left + right);
    }

    /// <summary>
    /// 减法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<int> Subtract(this in Attribute<int> attr, int value, int fallback = 0)
    {
        int baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<int>(baseValue - value);
    }

    /// <summary>
    /// 减法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<int> Subtract(this in Attribute<int> attr, in Attribute<int> value, int fallback = 0)
    {
        int left = attr.HasValue ? attr.Value : fallback;
        int right = value.HasValue ? value.Value : fallback;
        return new Attribute<int>(left - right);
    }

    /// <summary>
    /// 乘法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<int> Multiply(this in Attribute<int> attr, int value, int fallback = 1)
    {
        int baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<int>(baseValue * value);
    }

    /// <summary>
    /// 乘法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<int> Multiply(this in Attribute<int> attr, in Attribute<int> value, int fallback = 1)
    {
        int left = attr.HasValue ? attr.Value : fallback;
        int right = value.HasValue ? value.Value : fallback;
        return new Attribute<int>(left * right);
    }

    /// <summary>
    /// 除法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<int> Divide(this in Attribute<int> attr, int value, int fallback = 0)
    {
        int baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<int>(baseValue / value);
    }

    /// <summary>
    /// 除法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<int> Divide(this in Attribute<int> attr, in Attribute<int> value, int fallback = 0)
    {
        int left = attr.HasValue ? attr.Value : fallback;
        int right = value.HasValue ? value.Value : 1;
        return new Attribute<int>(left / right);
    }

    /// <summary>
    /// 取负值
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <returns>计算结果</returns>
    public static Attribute<int> Negate(this in Attribute<int> attr)
    {
        if (!attr.HasValue) return attr;
        return new Attribute<int>(-attr.Value);
    }

    #endregion

    #region float 算术运算

    /// <summary>
    /// 加法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<float> Add(this in Attribute<float> attr, float value, float fallback = 0f)
    {
        float baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<float>(baseValue + value);
    }

    /// <summary>
    /// 加法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<float> Add(this in Attribute<float> attr, in Attribute<float> value, float fallback = 0f)
    {
        float left = attr.HasValue ? attr.Value : fallback;
        float right = value.HasValue ? value.Value : fallback;
        return new Attribute<float>(left + right);
    }

    /// <summary>
    /// 加法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<float> Add(this in Attribute<float> attr, in Attribute<int> value, float fallback = 0f)
    {
        float left = attr.HasValue ? attr.Value : fallback;
        float right = value.HasValue ? value.Value : fallback;
        return new Attribute<float>(left + right);
    }

    /// <summary>
    /// 减法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<float> Subtract(this in Attribute<float> attr, float value, float fallback = 0f)
    {
        float baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<float>(baseValue - value);
    }

    /// <summary>
    /// 减法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<float> Subtract(this in Attribute<float> attr, in Attribute<float> value, float fallback = 0f)
    {
        float left = attr.HasValue ? attr.Value : fallback;
        float right = value.HasValue ? value.Value : fallback;
        return new Attribute<float>(left - right);
    }

    /// <summary>
    /// 减法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<float> Subtract(this in Attribute<float> attr, in Attribute<int> value, float fallback = 0f)
    {
        float left = attr.HasValue ? attr.Value : fallback;
        float right = value.HasValue ? value.Value : fallback;
        return new Attribute<float>(left - right);
    }

    /// <summary>
    /// 乘法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<float> Multiply(this in Attribute<float> attr, float value, float fallback = 1f)
    {
        float baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<float>(baseValue * value);
    }

    /// <summary>
    /// 乘法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<float> Multiply(this in Attribute<float> attr, in Attribute<float> value, float fallback = 1f)
    {
        float left = attr.HasValue ? attr.Value : fallback;
        float right = value.HasValue ? value.Value : fallback;
        return new Attribute<float>(left * right);
    }

    /// <summary>
    /// 乘法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<float> Multiply(this in Attribute<float> attr, in Attribute<int> value, float fallback = 1f)
    {
        float left = attr.HasValue ? attr.Value : fallback;
        float right = value.HasValue ? value.Value : fallback;
        return new Attribute<float>(left * right);
    }

    /// <summary>
    /// 除法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<float> Divide(this in Attribute<float> attr, float value, float fallback = 0f)
    {
        float baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<float>(baseValue / value);
    }

    /// <summary>
    /// 除法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<float> Divide(this in Attribute<float> attr, in Attribute<float> value, float fallback = 0f)
    {
        float left = attr.HasValue ? attr.Value : fallback;
        float right = value.HasValue ? value.Value : 1f;
        return new Attribute<float>(left / right);
    }

    /// <summary>
    /// 除法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<float> Divide(this in Attribute<float> attr, in Attribute<int> value, float fallback = 0f)
    {
        float left = attr.HasValue ? attr.Value : fallback;
        float right = value.HasValue ? value.Value : 1f;
        return new Attribute<float>(left / right);
    }

    /// <summary>
    /// 取负值
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <returns>计算结果</returns>
    public static Attribute<float> Negate(this in Attribute<float> attr)
    {
        if (!attr.HasValue) return attr;
        return new Attribute<float>(-attr.Value);
    }

    #endregion

    #region double 算术运算

    /// <summary>
    /// 加法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Add(this in Attribute<double> attr, double value, double fallback = 0d)
    {
        double baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<double>(baseValue + value);
    }

    /// <summary>
    /// 加法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Add(this in Attribute<double> attr, in Attribute<double> value, double fallback = 0d)
    {
        double left = attr.HasValue ? attr.Value : fallback;
        double right = value.HasValue ? value.Value : fallback;
        return new Attribute<double>(left + right);
    }

    /// <summary>
    /// 加法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Add(this in Attribute<double> attr, in Attribute<float> value, double fallback = 0d)
    {
        double left = attr.HasValue ? attr.Value : fallback;
        double right = value.HasValue ? value.Value : fallback;
        return new Attribute<double>(left + right);
    }

    /// <summary>
    /// 加法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Add(this in Attribute<double> attr, in Attribute<int> value, double fallback = 0d)
    {
        double left = attr.HasValue ? attr.Value : fallback;
        double right = value.HasValue ? value.Value : fallback;
        return new Attribute<double>(left + right);
    }

    /// <summary>
    /// 加法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Add(this in Attribute<double> attr, float value, double fallback = 0d)
    {
        double baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<double>(baseValue + value);
    }

    /// <summary>
    /// 加法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Add(this in Attribute<double> attr, int value, double fallback = 0d)
    {
        double baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<double>(baseValue + value);
    }

    /// <summary>
    /// 减法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Subtract(this in Attribute<double> attr, double value, double fallback = 0d)
    {
        double baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<double>(baseValue - value);
    }

    /// <summary>
    /// 减法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Subtract(this in Attribute<double> attr, in Attribute<double> value, double fallback = 0d)
    {
        double left = attr.HasValue ? attr.Value : fallback;
        double right = value.HasValue ? value.Value : fallback;
        return new Attribute<double>(left - right);
    }

    /// <summary>
    /// 减法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Subtract(this in Attribute<double> attr, in Attribute<float> value, double fallback = 0d)
    {
        double left = attr.HasValue ? attr.Value : fallback;
        double right = value.HasValue ? value.Value : fallback;
        return new Attribute<double>(left - right);
    }

    /// <summary>
    /// 减法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Subtract(this in Attribute<double> attr, in Attribute<int> value, double fallback = 0d)
    {
        double left = attr.HasValue ? attr.Value : fallback;
        double right = value.HasValue ? value.Value : fallback;
        return new Attribute<double>(left - right);
    }

    /// <summary>
    /// 减法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Subtract(this in Attribute<double> attr, float value, double fallback = 0d)
    {
        double baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<double>(baseValue - value);
    }

    /// <summary>
    /// 减法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Subtract(this in Attribute<double> attr, int value, double fallback = 0d)
    {
        double baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<double>(baseValue - value);
    }

    /// <summary>
    /// 乘法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Multiply(this in Attribute<double> attr, double value, double fallback = 1d)
    {
        double baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<double>(baseValue * value);
    }

    /// <summary>
    /// 乘法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Multiply(this in Attribute<double> attr, in Attribute<double> value, double fallback = 1d)
    {
        double left = attr.HasValue ? attr.Value : fallback;
        double right = value.HasValue ? value.Value : fallback;
        return new Attribute<double>(left * right);
    }

    /// <summary>
    /// 乘法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Multiply(this in Attribute<double> attr, in Attribute<float> value, double fallback = 1d)
    {
        double left = attr.HasValue ? attr.Value : fallback;
        double right = value.HasValue ? value.Value : fallback;
        return new Attribute<double>(left * right);
    }

    /// <summary>
    /// 乘法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Multiply(this in Attribute<double> attr, in Attribute<int> value, double fallback = 1d)
    {
        double left = attr.HasValue ? attr.Value : fallback;
        double right = value.HasValue ? value.Value : fallback;
        return new Attribute<double>(left * right);
    }

    /// <summary>
    /// 乘法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Multiply(this in Attribute<double> attr, float value, double fallback = 1d)
    {
        double baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<double>(baseValue * value);
    }

    /// <summary>
    /// 乘法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Multiply(this in Attribute<double> attr, int value, double fallback = 1d)
    {
        double baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<double>(baseValue * value);
    }

    /// <summary>
    /// 除法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Divide(this in Attribute<double> attr, double value, double fallback = 0d)
    {
        double baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<double>(baseValue / value);
    }

    /// <summary>
    /// 除法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Divide(this in Attribute<double> attr, in Attribute<double> value, double fallback = 0d)
    {
        double left = attr.HasValue ? attr.Value : fallback;
        double right = value.HasValue ? value.Value : 1d;
        return new Attribute<double>(left / right);
    }

    /// <summary>
    /// 除法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Divide(this in Attribute<double> attr, in Attribute<float> value, double fallback = 0d)
    {
        double left = attr.HasValue ? attr.Value : fallback;
        double right = value.HasValue ? value.Value : 1d;
        return new Attribute<double>(left / right);
    }

    /// <summary>
    /// 除法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Divide(this in Attribute<double> attr, in Attribute<int> value, double fallback = 0d)
    {
        double left = attr.HasValue ? attr.Value : fallback;
        double right = value.HasValue ? value.Value : 1d;
        return new Attribute<double>(left / right);
    }

    /// <summary>
    /// 除法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Divide(this in Attribute<double> attr, float value, double fallback = 0d)
    {
        double baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<double>(baseValue / value);
    }

    /// <summary>
    /// 除法运算
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Divide(this in Attribute<double> attr, int value, double fallback = 0d)
    {
        double baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<double>(baseValue / value);
    }

    /// <summary>
    /// 取负值
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <returns>计算结果</returns>
    public static Attribute<double> Negate(this in Attribute<double> attr)
    {
        if (!attr.HasValue) return attr;
        return new Attribute<double>(-attr.Value);
    }

    #endregion

    #region 类型转换

    /// <summary>
    /// 四舍五入取整
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <returns>计算结果</returns>
    public static Attribute<int> RoundToInt(this in Attribute<float> attr)
    {
        if (!attr.HasValue) return Attribute<int>.None;
        return new Attribute<int>((int)Math.Round(attr.Value));
    }

    /// <summary>
    /// 向下取整
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <returns>计算结果</returns>
    public static Attribute<int> FloorToInt(this in Attribute<float> attr)
    {
        if (!attr.HasValue) return Attribute<int>.None;
        return new Attribute<int>((int)Math.Floor(attr.Value));
    }

    /// <summary>
    /// 四舍五入取整
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <returns>计算结果</returns>
    public static Attribute<int> RoundToInt(this in Attribute<double> attr)
    {
        if (!attr.HasValue) return Attribute<int>.None;
        return new Attribute<int>((int)Math.Round(attr.Value));
    }

    /// <summary>
    /// 向下取整
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <returns>计算结果</returns>
    public static Attribute<int> FloorToInt(this in Attribute<double> attr)
    {
        if (!attr.HasValue) return Attribute<int>.None;
        return new Attribute<int>((int)Math.Floor(attr.Value));
    }

    #endregion

    #region 精度比较

    /// <summary>
    /// 精度相等比较
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">比较值</param>
    /// <param name="epsilon">精度阈值</param>
    /// <returns>比较结果</returns>
    public static Attribute<bool> PrecisionEqual(this in Attribute<float> attr, float value, float epsilon = 0.0001f)
    {
        if (!attr.HasValue) return false;
        return Math.Abs(attr.Value - value) < epsilon;
    }

    /// <summary>
    /// 精度相等比较
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">比较值</param>
    /// <param name="epsilon">精度阈值</param>
    /// <returns>比较结果</returns>
    public static Attribute<bool> PrecisionEqual(this in Attribute<float> attr, in Attribute<float> value, float epsilon = 0.0001f)
    {
        if (!attr.HasValue || !value.HasValue) return false;
        return Math.Abs(attr.Value - value.Value) < epsilon;
    }

    /// <summary>
    /// 精度相等比较
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">比较值</param>
    /// <param name="epsilon">精度阈值</param>
    /// <returns>比较结果</returns>
    public static Attribute<bool> PrecisionEqual(this in Attribute<double> attr, double value, double epsilon = 0.000001d)
    {
        if (!attr.HasValue) return false;
        return Math.Abs(attr.Value - value) < epsilon;
    }

    /// <summary>
    /// 精度相等比较
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">比较值</param>
    /// <param name="epsilon">精度阈值</param>
    /// <returns>比较结果</returns>
    public static Attribute<bool> PrecisionEqual(this in Attribute<double> attr, in Attribute<double> value, double epsilon = 0.000001d)
    {
        if (!attr.HasValue || !value.HasValue) return false;
        return Math.Abs(attr.Value - value.Value) < epsilon;
    }

    #endregion

    #region 泛型比较运算

    /// <summary>
    /// 大于比较
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">比较值</param>
    /// <returns>比较结果</returns>
    public static Attribute<bool> GreaterThan<T>(this in Attribute<T> attr, T value) where T : IComparable<T>
    {
        if (!attr.HasValue) return Attribute<bool>.None;
        return attr.Value.CompareTo(value) > 0;
    }

    /// <summary>
    /// 大于比较
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">比较值</param>
    /// <returns>比较结果</returns>
    public static Attribute<bool> GreaterThan<T>(this in Attribute<T> attr, in Attribute<T> value) where T : IComparable<T>
    {
        if (!attr.HasValue || !value.HasValue) return Attribute<bool>.None;
        return attr.Value.CompareTo(value.Value) > 0;
    }

    /// <summary>
    /// 小于比较
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">比较值</param>
    /// <returns>比较结果</returns>
    public static Attribute<bool> LessThan<T>(this in Attribute<T> attr, T value) where T : IComparable<T>
    {
        if (!attr.HasValue) return Attribute<bool>.None;
        return attr.Value.CompareTo(value) < 0;
    }

    /// <summary>
    /// 小于比较
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">比较值</param>
    /// <returns>比较结果</returns>
    public static Attribute<bool> LessThan<T>(this in Attribute<T> attr, in Attribute<T> value) where T : IComparable<T>
    {
        if (!attr.HasValue || !value.HasValue) return Attribute<bool>.None;
        return attr.Value.CompareTo(value.Value) < 0;
    }

    /// <summary>
    /// 大于等于比较
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">比较值</param>
    /// <returns>比较结果</returns>
    public static Attribute<bool> GreaterThanOrEqual<T>(this in Attribute<T> attr, T value) where T : IComparable<T>
    {
        if (!attr.HasValue) return Attribute<bool>.None;
        return attr.Value.CompareTo(value) >= 0;
    }

    /// <summary>
    /// 大于等于比较
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">比较值</param>
    /// <returns>比较结果</returns>
    public static Attribute<bool> GreaterThanOrEqual<T>(this in Attribute<T> attr, in Attribute<T> value) where T : IComparable<T>
    {
        if (!attr.HasValue || !value.HasValue) return Attribute<bool>.None;
        return attr.Value.CompareTo(value.Value) >= 0;
    }

    /// <summary>
    /// 小于等于比较
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">比较值</param>
    /// <returns>比较结果</returns>
    public static Attribute<bool> LessThanOrEqual<T>(this in Attribute<T> attr, T value) where T : IComparable<T>
    {
        if (!attr.HasValue) return Attribute<bool>.None;
        return attr.Value.CompareTo(value) <= 0;
    }

    /// <summary>
    /// 小于等于比较
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">比较值</param>
    /// <returns>比较结果</returns>
    public static Attribute<bool> LessThanOrEqual<T>(this in Attribute<T> attr, in Attribute<T> value) where T : IComparable<T>
    {
        if (!attr.HasValue || !value.HasValue) return Attribute<bool>.None;
        return attr.Value.CompareTo(value.Value) <= 0;
    }

    /// <summary>
    /// 相等比较
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">比较值</param>
    /// <returns>比较结果</returns>
    public static Attribute<bool> ValueEqual<T>(this in Attribute<T> attr, T value) where T : IEquatable<T>
    {
        if (!attr.HasValue) return false;
        return attr.Value.Equals(value);
    }

    /// <summary>
    /// 相等比较
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">比较值</param>
    /// <returns>比较结果</returns>
    public static Attribute<bool> ValueEqual<T>(this in Attribute<T> attr, in Attribute<T> value) where T : IEquatable<T>
    {
        if (!attr.HasValue || !value.HasValue) return false;
        return attr.Value.Equals(value.Value);
    }

    #endregion

    #region 逻辑运算

    /// <summary>
    /// 逻辑与
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<bool> And(this in Attribute<bool> attr, bool value, bool fallback = true)
    {
        bool baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<bool>(baseValue && value);
    }

    /// <summary>
    /// 逻辑与
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<bool> And(this in Attribute<bool> attr, in Attribute<bool> value, bool fallback = true)
    {
        bool left = attr.HasValue ? attr.Value : fallback;
        bool right = value.HasValue ? value.Value : fallback;
        return new Attribute<bool>(left && right);
    }

    /// <summary>
    /// 逻辑或
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<bool> Or(this in Attribute<bool> attr, bool value, bool fallback = false)
    {
        bool baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<bool>(baseValue || value);
    }

    /// <summary>
    /// 逻辑或
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">运算值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>计算结果</returns>
    public static Attribute<bool> Or(this in Attribute<bool> attr, in Attribute<bool> value, bool fallback = false)
    {
        bool left = attr.HasValue ? attr.Value : fallback;
        bool right = value.HasValue ? value.Value : fallback;
        return new Attribute<bool>(left || right);
    }

    /// <summary>
    /// 逻辑非
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <returns>计算结果</returns>
    public static Attribute<bool> Not(this in Attribute<bool> attr)
    {
        if (!attr.HasValue) return false;
        return !attr.Value;
    }

    #endregion

    #region 条件选择

    /// <summary>
    /// 根据条件选择值
    /// </summary>
    /// <param name="attr">条件值</param>
    /// <param name="trueValue">真值时返回的值</param>
    /// <param name="falseValue">假值时返回的值</param>
    /// <returns>选择结果</returns>
    public static Attribute<T> Select<T>(this in Attribute<bool> attr, T trueValue, T falseValue)
    {
        if (!attr.HasValue) return Attribute<T>.None;
        return new Attribute<T>(attr.Value ? trueValue : falseValue);
    }

    /// <summary>
    /// 根据条件选择值
    /// </summary>
    /// <param name="attr">条件值</param>
    /// <param name="trueAttr">真值时返回的属性值</param>
    /// <param name="falseAttr">假值时返回的属性值</param>
    /// <returns>选择结果</returns>
    public static Attribute<T> Select<T>(this in Attribute<bool> attr, in Attribute<T> trueAttr, in Attribute<T> falseAttr)
    {
        if (!attr.HasValue) return Attribute<T>.None;
        return attr.Value ? trueAttr : falseAttr;
    }

    #endregion

    #region 字符串操作

    /// <summary>
    /// 字符串连接
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">连接值</param>
    /// <param name="fallback">缺省值</param>
    /// <returns>连接结果</returns>
    public static Attribute<string> Concat(this in Attribute<string> attr, string value, string fallback = "")
    {
        string baseValue = attr.HasValue ? attr.Value : fallback;
        return new Attribute<string>(baseValue + value);
    }

    #endregion

    #region 数值约束

    /// <summary>
    /// 取最大值
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">比较值</param>
    /// <returns>计算结果</returns>
    public static Attribute<T> Max<T>(this in Attribute<T> attr, T value) where T : IComparable<T>
    {
        if (!attr.HasValue) return Attribute<T>.None;
        return new Attribute<T>(attr.Value.CompareTo(value) > 0 ? attr.Value : value);
    }

    /// <summary>
    /// 取最小值
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="value">比较值</param>
    /// <returns>计算结果</returns>
    public static Attribute<T> Min<T>(this in Attribute<T> attr, T value) where T : IComparable<T>
    {
        if (!attr.HasValue) return Attribute<T>.None;
        return new Attribute<T>(attr.Value.CompareTo(value) < 0 ? attr.Value : value);
    }

    /// <summary>
    /// 限制在范围内
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="min">最小值</param>
    /// <param name="max">最大值</param>
    /// <returns>计算结果</returns>
    public static Attribute<T> Clamp<T>(this in Attribute<T> attr, T min, T max) where T : IComparable<T>
    {
        if (!attr.HasValue) return Attribute<T>.None;
        T value = attr.Value;
        if (value.CompareTo(min) < 0) value = min;
        if (value.CompareTo(max) > 0) value = max;
        return new Attribute<T>(value);
    }

    #endregion

    #region 空值处理

    /// <summary>
    /// 获取值，无值时返回降级值
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="fallback">降级值</param>
    /// <returns>值</returns>
    public static T Coalesce<T>(this in Attribute<T> attr, T fallback)
    {
        return attr.HasValue ? attr.Value : fallback;
    }

    /// <summary>
    /// 无值时使用备用值
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="fallback">备用值</param>
    /// <returns>属性值</returns>
    public static Attribute<T> FallbackTo<T>(this in Attribute<T> attr, in Attribute<T> fallback)
    {
        return attr.HasValue ? attr : fallback;
    }

    /// <summary>
    /// 无值时使用备用值
    /// </summary>
    /// <param name="attr">属性值</param>
    /// <param name="fallback">备用值</param>
    /// <returns>属性值</returns>
    public static Attribute<T> FallbackTo<T>(this in Attribute<T> attr, T fallback)
    {
        return attr.HasValue ? attr : new Attribute<T>(fallback);
    }

    #endregion
}