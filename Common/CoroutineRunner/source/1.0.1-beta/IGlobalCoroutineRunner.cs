using System.Collections;
using UnityEngine;

namespace CoroutineRunner
{
    /// <summary>
    /// 全局协程运行器接口
    /// </summary>
    /// <remarks>
    /// <para>为全局协程调用提供切片入口或者代理，或者希望在不实现自定义 <see cref="MonoBehaviour"/> 的前提下使用协程，通过该类可以便捷和快速地启动任何位于主线程的协程代码。</para>
    /// </remarks>
    public interface IGlobalCoroutineRunner
    {
        /// <summary>
        /// 启动协程
        /// </summary>
        /// <param name="routine">协程枚举器</param>
        /// <returns>协程对象</returns>
        Coroutine StartCoroutine(IEnumerator routine);

        /// <summary>
        /// 启动协程
        /// </summary>
        /// <param name="methodName">提供协程枚举器的方法名</param>
        /// <param name="value">协程参数值</param>
        /// <returns>协程对象</returns>
        /// <remarks>
        /// <para>该版本运行时开销更大</para>
        /// </remarks>
        Coroutine StartCoroutine(string methodName, object value);

        /// <summary>
        /// 停止协程
        /// </summary>
        /// <param name="coroutine">协程对象</param>
        void StopCoroutine(Coroutine coroutine);

        /// <summary>
        /// 停止协程
        /// </summary>
        /// <param name="routine">协程枚举器</param>
        void StopCoroutine(IEnumerator routine);

        /// <summary>
        /// 停止托管协程
        /// </summary>
        /// <param name="methodName">提供协程枚举器的方法名</param>
        void StopCoroutine(string methodName);

        /// <summary>
        /// 停止所有协程
        /// </summary>
        void StopAllCoroutines();

        /// <summary>
        /// 启动一个受自定义句柄控制的高级协程
        /// </summary>
        /// <param name="routine">协程枚举器</param>
        /// <returns>可控的生命周期句柄</returns>
        /// <remarks>
        /// <para>不排队，即时运行，受对象池优化管理。</para>
        /// </remarks>
        CoroutineHandleToken Run(IEnumerator routine);

        /// <summary>
        /// 将高级协程送入指定的并发通道排队执行
        /// </summary>
        /// <param name="routine">协程枚举器</param>
        /// <param name="channelKey">通道标识（若通道未通过 <see cref="ConfigureChannel"/> 配置，则默认作为单列强制排队通道）</param>
        /// <returns>可控的生命周期句柄</returns>
        CoroutineHandleToken RunQueued<T>(IEnumerator routine, T channelKey);

        /// <summary>
        /// 配置指定并发通道的最大并发任务数
        /// </summary>
        /// <param name="channelKey">通道标识</param>
        /// <param name="maxConcurrent">最大允许同时运行的高级协程数量（若小于或等于 0 则代表该通道不限制并发）</param>
        void ConfigureChannel<T>(T channelKey, int maxConcurrent);

        /// <summary>
        /// 释放资源
        /// </summary>
        void Dispose();
    }
}