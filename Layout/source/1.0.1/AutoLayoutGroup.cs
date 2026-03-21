using System;
using UnityEngine;
using UnityEngine.UI;

namespace UGUI.Layout.Extension
{
    /// <summary>
    /// 自动布局组件
    /// </summary>
    /// <remarks>
    /// <para>基于 <see cref="BaseAutoLayoutGroup"/> 和 <see cref="AnimationCurve"/> 扩展的自动布局组件，可通过动画曲线映射布局规律。</para>
    /// </remarks>
    [AddComponentMenu("Layout/AutoLayoutGroup")]
    public sealed class AutoLayoutGroup : BaseAutoLayoutGroup
    {
        [Tooltip("X 轴布局曲线：关键帧的 value 值决定布局元素水平方向的偏移因子"), SerializeField]
        private AnimationCurve curveX = new AnimationCurve()
        {
            preWrapMode = WrapMode.Default,
            postWrapMode = WrapMode.Default
        };

        /// <summary>X 轴曲线前端的行为模式</summary>
        public WrapMode PreWrapModeX
        {
            get => curveX.preWrapMode;
            set
            {
                if (value == curveX.preWrapMode) return;
                curveX.preWrapMode = value;
                SetDirty();
            }
        }
        [Tooltip("X 轴曲线前端的行为模式"), SerializeField]
        private WrapMode preWrapModeX = WrapMode.Default;

        /// <summary>X 轴曲线末端的行为模式</summary>
        public WrapMode PostWrapModeX
        {
            get => curveX.postWrapMode;
            set
            {
                if (value == curveX.postWrapMode) return;
                curveX.postWrapMode = value;
                SetDirty();
            }
        }
        [Tooltip("X 轴曲线末端的行为模式"), SerializeField]
        private WrapMode postWrapModeX = WrapMode.Default;

        /// <summary>X 轴布局曲线的关键帧数量</summary>
        public int LengthX => curveX.length;

        /// <summary>X 轴布局曲线的关键帧集合</summary>
        public Keyframe[] KeysX
        {
            get => curveX.keys;
            set { curveX.keys = value; SetDirty(); }
        }

        [Tooltip("Y 轴布局曲线：关键帧的 value 值决定布局元素垂直方向的偏移因子，value 为正时元素偏向上方"), SerializeField]
        private AnimationCurve curveY = new AnimationCurve()
        {
            preWrapMode = WrapMode.Default,
            postWrapMode = WrapMode.Default
        };

        /// <summary>Y 轴曲线前端的行为模式</summary>
        public WrapMode PreWrapModeY
        {
            get => curveY.preWrapMode;
            set
            {
                if (value == curveY.preWrapMode) return;
                curveY.preWrapMode = value;
                SetDirty();
            }
        }
        [Tooltip("Y 轴曲线前端的行为模式"), SerializeField]
        private WrapMode preWrapModeY = WrapMode.Default;

        /// <summary>Y 轴曲线末端的行为模式</summary>
        public WrapMode PostWrapModeY
        {
            get => curveY.postWrapMode;
            set
            {
                if (value == curveY.postWrapMode) return;
                curveY.postWrapMode = value;
                SetDirty();
            }
        }
        [Tooltip("Y 轴曲线末端的行为模式"), SerializeField]
        private WrapMode postWrapModeY = WrapMode.Default;

        /// <summary>Y 轴布局曲线的关键帧数量</summary>
        public int LengthY => curveY.length;

        /// <summary>Y 轴布局曲线的关键帧集合</summary>
        public Keyframe[] KeysY
        {
            get => curveY.keys;
            set { curveY.keys = value; SetDirty(); }
        }

        /// <summary>关键帧映射模式</summary>
        public KeyframeMappingMode MappingMode
        {
            get => mappingMode;
            set { if (value == mappingMode) return; mappingMode = value; SetDirty(); }
        }
        [Tooltip("关键帧映射模式"), SerializeField]
        private KeyframeMappingMode mappingMode = KeyframeMappingMode.Direct;

