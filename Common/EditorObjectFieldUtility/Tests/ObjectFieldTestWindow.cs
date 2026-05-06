#if UNITY_EDITOR

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public class ObjectFieldTestWindow : EditorWindow
{
    [MenuItem("Tools/EasyBinder/ObjectField Test Window")]
    private static void Open()
    {
        GetWindow<ObjectFieldTestWindow>("ObjectField Test").Show();
    }

    // 被测试的对象
    [Header("ReadOnly (Rect)")]
    [SerializeField] private Transform _readOnlyRectValue;

    [Header("ReadOnly (Layout)")]
    [SerializeField] private Transform _readOnlyLayoutValue;

    [Header("NoPicker (Rect)")]
    [SerializeField] private Texture2D _noPickerRectValue;

    [Header("NoPicker (Layout)")]
    [SerializeField] private Texture2D _noPickerLayoutValue;

    [Header("ObjectField (Rect) with Picker")]
    [SerializeField] private Object _objectFieldARectValue;
    private Type _typeA = typeof(Texture2D);

    [Header("ObjectField (Layout) with Picker")]
    [SerializeField] private Object _objectFieldBLayoutValue;
    private Type _typeB = typeof(Texture2D);

    // 回调日志
    private string _lastCallbackLog = "";

    private void Awake()
    {
        var transforms = EditorSceneManager.GetActiveScene().GetRootGameObjects().Select(go => go.transform).ToArray();
        if (transforms.Length > 0)
        {
            _readOnlyRectValue = transforms[UnityEngine.Random.Range(0, transforms.Length)];
            _readOnlyLayoutValue = transforms[UnityEngine.Random.Range(0, transforms.Length)];
        }
    }

    private void OnGUI()
    {
        // ==============
        // ReadOnly (Rect)
        // ==============
        GUILayout.Label("=== ReadOnlyObjectField (Rect) ===", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            Rect r = GUILayoutUtility.GetRect(200, EditorGUIUtility.singleLineHeight);
            EditorObjectFieldUtility.ReadOnlyObjectField(r, new GUIContent("Texture (Rect)"), _readOnlyRectValue);
            GUILayout.Label($"Value: {(_readOnlyRectValue ? _readOnlyRectValue.name : "null")}");
        }

        GUILayout.Space(10);

        // ==============
        // ReadOnly (Layout)
        // ==============
        GUILayout.Label("=== ReadOnlyObjectField (Layout) ===", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorObjectFieldUtility.ReadOnlyObjectFieldLayout(new GUIContent("Texture (Layout)"), _readOnlyLayoutValue);
            GUILayout.Label($"Value: {(_readOnlyLayoutValue ? _readOnlyLayoutValue.name : "null")}");
        }

        GUILayout.Space(20);

        // ==============
        // NoPicker (Rect)
        // ==============
        GUILayout.Label("=== NoPickerObjectField (Rect) ===", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            Rect r = GUILayoutUtility.GetRect(200, EditorGUIUtility.singleLineHeight);
            _noPickerRectValue = EditorObjectFieldUtility.NoPickerObjectField(
                r,
                new GUIContent("Texture (Rect)"),
                _noPickerRectValue,
                allowSceneObject: false) as Texture2D;

            GUILayout.Label($"Value: {(_noPickerRectValue ? _noPickerRectValue.name : "null")}");
        }

        GUILayout.Space(10);

        // ==============
        // NoPicker (Layout)
        // ==============
        GUILayout.Label("=== NoPickerObjectField (Layout) ===", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            _noPickerLayoutValue = EditorObjectFieldUtility.NoPickerObjectFieldLayout(
                new GUIContent("Texture (Layout)"),
                _noPickerLayoutValue,
                allowSceneObject: false) as Texture2D;

            GUILayout.Label($"Value: {(_noPickerLayoutValue ? _noPickerLayoutValue.name : "null")}");
        }

        GUILayout.Space(20);

        // ==============
        // ObjectField (Rect) + Picker
        // ==============
        GUILayout.Label("=== ObjectField (Rect) with Custom Picker ===", EditorStyles.boldLabel);
        GUILayout.Label($"Fixed Target Type: {_typeA.Name}");
        using (new EditorGUILayout.HorizontalScope())
        {
            Rect r2 = GUILayoutUtility.GetRect(200, EditorGUIUtility.singleLineHeight);
            _objectFieldARectValue = EditorObjectFieldUtility.ObjectField(
                r2,
                new GUIContent("Value (Rect)"),
                _objectFieldARectValue,
                _typeA,
                allowSceneObject: false,
                onPickerClick: obj => _lastCallbackLog = $"Picker clicked! Value: {obj?.name ?? "null"} (Type: {obj?.GetType().Name})");

            GUILayout.Label($"Value: {_objectFieldARectValue?.name ?? "null"}");
        }

        GUILayout.Space(10);

        // ==============
        // ObjectField (Layout) + Picker
        // ==============
        GUILayout.Label("=== ObjectField (Layout) with Custom Picker ===", EditorStyles.boldLabel);
        GUILayout.Label($"Fixed Target Type: {_typeB.Name}");
        using (new EditorGUILayout.HorizontalScope())
        {
            _objectFieldBLayoutValue = EditorObjectFieldUtility.ObjectFieldLayout(
                new GUIContent("Value (Layout)"),
                _objectFieldBLayoutValue,
                _typeB,
                allowSceneObject: false,
                onPickerClick: obj => _lastCallbackLog = $"Picker clicked! Value: {obj?.name ?? "null"} (Type: {obj?.GetType().Name})");

            GUILayout.Label($"Value: {_objectFieldBLayoutValue?.name ?? "null"}");
        }

        GUILayout.Space(20);

        // 回调日志
        EditorGUILayout.HelpBox(string.IsNullOrEmpty(_lastCallbackLog) ? "No callback yet" : _lastCallbackLog, MessageType.Info);

        // 清除按钮
        if (GUILayout.Button("Clear All Values"))
        {
            _readOnlyRectValue = null;
            _readOnlyLayoutValue = null;
            _objectFieldARectValue = null;
            _objectFieldBLayoutValue = null;
            _noPickerRectValue = null;
            _noPickerLayoutValue = null;
            _lastCallbackLog = "";
        }

        // 说明
        EditorGUILayout.HelpBox(
            "测试方法：\n" +
            "1. 从 Project 或 Hierarchy 拖拽对象到控件上。\n" +
            "2. 单击已赋值的控件 → 在 Project/Hierarchy 中定位该对象（PingObject）。\n" +
            "3. 单击 ObjectField 右侧小圆点按钮 → 触发自定义回调，下方 Info 显示日志。\n" +
            "4. 注意：这里所有字段都禁用了场景对象（allowSceneObject = false），拖拽场景对象会被拒绝。",
            MessageType.Info);
    }
}

#endif