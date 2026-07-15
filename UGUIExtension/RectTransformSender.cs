using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UIAssistant.Core
{
    /// <summary>
    /// RectTransform 组件状态发射器
    /// </summary>
    /// <remarks>
    /// <para>功能：挂载到 RectTransform 对象上，用于实时侦听并向外发射位置（AnchoredPosition）与尺寸（Rect）的变化事件。</para>
    /// <para>性能优化：采用按需轮询策略，仅在存在有效事件监听者时激活 LateUpdate 监测，大幅减少无意义的每帧 CPU 耗时。</para>
    /// </remarks>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformSender : UIBehaviour
    {
        /// <summary>
        /// 三维向量变更事件通道
        /// </summary>
        [Serializable]
        public class VectorThreeEvent : UnityEvent<Vector3, Vector3> { }

        /// <summary>
        /// 矩形区域变更事件通道
        /// </summary>
        [Serializable]
        public class RectEvent : UnityEvent<Rect, Rect> { }

        /// <summary>
        /// AnchoredPosition 更改回调
        /// </summary>
        public VectorThreeEvent OnAnchoredPositionChanged => m_OnAnchoredPositionChanged;
        [SerializeField] private VectorThreeEvent m_OnAnchoredPositionChanged = new VectorThreeEvent();

        /// <summary>
        /// Rect 尺寸更改回调
        /// </summary>
        public RectEvent OnRectChanged => m_OnRectChanged;
        [SerializeField] private RectEvent m_OnRectChanged = new RectEvent();

        [SerializeField] private Vector3 m_BackupAnchoredPosition;
        [SerializeField] private Rect m_BackupRect;

        /// <summary>
        /// 缓存的 RectTransform 引用
        /// </summary>
        public RectTransform RectTransform
        {
            get
            {
                if (m_RectTransform == null) m_RectTransform = GetComponent<RectTransform>();
                return m_RectTransform;
            }
        }
        [NonSerialized] private RectTransform m_RectTransform;

        private bool m_HasPositionListeners;

        /// <summary>
        /// 设置锚点坐标并主动触发变更事件
        /// </summary>
        /// <param name="anchoredPosition">新的三维锚点坐标</param>
        public void SetAnchoredPosition(Vector3 anchoredPosition)
        {
            if (anchoredPosition != m_BackupAnchoredPosition)
            {
                RectTransform.anchoredPosition3D = anchoredPosition;
                if (isActiveAndEnabled)
                {
                    m_OnAnchoredPositionChanged?.Invoke(m_BackupAnchoredPosition, anchoredPosition);
                }
                m_BackupAnchoredPosition = anchoredPosition;
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled) return;
            var rect = RectTransform.rect;
            if (rect != m_BackupRect)
            {
                m_OnRectChanged?.Invoke(m_BackupRect, rect);
                m_BackupRect = rect;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_BackupAnchoredPosition = RectTransform.anchoredPosition3D;
            m_BackupRect = RectTransform.rect;
            UpdateListenerState();
            OnRectTransformDimensionsChange();
        }

        private void LateUpdate()
        {
            if (!m_HasPositionListeners || !isActiveAndEnabled) return;

            var currentPos = RectTransform.anchoredPosition3D;
            if (currentPos.x != m_BackupAnchoredPosition.x ||
                currentPos.y != m_BackupAnchoredPosition.y ||
                currentPos.z != m_BackupAnchoredPosition.z)
            {
                m_OnAnchoredPositionChanged?.Invoke(m_BackupAnchoredPosition, currentPos);
                m_BackupAnchoredPosition = currentPos;
            }
        }

        /// <summary>
        /// 刷新监听者状态
        /// </summary>
        /// <remarks>
        /// 当外部动态添加或移除监听事件时，应调用此方法刷新发射器激活状态。
        /// </remarks>
        public void UpdateListenerState()
        {
            m_HasPositionListeners = m_OnAnchoredPositionChanged != null &&
                                     m_OnAnchoredPositionChanged.GetPersistentEventCount() > 0;

            if (Application.isPlaying && !m_HasPositionListeners && m_OnAnchoredPositionChanged != null)
            {
                m_HasPositionListeners = true;
            }
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            if (RectTransform != null)
            {
                m_BackupAnchoredPosition = RectTransform.anchoredPosition3D;
                m_BackupRect = RectTransform.rect;
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            UpdateListenerState();
        }
#endif
    }
}