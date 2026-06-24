using System;
using UnityEngine;

internal sealed class GameObjectFactory : IGameObjectFactory
{
    public bool ThrowOnError { get; set; }

    public GameObject Create(Action<GameObject> initialize = null)
    => CreateGameObjectInternal(null, initialize, null);

    public GameObject Create(string name, Action<GameObject> initialize = null)
    => CreateGameObjectInternal(name, initialize, null);

    public GameObject Create(string name, Action<GameObject> initialize = null, params Type[] components)
    => CreateGameObjectInternal(name, initialize, components);

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