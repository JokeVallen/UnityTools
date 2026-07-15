using System;
using System.Collections.Generic;

/// <summary>
/// Buff 组件
/// </summary>
public sealed class BuffComponent : ITickableComponent, IAttachableComponent, IResettable
{
    private readonly struct BuffEntry
    {
        public Type Type { get; }
        public IBuff Buff { get; }
        public BuffContext Context { get; }
        public ITypedContext ExtraContext { get; }

        public BuffEntry(IBuff buff, in BuffContext context, ITypedContext extraContext)
        {
            Type = buff.GetType();
            Buff = buff;
            Context = context;
            ExtraContext = extraContext;
        }
    }

    private enum CommandType { Add, Remove, Clear }

    private struct PendingCommand
    {
        public CommandType type;
        public IBuff buff;
        public BuffContext context;
        public ITypedContext extraContext;

        private PendingCommand(CommandType type, IBuff buff, in BuffContext context, ITypedContext extraContext)
        {
            this.type = type;
            this.buff = buff;
            this.context = context;
            this.extraContext = extraContext;
        }

        public static PendingCommand Add(IBuff buff, in BuffContext context, ITypedContext extraContext)
        {
            return new PendingCommand(CommandType.Add, buff, context, extraContext);
        }

        public static PendingCommand Remove(IBuff buff, in BuffContext context)
        {
            return new PendingCommand(CommandType.Remove, buff, context, null);
        }

        public static PendingCommand Clear()
        {
            return new PendingCommand(CommandType.Clear, null, default, null);
        }
    }

    private IEntity owner;
    private readonly List<BuffEntry> activeBuffs = new List<BuffEntry>();
    private readonly List<PendingCommand> pendingCommands = new List<PendingCommand>();
    private readonly Dictionary<Type, BuffEntry> buffMapper = new Dictionary<Type, BuffEntry>();
    private bool isUpdating;

    /// <summary>
    /// Buff 应用事件
    /// </summary>
    public event Action<IBuff, BuffContext> OnBuffApplied;

    /// <summary>
    /// Buff 移除事件
    /// </summary>
    public event Action<IBuff, BuffContext> OnBuffRemoved;

    /// <inheritdoc/>
    void IAttachableComponent.OnAttach(IComponentAttachable owner)
    {
        if (!(owner is IEntity))
            throw new ArgumentException($"The parameter '{nameof(owner)}' must implement the '{typeof(IEntity)}' interface.");
        this.owner = (IEntity)owner;
    }

    /// <inheritdoc/>
    void IAttachableComponent.OnDetach()
    {
        ClearImmediately();
        owner = null;
    }

    /// <inheritdoc/>
    void ITickableComponent.Tick(float deltaTime)
    {
        if (owner == null) return;
        isUpdating = true;

        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            var entry = activeBuffs[i];
            entry.Buff.Tick(entry.Context, entry.ExtraContext, deltaTime);
        }

