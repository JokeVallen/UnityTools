using System;

/// <summary>
/// Buff 全局事件总线
/// </summary>
public static class GlobalBuffEvents
{
    /// <summary>
    /// Buff 应用事件
    /// </summary>
    public static event Action<IEntity, IBuff, BuffContext> OnBuffApplied;

    /// <summary>
    /// Buff 移除事件
    /// </summary>
    public static event Action<IEntity, IBuff, BuffContext> OnBuffRemoved;

    internal static void BroadcastApplied(IEntity entity, IBuff buff, in BuffContext context)
    {
        if (OnBuffApplied == null || buff.IsNoGlobalBroadcast()) return;
        OnBuffApplied.Invoke(entity, buff, context);
    }

    internal static void BroadcastRemoved(IEntity entity, IBuff buff, in BuffContext context)
    {
        if (OnBuffRemoved == null || buff.IsNoGlobalBroadcast()) return;
        OnBuffRemoved.Invoke(entity, buff, context);
    }
}