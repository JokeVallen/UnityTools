using System.Collections.Generic;
using System.Linq;

namespace Orchestrator
{
    /// <summary>编排工具类</summary>
    /// <remarks>
    /// <para>提供工作流图的验证（如循环检测）和拓扑排序等辅助方法。</para>
    /// </remarks>
    public static class OrchestratorUtility
    {
        /// <summary>验证无环</summary>
        /// <param name="steps">所有步骤的集合</param>
        /// <param name="cycleSteps">循环步骤名称的集合</param>
        /// <returns>是否为无环图</returns>
        /// <remarks>
        /// <para>使用 Kahn 算法检测步骤依赖图中是否存在循环引用。</para>
        /// <para>若存在循环，<paramref name="cycleSteps"/> 将输出所有参与循环的步骤名称；否则为空集合。</para>
        /// <para>示例：</para>
        /// <code>
        /// if (!OrchestratorUtility.ValidateNoCycles(stepList, out var cycles))
        /// {
        ///     Console.WriteLine($"存在循环依赖: {string.Join(", ", cycles)}");
        /// }
        /// </code>
        /// </remarks>
        public static bool ValidateNoCycles(IEnumerable<IStep> steps, out IEnumerable<string> cycleSteps)
        {
            var stepList = steps as IReadOnlyList<IStep> ?? steps.ToList();
            var inDegree = new Dictionary<IStep, int>();
            var adjacency = new Dictionary<IStep, List<IStep>>();

            foreach (var step in stepList)
            {
                inDegree[step] = step.Dependencies?.Count ?? 0;
                if (!adjacency.ContainsKey(step))
                    adjacency[step] = new List<IStep>();
            }

            foreach (var step in stepList)
            {
                if (step.Dependencies != null)
                {
                    foreach (var dep in step.Dependencies)
                    {
                        if (!adjacency.ContainsKey(dep))
                            adjacency[dep] = new List<IStep>();
                        adjacency[dep].Add(step);
                    }
                }
            }

            var queue = new Queue<IStep>(stepList.Where(s => inDegree[s] == 0));
            int visited = 0;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                visited++;

                if (adjacency.TryGetValue(current, out var neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        inDegree[neighbor]--;
                        if (inDegree[neighbor] == 0)
                            queue.Enqueue(neighbor);
                    }
                }
            }

            if (visited != stepList.Count)
            {
                cycleSteps = stepList.Where(s => inDegree[s] > 0).Select(s => s.Name);
                return false;
            }

            cycleSteps = Enumerable.Empty<string>();
            return true;
        }

        /// <summary>拓扑排序</summary>
        /// <param name="steps">所有步骤的集合</param>
        /// <returns>按依赖顺序排列的步骤列表</returns>
        /// <remarks>
        /// <para>基于 Kahn 算法返回一个线性顺序，使得对于任意步骤 A → B（A 依赖 B），A 出现在 B 之后。</para>
        /// <para>注意：调用前应确保图中无环，否则返回的顺序将不完整。</para>
        /// <code>
        /// var sorted = OrchestratorUtility.TopologicalSort(allSteps);
        /// foreach (var step in sorted) { await step.ExecuteAsync(); }
        /// </code>
        /// </remarks>
        public static List<IStep> TopologicalSort(IEnumerable<IStep> steps)
        {
            var stepList = steps.ToList();
            var inDegree = new Dictionary<IStep, int>();
            var adjacency = new Dictionary<IStep, List<IStep>>();

            int count = stepList.Count;
            for (int i = 0; i < count; i++)
            {
                var step = stepList[i];
                inDegree[step] = step.Dependencies?.Count ?? 0;
                if (!adjacency.ContainsKey(step)) adjacency[step] = new List<IStep>();
            }

            for (int i = 0; i < count; i++)
            {
                var step = stepList[i];
                if (step.Dependencies != null)
                {
                    foreach (var dep in step.Dependencies)
                    {
                        if (!adjacency.ContainsKey(dep)) adjacency[dep] = new List<IStep>();
                        adjacency[dep].Add(step);
                    }
                }
            }

            var queue = new Queue<IStep>(stepList.Where(s => inDegree[s] == 0));
            var sorted = new List<IStep>();

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                sorted.Add(current);
                if (adjacency.TryGetValue(current, out var neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        inDegree[neighbor]--;
                        if (inDegree[neighbor] == 0)
                            queue.Enqueue(neighbor);
                    }
                }
            }

            return sorted;
        }
    }
}