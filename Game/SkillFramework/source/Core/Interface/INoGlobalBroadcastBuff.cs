/// <summary>
/// 无全局事件广播能力的 Buff 标记接口
/// </summary>
/// <remarks>
/// <para>不会触发 <see cref="GlobalBuffEvents"/> 的全局事件广播。</para>
/// </remarks>
public interface INoGlobalBroadcastBuff : IBuff
{
}