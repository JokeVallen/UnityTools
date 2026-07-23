using System.Collections;

namespace CoroutineRunner
{
    /// <summary>
    /// 自定义 Yield 指令抽象基类
    /// </summary>
    /// <remarks>
    /// <para>支持即时状态感知、实时暂停与取消。</para>
    /// <para>你可以通过继承该类扩展任何自定义的指令。</para>
    /// </remarks>
    public abstract class CustomYieldInstructionBase : IEnumerator
    {
        internal ICoroutineHandle Handle { set => handle = value; }

        /// <inheritdoc/>
        public object Current => null;
        private ICoroutineHandle handle;

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if (handle != null && handle.State == CoroutineState.Canceled)
                return false;

            if (handle != null && handle.State == CoroutineState.Paused)
                return true;

            return KeepWaiting();
        }

        /// <summary> 兼容早期版本 </summary>
        [System.Obsolete("This method is an interface method of IEnumerator, and it is explicitly stipulated in this class that this method is not supported.", true)]
        public void Reset() { throw new System.NotSupportedException(); }

        /// <summary>
        /// 是否继续等待
        /// </summary>
        /// <returns>如果返回 true，代表继续挂起等待；返回 false，代表等待周期结束。</returns>
        protected abstract bool KeepWaiting();
    }
}