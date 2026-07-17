using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 执行器构建器
    /// </summary>
    /// <remarks>
    /// <para>通过链式 API 注册处理器、注入功能、配置工厂，最终构建不可变的同步/异步执行器。</para>
    /// <para>
    /// 示例：
    /// <code>
    /// var executor = DefaultExecutorBuilder.Create()
    ///     .UseProcessor&lt;MyAttr, MyProcessor&gt;()
    ///     .UseFeature&lt;ILogger&gt;(new ConsoleLogger())
    ///     .Build();
    /// </code>
    /// </para>
    /// </remarks>
    public sealed class DefaultExecutorBuilder
    {
        private IReadOnlyDictionary<Type, IFeature> Features
        {
            get 
            { 
                if(readOnlyFeatures == null)
                    readOnlyFeatures = new ReadOnlyDictionary<Type, IFeature>(features);
                return readOnlyFeatures;
            }
        }
        private readonly ProcessorRegistry registry = new ProcessorRegistry();
        private readonly Dictionary<Type,IFeature> features = new Dictionary<Type,IFeature>();
        private IReadOnlyDictionary<Type, IFeature> readOnlyFeatures;
        private IProcessorFactory factory = TransientProcessorFactory.Default;
        private IExceptionHandler exceptionHandler = NullExceptionHandler.Instance;
        private bool built;

        private DefaultExecutorBuilder() { }

        /// <summary>创建构建器</summary>
        public static DefaultExecutorBuilder Create() => new DefaultExecutorBuilder();

        /// <summary>注册处理器（泛型）</summary>
        public DefaultExecutorBuilder UseProcessor<TAttr, TProcessor>() where TAttr : EasyAttribute where TProcessor : class
        => UseProcessor(typeof(TAttr), typeof(TProcessor));

        /// <summary>注册处理器</summary>
        public DefaultExecutorBuilder UseProcessor(Type attributeType, Type processorType)
        {
            ThrowErrorIfBuilt();
            registry.Register(new ProcessorDescriptor(attributeType, processorType));
            return this;
        }

        /// <summary>
        /// 注入全局功能实例（泛型）
        /// </summary>
        /// <remarks>
        /// <para>功能实例将在所有通过此构建器创建的上下文中可用，处理器可通过 <see cref="Extensions.GetFeature{TFeature}(IContext, TFeature)"/> 获取。</para>
        /// <para>
        /// <b>Features 与 Items 的职责区别</b>：
        /// <list type="bullet">
        ///   <item>
        ///     <description><see cref="Features"/>：用于注入全局、稳定的基础设施服务（如 <c>ILogger</c>、<c>ICache</c>、<c>ISerializer</c>）。
        ///     构建时注入，处理器只读，不可修改。</description>
        ///   </item>
        ///   <item>
        ///     <description><see cref="IContext.Items"/>：用于处理器间的临时数据传递（如事务对象、校验令牌）。
        ///     处理器可通过 <c>SetItem</c> / <c>GetItem</c> 读写，仅限当前上下文的一次执行链。</description>
        ///   </item>
        /// </list>
        /// </para>
        /// <para>
        /// 若处理器需要根据运行时条件动态选择不同实现，请使用工厂模式（将工厂作为全局功能注入），
        /// 或通过 <see cref="IContext.Items"/> 传递特定实例。
        /// </para>
        /// </remarks>
        /// <typeparam name="TFeature">功能类型，必须实现 <see cref="IFeature"/></typeparam>
        /// <param name="feature">功能实例</param>
        public DefaultExecutorBuilder UseFeature<TFeature>(TFeature feature) where TFeature : IFeature
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            features[typeof(TFeature)] = feature;
            return this;
        }

        /// <summary>
        /// 注入全局功能实例（非泛型）
        /// </summary>
        /// <remarks>
        /// <para>以实例的具体类型作为键。优先使用泛型版本以确保键的一致性。</para>
        /// <inheritdoc cref="UseFeature{TFeature}(TFeature)" path="/remarks/para[position() > 1]"/> 
        /// </remarks>
        /// <param name="feature">功能实例</param>
        public DefaultExecutorBuilder UseFeature(IFeature feature) 
        {
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            features[feature.GetType()] = feature;
            return this;
        }

        /// <summary>扫描程序集</summary>
        public DefaultExecutorBuilder Scan(Assembly assembly)
        {
            ThrowErrorIfBuilt();
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            ScanAssembly(assembly);
            return this;
        }

        /// <summary>扫描多个程序集</summary>
        public DefaultExecutorBuilder Scan(params Assembly[] assemblies)
        {
            ThrowErrorIfBuilt();
            if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));
            foreach (var assembly in assemblies)
            {
                if (assembly == null) continue;
                ScanAssembly(assembly);
            }
            return this;
        }

        /// <summary>设置工厂</summary>
        public DefaultExecutorBuilder UseFactory(IProcessorFactory factory)
        {
            ThrowErrorIfBuilt();
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            return this;
        }

        /// <summary>设置异常处理器</summary>
        public DefaultExecutorBuilder UseExceptionHandler(IExceptionHandler handler)
        {
            ThrowErrorIfBuilt();
            exceptionHandler = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <summary>构建同步执行器</summary>
        public IExecutor Build()
        {
            FinalizeAndSeal();
            return new DefaultExecutor(registry, factory, exceptionHandler, Features);
        }

        /// <summary>构建异步执行器</summary>
        public IExecutorAsync BuildAsync()
        {
            FinalizeAndSeal();
            return new DefaultExecutorAsync(registry, factory, exceptionHandler, Features);
        }

        private void FinalizeAndSeal()
        {
            ThrowErrorIfBuilt();
            built = true;
            registry.Seal();
        }

        private void ScanAssembly(Assembly assembly)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types ?? Array.Empty<Type>();
            }

            var processorTypes = new List<Type>(types.Length);
            foreach (var t in types)
            {
                if (t != null && t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition)
                    processorTypes.Add(t);
            }

            processorTypes.Sort(CompareTypesByFullName);

            foreach (var processorType in processorTypes)
            {
                foreach (var iface in processorType.GetInterfaces())
                {
                    if (!iface.IsGenericType) continue;

                    var genericDef = iface.GetGenericTypeDefinition();
                    bool isSyncProcessor = genericDef == typeof(IProcessor<>);
                    bool isAsyncProcessor = genericDef == typeof(IProcessorAsync<>);

                    if (!isSyncProcessor && !isAsyncProcessor) continue;

                    var attrType = iface.GetGenericArguments()[0];

                    if (IsAlreadyRegistered(attrType, processorType)) continue;

                    try
                    {
                        registry.Register(new ProcessorDescriptor(attrType, processorType));
                    }
                    catch (ArgumentException)
                    {
                       
                    }
                }
            }
        }

        private bool IsAlreadyRegistered(Type attributeType, Type processorType)
        {
            var existing = registry.GetDescriptors(attributeType);
            for (int i = 0; i < existing.Count; i++)
                if (existing[i].ProcessorType == processorType)
                    return true;
            return false;
        }

        private static int CompareTypesByFullName(Type a, Type b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal);

        private void ThrowErrorIfBuilt()
        {
            if (built) 
                throw new InvalidOperationException("This builder has already been used to build an executor and cannot be reused.");
        }
    }
}