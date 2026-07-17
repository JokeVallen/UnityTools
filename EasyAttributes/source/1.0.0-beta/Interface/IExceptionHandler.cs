namespace EasyAttributes.Core
{
    /// <summary>
    /// 异常处理器
    /// </summary>
    /// <remarks>
    /// <para>用于处理框架执行过程中产生的 <see cref="EasyAttributeException"/>。</para>
    /// <para>
    /// 若返回 <c>true</c>，则异常被视为已处理，执行器将继续后续流程；
    /// 若返回 <c>false</c>，则异常将重新抛出。
    /// </para>
    /// <para>
    /// 示例：
    /// <code>
    /// public class CustomExceptionHandler : IExceptionHandler
    /// {
    ///     public bool Handle(EasyAttributeException exception)
    ///     {
    ///         // 记录日志
    ///         Console.WriteLine(exception.Message);
    ///         return true; // 已处理
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public interface IExceptionHandler
    {
        /// <summary>
        /// 处理异常
        /// </summary>
        /// <param name="exception">异常</param>
        /// <returns>是否已处理</returns>
        bool Handle(EasyAttributeException exception);
    }
}
