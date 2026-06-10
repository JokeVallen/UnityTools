// Tests/PlayMode/ViewPipelinePerformanceTests.cs
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Unity.PerformanceTesting;
using ViewPipeline.Unity;
using ViewPipeline.Unity.Core;

namespace ViewPipeline.Tests.PlayMode
{
    #region 测试辅助类型

    public class PerformanceTestView : IView
    {
        public UniTask ShowAsync(CancellationToken cancellationToken) => UniTask.CompletedTask;
        public UniTask HideAsync(CancellationToken cancellationToken) => UniTask.CompletedTask;
    }

    public class EmptyMiddleware : IViewMiddleware
    {
        public async UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token)
        {
            await executor.NextAsync(view, token);
        }
    }

    public class EmptyCloseMiddleware : IViewMiddleware
    {
        public async UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token)
        {
            await executor.NextAsync(view, token);
        }
    }

    // 真实场景中间件（无延迟，仅测试框架开销）
    internal class AuthMiddleware : IViewMiddleware
    {
        public async UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token)
        {
            await executor.NextAsync(view, token);
        }
    }

    internal class CacheMiddleware : IViewMiddleware
    {
        public async UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token)
        {
            await executor.NextAsync(view, token);
        }
    }

    internal class LoadingMiddleware : IViewMiddleware
    {
        public async UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token)
        {
            await executor.NextAsync(view, token);
        }
    }

    internal class AnalyticsMiddleware : IViewMiddleware
    {
        public async UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token)
        {
            await executor.NextAsync(view, token);
        }
    }

    internal class AnimationMiddleware : IViewMiddleware
    {
        public async UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token)
        {
            await executor.NextAsync(view, token);
        }
    }

    internal class TypedContextReadWriteMiddleware : IViewMiddleware
    {
        public async UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token)
        {
            if (executor.Context is ITypedPipelineContext typed)
            {
                typed.Set("perf_test_key", 12345);
                var value = typed.Get<string, int>("perf_test_key");
            }
            await executor.NextAsync(view, token);
        }
    }

    #endregion

    [TestFixture]
    public class ViewPipelinePerformanceTests
    {
        #region 1. 构建会话性能测试

        [Test, Performance]
        public void BuildSession_NoMiddleware()
        {
            Measure.Method(() =>
            {
                var session = ViewSessionBuilder.Create().Build();
                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .WarmupCount(3)
            .MeasurementCount(5)
            .Run();
        }

        [Test, Performance]
        public void BuildSession_WithMiddlewares([Values(1, 5, 10, 20)] int middlewareCount)
        {
            Measure.Method(() =>
            {
                var builder = ViewSessionBuilder.Create();
                for (int i = 0; i < middlewareCount; i++)
                {
                    builder.AddOpenMiddleware(new EmptyMiddleware());
                }
                var session = builder.Build();
                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .WarmupCount(3)
            .MeasurementCount(5)
            .Run();
        }

        [Test, Performance]
        public void BuildSession_WithTypedContext()
        {
            Measure.Method(() =>
            {
                var session = ViewSessionBuilder.Create()
                    .WithTypedContext()
                    .Build();
                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .WarmupCount(3)
            .MeasurementCount(5)
            .Run();
        }

        #endregion

        #region 2. 打开/关闭视图性能测试

        [Test, Performance]
        public void OpenCloseView_NoMiddleware()
        {
            Measure.Method(() =>
            {
                var session = ViewSessionBuilder.Create().Build();
                var view = new PerformanceTestView();
                session.OpenViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.CloseViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void OpenCloseView_WithMiddlewares([Values(0, 1, 5, 10, 20)] int middlewareCount)
        {
            Measure.Method(() =>
            {
                var builder = ViewSessionBuilder.Create();
                for (int i = 0; i < middlewareCount; i++)
                {
                    builder.AddOpenMiddleware(new EmptyMiddleware());
                }
                var session = builder.Build();
                var view = new PerformanceTestView();
                session.OpenViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.CloseViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void OpenCloseView_WithCloseMiddlewares([Values(0, 1, 5, 10, 20)] int middlewareCount)
        {
            Measure.Method(() =>
            {
                var builder = ViewSessionBuilder.Create();
                for (int i = 0; i < middlewareCount; i++)
                {
                    builder.AddCloseMiddleware(new EmptyCloseMiddleware());
                }
                var session = builder.Build();
                var view = new PerformanceTestView();
                session.OpenViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.CloseViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void OpenCloseView_WithTypedContext()
        {
            Measure.Method(() =>
            {
                var session = ViewSessionBuilder.Create()
                    .WithTypedContext()
                    .Build();
                var view = new PerformanceTestView();
                session.OpenViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.CloseViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        #endregion

        #region 3. GC 分配测试

        [Test, Performance]
        public void GCAllocation_NoMiddleware()
        {
            Measure.Method(() =>
            {
                var session = ViewSessionBuilder.Create().Build();
                var view = new PerformanceTestView();
                session.OpenViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.CloseViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .WarmupCount(10)
            .MeasurementCount(20)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void GCAllocation_WithPoolWarmup()
        {
            Measure.Method(() =>
            {
                var session = ViewSessionBuilder.Create().Build();
                var view = new PerformanceTestView();
                session.OpenViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.CloseViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .WarmupCount(40)
            .MeasurementCount(20)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void GCAllocation_WithTypedContext()
        {
            Measure.Method(() =>
            {
                var session = ViewSessionBuilder.Create()
                    .WithTypedContext()
                    .Build();
                var view = new PerformanceTestView();
                session.OpenViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.CloseViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .WarmupCount(10)
            .MeasurementCount(20)
            .GC()
            .Run();
        }

        #endregion

        #region 4. 压力测试（顺序）

        [Test, Performance]
        public void StressTest_SequentialOpenClose([Values(10, 100, 500)] int operationCount)
        {
            var sampleGroup = new SampleGroup($"Sequential_{operationCount}", SampleUnit.Millisecond);

            Measure.Method(() =>
            {
                var session = ViewSessionBuilder.Create().Build();
                var view = new PerformanceTestView();
                for (int i = 0; i < operationCount; i++)
                {
                    session.OpenViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                    session.CloseViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                }
                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .SampleGroup(sampleGroup)
            .WarmupCount(1)
            .MeasurementCount(3)
            .Run();
        }

        #endregion

        #region 5. 压力测试（并行）

        [Test, Performance]
        public void StressTest_ParallelOpenClose([Values(10, 50)] int taskCount)
        {
            var sampleGroup = new SampleGroup($"Parallel_{taskCount}", SampleUnit.Millisecond);

            Measure.Method(() =>
            {
                var session = ViewSessionBuilder.Create().Build();
                var views = new List<PerformanceTestView>();
                for (int i = 0; i < taskCount; i++)
                {
                    views.Add(new PerformanceTestView());
                }

                var openTasks = new List<UniTask>();
                foreach (var view in views)
                {
                    openTasks.Add(session.OpenViewAsync(view, CancellationToken.None));
                }
                UniTask.WhenAll(openTasks).GetAwaiter().GetResult();

                var closeTasks = new List<UniTask>();
                foreach (var view in views)
                {
                    closeTasks.Add(session.CloseViewAsync(view, CancellationToken.None));
                }
                UniTask.WhenAll(closeTasks).GetAwaiter().GetResult();

                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .SampleGroup(sampleGroup)
            .WarmupCount(1)
            .MeasurementCount(3)
            .Run();
        }

        #endregion

        #region 6. ITypedPipelineContext 性能测试（新增）

        [Test, Performance]
        public void TypedContext_ReadWrite_Overhead()
        {
            var middleware = new TypedContextReadWriteMiddleware();

            Measure.Method(() =>
            {
                var session = ViewSessionBuilder.Create()
                    .WithTypedContext()
                    .AddOpenMiddleware(middleware)
                    .Build();
                var view = new PerformanceTestView();
                session.OpenViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.CloseViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void TypedContext_ReadWrite_1000Operations()
        {
            var sampleGroup = new SampleGroup("TypedContext_1000Ops", SampleUnit.Millisecond);
            var middleware = new TypedContextReadWriteMiddleware();

            Measure.Method(() =>
            {
                var session = ViewSessionBuilder.Create()
                    .WithTypedContext()
                    .AddOpenMiddleware(middleware)
                    .Build();
                var view = new PerformanceTestView();
                for (int i = 0; i < 1000; i++)
                {
                    session.OpenViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                    session.CloseViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                }
                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .SampleGroup(sampleGroup)
            .WarmupCount(1)
            .MeasurementCount(3)
            .Run();
        }

        #endregion

        #region 7. 真实场景模拟测试

        [Test, Performance]
        public void RealWorldScenario_ECommercePage()
        {
            Measure.Method(() =>
            {
                var builder = ViewSessionBuilder.Create()
                    .AddOpenMiddleware(new AuthMiddleware())
                    .AddOpenMiddleware(new CacheMiddleware())
                    .AddOpenMiddleware(new LoadingMiddleware())
                    .AddOpenMiddleware(new AnalyticsMiddleware())
                    .AddOpenMiddleware(new AnimationMiddleware());

                var session = builder.Build();
                var view = new PerformanceTestView();
                session.OpenViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.CloseViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        [Test, Performance]
        public void RealWorldScenario_ECommercePage_WithCloseMiddlewares()
        {
            Measure.Method(() =>
            {
                var builder = ViewSessionBuilder.Create()
                    .AddOpenMiddleware(new AuthMiddleware())
                    .AddOpenMiddleware(new CacheMiddleware())
                    .AddOpenMiddleware(new LoadingMiddleware())
                    .AddOpenMiddleware(new AnalyticsMiddleware())
                    .AddOpenMiddleware(new AnimationMiddleware())
                    .AddCloseMiddleware(new CacheMiddleware())
                    .AddCloseMiddleware(new AnalyticsMiddleware());

                var session = builder.Build();
                var view = new PerformanceTestView();
                session.OpenViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.CloseViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .GC()
            .Run();
        }

        #endregion

        #region 8. 中间件开销对比测试

        [Test, Performance]
        public void CompareMiddlewareOverhead()
        {
            Measure.Method(() =>
            {
                var session = ViewSessionBuilder.Create().Build();
                var view = new PerformanceTestView();
                session.OpenViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.CloseViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .Run();

            Measure.Method(() =>
            {
                var builder = ViewSessionBuilder.Create();
                for (int i = 0; i < 5; i++)
                {
                    builder.AddOpenMiddleware(new EmptyMiddleware());
                }
                var session = builder.Build();
                var view = new PerformanceTestView();
                session.OpenViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.CloseViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .Run();

            Measure.Method(() =>
            {
                var builder = ViewSessionBuilder.Create();
                for (int i = 0; i < 10; i++)
                {
                    builder.AddOpenMiddleware(new EmptyMiddleware());
                }
                var session = builder.Build();
                var view = new PerformanceTestView();
                session.OpenViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.CloseViewAsync(view, CancellationToken.None).GetAwaiter().GetResult();
                session.DisposeAsync().GetAwaiter().GetResult();
            })
            .WarmupCount(3)
            .MeasurementCount(10)
            .Run();
        }

        #endregion
    }
}