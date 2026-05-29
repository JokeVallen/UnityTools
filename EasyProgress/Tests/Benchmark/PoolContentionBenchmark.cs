using BenchmarkDotNet.Attributes;
using EasyProgress.Core;

namespace EasyProgress.Benchmark
{
    [SimpleJob(iterationCount: 3, warmupCount: 1)]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class PoolContentionBenchmark
    {
        [Params(1, 4, 16)]
        public int ConcurrencyLevel { get; set; }

        [Benchmark]
        public void AcquireReleaseList()
        {
            var tasks = new Task[ConcurrencyLevel];
            for (int i = 0; i < ConcurrencyLevel; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    var list = ListPool.Rent<int>();
                    list.Add(1);
                    ListPool.Return(list);
                });
            }
            Task.WaitAll(tasks);
        }

        [Benchmark]
        public void AcquireReleaseDict()
        {
            var tasks = new Task[ConcurrencyLevel];
            for (int i = 0; i < ConcurrencyLevel; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    var dict = DictionaryPool.Rent<int,int>();
                    dict[1] = 2;
                    DictionaryPool.Return(dict);
                });
            }
            Task.WaitAll(tasks);
        }
    }
}
