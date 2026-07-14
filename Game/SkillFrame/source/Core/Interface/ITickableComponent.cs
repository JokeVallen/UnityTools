/// <summary>
/// 组件的帧推进能力接口
/// </summary>
public interface ITickableComponent : IComponent
{
    /// <summary>
    /// 帧推进
    /// </summary>
    /// <param name="deltaTime">时间差</param>
    void Tick(float deltaTime);
}