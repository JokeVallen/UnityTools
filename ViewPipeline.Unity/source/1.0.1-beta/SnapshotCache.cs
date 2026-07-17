using System;
using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 快照缓存类
    /// </summary>
    public static class SnapshotCache
    {
        /// <summary>可快照组件接收刷新通知的事件</summary>
        public static event Action<Guid, Type> OnRefresh;

        /// <summary>缓存快照</summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <param name="key">会话唯一标识</param>
        /// <param name="snapshot">快照</param>
        public static void Store<TSnapshot>(Guid key, TSnapshot snapshot) 
        {
            SnapshotCacheInternal.Store(key, snapshot);
        }

        /// <summary>尝试获取快照</summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <param name="key">会话唯一标识</param>
        /// <param name="snapshot">快照接收变量</param>
        public static bool TryGet<TSnapshot>(Guid key, out TSnapshot snapshot) 
        { 
            return SnapshotCacheInternal.TryGet(key, out snapshot);
        }

        /// <summary>获取快照，不存在时抛出异常</summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <param name="key">会话唯一标识</param>
        public static TSnapshot Get<TSnapshot>(Guid key)
        {
            if (SnapshotCacheInternal.TryGet<TSnapshot>(key, out var snapshot))
                return snapshot;
            throw new KeyNotFoundException($"Snapshot of type {typeof(TSnapshot)} not found for session {key}");
        }

        /// <summary>检查快照是否存在</summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <param name="key">会话唯一标识</param>
        public static bool Exists<TSnapshot>(Guid key) 
        { 
            return SnapshotCacheInternal.Exists<TSnapshot>(key);
        }

        /// <summary>移除指定会话的指定类型快照</summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <param name="key">会话唯一标识</param>
        public static void Remove<TSnapshot>(Guid key) 
        {
            SnapshotCacheInternal.Remove<TSnapshot>(key);
        }

        /// <summary>移除指定会话的所有快照</summary>
        /// <param name="key">会话唯一标识</param>
        public static void RemoveAll(Guid key) 
        {
            SnapshotCacheInternal.RemoveAll(key);
        }

        /// <summary>获取所有指定类型的快照</summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <returns>快照集合</returns>
        public static IEnumerable<(Guid Key, TSnapshot Snapshot)> GetAll<TSnapshot>() 
        { 
            return SnapshotCacheInternal.GetAll<TSnapshot>();
        }

        /// <summary>清空所有缓存</summary>
        public static void Clear() 
        {
            SnapshotCacheInternal.Clear();
        }

        /// <summary>刷新指定类型快照缓存</summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <param name="key">会话唯一标识</param>
        public static void Refresh<TSnapshot>(Guid key) 
        { 
            if(OnRefresh != null)
                OnRefresh(key, typeof(TSnapshot));
        }

        /// <summary>刷新所有类型快照缓存</summary>
        /// <param name="key">会话唯一标识</param>
        public static void Refresh(Guid key) 
        { 
            if(OnRefresh != null)
                OnRefresh(key, null);
        }

        /// <summary>刷新并获取快照</summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <param name="key">会话唯一标识</param>
        public static TSnapshot RefreshAndGet<TSnapshot>(Guid key)
        {
            Refresh<TSnapshot>(key);
            return Get<TSnapshot>(key);
        }

        /// <summary>刷新并尝试获取快照</summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <param name="key">会话唯一标识</param>
        /// <param name="snapshot">快照接收变量</param>
        /// <returns>获取成功返回 true，否则返回false。</returns>
        public static bool TryRefreshAndGet<TSnapshot>(Guid key, out TSnapshot snapshot)
        {
            Refresh<TSnapshot>(key);
            return TryGet(key, out snapshot);
        }
    }

    /// <summary>
    /// 快照缓存类
    /// </summary>
    /// <typeparam name="TTag">标识类型</typeparam>
    public static class SnapshotCache<TTag> 
    {
        /// <summary>
        /// 可快照组件接收刷新通知的事件
        /// </summary>
        public static event Action<Guid, Optional<TTag>, Type> OnRefresh;

        /// <summary>
        /// 缓存带标签的快照
        /// </summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <param name="key">会话唯一标识</param>
        /// <param name="snapshot">快照实例</param>
        /// <param name="tag">标签</param>
        public static void Store<TSnapshot>(Guid key, TSnapshot snapshot, TTag tag)
        {
            SnapshotCacheInternal<TTag>.Store(key, snapshot, tag);
        }

        /// <summary>
        /// 尝试获取带标签的快照
        /// </summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <param name="key">会话唯一标识</param>
        /// <param name="snapshot">输出的快照实例</param>
        /// <param name="tag">标签</param>
        /// <returns>是否存在并成功获取</returns>
        public static bool TryGet<TSnapshot>(Guid key, out TSnapshot snapshot, TTag tag)
        {
            return SnapshotCacheInternal<TTag>.TryGet(key, out snapshot, tag);
        }

        /// <summary>
        /// 获取带标签的快照，不存在时抛出异常
        /// </summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <param name="key">会话唯一标识</param>
        /// <param name="tag">标签</param>
        /// <returns>快照实例</returns>
        /// <exception cref="KeyNotFoundException">当快照不存在时抛出</exception>
        public static TSnapshot Get<TSnapshot>(Guid key, TTag tag)
        {
            if (SnapshotCacheInternal<TTag>.TryGet<TSnapshot>(key, out var snapshot, tag))
                return snapshot;
            throw new KeyNotFoundException($"Snapshot of type {typeof(TSnapshot)} not found for session {key} with tag {tag}");
        }

        /// <summary>
        /// 检查带标签的快照是否存在
        /// </summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <param name="key">会话唯一标识</param>
        /// <param name="tag">标签</param>
        /// <returns>是否存在</returns>
        public static bool Exists<TSnapshot>(Guid key, TTag tag)
        {
            return SnapshotCacheInternal<TTag>.Exists<TSnapshot>(key, tag);
        }

        /// <summary>
        /// 移除指定会话和标签的指定类型快照
        /// </summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <param name="key">会话唯一标识</param>
        /// <param name="tag">标签</param>
        public static void Remove<TSnapshot>(Guid key, TTag tag)
        {
            SnapshotCacheInternal<TTag>.Remove<TSnapshot>(key, tag);
        }

        /// <summary>
        /// 移除指定会话的指定类型快照
        /// </summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <param name="key">会话唯一标识</param>
        public static void Remove<TSnapshot>(Guid key) 
        {
            SnapshotCacheInternal<TTag>.Remove<TSnapshot>(key);
        }

        /// <summary>
        /// 移除指定会话的所有所有类型快照
        /// </summary>
        /// <param name="key">会话唯一标识</param>
        public static void RemoveAll(Guid key)
        {
            SnapshotCacheInternal<TTag>.RemoveAll(key);
        }

        /// <summary>
        /// 获取所有指定类型的快照
        /// </summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <returns>包含会话标识、标签和快照的元组枚举</returns>
        public static IEnumerable<(Guid Key, TTag Tag, TSnapshot Snapshot)> GetAll<TSnapshot>()
        {
            return SnapshotCacheInternal<TTag>.GetAll<TSnapshot>();
        }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public static void Clear()
        {
            SnapshotCacheInternal<TTag>.Clear();
        }

        /// <summary>
        /// 刷新指定会话和标签的指定类型快照
        /// </summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <param name="key">会话唯一标识</param>
        /// <param name="tag">标签</param>
        public static void Refresh<TSnapshot>(Guid key, TTag tag)
        {
            if(OnRefresh != null)
                OnRefresh(key, tag, typeof(TSnapshot));
        }

        /// <summary>
        /// 刷新指定会话的指定类型快照
        /// </summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <param name="key">会话唯一标识</param>
        public static void Refresh<TSnapshot>(Guid key)
        {
            if (OnRefresh != null)
                OnRefresh(key, Optional<TTag>.None, typeof(TSnapshot));
        }

        /// <summary>
        /// 刷新指定会话和标签的所有类型快照
        /// </summary>
        /// <param name="key">会话唯一标识</param>
        /// <param name="tag">标签</param>
        public static void Refresh(Guid key, TTag tag)
        {
            if(OnRefresh != null)
                OnRefresh(key, tag, null);
        }

        /// <summary>
        /// 刷新指定会话的所有类型快照
        /// </summary>
        /// <param name="key">会话唯一标识</param>
        public static void Refresh(Guid key)
        {
            if (OnRefresh != null)
                OnRefresh(key, Optional<TTag>.None, null);
        }

        /// <summary>
        /// 刷新并获取快照
        /// </summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <param name="key">会话唯一标识</param>
        /// <param name="tag">标签</param>
        /// <returns>刷新后的快照实例</returns>
        public static TSnapshot RefreshAndGet<TSnapshot>(Guid key, TTag tag)
        {
            Refresh<TSnapshot>(key, tag);
            return Get<TSnapshot>(key, tag);
        }

        /// <summary>
        /// 刷新并尝试获取快照
        /// </summary>
        /// <typeparam name="TSnapshot">快照类型</typeparam>
        /// <param name="key">会话唯一标识</param>
        /// <param name="snapshot">输出的快照实例</param>
        /// <param name="tag">标签</param>
        /// <returns>是否成功获取</returns>
        public static bool TryRefreshAndGet<TSnapshot>(Guid key, out TSnapshot snapshot, TTag tag)
        {
            Refresh<TSnapshot>(key, tag);
            return TryGet(key, out snapshot, tag);
        }
    }
}
