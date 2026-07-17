using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 工具类
    /// </summary>
    internal static class ViewPipelineUtility
    {
        /// <summary>
        /// 辅助过滤并安全转换中间件集合
        /// </summary>
        public static IViewMiddleware[] FilterAndToArray(IEnumerable<IViewMiddleware> middlewares)
        {
            if (middlewares == null) return new IViewMiddleware[0];
            var list = new List<IViewMiddleware>();
            foreach (var m in middlewares)
            {
                if (m != null) list.Add(m);
            }
            return list.ToArray();
        }
    }
}
