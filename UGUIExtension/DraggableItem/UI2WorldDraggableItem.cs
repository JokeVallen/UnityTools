using UnityEngine;
using UnityEngine.EventSystems;

namespace UIAssistant.Core
{
    /// <summary>
    /// 可拖拽组件（UI -> World）
    /// </summary>
    /// <remarks>
    /// <para>适用场景：拖拽 Canvas 上的 UI 元素，并基于世界空间相机进行移动定位。</para>
    /// </remarks>
    [RequireComponent(typeof(RectTransform))]
    public class UI2WorldDraggableItem : BaseDraggableItem<Vector3>
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

        protected override Vector3 CaptureCurrentPosition()
        {
            return RectTransform.position;
        }

        protected override void UpdatePosition(PointerEventData eventData)
        {
            if (m_ReferenceCamera == null)
            {
                RectTransform.position = eventData.position;
                return;
            }

            if (UnityEngine.RectTransformUtility.ScreenPointToWorldPointInRectangle(RectTransform, eventData.position, m_ReferenceCamera, out Vector3 worldPoint))
            {
                RectTransform.position = worldPoint;
            }
        }
    }
}