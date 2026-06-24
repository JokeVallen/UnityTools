using System;
using UnityEngine;

/// <summary>
/// 默认组件工厂
/// </summary>
public sealed class ComponentFactory : IObjectFactory
{
    /// <inheritdoc/>
    public bool ThrowOnError { get; }

    /// <summary></summary>
    public ComponentFactory() : this(false) { }

    /// <summary></summary>
    /// <param name="throwOnError">发生错误时是否直接抛出异常</param>
    public ComponentFactory(bool throwOnError)
    {
        ThrowOnError = throwOnError;
    }

    /// <summary>
    /// 创建组件（泛型）
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <param name="gameObject">目标游戏对象</param>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建并初始化成功的组件实例</returns>
    public T Create<T>(GameObject gameObject, Action<T> initialize = null) where T : Component
    {
        if (gameObject == null)
        {
            if (ThrowOnError) throw new ArgumentNullException(nameof(gameObject));
            Debug.LogError($"[ObjectFactory] The parameter '{nameof(gameObject)}' cannot be null.");
            return null;
        }

        var com = gameObject.AddComponent<T>();
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
                Debug.LogError($"[ObjectFactory] Failed to initialize: {ex.Message}");
                return null;
            }
        }

        return com;
    }

    /// <summary>
    /// 创建组件（泛型）
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <typeparam name="TArg">自定义回调参数类型</typeparam>
    /// <param name="gameObject">目标游戏对象</param>
    /// <param name="arg">自定义回调参数</param>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建并初始化成功的组件实例</returns>
    public T Create<T, TArg>(GameObject gameObject, TArg arg, Action<T, TArg> initialize = null) where T : Component
    {
        if (gameObject == null)
        {
            if (ThrowOnError) throw new ArgumentNullException(nameof(gameObject));
            Debug.LogError($"[ObjectFactory] The parameter '{nameof(gameObject)}' cannot be null.");
            return null;
        }

        var com = gameObject.AddComponent<T>();
        if (initialize != null)
        {
            try
            {
                initialize(com, arg);
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
                Debug.LogError($"[ObjectFactory] Failed to initialize: {ex.Message}");
                return null;
            }
        }

        return com;
    }

    /// <summary>
    /// 创建组件（非泛型）
    /// </summary>
    /// <param name="gameObject">目标游戏对象</param>
    /// <param name="type">组件类型</param>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建并初始化成功的组件实例</returns>
    public Component Create(GameObject gameObject, Type type, Action<Component> initialize = null)
    {
        if (gameObject == null)
        {
            if (ThrowOnError) throw new ArgumentNullException(nameof(gameObject));
            Debug.LogError($"[ObjectFactory] The parameter '{nameof(gameObject)}' cannot be null.");
            return null;
        }

        if (type == null)
        {
            if (ThrowOnError) throw new ArgumentNullException(nameof(type));
            Debug.LogError($"[ObjectFactory] The parameter '{nameof(type)}' cannot be null.");
            return null;
        }

        if (!typeof(Component).IsAssignableFrom(type))
        {
            if (ThrowOnError) throw new ArgumentException($"[ObjectFactory] The type '{type}' is not derived from '{typeof(Component)}'.");
            Debug.LogError($"[ObjectFactory] The type '{type}' is not derived from '{typeof(Component)}'.");
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
                Debug.LogError($"[ObjectFactory] Failed to initialize: {ex.Message}");
                return null;
            }
        }

        return com;
    }

    /// <summary>
    /// 创建组件（非泛型）
    /// </summary>
    /// <param name="gameObject">目标游戏对象</param>
    /// <param name="type">组件类型</param>
    /// <param name="arg">自定义回调参数</param>
    /// <param name="initialize">初始化回调</param>
    /// <returns>创建并初始化成功的组件实例</returns>
    public Component Create(GameObject gameObject, Type type, object arg, Action<Component, object> initialize = null)
    {
        if (gameObject == null)
        {
            if (ThrowOnError) throw new ArgumentNullException(nameof(gameObject));
            Debug.LogError($"[ObjectFactory] The parameter '{nameof(gameObject)}' cannot be null.");
            return null;
        }

        if (type == null)
        {
            if (ThrowOnError) throw new ArgumentNullException(nameof(type));
            Debug.LogError($"[ObjectFactory] The parameter '{nameof(type)}' cannot be null.");
            return null;
        }

        if (!typeof(Component).IsAssignableFrom(type))
        {
            if (ThrowOnError) throw new ArgumentException($"[ObjectFactory] The type '{type}' is not derived from '{typeof(Component)}'.");
            Debug.LogError($"[ObjectFactory] The type '{type}' is not derived from '{typeof(Component)}'.");
            return null;
        }

        var com = gameObject.AddComponent(type);
        if (initialize != null)
        {
            try
            {
                initialize(com, arg);
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
                Debug.LogError($"[ObjectFactory] Failed to initialize: {ex.Message}");
                return null;
            }
        }

        return com;
    }
}