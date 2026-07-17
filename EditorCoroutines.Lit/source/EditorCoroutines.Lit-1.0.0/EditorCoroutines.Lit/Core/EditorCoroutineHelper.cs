#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 编辑器协程辅助工具类
/// </summary>
internal static class EditorCoroutineHelper
{
    /// <summary>
    /// 包装协程，支持嵌套协程自动等待，并捕获异常
    /// </summary>
    public static IEnumerator WrapRoutine(IEnumerator routine, Action<Exception> onException = null)
    {
        Stack<IEnumerator> stack = new Stack<IEnumerator>();
        stack.Push(routine);

        while (stack.Count > 0)
        {
            var current = stack.Peek();
            object yielded = null;
            bool hasYield = false;

            try
            {
                if (!current.MoveNext())
                {
                    stack.Pop();
                    continue;
                }

                if (current.Current is IEnumerator nested)
                {
                    stack.Push(nested);
                }
                else
                {
                    yielded = current.Current;
                    hasYield = true;
                }
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
                stack.Pop();
            }

            if (hasYield)
                yield return yielded;
        }
    }
}

#endif