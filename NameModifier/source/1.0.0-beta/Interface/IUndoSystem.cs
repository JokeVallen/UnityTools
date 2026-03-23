#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace EditorTools.NameModifier
{
    internal interface IUndoSystem
    {
        void ActivateGroup(string groupName, int capacity);
        void DeactivateGroup();
        bool IsGroupActive { get; }
        string ActiveGroupName { get; }

        void BeginBatch();
        void EndBatch();
        void Record(Object obj, string oldName, string newName);

        IReadOnlyList<RenameTarget> Undo();
        IReadOnlyList<RenameTarget> Restore();

        void PreloadKeys(Object[] objs);
        void SetCapacity(int capacity);
        void SetMaxTrackedObjects(int max);
        void ClearRecord(Object obj);
        void ClearAll();
        void ClearInvalid();
    }
}

#endif
