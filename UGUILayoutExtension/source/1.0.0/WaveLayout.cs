using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveLayout : AutoLayout
{
    [Serializable]
    private readonly struct LayoutKeyframe
    {
        public float Time { get; }
        public float Value { get; }
        public int TemplateIndex { get; }

        public LayoutKeyframe(float time, float value, int templateIndex)
        {
            Time = time;
            Value = value;
            TemplateIndex = templateIndex;
        }
    }

    public override WrapMode CurveEndMode => base.CurveEndMode;
    [NonSerialized] private int m_BackupChildrenCount;
    private Keyframe[] CurveTemplateGetter
    {
        get
        {
            if (m_CurveTemplate == null)
            {
                m_CurveTemplate = new Keyframe[]
                {
                        new Keyframe(0,0),
                        new Keyframe(0.5f, 0.5f),
                        new Keyframe(1,0)
                };
            }
            return m_CurveTemplate;
        }
    }
    [NonSerialized] private Keyframe[] m_CurveTemplate;

    private List<LayoutKeyframe> LayoutKeyframesGetter
    {
        get
        {
            if (m_LayoutKeyframes == null) m_LayoutKeyframes = new List<LayoutKeyframe>();
            return m_LayoutKeyframes;
        }
    }
    [SerializeField] private List<LayoutKeyframe> m_LayoutKeyframes;

    public override int AddKey(Keyframe key)
    {
        return -1;
    }

    public override int AddKey(float time, float value)
    {
        return -1;
    }

    public override void ClearKeys()
    {
        return;
    }

    public override int MoveKey(int index, Keyframe key)
    {
        return -1;
    }

    public override void RemoveKey(int index)
    {
        return;
    }

    public override void SmoothTangents(int index, float weight)
    {
        return;
    }

    protected override void DoCalculate(int axis)
    {
        if (rectChildren.Count != m_BackupChildrenCount)
        {
            m_BackupChildrenCount = rectChildren.Count;
            UpdateCurve();
        }

        base.DoCalculate(axis);
    }

    private void UpdateCurve()
    {
        int keyframeCount = m_Curve.length;
        int offset = keyframeCount - m_BackupChildrenCount;
        if (offset == 0) return;

        int len = Mathf.Abs(offset);
        int templateKeyframeCount = CurveTemplateGetter.Length;
        var lastLayoutKeyframe = LayoutKeyframesGetter.Count > 0 ? LayoutKeyframesGetter[keyframeCount - 1] : default;
        int preItemIndex = lastLayoutKeyframe.TemplateIndex;
        int nextItemIndex;

        for (int i = 0; i < len; i++)
        {
            if (offset < 0)
            {
                if (m_Curve.length > 0)
                {
                    Keyframe last = m_Curve.keys[m_Curve.length - 1];
                    while (true)
                    {
                        preItemIndex %= templateKeyframeCount;
                        nextItemIndex = (preItemIndex + 1) % templateKeyframeCount;
                        if (preItemIndex < nextItemIndex) break;
                        preItemIndex++;
                    }

                    Keyframe pre = CurveTemplateGetter[preItemIndex];
                    Keyframe next = CurveTemplateGetter[nextItemIndex];

                    float timeDelta = next.time - pre.time;
                    next.time = last.time + timeDelta;
                    m_Curve.AddKey(next);
                    LayoutKeyframesGetter.Add(new LayoutKeyframe(next.time, next.value, nextItemIndex));
                    preItemIndex++;
                }
                else
                {
                    Keyframe template = m_CurveTemplate[0];
                    m_Curve.AddKey(template);
                    LayoutKeyframesGetter.Add(new LayoutKeyframe(template.time, template.value, 0));
                }
            }
            else
            {
                LayoutKeyframesGetter.RemoveAt(m_Curve.length - 1);
                m_Curve.RemoveKey(m_Curve.length - 1);
            }
        }
    }

#if UNITY_EDITOR

    protected override void Reset()
    {
        m_Curve = new AnimationCurve(CurveTemplateGetter);
        m_CurveEndMode = WrapMode.ClampForever;
        m_XMultiplier = 0;
        m_YMultiplier = 0;
        m_BackupChildrenCount = 0;
        m_LayoutKeyframes?.Clear();
        int index = 0;
        foreach (var keyframe in m_Curve.keys)
        {
            LayoutKeyframesGetter.Add(new LayoutKeyframe(keyframe.time, keyframe.value, index++));
        }
    }

#endif
}