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

        public async UniTask InvokeAsync(IView view, UIPipelineExecutor executor, CancellationToken token)
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

        public async UniTask InvokeAsync(IView view, UIPipelineExecutor executor, CancellationToken token)
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

        public async UniTask InvokeAsync(IView view, UIPipelineExecutor executor, CancellationToken token)
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

        public void PopulateMiddlewares(IView view, IReadOnlyList<IViewMiddleware> staticMiddlewares, IDynamicMiddlewareCollection dynamicMiddlewares)
        {
            if (_condition(view))
            {
                dynamicMiddlewares.Add(_middleware);
            }
        }
    }

    // 测试执行策略
    public class TestExecutionPolicy : IMiddlewareExecutionPolicy
    {
        private readonly Func<IView, IViewMiddleware, bool> _shouldSkip;

        public TestExecutionPolicy(Func<IView, IViewMiddleware, bool> shouldSkip)
        {
            _shouldSkip = shouldSkip;
        }

        public bool ShouldSkip(IView view, IViewMiddleware middleware)
        {
            return _shouldSkip(view, middleware);
        }
    }

    // 测试扩展包
    internal class TestExtension : IExtension
    {
        private readonly IEnumerable<IViewMiddleware> _openMiddlewares;
        private readonly IEnumerable<IViewMiddleware> _closeMiddlewares;

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

        public IEnumerable<IMiddlewareValidator> GetMiddlewareValidators()
        {
            return Array.Empty<IMiddlewareValidator>();
        }

        public void Initialize() { }
    }

    [TestFixture]
    public class ViewPipelineEditModeTests
    {
        private MockView _mockView;

        [SetUp]
        public void SetUp()
        {
            _mockView = new MockView();
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
            Assert.DoesNotThrow(() =>
            {
                var session = ViewSessionBuilder.Create().Build();
            });
        }

        [UnityTest]
        public IEnumerator OpenViewAsync_WithoutMiddleware_ShouldShowView()
        {
            var session = ViewSessionBuilder.Create().Build();

            yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();

            Assert.IsTrue(_mockView.IsShown);
            Assert.AreEqual(1, _mockView.ShowCallCount);
        }

        [UnityTest]
        public IEnumerator CloseViewAsync_WithoutMiddleware_ShouldHideView()
        {
            var session = ViewSessionBuilder.Create().Build();

            yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();
            yield return session.CloseViewAsync(_mockView, CancellationToken.None).AsCoroutine();

            Assert.IsTrue(_mockView.IsHidden);
            Assert.AreEqual(1, _mockView.HideCallCount);
        }

        [UnityTest]
        public IEnumerator OpenViewAsync_WithStaticMiddleware_ShouldExecuteMiddleware()
        {
            var middleware = new TestMiddleware();
            var session = ViewSessionBuilder.Create()
                .AddOpenMiddleware(middleware)
                .Build();

            yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();

            Assert.IsTrue(middleware.BeforeNextCalled);
            Assert.IsTrue(middleware.AfterNextCalled);
            Assert.IsTrue(_mockView.IsShown);
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

            yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();

            Assert.AreEqual(4, executionOrder.Count);
            Assert.AreEqual("A-Before", executionOrder[0]);
            Assert.AreEqual("B-Before", executionOrder[1]);
            Assert.AreEqual("B-After", executionOrder[2]);
            Assert.AreEqual("A-After", executionOrder[3]);
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

            yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();

            Assert.IsTrue(middleware.BeforeNextCalled);
            Assert.IsTrue(middleware.AfterNextCalled);
            Assert.IsFalse(_mockView.IsShown);
        }

        [UnityTest]
        public IEnumerator OpenViewAsync_WithInterceptedMiddleware_ShouldAllowReopenAfterRollback()
        {
            var middleware = new TestMiddleware();
            middleware.SetIntercept(true);

            var session = ViewSessionBuilder.Create()
                .AddOpenMiddleware(middleware)
                .Build();

            // 第一次打开被拦截
            yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();
            Assert.IsFalse(_mockView.IsShown);

            // 取消拦截，重新打开
            middleware.SetIntercept(false);
            yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();

            Assert.IsTrue(_mockView.IsShown);
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

            yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();

            Assert.IsTrue(dynamicMiddleware.Executed);
        }

        [UnityTest]
        public IEnumerator OpenViewAsync_WithDynamicProvider_ConditionFalse_ShouldNotAddMiddleware()
        {
            var dynamicMiddleware = new TestDynamicMiddleware();
            var provider = new TestDynamicProvider(view => false, dynamicMiddleware);

            var session = ViewSessionBuilder.Create()
                .AddOpenDynamicProvider(provider)
                .Build();

            yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();

            Assert.IsFalse(dynamicMiddleware.Executed);
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
                .SetMiddlewareExecutionPolicy(policy)
                .Build();

            yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();

            Assert.IsFalse(middleware.BeforeNextCalled);
            Assert.IsFalse(middleware.AfterNextCalled);
            Assert.IsTrue(_mockView.IsShown);
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

            yield return session.OpenViewAsync(_mockView, CancellationToken.None).AsCoroutine();

            Assert.IsTrue(middleware1.BeforeNextCalled);
            Assert.IsTrue(middleware2.BeforeNextCalled);
        }

        #endregion

        #region 异常测试

        [Test]
        public void BuildSession_WithNullRegistry_ShouldThrow()
        {
            var builder = ViewSessionBuilder.Create();
            Assert.Throws<ArgumentNullException>(() => builder.WithRegistry(null));
        }

        [Test]
        public void BuildSession_WithNullStackPolicy_ShouldThrow()
        {
            var builder = ViewSessionBuilder.Create();
            Assert.Throws<ArgumentNullException>(() => builder.WithStackPolicy(null));
        }

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

            // 再次调用应无异常
            yield return session.DisposeAsync().AsCoroutine();
        }

        [UnityTest]
        public IEnumerator OpenViewAsync_AfterDispose_ShouldThrow()
        {
            var session = ViewSessionBuilder.Create().Build();
            yield return session.DisposeAsync().AsCoroutine();

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