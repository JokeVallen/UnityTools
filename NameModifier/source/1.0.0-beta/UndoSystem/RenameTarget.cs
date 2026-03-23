using UnityEngine;

namespace EditorTools.NameModifier
{
    internal readonly struct RenameTarget
    {
        internal readonly Object Obj;
        internal readonly string TargetName;
        internal readonly string Key;

        internal RenameTarget(Object obj, string targetName, string key)
        {
            Obj = obj;
            TargetName = targetName;
            Key = key;
        }
    }
}