using System;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 执行策略快照
    /// </summary>
    public readonly struct ExecutionPolicySnapshot
    {
        /// <summary>
        /// 执行策略的类型
        /// </summary>
        public Type PolicyType { get; }

        internal static readonly ExecutionPolicySnapshot Empty = new ExecutionPolicySnapshot();

        /// <param name="policyType">执行策略的类型</param>
        public ExecutionPolicySnapshot(Type policyType)
        {
            PolicyType = policyType;
        }
    }
}
