using System.Collections;
using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    internal sealed class DefaultDynamicMiddlewareList : IDynamicMiddlewareCollection, IResettable
    {
        private readonly List<IViewMiddleware> collection = new List<IViewMiddleware>();

        void IDynamicMiddlewareCollection.Add(IViewMiddleware middleware)
        {
            if(middleware == null) throw new System.ArgumentNullException(nameof(middleware));
            collection.Add(middleware);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IViewMiddleware> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        void IResettable.Reset()
        {
            collection.Clear();
        }
    }
}
