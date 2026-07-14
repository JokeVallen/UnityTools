public static partial class Extensions
{
    /// <summary>
    /// 是否不具备全局事件广播能力
    /// </summary>
    /// <param name="buff"></param>
    /// <returns></returns>
    public static bool IsNoGlobalBroadcast(this IBuff buff)
    {
        return buff is INoGlobalBroadcastBuff;
    }

    /// <summary>
    /// 是否不需要自定义上下文
    /// </summary>
    /// <param name="buff"></param>
    /// <returns></returns>
    public static bool IsNoExtraContext(this IBuff buff)
    {
        return buff is INoExtraContextBuff;
    }

    /// <summary>
    /// 是否具备可堆叠能力
    /// </summary>
    /// <param name="buff"></param>
    /// <returns></returns>
    public static bool IsStackable(this IBuff buff)
    {
        return buff is IStackableBuff;
    }
}