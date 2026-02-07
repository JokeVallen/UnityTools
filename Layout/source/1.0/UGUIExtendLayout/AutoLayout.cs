using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoLayout : LayoutGroup
{
    /// <summary>
    /// 关键点合集的只读视图
    /// </summary>
    public virtual IReadOnlyList<Keyframe> Keys
    {
        get
        {
            return m_Curve.keys;
        }
    }

    /// <summary>
    /// 关键点合集的数量
    /// </summary>
    public virtual int Length
    {
        get
        {
            return m_Curve.length;
        }
    }

    [Tooltip("布局曲线"), SerializeField] protected AnimationCurve m_Curve;

    /// <summary>
    /// 曲线末端的行为模式
    /// </summary>
    public virtual WrapMode CurveEndMode
    {
        get
        {
            return m_CurveEndMode;
        }
        set
        {
            SetProperty(ref m_CurveEndMode, value);
        }
    }
    [Tooltip("曲线末端的行为模式"), SerializeField] protected WrapMode m_CurveEndMode;

    /// <summary>
    /// 曲线水平轴乘数
    /// </summary>
    public virtual float XMultiplier
    {
        get
        {
            return m_XMultiplier;
        }
        set
        {
            SetProperty(ref m_XMultiplier, value);
        }
    }
    [Tooltip("曲线水平轴乘数"), SerializeField] protected float m_XMultiplier;

    /// <summary>
    /// 曲线垂直轴乘数
    /// </summary>
    public virtual float YMultiplier
    {
        get
        {
            return m_YMultiplier;
        }
        set
        {
            SetProperty(ref m_YMultiplier, value);
        }
    }
    [Tooltip("曲线垂直轴乘数"), SerializeField] protected float m_YMultiplier;

    /// <summary>
    /// 添加关键点
    /// </summary>
    /// <param name="time">添加关键点的时间（曲线图中的水平轴）</param>
    /// <param name="value">关键点的值（曲线图中的垂直轴）</param>
    /// <returns>已添加的关键点的索引，如果无法添加关键点，则为 -1</returns>
    public virtual int AddKey(float time, float value)
    {
        int result = m_Curve.AddKey(time, value);
        SetDirty();
        return result;
    }

    /// <summary>
    /// 添加关键点
    /// </summary>
    /// <param name="key">要添加到曲线中的关键点</param>
    /// <returns>已添加的关键点的索引，如果无法添加关键点，则为 -1</returns>
    public virtual int AddKey(Keyframe key)
    {
        int result = m_Curve.AddKey(key);
        SetDirty();
        return result;
    }

    /// <summary>
    /// 获取指定时间点的曲线值
    /// </summary>
    /// <param name="time">曲线内的时间点（曲线图中的水平轴）</param>
    /// <returns>曲线值</returns>
    public virtual float Evaluate(float time)
    {
        return m_Curve.Evaluate(time);
    }

    /// <summary>
    /// 移除指定索引的关键点，然后插入关键点
    /// </summary>
    /// <param name="index">索引</param>
    /// <param name="key">待插入的关键点</param>
    /// <returns>关键点在移动后的索引</returns>
    public virtual int MoveKey(int index, Keyframe key)
    {
        int result = m_Curve.MoveKey(index, key);
        SetDirty();
        return result;
    }

    /// <summary>
    /// 移除指定索引的关键点
    /// </summary>
    /// <param name="index">索引</param>
    public virtual void RemoveKey(int index)
    {
        m_Curve.RemoveKey(index);
        SetDirty();
    }

    /// <summary>
    /// 平滑指定索引的关键点的进出切线
    /// </summary>
    /// <param name="index">索引</param>
    /// <param name="weight">切线的平滑权值</param>
    public virtual void SmoothTangents(int index, float weight)
    {
        m_Curve.SmoothTangents(index, weight);
        SetDirty();
    }

    /// <summary>
    /// 清空所有关键点
    /// </summary>
    public virtual void ClearKeys()
    {
        m_Curve.keys = Array.Empty<Keyframe>();
        SetDirty();
    }

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        DoCalculate(0);
    }

    public override void CalculateLayoutInputVertical()
    {
        DoCalculate(1);
    }

    public override void SetLayoutHorizontal()
    {
        DoSetChildren(0);
    }

    public override void SetLayoutVertical()
    {
        DoSetChildren(1);
    }

    protected virtual void DoCalculate(int axis)
    {
        float combinedPadding = axis == 0 ? padding.horizontal : padding.vertical;
        float totalMin = combinedPadding;
        float totalPreferred = combinedPadding;
        float totalFlexible = 0;
        int count = rectChildren.Count;
        int index = 0;
        Func<int, int> indexGetter = NextIndexByModeGetter(m_CurveEndMode, 0, m_Curve.length - 1, 1);

        for (int i = 0; i < count; i++)
        {
            RectTransform child = rectChildren[i];
            Keyframe cur = m_Curve.keys[index];
            index = indexGetter(index);
            Keyframe next = m_Curve.keys[index];
            float min = LayoutUtility.GetMinSize(child, axis);
            float preferred = LayoutUtility.GetPreferredSize(child, axis);
            float flexible = LayoutUtility.GetFlexibleSize(child, axis);
            float delta;
            if (i < count - 1)
            {
                delta = Mathf.Abs(axis == 0 ? (next.time - cur.time) * m_XMultiplier : (next.value - cur.value) * m_YMultiplier);
            }
            else
            {
                delta = 0;
            }

            totalMin += min + delta;
            totalPreferred += preferred + delta;
            totalFlexible += flexible;
        }

        totalPreferred = Mathf.Max(totalMin, totalPreferred);
        SetLayoutInputForAxis(totalMin, totalPreferred, totalFlexible, axis);
    }

    protected virtual void DoSetChildren(int axis)
    {
        float startPos = axis == 0 ? padding.left : padding.top;
        float pos;
        int count = rectChildren.Count;
        float multiplier = axis == 0 ? m_XMultiplier : m_YMultiplier;
        int index = 0;
        int keyframeCount = m_Curve.length;
        Func<int, int> indexGetter = NextIndexByModeGetter(m_CurveEndMode, 0, keyframeCount - 1, 1);

        for (int i = 0; i < count; i++)
        {
            RectTransform child = rectChildren[i];
            Keyframe keyframe = m_Curve[index];
            float factor = axis == 0 ? keyframe.time : -keyframe.value;
            pos = child.sizeDelta[axis] * factor * multiplier;
            SetChildAlongAxis(child, axis, startPos + pos);
            index = indexGetter(index);
        }
    }

    protected Func<int, int> NextIndexByModeGetter(WrapMode mode, int minIndex, int maxIndex, int step)
    {
        int absStep = Mathf.Abs(step);
        switch ((int)mode)
        {
            case 0:
            case 1:
            case 8:
                return index => Mathf.Clamp(index + absStep, minIndex, maxIndex);
            case 2:
                return index => Mathf.Clamp(index >= maxIndex ? minIndex : index + absStep, minIndex, maxIndex);
            case 4:
                bool isMinToMax = true;
                return index =>
                {
                    if (index <= minIndex) isMinToMax = true;
                    else if (index >= maxIndex) isMinToMax = false;
                    return Mathf.Clamp(index + (isMinToMax ? absStep : -absStep), minIndex, maxIndex);
                };
        }
        return null;
    }

#if UNITY_EDITOR

    protected override void Reset()
    {
        m_Curve = AnimationCurve.Linear(0, 0, 1, 1);
        m_CurveEndMode = WrapMode.Default;
        m_XMultiplier = 1;
        m_YMultiplier = 1;
    }

#endif
}