        /// <summary>
        /// 布局元素位置计算模式
        /// </summary>
        /// <remarks>
        /// <para><see cref="PositionMode.ByElementSize"/>（默认）：<c>pos = sizeDelta * factor * scale</c>，
        /// 偏移量与布局元素自身尺寸成比例。</para>
        /// <para><see cref="PositionMode.ByPixel"/>：<c>pos = factor * scale</c>，
        /// 曲线直接描述像素偏移，与布局元素尺寸无关，不同尺寸的布局元素在同一关键帧下位置一致。</para>
        /// </remarks>
        public PositionMode PositionMode
        {
            get => positionMode;
            set { if (value == positionMode) return; positionMode = value; SetDirty(); }
        }
        [Tooltip("位置计算模式：ByElementSize 以布局元素自身尺寸为单位缩放偏移，ByPixel 以像素为单位直接定位"),
         SerializeField]
        private PositionMode positionMode = PositionMode.ByElementSize;

        /// <summary>
        /// 是否启用 <see cref="GroupSize"/> 约束
        /// </summary>
        /// <remarks>仅在 <see cref="KeyframeMappingMode.Interpolated"/> 模式下有效。</remarks>
        public bool ConstrainByGroup
        {
            get => constrainByGroup;
            set { if (value == constrainByGroup) return; constrainByGroup = value; SetDirty(); }
        }
        [Tooltip("启用后按 GroupSize 控制每周期布局元素数量；禁用时所有布局元素均匀分布在一个曲线周期内。"),
         SerializeField]
        private bool constrainByGroup = false;

        /// <summary>
        /// Interpolated 模式下每个曲线周期包含的布局元素数量
        /// </summary>
        /// <remarks>
        /// 仅在 <see cref="KeyframeMappingMode.Interpolated"/> 且 <see cref="ConstrainByGroup"/> 为
        /// <c>true</c> 时生效。布局元素总数超过该值时，超出部分由
        /// <see cref="PostWrapModeX"/> / <see cref="PostWrapModeY"/> 决定行为。
        /// </remarks>
        public int GroupSize
        {
            get => groupSize;
            set => SetProperty(ref groupSize, Mathf.Max(1, value));
        }
        [Tooltip("每个曲线周期包含的布局元素数量（仅 Interpolated 模式且启用 ConstrainByGroup 时有效）。"),
         SerializeField, Min(1)]
        private int groupSize = 4;

        /// <summary>
        /// Interpolated 模式下当前布局元素序列覆盖的曲线周期数（只读）
        /// </summary>
        /// <remarks>
        /// 不依赖 <see cref="BaseAutoLayoutGroup.RectChildren"/>，可在布局阶段外安全调用。
        /// 未启用 <see cref="ConstrainByGroup"/> 时固定返回 <c>1</c>。
        /// </remarks>
        public float Cycles => constrainByGroup && cachedChildCount > 1
            ? (float)(cachedChildCount - 1) / groupSize
            : 1f;

        /// <summary>
        /// Proportional 模式的布局元素分配策略
        /// </summary>
        /// <remarks>仅在 <see cref="KeyframeMappingMode.Proportional"/> 模式下有效。</remarks>
        public ProportionalDistributeMode DistributeMode
        {
            get => distributeMode;
            set { if (value == distributeMode) return; distributeMode = value; SetDirty(); }
        }
        [Tooltip("Proportional 模式的布局元素分配策略（仅 Proportional 模式有效）"), SerializeField]
        private ProportionalDistributeMode distributeMode = ProportionalDistributeMode.RoundToNearest;

        /// <summary>X 轴偏移缩放系数</summary>
        public float ScaleX
        {
            get => scaleX;
            set => SetProperty(ref scaleX, value);
        }
        [Tooltip("X 轴偏移缩放系数"), SerializeField] private float scaleX = 1;

