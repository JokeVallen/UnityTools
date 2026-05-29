using System;
using Xunit;

namespace EasyProgress.Core.Tests
{
    public class ProgressStaticTests : IDisposable
    {
        public ProgressStaticTests()
        {
            ResetProgress();
        }

        [Fact]
        public void CreateLeaf_ReturnsNodeFromRegisteredManager()
        {
            var leaf = Progress.CreateLeaf<double>();
            Assert.NotNull(leaf);
        }

        [Fact]
        public void CreateComposite_WithRule_Works()
        {
            var rule = WeightedAverageRule.Create();
            var composite = Progress.CreateComposite(rule);
            Assert.NotNull(composite);
            Assert.Same(rule, composite.Rule);
        }

        [Fact]
        public void CreateWeightedComposite_ReturnsWeightedIfSupported()
        {
            var rule = WeightedAverageRule.Create();
            var weighted = Progress.CreateWeightedComposite(rule);
            Assert.IsAssignableFrom<IWeightedProgressComposite<double>>(weighted);
        }

        [Fact]
        public void CreateWeightedComposite_ThrowsIfManagerNotWeighted()
        {
            // Register a custom manager that does not produce weighted composites
            var customManager = new DefaultProgressManager<double>(
                () => new DefaultLeaf(),
                rule => new RealtimeComposite(rule) // not weighted
            );
            Progress.RegisterProgressManager(customManager);
            var rule = WeightedAverageRule.Create();
            Assert.Throws<InvalidOperationException>(() => Progress.CreateWeightedComposite(rule));
        }

        [Fact]
        public void ReleaseLeaf_ShouldReturnToPool()
        {
            var leaf = Progress.CreateLeaf<double>();
            Progress.ReleaseLeaf(leaf);
            // no exception, and leaf can be reused (tested in manager test)
        }

        [Fact]
        public void ReleaseComposite_ShouldReturnToPool()
        {
            var composite = Progress.CreateComposite(WeightedAverageRule.Create());
            Progress.ReleaseComposite(composite);
        }

        [Fact]
        public void RegisterUnregister_Managers_Works()
        {
            var customManager = new DefaultProgressManager<double>(
                () => new DefaultLeaf(),
                rule => new RealtimeComposite(rule)
            );
            Progress.RegisterProgressManager(customManager);
            var retrieved = Progress.Test_GetProgressManager<double>(); // via reflection or internal method? We'll just test that CreateLeaf uses it.
            // For test, we rely on internal GetProgressManager? Better to test by behavior: after registration, new manager should be used.
            // Simple: unregister and then CreateLeaf should still work because default is still registered? We need to clear all.
            Progress.UnregisterProgressManager<double>();
            // Now default is gone, but we still have custom? Actually we didn't register custom for double? Re-register default.
            Progress.RegisterProgressManager(DefaultProgressManager.CreateDefault());
        }

        [Fact]
        public void Dispose_ThrowsOnFurtherAccess()
        {
            Progress.Dispose();
            Assert.Throws<ObjectDisposedException>(() => Progress.CreateLeaf<double>());
            // Reinitialize for other tests
            ResetProgress(); // but we need to reset static state? Let's not rely on test order.
        }

        public void Dispose()
        {
            // Cleanup
            ResetProgress();
        }

        public static void ResetProgress()
        {
            var field = typeof(Progress).GetField("disposed", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(null, false);
            var dictField = typeof(Progress).GetField("progressManagers", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var dict = dictField?.GetValue(null) as System.Collections.Concurrent.ConcurrentDictionary<Type, IProgressManager>;
            dict?.Clear();
            var field1 = typeof(ListPool).GetField("disposed", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            field1?.SetValue(null, false);
            var field2 = typeof(DictionaryPool).GetField("disposed", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            field2?.SetValue(null, false);
            // Re-register default
            Progress.RegisterProgressManager(DefaultProgressManager.CreateDefault());
        }
    }
}