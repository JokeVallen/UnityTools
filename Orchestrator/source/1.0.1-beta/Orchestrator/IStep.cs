using System.Collections.Generic;

namespace Orchestrator
{
    /// <summary>编排执行单元</summary>
    /// <typeparam name="TKey">步骤唯一标识类型</typeparam>
    public interface IStep<TKey>
    {
        /// <summary>步骤唯一标识</summary>
        TKey Key { get; }

        /// <summary>依赖步骤集合</summary>
        IReadOnlyCollection<IStep<TKey>> Dependencies { get; }
    }
}