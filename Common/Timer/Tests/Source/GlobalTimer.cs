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
        /// 注册基于 <see cref="UnityEngine.MonoBehaviour"/> 途径刷新的计时任务
        /// </summary>
        /// <param name="action">计时回调</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentNullException">计时回调不可为 null</exception>
        public static TimerHandle RegisterMonoUpdate(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return RegisterFrame(1, action, loop: true);
        }

        /// <summary>
        /// 注册基于 <see cref="UnityEngine.MonoBehaviour"/> 途径刷新的计时任务
        /// </summary>
        /// <param name="action">计时回调</param>
        /// <param name="groupID">组ID</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentNullException">计时回调不可为 null</exception>
        public static TimerHandle RegisterMonoUpdate(Action action, int groupID)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return RegisterFrame(1, action, loop: true, groupID);
        }

        /// <summary>
        /// 注册基于 <see cref="UnityEngine.MonoBehaviour"/> 途径晚刷新的计时任务
        /// </summary>
        /// <param name="action">计时回调</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentNullException">计时回调不可为 null</exception>
        public static TimerHandle RegisterMonoLateUpdate(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return InnerTimer.Instance.Register(1, action, TimeSource.MonoLateUpdate, loop: true, 0, false);
        }

        /// <summary>
        /// 注册基于 <see cref="UnityEngine.MonoBehaviour"/> 途径晚刷新的计时任务
        /// </summary>
        /// <param name="action">计时回调</param>
        /// <param name="groupID">组ID</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentNullException">计时回调不可为 null</exception>
        public static TimerHandle RegisterMonoLateUpdate(Action action, int groupID)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return InnerTimer.Instance.Register(1, action, TimeSource.MonoLateUpdate, loop: true, groupID, true);
        }

        /// <summary>
        /// 注册基于 <see cref="UnityEngine.MonoBehaviour"/> 途径固定物理帧刷新的计时任务
        /// </summary>
        /// <param name="action">计时回调</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentNullException">计时回调不可为 null</exception>
        public static TimerHandle RegisterMonoFixedUpdate(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return InnerTimer.Instance.Register(Time.fixedDeltaTime, action, TimeSource.MonoFixedUpdate, loop: true, 0, false);
        }

        /// <summary>
        /// 注册基于 <see cref="UnityEngine.MonoBehaviour"/> 途径固定物理帧刷新的计时任务
        /// </summary>
        /// <param name="action">计时回调</param>
        /// <param name="groupID">组ID</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentNullException">计时回调不可为 null</exception>
        public static TimerHandle RegisterMonoFixedUpdate(Action action, int groupID)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return InnerTimer.Instance.Register(Time.fixedDeltaTime, action, TimeSource.MonoFixedUpdate, loop: true, groupID, true);
        }

        /// <summary>
        /// 注册基于协程途径刷新的计时任务
        /// </summary>
        /// <param name="action">计时回调</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentNullException">计时回调不可为 null</exception>
        public static TimerHandle RegisterCoroutineUpdate(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return InnerTimer.Instance.Register(1, action, TimeSource.CoroutineUpdate, loop: true, 0, false);
        }

        /// <summary>
        /// 注册基于协程途径刷新的计时任务
        /// </summary>
        /// <param name="action">计时回调</param>
        /// <param name="groupID">组ID</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentNullException">计时回调不可为 null</exception>
        public static TimerHandle RegisterCoroutineUpdate(Action action, int groupID)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return InnerTimer.Instance.Register(1, action, TimeSource.CoroutineUpdate, loop: true, groupID, true);
        }

        /// <summary>
        /// 注册协程途径的帧末计时任务
        /// </summary>
        /// <param name="action">计时回调</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentNullException">计时回调不可为 null</exception>
        public static TimerHandle RegisterCoroutineEndOfFrame(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return InnerTimer.Instance.Register(1, action, TimeSource.CoroutineEndOfFrame, loop: true, 0, false);
        }

        /// <summary>
        /// 注册协程途径的帧末计时任务
        /// </summary>
        /// <param name="action">计时回调</param>
        /// <param name="groupID">组ID</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentNullException">计时回调不可为 null</exception>
        public static TimerHandle RegisterCoroutineEndOfFrame(Action action, int groupID)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return InnerTimer.Instance.Register(1, action, TimeSource.CoroutineEndOfFrame, loop: true, groupID, true);
        }

        /// <summary>
        /// 注册受时间缩放影响的计时事件
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">事件回调</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterScaled(TimeSpan interval, Action callback)
        {
            return InnerTimer.Instance.Register((float)interval.TotalSeconds, callback, TimeSource.ScaledTime, loop: true, 0, false);
        }

        /// <summary>
        /// 注册受时间缩放影响的计时事件
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">事件回调</param>
        /// <param name="groupID">组ID</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterScaled(TimeSpan interval, Action callback, int groupID)
        {
            return InnerTimer.Instance.Register((float)interval.TotalSeconds, callback, TimeSource.ScaledTime, loop: true, groupID, true);
        }

        /// <summary>
        /// 注册受时间缩放影响的计时事件
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">事件回调</param>
        /// <param name="loop">是否循环</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterScaled(TimeSpan interval, Action callback, bool loop)
        {
            return InnerTimer.Instance.Register((float)interval.TotalSeconds, callback, TimeSource.ScaledTime, loop, 0, false);
        }

        /// <summary>
        /// 注册受时间缩放影响的计时事件
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">事件回调</param>
        /// <param name="loop">是否循环</param>
        /// <param name="groupID">组ID</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterScaled(TimeSpan interval, Action callback, bool loop, int groupID)
        {
            return InnerTimer.Instance.Register((float)interval.TotalSeconds, callback, TimeSource.ScaledTime, loop, groupID, true);
        }

        /// <summary>
        /// 注册受时间缩放影响的计时事件
        /// </summary>
        /// <param name="interval">时间间隔（单位：秒）</param>
        /// <param name="callback">事件回调</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterScaled(float interval, Action callback)
        {
            return InnerTimer.Instance.Register(interval, callback, TimeSource.ScaledTime, loop: true, 0, false);
        }

        /// <summary>
        /// 注册受时间缩放影响的计时事件
        /// </summary>
        /// <param name="interval">时间间隔（单位：秒）</param>
        /// <param name="callback">事件回调</param>
        /// <param name="groupID">组ID</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterScaled(float interval, Action callback, int groupID)
        {
            return InnerTimer.Instance.Register(interval, callback, TimeSource.ScaledTime, loop: true, groupID, true);
        }

        /// <summary>
        /// 注册受时间缩放影响的计时事件
        /// </summary>
        /// <param name="interval">时间间隔（单位：秒）</param>
        /// <param name="callback">事件回调</param>
        /// <param name="loop">是否循环</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterScaled(float interval, Action callback, bool loop)
        {
            return InnerTimer.Instance.Register(interval, callback, TimeSource.ScaledTime, loop, 0, false);
        }

        /// <summary>
        /// 注册受时间缩放影响的计时事件
        /// </summary>
        /// <param name="interval">时间间隔（单位：秒）</param>
        /// <param name="callback">事件回调</param>
        /// <param name="loop">是否循环</param>
        /// <param name="groupID">组ID</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterScaled(float interval, Action callback, bool loop, int groupID)
        {
            return InnerTimer.Instance.Register(interval, callback, TimeSource.ScaledTime, loop, groupID, true);
        }

        /// <summary>
        /// 注册真实物理时间的计时事件
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">事件回调</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterUnscaled(TimeSpan interval, Action callback)
        {
            return InnerTimer.Instance.Register((float)interval.TotalSeconds, callback, TimeSource.UnscaledTime, loop: true, 0, false);
        }

        /// <summary>
        /// 注册真实物理时间的计时事件
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">事件回调</param>
        /// <param name="groupID">组ID</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterUnscaled(TimeSpan interval, Action callback, int groupID)
        {
            return InnerTimer.Instance.Register((float)interval.TotalSeconds, callback, TimeSource.UnscaledTime, loop: true, groupID, true);
        }

        /// <summary>
        /// 注册真实物理时间的计时事件
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">事件回调</param>
        /// <param name="loop">是否循环</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterUnscaled(TimeSpan interval, Action callback, bool loop)
        {
            return InnerTimer.Instance.Register((float)interval.TotalSeconds, callback, TimeSource.UnscaledTime, loop, 0, false);
        }

        /// <summary>
        /// 注册真实物理时间的计时事件
        /// </summary>
        /// <param name="interval">时间间隔</param>
        /// <param name="callback">事件回调</param>
        /// <param name="loop">是否循环</param>
        /// <param name="groupID">组ID</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterUnscaled(TimeSpan interval, Action callback, bool loop, int groupID)
        {
            return InnerTimer.Instance.Register((float)interval.TotalSeconds, callback, TimeSource.UnscaledTime, loop, groupID, true);
        }

        /// <summary>
        /// 注册真实物理时间的计时事件
        /// </summary>
        /// <param name="interval">时间间隔（单位：秒）</param>
        /// <param name="callback">事件回调</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterUnscaled(float interval, Action callback)
        {
            return InnerTimer.Instance.Register(interval, callback, TimeSource.UnscaledTime, loop: true, 0, false);
        }

        /// <summary>
        /// 注册真实物理时间的计时事件
        /// </summary>
        /// <param name="interval">时间间隔（单位：秒）</param>
        /// <param name="callback">事件回调</param>
        /// <param name="groupID">组ID</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterUnscaled(float interval, Action callback, int groupID)
        {
            return InnerTimer.Instance.Register(interval, callback, TimeSource.UnscaledTime, loop: true, groupID, true);
        }

        /// <summary>
        /// 注册真实物理时间的计时事件
        /// </summary>
        /// <param name="interval">时间间隔（单位：秒）</param>
        /// <param name="callback">事件回调</param>
        /// <param name="loop">是否循环</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterUnscaled(float interval, Action callback, bool loop)
        {
            return InnerTimer.Instance.Register(interval, callback, TimeSource.UnscaledTime, loop, 0, false);
        }

        /// <summary>
        /// 注册真实物理时间的计时事件
        /// </summary>
        /// <param name="interval">时间间隔（单位：秒）</param>
        /// <param name="callback">事件回调</param>
        /// <param name="loop">是否循环</param>
        /// <param name="groupID">组ID</param>
        /// <returns>句柄</returns>
        public static TimerHandle RegisterUnscaled(float interval, Action callback, bool loop, int groupID)
        {
            return InnerTimer.Instance.Register(interval, callback, TimeSource.UnscaledTime, loop, groupID, true);
        }

        /// <summary>
        /// 注册帧数级计时事件
        /// </summary>
        /// <param name="frameCount">帧数</param>
        /// <param name="callback">事件回调</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="frameCount"/> 不可为负数</exception>
        public static TimerHandle RegisterFrame(int frameCount, Action callback)
        {
            if (frameCount < 0) throw new ArgumentOutOfRangeException(nameof(frameCount));
            return InnerTimer.Instance.Register(frameCount, callback, TimeSource.MonoUpdate, loop: true, 0, false);
        }

        /// <summary>
        /// 注册帧数级计时事件
        /// </summary>
        /// <param name="frameCount">帧数</param>
        /// <param name="callback">事件回调</param>
        /// <param name="groupID">组ID</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="frameCount"/> 不可为负数</exception>
        public static TimerHandle RegisterFrame(int frameCount, Action callback, int groupID)
        {
            if (frameCount < 0) throw new ArgumentOutOfRangeException(nameof(frameCount));
            return InnerTimer.Instance.Register(frameCount, callback, TimeSource.MonoUpdate, loop: true, groupID, true);
        }

        /// <summary>
        /// 注册帧数级计时事件
        /// </summary>
        /// <param name="frameCount">帧数</param>
        /// <param name="callback">事件回调</param>
        /// <param name="loop">是否循环</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="frameCount"/> 不可为负数</exception>
        public static TimerHandle RegisterFrame(int frameCount, Action callback, bool loop)
        {
            if (frameCount < 0) throw new ArgumentOutOfRangeException(nameof(frameCount));
            return InnerTimer.Instance.Register(frameCount, callback, TimeSource.MonoUpdate, loop, 0, false);
        }

        /// <summary>
        /// 注册帧数级计时事件
        /// </summary>
        /// <param name="frameCount">帧数</param>
        /// <param name="callback">事件回调</param>
        /// <param name="loop">是否循环</param>
        /// <param name="groupID">组ID</param>
        /// <returns>句柄</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="frameCount"/> 不可为负数</exception>
        public static TimerHandle RegisterFrame(int frameCount, Action callback, bool loop, int groupID)
        {
            if (frameCount < 0) throw new ArgumentOutOfRangeException(nameof(frameCount));
            return InnerTimer.Instance.Register(frameCount, callback, TimeSource.MonoUpdate, loop, groupID, true);
        }

        /// <summary>
        /// 取消某组计时任务
        /// </summary>
        /// <param name="groupId">组ID</param>
        public static void CancelGroup(int groupId)
        {
            InnerTimer.Instance.CancelGroup(groupId);
        }

        /// <summary>
        /// 暂停某组计时任务
        /// </summary>
        /// <param name="groupId">组ID</param>
        public static void PauseGroup(int groupId)
        {
            InnerTimer.Instance.SetGroupPaused(groupId, true);
        }

        /// <summary>
        /// 恢复某组计时任务
        /// </summary>
        /// <param name="groupId">组ID</param>
        public static void ResumeGroup(int groupId)
        {
            InnerTimer.Instance.SetGroupPaused(groupId, false);
        }

        /// <summary>
        /// 设置某组计时任务的暂停状态
        /// </summary>
        /// <param name="groupId">组ID</param>
        /// <param name="isPaused">是否暂停</param>
        public static void SetGroupPaused(int groupId, bool isPaused)
        {
            InnerTimer.Instance.SetGroupPaused(groupId, isPaused);
        }

        public static void CancelAll() => InnerTimer.Instance.CancelAll();
    }
}