        /// <summary>Y 轴偏移缩放系数</summary>
        public float ScaleY
        {
            get => scaleY;
            set => SetProperty(ref scaleY, value);
        }
        [Tooltip("Y 轴偏移缩放系数"), SerializeField] private float scaleY = 1;

        /// <summary>布局元素之间水平方向的固定间距（像素），与曲线偏移叠加</summary>
        public float SpacingHorizontal
        {
            get => spacingHorizontal;
            set => SetProperty(ref spacingHorizontal, Mathf.Max(0, value));
        }
        [Tooltip("布局元素之间水平方向的固定间距（像素），在曲线偏移基础上叠加"),
         SerializeField, Min(0)]
        private float spacingHorizontal = 0;

        /// <summary>布局元素之间垂直方向的固定间距（像素），与曲线偏移叠加</summary>
        public float SpacingVertical
        {
            get => spacingVertical;
            set => SetProperty(ref spacingVertical, Mathf.Max(0, value));
        }
        [Tooltip("布局元素之间垂直方向的固定间距（像素），在曲线偏移基础上叠加"),
         SerializeField, Min(0)]
        private float spacingVertical = 0;

        /// <summary>
        /// 是否反序排列布局元素
        /// </summary>
        /// <remarks>
        /// 启用后布局元素按倒序与曲线对应：最后一个元素对应第一个关键帧/采样点，
        /// 可在不修改曲线的情况下快速翻转排列方向。
        /// </remarks>
        public bool ReverseArrangement
        {
            get => reverseArrangement;
            set { if (value == reverseArrangement) return; reverseArrangement = value; SetDirty(); }
        }
        [Tooltip("启用后布局元素按倒序与曲线对应，可在不修改曲线的情况下翻转排列方向。"),
         SerializeField]
        private bool reverseArrangement = false;

        private int cachedChildCount;
        private float[] factorCache = new float[0];
        private float[] effectiveSizeCache = new float[0];

        /// <summary>获取 X 轴曲线指定索引的关键帧</summary>
        /// <exception cref="IndexOutOfRangeException">索引越界</exception>
        public Keyframe GetKeyX(int index)
        {
            if (index < 0 || index >= curveX.length)
                throw new IndexOutOfRangeException($"X 轴曲线关键帧索引 {index} 越界，当前长度为 {curveX.length}");
            return curveX[index];
        }

        /// <summary>向 X 轴曲线添加关键帧</summary>
        public int AddKeyX(float time, float value) { int r = curveX.AddKey(time, value); SetDirty(); return r; }

        /// <summary>向 X 轴曲线添加关键帧</summary>
        public int AddKeyX(Keyframe key) { int r = curveX.AddKey(key); SetDirty(); return r; }

        /// <summary>获取 X 轴曲线在指定时间的值</summary>
        public float EvaluateX(float time) => curveX.Evaluate(time);

        /// <summary>移动 X 轴曲线的指定关键帧</summary>
        public int MoveKeyX(int index, Keyframe key) { int r = curveX.MoveKey(index, key); SetDirty(); return r; }

        /// <summary>移除 X 轴曲线的指定关键帧</summary>
        public void RemoveKeyX(int index) { curveX.RemoveKey(index); SetDirty(); }

        /// <summary>平滑 X 轴曲线的指定关键帧切线</summary>
        public void SmoothTangentsX(int index, float weight) { curveX.SmoothTangents(index, weight); SetDirty(); }

        /// <summary>清空 X 轴曲线所有关键帧</summary>
        public void ClearKeysX() { curveX.keys = Array.Empty<Keyframe>(); SetDirty(); }

        /// <summary>获取 Y 轴曲线指定索引的关键帧</summary>
        /// <exception cref="IndexOutOfRangeException">索引越界</exception>
        public Keyframe GetKeyY(int index)
        {
            if (index < 0 || index >= curveY.length)
                throw new IndexOutOfRangeException($"Y 轴曲线关键帧索引 {index} 越界，当前长度为 {curveY.length}");
            return curveY[index];
        }

