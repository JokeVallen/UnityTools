using System;
using UnityEngine;

/// <summary>
/// 泛型组件工厂
/// </summary>
/// <typeparam name="T">要创建的组件类型，必须派生自 <see cref="Component"/></typeparam>
/// <remarks>
/// <para>默认的泛型组件工厂实现，可在指定的 <see cref="GameObject"/> 上添加 <typeparamref name="T"/> 组件并执行可选初始化。</para>
/// <para>当初始化回调抛出异常时，会根据 <see cref="ThrowOnError"/> 决定是否重新抛出；无论何种情况，尚未完全初始化的组件都会被销毁，避免残留。</para>
/// <para>示例：</para>
/// <code>
/// var factory = ObjectFactory.GetComponentFactory&lt;Rigidbody&gt;();
/// Rigidbody rb = factory.Create(gameObject, r => r.mass = 2f);
/// </code>
/// </remarks>
public class ComponentFactory<T> : IComponentFactory<T> where T : Component
{
    /// <summary>
    /// 是否抛出错误
    /// </summary>
    /// <remarks>
    /// <para>设置或获取一个值，指示初始化回调发生异常时是否直接抛出。默认为 <see langword="false"/>，表示仅记录错误并清理组件。</para>
    /// </remarks>
    public bool ThrowOnError { get; set; }

    /// <summary>
    /// 创建组件
    /// </summary>
    /// <param name="gameObject">游戏对象</param>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建并初始化成功的组件实例；若 <paramref name="gameObject"/> 为 <see langword="null"/> 或初始化失败（且未抛出异常），则返回 <see langword="null"/>。</returns>
    /// <remarks>
    /// <para><paramref name="gameObject"/> 是要添加组件的目标对象，不能为 <see langword="null"/>。</para>
    /// <para><paramref name="initialize"/> 是可选的初始化委托，接收刚添加的组件实例，可用于设置初始状态。</para>
    /// <para>异常处理：当 <paramref name="initialize"/> 抛出异常时，会先销毁已添加的组件，然后检查 <see cref="ThrowOnError"/>；若该属性为 <see langword="true"/> 则重新抛出异常，否则仅记录错误并返回 <see langword="null"/>。</para>
    /// </remarks>
    public virtual T Create(GameObject gameObject, Action<T> initialize = null)
    {
        if (gameObject == null)
        {
            Debug.LogError($"The parameter '{nameof(gameObject)}' cannot be null.");
            return null;
        }

        T com = gameObject.AddComponent<T>();

        if (initialize != null)
        {
            try
            {
                initialize(com);
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                UnityEditor.Undo.DestroyObjectImmediate(com);
#else
                if (Application.isPlaying) UnityEngine.Object.Destroy(com);
                else UnityEngine.Object.DestroyImmediate(com);
#endif
                if (ThrowOnError) throw;
                Debug.LogError($"Failed to initialize: {ex.Message}");
                return null;
            }
        }

        return com;
    }
}

/// <summary>
/// 组件工厂
/// </summary>
/// <remarks>
/// <para>默认的非泛型组件工厂实现，可动态地通过 <see cref="Type"/> 在指定的 <see cref="GameObject"/> 上添加组件并执行可选初始化。</para>
/// <para>支持运行时组件创建，并包含与泛型工厂相同的错误处理机制：初始化失败时会先销毁组件，再根据 <see cref="ThrowOnError"/> 决定是否抛出异常。</para>
/// <para>示例：</para>
/// <code>
/// var factory = ObjectFactory.GetComponentFactory();
/// Component comp = factory.Create(gameObject, typeof(Rigidbody), c => ((Rigidbody)c).mass = 2f);
/// </code>
/// </remarks>
public class ComponentFactory : IComponentFactory
{
    /// <summary>
    /// 是否抛出错误
    /// </summary>
    /// <remarks>
    /// <para>获取或设置一个值，指示初始化回调发生异常时是否直接抛出。默认为 <see langword="false"/>，表示仅记录错误并清理组件。</para>
    /// </remarks>
    public bool ThrowOnError { get; set; }

    /// <summary>
    /// 创建组件
    /// </summary>
    /// <param name="gameObject">游戏对象</param>
    /// <param name="type">组件类型</param>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建并初始化成功的组件实例；若参数无效或初始化失败（且未抛出异常），则返回 <see langword="null"/>。</returns>
    /// <remarks>
    /// <para><paramref name="gameObject"/> 是要添加组件的目标对象，不能为 <see langword="null"/>。</para>
    /// <para><paramref name="type"/> 必须是派生自 <see cref="Component"/> 的类型，不能为 <see langword="null"/>。</para>
    /// <para><paramref name="initialize"/> 是可选的初始化委托，接收刚添加的组件实例（类型为 <see cref="Component"/>），可在回调中进行类型转换和设置。</para>
    /// <para>异常处理：当 <paramref name="initialize"/> 抛出异常时，会先销毁已添加的组件，然后检查 <see cref="ThrowOnError"/>；若该属性为 <see langword="true"/> 则重新抛出异常，否则仅记录错误并返回 <see langword="null"/>。</para>
    /// </remarks>
    public virtual Component Create(GameObject gameObject, Type type, Action<Component> initialize = null)
    {
        if (gameObject == null)
        {
            Debug.LogError($"The parameter '{nameof(gameObject)}' cannot be null.");
            return null;
        }

        if (type == null)
        {
            Debug.LogError($"The parameter '{nameof(type)}' cannot be null.");
            return null;
        }

        if (!typeof(Component).IsAssignableFrom(type))
        {
            Debug.LogError($"The type '{type}' didn't inherit from '{typeof(Component)}'.");
            return null;
        }

        var com = gameObject.AddComponent(type);
        if (initialize != null)
        {
            try
            {
                initialize(com);
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                UnityEditor.Undo.DestroyObjectImmediate(com);
#else
                if (Application.isPlaying) UnityEngine.Object.Destroy(com);
                else UnityEngine.Object.DestroyImmediate(com);
#endif
                if (ThrowOnError) throw;
                Debug.LogError($"Failed to initialize: {ex.Message}");
                return null;
            }
        }

        return com;
    }
}