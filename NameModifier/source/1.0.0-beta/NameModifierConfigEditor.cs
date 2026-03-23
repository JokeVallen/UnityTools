#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;

namespace EditorTools.NameModifier
{
    [CustomEditor(typeof(NameModifierConfig))]
    internal sealed class NameModifierConfigEditor : Editor
    {
        private SerializedProperty m_HandlerPath;
        private SerializedProperty m_UndoSystemType;
        private SerializedProperty m_UndoCapacity;
        private SerializedProperty m_MaxTrackedObjects;
        private SerializedProperty m_DefaultGroupCapacity;
        private SerializedProperty m_DefaultGroupNameTemplate;
        private SerializedProperty m_AutoReset;
        private SerializedProperty m_AutoClearInvalidCache;
        private SerializedProperty m_AutoClearCache;
        private SerializedProperty m_LogEnabled;

        private void OnEnable()
        {
            if (target == null) return;

            m_HandlerPath = serializedObject.FindProperty("m_HandlerPath");
            m_UndoSystemType = serializedObject.FindProperty("m_UndoSystemType");
            m_UndoCapacity = serializedObject.FindProperty("m_UndoCapacity");
            m_MaxTrackedObjects = serializedObject.FindProperty("m_MaxTrackedObjects");
            m_DefaultGroupCapacity = serializedObject.FindProperty("m_DefaultGroupCapacity");
            m_DefaultGroupNameTemplate = serializedObject.FindProperty("m_DefaultGroupNameTemplate");
            m_AutoReset = serializedObject.FindProperty("m_AutoReset");
            m_AutoClearInvalidCache = serializedObject.FindProperty("m_AutoClearInvalidCache");
            m_AutoClearCache = serializedObject.FindProperty("m_AutoClearCache");
            m_LogEnabled = serializedObject.FindProperty("m_LogEnabled");

            NameModifierConfig config = (NameModifierConfig)target;
            string assetPath = AssetDatabase.GetAssetPath(config);
            EditorPrefs.SetString(NameModifierConfig.AssetPathQualifiedKey, assetPath);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            NameModifierConfig config = (NameModifierConfig)target;

            string assetPath = EditorPrefs.HasKey(NameModifierConfig.AssetPathQualifiedKey)
                ? EditorPrefs.GetString(NameModifierConfig.AssetPathQualifiedKey)
                : string.Empty;
            if (!string.IsNullOrWhiteSpace(assetPath)) EditorGUILayout.SelectableLabel(assetPath);

            EditorGUI.BeginDisabledGroup(!config.IsPersistent);

            EditorGUILayout.PropertyField(m_UndoSystemType, new GUIContent("撤销系统类型",
                "SessionState：跨会话保留历史\nMemory：性能最好，关闭丢失\nNone：禁用撤销"));
            m_UndoCapacity.intValue = EditorGUILayout.IntField(
                new GUIContent("每对象历史条数", "每个对象最多保留的历史记录数，0 表示不限制"),
                m_UndoCapacity.intValue);
            m_UndoCapacity.intValue = Mathf.Max(0, m_UndoCapacity.intValue);
            m_MaxTrackedObjects.intValue = EditorGUILayout.IntField(
                new GUIContent("最大追踪对象数", "超出后新对象改名不记录历史，0 表示使用默认值 10000"),
                m_MaxTrackedObjects.intValue);
            m_MaxTrackedObjects.intValue = Mathf.Max(0, m_MaxTrackedObjects.intValue);
            m_DefaultGroupCapacity.intValue = EditorGUILayout.IntField(
                new GUIContent("分组最大步数", "达到步数上限后自动结束分组，0 表示不限制"),
                m_DefaultGroupCapacity.intValue);
            m_DefaultGroupCapacity.intValue = Mathf.Max(0, m_DefaultGroupCapacity.intValue);
            EditorGUILayout.PropertyField(m_DefaultGroupNameTemplate,
                new GUIContent("分组名模板", "支持 {Date} {Time} {DateTime}"));

            if (GroupNameFormatter.ContainsTokens(m_DefaultGroupNameTemplate.stringValue))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("预览", GroupNameFormatter.Format(m_DefaultGroupNameTemplate.stringValue));
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.PropertyField(m_AutoReset);
            EditorGUILayout.PropertyField(m_AutoClearInvalidCache);
            EditorGUILayout.PropertyField(m_AutoClearCache);
            EditorGUILayout.PropertyField(m_LogEnabled);

            if (!string.IsNullOrWhiteSpace(m_HandlerPath.stringValue))
                EditorGUILayout.SelectableLabel(m_HandlerPath.stringValue);

            if (GUILayout.Button("选择处理器目录"))
            {
                string handlerPath = EditorUtility.OpenFolderPanel("选择处理器目录", Application.dataPath, string.Empty);
                handlerPath = NameModifierUtility.GetAssetPath(handlerPath);
                if (!string.Equals(handlerPath, m_HandlerPath.stringValue, System.StringComparison.Ordinal))
                    m_HandlerPath.stringValue = handlerPath;
            }

            if (GUILayout.Button("导出为JSON文件"))
            {
                string directory = EditorUtility.OpenFolderPanel("选择导出目录", Application.dataPath, string.Empty);
                if (!string.IsNullOrWhiteSpace(directory))
                    config.ExportToJSON(Path.Combine(directory, $"{nameof(NameModifierConfig)}.json"));
            }

            if (GUILayout.Button("从JSON文件加载"))
            {
                string path = EditorUtility.OpenFilePanel("选择JSON文件路径", Application.dataPath, "json");
                if (!string.IsNullOrWhiteSpace(path))
                    config.LoadFromJSON(path);
            }

            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif