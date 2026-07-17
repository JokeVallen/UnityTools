using System;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 处理器工厂
    /// </summary>
    /// <remarks>
    /// <para>根据处理器类型创建或获取处理器实例。</para>
    /// <para>
    /// 框架内置了 <see cref="TransientProcessorFactory"/>（每次新建）和
    /// <see cref="SingletonProcessorFactory"/>（单例复用）两种实现。
    /// 使用者可替换此接口接入依赖注入容器。
    /// </para>
    /// </remarks>
    public interface IProcessorFactory
    {
        /// <summary>
        /// 创建处理器实例
        /// </summary>
        /// <param name="processorType">处理器类型</param>
        /// <returns>处理器实例</returns>
        /// <exception cref="ExecutorException">无法创建时抛出</exception>
        object Create(Type processorType);
    }
}
