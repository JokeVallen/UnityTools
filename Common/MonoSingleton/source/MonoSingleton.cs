using UnityEngine;

/// <summary>
/// 非持久化单例：当其所在场景销毁时会随之一并销毁，这个属于临时性单例，其生命周期与所在场景一致。
/// </summary>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance => instance;
    private static T instance;

    /// <summary>
    /// 请注意调用 <code> base.Awake(); </code>
    /// </summary>
    protected virtual void Awake()
    {
        if (ReferenceEquals(instance, null)) instance = this as T;
        else Destroy(this);
    }

    /// <summary>
    /// 请注意调用 <code> base.OnDestroy(); </code>
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (ReferenceEquals(this, instance))
            instance = null;
    }
}

/// <summary>
/// 非持久化单例：附带接口访问的变体版本，如果你希望进一步控制单例的访问，可以通过继承该类型以及实现相关接口来提供外部的可访问成员。
/// </summary>
public abstract class MonoSingleton<T, I> : MonoBehaviour where T : MonoBehaviour, I
{
    public static I Instance => instance;
    private static T instance;
    
    /// <summary>
    /// 请注意调用 <code> base.Awake(); </code>
    /// </summary>
    protected virtual void Awake()
    {
        if (ReferenceEquals(instance, null)) instance = this as T;
        else Destroy(this);
    }
    
    /// <summary>
    /// 请注意调用 <code> base.OnDestroy(); </code>
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (ReferenceEquals(this, instance))
            instance = null;
    }
}