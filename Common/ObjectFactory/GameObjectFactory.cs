using System;
using UnityEngine;

/// <summary>
/// 游戏对象工厂
/// </summary>
/// <remarks>
/// <para>默认的游戏对象工厂实现，负责创建新的 <see cref="GameObject"/> 并可选地在创建后立即添加组件和执行初始化回调。</para>
/// <para>初始化失败时会根据 <see cref="ThrowOnError"/> 决定是否抛出异常，并在返回前销毁已创建的 <see cref="GameObject"/>。</para>
/// <para>示例：</para>
/// <code>
/// var factory = ObjectFactory.GetGameObjectFactory();
/// GameObject player = factory.Create("Player", go => go.tag = "Player", typeof(Rigidbody), typeof(BoxCollider));
/// </code>
/// </remarks>
public sealed class GameObjectFactory : IGameObjectFactory
{
    /// <summary>
    /// 是否抛出错误
    /// </summary>
    /// <remarks>
    /// <para>获取或设置一个值，指示初始化回调发生异常时是否直接抛出。默认为 <see langword="false"/>，会记录错误并清理已创建的对象。</para>
    /// </remarks>
    public bool ThrowOnError { get; set; }

    /// <summary>
    /// 创建游戏对象
    /// </summary>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建并初始化成功的 <see cref="GameObject"/>；若初始化失败且未抛出异常，则返回 <see langword="null"/>。</returns>
    /// <remarks>
    /// <para>创建一个未命名的空 <see cref="GameObject"/>。</para>
    /// <para><paramref name="initialize"/> 是可选的初始化委托，接收新创建的 <see cref="GameObject"/>。</para>
    /// </remarks>
    public GameObject Create(Action<GameObject> initialize = null)
        => CreateGameObjectInternal(null, initialize, null);

    /// <summary>
    /// 使用名称创建游戏对象
    /// </summary>
    /// <param name="name">游戏对象名称</param>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建并初始化成功的 <see cref="GameObject"/></returns>
    /// <remarks>
    /// <para>创建具有指定名称的空 <see cref="GameObject"/>。</para>
    /// </remarks>
    public GameObject Create(string name, Action<GameObject> initialize = null)
        => CreateGameObjectInternal(name, initialize, null);

    /// <summary>
    /// 使用名称和组件类型创建游戏对象
    /// </summary>
    /// <param name="name">游戏对象名称</param>
    /// <param name="initialize">初始化回调</param>
    /// <param name="components">要添加的组件类型数组</param>
    /// <returns>创建并初始化成功的 <see cref="GameObject"/></returns>
    /// <remarks>
    /// <para>创建具有指定名称的 <see cref="GameObject"/>，并立即添加 <paramref name="components"/> 中指定的组件。</para>
    /// <para>如果 <paramref name="components"/> 中包含 <see langword="null"/> 或非 <see cref="Component"/> 派生类型，会记录错误并返回 <see langword="null"/>。</para>
    /// </remarks>
    public GameObject Create(string name, Action<GameObject> initialize = null, params Type[] components)
        => CreateGameObjectInternal(name, initialize, components);

    private GameObject CreateGameObjectInternal(string name, Action<GameObject> initialize, Type[] components)
    {
        if (components != null)
        {
            foreach (var t in components)
            {
                if (t == null || !typeof(Component).IsAssignableFrom(t))
                {
                    Debug.LogError($"Invalid component type: {t?.Name ?? "null"}");
                    return null;
                }
            }
        }

        GameObject go;
        bool nameValid = !string.IsNullOrEmpty(name);
        bool componentsValid = components?.Length > 0;

        if (nameValid && componentsValid) go = new GameObject(name, components);
        else if (nameValid) go = new GameObject(name);
        else go = new GameObject();

        if (initialize != null)
        {
            try
            {
                initialize(go);
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                UnityEditor.Undo.DestroyObjectImmediate(go);
#else
                if (Application.isPlaying) UnityEngine.Object.Destroy(go);
                else UnityEngine.Object.DestroyImmediate(go);
#endif
                if (ThrowOnError) throw;
                Debug.LogError($"Failed to initialize: {ex.Message}");
                return null;
            }
        }

        return go;
    }
}