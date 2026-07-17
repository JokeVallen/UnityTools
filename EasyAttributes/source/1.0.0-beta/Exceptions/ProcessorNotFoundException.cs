using System;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 处理器未找到异常
    /// </summary>
    /// <remarks>
    /// <para>在注册表中找不到指定属性类型的处理器时抛出。</para>
    /// </remarks>
    public sealed class ProcessorNotFoundException : ExecutorException
    {
        /// <summary>
        /// 属性类型
        /// </summary>
        public Type AttributeType { get; }

        /// <summary>
        /// 初始化异常
        /// </summary>
        /// <param name="attributeType">属性类型</param>
        public ProcessorNotFoundException(Type attributeType) 
        : base($"No processor is registered for attribute type '{attributeType.FullName}'.")
        {
            AttributeType = attributeType;
        }
    }
}