        isUpdating = false;
        ExecutePendingCommands();
    }

    /// <summary>
    /// 添加 Buff
    /// </summary>
    /// <param name="buff">Buff</param>
    /// <param name="context">Buff 上下文</param>
    /// <param name="extraContext">自定义上下文</param>
    /// <exception cref="ArgumentNullException"><paramref name="buff"/> 不能为 null。</exception>
    /// <exception cref="ArgumentException"><paramref name="buff"/> 需要自定义上下文，但 <paramref name="extraContext"/> 为 null。</exception>
    public void AddBuff(IBuff buff, in BuffContext context, ITypedContext extraContext)
    {
        if (buff == null) throw new ArgumentNullException(nameof(buff));
        if (!buff.IsNoExtraContext() && extraContext == null)
            throw new ArgumentException($"The '{buff.GetType().Name}' buff requires an '{typeof(ITypedContext)}' context, but null was passed.");

        if (isUpdating) pendingCommands.Add(PendingCommand.Add(buff, context, extraContext));
        else ExecuteAdd(buff, context, extraContext);
    }

    /// <summary>
    /// 移除指定类型的 Buff
    /// </summary>
    /// <param name="buff">Buff 类型</param>
    public void RemoveBuff(Type type)
    {
        if (type == null || !buffMapper.ContainsKey(type)) return;
        var index = activeBuffs.FindIndex(e => e.Type == type);
        if (index < 0) return;
        var buff = activeBuffs[index].Buff;
        if (isUpdating) pendingCommands.Add(PendingCommand.Remove(buff, default));
        else ExecuteRemove(buff);
    }

    /// <summary>
    /// 移除指定类型的 Buff
    /// </summary>
    /// <typeparam name="T">Buff 类型</typeparam>
    public void RemoveBuff<T>() where T : IBuff
    {
        var type = typeof(T);
        if (!buffMapper.ContainsKey(type)) return;
        var index = activeBuffs.FindIndex(e => e.Type == type);
        if (index < 0) return;
        var buff = activeBuffs[index].Buff;
        if (isUpdating) pendingCommands.Add(PendingCommand.Remove(buff, default));
        else ExecuteRemove(buff);
    }

    /// <summary>
    /// 是否包含指定类型 Buff
    /// </summary>
    /// <param name="type">Buff 类型</param>
    /// <returns>包含则返回 true，否则返回 false</returns>
    public bool HasBuff(Type type)
    {
        if (type == null) return false;
        return buffMapper.ContainsKey(type);
    }

    /// <summary>
    /// 是否包含指定类型的 Buff
    /// </summary>
    /// <typeparam name="T">Buff 类型</typeparam>
    /// <returns>包含则返回 true，否则返回 false</returns>
    public bool HasBuff<T>() where T : IBuff
    {
        var type = typeof(T);
        return buffMapper.ContainsKey(type);
    }

    /// <summary>
    /// 清空所有 Buff
    /// </summary>
    public void Clear()
    {
        if (isUpdating) pendingCommands.Add(PendingCommand.Clear());
        else ClearImmediately();
    }

    /// <inheritdoc/>
    void IResettable.Reset()
    {
        owner = null;
        activeBuffs.Clear();
        pendingCommands.Clear();
        buffMapper.Clear();
        isUpdating = false;
        OnBuffApplied = null;
        OnBuffRemoved = null;
    }

    private void ExecuteAdd(IBuff buff, in BuffContext context, ITypedContext extraContext)
    {
        var type = buff.GetType();
        if (buffMapper.TryGetValue(type, out _))
        {
            if (buff.IsStackable())
                ((IStackableBuff)buff).Stack(context, extraContext);
            return;
        }

        buff.ApplyTo(context, extraContext);
        var entry = new BuffEntry(buff, context, extraContext);
        activeBuffs.Add(entry);
        buffMapper[buff.GetType()] = entry;
        if (OnBuffApplied != null) OnBuffApplied.Invoke(buff, context);
        GlobalBuffEvents.BroadcastApplied(owner, buff, context);
    }

    private void ExecuteRemove(IBuff buff)
    {
        for (int i = 0; i < activeBuffs.Count; i++)
        {
            if (activeBuffs[i].Buff == buff)
            {
                var entry = activeBuffs[i];
                entry.Buff.RemoveFrom(entry.Context, entry.ExtraContext);
                activeBuffs.RemoveAt(i);
                buffMapper.Remove(entry.Type);
                if (OnBuffRemoved != null) OnBuffRemoved.Invoke(buff, entry.Context);
                GlobalBuffEvents.BroadcastRemoved(owner, buff, entry.Context);
                return;
            }
        }
    }

    private void ClearImmediately()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            var entry = activeBuffs[i];
            entry.Buff.RemoveFrom(entry.Context, entry.ExtraContext);
            if (OnBuffRemoved != null) OnBuffRemoved.Invoke(entry.Buff, entry.Context);
            GlobalBuffEvents.BroadcastRemoved(owner, entry.Buff, entry.Context);
        }
        activeBuffs.Clear();
        buffMapper.Clear();
    }

    private void ExecutePendingCommands()
    {
        if (pendingCommands.Count == 0) return;

        for (int i = 0; i < pendingCommands.Count; i++)
        {
            var cmd = pendingCommands[i];
            switch (cmd.type)
            {
                case CommandType.Add:
                    ExecuteAdd(cmd.buff, cmd.context, cmd.extraContext);
                    break;
                case CommandType.Remove:
                    ExecuteRemove(cmd.buff);
                    break;
                case CommandType.Clear:
                    ClearImmediately();
                    break;
            }
        }
        pendingCommands.Clear();
    }
}