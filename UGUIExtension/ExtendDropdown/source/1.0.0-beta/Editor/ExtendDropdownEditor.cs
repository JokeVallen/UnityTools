#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(ExtendDropdown), true)]
[CanEditMultipleObjects]
public class ExtendDropdownEditor : SelectableEditor
{
    private SerializedProperty m_Template;

    private SerializedProperty m_CaptionText;

    private SerializedProperty m_CaptionImage;

    private SerializedProperty m_ItemText;

    private SerializedProperty m_ItemImage;

    private SerializedProperty m_OnSelectionChanged;

    private SerializedProperty m_Value;

    private SerializedProperty m_Options;

    private SerializedProperty m_AlphaFadeSpeed;

    private SerializedProperty m_ManualInitialize;

    private SerializedProperty m_ReuseDropdownList;

    private SerializedProperty m_PoolingItems;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_Template = serializedObject.FindProperty("m_Template");
        m_CaptionText = serializedObject.FindProperty("m_CaptionText");
        m_CaptionImage = serializedObject.FindProperty("m_CaptionImage");
        m_ItemText = serializedObject.FindProperty("m_ItemText");
        m_ItemImage = serializedObject.FindProperty("m_ItemImage");
        m_OnSelectionChanged = serializedObject.FindProperty("m_OnValueChanged");
        m_Value = serializedObject.FindProperty("m_Value");
        m_Options = serializedObject.FindProperty("m_Options");
        m_AlphaFadeSpeed = serializedObject.FindProperty("m_AlphaFadeSpeed");
        m_ManualInitialize = serializedObject.FindProperty("m_ManualInitialize");
        m_ReuseDropdownList = serializedObject.FindProperty("m_ReuseDropdownList");
        m_PoolingItems = serializedObject.FindProperty("m_PoolingItems");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_Template);
        EditorGUILayout.PropertyField(m_CaptionText);
        EditorGUILayout.PropertyField(m_CaptionImage);
        EditorGUILayout.PropertyField(m_ItemText);
        EditorGUILayout.PropertyField(m_ItemImage);
        EditorGUILayout.PropertyField(m_Value);
        EditorGUILayout.PropertyField(m_AlphaFadeSpeed);
        EditorGUILayout.PropertyField(m_Options);
        EditorGUILayout.PropertyField(m_OnSelectionChanged);
        EditorGUILayout.PropertyField(m_ManualInitialize);
        EditorGUILayout.PropertyField(m_ReuseDropdownList);
        EditorGUILayout.PropertyField(m_PoolingItems);
        serializedObject.ApplyModifiedProperties();
    }
}

#endif