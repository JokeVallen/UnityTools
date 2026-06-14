using System.Collections.Generic;
using System.Linq;

namespace Orchestrator
{
    /// <summary>编排器工具类</summary>
    public static class OrchestratorUtility
    {
        /// <summary>验证无环</summary>
        /// <param name="steps">所有步骤的集合</param>
        /// <param name="cycleSteps">循环步骤名称的集合</param>
        /// <returns>是否为无环图</returns>
        public static bool ValidateNoCycles<TKey>(IEnumerable<IStep<TKey>> steps, out IEnumerable<TKey> cycleSteps)
        {
            var stepList = steps.ToList();
            var inDegree = new Dictionary<IStep<TKey>, int>();
            var adjacency = new Dictionary<IStep<TKey>, List<IStep<TKey>>>();
            var queue = new Queue<IStep<TKey>>();

            int count = stepList.Count;
            for (int i = 0; i < count; i++)
            {
                var step = stepList[i];
                inDegree[step] = step.Dependencies == null ? 0 : step.Dependencies.Count;
                if (!adjacency.TryGetValue(step, out var list))
                {
                    list = new List<IStep<TKey>>();
                    adjacency[step] = list;
                }
            }

            for (int i = 0; i < count; i++)
            {
                var step = stepList[i];
                if (step.Dependencies != null)
                {
                    foreach (var dep in step.Dependencies)
                    {
                        if (!adjacency.TryGetValue(dep, out var list))
                        {
                            list = new List<IStep<TKey>>();
                            adjacency[dep] = list;
                        }
                        list.Add(step);
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                var step = stepList[i];
                if (inDegree[step] == 0)
                    queue.Enqueue(step);
            }
            int visited = 0;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                visited++;

                if (adjacency.TryGetValue(current, out var neighbors))
                {
                    int count2 = neighbors.Count;
                    for (int i = 0; i < count2; i++) 
                    { 
                        var neighbor = neighbors[i];
                        inDegree[neighbor]--;
                        if (inDegree[neighbor] == 0)
                            queue.Enqueue(neighbor);
                    }
                }
            }

            if (visited != count)
            {
                var cycles = new List<TKey>();
                for (int i = 0; i < count; i++)
                {
                    var step = stepList[i];
                    if (inDegree[step] > 0)
                        cycles.Add(step.Key);
                }
                cycleSteps = cycles;
                return false;
            }

            cycleSteps = Enumerable.Empty<TKey>();
            return true;
        }

        /// <summary>拓扑排序</summary>
        /// <param name="steps">所有步骤的集合</param>
        /// <returns>按依赖顺序排列的步骤列表</returns>
        public static List<IStep<TKey>> TopologicalSort<TKey>(IEnumerable<IStep<TKey>> steps)
        {
            var stepList = steps.ToList();
            var inDegree = new Dictionary<IStep<TKey>, int>();
            var adjacency = new Dictionary<IStep<TKey>, List<IStep<TKey>>>();
            var queue = new Queue<IStep<TKey>>();
            var sorted = new List<IStep<TKey>>(stepList.Count);

            int count = stepList.Count;
            for (int i = 0; i < count; i++)
            {
                var step = stepList[i];
                inDegree[step] = step.Dependencies == null ? 0 : step.Dependencies.Count;
                if (!adjacency.TryGetValue(step, out var list))
                {
                    list = new List<IStep<TKey>>();
                    adjacency[step] = list;
                }
            }

            for (int i = 0; i < count; i++)
            {
                var step = stepList[i];
                if (step.Dependencies != null)
                {
                    foreach (var dep in step.Dependencies)
                    {
                        if (!adjacency.TryGetValue(dep, out var list))
                        {
                            list = new List<IStep<TKey>>();
                            adjacency[dep] = list;
                        }
                        list.Add(step);
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                var step = stepList[i];
                if (inDegree[step] == 0)
                    queue.Enqueue(step);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                sorted.Add(current);
                if (adjacency.TryGetValue(current, out var neighbors))
                {
                    int count2 = neighbors.Count;
                    for (int i = 0; i < count2; i++)
                    {
                        var neighbor = neighbors[i];
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