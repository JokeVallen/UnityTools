// Tests/EditMode/ViewPipelineEditModeTests.cs
using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using Cysharp.Threading.Tasks;
using ViewPipeline.Unity;
using ViewPipeline.Unity.Core;
using System.Threading;

namespace ViewPipeline.Tests.EditMode
{
    // 模拟视图
    public class MockView : IView
    {
        public bool IsShown { get; private set; }
        public bool IsHidden { get; private set; }
        public int ShowCallCount { get; private set; }
        public int HideCallCount { get; private set; }

        public UniTask ShowAsync(CancellationToken cancellationToken)
        {
            IsShown = true;
            ShowCallCount++;
            return UniTask.CompletedTask;
        }

        public UniTask HideAsync(CancellationToken cancellationToken)
        {
            IsHidden = true;
            HideCallCount++;
            return UniTask.CompletedTask;
        }

        public void Reset()
        {
            IsShown = false;
            IsHidden = false;
            ShowCallCount = 0;
            HideCallCount = 0;
        }
    }

    // 测试中间件
    public class TestMiddleware : IViewMiddleware
    {
        public bool BeforeNextCalled { get; private set; }
        public bool AfterNextCalled { get; private set; }
        public bool Intercepted { get; private set; }

        public async UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token)
        {
            BeforeNextCalled = true;

            if (Intercepted)
            {
                executor.Abort();
            }
            else
            {
                await executor.NextAsync(view, token);
            }

            AfterNextCalled = true;
        }

