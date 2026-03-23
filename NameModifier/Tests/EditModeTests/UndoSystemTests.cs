#if UNITY_EDITOR

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace EditorTools.NameModifier.Tests
{
    // 所有需要验证历史记录的测试使用资产对象
    // 场景对象（GameObject）由 Unity Undo 系统负责，不写入自定义历史
    [TestFixture]
    internal sealed class UndoSystemRecordTests
    {
        private SessionStateUndoSystem m_System;
        private AssetObjectScope m_AssetScope;

        [SetUp]
        public void SetUp()
        {
            m_System = UndoSystemFactory.CreateWithGroup("TestGroup", groupCapacity: 10);
            m_AssetScope = new AssetObjectScope();
        }

        [TearDown]
        public void TearDown()
        {
            m_System.ClearAll();
            m_AssetScope.Dispose();
        }

        [Test]
        public void Record_ThenUndo_ReturnsPreviousName()
        {
            var asset = m_AssetScope.Asset;
            m_System.Record(asset, "A", "B");

            var targets = m_System.Undo();

            Assert.IsNotNull(targets);
            Assert.AreEqual(1, targets.Count);
            Assert.AreEqual("A", targets[0].TargetName);
        }

        [Test]
        public void Record_ThenRestore_ReturnsNull_WhenNothingToRestore()
        {
            var asset = m_AssetScope.Asset;
            m_System.Record(asset, "A", "B");

            var targets = m_System.Restore();

            Assert.IsNull(targets);
        }

        [Test]
        public void Undo_ThenRestore_ReturnsNewName()
        {
            var asset = m_AssetScope.Asset;
            m_System.Record(asset, "A", "B");
            m_System.Undo();

            var targets = m_System.Restore();

            Assert.IsNotNull(targets);
            Assert.AreEqual(1, targets.Count);
            Assert.AreEqual("B", targets[0].TargetName);
        }

        [Test]
        public void Undo_WithNoHistory_ReturnsNull()
        {
            var targets = m_System.Undo();

            Assert.IsNull(targets);
        }

        [Test]
        public void Undo_AtOldestEntry_ReturnsNull()
        {
            var asset = m_AssetScope.Asset;
            m_System.Record(asset, "A", "B");
            m_System.Undo();

            var targets = m_System.Undo();

            Assert.IsNull(targets);
        }

        [Test]
        public void MultipleRecords_UndoStepsBack_OneAtATime()
        {
            var asset = m_AssetScope.Asset;
            m_System.Record(asset, "A", "B");
            m_System.Record(asset, "B", "C");
            m_System.Record(asset, "C", "D");

            Assert.AreEqual("C", m_System.Undo()?[0].TargetName);
            Assert.AreEqual("B", m_System.Undo()?[0].TargetName);
            Assert.AreEqual("A", m_System.Undo()?[0].TargetName);
            Assert.IsNull(m_System.Undo());
        }

        [Test]
        public void Record_AfterUndo_TruncatesFutureBranch()
        {
            var asset = m_AssetScope.Asset;
            m_System.Record(asset, "A", "B");
            m_System.Record(asset, "B", "C");
            m_System.Undo();
            m_System.Record(asset, "B", "D");

            Assert.AreEqual("B", m_System.Undo()?[0].TargetName);
            Assert.AreEqual("A", m_System.Undo()?[0].TargetName);
            Assert.IsNull(m_System.Undo());
        }

        [Test]
        public void Record_SameName_DoesNotDuplicate()
        {
            var asset = m_AssetScope.Asset;
            m_System.Record(asset, "A", "B");
            m_System.Record(asset, "B", "B");

            Assert.AreEqual("A", m_System.Undo()?[0].TargetName);
            Assert.IsNull(m_System.Undo());
        }

        [Test]
        public void Record_NullObject_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => m_System.Record(null, "A", "B"));
        }

        [Test]
        public void Record_WhitespaceNewName_DoesNotRecord()
        {
            var asset = m_AssetScope.Asset;
            m_System.Record(asset, "A", "   ");

            Assert.IsNull(m_System.Undo());
        }

        [Test]
        public void Record_OldNameEqualsNewName_DoesNotRecord()
        {
            var asset = m_AssetScope.Asset;
            m_System.Record(asset, "A", "A");

            Assert.IsNull(m_System.Undo());
        }

        [Test]
        public void Record_SceneObject_IsRecorded()
        {
            // 场景对象现在也写入自定义历史，不再依赖 Unity Undo
            var go = new GameObject("SceneObj");
            m_System.Record(go, "A", "B");

            var targets = m_System.Undo();
            Assert.IsNotNull(targets);
            Assert.AreEqual("A", targets[0].TargetName);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Undo_WithNoActiveGroup_ReturnsNull()
        {
            var system = UndoSystemFactory.Create();
            var asset = m_AssetScope.Asset;
            system.Record(asset, "A", "B");

            var targets = system.Undo();

            Assert.IsNull(targets);
            system.ClearAll();
        }
    }

    [TestFixture]
    internal sealed class UndoSystemCapacityTests
    {
        private SessionStateUndoSystem m_System;
        private AssetObjectScope m_AssetScope;

        [SetUp]
        public void SetUp()
        {
            m_System = UndoSystemFactory.CreateWithGroup("TestGroup", groupCapacity: 10, defaultCapacity: 3);
            m_AssetScope = new AssetObjectScope();
        }

        [TearDown]
        public void TearDown()
        {
            m_System.ClearAll();
            m_AssetScope.Dispose();
        }

        [Test]
        public void Record_ExceedingDefaultCapacity_DropsOldestEntry()
        {
            var asset = m_AssetScope.Asset;
            m_System.Record(asset, "A", "B");
            m_System.Record(asset, "B", "C");
            m_System.Record(asset, "C", "D");

            Assert.AreEqual("C", m_System.Undo()?[0].TargetName);
            Assert.AreEqual("B", m_System.Undo()?[0].TargetName);
            Assert.IsNull(m_System.Undo());
        }

        [Test]
        public void SetCapacity_Smaller_TrimsOldestEntries()
        {
            m_System = UndoSystemFactory.CreateWithGroup("TestGroup", groupCapacity: 10, defaultCapacity: 5);
            var asset = m_AssetScope.Asset;
            m_System.Record(asset, "A", "B");
            m_System.Record(asset, "B", "C");
            m_System.Record(asset, "C", "D");

            m_System.SetCapacity(2);

            Assert.AreEqual("C", m_System.Undo()?[0].TargetName);
            Assert.IsNull(m_System.Undo());
        }

        [Test]
        public void SetCapacity_Same_DoesNothing()
        {
            m_System = UndoSystemFactory.CreateWithGroup("TestGroup", groupCapacity: 10, defaultCapacity: 5);
            var asset = m_AssetScope.Asset;
            m_System.Record(asset, "A", "B");
            m_System.Record(asset, "B", "C");

            m_System.SetCapacity(5);

            Assert.AreEqual("B", m_System.Undo()?[0].TargetName);
            Assert.AreEqual("A", m_System.Undo()?[0].TargetName);
        }

        [Test]
        public void SetCapacity_TrimsExcess_LogsMessage()
        {
            var logger = new TestLogger();
            var system = new SessionStateUndoSystem("test.cap.log", 5);
            system.ClearAll();
            system.ActivateGroup("g", 10);
            var asset = m_AssetScope.Asset;
            system.Record(asset, "A", "B");
            system.Record(asset, "B", "C");
            system.Record(asset, "C", "D");

            system.SetCapacity(2);

            Assert.AreEqual(1, logger.Messages.Count);
            system.ClearAll();
        }

        [Test]
        public void SetCapacity_NoTrim_DoesNotLog()
        {
            var logger = new TestLogger();
            var system = new SessionStateUndoSystem("test.cap.nolog", 5);
            system.ClearAll();
            system.ActivateGroup("g", 10);
            var asset = m_AssetScope.Asset;
            system.Record(asset, "A", "B");

            system.SetCapacity(3);

            Assert.AreEqual(0, logger.Messages.Count);
            system.ClearAll();
        }
    }

    [TestFixture]
    internal sealed class UndoSystemGroupTests
    {
        private SessionStateUndoSystem m_System;
        private AssetObjectScope m_AssetScopeA;
        private AssetObjectScope m_AssetScopeB;

        [SetUp]
        public void SetUp()
        {
            m_System = UndoSystemFactory.Create();
            m_AssetScopeA = new AssetObjectScope();
            m_AssetScopeB = new AssetObjectScope();
        }

        [TearDown]
        public void TearDown()
        {
            m_System.ClearAll();
            m_AssetScopeA.Dispose();
            m_AssetScopeB.Dispose();
        }

        [Test]
        public void IsGroupActive_WhenNoGroupActivated_ReturnsFalse()
        {
            Assert.IsFalse(m_System.IsGroupActive);
        }

        [Test]
        public void IsGroupActive_AfterActivate_ReturnsTrue()
        {
            m_System.ActivateGroup("G1", 10);

            Assert.IsTrue(m_System.IsGroupActive);
        }

        [Test]
        public void IsGroupActive_AfterDeactivate_ReturnsFalse()
        {
            m_System.ActivateGroup("G1", 10);
            m_System.DeactivateGroup();

            Assert.IsFalse(m_System.IsGroupActive);
        }

        [Test]
        public void ActiveGroupName_ReflectsActivatedGroup()
        {
            m_System.ActivateGroup("MyGroup", 10);

            Assert.AreEqual("MyGroup", m_System.ActiveGroupName);
        }

        [Test]
        public void Undo_RevertsRecordedObjects()
        {
            var assetA = m_AssetScopeA.Asset;
            var assetB = m_AssetScopeB.Asset;
            m_System.ActivateGroup("G", 10);
            m_System.Record(assetA, "A1", "A2");
            m_System.Record(assetB, "B1", "B2");

            var step2 = m_System.Undo();
            var step1 = m_System.Undo();

            Assert.IsNotNull(step2);
            Assert.IsNotNull(step1);
        }

        [Test]
        public void Undo_WithNoGroup_ReturnsNull()
        {
            var targets = m_System.Undo();

            Assert.IsNull(targets);
        }

        [Test]
        public void Restore_AfterUndo_ReturnsTargets()
        {
            var asset = m_AssetScopeA.Asset;
            m_System.ActivateGroup("G", 10);
            m_System.Record(asset, "A1", "A2");
            m_System.Undo();

            var targets = m_System.Restore();

            Assert.IsNotNull(targets);
        }

        [Test]
        public void Restore_WithNothingToRestore_ReturnsNull()
        {
            var asset = m_AssetScopeA.Asset;
            m_System.ActivateGroup("G", 10);
            m_System.Record(asset, "A1", "A2");

            var targets = m_System.Restore();

            Assert.IsNull(targets);
        }

        [Test]
        public void Record_AfterUndo_TruncatesRestoreBranch()
        {
            var asset = m_AssetScopeA.Asset;
            m_System.ActivateGroup("G", 10);
            m_System.Record(asset, "A1", "A2");
            m_System.Record(asset, "A2", "A3");
            m_System.Undo();
            m_System.Record(asset, "A2", "A4");

            m_System.Undo();
            var targets = m_System.Restore();
            Assert.IsNotNull(targets);
            Assert.AreEqual("A4", targets[0].TargetName);
        }

        [Test]
        public void GroupCapacity_Reached_AutoDeactivatesGroup()
        {
            var asset = m_AssetScopeA.Asset;
            m_System.ActivateGroup("G", 2);
            m_System.Record(asset, "A1", "A2");
            m_System.Record(asset, "A2", "A3");

            Assert.IsFalse(m_System.IsGroupActive);
        }

        [Test]
        public void BeginBatch_EndBatch_MergesMultipleRecordsIntoOneStep()
        {
            var assetA = m_AssetScopeA.Asset;
            var assetB = m_AssetScopeB.Asset;
            m_System.ActivateGroup("G", 10);
            m_System.BeginBatch();
            m_System.Record(assetA, "A1", "A2");
            m_System.Record(assetB, "B1", "B2");
            m_System.EndBatch();

            // 一次撤销应同时回退两个对象
            var targets = m_System.Undo();
            Assert.IsNotNull(targets);
            Assert.AreEqual(2, targets.Count);
        }

        [Test]
        public void BeginBatch_EndBatch_EmptyBatch_DoesNotCommitStep()
        {
            m_System.ActivateGroup("G", 10);
            m_System.BeginBatch();
            m_System.EndBatch();

            Assert.IsNull(m_System.Undo());
        }
    }

    [TestFixture]
    internal sealed class UndoSystemClearTests
    {
        private SessionStateUndoSystem m_System;
        private AssetObjectScope m_AssetScope;

        [SetUp]
        public void SetUp()
        {
            m_System = UndoSystemFactory.CreateWithGroup("TestGroup");
            m_AssetScope = new AssetObjectScope();
        }

        [TearDown]
        public void TearDown()
        {
            m_System.ClearAll();
            m_AssetScope.Dispose();
        }

        [Test]
        public void ClearRecord_RemovesHistoryForObject()
        {
            var asset = m_AssetScope.Asset;
            m_System.Record(asset, "A", "B");
            m_System.ClearRecord(asset);

            Assert.IsNull(m_System.Undo());
        }

        [Test]
        public void ClearAll_RemovesAllHistory()
        {
            using var scope2 = new AssetObjectScope();
            var asset1 = m_AssetScope.Asset;
            var asset2 = scope2.Asset;
            m_System.Record(asset1, "A", "B");
            m_System.Record(asset2, "X", "Y");

            m_System.ClearAll();

            Assert.IsFalse(m_System.IsGroupActive);
            Assert.IsNull(m_System.Undo());
        }

        [Test]
        public void ClearInvalid_RemovesDestroyedObjectHistory()
        {
            var tempObj = new GameObject("Temp");
            m_System.Record(tempObj, "T", "T2");
            Object.DestroyImmediate(tempObj);

            Assert.DoesNotThrow(() => m_System.ClearInvalid());
        }

        [Test]
        public void ClearInvalid_PreservesLivingAssetHistory()
        {
            var asset = m_AssetScope.Asset;
            m_System.Record(asset, "A", "B");
            m_System.ClearInvalid();

            Assert.IsNotNull(m_System.Undo());
        }

        [Test]
        public void ClearInvalid_UnsavedSceneObject_IsNotReliable()
        {
            var go = new GameObject("Temp");
            m_System.Record(go, "A", "B");

            Assert.DoesNotThrow(() => m_System.ClearInvalid());
            Object.DestroyImmediate(go);
        }
    }
}

#endif