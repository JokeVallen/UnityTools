using UnityEngine;
using UnityEngine.UI;

namespace UGUI.Layout.Extension
{
    /// <summary>
    /// 圆形布局组件
    /// </summary>
    /// <remarks>
    /// <para>基于 UGUI 布局系统和 <see cref="LayoutGroup"/> 扩展的圆形布局组件，可以设置圆的半径、旋转和是否顺时针排序。</para>
    /// </remarks>
    [AddComponentMenu("Layout/CircleLayoutGroup")]
    public sealed class CircleLayoutGroup : LayoutGroup
    {
        /// <summary>
        /// 半径
        /// </summary>
        public float Radius
        {
            get => radius;
            set
            {
                if (value < 0) return;
                SetProperty(ref radius, value);
            }
        }
        [Tooltip("圆形的半径"), SerializeField] private float radius;

        /// <summary>
        /// 起始旋转角度（度）
        /// </summary>
        /// <remarks>
        /// <para>支持任意整数值，自动归一化到 [0, 360)。</para>
        /// </remarks>
        public int Rotation
        {
            get => rotation;
            set => SetProperty(ref rotation, ((value % 360) + 360) % 360);
        }
        [Tooltip("起始旋转角度（度），支持任意整数值"), SerializeField] private int rotation;

        /// <summary>
        /// 是否顺时针布局
        /// </summary>
        public bool ClockWise
        {
            get => clockWise;
            set => SetProperty(ref clockWise, value);
        }
        [Tooltip("是否顺时针布局"), SerializeField] private bool clockWise;

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
            float combinedPadding = axis == 0 ? padding.horizontal : padding.vertical;
            float size = combinedPadding + radius * 2;
            SetLayoutInputForAxis(size, size, 0, axis);
        }

        private void DoSetChildren(int axis)
        {
            int count = rectChildren.Count;
            if (count == 0) return;

            float containerSize = axis == 0 ? rectTransform.rect.width : rectTransform.rect.height;
            float centerPos = containerSize * 0.5f;

            float angleDelta = 360f / count;
            if (!clockWise) angleDelta = -angleDelta;
            float startAngle = rotation;

            for (int i = 0; i < count; i++)
            {
                RectTransform child = rectChildren[i];
                float childSize = child.rect.size[axis];
                float offset = GetCircleOffset(axis, startAngle + i * angleDelta, radius);
                SetChildAlongAxis(child, axis, centerPos + offset - childSize * 0.5f);
            }
        }

        private static float GetCircleOffset(int axis, float angleDeg, float radius)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            return radius * (axis == 0 ? Mathf.Cos(rad) : Mathf.Sin(rad));
        }

#if UNITY_EDITOR

        /// <summary>
        /// 编辑器中触发布局刷新
        /// </summary>
        public void RebuildLayout() => SetDirty();

        protected override void Reset()
        {
            base.Reset();
            radius = 0;
            rotation = 0;
            clockWise = false;
        }

#endif
    }
}