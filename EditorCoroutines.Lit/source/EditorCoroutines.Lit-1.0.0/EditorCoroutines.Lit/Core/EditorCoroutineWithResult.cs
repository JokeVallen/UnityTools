#if UNITY_EDITOR

using System;
using System.Collections;
using UnityEditor;

/// <summary>
/// 编辑器协程
/// </summary>
/// <typeparam name="T">执行结果的类型</typeparam>
public class EditorCoroutine<T> : IDisposable
{
    #region Public

    /// <summary>
    /// 是否正在运行
    /// </summary>
    public bool IsRunning => isRunning;

    /// <summary>
    /// 是否已完成
    /// </summary>
    public bool IsCompleted => isCompleted;

    /// <summary>
    /// 执行结果
    /// </summary>
    public T Result => result;

    /// <summary>
    /// 异常
    /// </summary>
    public Exception Exception => exception;

    /// <summary>
    /// 启动协程
    /// </summary>
    /// <param name="routine">迭代器对象</param>
    /// <param name="onComplete">完成回调</param>
    /// <param name="onException">异常回调</param>
    /// <returns>编辑器协程</returns>
    public static EditorCoroutine<T> StartCoroutine(IEnumerator routine, Action<T> onComplete = null, Action<Exception> onException = null)
    {
        return StartCoroutineInternal(routine, onComplete, onException);
    }

    /// <summary>
    /// 启动协程
    /// </summary>
    public void Start()
    {
        StartInternal();
    }

    /// <summary>
    /// 停止协程
    /// </summary>
    public void Stop()
    {
        StopInternal();
    }

    /// <summary>
    /// 释放协程
    /// </summary>
    public void Dispose()
    {
        DisposeInternal();
    }

    #endregion

    #region Internal

    private readonly IEnumerator routine;
    private bool isRunning;
    private bool isCompleted;
    private Action<T> onComplete;
    private Action<Exception> onException;
    private T result;
    private bool disposed;
    private Exception exception;

    private EditorCoroutine(IEnumerator routine)
    {
        this.routine = EditorCoroutineHelper.WrapRoutine(routine, HandleException);
    }

    private static EditorCoroutine<T> StartCoroutineInternal(IEnumerator routine, Action<T> onComplete, Action<Exception> onException)
    {
        var ec = new EditorCoroutine<T>(routine);
        ec.onComplete = onComplete;
        ec.onException = onException;
        ec.StartInternal();
        return ec;
    }

    private void StartInternal()
    {
        if (disposed)
        {
            var ex = new ObjectDisposedException("EditorCoroutine<T>");
            onException?.Invoke(ex);
            throw ex;
        }

        if (isRunning)
            return;

        isRunning = true;

        EditorApplication.update += Update;
    }

    private void StopInternal()
    {
        if (!isRunning)
            return;

        isRunning = false;
        EditorApplication.update -= Update;
    }

    private void DisposeInternal()
    {
        if (disposed)
            return;

        disposed = true;
        StopInternal();
        onComplete = null;
        onException = null;
    }

    private void Update()
    {
        try
        {
            if (!routine.MoveNext())
            {
                isCompleted = true;
                onComplete?.Invoke(result);
                StopInternal();
            }
            else if (routine.Current is Func<T> func)
            {
                result = func();
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void HandleException(Exception ex)
    {
        exception = ex;
        onException?.Invoke(ex);
        StopInternal();
        isCompleted = true;
    }

    #endregion
}

#endif