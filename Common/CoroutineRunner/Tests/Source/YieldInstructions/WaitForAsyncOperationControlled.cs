using UnityEngine;

namespace CoroutineRunner
{
    /// <summary>
    /// 受控等待 <see cref="AsyncOperation"/> 完成
    /// </summary>
    public sealed class WaitForAsyncOperationControlled : CustomYieldInstructionBase,
        IPoolableYieldInstruction<AsyncOperation>,
        IPoolableYieldInstruction
    {
        private AsyncOperation asyncOp;

        /// <inheritdoc/>
        public void Reset(AsyncOperation op) => asyncOp = op;
        void IPoolableYieldInstruction.Reset(object value) => asyncOp = value as AsyncOperation;

        /// <inheritdoc/>
        protected override bool KeepWaiting()
        {
            return asyncOp != null && !asyncOp.isDone;
        }
    }
}