        /// <summary>向 Y 轴曲线添加关键帧</summary>
        public int AddKeyY(float time, float value) { int r = curveY.AddKey(time, value); SetDirty(); return r; }

        /// <summary>向 Y 轴曲线添加关键帧</summary>
        public int AddKeyY(Keyframe key) { int r = curveY.AddKey(key); SetDirty(); return r; }

        /// <summary>获取 Y 轴曲线在指定时间的值</summary>
        public float EvaluateY(float time) => curveY.Evaluate(time);

        /// <summary>移动 Y 轴曲线的指定关键帧</summary>
        public int MoveKeyY(int index, Keyframe key) { int r = curveY.MoveKey(index, key); SetDirty(); return r; }

        /// <summary>移除 Y 轴曲线的指定关键帧</summary>
        public void RemoveKeyY(int index) { curveY.RemoveKey(index); SetDirty(); }

        /// <summary>平滑 Y 轴曲线的指定关键帧切线</summary>
        public void SmoothTangentsY(int index, float weight) { curveY.SmoothTangents(index, weight); SetDirty(); }

        /// <summary>清空 Y 轴曲线所有关键帧</summary>
        public void ClearKeysY() { curveY.keys = Array.Empty<Keyframe>(); SetDirty(); }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalculateLayoutInput(0);
        }

        public override void CalculateLayoutInputVertical()
        {
            CalculateLayoutInput(1);
        }

        public override void SetLayoutHorizontal()
        {
            SetLayout(0);
        }

        public override void SetLayoutVertical()
        {
            SetLayout(1);
        }

        private void CalculateLayoutInput(int axis)
        {
            int count = RectChildren.Count;
            cachedChildCount = count;

            float startPadding = axis == 0 ? padding.left : padding.top;
            float endPadding = axis == 0 ? padding.right : padding.bottom;

            if (count == 0)
            {
                float empty = startPadding + endPadding;
                SetLayoutInputForAxis(empty, empty, 0, axis);
                return;
            }

            float scale = axis == 0 ? scaleX : scaleY;
            float spacing = axis == 0 ? spacingHorizontal : spacingVertical;

            EnsureFactorCache(count);
            float[] factors = factorCache;
            FillFactors(factors, axis, count);

            float posMin = float.MaxValue;
            float posMax = float.MinValue;
            float totalMinSize = 0;
            float totalPreferred = 0;
            float totalFlexible = 0;

            for (int i = 0; i < count; i++)
            {
                int childIndex = reverseArrangement ? count - 1 - i : i;
                RectTransform child = RectChildren[childIndex];

                float minSize = LayoutUtility.GetMinSize(child, axis);
                float preferredSize = LayoutUtility.GetPreferredSize(child, axis);
                float flexible = LayoutUtility.GetFlexibleSize(child, axis);

                float rawSize = Mathf.Max(0, child.sizeDelta[axis]);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (child.sizeDelta[axis] < 0)
                    Debug.LogWarning(
                        $"[AutoLayout] 布局元素 '{child.name}' 的 sizeDelta[{axis}] 为负值，已按 0 处理。", this);
#endif
                float effectiveSize = Mathf.Max(rawSize, Mathf.Max(minSize, preferredSize));

                float edge = positionMode == PositionMode.ByPixel
                    ? factors[i] * scale
                    : effectiveSize * factors[i] * scale;
                edge += i * spacing;

                posMin = Mathf.Min(posMin, edge);
                posMax = Mathf.Max(posMax, edge + effectiveSize);

                totalMinSize += minSize + (i < count - 1 ? spacing : 0);
                totalPreferred += preferredSize + (i < count - 1 ? spacing : 0);
                totalFlexible += flexible;
            }

            float contentSpan = posMax - posMin;
            float span = contentSpan + startPadding + endPadding;

            totalMinSize += startPadding + endPadding;
            totalPreferred += startPadding + endPadding;

            SetLayoutInputForAxis(
                Mathf.Max(totalMinSize, span),
                Mathf.Max(totalPreferred, span),
                totalFlexible,
                axis);
        }

