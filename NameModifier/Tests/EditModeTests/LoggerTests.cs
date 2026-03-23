#if UNITY_EDITOR

using NUnit.Framework;
using UnityEngine;

namespace EditorTools.NameModifier.Tests
{
    [TestFixture]
    internal sealed class LoggerTests
    {
        private AssetObjectScope m_AssetScope;

        [SetUp]
        public void SetUp()
        {
            m_AssetScope = new AssetObjectScope();
        }

        [TearDown]
        public void TearDown()
        {
            m_AssetScope.Dispose();
        }

        [Test]
        public void NullLogger_SetCapacity_DoesNotThrow()
        {
            var asset = m_AssetScope.Asset;
            var system = new SessionStateUndoSystem("test.logger.null", 5);
            system.ClearAll();
            system.ActivateGroup("g", 10);
            system.Record(asset, "A", "B");
            system.Record(asset, "B", "C");
            system.Record(asset, "C", "D");

            Assert.DoesNotThrow(() => system.SetCapacity(2));
            system.ClearAll();
        }

        [Test]
        public void CustomLogger_ReceivesMessage_WhenCapacityTrimOccurs()
        {
            var asset = m_AssetScope.Asset;
            var logger = new TestLogger();
            var system = new SessionStateUndoSystem("test.logger.custom", 5);
            system.ClearAll();
            system.ActivateGroup("g", 10);
            system.Record(asset, "A", "B");
            system.Record(asset, "B", "C");
            system.Record(asset, "C", "D");

            system.SetCapacity(2);

            Assert.Greater(logger.Messages.Count, 0);
            system.ClearAll();
        }

        [Test]
        public void CustomLogger_ReceivesNoMessage_WhenNoTrimOccurs()
        {
            var asset = m_AssetScope.Asset;
            var logger = new TestLogger();
            var system = new SessionStateUndoSystem("test.logger.notrim", 5);
            system.ClearAll();
            system.ActivateGroup("g", 10);
            system.Record(asset, "A", "B");

            system.SetCapacity(3);

            Assert.AreEqual(0, logger.Messages.Count);
            system.ClearAll();
        }
    }
}

#endif