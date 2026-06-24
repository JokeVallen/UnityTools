using System;
using UnityEngine;

internal sealed class ComponentFactory : IComponentFactory
{
    public bool ThrowOnError { get; set; }

    public T Create<T>(GameObject gameObject, Action<T> initialize = null) where T : Component
    {
        if (initialize == null) return Create(gameObject, typeof(T), null) as T;
        else return Create(gameObject, typeof(T), com => initialize((T)com)) as T;
    }

    public Component Create(GameObject gameObject, Type type, Action<Component> initialize = null)
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
            Debug.LogError($"The type '{type}' doesn't inherit from '{typeof(Component)}'.");
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