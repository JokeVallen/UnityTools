using UnityEngine;

namespace Timer
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class Extension
    {
        /// <summary>
        /// 取消计时事件
        /// </summary>
        /// <param name="handle"></param>
        public static void Cancel(this in TimerHandle handle)
        {
            if (handle.IsNull) return;
            InnerTimer.Instance.Cancel(handle);
        }

        /// <summary>
        /// 暂停计时事件
        /// </summary>
        /// <param name="handle"></param>
        public static void Pause(this in TimerHandle handle)
        {
            if (handle.IsNull) return;
            InnerTimer.Instance.SetPaused(handle, true);
        }

        /// <summary>
        /// 恢复计时事件
        /// </summary>
        /// <param name="handle"></param>
        public static void Resume(this in TimerHandle handle)
        {
            if (handle.IsNull) return;
            InnerTimer.Instance.SetPaused(handle, false);
        }

        /// <summary>
        /// 设置暂停状态
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="isPaused">是否暂停</param>
        public static void SetPaused(this in TimerHandle handle, bool isPaused)
        {
            if (handle.IsNull) return;
            InnerTimer.Instance.SetPaused(handle, isPaused);
        }

        /// <summary>
        /// 获取活动状态
        /// </summary>
        /// <param name="handle"></param>
        /// <returns>活动状态返回 true，否则返回 false。</returns>
        public static bool IsActive(this in TimerHandle handle)
        {
            if (handle.IsNull) return false;
            return InnerTimer.Instance.IsActive(handle);
        }

        /// <summary>
        /// 尝试获取剩余计时
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="remaining">接收变量</param>
        /// <returns>获取成功返回 true，否则返回 false。</returns>
        public static bool TryGetTimeRemaining(this in TimerHandle handle, out float remaining)
        {
            remaining = 0f;
            if (handle.IsNull) return false;
            return InnerTimer.Instance.TryGetTimeRemaining(handle, out remaining);
        }

        /// <summary>
        /// 尝试获取进度
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="progress">接收变量</param>
        /// <returns>获取成功返回 true，否则返回 false。</returns>
        /// <remarks>
        /// <para>对帧驱动类型（MonoUpdate 等），进度以帧数为单位计算，每帧固定步进，与实际时间无关。</para>
        /// </remarks>
        public static bool TryGetProgress(this in TimerHandle handle, out float progress)
        {
            progress = 0f;
            if (handle.IsNull) return false;
            return InnerTimer.Instance.TryGetProgress(handle, out progress);
        }

        /// <summary>
        /// 重置计时
        /// </summary>
        /// <param name="handle"></param>
        /// <returns>重置成功返回 true，否则返回 false。</returns>
        public static bool Reset(this in TimerHandle handle)
        {
            if (handle.IsNull) return false;
            return InnerTimer.Instance.Reset(handle);
        }

        /// <summary>
        /// 设置计时间隔
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="interval">间隔</param>
        /// <returns>设置成功返回 true，否则返回 false。</returns>
        public static bool SetInterval(this in TimerHandle handle, float interval)
        {
            if (handle.IsNull) return false;
            return InnerTimer.Instance.SetInterval(handle, interval);
        }

        /// <summary>
        /// 尝试获取计时器所属的组ID
        /// </summary>
        public static bool TryGetGroupId(this in TimerHandle handle, out int groupId)
        {
            if (handle.IsNull)
            {
                groupId = 0;
                return false;
            }
            return InnerTimer.Instance.TryGetGroupId(handle, out groupId);
        }

        /// <summary>
        /// 尝试获取计时间隔
        /// </summary>
        public static bool TryGetInterval(this in TimerHandle handle, out float interval)
        {
            if (handle.IsNull)
            {
                interval = 0f;
                return false;
            }
            return InnerTimer.Instance.TryGetInterval(handle, out interval);
        }

        /// <summary>
        /// 尝试获取是否为循环计时
        /// </summary>
        public static bool TryGetIsLoop(this in TimerHandle handle, out bool isLoop)
        {
            if (handle.IsNull)
            {
                isLoop = false;
                return false;
            }
            return InnerTimer.Instance.TryGetIsLoop(handle, out isLoop);
        }

        /// <summary>
        /// 设置循环标志
        /// </summary>
        /// <returns>设置成功返回 true，否则返回 false（句柄无效或已失效）</returns>
        public static bool SetLoop(this in TimerHandle handle, bool loop)
        {
            if (handle.IsNull) return false;
            return InnerTimer.Instance.SetLoop(handle, loop);
        }

        /// <summary>
        /// 尝试获取剩余帧数
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="frames">接收变量</param>
        /// <returns>如果句柄有效且为帧驱动类型返回 true，否则返回 false。</returns>
        /// <remarks>
        /// <para>仅对帧驱动类型有效。</para>
        /// </remarks>
        public static bool TryGetFramesRemaining(this in TimerHandle handle, out int frames)
        {
            frames = 0;
            if (handle.IsNull) return false;
            if (!InnerTimer.Instance.TryGetFramesRemaining(handle, out var rawFrames)) return false;
            frames = Mathf.CeilToInt(rawFrames);
            return true;
        }
    }
}