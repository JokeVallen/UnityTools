using System;
using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    internal sealed class PipelineSession : IPipelineSession, IFullSnapshotable<PipelineSessionSnapshot>
    {
        public Guid Key
        {
            get 
            {
                ThrowErrorIfRecycled();
                return key;
            }
        }

        public int MaxExecutedIndex
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

        public bool IsTerminalReached
        {
            get 
            {
                ThrowErrorIfRecycled();
                return isTerminalReached;
            }
        }

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

        public bool IsAborted
        {
            get 
            {
                ThrowErrorIfRecycled();
                return isAborted;
            }
        }

        private Guid key;
        private int maxExecutedIndex;
        private PipelineDirection direction;
        private bool isTerminalReached;
        private bool isAborted;
        private bool recycled;
        private static readonly Stack<PipelineSession> pool = new Stack<PipelineSession>();
        private const int MAX_PIPELINE_SESSIONS = 1024;

        private PipelineSession(Guid key)
        {
            this.key = key;
            recycled = false;
        }

        public static PipelineSession Acquire(Guid key) 
        {
            if (pool.Count > 0)
            {
                var instance = pool.Pop();
                instance.key = key;
                instance.recycled = false;
                return instance;
            }
            return new PipelineSession(key);
        }

        public void Complete() 
        {
            ThrowErrorIfRecycled();
            isTerminalReached = true;
        }

        public void Abort() 
        {
            ThrowErrorIfRecycled();
            isAborted = true;
        }

        public void Release() 
        {
            if (pool.Count >= MAX_PIPELINE_SESSIONS) return;
            Reset();
            pool.Push(this);
        }

        public PipelineSessionSnapshot GetFullSnapshot()
        {
            ThrowErrorIfRecycled();
            return new PipelineSessionSnapshot(
                key,
                maxExecutedIndex,
                isTerminalReached,
                direction,
                isAborted
            );
        }

        private void Reset()
        {
            key = Guid.Empty;
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