using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 框架内核管线执行运行时会话状态
    /// </summary>
    internal sealed class PipelineSession : IPipelineSession
    {
        /// <summary>
        /// 当前最大执行索引
        /// </summary>
        internal int MaxExecutedIndex
        {
            get 
            {
                ThrowErrorIfRecycled();
                return maxExecutedIndex;
            }
            set 
            { 
                ThrowErrorIfRecycled();
                maxExecutedIndex = value;
            }
        }

        /// <inheritdoc/>
        public bool IsTerminalReached
        {
            get 
            {
                ThrowErrorIfRecycled();
                return isTerminalReached;
            }
        }

        /// <inheritdoc/>
        public PipelineDirection Direction 
        {
            get 
            {
                ThrowErrorIfRecycled();
                return direction;
            }
            internal set 
            {
                ThrowErrorIfRecycled();
                direction = value;
            }
        }

        /// <inheritdoc/>
        public bool IsAborted
        {
            get 
            {
                ThrowErrorIfRecycled();
                return isAborted;
            }
        }

        private int maxExecutedIndex;
        private PipelineDirection direction;
        private bool isTerminalReached;
        private bool isAborted;
        private bool recycled;
        private static readonly Stack<PipelineSession> pool = new Stack<PipelineSession>();
        private const int MAX_PIPELINE_SESSIONS = 1024;

        private PipelineSession() { Reset(); }

        /// <summary>
        /// 获取会话实例
        /// </summary>
        /// <returns>会话实例</returns>
        public static PipelineSession Acquire() 
        {
            if (pool.Count > 0)
            {
                var instance = pool.Pop();
                instance.recycled = false;
                return instance;
            }
            return new PipelineSession() { recycled = false };
        }

        /// <summary>
        /// 标记管道执行完成
        /// </summary>
        /// <exception cref="System.NotSupportedException">会话实例已回收。</exception>
        public void Complete() 
        {
            ThrowErrorIfRecycled();
            isTerminalReached = true;
        }

        /// <summary>
        /// 标记管道已中断执行
        /// </summary>
        /// <exception cref="System.NotSupportedException">会话实例已回收。</exception>
        public void Abort() 
        {
            ThrowErrorIfRecycled();
            isAborted = true;
        }

        internal void Release() 
        {
            if (pool.Count >= MAX_PIPELINE_SESSIONS) return;
            Reset();
            pool.Push(this);
        }

        private void Reset()
        {
            maxExecutedIndex = 0;
            isTerminalReached = false;
            direction = PipelineDirection.Open;
            isAborted = false;
            recycled = true;
        }

        private void ThrowErrorIfRecycled() 
        {
            if (recycled) 
                throw new System.NotSupportedException("[ViewPipeline] The pipeline session has been recycled.");
        }
    }
}