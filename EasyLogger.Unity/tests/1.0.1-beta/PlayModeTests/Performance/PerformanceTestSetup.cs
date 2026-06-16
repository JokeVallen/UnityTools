// 文件: Tests/Performance/PerformanceTestSetup.cs
using NUnit.Framework;

namespace EasyLogger.Unity.PerformanceTests
{
    [SetUpFixture]
    public class PerformanceTestSetup
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // 确保日志系统已初始化
            LogUtility.Info("Performance test started");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            LogUtility.Flush();
        }
    }
}