#if !EVENTHUB_EXTENSION_ENABLE

namespace EventHub.Unity
{
    public static partial class EventDispatcher
    {
        /// <summary>
        /// 尝试同步清理未使用的锁对象
        /// </summary>
        /// <returns>成功清理的锁对象的个数</returns>
        public static int TryCleanupUnusedLocks()
        {
            return GetCleanable().TryCleanupUnusedLocks();
        }

        /// <summary>
        /// 尝试同步清理未使用的订阅者集合
        /// </summary>
        /// <returns>成功清理的订阅者集合的个数</returns>
        public static int TryCleanupUnusedCollections()
        {
            return GetCleanable().TryCleanupUnusedCollections();
        }

        /// <summary>
        /// 尝试同步清理未使用的锁对象和订阅者集合
        /// </summary>
        /// <returns>成功清理的锁对象和订阅者集合的总个数</returns>
        public static int TryCleanupUnusedLocksAndCollections()
        {
            return GetCleanable().TryCleanupUnusedLocksAndCollections();
        }
    }
}

#endif