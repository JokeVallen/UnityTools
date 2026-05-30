using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CoroutineRunner
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 暂停协程
        /// </summary>
        /// <param name="token"></param>
        public static void Pause(this in CoroutineHandleToken token)
        {
            if (!token.IsValid) return;
            ((InternalCoroutineRunner)InternalCoroutineRunner.Instance).Pause(token);
        }

        /// <summary>
        /// 恢复协程
        /// </summary>
        /// <param name="token"></param>
        public static void Resume(this in CoroutineHandleToken token)
        {
            if (!token.IsValid) return;
            ((InternalCoroutineRunner)InternalCoroutineRunner.Instance).Resume(token);
        }

        /// <summary>
        /// 取消协程
        /// </summary>
        /// <param name="token"></param>
        public static void Cancel(this in CoroutineHandleToken token)
        {
            if (!token.IsValid) return;
            ((InternalCoroutineRunner)InternalCoroutineRunner.Instance).Cancel(token);
        }

        /// <summary>
        /// 获取协程状态
        /// </summary>
        /// <param name="token"></param>
        /// <returns>协程状态</returns>
        public static CoroutineState GetState(this in CoroutineHandleToken token)
        {
            if (token.TryGetState(out var state))
                return state;
            return CoroutineState.Completed;
        }

        /// <summary>
        /// 尝试获取协程状态
        /// </summary>
        /// <param name="token"></param>
        /// <param name="state">接收变量</param>
        /// <returns>协程已失效则返回 false，否则返回 true。</returns>
        public static bool TryGetState(this in CoroutineHandleToken token, out CoroutineState state)
        {
            if (!token.IsValid)
            {
                state = default;
                return false;
            }
            return ((InternalCoroutineRunner)InternalCoroutineRunner.Instance).TryGetState(token, out state);
        }

        /// <summary>
        /// 协程是否已失效
        /// </summary>
        /// <param name="token"></param>
        /// <returns>协程失效则返回 true，否则返回 false。</returns>
        public static bool IsDone(this in CoroutineHandleToken token)
        {
            return !token.TryGetState(out var state) || state == CoroutineState.Completed || state == CoroutineState.Canceled;
        }

        /// <summary>
        /// 扩展获取原生 C# 异步等待器，使 <see cref="ICoroutineHandle"/> 能够直接支持现代 C# 的 async/await 强类型强同步等待
        /// </summary>
        /// <param name="token">协程句柄令牌</param>
        /// <returns>异步任务等待结构</returns>
        public static TaskAwaiter<bool> GetAwaiter(this in CoroutineHandleToken token)
        {
            var tcs = new TaskCompletionSource<bool>();
            var runner = (InternalCoroutineRunner)InternalCoroutineRunner.Instance;

            if (runner.TryGetHandle(token.Id, out var handle) && handle.Version == token.Version)
            {
                var state = handle.State;
                if (state == CoroutineState.Completed || state == CoroutineState.Canceled)
                {
                    tcs.SetResult(true);
                }
                else
                {
                    handle.OnAwaiterComplete += () => tcs.TrySetResult(true);
                }
            }
            else
            {
                tcs.SetResult(false);
            }

            return tcs.Task.GetAwaiter();
        }
    }
}