#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EditorTools.NameModifier
{
    internal sealed class SessionStateUndoSystem : IUndoSystem
    {
        [Serializable]
        private sealed class Entry
        {
            public List<string> names = new List<string>();
            public int cursor = -1;
        }

        [Serializable]
        private sealed class GroupHistory
        {
            public string name = string.Empty;
            public int capacity = 10;
            public List<string> steps = new List<string>();
            public int cursor = -1;
        }

        [Serializable]
        private sealed class KeyIndex
        {
            public List<string> keys = new List<string>();
        }

        [Serializable]
        private sealed class StringList
        {
            public List<string> values = new List<string>();
        }

        private readonly string m_KeyPrefix;
        private int m_DefaultCapacity;
        private int m_MaxTrackedObjects;

        private const int DEFAULT_MAX_TRACKED_OBJECTS = 10000;

        private string m_ActiveGroupKey;

        private bool m_InBatch;
        private readonly List<string> m_BatchKeys = new List<string>();
        private readonly HashSet<string> m_BatchNewKeys = new HashSet<string>();

        private readonly Dictionary<string, Entry> m_Cache = new Dictionary<string, Entry>();
        private readonly HashSet<string> m_KnownKeys = new HashSet<string>();
        private readonly Dictionary<int, string> m_InstanceIDToKey = new Dictionary<int, string>();

        private const string INDEX_SUFFIX = ".index";
        private const string GROUP_PREFIX = ".grp.";
        private const string ASSET_PREFIX = "asset.";
        private const string SCENE_PREFIX = "scene.";

        public bool IsGroupActive => !string.IsNullOrEmpty(m_ActiveGroupKey);
        public string ActiveGroupName
        {
            get
            {
                if (!IsGroupActive) return string.Empty;
                GroupHistory h = LoadGroup(m_ActiveGroupKey);
                return h?.name ?? string.Empty;
            }
        }

        internal SessionStateUndoSystem(string keyPrefix, int defaultCapacity)
        {
            m_KeyPrefix = keyPrefix;
            m_DefaultCapacity = defaultCapacity;
            m_MaxTrackedObjects = DEFAULT_MAX_TRACKED_OBJECTS;
            LoadKnownKeys();
        }

        public void ActivateGroup(string groupName, int capacity)
        {
            m_ActiveGroupKey = $"{m_KeyPrefix}{GROUP_PREFIX}{groupName}";
            GroupHistory history = LoadGroup(m_ActiveGroupKey);
            if (history == null)
            {
                history = new GroupHistory { name = groupName, capacity = capacity };
                SaveGroup(m_ActiveGroupKey, history);
            }
            else
            {
                history.capacity = capacity;
                SaveGroup(m_ActiveGroupKey, history);
            }
        }

        public void DeactivateGroup()
        {
            m_ActiveGroupKey = null;
        }

        public void BeginBatch()
        {
            m_InBatch = true;
            m_BatchKeys.Clear();
            m_BatchNewKeys.Clear();
        }

        public void EndBatch()
        {
            if (!m_InBatch) return;
            m_InBatch = false;

            if (m_BatchKeys.Count > 0 && IsGroupActive)
                AppendBatchStep(m_ActiveGroupKey, m_BatchKeys);

            m_BatchKeys.Clear();

            if (m_BatchNewKeys.Count > 0)
            {
                foreach (string k in m_BatchNewKeys) m_KnownKeys.Add(k);
                FlushKnownKeys();
                m_BatchNewKeys.Clear();
            }
        }

        public void Record(Object obj, string oldName, string newName)
        {
            if (obj == null) return;
            if (string.IsNullOrWhiteSpace(newName)) return;
            if (oldName == newName) return;

            string key = GetObjectKey(obj);

            if (!m_KnownKeys.Contains(key) && m_KnownKeys.Count >= m_MaxTrackedObjects)
            {
                NameModifierConfig.Logger.Log($"[UndoSystem] 已达到最大追踪对象数 {m_MaxTrackedObjects}，对象 '{obj.name}' 的历史将不被记录。");
                return;
            }
            Entry entry = GetOrLoadEntry(key);

            if (entry.cursor < entry.names.Count - 1)
                entry.names.RemoveRange(entry.cursor + 1, entry.names.Count - entry.cursor - 1);

            if (entry.names.Count == 0)
            {
                entry.names.Add(oldName);
                entry.cursor = 0;
            }

            if (newName == entry.names[entry.names.Count - 1])
            {
                FlushEntry(key, entry);
                return;
            }

            int cap = m_DefaultCapacity;
            if (cap > 0 && entry.names.Count >= cap)
            {
                entry.names.RemoveAt(0);
                entry.cursor = Mathf.Max(0, entry.cursor - 1);
            }

            entry.names.Add(newName);
            entry.cursor = entry.names.Count - 1;
            FlushEntry(key, entry);

            if (IsGroupActive)
            {
                if (m_InBatch)
                {
                    if (!m_BatchKeys.Contains(key))
                        m_BatchKeys.Add(key);
                }
                else
                {
                    AppendGroupStep(m_ActiveGroupKey, key);
                }
            }
        }

        public IReadOnlyList<RenameTarget> Undo()
        {
            if (!IsGroupActive) return null;

            GroupHistory history = LoadGroup(m_ActiveGroupKey);
            if (history == null || history.cursor < 0) return null;

            string stepJson = history.steps[history.cursor];
            List<string> stepKeys = JsonUtility.FromJson<StringList>(stepJson)?.values;
            if (stepKeys == null || stepKeys.Count == 0) return null;

            var results = new List<RenameTarget>();
            foreach (string key in stepKeys)
            {
                Entry entry = GetOrLoadEntry(key);
                if (entry.cursor <= 0) continue;
                entry.cursor--;
                FlushEntry(key, entry);

                results.Add(new RenameTarget(null, entry.names[entry.cursor], key));
            }

            history.cursor--;
            SaveGroup(m_ActiveGroupKey, history);

            return results.Count > 0 ? results : null;
        }

        public IReadOnlyList<RenameTarget> Restore()
        {
            if (!IsGroupActive) return null;

            GroupHistory history = LoadGroup(m_ActiveGroupKey);
            if (history == null || history.cursor >= history.steps.Count - 1) return null;

            history.cursor++;
            string stepJson = history.steps[history.cursor];
            List<string> stepKeys = JsonUtility.FromJson<StringList>(stepJson)?.values;
            if (stepKeys == null || stepKeys.Count == 0) return null;

            var results = new List<RenameTarget>();
            foreach (string key in stepKeys)
            {
                Entry entry = GetOrLoadEntry(key);
                if (entry.cursor >= entry.names.Count - 1) continue;
                entry.cursor++;
                FlushEntry(key, entry);

                results.Add(new RenameTarget(null, entry.names[entry.cursor], key));
            }

            SaveGroup(m_ActiveGroupKey, history);

            return results.Count > 0 ? results : null;
        }

        public void PreloadKeys(Object[] objs)
        {
            if (objs == null || objs.Length == 0) return;

            var sceneObjs = new List<Object>();
            foreach (Object obj in objs)
            {
                if (obj == null) continue;
                if (EditorUtility.IsPersistent(obj))
                {
                    int id = obj.GetInstanceID();
                    if (!m_InstanceIDToKey.ContainsKey(id))
                        m_InstanceIDToKey[id] = $"{ASSET_PREFIX}{id}";
                }
                else
                {
                    sceneObjs.Add(obj);
                }
            }

            if (sceneObjs.Count == 0) return;

            var gids = new GlobalObjectId[sceneObjs.Count];
            GlobalObjectId.GetGlobalObjectIdsSlow(sceneObjs.ToArray(), gids);
            for (int i = 0; i < sceneObjs.Count; i++)
            {
                int id = sceneObjs[i].GetInstanceID();
                if (!m_InstanceIDToKey.ContainsKey(id))
                    m_InstanceIDToKey[id] = $"{SCENE_PREFIX}{gids[i]}";
            }
        }

        public void SetCapacity(int capacity)
        {
            if (capacity == m_DefaultCapacity) return;
            m_DefaultCapacity = capacity;

            if (m_DefaultCapacity <= 0) return;

            bool anyFlushed = false;
            foreach (string key in m_KnownKeys)
            {
                Entry entry = GetOrLoadEntry(key);
                if (entry.names.Count <= m_DefaultCapacity) continue;

                int excess = entry.names.Count - m_DefaultCapacity;
                entry.names.RemoveRange(0, excess);
                entry.cursor = Mathf.Max(0, entry.cursor - excess);
                FlushEntry(key, entry);
                anyFlushed = true;
            }

            if (anyFlushed)
                NameModifierConfig.Logger.Log($"[UndoSystem] Capacity changed to {m_DefaultCapacity}, trimmed excess history entries.");
        }

        public void SetMaxTrackedObjects(int max)
        {
            m_MaxTrackedObjects = max > 0 ? max : DEFAULT_MAX_TRACKED_OBJECTS;
        }

        public void ClearRecord(Object obj)
        {
            if (obj == null) return;
            string key = GetObjectKey(obj);
            m_InstanceIDToKey.Remove(obj.GetInstanceID());
            m_Cache.Remove(key);
            if (m_KnownKeys.Remove(key))
            {
                SessionState.EraseString(GetEntryKey(key));
                FlushKnownKeys();
            }
        }

        public void ClearAll()
        {
            foreach (string key in m_KnownKeys)
                SessionState.EraseString(GetEntryKey(key));
            m_Cache.Clear();
            m_KnownKeys.Clear();
            m_InstanceIDToKey.Clear();
            SessionState.EraseString(GetIndexKey());
            if (IsGroupActive)
            {
                SessionState.EraseString(m_ActiveGroupKey);
                m_ActiveGroupKey = null;
            }
        }

        public void ClearInvalid()
        {
            var toRemove = new HashSet<string>();
            foreach (string key in m_KnownKeys)
            {
                if (!IsObjectAlive(key))
                    toRemove.Add(key);
            }

            if (toRemove.Count == 0) return;

            foreach (string key in toRemove)
            {
                m_Cache.Remove(key);
                m_KnownKeys.Remove(key);
                SessionState.EraseString(GetEntryKey(key));
            }

            var staleIDs = new List<int>();
            foreach (var pair in m_InstanceIDToKey)
            {
                if (toRemove.Contains(pair.Value))
                    staleIDs.Add(pair.Key);
            }
            foreach (int id in staleIDs)
                m_InstanceIDToKey.Remove(id);

            FlushKnownKeys();
        }

        internal static Object ResolveObject(string key)
        {
            if (key.StartsWith(ASSET_PREFIX))
            {
                if (int.TryParse(key.Substring(ASSET_PREFIX.Length), out int id))
                    return EditorUtility.InstanceIDToObject(id);
                return null;
            }

            if (key.StartsWith(SCENE_PREFIX))
            {
                string gidStr = key.Substring(SCENE_PREFIX.Length);
                if (GlobalObjectId.TryParse(gidStr, out GlobalObjectId gid))
                    return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(gid);
                return null;
            }

            return null;
        }

        private int GetActiveGroupCapacity()
        {
            GroupHistory h = LoadGroup(m_ActiveGroupKey);
            return h?.capacity ?? m_DefaultCapacity;
        }

        private void AppendGroupStep(string groupKey, string objectKey)
        {
            GroupHistory history = LoadGroup(groupKey);
            if (history == null) return;

            if (history.cursor < history.steps.Count - 1)
                history.steps.RemoveRange(history.cursor + 1, history.steps.Count - history.cursor - 1);

            var stepList = new StringList { values = new List<string> { objectKey } };
            history.steps.Add(JsonUtility.ToJson(stepList));
            history.cursor = history.steps.Count - 1;

            SaveGroup(groupKey, history);

            if (history.capacity > 0 && history.steps.Count >= history.capacity)
                m_ActiveGroupKey = null;
        }

        private void AppendBatchStep(string groupKey, List<string> keys)
        {
            GroupHistory history = LoadGroup(groupKey);
            if (history == null) return;

            if (history.cursor < history.steps.Count - 1)
                history.steps.RemoveRange(history.cursor + 1, history.steps.Count - history.cursor - 1);

            var stepList = new StringList { values = new List<string>(keys) };
            history.steps.Add(JsonUtility.ToJson(stepList));
            history.cursor = history.steps.Count - 1;

            SaveGroup(groupKey, history);

            if (history.capacity > 0 && history.steps.Count >= history.capacity)
                m_ActiveGroupKey = null;
        }

        private string GetObjectKey(Object obj)
        {
            int instanceID = obj.GetInstanceID();
            if (m_InstanceIDToKey.TryGetValue(instanceID, out string cached))
                return cached;

            string key;
            if (EditorUtility.IsPersistent(obj))
                key = $"{ASSET_PREFIX}{instanceID}";
            else
            {
                GlobalObjectId gid = GlobalObjectId.GetGlobalObjectIdSlow(obj);
                key = $"{SCENE_PREFIX}{gid}";
            }

            m_InstanceIDToKey[instanceID] = key;
            return key;
        }

        private static bool IsObjectAlive(string key)
        {
            return ResolveObject(key) != null;
        }

        private Entry GetOrLoadEntry(string key)
        {
            if (m_Cache.TryGetValue(key, out Entry cached)) return cached;
            string json = SessionState.GetString(GetEntryKey(key), string.Empty);
            Entry entry = string.IsNullOrEmpty(json)
                ? new Entry()
                : JsonUtility.FromJson<Entry>(json) ?? new Entry();
            m_Cache[key] = entry;
            return entry;
        }

        private void FlushEntry(string key, Entry entry)
        {
            m_Cache[key] = entry;
            SessionState.SetString(GetEntryKey(key), JsonUtility.ToJson(entry));

            if (m_InBatch)
            {
                if (!m_KnownKeys.Contains(key))
                    m_BatchNewKeys.Add(key);
            }
            else
            {
                if (m_KnownKeys.Add(key)) FlushKnownKeys();
            }
        }

        private void FlushKnownKeys()
        {
            var index = new KeyIndex();
            index.keys.AddRange(m_KnownKeys);
            SessionState.SetString(GetIndexKey(), JsonUtility.ToJson(index));
        }

        private void LoadKnownKeys()
        {
            string json = SessionState.GetString(GetIndexKey(), string.Empty);
            if (string.IsNullOrEmpty(json)) return;
            KeyIndex index = JsonUtility.FromJson<KeyIndex>(json);
            if (index?.keys == null) return;
            foreach (string key in index.keys) m_KnownKeys.Add(key);
        }

        private GroupHistory LoadGroup(string groupKey)
        {
            string json = SessionState.GetString(groupKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return null;
            return JsonUtility.FromJson<GroupHistory>(json);
        }

        private void SaveGroup(string groupKey, GroupHistory history)
        {
            SessionState.SetString(groupKey, JsonUtility.ToJson(history));
        }

        private string GetEntryKey(string key) => $"{m_KeyPrefix}.{key}";
        private string GetIndexKey() => m_KeyPrefix + INDEX_SUFFIX;
    }
}

#endif
