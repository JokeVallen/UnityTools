using System;
using UnityEngine;

namespace CoroutineRunner
{
    /// <summary>
    /// 受控等待真实秒数
    /// </summary>
    /// <remarks>
    /// <para>忽略 <see cref="Time.timeScale"/>。</para>
    /// </remarks>
    public sealed class WaitForRealtimeSecondsControlled : CustomYieldInstructionBase,
        IPoolableYieldInstruction<float>,
        IPoolableYieldInstruction<int>,
        IPoolableYieldInstruction
    {
        private float seconds;

        /// <inheritdoc/>
        public void Reset(float seconds) => this.seconds = seconds;
        /// <inheritdoc/>
        public void Reset(int seconds) => this.seconds = seconds;
        void IPoolableYieldInstruction.Reset(object value) => this.seconds = Convert.ToSingle(value);

        /// <inheritdoc/>
        protected override bool KeepWaiting()
        {
            seconds -= Time.unscaledDeltaTime;
            return seconds > 0;
        }
    }
}