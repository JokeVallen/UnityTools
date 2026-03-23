#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EditorTools.NameModifier.Tests
{
    internal static class UndoSystemFactory
    {
        private static int s_Counter;

        internal static SessionStateUndoSystem Create(int capacity = 10)
        {
            string prefix = $"test.undosys.{s_Counter++}";
            var system = new SessionStateUndoSystem(prefix, capacity);
            system.ClearAll();
            return system;
        }

        internal static SessionStateUndoSystem CreateWithGroup(
            string groupName, int groupCapacity = 10, int defaultCapacity = 10)
        {
            var system = Create(defaultCapacity);
            system.ActivateGroup(groupName, groupCapacity);
            return system;
        }
    }

    // 管理测试用资产对象的生命周期，在 Dispose 时自动删除
    internal sealed class AssetObjectScope : IDisposable
    {
        internal readonly ScriptableObject Asset;
        private readonly string m_AssetPath;
        private static int s_Counter;

        internal AssetObjectScope()
        {
            Asset = ScriptableObject.CreateInstance<TestAsset>();
            m_AssetPath = $"Assets/Temp_TestAsset_{s_Counter++}.asset";
            AssetDatabase.CreateAsset(Asset, m_AssetPath);
            AssetDatabase.SaveAssets();
        }

        public void Dispose()
        {
            AssetDatabase.DeleteAsset(m_AssetPath);
        }
    }

    internal sealed class TestLogger : INameModifierLogger
    {
        internal readonly List<string> Messages = new List<string>();

        public void Log(object message)
        {
            Messages.Add($"{message}");
        }

        public void LogError(object message)
        {
            Messages.Add($"{message}");
        }

        public void LogException(Exception exception)
        {
            Messages.Add($"{exception.Message}");
        }

        public void LogWarning(object message)
        {
            Messages.Add($"{message}");
        }

    }

    internal sealed class TestHandler : NameModifierHandler
    {
        public override string OptionName => "Test";

        private string m_TargetName;

        public override void Modify(Object obj, int index, int count)
        {
            ApplyRename(obj, m_TargetName);
        }

        internal void SetTargetName(string name) => m_TargetName = name;

        internal void InitForTest(IUndoSystem undoSystem)
        {
            Initialize(() => { }, undoSystem);
        }

        internal new void ApplyRename(Object obj, string newName)
        {
            base.ApplyRename(obj, newName);
        }
    }
}

#endif