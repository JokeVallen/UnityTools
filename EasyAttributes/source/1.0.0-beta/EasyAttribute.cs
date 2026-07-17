using System;

namespace EasyAttributes
{
    /// <summary>
    /// 纳入框架管理的 Attribute 基类
    /// </summary>
    public abstract class EasyAttribute : Attribute
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        public virtual bool Enabled { get; set; } = true;

        /// <summary>
        /// 优先级
        /// </summary>
        public virtual int Priority { get; set; } = 0;
    }
}
