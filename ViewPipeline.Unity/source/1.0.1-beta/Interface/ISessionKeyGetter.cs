using System;

namespace ViewPipeline.Unity.Core
{
    /// <summary>
    /// 会话唯一标识访问接口
    /// </summary>
    public interface ISessionKeyGetter
    {
        /// <summary>
        /// 会话唯一标识
        /// </summary>
        Guid Key { get; }
    }
}
