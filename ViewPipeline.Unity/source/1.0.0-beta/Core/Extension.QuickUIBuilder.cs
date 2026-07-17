using System;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static partial class Extension
    {
        /// <summary>
        /// 自定义日志记录器
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <returns>构建器实例</returns>
        /// <exception cref="ArgumentNullException"><paramref name="logger"/> 不能为 null。</exception>
        public static ViewSessionBuilder WithLogger(this ViewSessionBuilder builder, ILogger logger)
        {
            if (builder.Built) throw new InvalidOperationException("[ViewPipeline] The builder cannot be reused.");
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            Log.Logger = logger;
            return builder;
        }

        /// <summary>
        /// 添加扩展包
        /// </summary>
        /// <param name="extension">扩展包</param>
        /// <exception cref="ArgumentNullException"><paramref name="extension"/> 不能为 null。</exception>
        public static ViewSessionBuilder AddExtension(this ViewSessionBuilder builder, IExtension extension)
        {
            if(builder.Built) throw new InvalidOperationException("[ViewPipeline] The builder cannot be reused.");
            if (extension == null) throw new ArgumentNullException(nameof(extension));

            // 装配静态中间件
            foreach (var m in extension.GetMiddlewares(PipelineDirection.Open))
                builder.AddOpenMiddleware(m);

            foreach (var m in extension.GetMiddlewares(PipelineDirection.Close))
                builder.AddCloseMiddleware(m);

            // 装配动态供应器
            foreach (var p in extension.GetDynamicProviders(PipelineDirection.Open))
                builder.AddOpenDynamicProvider(p);

            foreach (var p in extension.GetDynamicProviders(PipelineDirection.Close))
                builder.AddCloseDynamicProvider(p);

            // 装配验证器
            foreach (var v in extension.GetMiddlewareValidators())
                Validation.RegisterValidator(builder.Key, v);

            // 初始化
            extension.Initialize();

            return builder;
        }

        /// <summary>
        /// 设置中间件执行策略
        /// </summary>
        /// <param name="executionPolicy">中间件执行策略</param>
        /// <returns>构建器实例</returns>
        /// <exception cref="ArgumentNullException"><paramref name="executionPolicy"/> 不能为 null。</exception>
        public static ViewSessionBuilder SetMiddlewareExecutionPolicy(this ViewSessionBuilder builder, IMiddlewareExecutionPolicy executionPolicy)
        {
            if (builder.Built) throw new InvalidOperationException("[ViewPipeline] The builder cannot be reused.");
            if (executionPolicy == null) throw new ArgumentNullException(nameof(executionPolicy));
            ExecutionPolicy.Register(builder.Key, executionPolicy);
            return builder;
        }
    }
}
