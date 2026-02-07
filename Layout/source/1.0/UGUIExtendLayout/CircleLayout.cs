/*
    简介：这是基于UGUI布局系统扩展的圆形布局组件，可以设置园的半径、旋转和是否顺时针排序。
*/

using UnityEngine;
using UnityEngine.UI;

public class CircleLayout : LayoutGroup
{
    /// <summary>
    /// 半径
    /// </summary>
    public float Radius
    {
        get { return m_Radius; }
        set
        {
            if (value < 0) return;
            SetProperty(ref m_Radius, value);
        }
    }
    [Tooltip("The radius of layout."), SerializeField] private float m_Radius;

    /// <summary>
    /// 旋转角度
    /// </summary>
    public int Rotation
    {
        get { return m_Rotation; }
        set
        {
            if (value < 0 || value > 360) return;
            SetProperty(ref m_Rotation, value);
        }
    }
    [Tooltip("The angle of rotation."), SerializeField, Range(0, 360)] private int m_Rotation;

    /// <summary>
    /// 是否顺时针布局
    /// </summary>
    public bool ClockWise
    {
        get { return m_ClockWise; }
        set { SetProperty(ref m_ClockWise, value); }
    }
    [Tooltip("Whether to arrange in clockwise order."), SerializeField] private bool m_ClockWise;

    protected CircleLayout() { }

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

    private void DoCalculate(int axis)
    {
        float combinedPadding = Mathf.Max(padding.horizontal, padding.vertical);
        float totalMin = combinedPadding + m_Radius * 2;
        float totalPreferred = combinedPadding + m_Radius * 2;
        float totalFlexible = combinedPadding + m_Radius * 2;
        SetLayoutInputForAxis(totalMin, totalPreferred, totalFlexible, axis);
    }

    private void DoSetChildren(int axis)
    {
        int count = rectChildren.Count;
        float centerPos = rectTransform.anchoredPosition[axis];
        float angleDelta = count > 0 ? 360f / count : 0;
        if (!ClockWise) angleDelta = -angleDelta;
        int startAngle = m_Rotation % 360;

        for (int i = 0; i < count; i++)
        {
            RectTransform child = rectChildren[i];
            float pos = centerPos + GetChildPosOffset(axis, startAngle + i * angleDelta, m_Radius);
            SetChildAlongAxis(child, axis, pos);
        }
    }

    private float GetChildPosOffset(int axis, float angle, float len)
    {
        float value = angle % 360 / 180 * Mathf.PI;
        return len * (axis == 0 ? Mathf.Cos(value) : Mathf.Sin(value));
    }

#if UNITY_EDITOR

    protected override void Reset()
    {
        base.Reset();

        m_Radius = 0;
        m_Rotation = 0;
        m_ClockWise = false;
    }

#endif
}