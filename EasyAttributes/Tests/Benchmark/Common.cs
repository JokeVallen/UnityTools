using EasyAttributes.Core;

namespace EasyAttributes.Benchmark
{
    // ── 测试服务 ─────────────────────────────────────────────────────────────────
    public class TestService
    {
        public string Name { get; set; } = "test";

        public int DoWork(int a, string b) => a + b.Length;

        public async Task<int> DoWorkAsync(int a, string b)
        {
            await Task.Yield();
            return a + b.Length;
        }
    }

    // ── 测试 Attribute ──────────────────────────────────────────────────────────
    public class TestLogAttribute : EasyAttribute
    {
        public string Level { get; set; } = "Info";
    }

    // ── 测试处理器（同步） ─────────────────────────────────────────────────────
    public class LogProcessor : Processor<TestLogAttribute>
    {
        public override IProcessorHandle Process(IContext context, TestLogAttribute attribute)
            => ProcessorHandle.Continue;
    }

    public class AnotherLogProcessor : Processor<TestLogAttribute>
    {
        public override IProcessorHandle Process(IContext context, TestLogAttribute attribute)
            => ProcessorHandle.Continue;
    }

    public class SyncLogProcessor : Processor<TestLogAttribute>
    {
        public override IProcessorHandle Process(IContext context, TestLogAttribute attribute)
            => ProcessorHandle.Continue;
    }

    // ── 测试处理器（异步） ─────────────────────────────────────────────────────
    public class AsyncLogProcessor : AsyncProcessor<TestLogAttribute>
    {
        public override async Task<IProcessorHandle> ProcessAsync(IContext context, TestLogAttribute attribute)
        {
            await Task.CompletedTask;
            return ProcessorHandle.Continue;
        }
    }

    // ── 虚拟处理器（用于扩展链长度） ───────────────────────────────────────────
    public class DummyProcessor1 : Processor<TestLogAttribute>
    {
        public override IProcessorHandle Process(IContext context, TestLogAttribute attribute)
            => ProcessorHandle.Continue;
    }
    public class DummyProcessor2 : Processor<TestLogAttribute>
    {
        public override IProcessorHandle Process(IContext context, TestLogAttribute attribute)
            => ProcessorHandle.Continue;
    }
    public class DummyProcessor3 : Processor<TestLogAttribute>
    {
        public override IProcessorHandle Process(IContext context, TestLogAttribute attribute)
            => ProcessorHandle.Continue;
    }
    public class DummyProcessor4 : Processor<TestLogAttribute>
    {
        public override IProcessorHandle Process(IContext context, TestLogAttribute attribute)
            => ProcessorHandle.Continue;
    }
    public class DummyProcessor5 : Processor<TestLogAttribute>
    {
        public override IProcessorHandle Process(IContext context, TestLogAttribute attribute)
            => ProcessorHandle.Continue;
    }
    public class DummyProcessor6 : Processor<TestLogAttribute>
    {
        public override IProcessorHandle Process(IContext context, TestLogAttribute attribute)
            => ProcessorHandle.Continue;
    }
    public class DummyProcessor7 : Processor<TestLogAttribute>
    {
        public override IProcessorHandle Process(IContext context, TestLogAttribute attribute)
            => ProcessorHandle.Continue;
    }
    public class DummyProcessor8 : Processor<TestLogAttribute>
    {
        public override IProcessorHandle Process(IContext context, TestLogAttribute attribute)
            => ProcessorHandle.Continue;
    }
    public class DummyProcessor9 : Processor<TestLogAttribute>
    {
        public override IProcessorHandle Process(IContext context, TestLogAttribute attribute)
            => ProcessorHandle.Continue;
    }
    public class DummyProcessor10 : Processor<TestLogAttribute>
    {
        public override IProcessorHandle Process(IContext context, TestLogAttribute attribute)
            => ProcessorHandle.Continue;
    }

    // ── Feature 相关 ───────────────────────────────────────────────────────────
    public interface IFakeLogger : IFeature
    {
        void Log(string msg);
    }

    public class FakeLogger : IFakeLogger
    {
        public void Log(string msg) { }
    }
}
