#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorTools.NameModifier
{
    internal sealed class MemoryUndoSystem : IUndoSystem
    {
        private sealed class Entry
        {
            public List<string> names = new List<string>();
            public int cursor = -1;
        }

        private sealed class GroupHistory
        {
            public string name;
            public int capacity;
            public List<List<string>> steps = new List<List<string>>();
            public int cursor = -1;
        }

        private int m_DefaultCapacity;
        private int m_MaxTrackedObjects;
        private string m_ActiveGroupName;
        private GroupHistory m_ActiveGroup;

        private bool m_InBatch;
        private readonly List<string> m_BatchKeys = new List<string>();

        private readonly Dictionary<string, Entry> m_Entries = new Dictionary<string, Entry>();
        private readonly Dictionary<string, GroupHistory> m_Groups = new Dictionary<string, GroupHistory>();
        private readonly Dictionary<int, string> m_InstanceIDToKey = new Dictionary<int, string>();

        private const int DEFAULT_MAX_TRACKED = 10000;

        public bool IsGroupActive => m_ActiveGroup != null;
        public string ActiveGroupName => m_ActiveGroupName ?? string.Empty;

        internal MemoryUndoSystem(int defaultCapacity = 10)
        {
            m_DefaultCapacity = defaultCapacity;
            m_MaxTrackedObjects = DEFAULT_MAX_TRACKED;
        }

        public void ActivateGroup(string groupName, int capacity)
        {
            m_ActiveGroupName = groupName;
            if (!m_Groups.TryGetValue(groupName, out GroupHistory h))
            {
                h = new GroupHistory { name = groupName, capacity = capacity };
                m_Groups[groupName] = h;
            }
            else
            {
                h.capacity = capacity;
            }
            m_ActiveGroup = h;
        }

        public void DeactivateGroup()
        {
            m_ActiveGroupName = null;
            m_ActiveGroup = null;
        }

        public void BeginBatch()
        {
            m_InBatch = true;
            m_BatchKeys.Clear();
        }

        public void EndBatch()
        {
            if (!m_InBatch) return;
            m_InBatch = false;

            if (m_BatchKeys.Count > 0 && IsGroupActive)
                AppendStep(m_ActiveGroup, new List<string>(m_BatchKeys));

            m_BatchKeys.Clear();
        }

        public void Record(Object obj, string oldName, string newName)
        {
            if (obj == null) return;
            if (string.IsNullOrWhiteSpace(newName)) return;
            if (oldName == newName) return;

            string key = GetObjectKey(obj);

            if (!m_Entries.ContainsKey(key) && m_Entries.Count >= m_MaxTrackedObjects) return;

            if (!m_Entries.TryGetValue(key, out Entry entry))
            {
                entry = new Entry();
                m_Entries[key] = entry;
            }

            if (entry.cursor < entry.names.Count - 1)
                entry.names.RemoveRange(entry.cursor + 1, entry.names.Count - entry.cursor - 1);

            if (entry.names.Count == 0) { entry.names.Add(oldName); entry.cursor = 0; }

            if (newName == entry.names[entry.names.Count - 1]) return;

            if (m_DefaultCapacity > 0 && entry.names.Count >= m_DefaultCapacity)
            {
                entry.names.RemoveAt(0);
                entry.cursor = Mathf.Max(0, entry.cursor - 1);
            }

            entry.names.Add(newName);
            entry.cursor = entry.names.Count - 1;

            if (IsGroupActive)
            {
                if (m_InBatch) { if (!m_BatchKeys.Contains(key)) m_BatchKeys.Add(key); }
                else AppendStep(m_ActiveGroup, new List<string> { key });
            }
        }

        public IReadOnlyList<RenameTarget> Undo()
        {
            if (!IsGroupActive || m_ActiveGroup.cursor < 0) return null;

            List<string> stepKeys = m_ActiveGroup.steps[m_ActiveGroup.cursor];
            var results = new List<RenameTarget>();
            foreach (string key in stepKeys)
            {
                if (!m_Entries.TryGetValue(key, out Entry entry) || entry.cursor <= 0) continue;
                entry.cursor--;
                Object obj = ResolveObject(key);
                if (obj != null) results.Add(new RenameTarget(obj, entry.names[entry.cursor], key));
            }
            m_ActiveGroup.cursor--;
            return results.Count > 0 ? results : null;
        }

        public IReadOnlyList<RenameTarget> Restore()
        {
            if (!IsGroupActive || m_ActiveGroup.cursor >= m_ActiveGroup.steps.Count - 1) return null;

            m_ActiveGroup.cursor++;
            List<string> stepKeys = m_ActiveGroup.steps[m_ActiveGroup.cursor];
            var results = new List<RenameTarget>();
            foreach (string key in stepKeys)
            {
                if (!m_Entries.TryGetValue(key, out Entry entry) || entry.cursor >= entry.names.Count - 1) continue;
                entry.cursor++;
                Object obj = ResolveObject(key);
                if (obj != null) results.Add(new RenameTarget(obj, entry.names[entry.cursor], key));
            }
            return results.Count > 0 ? results : null;
        }

        public void PreloadKeys(Object[] objs)
        {
            if (objs == null) return;
            var sceneObjs = new List<Object>();
            foreach (Object obj in objs)
            {
                if (obj == null) continue;
                if (EditorUtility.IsPersistent(obj))
                {
                    int id = obj.GetInstanceID();
                    if (!m_InstanceIDToKey.ContainsKey(id))
                        m_InstanceIDToKey[id] = $"asset.{id}";
                }
                else sceneObjs.Add(obj);
            }
            if (sceneObjs.Count == 0) return;
            var gids = new GlobalObjectId[sceneObjs.Count];
            GlobalObjectId.GetGlobalObjectIdsSlow(sceneObjs.ToArray(), gids);
            for (int i = 0; i < sceneObjs.Count; i++)
            {
                int id = sceneObjs[i].GetInstanceID();
                if (!m_InstanceIDToKey.ContainsKey(id))
                    m_InstanceIDToKey[id] = $"scene.{gids[i]}";
            }
        }

        public void SetCapacity(int capacity)
        {
            if (capacity == m_DefaultCapacity) return;
            m_DefaultCapacity = capacity;
            if (m_DefaultCapacity <= 0) return;
            foreach (var entry in m_Entries.Values)
            {
                if (entry.names.Count <= m_DefaultCapacity) continue;
                int excess = entry.names.Count - m_DefaultCapacity;
                entry.names.RemoveRange(0, excess);
                entry.cursor = Mathf.Max(0, entry.cursor - excess);
            }
        }

        public void SetMaxTrackedObjects(int max)
        {
            m_MaxTrackedObjects = max > 0 ? max : DEFAULT_MAX_TRACKED;
        }

        public void ClearRecord(Object obj)
        {
            if (obj == null) return;
            string key = GetObjectKey(obj);
            m_InstanceIDToKey.Remove(obj.GetInstanceID());
            m_Entries.Remove(key);
        }

        public void ClearAll()
        {
            m_Entries.Clear();
            m_Groups.Clear();
            m_InstanceIDToKey.Clear();
            m_ActiveGroup = null;
            m_ActiveGroupName = null;
        }

        public void ClearInvalid()
        {
            var toRemove = new List<string>();
            foreach (string key in m_Entries.Keys)
            {
                if (SessionStateUndoSystem.ResolveObject(key) == null)
                    toRemove.Add(key);
            }
            foreach (string key in toRemove)
            {
                m_Entries.Remove(key);
                var staleIDs = new List<int>();
                foreach (var pair in m_InstanceIDToKey)
                    if (pair.Value == key) staleIDs.Add(pair.Key);
                foreach (int id in staleIDs) m_InstanceIDToKey.Remove(id);
            }
        }

        private void AppendStep(GroupHistory group, List<string> keys)
        {
            if (group.cursor < group.steps.Count - 1)
                group.steps.RemoveRange(group.cursor + 1, group.steps.Count - group.cursor - 1);
            group.steps.Add(keys);
            group.cursor = group.steps.Count - 1;
            if (group.capacity > 0 && group.steps.Count >= group.capacity)
                DeactivateGroup();
        }

        private string GetObjectKey(Object obj)
        {
            int id = obj.GetInstanceID();
            if (m_InstanceIDToKey.TryGetValue(id, out string cached)) return cached;
            string key = EditorUtility.IsPersistent(obj)
                ? $"asset.{id}"
                : $"scene.{GlobalObjectId.GetGlobalObjectIdSlow(obj)}";
            m_InstanceIDToKey[id] = key;
            return key;
        }

        private static Object ResolveObject(string key)
            => SessionStateUndoSystem.ResolveObject(key);
    }
}

#endif
