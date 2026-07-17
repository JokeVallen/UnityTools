using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 视图会话注册表
    /// </summary>
    public static class ViewSessionRegistry
    {
        /// <summary>
        /// 视图会话集合的只读视图
        /// </summary>
        public static IReadOnlyDictionary<Guid, IViewSession> Sessions => sessions;
        private static readonly Dictionary<Guid, IViewSession> sessions = new Dictionary<Guid, IViewSession>();

        internal static void Register(Guid key, IViewSession session) 
        {
            if(session == null) throw new ArgumentNullException(nameof(session));
            sessions[key] = session;
        }

        internal static void Unregister(Guid key) 
        {
            sessions.Remove(key);
        }

        /// <summary>
        /// 释放注册表资源
        /// </summary>
        /// <returns>异步句柄</returns>
        public static async UniTask DisposeAsync() 
        {
            foreach (var session in sessions.Values)
            {
                if(session is IAsyncDisposable)
                    await ((IAsyncDisposable)session).DisposeAsync();
                if(session is IDisposable)
                    ((IDisposable)session).Dispose();
            }
            sessions.Clear();
        } 

        /// <summary>
        /// 清空注册表
        /// </summary>
        public static void Clear() 
        { 
            sessions.Clear();
        }
    }
}
