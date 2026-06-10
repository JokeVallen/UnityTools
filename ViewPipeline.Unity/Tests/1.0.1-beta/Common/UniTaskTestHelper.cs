using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// UniTask 协程辅助类
/// </summary>
public static class UniTaskTestHelper
{
    /// <summary>
    /// 将 UniTask 转换为协程
    /// </summary>
    public static IEnumerator AsCoroutine(this UniTask task)
    {
        var awaiter = task.GetAwaiter();
        while (!awaiter.IsCompleted)
        {
            yield return null;
        }

        awaiter.GetResult();
    }

    /// <summary>
    /// 将 UniTask<T> 转换为协程并返回值
    /// </summary>
    public static IEnumerator AsCoroutine<T>(this UniTask<T> task, Action<T> onResult)
    {
        var awaiter = task.GetAwaiter();
        while (!awaiter.IsCompleted)
        {
            yield return null;
        }
        onResult(awaiter.GetResult());
    }

    /// <summary>
    /// 将带 CancellationToken 的 UniTask 转换为协程
    /// </summary>
    public static IEnumerator AsCoroutine(Func<CancellationToken, UniTask> taskFactory, CancellationToken ct = default)
    {
        var task = taskFactory(ct);
        return task.AsCoroutine();
    }

    /// <summary>
    /// 安全执行可能抛出异常的 UniTask 协程
    /// </summary>
    public static IEnumerator AsCoroutineWithExceptionCheck(Func<CancellationToken, UniTask> taskFactory, Action<Exception> onException, CancellationToken ct = default)
    {
        var task = taskFactory(ct);
        var awaiter = task.GetAwaiter();
        while (!awaiter.IsCompleted)
        {
            yield return null;
        }

        try
        {
            awaiter.GetResult();
        }
        catch (Exception ex)
        {
            onException?.Invoke(ex);
        }
    }
}