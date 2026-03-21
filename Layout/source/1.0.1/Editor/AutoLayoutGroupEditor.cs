#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace UGUI.Layout.Extension
{
    [CustomEditor(typeof(AutoLayoutGroup))]
    internal sealed class AutoLayoutGroupEditor : BaseAutoLayoutGroupEditor
    {
        private SerializedProperty curveX;
        private SerializedProperty preWrapModeX;
        private SerializedProperty postWrapModeX;
        private SerializedProperty curveY;
        private SerializedProperty preWrapModeY;
        private SerializedProperty postWrapModeY;
        private SerializedProperty scaleX;
        private SerializedProperty scaleY;
        private SerializedProperty spacingHorizontal;
        private SerializedProperty spacingVertical;
        private SerializedProperty mappingMode;
        private SerializedProperty positionMode;
        private SerializedProperty constrainByGroup;
        private SerializedProperty groupSize;
        private SerializedProperty distributeMode;
        private SerializedProperty reverseArrangement;

        private string SessionKey(string group) => $"AutoLayoutEditor_{target.GetInstanceID()}_{group}";
        private bool foldCurveX;
        private bool foldCurveY;

        protected override void OnEnable()
        {
            base.OnEnable();
            curveX = serializedObject.FindProperty("curveX");
            preWrapModeX = serializedObject.FindProperty("preWrapModeX");
            postWrapModeX = serializedObject.FindProperty("postWrapModeX");
            curveY = serializedObject.FindProperty("curveY");
            preWrapModeY = serializedObject.FindProperty("preWrapModeY");
            postWrapModeY = serializedObject.FindProperty("postWrapModeY");
            scaleX = serializedObject.FindProperty("scaleX");
            scaleY = serializedObject.FindProperty("scaleY");
            spacingHorizontal = serializedObject.FindProperty("spacingHorizontal");
            spacingVertical = serializedObject.FindProperty("spacingVertical");
            mappingMode = serializedObject.FindProperty("mappingMode");
            positionMode = serializedObject.FindProperty("positionMode");
            constrainByGroup = serializedObject.FindProperty("constrainByGroup");
            groupSize = serializedObject.FindProperty("groupSize");
            distributeMode = serializedObject.FindProperty("distributeMode");
            reverseArrangement = serializedObject.FindProperty("reverseArrangement");
            foldCurveX = SessionState.GetBool(SessionKey("CurveX"), false);
            foldCurveY = SessionState.GetBool(SessionKey("CurveY"), false);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(script);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(padding);
            EditorGUILayout.PropertyField(spacingHorizontal);
            EditorGUILayout.PropertyField(spacingVertical);
            EditorGUILayout.PropertyField(reverseArrangement);
            EditorGUILayout.PropertyField(childAlignment);
            EditorGUILayout.PropertyField(positionMode);
            EditorGUILayout.PropertyField(mappingMode);

            var mode = (KeyframeMappingMode)mappingMode.enumValueIndex;
            bool isInterpolated = mode == KeyframeMappingMode.Interpolated;
            bool isProportional = mode == KeyframeMappingMode.Proportional;

            if (isInterpolated)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(constrainByGroup);
                if (constrainByGroup.boolValue)
                {
                    EditorGUILayout.PropertyField(groupSize);
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.FloatField("Cycles", (target as AutoLayoutGroup).Cycles);
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUI.indentLevel--;
            }

            if (isProportional)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(distributeMode);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(4);

            DrawCurveFields();

            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(scaleX);
            EditorGUILayout.PropertyField(scaleY);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                if (target is AutoLayoutGroup typed)
                    typed.RebuildLayout();
            }
            else
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawCurveFields()
        {
            var mode = (KeyframeMappingMode)mappingMode.enumValueIndex;
            bool isProportional = mode == KeyframeMappingMode.Proportional;
            bool isInterpolated = mode == KeyframeMappingMode.Interpolated;

            bool disablePost = isProportional || (isInterpolated && !constrainByGroup.boolValue);

            DrawCurveSection(ref foldCurveX, "X Axis Curve", SessionKey("CurveX"),
                curveX, preWrapModeX, postWrapModeX,
                disablePre: true,
                disablePost: disablePost);

            DrawCurveSection(ref foldCurveY, "Y Axis Curve", SessionKey("CurveY"),
                curveY, preWrapModeY, postWrapModeY,
                disablePre: true,
                disablePost: disablePost);
        }

        private void DrawCurveSection(ref bool fold, string label, string sessionKey,
            SerializedProperty curve, SerializedProperty preWrap, SerializedProperty postWrap,
            bool disablePre, bool disablePost)
        {
            bool next = EditorGUILayout.BeginFoldoutHeaderGroup(fold, label);
            if (next != fold)
            {
                fold = next;
                SessionState.SetBool(sessionKey, fold);
            }

            if (fold)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(curve, GUIContent.none);
                EditorGUI.BeginDisabledGroup(disablePre);
                EditorGUILayout.PropertyField(preWrap);
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(disablePost);
                EditorGUILayout.PropertyField(postWrap);
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}

#endif