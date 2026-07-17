using System;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 从共享状态字典获取值
        /// </summary>
        /// <remarks>
        /// <para>用于处理器间传递临时数据。键不存在或类型不匹配时返回 <paramref name="defaultValue"/>。</para>
        /// <para>与 <see cref="Features"/> 不同，<see cref="Items"/> 是处理器可读写的临时存储空间。</para>
        /// </remarks>
        public static TValue GetItem<TValue>(this IContext context, string key, TValue defaultValue = default)
        {
            if (context.Items.TryGetValue(key, out var value) && value is TValue tValue)
                return tValue;
            return defaultValue;
        }

        /// <summary>
        /// 尝试从共享状态字典获取值
        /// </summary>
        /// <remarks>
        /// <para>用于处理器间传递临时数据。键不存在或类型不匹配时返回 <paramref name="defaultValue"/>。</para>
        /// <para>与 <see cref="Features"/> 不同，<see cref="Items"/> 是处理器可读写的临时存储空间。</para>
        /// </remarks>
        public static bool TryGetItem<TValue>(this IContext context, string key, out TValue value, TValue defaultValue = default) 
        {
            if (context.Items.TryGetValue(key, out var rawValue) && rawValue is TValue tValue)
            {
                value = tValue;
                return true;
            }

            value = defaultValue;
            return false;
        }

        /// <summary>
        /// 从Items中移除指定键所对应的Item
        /// </summary>
        public static void RemoveItem(this IContext context, string key) 
        {
            if (key == null) return;
            if (!(context is IContextWriter writer)) throw new ArgumentException("The context is read-only.");
            writer.RemoveItem(key);
        }

        /// <summary>
        /// 向共享状态字典写入值
        /// </summary>
        /// <remarks>
        /// <para>处理器可用此方法在链内传递临时数据（如事务对象、校验令牌）。</para>
        /// <para>若上下文不可写（例如外部自定义的只读上下文），会抛出 <see cref="ArgumentException"/>。</para>
        /// </remarks>
        public static void SetItem(this IContext context, string key, object value) 
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            if (!(context is IContextWriter writer)) throw new ArgumentException("The context is read-only.");
            writer.SetItem(key, value);
        }

        /// <summary>
        /// 从全局功能扩展槽获取功能实例
        /// </summary>
        /// <remarks>
        /// <para>功能实例通过 <see cref="DefaultExecutorBuilder.UseFeature{TFeature}(TFeature)"/> 注入，全局共享且不可被处理器修改。</para>
        /// <para>
        /// 若不存在对应类型的功能，返回 <paramref name="defaultValue"/>。
        /// 对于需要根据运行时条件动态选择不同实现的场景，建议将工厂接口作为全局功能注入，
        /// 而非通过此方法获取后自行判断。
        /// </para>
        /// </remarks>
        public static TFeature GetFeature<TFeature>(this IContext context, TFeature defaultValue = default) where TFeature : IFeature
        {
            return context.Features.TryGetValue(typeof(TFeature), out var feature)
                ? (TFeature)feature
                : defaultValue;
        }

        /// <summary>
        /// 尝试从全局功能扩展槽获取功能实例
        /// </summary>
        /// <remarks>
        /// <para>功能实例通过 <see cref="DefaultExecutorBuilder.UseFeature{TFeature}(TFeature)"/> 注入，全局共享且不可被处理器修改。</para>
        /// <para>
        /// 若不存在对应类型的功能，返回 <paramref name="defaultValue"/>。
        /// 对于需要根据运行时条件动态选择不同实现的场景，建议将工厂接口作为全局功能注入，
        /// 而非通过此方法获取后自行判断。
        /// </para>
        /// </remarks>
        public static bool TryGetFeature<TFeature>(this IContext context, out TFeature feature, TFeature defaultValue = default) where TFeature : IFeature 
        {
            if (context.Features.TryGetValue(typeof(TFeature), out var rawFeature) && rawFeature is TFeature typed)
            {
                feature = typed;
                return true;
            }

            feature = defaultValue;
            return false;
        }
    }
}