namespace EasyMapper.Runtime
{
    /// <summary> 映射服务入口 </summary>
    /// <remarks>
    /// <para> 提供对字符串和 <see cref="UnityEngine.Object"/> 的默认映射方法，可通过 <see cref="Current"/> 属性替换内部实例以自定义行为。 </para>
    /// <para> 当 <see cref="Current"/> 为 <c>null</c> 时自动回退到默认实例，保证服务可用。 </para>
    /// </remarks>
    public static partial class IDMap
    {
        /// <summary> 当前活动的映射实例 </summary>
        public static IDMapInstance Current
        {
            get => currentInstance ?? defaultInstance;
            set => currentInstance = value;
        }

        private static IDMapInstance currentInstance;
        private static readonly IDMapInstance defaultInstance;

        static IDMap()
        {
            defaultInstance = IDMapInstance.Builder.Create().Build();
        }

        /// <summary> 为字符串分配令牌 </summary>
        /// <param name="name"> 字符串 </param>
        public static long Assign(string name) => Current.Assign(name);

        /// <summary> 查找字符串 </summary>
        /// <param name="id"> 令牌 </param>
        public static string Locate(long id) => Current.Locate(id);

        /// <summary> 查询字符串令牌是否存在 </summary>
        /// <param name="id"> 令牌 </param>
        public static bool ContainsString(long id) => Current.ContainsString(id);

        /// <summary> 为 UnityEngine.Object 分配令牌 </summary>
        /// <param name="obj"> Unity 对象 </param>
        public static long Assign(UnityEngine.Object obj) => Current.Assign(obj);

        /// <summary> 根据令牌查找存活的 UnityEngine.Object </summary>
        /// <typeparam name="T"> 对象具体类型 </typeparam>
        /// <param name="id"> 令牌 </param>
        public static T Locate<T>(long id) where T : UnityEngine.Object => Current.Locate<T>(id);

        /// <summary> 查询对象令牌是否对应存活对象 </summary>
        /// <param name="id"> 令牌 </param>
        public static bool ContainsObject(long id) => Current.ContainsObject(id);

        /// <summary> 将长整型令牌序列化为 8 字节数组 </summary>
        public static byte[] Pack(long id) => Current.Pack(id);

        /// <summary> 从字节数组反序列化令牌 </summary>
        public static long Unpack(byte[] bytes) => Current.Unpack(bytes);

        /// <summary> 清理所有流水线中的映射 </summary>
        public static void Cleanup() => Current.Cleanup();
    }
}