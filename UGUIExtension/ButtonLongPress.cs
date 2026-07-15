using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIAssistant.Core
{
    /// <summary>
    /// 按钮长按组件
    /// </summary>
    /// <remarks>
    /// <para>功能：该组件挂载到按钮对象上，可以为常规 Button 组件集成长按功能。</para>
    /// <para>性能优化：采用基于 Update 的轮询计时器状态机，彻底消除协程（Coroutine）启动产生的 IEnumerator 堆内存分配（Zero-Allocation）。</para>
    /// <para>时间精度：采用 Time.unscaledTime 计时，不受 Time.timeScale 暂停影响，保证 UI 交互响应的鲁棒性。</para>
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public class ButtonLongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        /// <summary>
        /// 长按时间阈值(秒)
        /// </summary>
        public float Threshold
        {
            get => m_Threshold;
            set => m_Threshold = Mathf.Max(value, 0.001f);
        }
        [Tooltip("长按时间阈值(秒)"), Min(0.001f), SerializeField] private float m_Threshold = 0.5f;

        /// <summary>
        /// 长按时间间隔(秒)
        /// </summary>
        public float Interval
        {
            get => m_Interval;
            set => m_Interval = Mathf.Max(value, 0.001f);
        }
        [Tooltip("长按时间间隔(秒)"), Min(0.001f), SerializeField] private float m_Interval = 0.1f;

        /// <summary>
        /// 按下事件
        /// </summary>
        public UnityEvent OnPressDown => m_OnPressDown;
        [Space, SerializeField] private UnityEvent m_OnPressDown = new UnityEvent();

        /// <summary>
        /// 长按开始事件
        /// </summary>
        public UnityEvent OnLongPressBegin => m_OnLongPressBegin;
        [Space, SerializeField] private UnityEvent m_OnLongPressBegin = new UnityEvent();

        /// <summary>
        /// 长按中持续触发事件
        /// </summary>
        public UnityEvent OnLongPress => m_OnLongPress;
        [Space, SerializeField] private UnityEvent m_OnLongPress = new UnityEvent();

        /// <summary>
        /// 抬起事件
        /// </summary>
        public UnityEvent OnPressUp => m_OnPressUp;
        [Space, SerializeField] private UnityEvent m_OnPressUp = new UnityEvent();

        private bool m_IsDown;
        private bool m_IsLongPressedBegin;
        private float m_PressStartTime;
        private float m_NextTriggerTime;

        private Button Button
        {
            get
            {
                if (m_Button == null) m_Button = GetComponent<Button>();
                return m_Button;
            }
        }
        [NonSerialized] private Button m_Button;

        private void Update()
        {
            if (!m_IsDown || !isActiveAndEnabled) return;

            if (!Button.interactable)
            {
                ResetPressState();
                return;
            }

            float currentTime = Time.unscaledTime;
            float elapsed = currentTime - m_PressStartTime;

            if (elapsed >= m_Threshold)
            {
                if (!m_IsLongPressedBegin)
                {
                    m_IsLongPressedBegin = true;
                    m_OnLongPressBegin?.Invoke();
                    m_NextTriggerTime = currentTime + m_Interval;
                }

                if (currentTime >= m_NextTriggerTime)
                {
                    m_OnLongPress?.Invoke();
                    m_NextTriggerTime = currentTime + m_Interval;
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isActiveAndEnabled) return;

            m_IsDown = true;
            m_IsLongPressedBegin = false;
            m_PressStartTime = Time.unscaledTime;

            m_OnPressDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!m_IsDown) return;
            ResetPressState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!m_IsDown) return;
            ResetPressState();
        }

        private void OnDisable()
        {
            if (m_IsDown)
            {
                ResetPressState();
            }
        }

        private void ResetPressState()
        {
            m_IsDown = false;
            m_IsLongPressedBegin = false;
            m_OnPressUp?.Invoke();
        }
    }
}