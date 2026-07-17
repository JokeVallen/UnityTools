using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 验证器接口
    /// </summary>
    public interface IMiddlewareValidator
    {
        /// <summary>
        /// 执行验证
        /// </summary>
        /// <param name="middlewares">静态中间件数组</param>
        /// <param name="errors">错误集合</param>
        void Validate(IReadOnlyCollection<IViewMiddleware> middlewares, IList<ValidationError> errors);
    }
}
