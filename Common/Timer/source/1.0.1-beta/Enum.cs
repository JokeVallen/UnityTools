namespace Timer
{
    /// <summary>
    /// 原子级增量计算方式（时间如何流逝）
    /// </summary>
    public enum TimeDelta : byte
    {
        /// <summary> 使用 Unity 的 <see cref="UnityEngine.Time.deltaTime"/>，受 <see cref="UnityEngine.Time.timeScale"/> 影响 </summary>
        Scaled,

        /// <summary> 使用 Unity 的 <see cref="UnityEngine.Time.unscaledDeltaTime"/>，不受时间缩放影响 </summary>
        Unscaled,

        /// <summary> 基于系统高精度时钟 (<see cref="System.Diagnostics.Stopwatch"/>)，应用切到后台时仍继续流逝 </summary>
        WallClock,

        /// <summary> 离散帧计数，每次 Tick 固定推进 1 帧（无时间概念，仅计数） </summary>
        Frame,

        /// <summary> 由外部手动注入增量值，适用于自定义游戏循环或离线模拟 </summary>
        Manual
    }

    /// <summary>
    /// 原子级驱动调度时机（何时检查推进）
    /// </summary>
    public enum TimeSchedule : byte
    {
        /// <summary> 在 <c>UnityEngine.MonoBehaviour.Update</c> 生命周期中执行 </summary>
        Update,

        /// <summary> 在 <c>UnityEngine.MonoBehaviour.LateUpdate</c> 生命周期中执行 </summary>
        LateUpdate,

        /// <summary> 在 <c>UnityEngine.MonoBehaviour.FixedUpdate</c> 生命周期中执行 </summary>
        FixedUpdate,

        /// <summary> 在协程 <c>yield return null</c> 后执行（每帧一次，位于 Update 与 LateUpdate 之间） </summary>
        Coroutine,

        /// <summary> 在协程 <c>yield return new WaitForEndOfFrame()</c> 后执行（所有渲染完成后） </summary>
        EndOfFrame,

        /// <summary> 在协程 <c>yield return new WaitForFixedUpdate()</c> 后执行（物理帧刚刚结束后） </summary>

        WaitForFixedUpdate,

        /// <summary> 由外部显式调用 <see cref="GlobalTimer.ManualUpdate"/> 驱动，不依赖 Unity 生命周期 </summary>
        Manual
    }
}