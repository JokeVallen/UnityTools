namespace PoolKit.Unity
{
    /// <summary>
    /// Unity对象池设置
    /// </summary>
    public class UnityObjectPoolSettings<T> where T : UnityEngine.Object
    {
        /// <summary>
        /// 对象池初始容量
        /// </summary>
        public int capacity = 100;

        /// <summary>
        /// 对象池是否持久化
        /// </summary>
        public bool isPersistant = true;

        /// <summary>
        /// 对象池是否固定容量
        /// </summary>
        public bool isFixed = false;

        /// <summary>
        /// 对象池容器
        /// </summary>
        public UnityEngine.GameObject container;

        /// <summary>
        /// 对象原型
        /// </summary>
        public T original;

        /// <summary>
        /// 对象默认名称
        /// </summary>
        public string defaultName = string.Empty;

        /// <summary>
        /// 获取时激活对象
        /// </summary>
        public bool activeWhenGet = true;
    }
}