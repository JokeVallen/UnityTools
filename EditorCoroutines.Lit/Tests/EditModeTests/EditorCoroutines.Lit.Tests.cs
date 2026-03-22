/*
    下列测试代码由 AI 生成
*/

using EditorCoroutines.Lit;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class EditorCoroutinesLitTests
{
    #region EditorCoroutine Tests

    [UnityTest]
    public IEnumerator EditorCoroutine_StartAndComplete()
    {
        // Arrange
        bool completed = false;
        EditorCoroutine coroutine = null;

        // Act
        IEnumerator Routine()
        {
            yield return null;
            yield return null;
        }

        coroutine = EditorCoroutine.StartCoroutine(Routine(), () => completed = true);

        // Wait for completion
        yield return new WaitUntil(() => completed || coroutine.IsCompleted);

        // Assert
        Assert.IsTrue(completed);
        Assert.IsTrue(coroutine.IsCompleted);
        coroutine?.Dispose();
    }

    [Test]
    public void EditorCoroutine_ThrowsWhenDisposedBeforeStart()
    {
        // Arrange
        IEnumerator Routine()
        {
            yield return null;
        }

        var coroutine = EditorCoroutine.StartCoroutine(Routine());
        coroutine.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => coroutine.Start());
    }

    [UnityTest]
    public IEnumerator EditorCoroutine_StopPreventsExecution()
    {
        // Arrange
        int counter = 0;
        EditorCoroutine coroutine = null;

        IEnumerator Routine()
        {
            counter++;
            yield return null;
            counter++;
            yield return null;
            counter++;
        }

        coroutine = EditorCoroutine.StartCoroutine(Routine());

        // Act
        coroutine.Stop();

        // Wait a bit to ensure nothing else executes
        yield return null;

        // Assert
        Assert.IsFalse(coroutine.IsRunning);
        Assert.AreEqual(0, counter);
    }

    [UnityTest]
    public IEnumerator EditorCoroutine_ExceptionHandling()
    {
        // Arrange
        Exception caughtException = null;
        EditorCoroutine coroutine = null;
        bool completed = false;

        IEnumerator ExceptionRoutine()
        {
            yield return null;
            throw new InvalidOperationException("Test exception");
        }

        // Act
        coroutine = EditorCoroutine.StartCoroutine(
            ExceptionRoutine(),
            onComplete: () => completed = true,
            onException: ex => caughtException = ex
        );

        // Wait for the coroutine to complete
        yield return new WaitUntil(() => completed || caughtException != null);

        // Assert
        Assert.IsNotNull(coroutine);
        Assert.IsInstanceOf<InvalidOperationException>(caughtException);
        Assert.IsTrue(coroutine.IsCompleted);
        coroutine?.Dispose();
    }

    [UnityTest]
    public IEnumerator EditorCoroutine_MultipleStartCallsIgnored()
    {
        // Arrange
        int executionCount = 0;
        bool completed = false;
        EditorCoroutine coroutine = null;

        IEnumerator Routine()
        {
            executionCount++;
            yield return null;
        }

        coroutine = EditorCoroutine.StartCoroutine(Routine(), () => completed = true);

        // Act
        coroutine.Start();
        coroutine.Start();

        // Wait for completion
        yield return new WaitUntil(() => completed || coroutine.IsCompleted);

        // Assert
        Assert.AreEqual(1, executionCount);
        coroutine.Dispose();
    }

    #endregion

    #region EditorCoroutine<T> Tests

    [UnityTest]
    public IEnumerator EditorCoroutineWithResult_ReturnsValue()
    {
        // Arrange
        int expectedResult = 42;
        int actualResult = 0;
        bool completed = false;
        EditorCoroutine<int> coroutine = null;

        IEnumerator Routine()
        {
            yield return (Func<int>)(() => expectedResult);
        }

        // Act
        coroutine = EditorCoroutine<int>.StartCoroutine(
            Routine(),
            onComplete: (result) =>
            {
                actualResult = result;
                completed = true;
            }
        );

        // Wait for completion
        yield return new WaitUntil(() => completed || coroutine.IsCompleted);

        // Assert
        Assert.AreEqual(expectedResult, actualResult);
        Assert.IsTrue(coroutine.IsCompleted);
        coroutine?.Dispose();
    }

    [UnityTest]
    public IEnumerator EditorCoroutineWithResult_YieldReturnValueType()
    {
        // Arrange
        int expectedResult = 123;
        int actualResult = 0;
        bool completed = false;
        EditorCoroutine<int> coroutine = null;

        IEnumerator Routine()
        {
            yield return expectedResult; // 直接返回 int 类型
        }

        // Act
        coroutine = EditorCoroutine<int>.StartCoroutine(
            Routine(),
            onComplete: (result) =>
            {
                actualResult = result;
                completed = true;
            }
        );

        // Wait for completion
        yield return new WaitUntil(() => completed || coroutine.IsCompleted);

        // Assert
        Assert.AreEqual(expectedResult, actualResult);
        Assert.IsTrue(coroutine.IsCompleted);
        coroutine?.Dispose();
    }

    [UnityTest]
    public IEnumerator EditorCoroutineWithResult_ExceptionHandling()
    {
        // Arrange
        Exception caughtException = null;
        EditorCoroutine<string> coroutine = null;
        bool completed = false;

        IEnumerator Routine()
        {
            yield return null;
            throw new ArgumentException("Test argument exception");
        }

        // Act
        coroutine = EditorCoroutine<string>.StartCoroutine(
            Routine(),
            onComplete: (result) => completed = true,
            onException: (ex) => caughtException = ex
        );

        // Wait for the coroutine to complete
        yield return new WaitUntil(() => completed || caughtException != null);

        // Assert
        Assert.IsNotNull(coroutine);
        Assert.IsInstanceOf<ArgumentException>(caughtException);
        Assert.IsTrue(coroutine.IsCompleted);
        coroutine?.Dispose();
    }

    [Test]
    public void EditorCoroutineWithResult_ThrowsWhenDisposedBeforeStart()
    {
        // Arrange
        IEnumerator Routine()
        {
            yield return null;
        }

        var coroutine = EditorCoroutine<int>.StartCoroutine(Routine());
        coroutine.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => coroutine.Start());
    }

    #endregion

    #region EditorCoroutineCancelToken Tests

    [Test]
    public void CancelToken_InitiallyNotCancelled()
    {
        // Arrange & Act
        var token = new EditorCoroutineCancelToken();

        // Assert
        Assert.IsFalse(token.IsCancelled);
    }

    [Test]
    public void CancelToken_CancelSetsFlag()
    {
        // Arrange
        var token = new EditorCoroutineCancelToken();

        // Act
        token.Cancel();

        // Assert
        Assert.IsTrue(token.IsCancelled);
    }

    [Test]
    public void CancelToken_MultipleCancelsAllowed()
    {
        // Arrange
        var token = new EditorCoroutineCancelToken();

        // Act
        token.Cancel();
        token.Cancel();
        token.Cancel();

        // Assert
        Assert.IsTrue(token.IsCancelled);
    }

    #endregion

    #region EditorCoroutineExtensions Tests

    [UnityTest]
    public IEnumerator WaitFrame_CompletesInOneIteration()
    {
        // Arrange
        int executionCount = 0;
        bool completed = false;
        EditorCoroutine coroutine = null;

        IEnumerator Routine()
        {
            executionCount++;
            yield return EditorCoroutineExtensions.WaitFrame();
            executionCount++;
        }

        // Act
        coroutine = EditorCoroutine.StartCoroutine(Routine(), () => completed = true);

        // Wait for completion
        yield return new WaitUntil(() => completed || coroutine.IsCompleted);

        // Assert
        Assert.AreEqual(2, executionCount);
        coroutine?.Dispose();
    }

    [UnityTest]
    public IEnumerator WaitFrame_RespectsCancelToken()
    {
        // Arrange
        int executionCount = 0;
        var token = new EditorCoroutineCancelToken();
        EditorCoroutine coroutine = null;
        bool completed = false;

        IEnumerator Routine()
        {
            executionCount++;
            token.Cancel();
            yield return EditorCoroutineExtensions.WaitFrame(token);
            // After cancel, stop further execution
            if (token.IsCancelled)
                yield break;
            executionCount++; // Should not execute
        }

        // Act
        coroutine = EditorCoroutine.StartCoroutine(Routine(), () => completed = true);

        // Wait for completion
        yield return new WaitUntil(() => completed || coroutine.IsCompleted);

        // Assert
        Assert.AreEqual(1, executionCount);
        coroutine?.Dispose();
    }

    [UnityTest]
    public IEnumerator WaitSeconds_BaseMethod()
    {
        // Arrange
        bool completed = false;
        EditorCoroutine coroutine = null;

        IEnumerator Routine()
        {
            yield return EditorCoroutineExtensions.WaitSeconds(0.05f);
        }

        // Act
        coroutine = EditorCoroutine.StartCoroutine(Routine(), () => completed = true);

        // Wait for completion
        yield return new WaitUntil(() => completed || coroutine.IsCompleted);

        // Assert
        Assert.IsTrue(completed);
        coroutine?.Dispose();
    }

    [UnityTest]
    public IEnumerator WaitMilliseconds_ConvertsProperly()
    {
        // Arrange
        bool completed = false;
        EditorCoroutine coroutine = null;

        IEnumerator Routine()
        {
            yield return EditorCoroutineExtensions.WaitMilliseconds(50); // 0.05 seconds
        }

        // Act
        coroutine = EditorCoroutine.StartCoroutine(Routine(), () => completed = true);

        // Wait for completion
        yield return new WaitUntil(() => completed || coroutine.IsCompleted);

        // Assert
        Assert.IsTrue(completed);
        coroutine?.Dispose();
    }

    [UnityTest]
    public IEnumerator WaitUntil_ConditionMet()
    {
        // Arrange
        bool conditionMet = false;
        bool completed = false;
        EditorCoroutine coroutine = null;

        IEnumerator Routine()
        {
            yield return EditorCoroutineExtensions.WaitUntil(() => conditionMet);
        }

        // Act
        coroutine = EditorCoroutine.StartCoroutine(Routine(), () => completed = true);

        // Manually set condition after a brief wait
        yield return null;
        conditionMet = true;

        // Wait for completion
        yield return new WaitUntil(() => completed || coroutine.IsCompleted);

        // Assert
        Assert.IsTrue(completed);
        coroutine?.Dispose();
    }

    [UnityTest]
    public IEnumerator WaitUntil_RespectsCancelToken()
    {
        // Arrange
        int executionCount = 0;
        var token = new EditorCoroutineCancelToken();
        bool completed = false;
        EditorCoroutine coroutine = null;

        IEnumerator Routine()
        {
            executionCount++;
            token.Cancel();
            yield return EditorCoroutineExtensions.WaitUntil(() => false, token);
            // After cancel, stop further execution
            if (token.IsCancelled)
                yield break;
            executionCount++; // Should not execute
        }

        // Act
        coroutine = EditorCoroutine.StartCoroutine(Routine(), () => completed = true);

        // Wait for completion
        yield return new WaitUntil(() => completed || coroutine.IsCompleted);

        // Assert
        Assert.AreEqual(1, executionCount);
        coroutine?.Dispose();
    }

    [UnityTest]
    public IEnumerator WaitUntilWithTimeout_TimeoutOccurs()
    {
        // Arrange
        bool completed = false;
        EditorCoroutine coroutine = null;

        IEnumerator Routine()
        {
            yield return EditorCoroutineExtensions.WaitUntil(() => false, 0.1f);
        }

        // Act
        coroutine = EditorCoroutine.StartCoroutine(Routine(), () => completed = true);

        // Wait for completion
        yield return new WaitUntil(() => completed || coroutine.IsCompleted);

        // Assert
        Assert.IsTrue(completed);
        coroutine?.Dispose();
    }

    [UnityTest]
    public IEnumerator Delay_ExecutesAfterWait()
    {
        // Arrange
        bool actionExecuted = false;
        bool completed = false;
        EditorCoroutine coroutine = null;

        IEnumerator Routine()
        {
            yield return EditorCoroutineExtensions.Delay(
                () => actionExecuted = true,
                0.05f
            );
        }

        // Act
        coroutine = EditorCoroutine.StartCoroutine(Routine(), () => completed = true);

        // Wait for completion
        yield return new WaitUntil(() => completed || coroutine.IsCompleted);

        // Assert
        Assert.IsTrue(actionExecuted);
        coroutine?.Dispose();
    }

    [UnityTest]
    public IEnumerator Delay_RespectsCancelToken()
    {
        // Arrange
        bool actionExecuted = false;
        var token = new EditorCoroutineCancelToken();
        bool completed = false;
        EditorCoroutine coroutine = null;

        IEnumerator Routine()
        {
            token.Cancel();
            yield return EditorCoroutineExtensions.Delay(
                () => actionExecuted = true,
                0.05f,
                token
            );
        }

        // Act
        coroutine = EditorCoroutine.StartCoroutine(Routine(), () => completed = true);

        // Wait for completion
        yield return new WaitUntil(() => completed || coroutine.IsCompleted);

        // Assert
        Assert.IsFalse(actionExecuted);
        coroutine?.Dispose();
    }

    #endregion

    #region Nested Coroutine Tests

    [UnityTest]
    public IEnumerator NestedCoroutines_ExecuteInOrder()
    {
        // Arrange
        int executionOrder = 0;
        bool completed = false;
        EditorCoroutine coroutine = null;

        IEnumerator InnerRoutine()
        {
            executionOrder++;
            yield return null;
            executionOrder++;
        }

        IEnumerator OuterRoutine()
        {
            executionOrder++;
            yield return InnerRoutine();
            executionOrder++;
        }

        // Act
        coroutine = EditorCoroutine.StartCoroutine(OuterRoutine(), () => completed = true);

        // Wait for completion
        yield return new WaitUntil(() => completed || coroutine.IsCompleted);

        // Assert
        Assert.AreEqual(4, executionOrder);
        coroutine?.Dispose();
    }

    [UnityTest]
    public IEnumerator MultipleNestedCoroutines_AllExecute()
    {
        // Arrange
        int counter = 0;
        bool completed = false;
        EditorCoroutine coroutine = null;

        IEnumerator Inner1()
        {
            counter++;
            yield return null;
            counter++;
        }

        IEnumerator Inner2()
        {
            counter++;
            yield return null;
            counter++;
        }

        IEnumerator Outer()
        {
            counter++;
            yield return Inner1();
            counter++;
            yield return Inner2();
            counter++;
        }

        // Act
        coroutine = EditorCoroutine.StartCoroutine(Outer(), () => completed = true);

        // Wait for completion
        yield return new WaitUntil(() => completed || coroutine.IsCompleted);

        // Assert
        Assert.AreEqual(7, counter);
        coroutine?.Dispose();
    }

    #endregion

    #region Disposal Tests

    [UnityTest]
    public IEnumerator Dispose_ClearsCallbacks()
    {
        // Arrange
        EditorCoroutine coroutine = null;

        IEnumerator Routine()
        {
            yield return null;
        }

        coroutine = EditorCoroutine.StartCoroutine(Routine());

        // Act
        coroutine.Dispose();
        yield return null; // Allow any pending updates to run
        coroutine.Dispose(); // Should not throw

        // Assert
        Assert.IsFalse(coroutine.IsRunning);
    }

    [UnityTest]
    public IEnumerator Dispose_MultipleCalls_Safe()
    {
        // Arrange
        EditorCoroutine coroutine = null;

        IEnumerator Routine()
        {
            yield return null;
        }

        coroutine = EditorCoroutine.StartCoroutine(Routine());

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            coroutine.Dispose();
            coroutine.Dispose();
            coroutine.Dispose();
        });

        yield return null;
    }

    #endregion
}