using System.Collections;
using UnityEngine;

namespace CoroutineRunner
{
    /// <summary>
    /// 全局协程运行器
    /// </summary>
    public static class GlobalCoroutineRunner
    {
        private static IGlobalCoroutineRunner Instance => InternalCoroutineRunner.Instance;

        /// <summary>启动协程</summary>
        public static Coroutine StartCoroutine(IEnumerator routine) => Instance.StartCoroutine(routine);

        /// <summary>通过方法名启动协程（运行时开销较大）</summary>
        public static Coroutine StartCoroutine(string methodName, object value) => Instance.StartCoroutine(methodName, value);

        /// <summary>停止指定协程对象</summary>
        public static void StopCoroutine(Coroutine coroutine) => Instance.StopCoroutine(coroutine);

        /// <summary>停止指定迭代器的协程</summary>
        public static void StopCoroutine(IEnumerator routine) => Instance.StopCoroutine(routine);

        /// <summary>通过方法名停止协程</summary>
        public static void StopCoroutine(string methodName) => Instance.StopCoroutine(methodName);

        /// <summary>停止所有协程</summary>
        public static void StopAllCoroutines() => Instance.StopAllCoroutines();

        /// <summary>配置命名通道的最大并发数</summary>
        public static void ConfigureChannel(string channelName, int maxConcurrent) => Instance.ConfigureChannel(channelName, maxConcurrent);

        /// <summary>启动一个可控协程（不排队，立即运行）</summary>
        public static CoroutineHandleToken Run(IEnumerator routine) => Instance.Run(routine);

        /// <summary>将可控协程送入指定通道排队执行</summary>
        public static CoroutineHandleToken RunQueued(IEnumerator routine, string channelName) => Instance.RunQueued(routine, channelName);

        /// <summary>释放运行器资源</summary>
        public static void Dispose() => Instance.Dispose();
    }
}
