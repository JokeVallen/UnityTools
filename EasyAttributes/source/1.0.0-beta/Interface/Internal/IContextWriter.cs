using System;

namespace EasyAttributes.Core
{
    /// <summary>
    /// 上下文写入接口
    /// </summary>
    internal interface IContextWriter
    {
        /// <summary>
        /// 向上下文写入或更新一个条目
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        void SetItem(string key, object value);

        /// <summary>
        /// 从上下文移除指定键的条目
        /// </summary>
        /// <param name="key">要移除的条目的键</param>
        void RemoveItem(string key);

        /// <summary>
        /// 向上下文写入或更新一个功能实例
        /// </summary>
        void SetFeature(Type featureType, IFeature feature);

        /// <summary>
        /// 从上下文中移除指定类型的功能实例
        /// </summary>
        /// <param name="featureType">要移除的功能实例对应的类型</param>
        void RemoveFeature(Type featureType);
    }
}
