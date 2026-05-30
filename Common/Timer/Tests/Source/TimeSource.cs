namespace Timer
{
    /// <summary>
    /// 时间类型
    /// </summary>
    public enum TimeSource : byte
    {
        /// <summary> 游戏内秒数 (受 Time.timeScale 暂停/倍速影响) </summary>
        ScaledTime,

        /// <summary> 现实物理秒数 (不受 Time.timeScale 影响) </summary>
        UnscaledTime,

        /// <summary> [Mono途径] 满帧标准 Update 循环 (在 MonoBehaviour.Update 期间顺排执行) </summary>
        MonoUpdate,

        /// <summary> [Mono途径] 满帧后置 LateUpdate 循环 (在 MonoBehaviour.LateUpdate 期间顺排执行) </summary>
        MonoLateUpdate,

        /// <summary> [Mono途径] 满物理帧 FixedUpdate 循环 (在 MonoBehaviour.FixedUpdate 期间顺排执行) </summary>
        MonoFixedUpdate,

        /// <summary> [协程途径] 满帧标准协程循环 (在 yield return null 的瞬间步进) </summary>
        CoroutineUpdate,

        /// <summary> [协程途径] 满帧渲染结束循环 (在 yield return new WaitForEndOfFrame() 完全渲染后执行) </summary>
        CoroutineEndOfFrame
    }
}