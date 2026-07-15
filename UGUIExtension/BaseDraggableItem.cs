using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

namespace UIAssistant.Core
{
    /// <summary>
    /// 拖拽组件事件数据包装
    /// </summary>
    [Serializable]
    public class DraggableItemEvent : UnityEvent<PointerEventData> { }

    /// <summary>
    /// 可拖拽组件抽象基类
    /// </summary>
    /// <remarks>
    /// <para>功能：定义了拖拽组件的核心生命周期事件流与基础属性。</para>
    /// <para>设计：使用模板方法模式，将具体的坐标更新算法推迟到子类实现。</para>
    /// </remarks>
    [DisallowMultipleComponent]
    public abstract class BaseDraggableItem<TPosition> : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /// <summary>
        /// 起始位置备份
        /// </summary>
        public TPosition OriginalPosition => m_OriginalPosition;

        /// <summary>
        /// 参考相机
        /// </summary>
        public Camera ReferenceCamera
        {
            get => m_ReferenceCamera;
            set => m_ReferenceCamera = value;
        }

        /// <summary>
        /// 开始拖拽回调
        /// </summary>
        public DraggableItemEvent OnBeginDragEvent => m_OnBeginDragEvent;

        /// <summary>
        /// 拖拽中回调
        /// </summary>
        public DraggableItemEvent OnDraggingEvent => m_OnDraggingEvent;

        /// <summary>
        /// 结束拖拽回调
        /// </summary>
        public DraggableItemEvent OnEndDragEvent => m_OnEndDragEvent;

        [Tooltip("参考相机：用于处理不同坐标系之间的位置转换（留空则尝试使用事件相机）"), SerializeField]
        protected Camera m_ReferenceCamera;

        [Space, SerializeField] private DraggableItemEvent m_OnBeginDragEvent = new DraggableItemEvent();
        [Space, SerializeField] private DraggableItemEvent m_OnDraggingEvent = new DraggableItemEvent();
        [Space, SerializeField] private DraggableItemEvent m_OnEndDragEvent = new DraggableItemEvent();

        protected TPosition m_OriginalPosition;

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!isActiveAndEnabled) return;

            // 自动后备：如果未指定相机，尝试从当前事件系统中获取触发该事件的相机
            if (m_ReferenceCamera == null && eventData != null)
            {
                m_ReferenceCamera = eventData.pressEventCamera;
            }

            m_OriginalPosition = CaptureCurrentPosition();
            UpdatePosition(eventData);

            m_OnBeginDragEvent?.Invoke(eventData);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!isActiveAndEnabled) return;

            UpdatePosition(eventData);
            m_OnDraggingEvent?.Invoke(eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!isActiveAndEnabled) return;

            m_OnEndDragEvent?.Invoke(eventData);
        }

        /// <summary>
        /// 捕获当前的初始坐标（在 BeginDrag 时触发）
        /// </summary>
        protected abstract TPosition CaptureCurrentPosition();

        /// <summary>
        /// 核心位置更新算法
        /// </summary>
        protected abstract void UpdatePosition(PointerEventData eventData);
    }
}