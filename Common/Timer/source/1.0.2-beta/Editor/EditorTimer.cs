#if UNITY_EDITOR

using System;

namespace Timer
{
    /// <summary>
    /// 编辑器全局计时器
    /// </summary>
    public static class EditorTimer
    {
        /// <summary>
        /// 注册受时间缩放影响的计时事件
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">事件回调</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        public static EditorTimerHandle RegisterScaled(TimeSpan interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
        {
            return Register((float)interval.TotalSeconds, callback, TimeSource.ScaledUpdate, loop, groupID);
        }

        /// <summary>
        /// 注册受时间缩放影响的计时事件
        /// </summary>
        /// <param name="interval">时间间隔（单位：秒）</param>
        /// <param name="callback">事件回调</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        public static EditorTimerHandle RegisterScaled(float interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
        {
            return Register(interval, callback, TimeSource.ScaledUpdate, loop, groupID);
        }

        /// <summary>
        /// 注册真实物理时间的计时事件
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">事件回调</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        public static EditorTimerHandle RegisterUnscaled(TimeSpan interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
        {
            return Register((float)interval.TotalSeconds, callback, TimeSource.UnscaledUpdate, loop, groupID);
        }

        /// <summary>
        /// 注册真实物理时间的计时事件
        /// </summary>
        /// <param name="interval">时间间隔（单位：秒）</param>
        /// <param name="callback">事件回调</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        public static EditorTimerHandle RegisterUnscaled(float interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
        {
            return Register(interval, callback, TimeSource.UnscaledUpdate, loop, groupID);
        }

