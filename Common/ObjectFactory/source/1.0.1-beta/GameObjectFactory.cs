using System;
using UnityEngine;

/// <summary>
/// 默认游戏对象工厂
/// </summary>
public sealed class GameObjectFactory : IObjectFactory
{
    /// <inheritdoc/>
    public bool ThrowOnError { get; }

    /// <summary></summary>
    public GameObjectFactory() : this(false) { }

    /// <summary></summary>
    /// <param name="throwOnError">发生错误时是否直接抛出异常</param>
    public GameObjectFactory(bool throwOnError)
    {
        ThrowOnError = throwOnError;
    }

    /// <summary>
    /// 创建游戏对象
    /// </summary>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建的 <see cref="GameObject"/> 实例</returns>
    public GameObject Create(Action<GameObject> initialize = null)
    {
        return CreateGameObjectInternal(null, initialize, null);
    }

    /// <summary>
    /// 创建游戏对象
    /// </summary>
    /// <param name="arg">自定义回调参数</param>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建的 <see cref="GameObject"/> 实例</returns>
    public GameObject Create<TArg>(TArg arg, Action<GameObject, TArg> initialize = null)
    {
        return CreateGameObjectInternal(null, arg, initialize, null);
    }

    /// <summary>
    /// 使用名称创建游戏对象
    /// </summary>
    /// <param name="name">游戏对象名称</param>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建的 <see cref="GameObject"/> 实例</returns>
    public GameObject Create(string name, Action<GameObject> initialize = null)
    {
        return CreateGameObjectInternal(name, initialize, null);
    }

    /// <summary>
    /// 使用名称创建游戏对象
    /// </summary>
    /// <param name="name">游戏对象名称</param>
    /// <param name="arg">自定义回调参数</param>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建的 <see cref="GameObject"/> 实例</returns>
    public GameObject Create<TArg>(string name, TArg arg, Action<GameObject, TArg> initialize = null)
    {
        return CreateGameObjectInternal(name, arg, initialize, null);
    }

    /// <summary>
    /// 使用名称和组件类型创建游戏对象
    /// </summary>
    /// <param name="name">游戏对象名称</param>
    /// <param name="initialize">初始化回调</param>
    /// <param name="components">要添加的组件类型数组</param>
    /// <returns>创建的 <see cref="GameObject"/> 实例</returns>
    public GameObject Create(string name, Action<GameObject> initialize = null, params Type[] components)
    {
        return CreateGameObjectInternal(name, initialize, components);
    }

    /// <summary>
    /// 使用名称和组件类型创建游戏对象
    /// </summary>
    /// <param name="name">游戏对象名称</param>
    /// <param name="arg">自定义回调参数</param>
    /// <param name="initialize">初始化回调</param>
    /// <param name="components">要添加的组件类型数组</param>
    /// <returns>创建的 <see cref="GameObject"/> 实例</returns>
    public GameObject Create<TArg>(string name, TArg arg, Action<GameObject, TArg> initialize = null, params Type[] components)
    {
        return CreateGameObjectInternal(name, arg, initialize, components);
    }

    private GameObject CreateGameObjectInternal(string name, Action<GameObject> initialize, Type[] components)
    {
        if (components != null)
        {
            int count = components.Length;
            for (int i = 0; i < count; i++)
            {
                var t = components[i];
                if (t == null || !typeof(Component).IsAssignableFrom(t))
                {
                    Debug.LogError($"[ObjectFactory] Invalid component type: {(t != null ? t.Name : "null")}");
                    return null;
                }
            }
        }

        GameObject go;
        bool nameValid = !string.IsNullOrEmpty(name);
        bool componentsValid = components != null && components.Length > 0;

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
                Debug.LogError($"[ObjectFactory] Failed to initialize: {ex.Message}");
                return null;
            }
        }

        return go;
    }

    private GameObject CreateGameObjectInternal<TArg>(string name, TArg arg, Action<GameObject, TArg> initialize, Type[] components)
    {
        if (components != null)
        {
            int count = components.Length;
            for (int i = 0; i < count; i++)
            {
                var t = components[i];
                if (t == null || !typeof(Component).IsAssignableFrom(t))
                {
                    Debug.LogError($"[ObjectFactory] Invalid component type: {(t != null ? t.Name : "null")}");
                    return null;
                }
            }
        }

        GameObject go;
        bool nameValid = !string.IsNullOrEmpty(name);
        bool componentsValid = components != null && components.Length > 0;

        if (nameValid && componentsValid) go = new GameObject(name, components);
        else if (nameValid) go = new GameObject(name);
        else go = new GameObject();

        if (initialize != null)
        {
            try
            {
                initialize(go, arg);
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
                Debug.LogError($"[ObjectFactory] Failed to initialize: {ex.Message}");
                return null;
            }
        }

        return go;
    }
}