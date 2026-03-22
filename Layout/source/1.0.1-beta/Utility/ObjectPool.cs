using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UGUI.Layout.Extension
{
    internal class ObjectPool<T> where T : new()
    {
        public int Count { get; private set; }
        public int ActiveCount => Count - InactiveCount;
        public int InactiveCount => stack.Count;

        private readonly Stack<T> stack = new Stack<T>();
        private readonly UnityAction<T> onGet;
        private readonly UnityAction<T> onRelease;

        public ObjectPool(UnityAction<T> onGet, UnityAction<T> onRelease)
        {
            this.onGet = onGet;
            this.onRelease = onRelease;
        }

        public T Get()
        {
            T element;
            if (stack.Count == 0)
            {
                element = new T();
                Count++;
            }
            else
            {
                element = stack.Pop();
            }

            onGet?.Invoke(element);
            return element;
        }

        public void Release(T element)
        {
            if (stack.Count > 0 && ReferenceEquals(stack.Peek(), element))
                Debug.LogError("该对象已被回收至对象池");
            onRelease?.Invoke(element);
            stack.Push(element);
        }
    }
}