using System;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 功能类型异常
    /// </summary>
    /// <remarks>
    /// <para>当向功能扩展槽写入的类型未实现 <see cref="IFeature"/> 时抛出。</para>
    /// </remarks>
    public sealed class FeatureTypeException : ContextException
    {
        /// <summary>
        /// 功能类型
        /// </summary>
        public Type FeatureType { get; }

        /// <summary>
        /// 初始化异常
        /// </summary>
        /// <param name="featureType">功能类型</param>
        /// <param name="context">上下文</param>
        public FeatureTypeException(Type featureType, IContext context)
        : base($"The type '{(featureType != null ? featureType.FullName : "null")}' does not implement {nameof(IFeature)} and cannot be used as a feature key.", context)
        {
            FeatureType = featureType;
        }
    }
}