        private void SetLayout(int axis)
        {
            int count = RectChildren.Count;
            if (count == 0) return;

            float scale = axis == 0 ? scaleX : scaleY;
            float spacing = axis == 0 ? spacingHorizontal : spacingVertical;

            EnsureFactorCache(count);
            EnsureEffectiveSizeCache(count);
            float[] factors = factorCache;
            float[] effectiveSizes = effectiveSizeCache;
            FillFactors(factors, axis, count);

            float posMin = float.MaxValue;
            float posMax = float.MinValue;
            for (int i = 0; i < count; i++)
            {
                int childIndex = reverseArrangement ? count - 1 - i : i;
                RectTransform child = RectChildren[childIndex];
                float rawSize = Mathf.Max(0, child.sizeDelta[axis]);
                float effectiveSize = Mathf.Max(rawSize,
                                         Mathf.Max(LayoutUtility.GetMinSize(child, axis),
                                                   LayoutUtility.GetPreferredSize(child, axis)));
                effectiveSizes[i] = effectiveSize;

                float edge = positionMode == PositionMode.ByPixel
                    ? factors[i] * scale
                    : effectiveSize * factors[i] * scale;
                edge += i * spacing;
                posMin = Mathf.Min(posMin, edge);
                posMax = Mathf.Max(posMax, edge + effectiveSize);
            }

            float contentSize = posMax - posMin;
            float startOffset = GetStartOffset(axis, contentSize) - posMin;

            for (int i = 0; i < count; i++)
            {
                int childIndex = reverseArrangement ? count - 1 - i : i;
                RectTransform child = RectChildren[childIndex];
                float effectiveSize = effectiveSizes[i];

                float pos = positionMode == PositionMode.ByPixel
                    ? factors[i] * scale
                    : effectiveSize * factors[i] * scale;
                pos += i * spacing;

                SetChildAlongAxis(child, axis, startOffset + pos, effectiveSize);
            }
        }

        private void EnsureFactorCache(int count)
        {
            if (factorCache.Length < count)
                factorCache = new float[Mathf.NextPowerOfTwo(count)];
        }

        private void EnsureEffectiveSizeCache(int count)
        {
            if (effectiveSizeCache.Length < count)
                effectiveSizeCache = new float[Mathf.NextPowerOfTwo(count)];
        }

        private void FillFactors(float[] factors, int axis, int count)
        {
            switch (mappingMode)
            {
                case KeyframeMappingMode.Direct:
                    FillFactorsDirect(factors, axis, count);
                    break;
                case KeyframeMappingMode.Interpolated:
                    FillFactorsInterpolated(factors, axis, count);
                    break;
                case KeyframeMappingMode.Proportional:
                    FillFactorsProportional(factors, axis, count);
                    break;
                default:
                    for (int i = 0; i < count; i++) factors[i] = 0;
                    break;
            }
        }

        private void FillFactorsDirect(float[] factors, int axis, int count)
        {
            AnimationCurve c = axis == 0 ? curveX : curveY;
            int keyCount = c.length;
            WrapMode wrapMode = axis == 0 ? PostWrapModeX : PostWrapModeY;

            if (keyCount == 0)
            {
                for (int i = 0; i < count; i++) factors[i] = 0;
                return;
            }

            Keyframe[] keys = c.keys;
            for (int i = 0; i < count; i++)
            {
                int keyIndex = CurveIndexStepper.Resolve(i, keyCount, wrapMode);
                Keyframe kf = keys[keyIndex];
                factors[i] = axis == 0 ? kf.value : -kf.value;
            }
        }

        private void FillFactorsInterpolated(float[] factors, int axis, int count)
        {
            AnimationCurve c = axis == 0 ? curveX : curveY;
            for (int i = 0; i < count; i++)
            {
                float t = constrainByGroup
                    ? (float)i / groupSize
                    : (count > 1 ? (float)i / (count - 1) : 0f);

                factors[i] = axis == 0 ? c.Evaluate(t) : -c.Evaluate(t);
            }
        }

