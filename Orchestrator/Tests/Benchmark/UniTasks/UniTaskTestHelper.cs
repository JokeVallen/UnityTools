using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;

public static class UniTaskTestHelper
{
    public static IEnumerator RunTest(Func<CancellationToken, UniTask> testBody)
    {
        var cts = new CancellationTokenSource();
        var task = testBody(cts.Token);
        while (!task.Status.IsCompleted())
        {
            if (task.Status == UniTaskStatus.Faulted)
                throw task.AsTask().Exception; // 重新抛出异常
            yield return null;
        }

        // 检查是否被取消
        if (task.Status == UniTaskStatus.Canceled)
            throw new OperationCanceledException();
        // 成功完成，无需断言
        yield break;
    }
}