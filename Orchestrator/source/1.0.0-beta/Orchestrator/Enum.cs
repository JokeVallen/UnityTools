namespace Orchestrator
{
    /// <summary>步骤流转状态</summary>
    /// <remarks>
    /// <para>用于控制执行引擎在处理完当前步骤后的动作逻辑：</para>
    /// <list type="bullet">
    /// <item><description>Continue：寻找并执行后续依赖节点。</description></item>
    /// <item><description>Break：停止当前分支，不抛出异常。</description></item>
    /// <item><description>Fail：立即停止并进入错误处理状态。</description></item>
    /// </list>
    /// </remarks>
    public enum StepFlow
    {
        /// <summary>正常继续</summary>
        Continue,
        /// <summary>业务中断</summary>
        Break,
        /// <summary>执行失败</summary>
        Fail
    }

    /// <summary>工作流中断策略</summary>
    /// <remarks>
    /// <para>定义当工作流中的某个步骤返回非 <see cref="StepFlow.Continue"/> 状态时，引擎如何控制后续步骤的执行：</para>
    /// <list type="bullet">
    /// <item>
    /// <term>Strict</term>
    /// <description>严格模式。一旦任一步骤中断，全局范围内尚未开始的步骤将全部取消执行。</description>
    /// </item>
    /// <item>
    /// <term>DependencyBased</term>
    /// <description>依赖模式。仅阻断直接或间接依赖于该中断步骤的后续节点，其他无关分支将继续并行执行。</description>
    /// </item>
    /// <item>
    /// <term>Ignore</term>
    /// <description>忽略模式。无论中间步骤状态如何，引擎都会尝试运行所有已定义的步骤。</description>
    /// </item>
    /// </list>
    /// </remarks>
    public enum InterruptionPolicy
    {
        /// <summary>严格模式</summary>
        Strict,
        /// <summary>依赖模式</summary>
        DependencyBased,
        /// <summary>忽略模式</summary>
        Ignore
    }
}