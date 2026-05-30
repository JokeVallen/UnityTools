using System;

namespace CoroutineRunner
{
    /// <summary>
    /// 受控等待指定帧数
    /// </summary>
    public sealed class WaitForFramesControlled : CustomYieldInstructionBase,
        IPoolableYieldInstruction<int>,
        IPoolableYieldInstruction
    {
        private int framesLeft;

        /// <inheritdoc/>
        public void Reset(int frames) => framesLeft = frames;
        void IPoolableYieldInstruction.Reset(object value) => framesLeft = Convert.ToInt32(value);

        /// <inheritdoc/>
        protected override bool KeepWaiting()
        {
            if (framesLeft <= 0) return false;
            framesLeft--;
            return true;
        }
    }
}