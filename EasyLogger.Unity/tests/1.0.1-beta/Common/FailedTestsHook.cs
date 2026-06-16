#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

[InitializeOnLoad]
public static class FailedTestsHook
{
    private static readonly string ReportPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/FailedTestsReport.txt"));
    private static readonly string Separator = new string('-', 80);

    static FailedTestsHook()
    {
        var api = ScriptableObject.CreateInstance<TestRunnerApi>();
        api.RegisterCallbacks(new TestRunCallbacks());
    }

    private class TestRunCallbacks : ICallbacks
    {
        public void RunStarted(ITestAdaptor testsToRun)
        {
            // 运行开始时可以执行清理逻辑
        }

        public void TestStarted(ITestAdaptor test)
        {
            // 单个测试开始
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            // 单个测试结束
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            // 只有测试失败时才处理报告
            if (result.TestStatus != TestStatus.Failed)
            {
                UnityEngine.Debug.Log("Unity Test Runner: 所有测试均已通过。");
                return;
            }

            // 使用 Task 避免阻塞主线程 IO
            System.Threading.Tasks.Task.Run(() => GenerateReport(result));
        }

        private void GenerateReport(ITestResultAdaptor result)
        {
            var sb = new StringBuilder();
            int totalFailures = CollectFailedTests(result, sb);

            if (totalFailures > 0)
            {
                string header = $"测试失败报告 - {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
                string content = $"{header}失败数量: {totalFailures}\n{sb}";

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
                    File.WriteAllText(ReportPath, content, Encoding.UTF8);

                    // 回到主线程打印日志
                    EditorApplication.delayCall += () =>
                        UnityEngine.Debug.LogWarning($"<color=yellow>失败测试报告已生成：{ReportPath}</color>");
                }
                catch (System.Exception ex)
                {
                    EditorApplication.delayCall += () =>
                        UnityEngine.Debug.LogError($"无法写入测试报告: {ex.Message}");
                }
            }
        }

        private int CollectFailedTests(ITestResultAdaptor node, StringBuilder sb)
        {
            int count = 0;

            // 仅记录叶子测试节点的失败，或者导致整个 Class 崩溃的 Fixture 错误
            if (node.TestStatus == TestStatus.Failed)
            {
                if (!node.HasChildren || IsFixtureError(node))
                {
                    sb.AppendLine(Separator);
                    sb.AppendLine($"测试: {node.Test?.FullName ?? node.Name}");
                    sb.AppendLine($"状态: {node.ResultState}");
                    sb.AppendLine($"信息: {node.Message}");
                    if (!string.IsNullOrEmpty(node.StackTrace))
                        sb.AppendLine($"堆栈:\n{node.StackTrace}");
                    sb.AppendLine();
                    count++;
                }
            }

            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                    count += CollectFailedTests(child, sb);
            }

            return count;
        }

        private bool IsFixtureError(ITestResultAdaptor node)
        {
            // 如果节点有子节点但自身报错，通常是 OneTimeSetUp 或 TearDown 失败
            return node.Message != null &&
                  (node.Message.Contains("SetUp") || node.Message.Contains("TearDown"));
        }
    }
}
#endif