// 文件: Tests/Performance/TestConfig.cs
namespace EasyLogger.Unity.PerformanceTests
{
    public static class TestConfig
    {
        // 测试数量级
        public static readonly int[] BatchSizes = { 10, 100, 1000, 5000 };

        // 每个测试的迭代次数
        public const int Iterations = 100;

        // 预热次数
        public const int WarmupCount = 3;

        // 测量次数
        public const int MeasureCount = 10;

        // 测试用消息
        public static readonly string TestMessage = "这是一条测试日志消息，用于性能基准测试 - 索引: {0}";
    }
}