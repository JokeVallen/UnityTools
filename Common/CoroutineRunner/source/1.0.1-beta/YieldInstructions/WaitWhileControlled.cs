using System;

namespace CoroutineRunner
{
    /// <summary>
    /// 受控等待条件指令
    /// </summary>
    /// <remarks>
    /// <para>直到条件为 false 为止。</para>
    /// </remarks>
    public sealed class WaitWhileControlled : CustomYieldInstructionBase,
        IPoolableYieldInstruction<Func<bool>>,
        IPoolableYieldInstruction
    {
        private Func<bool> predicate;

        /// <inheritdoc/>
        public void Reset(Func<bool> predicate) => this.predicate = predicate;
        void IPoolableYieldInstruction.Reset(object value) => predicate = (Func<bool>)value;

        /// <inheritdoc/>
        protected override bool KeepWaiting()
        {
            return predicate();
        }
    }
}