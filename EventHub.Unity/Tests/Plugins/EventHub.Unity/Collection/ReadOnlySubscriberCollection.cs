#if !EVENTHUB_EXTENSION_ENABLE

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace EventHub.Unity
{
    [Preserve]
    internal class ReadOnlySubscriberCollection<T, TElement> : IReadOnlyCollection<TElement>, IEnumerable<TElement>, IIndexable<TElement>, ICountable
    where T : IReadOnlyCollection<TElement>
    {
        public virtual TElement this[int index]
        {
            get
            {
                if (isIndexable) return ((IIndexable<TElement>)collection)[index];
                throw new NotSupportedException($"The collection does not implement the interface '{nameof(IIndexable<TElement>)}'.");
            }
        }

        public virtual int Count => collection.Count;
        protected readonly T collection;
        protected static readonly bool isIndexable = typeof(IIndexable<TElement>).IsAssignableFrom(typeof(T));

        public ReadOnlySubscriberCollection(T collection)
        {
            this.collection = collection;
        }

        public virtual IEnumerator<TElement> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

#endif