using UnityEngine;
using UnityEngine.EventSystems;

namespace UIAssistant.Core
{
    /// <summary>
    /// 可拖拽组件（UI -> UI）
    /// </summary>
    /// <remarks>
    /// <para>适用场景：拖拽 UI 元素，并希望在父级 UGUI 容器的局部坐标（AnchoredPosition）下完美移动。</para>
    /// </remarks>
    [RequireComponent(typeof(RectTransform))]
    public class UI2UIDraggableItem : BaseDraggableItem<Vector2>
    {
        /// <summary>
        /// 矩形变换组件
        /// </summary>
        public RectTransform RectTransform
        {
            get
            {
                if (m_RectTransform == null) m_RectTransform = GetComponent<RectTransform>();
                return m_RectTransform;
            }
        }
        private RectTransform m_RectTransform;

        protected override Vector2 CaptureCurrentPosition()
        {
            return RectTransform.anchoredPosition;
        }

        protected override void UpdatePosition(PointerEventData eventData)
        {
            if (m_ReferenceCamera == null)
            {
                RectTransform.position = eventData.position;
                return;
            }

            if (RectTransform.parent is RectTransform parentRect)
            {
                if (UnityEngine.RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, m_ReferenceCamera, out Vector2 localPoint))
                {
                    RectTransform.anchoredPosition = localPoint;
                }
            }
        }
    }
}