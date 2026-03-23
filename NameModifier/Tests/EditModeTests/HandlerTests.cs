#if UNITY_EDITOR

using NUnit.Framework;
using UnityEngine;

namespace EditorTools.NameModifier.Tests
{
    [TestFixture]
    internal sealed class HandlerApplyRenameTests
    {
        private SessionStateUndoSystem m_UndoSystem;
        private TestHandler m_Handler;
        private AssetObjectScope m_AssetScope;
        private GameObject m_SceneObj;

        [SetUp]
        public void SetUp()
        {
            m_UndoSystem = UndoSystemFactory.CreateWithGroup("TestGroup");
            m_Handler = ScriptableObject.CreateInstance<TestHandler>();
            m_Handler.InitForTest(m_UndoSystem);
            m_AssetScope = new AssetObjectScope();
            m_SceneObj = new GameObject("SceneObj");
        }

        [TearDown]
        public void TearDown()
        {
            m_UndoSystem.ClearAll();
            Object.DestroyImmediate(m_Handler);
            m_AssetScope.Dispose();
            Object.DestroyImmediate(m_SceneObj);
        }

        [Test]
        public void ApplyRename_SceneObject_ChangesName()
        {
            m_Handler.ApplyRename(m_SceneObj, "NewName");

            Assert.AreEqual("NewName", m_SceneObj.name);
        }

        [Test]
        public void ApplyRename_SceneObject_RecordsInCustomHistory()
        {
            // 场景对象改名写入自定义历史，不依赖 Unity Undo
            m_Handler.ApplyRename(m_SceneObj, "NewName");

            var targets = m_UndoSystem.Undo();
            Assert.IsNotNull(targets);
            Assert.AreEqual("SceneObj", targets[0].TargetName);
        }

        [Test]
        public void ApplyRename_AssetObject_RecordsHistory()
        {
            var asset = m_AssetScope.Asset;
            string originalName = asset.name;
            m_Handler.ApplyRename(asset, "NewAssetName");

            var targets = m_UndoSystem.Undo();
            Assert.IsNotNull(targets);
            Assert.AreEqual(originalName, targets[0].TargetName);
        }

        [Test]
        public void ApplyRename_SameName_DoesNotRecord()
        {
            var asset = m_AssetScope.Asset;
            m_Handler.ApplyRename(asset, asset.name);

            Assert.IsNull(m_UndoSystem.Undo());
        }

        [Test]
        public void ApplyRename_NullObject_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => m_Handler.ApplyRename(null, "NewName"));
        }

        [Test]
        public void ApplyRename_WhitespaceName_DoesNotChange()
        {
            var asset = m_AssetScope.Asset;
            string originalName = asset.name;
            m_Handler.ApplyRename(asset, "   ");

            Assert.AreEqual(originalName, asset.name);
        }
    }

    [TestFixture]
    internal sealed class HandlerModifyTests
    {
        private SessionStateUndoSystem m_UndoSystem;
        private TestHandler m_Handler;
        private AssetObjectScope m_AssetScope;

        [SetUp]
        public void SetUp()
        {
            m_UndoSystem = UndoSystemFactory.CreateWithGroup("TestGroup");
            m_Handler = ScriptableObject.CreateInstance<TestHandler>();
            m_Handler.InitForTest(m_UndoSystem);
            m_AssetScope = new AssetObjectScope();
        }

        [TearDown]
        public void TearDown()
        {
            m_UndoSystem.ClearAll();
            Object.DestroyImmediate(m_Handler);
            m_AssetScope.Dispose();
        }

        [Test]
        public void Modify_RecordsHistory_UndoReturnsOriginalName()
        {
            var asset = m_AssetScope.Asset;
            string originalName = asset.name;
            m_Handler.SetTargetName("Modified");
            m_Handler.Modify(asset, 0, 1);

            var targets = m_UndoSystem.Undo();
            Assert.IsNotNull(targets);
            Assert.AreEqual(originalName, targets[0].TargetName);
        }

        [Test]
        public void Modify_ThenUndo_ThenRestore_ReturnsCyclically()
        {
            var asset = m_AssetScope.Asset;
            string originalName = asset.name;
            m_Handler.SetTargetName("Modified");
            m_Handler.Modify(asset, 0, 1);

            var undoTargets = m_UndoSystem.Undo();
            Assert.IsNotNull(undoTargets);
            Assert.AreEqual(originalName, undoTargets[0].TargetName);

            var restoreTargets = m_UndoSystem.Restore();
            Assert.IsNotNull(restoreTargets);
            Assert.AreEqual("Modified", restoreTargets[0].TargetName);
        }
    }
}

#endif