#if UNITY_EDITOR

using UnityEditor;

namespace GameAssistant.Core.UI.Layout
{
    [CustomEditor(typeof(AutoLayout))]
    public class AutoLayoutEditor : Editor
    {
        private SerializedProperty m_Script;
        private SerializedProperty m_Padding;
        private SerializedProperty m_Curve;
        private SerializedProperty m_CurveEndMode;
        private SerializedProperty m_XMultiplier;
        private SerializedProperty m_YMultiplier;

        private void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
            m_Padding = serializedObject.FindProperty("m_Padding");
            m_Curve = serializedObject.FindProperty("m_Curve");
            m_CurveEndMode = serializedObject.FindProperty("m_CurveEndMode");
            m_XMultiplier = serializedObject.FindProperty("m_XMultiplier");
            m_YMultiplier = serializedObject.FindProperty("m_YMultiplier");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(m_Script);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(m_Padding);
            EditorGUILayout.PropertyField(m_Curve);
            EditorGUILayout.PropertyField(m_CurveEndMode);
            EditorGUILayout.PropertyField(m_XMultiplier);
            EditorGUILayout.PropertyField(m_YMultiplier);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif