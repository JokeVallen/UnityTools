using UnityEngine.Scripting;

namespace EventHub
{
    /// <summary>
    /// 复合事件分发器接口
    /// </summary>
    /// <remarks>
    /// <para>类型：复合型扩展接口</para>
    /// <para>通过该接口可同时对同步、异步和并发等任意一个或多个局部行为进行自定义，而无需实现多个实例。</para>
    /// <para>该接口现在及未来不会提供任何强制约束，可以和任何其它局部扩展接口组合，以提供复合型扩展。</para>
    /// <para>注意：该接口优先级仅大于默认实现，小于局部扩展的接口。</para>
    /// <para>框架级接口采用版本接口，稳定版本的接口不会在后续版本出现更改，且新版本兼容旧版本。</para>
    /// </remarks>
    [Preserve]
    public interface IEventDispatcher
    {
    }
}