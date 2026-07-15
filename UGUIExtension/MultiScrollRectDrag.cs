using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using System;

namespace UIAssistant.Core
{
    /// <summary>
    /// 多滚动视图拖拽组件
    /// </summary>
    /// <remarks>
    /// <para>功能：用于指定该脚本所挂载对象的 ScrollRect 与重叠的 ScrollRect 之间的处理。</para>
    /// <para>检测方向：检测拖拽的方向，分为横向拖拽和纵向拖拽。</para>
    /// <para>拖拽角度阈值：x 和 y 表示角度范围，横向拖拽与 Vector2.up 计算夹角，纵向拖拽与 Vector2.right 计算夹角，在阈值范围内则被认为拖拽有效。</para>
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ScrollRect))]
    public class MultiScrollRectDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /// <summary>
        /// 拖拽方向
        /// </summary>
        public enum DragDirection : ushort
        {
            None = 0,
            Horizontal,
            Vertical
        }

        /// <summary>
        /// 检测的拖拽方向
        /// </summary>
        [Tooltip("检测的拖拽方向")]
        public DragDirection direction;

        /// <summary>
        /// 拖拽角度阈值
        /// </summary>
        /// <remarks>当拖拽角度在阈值范围内时被视为水平或垂直拖拽,范围[0,180],x不可大于y</remarks>
        public Vector2 DragThreshold
        {
            get => m_DragThreshold;
            set
            {
                m_DragThreshold = value;
                RegulateDragThreshold();
            }
        }
        [Tooltip("拖拽角度阈值：当拖拽角度在阈值范围内时被视为水平或垂直拖拽,范围[0,180],x不可大于y"), SerializeField]
        private Vector2 m_DragThreshold = new Vector2(45, 135);

        /// <summary>
        /// 重叠的滚动视图
        /// </summary>
        public ScrollRect OverlappingScrollRect
        {
            get
            {
                return m_OverlappingScrollRect;
            }
            set
            {
                SetOverlappingRect(value);
            }
        }
        [Tooltip("重叠的滚动视图"), SerializeField] private ScrollRect m_OverlappingScrollRect;

        /// <summary>
        /// 当前对象的滚动视图
        /// </summary>
        public ScrollRect ScrollRect
        {
            get
            {
                if (m_ScrollRect == null) m_ScrollRect = GetComponent<ScrollRect>();
                return m_ScrollRect;
            }
        }
        [NonSerialized] private ScrollRect m_ScrollRect;

        private bool m_IsOverlappingHorizontal;
        private bool m_IsOverlappingVertical;
        private bool m_IsDraggingSelf;
        private bool m_IsDraggingOverlapping;
        private bool m_DirectionEvaluated;

        private void Awake()
        {
            if (m_OverlappingScrollRect != null)
            {
                CacheOverlappingStates();
            }
        }

        private void OnEnable()
        {

        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isActiveAndEnabled || direction == DragDirection.None || OverlappingScrollRect == null) return;
            m_IsDraggingSelf = false;
            m_IsDraggingOverlapping = false;
            m_DirectionEvaluated = false;
            CacheOverlappingStates();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isActiveAndEnabled || direction == DragDirection.None || OverlappingScrollRect == null) return;
            if (!m_DirectionEvaluated)
            {
                Vector2 totalDelta = eventData.position - eventData.pressPosition;
                if (totalDelta.sqrMagnitude < 9f) return;
                EvaluateDragTarget(totalDelta, eventData);
                m_DirectionEvaluated = true;
            }

            if (m_IsDraggingSelf)
            {
                ScrollRect.OnDrag(eventData);
            }
            else if (m_IsDraggingOverlapping)
            {
                OverlappingScrollRect.OnDrag(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isActiveAndEnabled || direction == DragDirection.None || OverlappingScrollRect == null) return;
            if (m_IsDraggingSelf)
            {
                ScrollRect.OnEndDrag(eventData);
            }
            else if (m_IsDraggingOverlapping)
            {
                OverlappingScrollRect.OnEndDrag(eventData);
            }

            OverlappingScrollRect.horizontal = m_IsOverlappingHorizontal;
            OverlappingScrollRect.vertical = m_IsOverlappingVertical;
            m_IsDraggingSelf = false;
            m_IsDraggingOverlapping = false;
            m_DirectionEvaluated = false;
        }

        private void EvaluateDragTarget(Vector2 totalDelta, PointerEventData eventData)
        {
            float dragAngle = Vector2.Angle(totalDelta, direction == DragDirection.Horizontal ? Vector2.up : Vector2.right);
            bool isValidDrag = dragAngle > m_DragThreshold.x && dragAngle < m_DragThreshold.y;

            if (direction == DragDirection.Horizontal)
            {
                RouteDrag(isValidDrag, eventData, ScrollRect.horizontal, OverlappingScrollRect.vertical);
            }
            else
            {
                RouteDrag(isValidDrag, eventData, ScrollRect.vertical, OverlappingScrollRect.horizontal);
            }
        }

        private void RouteDrag(bool isPrimaryDirection, PointerEventData eventData, bool selfCanScroll, bool overlapCanScroll)
        {
            if (isPrimaryDirection)
            {
                if (selfCanScroll)
                {
                    m_IsDraggingSelf = true;
                    SetOverlappingAxisActive(!direction.Equals(DragDirection.Horizontal));
                    ScrollRect.OnBeginDrag(eventData);
                }
                else
                {
                    m_IsDraggingOverlapping = true;
                    OverlappingScrollRect.OnBeginDrag(eventData);
                }
            }
            else
            {
                if (overlapCanScroll)
                {
                    m_IsDraggingOverlapping = true;
                    OverlappingScrollRect.OnBeginDrag(eventData);
                }
                else
                {
                    m_IsDraggingSelf = true;
                    ScrollRect.OnBeginDrag(eventData);
                }
            }
        }

        private void SetOverlappingAxisActive(bool horizontal)
        {
            if (horizontal)
            {
                OverlappingScrollRect.vertical = false;
            }
            else
            {
                OverlappingScrollRect.horizontal = false;
            }
        }

        private void CacheOverlappingStates()
        {
            m_IsOverlappingHorizontal = m_OverlappingScrollRect.horizontal;
            m_IsOverlappingVertical = m_OverlappingScrollRect.vertical;
        }

        private void RegulateDragThreshold()
        {
            m_DragThreshold.x = Mathf.Clamp(m_DragThreshold.x, 0, 180);
            m_DragThreshold.y = Mathf.Clamp(m_DragThreshold.y, 0, 180);
            if (m_DragThreshold.x > m_DragThreshold.y) m_DragThreshold.x = m_DragThreshold.y;
        }

        private void SetOverlappingRect(ScrollRect value)
        {
            m_OverlappingScrollRect = value;
            if (m_OverlappingScrollRect != null)
            {
                CacheOverlappingStates();
            }
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            RegulateDragThreshold();
        }

#endif
    }
}