        /// <summary>
        /// 注册帧数级计时事件
        /// </summary>
        /// <param name="frameCount">帧数</param>
        /// <param name="callback">事件回调</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="frameCount"/> 不可为负数</exception>
        public static EditorTimerHandle RegisterFrame(int frameCount, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
        {
            if (frameCount < 0) throw new ArgumentOutOfRangeException(nameof(frameCount));
            return Register(frameCount, callback, TimeSource.FrameUpdate, loop, groupID);
        }

        /// <summary>
        /// 自定义组合注册
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">计时回调</param>
        /// <param name="delta">原子级增量计算方式</param>
        /// <param name="schedule">原子级驱动调度时机</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <param name="customScale">自定义倍率，默认 1.0</param>
        /// <returns>句柄</returns>
        public static EditorTimerHandle Register(TimeSpan interval, Action callback, TimeDelta delta, TimeSchedule schedule, Optional<bool> loop = default,
        Optional<int> groupID = default, Optional<float> customScale = default)
        {
            if (schedule != TimeSchedule.Update && schedule != TimeSchedule.Manual)
                throw new NotSupportedException($"[GlobalTimer] EditorTimer only supports {nameof(TimeSchedule.Update)} and {nameof(TimeSchedule.Manual)}. '{schedule}' is not available in editor mode.");
            var timeSource = new TimeSource(delta, schedule, customScale.HasValue ? customScale.Value : 1f);
            return Register((float)interval.TotalSeconds, callback, timeSource, loop, groupID);
        }

        /// <summary>
        /// 自定义组合注册
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">计时回调</param>
        /// <param name="delta">原子级增量计算方式</param>
        /// <param name="schedule">原子级驱动调度时机</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <param name="customScale">自定义倍率，默认 1.0</param>
        /// <returns>句柄</returns>
        public static EditorTimerHandle Register(float interval, Action callback, TimeDelta delta, TimeSchedule schedule, Optional<bool> loop = default,
        Optional<int> groupID = default, Optional<float> customScale = default)
        {
            if (schedule != TimeSchedule.Update && schedule != TimeSchedule.Manual)
                throw new NotSupportedException($"[GlobalTimer] EditorTimer only supports {nameof(TimeSchedule.Update)} and {nameof(TimeSchedule.Manual)}. '{schedule}' is not available in editor mode.");
            var timeSource = new TimeSource(delta, schedule, customScale.HasValue ? customScale.Value : 1f);
            return Register(interval, callback, timeSource, loop, groupID);
        }

        /// <summary>
        /// 注册独立缩放计时
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">计时回调</param>
        /// <param name="customScale">自定义倍率</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        public static EditorTimerHandle RegisterIndependent(TimeSpan interval, Action callback, float customScale, Optional<bool> loop = default, Optional<int> groupID = default)
        {
            return Register((float)interval.TotalSeconds, callback, TimeSource.Independent(customScale), loop, groupID);
        }

        /// <summary>
        /// 注册独立缩放计时
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">计时回调</param>
        /// <param name="customScale">自定义倍率</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        public static EditorTimerHandle RegisterIndependent(float interval, Action callback, float customScale, Optional<bool> loop = default, Optional<int> groupID = default)
        {
            return Register(interval, callback, TimeSource.Independent(customScale), loop, groupID);
        }

        /// <summary>
        /// 注册独立缩放的帧驱动计时器
        /// </summary>
        /// <param name="frameCount">帧数间隔</param>
        /// <param name="callback">计时回调</param>
        /// <param name="customScale">自定义倍率</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        public static EditorTimerHandle RegisterIndependentFrame(int frameCount, Action callback, float customScale, Optional<bool> loop = default, Optional<int> groupID = default)
        {
            if (frameCount < 0) throw new ArgumentOutOfRangeException(nameof(frameCount));
            return Register(frameCount, callback, new TimeSource(TimeDelta.Frame, TimeSchedule.Update, customScale), loop, groupID);
        }

        /// <summary>
        /// 注册挂钟计时
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">计时回调</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        public static EditorTimerHandle RegisterWallClock(TimeSpan interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
        {
            return Register((float)interval.TotalSeconds, callback, TimeSource.WallClockUpdate, loop, groupID);
        }

        /// <summary>
        /// 注册挂钟计时
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">计时回调</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        public static EditorTimerHandle RegisterWallClock(float interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
        {
            return Register(interval, callback, TimeSource.WallClockUpdate, loop, groupID);
        }

        /// <summary>
        /// 注册手动驱动计时
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">计时回调</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        public static EditorTimerHandle RegisterManual(TimeSpan interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
        {
            return Register((float)interval.TotalSeconds, callback, TimeSource.ManualSource, loop, groupID);
        }

        /// <summary>
        /// 注册手动驱动计时
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">计时回调</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        public static EditorTimerHandle RegisterManual(float interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
        {
            return Register(interval, callback, TimeSource.ManualSource, loop, groupID);
        }

        /// <summary>
        /// 手动驱动刷新
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public static void ManualUpdate(float deltaTime)
        {
            InnerEditorTimer.Instance.ManualUpdate(deltaTime);
        }

        /// <summary>
        /// 取消某组计时任务
        /// </summary>
        /// <param name="groupId">组ID</param>
        public static void CancelGroup(int groupId)
        {
            InnerEditorTimer.Instance.CancelGroup(groupId);
        }

        /// <summary>
        /// 暂停某组计时任务
        /// </summary>
        /// <param name="groupId">组ID</param>
        public static void PauseGroup(int groupId)
        {
            InnerEditorTimer.Instance.SetGroupPaused(groupId, true);
        }

        /// <summary>
        /// 恢复某组计时任务
        /// </summary>
        /// <param name="groupId">组ID</param>
        public static void ResumeGroup(int groupId)
        {
            InnerEditorTimer.Instance.SetGroupPaused(groupId, false);
        }

        /// <summary>
        /// 设置某组计时任务的暂停状态
        /// </summary>
        /// <param name="groupId">组ID</param>
        /// <param name="isPaused">是否暂停</param>
        public static void SetGroupPaused(int groupId, bool isPaused)
        {
            InnerEditorTimer.Instance.SetGroupPaused(groupId, isPaused);
        }

        /// <summary>
        /// 取消所有正在运行的计时任务（同时丢弃尚未执行的到期回调）
        /// </summary>
        public static void CancelAll()
        {
            InnerEditorTimer.Instance.CancelAll();
        }

        private static EditorTimerHandle Register(float interval, Action callback, TimeSource source, Optional<bool> loop, Optional<int> groupID)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            return InnerEditorTimer.Instance.Register(interval, callback, source, loop.HasValue ? loop.Value : true, groupID);
        }
    }
}
#endif