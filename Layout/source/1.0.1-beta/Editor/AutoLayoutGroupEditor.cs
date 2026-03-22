#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace UGUI.Layout.Extension
{
    [CustomEditor(typeof(AutoLayoutGroup))]
    internal sealed class AutoLayoutGroupEditor : BaseAutoLayoutGroupEditor
    {
        // X 轴
        private SerializedProperty curveX;
        private SerializedProperty preWrapModeX;
        private SerializedProperty postWrapModeX;
        private SerializedProperty mappingModeX;
        private SerializedProperty positionModeX;
        private SerializedProperty constrainByGroupX;
        private SerializedProperty groupSizeX;
        private SerializedProperty distributeModeX;
        private SerializedProperty scaleX;

        // Y 轴
        private SerializedProperty curveY;
        private SerializedProperty preWrapModeY;
        private SerializedProperty postWrapModeY;
        private SerializedProperty mappingModeY;
        private SerializedProperty positionModeY;
        private SerializedProperty constrainByGroupY;
        private SerializedProperty groupSizeY;
        private SerializedProperty distributeModeY;
        private SerializedProperty scaleY;

        // 通用
        private SerializedProperty spacingHorizontal;
        private SerializedProperty spacingVertical;
        private SerializedProperty reverseArrangement;

        private string SessionKey(string id) => $"AutoLayoutEditor_{target.GetInstanceID()}_{id}";
        private bool foldX;
        private bool foldY;

        protected override void OnEnable()
        {
            base.OnEnable();

            curveX = serializedObject.FindProperty("curveX");
            preWrapModeX = serializedObject.FindProperty("preWrapModeX");
            postWrapModeX = serializedObject.FindProperty("postWrapModeX");
            mappingModeX = serializedObject.FindProperty("mappingModeX");
            positionModeX = serializedObject.FindProperty("positionModeX");
            constrainByGroupX = serializedObject.FindProperty("constrainByGroupX");
            groupSizeX = serializedObject.FindProperty("groupSizeX");
            distributeModeX = serializedObject.FindProperty("distributeModeX");
            scaleX = serializedObject.FindProperty("scaleX");

            curveY = serializedObject.FindProperty("curveY");
            preWrapModeY = serializedObject.FindProperty("preWrapModeY");
            postWrapModeY = serializedObject.FindProperty("postWrapModeY");
            mappingModeY = serializedObject.FindProperty("mappingModeY");
            positionModeY = serializedObject.FindProperty("positionModeY");
            constrainByGroupY = serializedObject.FindProperty("constrainByGroupY");
            groupSizeY = serializedObject.FindProperty("groupSizeY");
            distributeModeY = serializedObject.FindProperty("distributeModeY");
            scaleY = serializedObject.FindProperty("scaleY");

            spacingHorizontal = serializedObject.FindProperty("spacingHorizontal");
            spacingVertical = serializedObject.FindProperty("spacingVertical");
            reverseArrangement = serializedObject.FindProperty("reverseArrangement");

            foldX = SessionState.GetBool(SessionKey("X"), true);
            foldY = SessionState.GetBool(SessionKey("Y"), true);
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

            DrawAxisGroup(
                ref foldX, "X Axis", SessionKey("X"),
                curveX, preWrapModeX, postWrapModeX,
                mappingModeX, positionModeX,
                constrainByGroupX, groupSizeX, distributeModeX,
                scaleX,
                cyclesGetter: () => (target as AutoLayoutGroup).CyclesX);

            DrawAxisGroup(
                ref foldY, "Y Axis", SessionKey("Y"),
                curveY, preWrapModeY, postWrapModeY,
                mappingModeY, positionModeY,
                constrainByGroupY, groupSizeY, distributeModeY,
                scaleY,
                cyclesGetter: () => (target as AutoLayoutGroup).CyclesY);

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

        private void DrawAxisGroup(
            ref bool fold, string label, string sessionKey,
            SerializedProperty curve,
            SerializedProperty preWrap, SerializedProperty postWrap,
            SerializedProperty mappingMode, SerializedProperty positionMode,
            SerializedProperty constrainByGroup, SerializedProperty groupSize,
            SerializedProperty distributeMode,
            SerializedProperty scale,
            System.Func<float> cyclesGetter)
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

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(preWrap);
                EditorGUI.EndDisabledGroup();

                var mode = (KeyframeMappingMode)mappingMode.enumValueIndex;
                EditorGUI.BeginDisabledGroup(mode == KeyframeMappingMode.Proportional);
                EditorGUILayout.PropertyField(postWrap);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.PropertyField(mappingMode);
                EditorGUILayout.PropertyField(positionMode);

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
                        EditorGUILayout.FloatField("Cycles", cyclesGetter());
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

                EditorGUILayout.PropertyField(scale);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}

#endif