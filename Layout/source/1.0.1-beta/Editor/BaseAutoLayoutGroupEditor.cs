#if UNITY_EDITOR

using UnityEditor;

namespace UGUI.Layout.Extension
{
    [CustomEditor(typeof(BaseAutoLayoutGroup), true)]
    public abstract class BaseAutoLayoutGroupEditor : Editor
    {
        protected SerializedProperty script;
        protected SerializedProperty padding;
        protected SerializedProperty childAlignment;

        protected virtual void OnEnable()
        {
            script = serializedObject.FindProperty("m_Script");
            padding = serializedObject.FindProperty("padding");
            childAlignment = serializedObject.FindProperty("childAlignment");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(script);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(padding);
            EditorGUILayout.PropertyField(childAlignment);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                if (target is BaseAutoLayoutGroup typed)
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