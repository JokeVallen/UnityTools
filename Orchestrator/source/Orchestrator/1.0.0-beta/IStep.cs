using System.Collections.Generic;

namespace Orchestrator
{
    /// <summary>编排执行单元</summary>
    /// <remarks>
    /// <para>代表工作流中的一个可执行节点，包含名称和依赖关系。</para>
    /// </remarks>
    public interface IStep
    {
        /// <summary>步骤识别名称</summary>
        /// <remarks>
        /// <para>在工作流中必须唯一，用于日志、调试及依赖查找。</para>
        /// </remarks>
        string Name { get; }

        /// <summary>依赖步骤集合</summary>
        /// <remarks>
        /// <para>表示当前步骤执行前必须完成的其他步骤。若集合为空或无依赖，则视为起始节点。</para>
        /// </remarks>
        IReadOnlyCollection<IStep> Dependencies { get; }
    }
}