using System;
using UnityEngine;

namespace CoroutineRunner
{
    /// <summary>
    /// 受控等待秒数
    /// </summary>
    /// <remarks>
    /// <para>受 <see cref="Time.timeScale"/> 影响。</para>
    /// </remarks>
    public sealed class WaitForSecondsControlled : CustomYieldInstructionBase, 
        IPoolableYieldInstruction<float>, 
        IPoolableYieldInstruction<int>, 
        IPoolableYieldInstruction
    {
        private float seconds;

        /// <inheritdoc/>
        public void Reset(float seconds) { this.seconds = seconds; }
        /// <inheritdoc/>
        public void Reset(int seconds) { this.seconds = seconds; }
        void IPoolableYieldInstruction.Reset(object seconds) { this.seconds = Convert.ToSingle(seconds); }

        /// <inheritdoc/>
        protected override bool KeepWaiting()
        {
            seconds -= Time.deltaTime;
            return seconds > 0;
        }
    }
}