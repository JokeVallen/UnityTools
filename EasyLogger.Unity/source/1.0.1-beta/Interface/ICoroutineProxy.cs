namespace EasyLogger.Unity
{
    /// <summary>
    /// 协程代理接口
    /// </summary>
    public interface ICoroutineProxy
    {
        /// <summary>
        /// 开启协程
        /// </summary>
        /// <param name="enumerator">迭代器</param>
        /// <returns>协程实例</returns>
        UnityEngine.Coroutine StartCoroutine(System.Collections.IEnumerator enumerator);

        /// <summary>
        /// 停止协程
        /// </summary>
        /// <param name="coroutine">协程实例</param>
        void StopCoroutine(UnityEngine.Coroutine coroutine);
    }
}