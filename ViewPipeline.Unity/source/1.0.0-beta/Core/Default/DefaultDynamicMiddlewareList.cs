using System.Collections;
using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 默认中间件动态增删列表（包装器）
    /// </summary>
    internal sealed class DefaultDynamicMiddlewareList : IDynamicMiddlewareCollection, IResstable
    {
        private readonly List<IViewMiddleware> collection = new List<IViewMiddleware>();

        /// <inheritdoc/>
        void IDynamicMiddlewareCollection.Add(IViewMiddleware middleware)
        {
            if(middleware == null) throw new System.ArgumentNullException(nameof(middleware));
            collection.Add(middleware);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        public IEnumerator<IViewMiddleware> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        /// <inheritdoc/>
        void IResstable.Reset()
        {
            collection.Clear();
        }
    }
}
