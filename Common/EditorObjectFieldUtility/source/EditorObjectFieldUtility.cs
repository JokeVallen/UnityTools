#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

public static class EditorObjectFieldUtility
{
    #region Public

    /// <summary>
    /// 只读的 <see cref="EditorGUI.ObjectField"/>
    /// </summary>
    public static void ReadOnlyObjectField<T>(Rect rect, GUIContent label, T value) where T : UnityEngine.Object
    => NoPickerObjectFieldInternal(rect, label, value, typeof(T), true, true);

    /// <summary>
    /// 只读的 <see cref="EditorGUI.ObjectField"/>
    /// </summary>
    public static void ReadOnlyObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Type objectType)
    => NoPickerObjectFieldInternal(rect, label, value, objectType, true, true);

    /// <summary>
    /// 无选择器按钮的 <see cref="EditorGUI.ObjectField"/>
    /// </summary>
    public static T NoPickerObjectField<T>(Rect rect, GUIContent label, T value, bool allowSceneObject = true) where T : UnityEngine.Object
    => NoPickerObjectFieldInternal(rect, label, value, typeof(T), allowSceneObject, false) as T;

    /// <summary>
    /// 无选择器按钮的 <see cref="EditorGUI.ObjectField"/>
    /// </summary>
    public static UnityEngine.Object NoPickerObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObject = true)
    => NoPickerObjectFieldInternal(rect, label, value, objectType, allowSceneObject, false);

    /// <summary>
    /// 自定义选择器按钮点击行为的 <see cref="EditorGUI.ObjectField"/>
    /// </summary>
    public static T ObjectField<T>(Rect rect, GUIContent label, T value, bool allowSceneObject = true, Action<T> onPickerClick = null) where T : UnityEngine.Object
    => ObjectFieldInternal(rect, label, value, typeof(T), allowSceneObject, obj => onPickerClick?.Invoke((T)obj)) as T;

    /// <summary>
    /// 自定义选择器按钮点击行为的 <see cref="EditorGUI.ObjectField"/>
    /// </summary>
    public static UnityEngine.Object ObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObject = true, Action<UnityEngine.Object> onPickerClick = null)
    => ObjectFieldInternal(rect, label, value, objectType, allowSceneObject, onPickerClick);

    /// <summary>
    /// 只读的 <see cref="EditorGUILayout.ObjectField"/>
    /// </summary>
    public static void ReadOnlyObjectFieldLayout<T>(GUIContent label, T value, params GUILayoutOption[] options) where T : UnityEngine.Object
    {
        Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, options);
        NoPickerObjectFieldInternal(rect, label, value, typeof(T), true, true);
    }

    /// <summary>
    /// 只读的 <see cref="EditorGUILayout.ObjectField"/>
    /// </summary>
    public static void ReadOnlyObjectFieldLayout(GUIContent label, UnityEngine.Object value, Type objectType, params GUILayoutOption[] options)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, options);
        NoPickerObjectFieldInternal(rect, label, value, objectType, true, true);
    }

    /// <summary>
    /// 无选择器按钮的 <see cref="EditorGUILayout.ObjectField"/>
    /// </summary>
    public static T NoPickerObjectFieldLayout<T>(GUIContent label, T value, bool allowSceneObject = true, params GUILayoutOption[] options) where T : UnityEngine.Object
    {
        Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, options);
        return NoPickerObjectFieldInternal(rect, label, value, typeof(T), allowSceneObject, false) as T;
    }

    /// <summary>
    /// 无选择器按钮的 <see cref="EditorGUILayout.ObjectField"/>
    /// </summary>
    public static UnityEngine.Object NoPickerObjectFieldLayout(GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObject = true, params GUILayoutOption[] options)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, options);
        return NoPickerObjectFieldInternal(rect, label, value, objectType, allowSceneObject, false);
    }

    /// <summary>
    /// 自定义选择器按钮点击行为的 <see cref="EditorGUILayout.ObjectField"/>
    /// </summary>
    public static T ObjectFieldLayout<T>(GUIContent label, T value, bool allowSceneObject = true, Action<T> onPickerClick = null, params GUILayoutOption[] options) where T : UnityEngine.Object
    {
        Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, options);
        return ObjectFieldInternal(rect, label, value, typeof(T), allowSceneObject, obj => onPickerClick?.Invoke((T)obj)) as T;
    }

    /// <summary>
    /// 自定义选择器按钮点击行为的 <see cref="EditorGUILayout.ObjectField"/>
    /// </summary>
    public static UnityEngine.Object ObjectFieldLayout(GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObject = true, Action<UnityEngine.Object> onPickerClick = null, params GUILayoutOption[] options)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, options);
        return ObjectFieldInternal(rect, label, value, objectType, allowSceneObject, onPickerClick);
    }

    #endregion

    #region Internal

    private const float ICON_SIZE = 12f;
    private const float PICKER_WIDTH = 18f;
    private static GUIStyle NoPickerStyle
    {
        get
        {
            if (noPickerStyle == null)
            {
                noPickerStyle = new GUIStyle(EditorStyles.objectField);
                noPickerStyle.border.right = 4;
                noPickerStyle.padding.right = 3;
            }
            return noPickerStyle;
        }
    }
    private static GUIStyle noPickerStyle;
    private static GUIStyle PickerStyle
    {
        get
        {
            if (pickerStyle == null)
                pickerStyle = "objectFieldButton";
            return pickerStyle;
        }
    }
    private static GUIStyle pickerStyle;

    private static UnityEngine.Object NoPickerObjectFieldInternal(Rect rect, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObject, bool readOnly)
    {
        if (objectType == null) throw new ArgumentNullException(nameof(objectType));
        if (!typeof(UnityEngine.Object).IsAssignableFrom(objectType))
            throw new ArgumentException($"{objectType} is not a '{nameof(UnityEngine.Object)}' type", nameof(objectType));

        int id = GUIUtility.GetControlID(FocusType.Passive, rect);
        if (label != null) rect = EditorGUI.PrefixLabel(rect, id, label);

        if (value != null && !objectType.IsAssignableFrom(value.GetType()))
            value = null;

        Event e = Event.current;
        if (e != null)
        {
            if (value != null && e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
            {
                EditorGUIUtility.PingObject(value);
                e.Use();
            }

            switch (e.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!readOnly && rect.Contains(e.mousePosition))
                    {
                        if (DragAndDrop.objectReferences.Length == 0) break;
                        UnityEngine.Object dragged = DragAndDrop.objectReferences[0];
                        if (dragged == null) break;
                        if (!objectType.IsAssignableFrom(dragged.GetType())) break;
                        if (!allowSceneObject && !EditorUtility.IsPersistent(dragged)) break;

                        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

                        if (e.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            if (value != dragged) GUI.changed = true;
                            value = dragged;
                        }

                        e.Use();
                    }
                    break;
                case EventType.Repaint:
                    GUIContent content = EditorGUIUtility.ObjectContent(value, objectType);
                    EditorGUIUtility.SetIconSize(new Vector2(ICON_SIZE, ICON_SIZE));
                    NoPickerStyle.Draw(rect, content, id, DragAndDrop.activeControlID == id, rect.Contains(e.mousePosition));
                    break;
            }
        }

        return value;
    }

    private static UnityEngine.Object ObjectFieldInternal(Rect rect, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObject, Action<UnityEngine.Object> onPickerClick)
    {
        if (objectType == null) throw new ArgumentNullException(nameof(objectType));
        if (!typeof(UnityEngine.Object).IsAssignableFrom(objectType))
            throw new ArgumentException($"{objectType} is not a UnityEngine.Object type.", nameof(objectType));

        int id = GUIUtility.GetControlID(FocusType.Passive, rect);
        if (label != null) rect = EditorGUI.PrefixLabel(rect, id, label);

        if (value != null && !objectType.IsAssignableFrom(value.GetType()))
            value = null;

        Event e = Event.current;
        if (e != null)
        {
            switch (e.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (rect.Contains(e.mousePosition))
                    {
                        if (DragAndDrop.objectReferences.Length == 0) break;
                        UnityEngine.Object dragged = DragAndDrop.objectReferences[0];
                        if (dragged == null) break;
                        if (!objectType.IsAssignableFrom(dragged.GetType())) break;
                        if (!allowSceneObject && !EditorUtility.IsPersistent(dragged)) break;

                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        if (e.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            if (value != dragged) GUI.changed = true;
                            value = dragged;
                        }

                        e.Use();
                    }
                    break;
                case EventType.Repaint:
                    GUIContent content = EditorGUIUtility.ObjectContent(value, objectType);
                    EditorGUIUtility.SetIconSize(new Vector2(ICON_SIZE, ICON_SIZE));
                    EditorStyles.objectField.Draw(rect, content, id, DragAndDrop.activeControlID == id, rect.Contains(e.mousePosition));
                    break;
            }

            Rect buttonRect = new Rect(rect.xMax - PICKER_WIDTH - 1, rect.y + 1, PICKER_WIDTH, rect.height - 2);
            if (GUI.Button(buttonRect, GUIContent.none, PickerStyle))
                onPickerClick?.Invoke(value);

            if (value != null && e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
            {
                EditorGUIUtility.PingObject(value);
                e.Use();
            }
        }

        return value;
    }

    #endregion
}

#endif