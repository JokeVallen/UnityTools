#if UNITY_EDITOR

using System;
using System.Collections;
using UnityEditor;

namespace EditorCoroutines.Lit
{
    /// <summary>
    /// 编辑器协程
    /// </summary>
    public class EditorCoroutine : IDisposable
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
        public static EditorCoroutine StartCoroutine(IEnumerator routine, Action onComplete = null, Action<Exception> onException = null)
        {
            return StartCoroutineInternal(routine, onComplete, onException);
        }

        /// <summary>
        /// 启动协程
        /// </summary>
        public void Start() => StartInternal();

        /// <summary>
        /// 停止协程
        /// </summary>
        public void Stop() => StopInternal();

        /// <summary>
        /// 释放协程
        /// </summary>
        public void Dispose() => DisposeInternal();

        #endregion

        #region Internal

        private readonly IEnumerator routine;
        private bool isRunning;
        private bool isCompleted;
        private Action onComplete;
        private Action<Exception> onException;
        private bool disposed;
        private Exception exception;

        private EditorCoroutine(IEnumerator routine)
        {
            this.routine = EditorCoroutineHelper.WrapRoutine(routine, HandleException);
        }

        private static EditorCoroutine StartCoroutineInternal(IEnumerator routine, Action onComplete, Action<Exception> onException)
        {
            var ec = new EditorCoroutine(routine);
            ec.onComplete = onComplete;
            ec.onException = onException;
            ec.StartInternal();
            return ec;
        }

        private void StartInternal()
        {
            if (disposed)
            {
                var ex = new ObjectDisposedException("EditorCoroutine");
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
                    onComplete?.Invoke();
                    StopInternal();
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
}
#endif