#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace EditorTools.NameModifier
{
    internal sealed class NullUndoSystem : IUndoSystem
    {
        public bool IsGroupActive => false;
        public string ActiveGroupName => string.Empty;

        public void ActivateGroup(string groupName, int capacity) { }
        public void DeactivateGroup() { }
        public void BeginBatch() { }
        public void EndBatch() { }
        public void Record(Object obj, string oldName, string newName) { }
        public IReadOnlyList<RenameTarget> Undo() => null;
        public IReadOnlyList<RenameTarget> Restore() => null;
        public void PreloadKeys(Object[] objs) { }
        public void SetCapacity(int capacity) { }
        public void SetMaxTrackedObjects(int max) { }
        public void ClearRecord(Object obj) { }
        public void ClearAll() { }
        public void ClearInvalid() { }
    }
}

#endif
