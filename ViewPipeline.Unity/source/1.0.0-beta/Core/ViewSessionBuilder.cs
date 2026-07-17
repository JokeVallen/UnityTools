using System;
using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 视图会话构建器
    /// </summary>
    public sealed class ViewSessionBuilder
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public Guid Key => key;

        /// <summary>
        /// 构建器已执行构建
        /// </summary>
        public bool Built => built;

        private IViewRegistry registry;
        private IViewStackPolicy stackPolicy;
        private Func<IPipelineContext> contextFactory;
        private IPipelineContextCollection contextCollection;
        private Func<IDynamicMiddlewareCollection> dynamicMiddlewareCollectionFactory;
        private readonly Guid key;
        private bool built;

        private readonly List<IViewMiddleware> openMiddlewares = new List<IViewMiddleware>();
        private readonly List<IViewMiddleware> closeMiddlewares = new List<IViewMiddleware>();
        private readonly HashSet<IDynamicMiddlewareProvider> openDynamicMiddlewareProviders = new HashSet<IDynamicMiddlewareProvider>();
        private readonly HashSet<IDynamicMiddlewareProvider> closeDynamicMiddlewareProviders = new HashSet<IDynamicMiddlewareProvider>();

        private ViewSessionBuilder() { key = Guid.NewGuid(); }

        /// <summary>创建一个配置流式构建器</summary>
        public static ViewSessionBuilder Create() 
        { 
            return new ViewSessionBuilder();
        }

        /// <summary>
        /// 自定义视图注册表
        /// </summary>
        /// <param name="registry">视图注册表</param>
        /// <returns>构建器实例</returns>
        /// <exception cref="ArgumentNullException"><paramref name="registry"/> 不能为 null。</exception>
        public ViewSessionBuilder WithRegistry(IViewRegistry registry)
        {
            ThrowErrorIfBuilt();
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            this.registry = registry;
            return this;
        }

        /// <summary>
        /// 自定义导航栈策略
        /// </summary>
        /// <param name="stackPolicy">导航栈策略</param>
        /// <returns>构建器实例</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stackPolicy"/> 不能为 null。</exception>
        public ViewSessionBuilder WithStackPolicy(IViewStackPolicy stackPolicy)
        {
            ThrowErrorIfBuilt();
            if (stackPolicy == null) throw new ArgumentNullException(nameof(stackPolicy));
            this.stackPolicy = stackPolicy;
            return this;
        }

        /// <summary>
        /// 自定义管道上下文工厂方法
        /// </summary>
        /// <param name="contextFactory">管道上下文工厂方法</param>
        /// <returns>构建器实例</returns>
        /// <exception cref="ArgumentNullException"><paramref name="contextFactory"/> 不能为 null。</exception>
        public ViewSessionBuilder WithContextFactory(Func<IPipelineContext> contextFactory) 
        {
            ThrowErrorIfBuilt();
            if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));
            this.contextFactory = contextFactory;
            return this;
        }

        /// <summary>
        /// 自定义管道上下文集合
        /// </summary>
        /// <param name="contextCollection">管道上下文集合</param>
        /// <returns>构建器实例</returns>
        /// <exception cref="ArgumentNullException"><paramref name="contextCollection"/> 不能为 null。</exception>
        public ViewSessionBuilder WithContextCollection(IPipelineContextCollection contextCollection)
        {
            ThrowErrorIfBuilt();
            if (contextCollection == null) throw new ArgumentNullException(nameof(contextCollection));
            this.contextCollection = contextCollection;
            return this;
        }

        /// <summary>
        /// 自定义动态中间件集合工厂方法
        /// </summary>
        /// <param name="dynamicMiddlewareCollectionFactory">动态中间件集合工厂方法</param>
        /// <returns>构建器实例</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dynamicMiddlewareCollectionFactory"/> 不能为 null。</exception>
        public ViewSessionBuilder WithDynamicMiddlewareCollectionFactory(Func<IDynamicMiddlewareCollection> dynamicMiddlewareCollectionFactory) 
        {
            ThrowErrorIfBuilt();
            if (dynamicMiddlewareCollectionFactory == null) throw new ArgumentNullException(nameof(dynamicMiddlewareCollectionFactory));
            this.dynamicMiddlewareCollectionFactory = dynamicMiddlewareCollectionFactory;
            return this;
        }

        /// <summary>
        /// 向激活/打开管线注册静态中间件
        /// </summary>
        /// <param name="middleware">静态中间件</param>
        /// <returns>构建器实例</returns>
        /// <exception cref="ArgumentNullException"><paramref name="middleware"/> 不能为 null。</exception>
        public ViewSessionBuilder AddOpenMiddleware(IViewMiddleware middleware)
        {
            ThrowErrorIfBuilt();
            if (middleware == null) throw new ArgumentNullException(nameof(middleware));
            openMiddlewares.Add(middleware);
            return this;
        }

        /// <summary>
        /// 向激活/打开管线注册动态中间件流式供应器
        /// </summary>
        /// <param name="provider">动态中间件流式供应器</param>
        /// <returns>构建器实例</returns>
        /// <exception cref="ArgumentNullException"><paramref name="provider"/> 不能为 null。</exception>
        public ViewSessionBuilder AddOpenDynamicProvider(IDynamicMiddlewareProvider provider) 
        {
            ThrowErrorIfBuilt();
            if(provider == null) throw new ArgumentNullException(nameof(provider));
            openDynamicMiddlewareProviders.Add(provider);
            return this;
        }

        /// <summary>
        /// 向隐藏/关闭管线注册静态中间件
        /// </summary>
        /// <param name="middleware">静态中间件</param>
        /// <returns>构建器实例</returns>
        /// <exception cref="ArgumentNullException"><paramref name="middleware"/> 不能为 null。</exception>
        public ViewSessionBuilder AddCloseMiddleware(IViewMiddleware middleware)
        {
            ThrowErrorIfBuilt();
            if (middleware == null) throw new ArgumentNullException(nameof(middleware));
            closeMiddlewares.Add(middleware);
            return this;
        }

        /// <summary>
        /// 向隐藏/关闭管线注册动态中间件流式供应器
        /// </summary>
        /// <param name="provider">动态中间件流式供应器</param>
        /// <returns>构建器实例</returns>
        /// <exception cref="ArgumentNullException"><paramref name="provider"/> 不能为 null。</exception>
        public ViewSessionBuilder AddCloseDynamicProvider(IDynamicMiddlewareProvider provider)
        {
            ThrowErrorIfBuilt();
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            closeDynamicMiddlewareProviders.Add(provider);
            return this;
        }

        /// <summary>
        /// 构建 UI 会话实例
        /// </summary>
        /// <returns>UI 会话实例</returns>
        public IExtendedViewSession Build()
        {
            ThrowErrorIfBuilt();
            built = true;

            var allMiddlewares = new List<IViewMiddleware>();
            allMiddlewares.AddRange(openMiddlewares);
            allMiddlewares.AddRange(closeMiddlewares);
            Validation.ValidateAll(key, allMiddlewares);
            Validation.Dispose(key);

            var finalRegistry = registry ?? new DefaultViewRegistry();
            var finalStackPolicy = stackPolicy ?? new DefaultViewStackPolicy();
            var finalContextFactory = contextFactory ?? (() => DefaultPipelineContext.Empty);
            var finalPool = contextCollection ?? new DefaultPipelineContextCollection(finalContextFactory);
            var finalDynamicMiddlewareCollectionFactory = dynamicMiddlewareCollectionFactory ?? (() => new DefaultDynamicMiddlewareList());

            IPipelineEngineInternal finalOpenEngine = new UIPipelineEngine(
                key,
                openMiddlewares,
                openDynamicMiddlewareProviders,
                finalDynamicMiddlewareCollectionFactory
            );

            IPipelineEngineInternal finalCloseEngine = new UIPipelineEngine(
                key,
                closeMiddlewares,
                closeDynamicMiddlewareProviders,
                finalDynamicMiddlewareCollectionFactory
            );

            return new ViewSession(
                key,
                finalRegistry,
                finalStackPolicy,
                finalOpenEngine,
                finalCloseEngine,
                finalPool
            );
        }

        private void ThrowErrorIfBuilt() 
        {
            if (built)
                throw new InvalidOperationException("[ViewPipeline] The builder cannot be reused.");
        }
    }
}