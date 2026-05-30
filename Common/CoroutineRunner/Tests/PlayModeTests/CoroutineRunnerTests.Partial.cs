using System;
using System.Collections;
using System.Threading.Tasks;
using CoroutineRunner;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CoroutineRunnerAwaiterTests
{
    [UnityTest]
    public IEnumerator GetAwaiter_CompletesWhenCoroutineFinishes()
    {
        bool asyncCompleted = false;
        bool coroutineCompleted = false;
        Exception asyncException = null;

        IEnumerator Coro()
        {
            yield return new WaitForSeconds(0.1f);
            coroutineCompleted = true;
        }

        var token = GlobalCoroutineRunner.Run(Coro());

        // 创建临时 GameObject 和 MonoBehaviour 来执行 async/await
        var go = new GameObject("AsyncTestRunner");
        var runner = go.AddComponent<AsyncAwaitTestRunner>();

        // 启动异步等待
        runner.RunAsyncTest(async () =>
        {
            try
            {
                await token;
                asyncCompleted = true;
            }
            catch (Exception ex)
            {
                asyncException = ex;
            }
            finally
            {
                // 通知主测试循环继续
                runner.OnComplete();
            }
        });

        // 等待异步测试完成（最多 5 秒超时）
        float timeout = Time.time + 5f;
        while (!runner.IsComplete && Time.time < timeout)
            yield return null;

        // 清理临时对象
        UnityEngine.Object.Destroy(go);

        // 断言
        Assert.IsNull(asyncException, $"Async await failed: {asyncException}");
        Assert.IsTrue(asyncCompleted, "await token did not complete");
        Assert.IsTrue(coroutineCompleted, "Coroutine did not complete");
        Assert.IsTrue(token.IsDone());
    }

    /// <summary>
    /// 内部测试辅助组件：用于在 MonoBehaviour 的 Update 中执行 async 代码
    /// </summary>
    private class AsyncAwaitTestRunner : MonoBehaviour
    {
        private bool isComplete;
        private Action asyncAction;

        public bool IsComplete => isComplete;

        public void RunAsyncTest(Func<Task> asyncTestFunc)
        {
            asyncAction = async () => await asyncTestFunc();
        }

        public void OnComplete()
        {
            isComplete = true;
        }

        private void Start()
        {
            // 在 Start 中触发异步任务（不阻塞 Unity 主循环）
            _ = ExecuteAsync();
        }

        private async Task ExecuteAsync()
        {
            if (asyncAction != null)
                asyncAction();
            else
                OnComplete(); // 无任务时直接完成
        }
    }
}