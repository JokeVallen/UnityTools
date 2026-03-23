#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace EditorTools.NameModifier
{
    internal sealed class NameModifier : EditorWindow
    {
        private static NameModifier m_Window;
        private NameModifierHandler[] m_Handlers;
        private string[] m_Options;
        private int m_SelectedHandlerIndex, m_LastSelectedHandlerIndex;
        private bool m_HandlerDirty, m_ShowConfig;
        private string m_HandlerPath;
        private int m_LastUndoCapacity;
        private Vector2 m_ScrollPos;
        private IUndoSystem m_UndoSystem;

        private bool m_ShowGroupPanel;
        private string m_PendingGroupName;
        private int m_PendingGroupCapacity;

        private enum PendingOperation { None, Undo, Restore }
        private PendingOperation m_PendingOperation;

        [MenuItem("EditorTools/NameModifier")]
        private static void ShowWindow()
        {
            m_Window = GetWindow<NameModifier>();
            m_Window.Show();
            m_Window.titleContent = new GUIContent("名称修改器");
        }

        private void OnEnable()
        {
            m_UndoSystem = CreateUndoSystem();
            m_LastUndoCapacity = NameModifierConfig.Instance.UndoCapacity;
            ResetPendingGroupToDefaults();
            LoadHandlersAndOptions();
        }

        private void OnDestroy()
        {
            bool autoReset = NameModifierConfig.Instance.AutoReset;
            bool autoClearCache = NameModifierConfig.Instance.AutoClearCache;
            for (int i = 0, count = m_Handlers == null ? 0 : m_Handlers.Length; i < count; i++)
            {
                NameModifierHandler handler = m_Handlers[i];
                if (autoReset) handler.Reset();
                if (autoClearCache) handler.ClearCache();
            }
        }

        private void OnGUI()
        {
            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

            if (!NameModifierConfig.Instance.IsPersistent)
                EditorGUILayout.HelpBox(NameModifierConfig.NO_ASSET_TIPS, MessageType.Warning);

            if (!m_ShowConfig)
            {
                if (GUILayout.Button("配置")) m_ShowConfig = true;
            }
            else
            {
                if (GUILayout.Button("返回"))
                {
                    m_ShowConfig = false;
                    EditorGUILayout.EndScrollView();
                    return;
                }
                NameModifierConfig.Instance.DrawDetailGUI();
                EditorGUILayout.EndScrollView();
                return;
            }

            if (NameModifierConfig.Instance.IsPersistent)
            {
                int currentCapacity = NameModifierConfig.Instance.UndoCapacity;
                if (currentCapacity != m_LastUndoCapacity)
                {
                    m_UndoSystem.SetCapacity(currentCapacity);
                    m_LastUndoCapacity = currentCapacity;
                }

                if (!string.Equals(m_HandlerPath, NameModifierConfig.Instance.HandlerPath, StringComparison.Ordinal))
                    LoadHandlersAndOptions();

                DrawGroupPanel();

                if (m_Options?.Length > 0)
                {
                    m_SelectedHandlerIndex = EditorGUILayout.Popup("处理器：", m_SelectedHandlerIndex, m_Options);
                    if (m_SelectedHandlerIndex != m_LastSelectedHandlerIndex)
                    {
                        if (NameModifierConfig.Instance.AutoClearInvalidCache)
                        {
                            if (m_LastSelectedHandlerIndex >= 0 && m_LastSelectedHandlerIndex < m_Handlers.Length)
                                m_Handlers[m_LastSelectedHandlerIndex].ClearInvalidCache();
                        }
                        m_LastSelectedHandlerIndex = m_SelectedHandlerIndex;
                        m_HandlerDirty = true;
                    }

                    NameModifierHandler handler = m_Handlers[m_SelectedHandlerIndex];
                    if (m_HandlerDirty) handler.OnSelected();
                    handler.DrawGUI();

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("修改")) Modify();

                    EditorGUI.BeginDisabledGroup(!m_UndoSystem.IsGroupActive);
                    if (GUILayout.Button("撤销")) Undo();
                    if (GUILayout.Button("恢复")) Restore();
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    if (!NameModifierConfig.Instance.AutoReset)
                        if (GUILayout.Button("重置")) handler.Reset();
                    if (!NameModifierConfig.Instance.AutoClearInvalidCache)
                        if (GUILayout.Button("清除无效缓存"))
                        {
                            handler.ClearInvalidCache();
                            NameModifierConfig.Logger.Log("已清除无效历史缓存。");
                        }
                    if (!NameModifierConfig.Instance.AutoClearCache)
                        if (GUILayout.Button("清除缓存"))
                        {
                            handler.ClearCache();
                            NameModifierConfig.Logger.Log("已清除全部历史缓存。");
                        }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.HelpBox("1.可批量修改所选择的对象的名称，包括场景中的游戏对象或项目中的资产对象。 2.此版本暂不支持预制体模式下的修改，请将预制体拉入场景中修改后应用于预制体资产。 3.可自由扩展不同的名称修改处理器，可参考目前预设的三种处理器。", MessageType.Warning);
                    EditorGUILayout.HelpBox(handler.Tip, MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox(NameModifierConfig.NO_HANDLER_TIPS, MessageType.Warning);
                }

                if (GUILayout.Button("重新加载处理器")) LoadHandlersAndOptions();

                if (m_HandlerDirty)
                {
                    m_HandlerDirty = false;
                    Repaint();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawGroupPanel()
        {
            m_ShowGroupPanel = EditorGUILayout.Foldout(m_ShowGroupPanel, "分组管理", true);
            if (!m_ShowGroupPanel) return;

            EditorGUI.indentLevel++;

            if (!m_UndoSystem.IsGroupActive)
            {
                m_PendingGroupName = EditorGUILayout.TextField("组名模板", m_PendingGroupName);

                if (GroupNameFormatter.ContainsTokens(m_PendingGroupName))
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.TextField("预览", GroupNameFormatter.Format(m_PendingGroupName));
                    EditorGUI.EndDisabledGroup();
                }

                m_PendingGroupCapacity = EditorGUILayout.IntSlider("组容量", m_PendingGroupCapacity, 0, 100);
                EditorGUILayout.HelpBox("组容量：每组最多记录的操作步数，达到上限后自动结束分组。0 表示不限制。", MessageType.None);

                if (GUILayout.Button("激活分组"))
                {
                    string resolvedName = GroupNameFormatter.Format(
                        string.IsNullOrWhiteSpace(m_PendingGroupName)
                            ? NameModifierConfig.Instance.DefaultGroupNameTemplate
                            : m_PendingGroupName);
                    m_UndoSystem.ActivateGroup(resolvedName, m_PendingGroupCapacity);
                    NameModifierConfig.Logger.Log($"分组已激活：'{resolvedName}'，容量：{m_PendingGroupCapacity}。");
                }
            }
            else
            {
                EditorGUILayout.LabelField("当前分组", m_UndoSystem.ActiveGroupName);
                if (GUILayout.Button("结束分组"))
                {
                    string groupName = m_UndoSystem.ActiveGroupName;
                    m_UndoSystem.DeactivateGroup();
                    ResetPendingGroupToDefaults();
                    NameModifierConfig.Logger.Log($"分组已结束：'{groupName}'。");
                }
            }

            EditorGUI.indentLevel--;
        }

        private void Modify()
        {
            if (!ValidateHandler(out NameModifierHandler handler)) return;

            if (!m_UndoSystem.IsGroupActive)
            {
                string resolvedName = GroupNameFormatter.Format(
                    string.IsNullOrWhiteSpace(m_PendingGroupName)
                        ? NameModifierConfig.Instance.DefaultGroupNameTemplate
                        : m_PendingGroupName);
                m_UndoSystem.ActivateGroup(resolvedName, m_PendingGroupCapacity);
            }

            ExecuteBatchOperation("重命名", (obj, i, count) => handler.Modify(obj, i, count));
        }

        private void Undo()
        {
            m_PendingOperation = PendingOperation.Undo;
            EditorApplication.delayCall += ExecutePendingOperation;
        }

        private void Restore()
        {
            m_PendingOperation = PendingOperation.Restore;
            EditorApplication.delayCall += ExecutePendingOperation;
        }

        private void ExecutePendingOperation()
        {
            EditorApplication.delayCall -= ExecutePendingOperation;

            if (m_PendingOperation == PendingOperation.Undo)
            {
                IReadOnlyList<RenameTarget> targets = m_UndoSystem.Undo();
                if (targets != null && targets.Count > 0)
                {
                    ApplyRenameTargets(targets, "撤销命名");
                    NameModifierConfig.Logger.Log($"撤销完成，共处理 {targets.Count} 个对象。");
                }
                else
                {
                    NameModifierConfig.Logger.Log("没有可撤销的操作。");
                }
            }
            else if (m_PendingOperation == PendingOperation.Restore)
            {
                IReadOnlyList<RenameTarget> targets = m_UndoSystem.Restore();
                if (targets != null && targets.Count > 0)
                {
                    ApplyRenameTargets(targets, "恢复命名");
                    NameModifierConfig.Logger.Log($"恢复完成，共处理 {targets.Count} 个对象。");
                }
                else
                {
                    NameModifierConfig.Logger.Log("没有可恢复的操作。");
                }
            }

            m_PendingOperation = PendingOperation.None;
        }

        private void ApplyRenameTargets(IReadOnlyList<RenameTarget> targets, string progressTitle)
        {
            int total = targets.Count;

            var resolved = new (Object obj, string targetName)[total];
            bool hasAsset = false;
            for (int i = 0; i < total; i++)
            {
                Object obj = SessionStateUndoSystem.ResolveObject(targets[i].Key);
                resolved[i] = (obj, targets[i].TargetName);
                if (obj != null && EditorUtility.IsPersistent(obj)) hasAsset = true;
            }

            if (hasAsset) AssetDatabase.StartAssetEditing();

            Stopwatch stopwatch = new Stopwatch();
            const int PROGRESS_INTERVAL = 10;
            const double PROGRESS_TIME_INTERVAL = 0.1;
            double lastProgressTime = 0;
            try
            {
                stopwatch.Start();
                for (int i = 0; i < total; i++)
                {
                    var (obj, targetName) = resolved[i];
                    if (obj == null) continue;
                    NameModifierHandler.ApplyNameOnly(obj, targetName);

                    double elapsed = stopwatch.Elapsed.TotalSeconds;
                    bool isLast = i == total - 1;
                    if (isLast || (i % PROGRESS_INTERVAL == 0 && elapsed - lastProgressTime >= PROGRESS_TIME_INTERVAL))
                    {
                        EditorUtility.DisplayProgressBar(progressTitle,
                            $"正在处理 {i + 1}/{total}, 耗时 {elapsed:F2} 秒。",
                            (float)(i + 1) / total);
                        lastProgressTime = elapsed;
                    }
                }
            }
            finally
            {
                if (hasAsset)
                {
                    AssetDatabase.StopAssetEditing();
                    AssetDatabase.SaveAssets();
                }
                EditorUtility.ClearProgressBar();
                stopwatch.Stop();
            }
        }

        private void ExecuteBatchOperation(string progressTitle, Action<Object, int, int> perObjectAction)
        {
            Object[] selections = Selection.objects;
            int total = selections.Length;
            if (total == 0) return;

            bool hasAsset = Array.Exists(selections, d => EditorUtility.IsPersistent(d));
            if (hasAsset) AssetDatabase.StartAssetEditing();

            m_UndoSystem.PreloadKeys(selections);
            m_UndoSystem.BeginBatch();

            Stopwatch stopwatch = new Stopwatch();
            try
            {
                const int PROGRESS_INTERVAL = 10;
                const double PROGRESS_TIME_INTERVAL = 0.1;
                double lastProgressTime = 0;
                stopwatch.Start();
                for (int i = 0; i < total; i++)
                {
                    perObjectAction(selections[i], i, total);

                    double elapsed = stopwatch.Elapsed.TotalSeconds;
                    bool isLast = i == total - 1;
                    if (isLast || (i % PROGRESS_INTERVAL == 0 && elapsed - lastProgressTime >= PROGRESS_TIME_INTERVAL))
                    {
                        EditorUtility.DisplayProgressBar(progressTitle,
                            $"正在处理 {i + 1}/{total}, 耗时 {elapsed:F2} 秒。",
                            (float)(i + 1) / total);
                        lastProgressTime = elapsed;
                    }
                }
            }
            finally
            {
                m_UndoSystem.EndBatch();
                if (hasAsset)
                {
                    AssetDatabase.StopAssetEditing();
                    AssetDatabase.SaveAssets();
                }
                EditorUtility.ClearProgressBar();
                NameModifierConfig.Logger.Log($"累计处理 {total} 个对象, 累计耗时 {stopwatch.Elapsed.TotalSeconds} 秒。");
                stopwatch.Stop();
            }
        }

        private void ResetPendingGroupToDefaults()
        {
            m_PendingGroupCapacity = NameModifierConfig.Instance.DefaultGroupCapacity;
            m_PendingGroupName = NameModifierConfig.Instance.DefaultGroupNameTemplate;
        }

        private bool ValidateHandler(out NameModifierHandler handler)
        {
            handler = null;
            if (m_Handlers == null || m_Handlers.Length == 0) return false;
            if (m_SelectedHandlerIndex < 0 || m_SelectedHandlerIndex >= m_Handlers.Length) return false;
            handler = m_Handlers[m_SelectedHandlerIndex];
            return true;
        }

        private void LoadHandlersAndOptions()
        {
            m_HandlerPath = NameModifierConfig.Instance.HandlerPath;
            string relativePath = NameModifierUtility.GetAssetPath(m_HandlerPath);
            if (!AssetDatabase.IsValidFolder(relativePath))
            {
                m_Handlers = null;
                m_Options = null;
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:NameModifierHandler", new[] { relativePath });
            List<NameModifierHandler> list = new List<NameModifierHandler>(guids.Length);
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                NameModifierHandler handler = AssetDatabase.LoadAssetAtPath<NameModifierHandler>(assetPath);
                if (handler != null) list.Add(handler);
            }

            foreach (var handler in list.Where(h => m_Handlers?.Length > 0 ? !m_Handlers.Contains(h) : true))
                handler.Initialize(Repaint, m_UndoSystem);

            m_Handlers = list.ToArray();
            m_Options = m_Handlers.Select(h => h.OptionName).ToArray();
        }

        private IUndoSystem CreateUndoSystem()
        {
            NameModifierConfig cfg = NameModifierConfig.Instance;

            IUndoSystem system;
            switch (cfg.UndoSystemType)
            {
                case UndoSystemType.Memory:
                    var mem = new MemoryUndoSystem(cfg.UndoCapacity);
                    if (cfg.MaxTrackedObjects > 0) mem.SetMaxTrackedObjects(cfg.MaxTrackedObjects);
                    system = mem;
                    break;
                case UndoSystemType.None:
                    system = new NullUndoSystem();
                    break;
                default:
                    string projectGuid = NameModifierUtility.GUIDUtility.GetOrCreateProjectQualifiedGUID();
                    string keyPrefix = $"{projectGuid}.undosys";
                    var ss = new SessionStateUndoSystem(keyPrefix, cfg.UndoCapacity);
                    if (cfg.MaxTrackedObjects > 0) ss.SetMaxTrackedObjects(cfg.MaxTrackedObjects);
                    system = ss;
                    break;
            }
            return system;
        }
    }
}

#endif