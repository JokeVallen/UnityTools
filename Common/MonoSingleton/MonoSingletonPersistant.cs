using UnityEngine;

/// <summary>
/// 持久化单例：该类型单例会被记录为跨场景不被销毁的对象，所以它可以在任何场景中安全地使用。它的生命周期相比非持久化单例更持久，所以通常适合作为程序全局的单例存在。
/// </summary>
public abstract class MonoSingletonPersistant<T> : MonoSingleton<T> where T : MonoBehaviour
{
    /// <inheritdoc/>
    protected override void Awake()
    {
        base.Awake();
        if (ReferenceEquals(this, Instance))
            DontDestroyOnLoad(gameObject);
    }
}

/// <summary>
/// 持久化单例：附带接口访问的变体版本，如果你希望进一步控制单例的访问，可以通过继承该类型以及实现相关接口来提供外部的可访问成员。
/// </summary>
public abstract class MonoSingletonPersistant<T, I> : MonoSingleton<T, I> where T : MonoBehaviour, I
{
    /// <inheritdoc/>
    protected override void Awake()
    {
        base.Awake();
        if (ReferenceEquals(this, Instance))
            DontDestroyOnLoad(gameObject);
    }
}