        private void FillFactorsProportional(float[] factors, int axis, int count)
        {
            AnimationCurve c = axis == 0 ? curveX : curveY;
            int keyCount = c.length;

            if (keyCount == 0)
            {
                for (int i = 0; i < count; i++) factors[i] = 0;
                return;
            }

            Keyframe[] keys = c.keys;
            for (int i = 0; i < count; i++)
            {
                int keyIdx = ResolveProportionalKeyIndex(i, count, keyCount, distributeMode);
                Keyframe kf = keys[keyIdx];
                factors[i] = axis == 0 ? kf.value : -kf.value;
            }
        }

        private static int ResolveProportionalKeyIndex(
            int childIndex, int childCount, int keyCount, ProportionalDistributeMode mode)
        {
            if (childCount <= 1 || keyCount == 1) return 0;

            switch (mode)
            {
                case ProportionalDistributeMode.RoundToNearest:
                    {
                        float t = (float)childIndex / (childCount - 1);
                        return Mathf.Clamp(Mathf.RoundToInt(t * (keyCount - 1)), 0, keyCount - 1);
                    }
                case ProportionalDistributeMode.FloorBias:
                    {
                        float t = (float)childIndex / (childCount - 1);
                        return Mathf.Clamp(Mathf.FloorToInt(t * (keyCount - 1)), 0, keyCount - 1);
                    }
                case ProportionalDistributeMode.CeilBias:
                    {
                        float t = (float)childIndex / (childCount - 1);
                        return Mathf.Clamp(Mathf.CeilToInt(t * (keyCount - 1)), 0, keyCount - 1);
                    }
                case ProportionalDistributeMode.Uniform:
                    {
                        int accumulated = childIndex * (keyCount - 1);
                        return Mathf.Clamp(accumulated / (childCount - 1), 0, keyCount - 1);
                    }
                default:
                    {
                        float t = (float)childIndex / (childCount - 1);
                        return Mathf.Clamp(Mathf.RoundToInt(t * (keyCount - 1)), 0, keyCount - 1);
                    }
            }
        }

#if UNITY_EDITOR

        public override void RebuildLayout() => SetDirty();

        protected override void Reset()
        {
            curveX = AnimationCurve.Linear(0, 0, 1, 1);
            curveY = AnimationCurve.Linear(0, 0, 1, 1);
            curveX.preWrapMode = WrapMode.Default;
            curveX.postWrapMode = WrapMode.Default;
            curveY.preWrapMode = WrapMode.Default;
            curveY.postWrapMode = WrapMode.Default;
            preWrapModeX = WrapMode.Default;
            postWrapModeX = WrapMode.Default;
            preWrapModeY = WrapMode.Default;
            postWrapModeY = WrapMode.Default;
            scaleX = 1;
            scaleY = 1;
            spacingHorizontal = 0;
            spacingVertical = 0;
            mappingMode = KeyframeMappingMode.Direct;
            positionMode = PositionMode.ByElementSize;
            constrainByGroup = false;
            groupSize = 4;
            distributeMode = ProportionalDistributeMode.RoundToNearest;
            reverseArrangement = false;
        }

        protected override void OnValidate()
        {
            if (preWrapModeX != curveX.preWrapMode) { curveX.preWrapMode = preWrapModeX; SetDirty(); }
            if (postWrapModeX != curveX.postWrapMode) { curveX.postWrapMode = postWrapModeX; SetDirty(); }
            if (preWrapModeY != curveY.preWrapMode) { curveY.preWrapMode = preWrapModeY; SetDirty(); }
            if (postWrapModeY != curveY.postWrapMode) { curveY.postWrapMode = postWrapModeY; SetDirty(); }
            base.OnValidate();
        }

#endif
    }
}