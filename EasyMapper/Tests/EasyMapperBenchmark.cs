using EasyMapper.Runtime;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyMapper.Tests.Performance
{
    /// <summary>
    /// EasyMapper 基准测试，覆盖核心路径的耗时与 GC 分配。
    /// 所有测试在 EditMode 下运行。
    /// </summary>
    public class EasyMapperBenchmark
    {
        private const int TenThousand = 10_000;
        private const int HundredThousand = 100_000;

        // 预定义测试字符串
        private static readonly string ShortAlphaStr = "player";
        private static readonly string MaxLenStr = "max_len_10";  // 10 字符
        private static readonly string LongStr = "a_very_long_identifier_that_exceeds_ten_chars";

        private Char10PackingBlueprint char10Blueprint;
        private InterningBlueprint interningBlueprint;
        private SmartDistributor smartDistributor;
        private StandardPipeline<string, LongToken> standardPipeline;
        private SmartDistributor testStringDistributor;

        [SetUp]
        public void SetUp()
        {
            char10Blueprint = new Char10PackingBlueprint();
            interningBlueprint = new InterningBlueprint();
            smartDistributor = new SmartDistributor(char10Blueprint, interningBlueprint);
            testStringDistributor = smartDistributor;
            standardPipeline = new StandardPipeline<string, LongToken>(testStringDistributor, testStringDistributor);
        }

        [TearDown]
        public void TearDown()
        {
            // 清理流水线中的字典，保证测试独立
            (standardPipeline as IMaintainable)?.Cleanup();
            interningBlueprint = null;
            char10Blueprint = null;
        }

        #region Char10PackingBlueprint 性能

        [Test, Performance]
        public void Char10Packing_Refine_ShortString_10k_Iterations()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < TenThousand; i++)
                    char10Blueprint.Refine(ShortAlphaStr);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void Char10Packing_Refine_MaxLenString_10k_Iterations()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < TenThousand; i++)
                    char10Blueprint.Refine(MaxLenStr);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void Char10Packing_Restore_10k_Iterations()
        {
            var token = char10Blueprint.Refine(ShortAlphaStr);
            Measure.Method(() =>
            {
                for (int i = 0; i < TenThousand; i++)
                    char10Blueprint.Restore(token);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        #endregion

        #region InterningBlueprint 性能

        [Test, Performance]
        public void Interning_Refine_SameLongString_10k_Iterations()
        {
            // 第一次调用会插入字典，后续命中
            interningBlueprint.Refine(LongStr);
            Measure.Method(() =>
            {
                for (int i = 0; i < TenThousand; i++)
                    interningBlueprint.Refine(LongStr);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void Interning_Refine_UniqueStrings_10k_Iterations()
        {
            var strings = new string[TenThousand];
            for (int i = 0; i < TenThousand; i++)
                strings[i] = $"unique_long_string_{i}";

            Measure.Method(() =>
            {
                for (int i = 0; i < TenThousand; i++)
                    interningBlueprint.Refine(strings[i]);
            })
            .WarmupCount(3)
            .MeasurementCount(5)
            .GC()
            .Run();
        }

        #endregion

        #region SmartDistributor 路径选择性能

        [Test, Performance]
        public void SmartDistributor_FastPath_10k_Iterations()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < TenThousand; i++)
                    smartDistributor.Refine(ShortAlphaStr);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void SmartDistributor_FallbackLongString_10k_Iterations()
        {
            // 首次插入
            smartDistributor.Refine(LongStr);
            Measure.Method(() =>
            {
                for (int i = 0; i < TenThousand; i++)
                    smartDistributor.Refine(LongStr);
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        #endregion

        #region Pipeline 导入性能 (标准流水线)

        [Test, Performance]
        public void StandardPipeline_Import_ExistingString_100k_Iterations()
        {
            standardPipeline.Import("some_key");
            Measure.Method(() =>
            {
                for (int i = 0; i < HundredThousand; i++)
                    standardPipeline.Import("some_key");
            })
            .WarmupCount(3)
            .MeasurementCount(5)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void StandardPipeline_Import_UniqueStrings_100k_Iterations()
        {
            var keys = new string[HundredThousand];
            for (int i = 0; i < HundredThousand; i++)
                keys[i] = $"key_long_enough_{i}";

            Measure.Method(() =>
            {
                for (int i = 0; i < HundredThousand; i++)
                    standardPipeline.Import(keys[i]);
            })
            .WarmupCount(1)
            .MeasurementCount(3)
            .GC()
            .Run();
        }

        #endregion

        #region UnityWeakPipeline 导入性能 (使用临时 GameObject)

        // 注意：每次测试会创建/销毁对象，测量整体耗时
        [Test, Performance]
        public void UnityWeakPipeline_Import_DestroyedObjects_NoLeak()
        {
            var blueprint = new ObjectNamingBlueprint(testStringDistributor);
            var pipeline = new UnityWeakPipeline<Object, LongToken>(blueprint, blueprint);

            Measure.Method(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    var go = new GameObject($"obj_{i}");
                    pipeline.Import(go);
                    Object.DestroyImmediate(go);
                }
                pipeline.Cleanup();
            })
            .WarmupCount(2)
            .MeasurementCount(5)
            .GC()
            .Run();
        }

        #endregion

        #region 装饰器开销对比

        [Test, Performance]
        public void CappedPipeline_Overhead_FullCache_10k()
        {
            var inner = new StandardPipeline<string, LongToken>(testStringDistributor, testStringDistributor);
            var capped = new CappedPipeline<string, LongToken>(
                testStringDistributor,  // IBlueprint<string, LongToken>
                testStringDistributor,  // 同时是 IFeature
                5000);
            // 预填满缓存
            for (int i = 0; i < 5000; i++)
                capped.Import($"item_{i}");

            Measure.Method(() =>
            {
                for (int i = 0; i < TenThousand; i++)
                    capped.Import($"item_{i % 5000}");
            })
            .WarmupCount(3)
            .MeasurementCount(5)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void GuardedPipeline_NullCheck_Overhead_10k()
        {
            var inner = new StandardPipeline<string, LongToken>(testStringDistributor, testStringDistributor);
            var guarded = new GuardedPipeline<string, LongToken>(inner);

            Measure.Method(() =>
            {
                for (int i = 0; i < TenThousand; i++)
                    guarded.Import("simple");
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        #endregion

        #region 序列化性能

        [Test, Performance]
        public void BinaryPackage_WrapUnwrap_10k()
        {
            var package = new BinaryIdentityPackage();
            var token = new LongToken(42);
            Measure.Method(() =>
            {
                for (int i = 0; i < TenThousand; i++)
                {
                    byte[] bytes = package.Wrap(token);
                    LongToken restored = package.Unwrap(bytes);
                }
            })
            .WarmupCount(5)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        #endregion
    }
}