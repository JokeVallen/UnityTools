#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal static partial class UICreateMenu
{
    [MenuItem("GameObject/UI/Extend/Dropdown")]
    private static void CreateExtendDropdown()
    {
        var template = Resources.Load<GameObject>("ExtendDropdown");
        if (template == null)
        {
            Debug.LogError("The 'ExtendDropdown' prefab doesn't exist.");
            return;
        }

        var rootCanvas = GetOrCreateBaseCanvas();
        var go = MonoBehaviour.Instantiate(template, rootCanvas.transform, false);
        go.name = "ExtendDropdown";
        Selection.activeGameObject = go;
    }

    private static Canvas GetOrCreateBaseCanvas()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (Object.FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        return canvas;
    }
}

#endif