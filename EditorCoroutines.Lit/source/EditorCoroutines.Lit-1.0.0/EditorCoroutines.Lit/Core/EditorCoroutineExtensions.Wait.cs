#if UNITY_EDITOR

using System;
using System.Collections;
using UnityEditor;

/// <summary>
/// 编辑器协程扩展方法集合
/// </summary>
public static partial class EditorCoroutineExtensions
{
    /// <summary>
    /// 在编辑器中等待指定秒数，可选取消
    /// </summary>
    public static IEnumerator WaitSeconds(float seconds, EditorCoroutineCancelToken token = null)
    {
        float start = (float)EditorApplication.timeSinceStartup;
        while ((float)EditorApplication.timeSinceStartup - start < seconds)
        {
            if (token?.IsCancelled == true)
                yield break;

            yield return null;
        }
    }

    /// <summary>
    /// 在编辑器中等待指定毫秒数
    /// </summary>
    public static IEnumerator WaitMilliseconds(float milliseconds, EditorCoroutineCancelToken token = null)
    {
        return WaitSeconds(milliseconds * 0.001f, token);
    }

    /// <summary>
    /// 在编辑器中等待下一帧
    /// </summary>
    public static IEnumerator WaitFrame(EditorCoroutineCancelToken token = null)
    {
        if (token?.IsCancelled == true)
            yield break;

        yield return null;
    }

    /// <summary>
    /// 等待条件为真
    /// </summary>
    public static IEnumerator WaitUntil(Func<bool> condition, EditorCoroutineCancelToken token = null)
    {
        while (!condition())
        {
            if (token?.IsCancelled == true)
                yield break;

            yield return null;
        }
    }

    /// <summary>
    /// 等待条件为真，带超时
    /// </summary>
    public static IEnumerator WaitUntil(Func<bool> condition, float timeoutSeconds, EditorCoroutineCancelToken token = null)
    {
        float start = (float)EditorApplication.timeSinceStartup;
        while (!condition())
        {
            if (token?.IsCancelled == true)
                yield break;

            if ((float)EditorApplication.timeSinceStartup - start > timeoutSeconds)
                yield break;

            yield return null;
        }
    }

    /// <summary>
    /// 延迟执行一个操作
    /// </summary>
    public static IEnumerator Delay(Action action, float seconds, EditorCoroutineCancelToken token = null)
    {
        yield return WaitSeconds(seconds, token);
        if (token?.IsCancelled != true)
            action?.Invoke();
    }
}

#endif