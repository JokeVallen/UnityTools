using System;
using System.Collections;
using System.Collections.Generic;

namespace CodeGenerator
{
    /// <summary>
    /// 代码生成器中介者基类
    /// </summary>
    /// <typeparam name="TGenerator">代码生成器类型</typeparam>
    public abstract class BaseGeneratorMediator<TGenerator> : IGeneratorMediator<TGenerator>, IEnumerable<KeyValuePair<Type, BaseGeneratorMediator<TGenerator>.MetaData>>, IReadOnlyDictionary<Type, BaseGeneratorMediator<TGenerator>.MetaData>, IReadOnlyCollection<KeyValuePair<Type, BaseGeneratorMediator<TGenerator>.MetaData>>
    where TGenerator : class, IGenerator
    {
        /// <summary>
        /// 元数据
        /// </summary>
        public readonly struct MetaData
        {
            /// <summary>
            /// 模板路径
            /// </summary>
            public string TemplatePath { get; }

            /// <summary>
            /// 输出路径
            /// </summary>
            public string OutputPath { get; }

            /// <summary>
            /// 生成器
            /// </summary>
            public TGenerator Generator { get; }

            /// <param name="templatePath">模板路径</param>
            /// <param name="outputPath">输出路径</param>
            /// <param name="generator">生成器</param>
            public MetaData(string templatePath, string outputPath, TGenerator generator)
            {
                TemplatePath = templatePath;
                OutputPath = outputPath;
                Generator = generator;
            }
        }

        public MetaData this[Type key] => generators[key];
        public IEnumerable<Type> Keys => generators.Keys;
        public IEnumerable<MetaData> Values => generators.Values;
        public int Count => generators.Count;
        protected readonly Dictionary<Type, MetaData> generators = new Dictionary<Type, MetaData>();

        public bool ContainsKey(Type key) => generators.ContainsKey(key);
        public IEnumerator<KeyValuePair<Type, MetaData>> GetEnumerator() => generators.GetEnumerator();
        public bool TryGetValue(Type key, out MetaData value) => generators.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public virtual void Clear()
        {
            foreach (var meta in generators.Values)
            {
                DisposeInstance(meta.Generator);
            }

            generators.Clear();
        }

        /// <inheritdoc/>
        public abstract void Rescan();

        /// <inheritdoc/>
        public abstract void Run<T>() where T : TGenerator;

        /// <inheritdoc/>
        public abstract void RunAll();

        /// <summary>
        /// 释放 <see cref="IDisposable"/> 对象
        /// </summary>
        /// <param name="instance"><see cref="IDisposable"/> 对象</param>
        protected static void DisposeInstance(object instance)
        {
            if (instance is IDisposable disposable)
                disposable.Dispose();
        }
    }
}