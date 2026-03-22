using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUI.Layout.Extension
{
    /// <summary>
    /// 自动布局组件基类
    /// </summary>
    /// <remarks>
    /// <para>基于 UGUI 布局系统扩展的自动布局组件基类。</para>
    /// <para>基于该组件扩展的其它组件针对影响布局的修改应显式标记为脏以触发布局系统重新计算。</para>
    /// </remarks>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseAutoLayoutGroup : UIBehaviour, ILayoutElement, ILayoutGroup, ILayoutController
    {
        public virtual float minWidth => GetTotalMinSize(0);
        public virtual float minHeight => GetTotalMinSize(1);
        public virtual float preferredWidth => GetTotalPreferredSize(0);
        public virtual float preferredHeight => GetTotalPreferredSize(1);
        public virtual float flexibleWidth => GetTotalFlexibleSize(0);
        public virtual float flexibleHeight => GetTotalFlexibleSize(1);
        public virtual int layoutPriority => 0;

        public RectOffset Padding
        {
            get => padding;
            set => SetProperty(ref padding, value);
        }
        [SerializeField] protected RectOffset padding = new RectOffset();

        /// <summary>
        /// 布局元素在容器内的对齐方式
        /// </summary>
        /// <remarks>
        /// 当容器尺寸大于所有布局元素的总占用空间时，通过对齐方式决定内容整体
        /// 在容器内的位置（靠左/居中/靠右，靠上/居中/靠下）。
        /// </remarks>
        public TextAnchor ChildAlignment
        {
            get => childAlignment;
            set => SetProperty(ref childAlignment, value);
        }
        [Tooltip("布局元素在容器内的对齐方式"), SerializeField]
        private TextAnchor childAlignment = TextAnchor.UpperLeft;

        protected RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null)
                    rectTransform = GetComponent<RectTransform>();
                return rectTransform;
            }
        }
        [NonSerialized] private RectTransform rectTransform;

        protected List<RectTransform> RectChildren => rectChildren;
        [NonSerialized] private readonly List<RectTransform> rectChildren = new List<RectTransform>();

        private bool IsRootLayoutGroup
        {
            get
            {
                Transform parent = transform.parent;
                if (parent == null) return true;
                return transform.parent.GetComponent(typeof(ILayoutGroup)) == null;
            }
        }

        protected DrivenRectTransformTracker tracker;

        [NonSerialized] private Vector2 totalMinSize = Vector2.zero;
        [NonSerialized] private Vector2 totalPreferredSize = Vector2.zero;
        [NonSerialized] private Vector2 totalFlexibleSize = Vector2.zero;

        public virtual void CalculateLayoutInputHorizontal()
        {
            rectChildren.Clear();

            List<Component> list = ListPool<Component>.Get();
            try
            {
                for (int i = 0; i < RectTransform.childCount; i++)
                {
                    RectTransform child = RectTransform.GetChild(i) as RectTransform;
                    if (child == null || !child.gameObject.activeInHierarchy)
                        continue;

                    child.GetComponents(typeof(ILayoutIgnorer), list);

                    if (list.Count == 0)
                    {
                        rectChildren.Add(child);
                        continue;
                    }

                    for (int j = 0; j < list.Count; j++)
                    {
                        ILayoutIgnorer ignorer = (ILayoutIgnorer)list[j];
                        if (!ignorer.ignoreLayout)
                        {
                            rectChildren.Add(child);
                            break;
                        }
                    }
                }
            }
            finally
            {
                ListPool<Component>.Release(list);
            }

            tracker.Clear();
        }

        public abstract void CalculateLayoutInputVertical();
        public abstract void SetLayoutHorizontal();
        public abstract void SetLayoutVertical();

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
            base.OnDisable();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            SetDirty();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (IsRootLayoutGroup)
                SetDirty();
        }

        protected override void OnValidate()
        {
            SetDirty();
        }

        protected virtual void OnTransformChildrenChanged()
        {
            SetDirty();
        }

        protected float GetTotalMinSize(int axis) => totalMinSize[axis];
        protected float GetTotalPreferredSize(int axis) => totalPreferredSize[axis];
        protected float GetTotalFlexibleSize(int axis) => totalFlexibleSize[axis];

        protected void SetLayoutInputForAxis(float totalMin, float totalPreferred, float totalFlexible, int axis)
        {
            totalMinSize[axis] = totalMin;
            totalPreferredSize[axis] = totalPreferred;
            totalFlexibleSize[axis] = totalFlexible;
        }

        /// <summary>
        /// 获取指定轴上的对齐系数
        /// </summary>
        /// <param name="axis">0 = 水平，1 = 垂直</param>
        /// <returns>0 = 靠前，0.5 = 居中，1 = 靠后</returns>
        protected float GetAlignmentOnAxis(int axis)
        {
            if (axis == 0)
                return (int)childAlignment % 3 * 0.5f;
            else
                return (int)childAlignment / 3 * 0.5f;
        }

        /// <summary>
        /// 获取布局内容在容器内的起始坐标
        /// </summary>
        /// <param name="axis">0 = 水平，1 = 垂直</param>
        /// <param name="requiredSpaceWithoutPadding">不含 padding 的内容占用空间</param>
        /// <returns>内容左/上边缘距容器左/上边缘的像素距离</returns>
        /// <remarks>
        /// 当容器有剩余空间时，通过 <see cref="ChildAlignment"/> 决定内容整体的对齐位置。
        /// 当内容超出容器时（surplusSpace 为负），内容从 startPadding 处开始，对齐不生效。
        /// </remarks>
        protected float GetStartOffset(int axis, float requiredSpaceWithoutPadding)
        {
            float startPadding = axis == 0 ? padding.left : padding.top;
            float endPadding = axis == 0 ? padding.right : padding.bottom;
            float requiredSpace = requiredSpaceWithoutPadding + startPadding + endPadding;
            float availableSpace = RectTransform.rect.size[axis];
            float surplusSpace = availableSpace - requiredSpace;
            float alignmentOnAxis = GetAlignmentOnAxis(axis);
            return startPadding + surplusSpace * alignmentOnAxis;
        }

        /// <summary>
        /// 沿指定轴设置布局元素的左/上边缘位置，保持布局元素当前尺寸不变。
        /// </summary>
        /// <remarks>
        /// 会同时驱动子节点的 Anchors，确保 anchor 归一化到左上角（与官方 LayoutGroup 行为一致），
        /// 避免非标准 anchor 导致 <see cref="RectTransform.SetInsetAndSizeFromParentEdge"/> 计算偏差。
        /// </remarks>
        protected void SetChildAlongAxis(RectTransform rect, int axis, float pos)
        {
            if (rect == null) return;

            tracker.Add(this, rect, axis == 0
                ? DrivenTransformProperties.AnchoredPositionX | DrivenTransformProperties.AnchorMinX | DrivenTransformProperties.AnchorMaxX
                : DrivenTransformProperties.AnchoredPositionY | DrivenTransformProperties.AnchorMinY | DrivenTransformProperties.AnchorMaxY);

            // 将该轴的 anchor 归一化到左/上（0），使 SetInsetAndSizeFromParentEdge 的计算基准一致
            Vector2 anchorMin = rect.anchorMin;
            Vector2 anchorMax = rect.anchorMax;
            anchorMin[axis] = 0f;
            anchorMax[axis] = 0f;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;

            rect.SetInsetAndSizeFromParentEdge(
                axis == 0 ? RectTransform.Edge.Left : RectTransform.Edge.Top,
                pos,
                rect.sizeDelta[axis]);
        }

        /// <summary>
        /// 沿指定轴设置布局元素的左/上边缘位置，同时驱动布局元素尺寸。
        /// </summary>
        /// <remarks>
        /// 会同时驱动子节点的 Anchors，确保 anchor 归一化到左上角（与官方 LayoutGroup 行为一致），
        /// 避免非标准 anchor 导致 <see cref="RectTransform.SetInsetAndSizeFromParentEdge"/> 计算偏差。
        /// </remarks>
        protected void SetChildAlongAxis(RectTransform rect, int axis, float pos, float size)
        {
            if (rect == null) return;

            tracker.Add(this, rect, axis == 0
                ? DrivenTransformProperties.AnchoredPositionX | DrivenTransformProperties.SizeDeltaX | DrivenTransformProperties.AnchorMinX | DrivenTransformProperties.AnchorMaxX
                : DrivenTransformProperties.AnchoredPositionY | DrivenTransformProperties.SizeDeltaY | DrivenTransformProperties.AnchorMinY | DrivenTransformProperties.AnchorMaxY);

            // 将该轴的 anchor 归一化到左/上（0），使 SetInsetAndSizeFromParentEdge 的计算基准一致
            Vector2 anchorMin = rect.anchorMin;
            Vector2 anchorMax = rect.anchorMax;
            anchorMin[axis] = 0f;
            anchorMax[axis] = 0f;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;

            rect.SetInsetAndSizeFromParentEdge(
                axis == 0 ? RectTransform.Edge.Left : RectTransform.Edge.Top,
                pos,
                size);
        }

        protected void SetProperty<T>(ref T currentValue, T newValue)
        {
            if (currentValue == null && newValue == null)
                return;

            if (currentValue != null && currentValue.Equals((object)newValue))
                return;

            currentValue = newValue;
            SetDirty();
        }

        protected void SetDirty()
        {
            if (!IsActive()) return;

            if (!CanvasUpdateRegistry.IsRebuildingLayout())
                LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
            else
                StartCoroutine(DelayedSetDirty(RectTransform));
        }

        private IEnumerator DelayedSetDirty(RectTransform rectTransform)
        {
            yield return null;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

#if UNITY_EDITOR

        /// <summary>
        /// 编辑器中触发布局刷新
        /// </summary>
        public virtual void RebuildLayout() => SetDirty();

#endif
    }
}