        public void SetIntercept(bool intercept) => Intercepted = intercept;
        public void Reset()
        {
            BeforeNextCalled = false;
            AfterNextCalled = false;
            Intercepted = false;
        }
    }

    // 顺序测试中间件
    internal class OrderTestMiddleware : IViewMiddleware
    {
        private readonly string _name;
        private readonly List<string> _order;

        public OrderTestMiddleware(string name, List<string> order)
        {
            _name = name;
            _order = order;
        }

        public async UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token)
        {
            _order.Add($"{_name}-Before");
            await executor.NextAsync(view, token);
            _order.Add($"{_name}-After");
        }
    }

    // 动态中间件
    public class TestDynamicMiddleware : IViewMiddleware
    {
        public bool Executed { get; private set; }

        public async UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token)
        {
            Executed = true;
            await executor.NextAsync(view, token);
        }

        public void Reset() => Executed = false;
    }

    // 动态供应器
    public class TestDynamicProvider : IDynamicMiddlewareProvider
    {
        private readonly Func<IView, bool> _condition;
        private readonly IViewMiddleware _middleware;

        public TestDynamicProvider(Func<IView, bool> condition, IViewMiddleware middleware)
        {
            _condition = condition;
            _middleware = middleware;
        }

        public void PopulateMiddlewares(IView view, IDynamicMiddlewareCollection dynamicMiddlewares)
        {
            if (_condition(view))
            {
                dynamicMiddlewares.Add(_middleware);
            }
        }
    }

    // 测试执行策略
    public class TestExecutionPolicy : IExecutionPolicy
    {
        private readonly Func<IView, IViewMiddleware, bool> _shouldSkipMiddleware;
        private readonly Func<IViewMiddleware, IView, bool> _shouldSkipView;
        private readonly Func<IView, bool> _shouldTerminateView;
        private readonly Func<IViewMiddleware, bool> _shouldTerminateMiddleware;

        public TestExecutionPolicy(
            Func<IView, IViewMiddleware, bool> shouldSkipMiddleware = null,
            Func<IViewMiddleware, IView, bool> shouldSkipView = null,
            Func<IView, bool> shouldTerminateView = null,
            Func<IViewMiddleware, bool> shouldTerminateMiddleware = null)
        {
            _shouldSkipMiddleware = shouldSkipMiddleware ?? ((v, m) => false);
            _shouldSkipView = shouldSkipView ?? ((m, v) => false);
            _shouldTerminateView = shouldTerminateView ?? (v => false);
            _shouldTerminateMiddleware = shouldTerminateMiddleware ?? (m => false);
        }

        public bool ShouldSkipMiddleware(IView view, IViewMiddleware middleware)
            => _shouldSkipMiddleware(view, middleware);

        public bool ShouldSkipView(IViewMiddleware middleware, IView view)
            => _shouldSkipView(middleware, view);

        public bool ShouldTerminate(IView view)
            => _shouldTerminateView(view);

        public bool ShouldTerminate(IViewMiddleware middleware)
            => _shouldTerminateMiddleware(middleware);
    }

    // 测试扩展包
    internal class TestExtension : IExtension, IValidatable
    {
        private readonly IEnumerable<IViewMiddleware> _openMiddlewares;
        private readonly IEnumerable<IViewMiddleware> _closeMiddlewares;

        public bool IsInitialized { get; private set; }

        public TestExtension(
            IEnumerable<IViewMiddleware> openMiddlewares = null,
            IEnumerable<IViewMiddleware> closeMiddlewares = null)
        {
            _openMiddlewares = openMiddlewares ?? Array.Empty<IViewMiddleware>();
            _closeMiddlewares = closeMiddlewares ?? Array.Empty<IViewMiddleware>();
        }

        public IEnumerable<IViewMiddleware> GetMiddlewares(PipelineDirection direction)
        {
            return direction == PipelineDirection.Open ? _openMiddlewares : _closeMiddlewares;
        }

        public IEnumerable<IDynamicMiddlewareProvider> GetDynamicProviders(PipelineDirection direction)
        {
            return Array.Empty<IDynamicMiddlewareProvider>();
        }

        public void Initialize() { IsInitialized = true; }

        public IValidator GetValidator()
        {
            return new TestValidator();
        }

        private class TestValidator : IValidator
        {
            public ValidationResult Validate()
            {
                return ValidationResult.Success();
            }
        }
    }

    // 需要 ITypedPipelineContext 的扩展包（用于测试验证器）
    internal class RequireTypedContextExtension : IExtension, IValidatable, IFullSnapshotable<ExtensionSnapshot>
    {
        private readonly Guid _builderKey;

        public RequireTypedContextExtension(Guid builderKey)
        {
            _builderKey = builderKey;
        }

        public bool IsInitialized { get; private set; }
        public void Initialize() => IsInitialized = true;

        public IEnumerable<IViewMiddleware> GetMiddlewares(PipelineDirection direction)
            => Array.Empty<IViewMiddleware>();

        public IEnumerable<IDynamicMiddlewareProvider> GetDynamicProviders(PipelineDirection direction)
            => Array.Empty<IDynamicMiddlewareProvider>();

        public IValidator GetValidator()
        {
            return new RequireTypedContextValidator(_builderKey);
        }

        public ExtensionSnapshot GetFullSnapshot()
        {
            return new ExtensionSnapshot(
                typeof(RequireTypedContextExtension),
                IsInitialized
            );
        }

        private class RequireTypedContextValidator : IValidator
        {
            private readonly Guid _builderKey;

            public RequireTypedContextValidator(Guid builderKey)
            {
                _builderKey = builderKey;
            }

            public ValidationResult Validate()
            {
                if (!SnapshotCache.TryRefreshAndGet<ViewSessionBuilderSnapshot>(_builderKey, out var snapshot))
                {
                    return ValidationResult.Error("Cannot get builder snapshot");
                }

                if (!typeof(ITypedPipelineContext).IsAssignableFrom(snapshot.ContextType))
                {
                    return ValidationResult.Error(
                        "This extension requires ITypedPipelineContext. " +
                        "Please use WithTypedContext() when building the session."
                    );
                }

                return ValidationResult.Success();
            }
        }
    }

    // ITypedPipelineContext 测试用的中间件
    public class ContextWriteTestMiddleware : IViewMiddleware
    {
        public bool WriteSuccess { get; private set; }

        public async UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token)
        {
            if (executor.Context is ITypedPipelineContext typed)
            {
                typed.Set("test_int", 42);
                typed.Set("test_string", "hello");
                WriteSuccess = true;
            }

            await executor.NextAsync(view, token);
        }
    }

    public class ContextReadTestMiddleware : IViewMiddleware
    {
        public bool ReadSuccess { get; private set; }
        public int IntValue { get; private set; }
        public string StringValue { get; private set; }

        public async UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token)
        {
            if (executor.Context is ITypedPipelineContext typed)
            {
                var intVal = typed.Get<string, int>("test_int");
                var strVal = typed.Get<string, string>("test_string");

                ReadSuccess = intVal.HasValue && strVal.HasValue;
                if (ReadSuccess)
                {
                    IntValue = intVal.Value;
                    StringValue = strVal.Value;
                }
            }

            await executor.NextAsync(view, token);
        }
    }

    [TestFixture]
    public class ViewPipelineEditModeTests
    {
        private MockView _mockView;

        [SetUp]
        public void SetUp()
        {
            _mockView = new MockView();
            // 清理快照缓存，避免测试间干扰
            SnapshotCache.Clear();
            SnapshotCache<PipelineDirection>.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            _mockView.Reset();
        }

        #region 基础功能测试

        [Test]
        public void BuildSession_ShouldNotThrow()
        {
            var session = ViewSessionBuilder.Create().Build();
            session.DisposeAsync().Forget();
            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator OpenViewAsync_WithoutMiddleware_ShouldShowView()
        {
            var session = ViewSessionBuilder.Create().Build();
            try
            {
                yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();
                Assert.IsTrue(_mockView.IsShown);
                Assert.AreEqual(1, _mockView.ShowCallCount);
            }
            finally
            {
                session.DisposeAsync().Forget();
            }
        }

        [UnityTest]
        public IEnumerator CloseViewAsync_WithoutMiddleware_ShouldHideView()
        {
            var session = ViewSessionBuilder.Create().Build();
            try
            {
                yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();
                yield return session.CloseViewAsync(_mockView, CancellationToken.None).AsCoroutine();

                Assert.IsTrue(_mockView.IsHidden);
                Assert.AreEqual(1, _mockView.HideCallCount);
            }
            finally
            {
                session.DisposeAsync().Forget();
            }
        }

        [UnityTest]
        public IEnumerator OpenViewAsync_WithStaticMiddleware_ShouldExecuteMiddleware()
        {
            var middleware = new TestMiddleware();
            var session = ViewSessionBuilder.Create()
                .AddOpenMiddleware(middleware)
                .Build();
            try
            {
                yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();

                Assert.IsTrue(middleware.BeforeNextCalled);
                Assert.IsTrue(middleware.AfterNextCalled);
                Assert.IsTrue(_mockView.IsShown);
            }
            finally
            {
                session.DisposeAsync().Forget();
            }
        }

        [UnityTest]
        public IEnumerator OpenViewAsync_WithMultipleMiddlewares_ShouldExecuteInOrder()
        {
            var executionOrder = new List<string>();
            var middleware1 = new OrderTestMiddleware("A", executionOrder);
            var middleware2 = new OrderTestMiddleware("B", executionOrder);

            var session = ViewSessionBuilder.Create()
                .AddOpenMiddleware(middleware1)
                .AddOpenMiddleware(middleware2)
                .Build();
            try
            {
                yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();

                Assert.AreEqual(4, executionOrder.Count);
                Assert.AreEqual("A-Before", executionOrder[0]);
                Assert.AreEqual("B-Before", executionOrder[1]);
                Assert.AreEqual("B-After", executionOrder[2]);
                Assert.AreEqual("A-After", executionOrder[3]);
            }
            finally
            {
                session.DisposeAsync().Forget();
            }
        }

        #endregion

        #region 拦截测试

        [UnityTest]
        public IEnumerator OpenViewAsync_WithInterceptedMiddleware_ShouldNotShowView()
        {
            var middleware = new TestMiddleware();
            middleware.SetIntercept(true);

            var session = ViewSessionBuilder.Create()
                .AddOpenMiddleware(middleware)
                .Build();
            try
            {
                yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();

                Assert.IsTrue(middleware.BeforeNextCalled);
                Assert.IsTrue(middleware.AfterNextCalled);
                Assert.IsFalse(_mockView.IsShown);
            }
            finally
            {
                session.DisposeAsync().Forget();
            }
        }

        [UnityTest]
        public IEnumerator OpenViewAsync_WithInterceptedMiddleware_ShouldAllowReopenAfterRollback()
        {
            var middleware = new TestMiddleware();
            middleware.SetIntercept(true);

            var session = ViewSessionBuilder.Create()
                .AddOpenMiddleware(middleware)
                .Build();
            try
            {
                yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();
                Assert.IsFalse(_mockView.IsShown);

                middleware.SetIntercept(false);
                yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();

                Assert.IsTrue(_mockView.IsShown);
            }
            finally
            {
                session.DisposeAsync().Forget();
            }
        }

        #endregion

        #region 动态中间件测试

        [UnityTest]
        public IEnumerator OpenViewAsync_WithDynamicProvider_ConditionTrue_ShouldAddMiddleware()
        {
            var dynamicMiddleware = new TestDynamicMiddleware();
            var provider = new TestDynamicProvider(view => true, dynamicMiddleware);

            var session = ViewSessionBuilder.Create()
                .AddOpenDynamicProvider(provider)
                .Build();
            try
            {
                yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();

                Assert.IsTrue(dynamicMiddleware.Executed);
            }
            finally
            {
                session.DisposeAsync().Forget();
            }
        }

        [UnityTest]
        public IEnumerator OpenViewAsync_WithDynamicProvider_ConditionFalse_ShouldNotAddMiddleware()
        {
            var dynamicMiddleware = new TestDynamicMiddleware();
            var provider = new TestDynamicProvider(view => false, dynamicMiddleware);

            var session = ViewSessionBuilder.Create()
                .AddOpenDynamicProvider(provider)
                .Build();
            try
            {
                yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();

                Assert.IsFalse(dynamicMiddleware.Executed);
            }
            finally
            {
                session.DisposeAsync().Forget();
            }
        }

        #endregion

        #region 执行策略测试

        [UnityTest]
        public IEnumerator OpenViewAsync_WithExecutionPolicy_ShouldSkipSpecifiedMiddleware()
        {
            var middleware = new TestMiddleware();
            var policy = new TestExecutionPolicy((view, m) => m == middleware);

            var session = ViewSessionBuilder.Create()
                .AddOpenMiddleware(middleware)
                .WithMiddlewareExecutionPolicy(policy)
                .Build();
            try
            {
                yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();

                Assert.IsFalse(middleware.BeforeNextCalled);
                Assert.IsFalse(middleware.AfterNextCalled);
                Assert.IsTrue(_mockView.IsShown);
            }
            finally
            {
                session.DisposeAsync().Forget();
            }
        }

        #endregion

        #region 扩展包测试

        [UnityTest]
        public IEnumerator AddExtension_ShouldAddAllMiddlewares()
        {
            var middleware1 = new TestMiddleware();
            var middleware2 = new TestMiddleware();

            var extension = new TestExtension(
                openMiddlewares: new[] { middleware1, middleware2 }
            );

            var session = ViewSessionBuilder.Create()
                .AddExtension(extension)
                .Build();
            try
            {
                yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();

                Assert.IsTrue(middleware1.BeforeNextCalled);
                Assert.IsTrue(middleware2.BeforeNextCalled);
            }
            finally
            {
                session.DisposeAsync().Forget();
            }
        }

        #endregion

        #region ITypedPipelineContext 测试

        [UnityTest]
        public IEnumerator WithTypedContext_MultipleMiddlewares_ShouldShareData()
        {
            var writeMiddleware = new ContextWriteTestMiddleware();
            var readMiddleware = new ContextReadTestMiddleware();

            var session = ViewSessionBuilder.Create()
                .WithTypedContext()
                .AddOpenMiddleware(writeMiddleware)
                .AddOpenMiddleware(readMiddleware)
                .Build();
            try
            {
                yield return session.OpenViewAsync(new MockView(), CancellationToken.None).AsCoroutine();

                Assert.IsTrue(writeMiddleware.WriteSuccess);
                Assert.IsTrue(readMiddleware.ReadSuccess);
                Assert.AreEqual(42, readMiddleware.IntValue);
                Assert.AreEqual("hello", readMiddleware.StringValue);
            }
            finally
            {
                session.DisposeAsync().Forget();
            }
        }

        [UnityTest]
        public IEnumerator WithoutTypedContext_ContextShouldNotSupportTypedOperations()
        {
            var middleware = new ContextWriteTestMiddleware();
            var session = ViewSessionBuilder.Create()
                .AddOpenMiddleware(middleware)
                .Build();
            try
            {
                yield return session.OpenViewAsync(new MockView(), CancellationToken.None).AsCoroutine();

                Assert.IsFalse(middleware.WriteSuccess);
            }
            finally
            {
                session.DisposeAsync().Forget();
            }
        }

        #endregion

        #region 扩展包验证器测试（修复：在 Build 前获取快照）

        [Test]
        public void ExtensionWithTypedContextRequirement_WithoutWithTypedContext_ExtensionNotAdded()
        {
            var builder = ViewSessionBuilder.Create();
            var extension = new RequireTypedContextExtension(builder.Key);

            SnapshotCache.TryGet<ViewSessionBuilderSnapshot>(builder.Key, out var snapshotBeforeAdd);
            Assert.IsNull(snapshotBeforeAdd.Extensions);

            try
            {
                builder.AddExtension(extension);
            }
            catch { }

            LogAssert.Expect(UnityEngine.LogType.Error, "[ViewPipeline] The component 'ViewPipeline.Tests.EditMode.RequireTypedContextExtension' failed the precondition validation: (Error) This extension requires ITypedPipelineContext. Please use WithTypedContext() when building the session.");
        }

        [Test]
        public void ExtensionWithTypedContextRequirement_WithWithTypedContext_ExtensionAdded()
        {
            var builder = ViewSessionBuilder.Create();
            var extension = new RequireTypedContextExtension(builder.Key);

            builder.WithTypedContext();

            SnapshotCache.TryGet<ViewSessionBuilderSnapshot>(builder.Key, out var snapshotBeforeAdd);
            Assert.IsNull(snapshotBeforeAdd.Extensions);

            builder.AddExtension(extension);

            var snapshotAfterAdd = SnapshotCache.RefreshAndGet<ViewSessionBuilderSnapshot>(builder.Key);
            Assert.IsNotEmpty(snapshotAfterAdd.Extensions);
            Assert.AreEqual(typeof(RequireTypedContextExtension), snapshotAfterAdd.Extensions[0].ExtensionType);

            var session = builder.Build();
            session.DisposeAsync().Forget();
        }

        #endregion

        #region 异常测试

        [UnityTest]
        public IEnumerator OpenViewAsync_WithNullView_ShouldThrow()
        {
            var session = ViewSessionBuilder.Create().Build();
            var exceptionCaught = false;

            yield return UniTaskTestHelper.AsCoroutineWithExceptionCheck(
                ct => session.OpenViewAsync(null, ct),
                ex =>
                {
                    if (ex is ArgumentNullException)
                    {
                        exceptionCaught = true;
                    }
                },
                CancellationToken.None
            );

            Assert.IsTrue(exceptionCaught, "Expected ArgumentNullException but none was thrown");
            session.DisposeAsync().Forget();
        }

        [Test]
        public void BuildSession_ReuseBuilder_ShouldThrow()
        {
            var builder = ViewSessionBuilder.Create();
            builder.Build();

            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        #endregion

        #region 资源释放测试

        [UnityTest]
        public IEnumerator DisposeAsync_ShouldCompleteWithoutError()
        {
            var session = ViewSessionBuilder.Create().Build();
            yield return session.DisposeAsync().AsCoroutine();
            yield return session.DisposeAsync().AsCoroutine(); // 第二次调用应无异常
            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator OpenViewAsync_AfterDispose_ShouldThrow()
        {
            var session = ViewSessionBuilder.Create().Build();
            session.DisposeAsync().Forget();

            var exceptionCaught = false;

            yield return UniTaskTestHelper.AsCoroutineWithExceptionCheck(
                ct => session.OpenViewAsync(_mockView, ct),
                ex =>
                {
                    if (ex is InvalidOperationException)
                    {
                        exceptionCaught = true;
                    }
                },
                CancellationToken.None
            );

            Assert.IsTrue(exceptionCaught, "Expected InvalidOperationException but none was thrown");
        }

        #endregion
    }
}