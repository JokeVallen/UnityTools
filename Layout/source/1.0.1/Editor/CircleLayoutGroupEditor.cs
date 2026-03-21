#if UNITY_EDITOR

using UnityEditor;

namespace UGUI.Layout.Extension
{
    [CustomEditor(typeof(CircleLayoutGroup))]
    internal sealed class CircleLayoutGroupEditor : Editor
    {
        private SerializedProperty m_Script;
        private SerializedProperty radius;
        private SerializedProperty rotation;
        private SerializedProperty clockWise;

        private void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
            radius = serializedObject.FindProperty("radius");
            rotation = serializedObject.FindProperty("rotation");
            clockWise = serializedObject.FindProperty("clockWise");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(m_Script);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(radius);
            EditorGUILayout.PropertyField(rotation);
            EditorGUILayout.PropertyField(clockWise);

            if (EditorGUI.EndChangeCheck() && target is CircleLayoutGroup typed)
            {
                serializedObject.ApplyModifiedProperties();
                typed.RebuildLayout();
            }
            else
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

#endif
