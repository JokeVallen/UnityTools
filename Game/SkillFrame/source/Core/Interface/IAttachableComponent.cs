/// <summary>
/// 组件的附着能力扩展接口
/// </summary>
public interface IAttachableComponent : IComponent
{
    /// <summary>
    /// 附着回调
    /// </summary>
    /// <param name="owner">附着对象</param>
    void OnAttach(IComponentAttachable owner);

    /// <summary>
    /// 取消附着回调
    /// </summary>
    void OnDetach();
}