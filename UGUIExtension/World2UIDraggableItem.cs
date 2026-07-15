using UnityEngine;
using UnityEngine.EventSystems;

namespace UIAssistant.Core
{
    /// <summary>
    /// 可拖拽组件（World -> UI）
    /// </summary>
    /// <remarks>
    /// <para>适用场景：挂载在带有物理碰撞体的 3D 场景物体上，通过主相机将 2D 屏幕拖拽投影回 3D 世界。</para>
    /// </remarks>
    [RequireComponent(typeof(Collider))]
    public class World2UIDraggableItem : BaseDraggableItem<Vector3>
    {
        private float m_ZOffset;

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (m_ReferenceCamera == null && eventData != null)
            {
                m_ReferenceCamera = eventData.pressEventCamera != null ? eventData.pressEventCamera : Camera.main;
            }

            if (m_ReferenceCamera != null)
            {
                Vector3 screenPoint = m_ReferenceCamera.WorldToScreenPoint(transform.position);
                m_ZOffset = screenPoint.z;
            }

            base.OnBeginDrag(eventData);
        }

        protected override Vector3 CaptureCurrentPosition()
        {
            return transform.position;
        }

        protected override void UpdatePosition(PointerEventData eventData)
        {
            if (m_ReferenceCamera == null) return;
            Vector3 screenPoint = new Vector3(eventData.position.x, eventData.position.y, m_ZOffset);
            Vector3 worldPoint = m_ReferenceCamera.ScreenToWorldPoint(screenPoint);
            transform.position = worldPoint;
        }
    }
}