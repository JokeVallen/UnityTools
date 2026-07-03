using System;
using UnityEngine;

namespace Timer
{
    /// <summary>
    /// 全局计时器
    /// </summary>
    public static class GlobalTimer
    {
        /// <summary>
        /// 注册受时间缩放影响的计时事件
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">事件回调</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterScaled(TimeSpan interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
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
        public static TimerHandle RegisterScaled(float interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
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
        public static TimerHandle RegisterUnscaled(TimeSpan interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
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
        public static TimerHandle RegisterUnscaled(float interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
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
        public static TimerHandle RegisterFrame(int frameCount, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
        {
            if (frameCount < 0) throw new ArgumentOutOfRangeException(nameof(frameCount));
            return Register(frameCount, callback, TimeSource.FrameUpdate, loop, groupID);
        }

        /// <summary>
        /// 注册基于 <see cref="UnityEngine.MonoBehaviour"/> 途径刷新的计时任务
        /// </summary>
        /// <param name="callback">计时回调</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentNullException">计时回调不可为 null</exception>
        public static TimerHandle RegisterMonoUpdate(Action callback, Optional<int> groupID = default)
        {
            return Register(1, callback, TimeSource.FrameUpdate, true, groupID);
        }

        /// <summary>
        /// 注册基于 <see cref="UnityEngine.MonoBehaviour"/> 途径晚刷新的计时任务
        /// </summary>
        /// <param name="callback">计时回调</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentNullException">计时回调不可为 null</exception>
        public static TimerHandle RegisterMonoLateUpdate(Action callback, Optional<int> groupID = default)
        {
            return Register(1, callback, TimeSource.ScaledLateUpdate, true, groupID);
        }

        /// <summary>
        /// 注册基于 <see cref="UnityEngine.MonoBehaviour"/> 途径固定物理帧刷新的计时任务
        /// </summary>
        /// <param name="callback">计时回调</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentNullException">计时回调不可为 null</exception>
        public static TimerHandle RegisterMonoFixedUpdate(Action callback, Optional<int> groupID = default)
        {
            return Register(Time.fixedDeltaTime, callback, TimeSource.ScaledFixedUpdate, true, groupID);
        }

        /// <summary>
        /// 注册未缩放固定物理帧计时器
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">计时回调</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterMonoFixedUnscaled(TimeSpan interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
        {
            return Register((float)interval.TotalSeconds, callback, TimeSource.UnscaledFixedUpdate, loop, groupID);
        }

        /// <summary>
        /// 注册未缩放固定物理帧计时器
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">计时回调</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterMonoFixedUnscaled(float interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
        {
            return Register(interval, callback, TimeSource.UnscaledFixedUpdate, loop, groupID);
        }

        /// <summary>
        /// 注册基于协程途径刷新的计时任务
        /// </summary>
        /// <param name="callback">计时回调</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentNullException">计时回调不可为 null</exception>
        public static TimerHandle RegisterCoroutineUpdate(Action callback, Optional<int> groupID = default)
        {
            return Register(1, callback, TimeSource.FrameCoroutine, true, groupID);
        }

        /// <summary>
        /// 注册协程物理帧后计时器
        /// </summary>
        /// <param name="frameCount">帧数间隔</param>
        /// <param name="callback">计时回调</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterCoroutineWaitForFixedUpdate(int frameCount, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
        {
            if (frameCount < 0) throw new ArgumentOutOfRangeException(nameof(frameCount));
            return Register(frameCount, callback, TimeSource.FrameWaitForFixedUpdate, loop, groupID);
        }

        /// <summary>
        /// 注册协程途径的帧末计时任务
        /// </summary>
        /// <param name="frameCount">帧数间隔</param>
        /// <param name="callback">计时回调</param>
        /// <param name="loop">是否循环，默认 true</param>
        /// <param name="groupID">组ID，默认不分组</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentNullException">计时回调不可为 null</exception>
        public static TimerHandle RegisterCoroutineEndOfFrame(int frameCount, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
        {
            if (frameCount < 0) throw new ArgumentOutOfRangeException(nameof(frameCount));
            return Register(frameCount, callback, TimeSource.FrameEndOfFrame, loop, groupID);
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
        public static TimerHandle Register(TimeSpan interval, Action callback, TimeDelta delta, TimeSchedule schedule, Optional<bool> loop = default,
        Optional<int> groupID = default, Optional<float> customScale = default)
        {
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
        public static TimerHandle Register(float interval, Action callback, TimeDelta delta, TimeSchedule schedule, Optional<bool> loop = default,
        Optional<int> groupID = default, Optional<float> customScale = default)
        {
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
        public static TimerHandle RegisterIndependent(TimeSpan interval, Action callback, float customScale, Optional<bool> loop = default, Optional<int> groupID = default)
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
        public static TimerHandle RegisterIndependent(float interval, Action callback, float customScale, Optional<bool> loop = default, Optional<int> groupID = default)
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
        public static TimerHandle RegisterIndependentFrame(int frameCount, Action callback, float customScale, Optional<bool> loop = default, Optional<int> groupID = default)
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
        public static TimerHandle RegisterWallClock(TimeSpan interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
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
        public static TimerHandle RegisterWallClock(float interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
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
        public static TimerHandle RegisterManual(TimeSpan interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
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
        public static TimerHandle RegisterManual(float interval, Action callback, Optional<bool> loop = default, Optional<int> groupID = default)
        {
            return Register(interval, callback, TimeSource.ManualSource, loop, groupID);
        }

        /// <summary>
        /// 手动驱动刷新
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public static void ManualUpdate(float deltaTime) 
        {
            InnerRuntimeTimer.Instance.ManualUpdate(deltaTime);
        }

        /// <summary>
        /// 取消某组计时任务
        /// </summary>
        /// <param name="groupId">组ID</param>
        public static void CancelGroup(int groupId) 
        { 
            InnerRuntimeTimer.Instance.CancelGroup(groupId);
        }

        /// <summary>
        /// 暂停某组计时任务
        /// </summary>
        /// <param name="groupId">组ID</param>
        public static void PauseGroup(int groupId) 
        {
            InnerRuntimeTimer.Instance.SetGroupPaused(groupId, true);
        }

        /// <summary>
        /// 恢复某组计时任务
        /// </summary>
        /// <param name="groupId">组ID</param>
        public static void ResumeGroup(int groupId) 
        {
            InnerRuntimeTimer.Instance.SetGroupPaused(groupId, false);
        }

        /// <summary>
        /// 设置某组计时任务的暂停状态
        /// </summary>
        /// <param name="groupId">组ID</param>
        /// <param name="isPaused">是否暂停</param>
        public static void SetGroupPaused(int groupId, bool isPaused) 
        {
            InnerRuntimeTimer.Instance.SetGroupPaused(groupId, isPaused);
        }

        /// <summary>
        /// 取消所有正在运行的计时任务（同时丢弃尚未执行的到期回调）
        /// </summary>
        public static void CancelAll() 
        {
            InnerRuntimeTimer.Instance.CancelAll();
        }

        private static TimerHandle Register(float interval, Action callback, TimeSource source, Optional<bool> loop, Optional<int> groupID)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            return InnerRuntimeTimer.Instance.Register(interval, callback, source, loop.HasValue ? loop.Value : true, groupID);
        }
    }
}
