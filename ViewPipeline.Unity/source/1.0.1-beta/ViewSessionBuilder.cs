using System;
using System.Collections.Generic;
using System.Linq;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 视图会话构建器
    /// </summary>
    public sealed class ViewSessionBuilder : ISessionKeyGetter, IFullSnapshotable<ViewSessionBuilderSnapshot>
    {
        /// <inheritdoc/>
        public Guid Key => key;

        /// <summary>
        /// 构建器已执行构建
        /// </summary>
        public bool Built => built;

        private Func<IPipelineContext> contextFactory;
        private ICollection<IPipelineContext> contextCollection;
        private Func<IDynamicMiddlewareCollection> dynamicMiddlewareCollectionFactory;
        private readonly Guid key;
        private Type contextType;
        private bool built;

        private readonly List<IViewMiddleware> openMiddlewares = new List<IViewMiddleware>();
        private readonly List<IViewMiddleware> closeMiddlewares = new List<IViewMiddleware>();
        private readonly List<IExtension> extensions = new List<IExtension>();
        private readonly HashSet<IDynamicMiddlewareProvider> openDynamicMiddlewareProviders = new HashSet<IDynamicMiddlewareProvider>();
        private readonly HashSet<IDynamicMiddlewareProvider> closeDynamicMiddlewareProviders = new HashSet<IDynamicMiddlewareProvider>();

        private ViewSessionBuilder()
        {
            key = Guid.NewGuid();
            contextType = typeof(DefaultPipelineContext);
            SnapshotCache.OnRefresh += OnSnapshotRefresh;
        }

        /// <summary>创建一个视图会话构建器实例</summary>
        public static ViewSessionBuilder Create() 
        { 
            return new ViewSessionBuilder();
        }

        /// <summary>
        /// 自定义管道上下文工厂方法
        /// </summary>
        /// <param name="contextFactory">管道上下文工厂方法</param>
        /// <returns>构建器实例</returns>
        /// <exception cref="ArgumentNullException"><paramref name="contextFactory"/> 不能为 null。</exception>
        public ViewSessionBuilder WithContextFactory<TContextType>(Func<IPipelineContext> contextFactory) where TContextType : IPipelineContext
        {
            ThrowErrorIfBuilt();
            if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));
            contextType = typeof(TContextType);
            this.contextFactory = contextFactory;
            return this;
        }

        /// <summary>
        /// 使用强类型可读写上下文
        /// </summary>
        /// <returns>构建器实例</returns>
        public ViewSessionBuilder WithTypedContext() 
        {
            return WithContextFactory<TypedPipelineContext>(() => new TypedPipelineContext());
        }

        /// <summary>
        /// 自定义管道上下文集合
        /// </summary>
        /// <param name="contextCollection">管道上下文集合</param>
        /// <returns>构建器实例</returns>
        /// <exception cref="ArgumentNullException"><paramref name="contextCollection"/> 不能为 null。</exception>
        public ViewSessionBuilder WithContextCollection<TContextType>(ICollection<IPipelineContext> contextCollection) where TContextType : IPipelineContext
        {
            ThrowErrorIfBuilt();
            if (contextCollection == null) throw new ArgumentNullException(nameof(contextCollection));
            contextType = typeof(TContextType);
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
        /// 向视图激活/打开管线添加静态中间件
        /// </summary>
        /// <param name="middleware">静态中间件</param>
        /// <returns>构建器实例</returns>
        /// <exception cref="ArgumentNullException"><paramref name="middleware"/> 不能为 null。</exception>
        public ViewSessionBuilder AddOpenMiddleware(IViewMiddleware middleware)
        {
            ThrowErrorIfBuilt();
            if (middleware == null) throw new ArgumentNullException(nameof(middleware));
            if (!ViewPipelineUtility.Validate(middleware)) return this;
            openMiddlewares.Add(middleware);
            return this;
        }

        /// <summary>
        /// 向视图激活/打开管线添加动态中间件流式供应器
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
        /// 向视图隐藏/关闭管线添加静态中间件
        /// </summary>
        /// <param name="middleware">静态中间件</param>
        /// <returns>构建器实例</returns>
        /// <exception cref="ArgumentNullException"><paramref name="middleware"/> 不能为 null。</exception>
        public ViewSessionBuilder AddCloseMiddleware(IViewMiddleware middleware)
        {
            ThrowErrorIfBuilt();
            if (middleware == null) throw new ArgumentNullException(nameof(middleware));
            if (!ViewPipelineUtility.Validate(middleware)) return this;
            closeMiddlewares.Add(middleware);
            return this;
        }

        /// <summary>
        /// 向视图隐藏/关闭管线添加动态中间件流式供应器
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
        /// 添加扩展包
        /// </summary>
        /// <param name="extension">扩展包</param>
        /// <exception cref="ArgumentNullException"><paramref name="extension"/> 不能为 null。</exception>
        public ViewSessionBuilder AddExtension(IExtension extension)
        {
            ThrowErrorIfBuilt();
            if (extension == null) throw new ArgumentNullException(nameof(extension));
            if (!ViewPipelineUtility.Validate(extension))
                throw new ArgumentException("[ViewPipeline] The extension has not passed the precondition verification.");

            extensions.Add(extension);

            foreach (var m in extension.GetMiddlewares(PipelineDirection.Open))
                AddOpenMiddleware(m);

            foreach (var m in extension.GetMiddlewares(PipelineDirection.Close))
                AddCloseMiddleware(m);

            foreach (var p in extension.GetDynamicProviders(PipelineDirection.Open))
                AddOpenDynamicProvider(p);

            foreach (var p in extension.GetDynamicProviders(PipelineDirection.Close))
                AddCloseDynamicProvider(p);

            extension.Initialize();

            return this;
        }

        /// <summary>
        /// 自定义日志记录器
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <returns>构建器实例</returns>
        /// <exception cref="ArgumentNullException"><paramref name="logger"/> 不能为 null。</exception>
        public ViewSessionBuilder WithLogger(ILogger logger)
        {
            ThrowErrorIfBuilt();
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            Log.Logger = logger;
            return this;
        }

        /// <summary>
        /// 自定义中间件执行策略
        /// </summary>
        /// <param name="executionPolicy">中间件执行策略</param>
        /// <returns>构建器实例</returns>
        /// <exception cref="ArgumentNullException"><paramref name="executionPolicy"/> 不能为 null。</exception>
        public ViewSessionBuilder WithMiddlewareExecutionPolicy(IExecutionPolicy executionPolicy)
        {
            ThrowErrorIfBuilt();
            if (executionPolicy == null) throw new ArgumentNullException(nameof(executionPolicy));
            ExecutionPolicy.Register(key, executionPolicy);
            return this;
        }

        /// <summary>
        /// 构建视图会话实例
        /// </summary>
        /// <returns>视图会话实例</returns>
        public IExtendedViewSession Build()
        {
            ThrowErrorIfBuilt();
            built = true;

            var finalContextFactory = contextFactory;
            if (finalContextFactory == null) finalContextFactory = () => DefaultPipelineContext.Empty;
            var finalPool = contextCollection;
            if (finalPool == null) finalPool = new DefaultPooledCollection<IPipelineContext>(finalContextFactory);
            var finalDynamicMiddlewareCollectionFactory = dynamicMiddlewareCollectionFactory;
            if (finalDynamicMiddlewareCollectionFactory == null) finalDynamicMiddlewareCollectionFactory = () => new DefaultDynamicMiddlewareList();

            IPipelineEngineInternal finalOpenEngine = new ViewPipelineEngine(
                PipelineDirection.Open,
                key,
                openMiddlewares,
                openDynamicMiddlewareProviders,
                finalDynamicMiddlewareCollectionFactory
            );

            IPipelineEngineInternal finalCloseEngine = new ViewPipelineEngine(
                PipelineDirection.Close,
                key,
                closeMiddlewares,
                closeDynamicMiddlewareProviders,
                finalDynamicMiddlewareCollectionFactory
            );

            var session = new ViewSession(
                key,
                finalOpenEngine,
                finalCloseEngine,
                finalPool,
                extensions
            );

            ViewSessionRegistry.Register(key, session);
            SnapshotCache.OnRefresh -= OnSnapshotRefresh;
            SnapshotCacheInternal.Remove<ViewSessionBuilderSnapshot>(key);
            return session;
        }

        /// <inheritdoc/>
        public ViewSessionBuilderSnapshot GetFullSnapshot()
        {
            return new ViewSessionBuilderSnapshot(
                key,
                built,
                contextType,
                openMiddlewares.Where(m => m is IFullSnapshotable<MiddlewareSnapshot>).Select(m => ((IFullSnapshotable<MiddlewareSnapshot>)m).GetFullSnapshot()).ToArray(),
                closeMiddlewares.Where(m => m is IFullSnapshotable<MiddlewareSnapshot>).Select(m => ((IFullSnapshotable<MiddlewareSnapshot>)m).GetFullSnapshot()).ToArray(),
                extensions.Where(m => m is IFullSnapshotable<ExtensionSnapshot>).Select(m => ((IFullSnapshotable<ExtensionSnapshot>)m).GetFullSnapshot()).ToArray(),
                openDynamicMiddlewareProviders.Where(m => m is IFullSnapshotable<DynamicMiddlewareProviderSnapshot>)
                .Select(m => ((IFullSnapshotable<DynamicMiddlewareProviderSnapshot>)m).GetFullSnapshot()).ToArray(),
                closeDynamicMiddlewareProviders.Where(m => m is IFullSnapshotable<DynamicMiddlewareProviderSnapshot>)
                .Select(m => ((IFullSnapshotable<DynamicMiddlewareProviderSnapshot>)m).GetFullSnapshot()).ToArray()
            );
        }

        private void OnSnapshotRefresh(Guid key, Type type)
        {
            if (this.key != key) return;
            if (type != null && type != typeof(ViewSessionBuilderSnapshot)) return;
            var snapshot = GetFullSnapshot();
            SnapshotCacheInternal.Store(key, snapshot);
        }

        private void ThrowErrorIfBuilt() 
        {
            if (built)
                throw new InvalidOperationException("[ViewPipeline] The builder cannot be reused.");
        }
    }
}