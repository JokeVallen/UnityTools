#if UNITY_EDITOR

using UnityEditor;

namespace GameAssistant.Core.UI.Layout
{
    [CustomEditor(typeof(CircleLayout))]
    public class CircleLayoutEditor : Editor
    {
        private SerializedProperty m_Script;
        private SerializedProperty m_Radius;
        private SerializedProperty m_Rotation;
        private SerializedProperty m_ClockWise;

        private void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
            m_Radius = serializedObject.FindProperty("m_Radius");
            m_Rotation = serializedObject.FindProperty("m_Rotation");
            m_ClockWise = serializedObject.FindProperty("m_ClockWise");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(m_Script);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(m_Radius);
            EditorGUILayout.PropertyField(m_Rotation);
            EditorGUILayout.PropertyField(m_ClockWise);

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif