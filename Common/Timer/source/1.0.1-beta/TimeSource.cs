using System;

namespace Timer
{
    /// <summary>
    /// 计时器时间源配置（组合原子）
    /// </summary>
    public readonly struct TimeSource : IEquatable<TimeSource>
    {
        /// <summary> 增量计算方式，决定时间如何流逝 </summary>
        public TimeDelta Delta => delta;

        /// <summary> 驱动调度时机，决定何时检查并推进计时器 </summary>
        public TimeSchedule Schedule => schedule;

        /// <summary> 自定义倍率，仅当 Delta 为 <see cref="TimeDelta.Unscaled"/>、<see cref="TimeDelta.WallClock"/> 或 <see cref="TimeDelta.Manual"/> 时生效 </summary>
        public float Scale => scale;

        private readonly TimeDelta delta;
        private readonly TimeSchedule schedule;
        private readonly float scale;

        /// <summary>
        /// 构造时间源
        /// </summary>
        /// <param name="delta">增量计算方式</param>
        /// <param name="schedule">驱动调度时机</param>
        /// <param name="scale">自定义倍率（默认 1.0），仅对 <see cref="TimeDelta.Unscaled"/>、<see cref="TimeDelta.WallClock"/>、<see cref="TimeDelta.Manual"/> 有效</param>
        public TimeSource(TimeDelta delta, TimeSchedule schedule, float scale = 1f)
        {
            this.delta = delta;
            this.schedule = schedule;
            this.scale = scale;
        }

        /// <summary> 时间缩放敏感 + Update </summary>
        public static readonly TimeSource ScaledUpdate = new TimeSource(TimeDelta.Scaled, TimeSchedule.Update);

        /// <summary> 时间缩放不敏感 + Update </summary>
        public static readonly TimeSource UnscaledUpdate = new TimeSource(TimeDelta.Unscaled, TimeSchedule.Update);

        /// <summary> 帧计数驱动 + Update </summary>
        public static readonly TimeSource FrameUpdate = new TimeSource(TimeDelta.Frame, TimeSchedule.Update);

        /// <summary> 时间缩放敏感 + LateUpdate </summary>
        public static readonly TimeSource ScaledLateUpdate = new TimeSource(TimeDelta.Scaled, TimeSchedule.LateUpdate);

        /// <summary> 时间缩放敏感 + FixedUpdate </summary>
        public static readonly TimeSource ScaledFixedUpdate = new TimeSource(TimeDelta.Scaled, TimeSchedule.FixedUpdate);

        /// <summary> 时间缩放不敏感 + FixedUpdate </summary>
        public static readonly TimeSource UnscaledFixedUpdate = new TimeSource(TimeDelta.Unscaled, TimeSchedule.FixedUpdate);

        /// <summary> 帧计数驱动 + 协程标准帧 </summary>
        public static readonly TimeSource FrameCoroutine = new TimeSource(TimeDelta.Frame, TimeSchedule.Coroutine);

        /// <summary> 帧计数驱动 + 协程渲染帧末 </summary>
        public static readonly TimeSource FrameEndOfFrame = new TimeSource(TimeDelta.Frame, TimeSchedule.EndOfFrame);

        /// <summary> 帧计数驱动 + 协程物理帧后 </summary>
        public static readonly TimeSource FrameWaitForFixedUpdate = new TimeSource(TimeDelta.Frame, TimeSchedule.WaitForFixedUpdate);

        /// <summary> 挂钟时间（后台仍走）+ Update </summary>
        public static readonly TimeSource WallClockUpdate = new TimeSource(TimeDelta.WallClock, TimeSchedule.Update);

        /// <summary> 手动驱动 </summary>
        public static readonly TimeSource ManualSource = new TimeSource(TimeDelta.Manual, TimeSchedule.Manual);

        /// <summary>
        /// 创建独立缩放时间源（自定义倍率，基于 Unscaled 增量，调度在 Update）
        /// </summary>
        /// <param name="scale">自定义倍率，如 2.0 表示双倍速，0.5 表示半速</param>
        /// <returns>对应的时间源配置</returns>
        public static TimeSource Independent(float scale) => new TimeSource(TimeDelta.Unscaled, TimeSchedule.Update, scale);
        
        /// <inheritdoc/>
        public bool Equals(TimeSource other) => delta == other.delta && schedule == other.schedule && scale.Equals(other.scale);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is TimeSource other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() 
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (int)delta;
                hash = hash * 31 + (int)schedule;
                hash = hash * 31 + scale.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public static bool operator ==(TimeSource left, TimeSource right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(TimeSource left, TimeSource right) => !left.Equals(right);
    }
}
