using System;
using System.Collections.Generic;

namespace EasyAttributes
{
    /// <summary>
    /// 上下文接口
    /// </summary>
    public interface IContext
    {
        /// <summary>
        /// 当前正在处理的 Attribute 实例
        /// </summary>
        EasyAttribute Attribute { get; }

        /// <summary>
        /// 处理器间共享的状态字典
        /// </summary>
        IReadOnlyDictionary<string, object> Items { get; }

        /// <summary>
        /// 功能扩展槽字典
        /// </summary>
        IReadOnlyDictionary<Type, IFeature> Features { get; }

        /// <summary>
        /// 上下文构建时从 <see cref="EasyAttribute.Enabled"/> 读取的启用状态快照
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// 上下文构建时从 <see cref="EasyAttribute.Priority"/> 读取的优先级快照
        /// </summary>
        int Priority { get; }
    }
}
