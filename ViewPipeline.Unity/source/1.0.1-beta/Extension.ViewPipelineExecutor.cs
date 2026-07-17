using System;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static partial class Extension
    {
        /// <summary>
        /// 获取强类型上下文，如果当前上下文不支持则抛出明确的异常
        /// </summary>
        /// <exception cref="InvalidOperationException">当上下文不支持 ITypedPipelineContext 时抛出</exception>
        public static ITypedPipelineContext GetTypedContext(this ViewPipelineExecutor executor)
        {
            if (executor.Context is ITypedPipelineContext typed)
                return typed;
            throw new InvalidOperationException("[ViewPipeline] The current context does not support ITypedPipelineContext.");
        }

        /// <summary>
        /// 尝试获取强类型上下文
        /// </summary>
        /// <param name="executor">执行器</param>
        /// <param name="typedContext">输出的强类型上下文</param>
        /// <returns>是否成功获取</returns>
        public static bool TryGetTypedContext(this ViewPipelineExecutor executor, out ITypedPipelineContext typedContext)
        {
            typedContext = executor.Context as ITypedPipelineContext;
            return typedContext != null;
        }

        /// <summary>
        /// 设置数据（如果上下文支持 ITypedPipelineContext）
        /// </summary>
        /// <returns>是否设置成功</returns>
        public static bool TrySetData<TKey, TValue>(this ViewPipelineExecutor executor, TKey key, TValue value)
        {
            if (executor.Context is ITypedPipelineContext typed)
            {
                typed.Set(key, value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取数据（如果上下文支持 ITypedPipelineContext）
        /// </summary>
        /// <returns>包含值的 Optional，如果上下文不支持或无值则返回 None</returns>
        public static Optional<TValue> TryGetData<TKey, TValue>(this ViewPipelineExecutor executor, TKey key)
        {
            if (executor.Context is ITypedPipelineContext typed)
                return typed.Get<TKey, TValue>(key);
            return Optional<TValue>.None;
        }

        /// <summary>
        /// 设置数据（如果上下文不支持则抛出异常）
        /// </summary>
        /// <exception cref="InvalidOperationException">当上下文不支持 ITypedPipelineContext 时抛出</exception>
        public static void SetData<TKey, TValue>(this ViewPipelineExecutor executor, TKey key, TValue value)
        {
            executor.GetTypedContext().Set(key, value);
        }

        /// <summary>
        /// 获取数据（如果上下文不支持则抛出异常）
        /// </summary>
        /// <exception cref="InvalidOperationException">当上下文不支持 ITypedPipelineContext 时抛出</exception>
        public static Optional<TValue> GetData<TKey, TValue>(this ViewPipelineExecutor executor, TKey key)
        {
            return executor.GetTypedContext().Get<TKey, TValue>(key);
        }

        /// <summary>
        /// 移除数据（如果上下文支持 ITypedPipelineContext）
        /// </summary>
        /// <returns>是否移除成功</returns>
        public static bool TryRemoveData<TKey, TValue>(this ViewPipelineExecutor executor, TKey key)
        {
            if (executor.Context is ITypedPipelineContext typed)
                return typed.Remove<TKey, TValue>(key);
            return false;
        }

        /// <summary>
        /// 移除数据（如果上下文不支持则抛出异常）
        /// </summary>
        /// <exception cref="InvalidOperationException">当上下文不支持 ITypedPipelineContext 时抛出</exception>
        public static bool RemoveData<TKey, TValue>(this ViewPipelineExecutor executor, TKey key)
        {
            return executor.GetTypedContext().Remove<TKey, TValue>(key);
        }

        /// <summary>
        /// 检查是否包含指定键（如果上下文支持 ITypedPipelineContext）
        /// </summary>
        public static bool TryContainsKey<TKey, TValue>(this ViewPipelineExecutor executor, TKey key)
        {
            if (executor.Context is ITypedPipelineContext typed)
                return typed.ContainsKey<TKey, TValue>(key);
            return false;
        }

        /// <summary>
        /// 检查是否包含指定键（如果上下文不支持则抛出异常）
        /// </summary>
        /// <exception cref="InvalidOperationException">当上下文不支持 ITypedPipelineContext 时抛出</exception>
        public static bool ContainsKey<TKey, TValue>(this ViewPipelineExecutor executor, TKey key)
        {
            return executor.GetTypedContext().ContainsKey<TKey, TValue>(key);
        }
    }
}
