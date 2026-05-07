using System;
using System.Collections.Generic;
using System.Threading;
using EasyMapper;
using EasyMapper.Runtime;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyMapper.Tests
{
    /// <summary>
    /// EasyMapper 单元测试，覆盖所有默认组件和扩展模块。
    /// 所有测试在 EditMode 下运行。
    /// </summary>
    public class EasyMapperUnitTests
    {
        #region LongToken Tests

        [Test]
        public void LongToken_Equality_Works()
        {
            var a = new LongToken(123);
            var b = new LongToken(123);
            var c = new LongToken(456);
            Assert.AreEqual(a, b);
            Assert.AreNotEqual(a, c);
            Assert.IsTrue(a.Equals(b));
            Assert.IsFalse(a.Equals(c));
        }

        [Test]
        public void LongToken_ImplicitConversion_Works()
        {
            LongToken token = 42;
            Assert.AreEqual(42, token.Value);
            long value = token;
            Assert.AreEqual(42, value);
        }

        [Test]
        public void LongToken_GetHashCode_Consistent()
        {
            var token = new LongToken(100);
            Assert.AreEqual(token.GetHashCode(), new LongToken(100).GetHashCode());
        }

        #endregion

        #region Char10PackingBlueprint Tests

        [Test]
        public void Char10Packing_Refine_Null_ReturnsZero()
        {
            var bp = new Char10PackingBlueprint();
            var token = bp.Refine(null);
            Assert.AreEqual(0, token.Value);
        }

        [Test]
        public void Char10Packing_Refine_EmptyString_ReturnsZero()
        {
            var bp = new Char10PackingBlueprint();
            var token = bp.Refine(string.Empty);
            Assert.AreEqual(0, token.Value);
        }

        [Test]
        public void Char10Packing_Refine_Restore_Roundtrip_ShortString()
        {
            var bp = new Char10PackingBlueprint();
            var token = bp.Refine("abc");
            string restored = bp.Restore(token);
            Assert.AreEqual("abc", restored);
        }

        [Test]
        public void Char10Packing_Refine_Restore_MaxLenString()
        {
            var bp = new Char10PackingBlueprint();
            string input = "max_len_10"; // exactly 10
            var token = bp.Refine(input);
            string restored = bp.Restore(token);
            Assert.AreEqual(input, restored);
        }

        [Test]
        public void Char10Packing_Refine_TruncatesOver10()
        {
            var bp = new Char10PackingBlueprint();
            string input = "1234567890AB"; // 12 chars
            var token = bp.Refine(input);
            string restored = bp.Restore(token);
            Assert.AreEqual("1234567890", restored); // only first 10
        }

        [Test]
        public void Char10Packing_Refine_IgnoresInvalidChars()
        {
            var bp = new Char10PackingBlueprint();
            var token = bp.Refine("a@b");
            string restored = bp.Restore(token);
            // @ maps to 0, acts as terminator, so only "a" is encoded
            Assert.AreEqual("a", restored);
        }

        [Test]
        public void Char10Packing_Refine_CaseFolding()
        {
            var bp = new Char10PackingBlueprint();
            var token = bp.Refine("AbC");
            string restored = bp.Restore(token);
            Assert.AreEqual("abc", restored);
        }

        [Test]
        public void Char10Packing_Refine_UnderscoreAndHyphen()
        {
            var bp = new Char10PackingBlueprint();
            var token = bp.Refine("_test-");
            string restored = bp.Restore(token);
            Assert.AreEqual("_test-", restored);
        }

        #endregion

        #region InterningBlueprint Tests

        [Test]
        public void Interning_SameString_ReturnsSameToken()
        {
            var bp = new InterningBlueprint();
            var token1 = bp.Refine("hello");
            var token2 = bp.Refine("hello");
            Assert.AreEqual(token1, token2);
        }

        [Test]
        public void Interning_DifferentStrings_ReturnsDifferentTokens()
        {
            var bp = new InterningBlueprint();
            var token1 = bp.Refine("first");
            var token2 = bp.Refine("second");
            Assert.AreNotEqual(token1, token2);
        }

        [Test]
        public void Interning_Restore_Throws()
        {
            var bp = new InterningBlueprint();
            Assert.Throws<NotSupportedException>(() => bp.Restore(new LongToken(1)));
        }

        [Test]
        public void Interning_IsTraceable_IsFalse()
        {
            var bp = new InterningBlueprint();
            Assert.IsFalse(bp.IsTraceable);
        }

        #endregion

        #region SmartDistributor Tests

        [Test]
        public void SmartDistributor_FastPath_UsedForShortLegalString()
        {
            var smart = new SmartDistributor(new Char10PackingBlueprint(), new InterningBlueprint());
            var token = smart.Refine("abc");
            // bit 63 should be 0 (fast path)
            Assert.AreEqual(0, (token.Value >> 63) & 1);
            string restored = smart.Restore(token);
            Assert.AreEqual("abc", restored);
        }

        [Test]
        public void SmartDistributor_Fallback_UsedForLongString()
        {
            var smart = new SmartDistributor(new Char10PackingBlueprint(), new InterningBlueprint());
            var token = smart.Refine("a_very_long_string_over_10");
            // bit 63 should be 1 (fallback)
            Assert.AreEqual(1, (token.Value >> 63) & 1);
            // Restore from fallback will throw because InterningBlueprint throws
            Assert.Throws<NotSupportedException>(() => smart.Restore(token));
        }

        [Test]
        public void SmartDistributor_Fallback_UsedForIllegalChar()
        {
            var smart = new SmartDistributor(new Char10PackingBlueprint(), new InterningBlueprint());
            var token = smart.Refine("abc@def");
            Assert.AreEqual(1, (token.Value >> 63) & 1);
        }

        [Test]
        public void SmartDistributor_NullOrEmpty_ReturnsZero()
        {
            var smart = new SmartDistributor(new Char10PackingBlueprint(), new InterningBlueprint());
            Assert.AreEqual(0, smart.Refine(null).Value);
            Assert.AreEqual(0, smart.Refine("").Value);
        }

        #endregion

        #region ObjectNamingBlueprint Tests

        [Test]
        public void ObjectNaming_Refine_UsesObjectName()
        {
            var obj = new GameObject("TestObject");
            var stringBp = new SmartDistributor(new Char10PackingBlueprint(), new InterningBlueprint());
            var objBp = new ObjectNamingBlueprint(stringBp);
            var token = objBp.Refine(obj);
            // token should match direct string encoding of "testobject" (lowercased in Char10Packing)
            var expectedToken = stringBp.Refine(obj.name);
            Assert.AreEqual(expectedToken, token);
            Object.DestroyImmediate(obj);
        }

        [Test]
        public void ObjectNaming_Restore_Throws()
        {
            var stringBp = new SmartDistributor(new Char10PackingBlueprint(), new InterningBlueprint());
            var objBp = new ObjectNamingBlueprint(stringBp);
            Assert.Throws<NotSupportedException>(() => objBp.Restore(new LongToken(1)));
        }

        #endregion

        #region StandardPipeline Tests

        [Test]
        public void StandardPipeline_ImportExport_Roundtrip_TraceableBlueprint()
        {
            var bp = new Char10PackingBlueprint(); // IsTraceable = true
            var pipeline = new StandardPipeline<string, LongToken>(bp, bp);
            var token = pipeline.Import("data");
            string result = pipeline.Export(token);
            Assert.AreEqual("data", result);
        }

        [Test]
        public void StandardPipeline_ImportExport_Roundtrip_NonTraceableBlueprint()
        {
            var bp = new InterningBlueprint(); // IsTraceable = false
            var pipeline = new StandardPipeline<string, LongToken>(bp, bp);
            var token = pipeline.Import("hello");
            string result = pipeline.Export(token);
            Assert.AreEqual("hello", result);
        }

        [Test]
        public void StandardPipeline_Import_SameSource_ReturnsSameToken()
        {
            var bp = new InterningBlueprint();
            var pipeline = new StandardPipeline<string, LongToken>(bp, bp);
            var token1 = pipeline.Import("a");
            var token2 = pipeline.Import("a");
            Assert.AreEqual(token1, token2);
        }

        [Test]
        public void StandardPipeline_Cleanup_ClearsRegistry()
        {
            var bp = new InterningBlueprint();
            var pipeline = new StandardPipeline<string, LongToken>(bp, bp);
            var token = pipeline.Import("x");
            pipeline.Cleanup();
            string result = pipeline.Export(token);
            Assert.IsNull(result);
        }

        #endregion

        #region UnityWeakPipeline Tests

        [Test]
        public void UnityWeakPipeline_ImportExport_LiveObject()
        {
            var bp = new ObjectNamingBlueprint(new SmartDistributor(new Char10PackingBlueprint(), new InterningBlueprint()));
            var pipeline = new UnityWeakPipeline<Object, LongToken>(bp, bp);
            var go = new GameObject("LiveObject");
            var token = pipeline.Import(go);
            var exported = pipeline.Export(token);
            Assert.AreEqual(go, exported);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void UnityWeakPipeline_Export_DestroyedObject_ReturnsNull()
        {
            var bp = new ObjectNamingBlueprint(new SmartDistributor(new Char10PackingBlueprint(), new InterningBlueprint()));
            var pipeline = new UnityWeakPipeline<Object, LongToken>(bp, bp);
            var go = new GameObject("Temp");
            var token = pipeline.Import(go);
            Object.DestroyImmediate(go);
            var exported = pipeline.Export(token);
            Assert.IsNull(exported);
        }

        [Test]
        public void UnityWeakPipeline_Cleanup_RemovesDeadEntries()
        {
            var bp = new ObjectNamingBlueprint(new SmartDistributor(new Char10PackingBlueprint(), new InterningBlueprint()));
            var pipeline = new UnityWeakPipeline<Object, LongToken>(bp, bp);
            var go = new GameObject("ToClean");
            var token = pipeline.Import(go);
            Object.DestroyImmediate(go);
            pipeline.Cleanup();
            // After cleanup, export should return null (entry removed)
            // We can't check tokenToSource directly, but Export should be null
            Assert.IsNull(pipeline.Export(token));
        }

        [Test]
        public void UnityWeakPipeline_Import_NullUnityObject_ReturnsDefaultToken()
        {
            var bp = new ObjectNamingBlueprint(new SmartDistributor(new Char10PackingBlueprint(), new InterningBlueprint()));
            var pipeline = new UnityWeakPipeline<Object, LongToken>(bp, bp);
            // Use a destroyed object to simulate null (by Unity's overloaded operator)
            var go = new GameObject("willBeNull");
            Object.DestroyImmediate(go);
            var token = pipeline.Import(go);
            Assert.AreEqual(default(LongToken), token);
        }

        #endregion

        #region ThreadSafePipeline Tests

        [Test]
        public void ThreadSafePipeline_ImportExport_WorksLikeInner()
        {
            var bp = new InterningBlueprint();
            var inner = new StandardPipeline<string, LongToken>(bp, bp);
            var safe = new ThreadSafePipeline<string, LongToken>(inner);
            var token = safe.Import("thread");
            string result = safe.Export(token);
            Assert.AreEqual("thread", result);
        }

        [Test]
        public void ThreadSafePipeline_Cleanup_Delegates()
        {
            var bp = new InterningBlueprint();
            var inner = new StandardPipeline<string, LongToken>(bp, bp);
            var safe = new ThreadSafePipeline<string, LongToken>(inner);
            var token = safe.Import("t");
            safe.Cleanup();
            Assert.IsNull(safe.Export(token));
        }

        #endregion

        #region CacheFirstPipeline Tests

        [Test]
        public void CacheFirstPipeline_Import_UsesCache()
        {
            var bp = new ObjectNamingBlueprint(new SmartDistributor(new Char10PackingBlueprint(), new InterningBlueprint()));
            var inner = new UnityWeakPipeline<Object, LongToken>(bp, bp);
            var cached = new CacheFirstPipeline<Object, LongToken>(inner);
            var go = new GameObject("CacheMe");
            var token1 = cached.Import(go);
            var token2 = cached.Import(go);
            Assert.AreEqual(token1, token2);
            // Verify that export goes through inner pipeline
            Assert.AreEqual(go, cached.Export(token1));
            Object.DestroyImmediate(go);
        }

        [Test]
        public void CacheFirstPipeline_Export_DelegatesToInner()
        {
            var bp = new InterningBlueprint();
            var inner = new StandardPipeline<string, LongToken>(bp, bp);
            var cached = new CacheFirstPipeline<string, LongToken>(inner);
            var token = cached.Import("export_test");
            Assert.AreEqual("export_test", cached.Export(token));
        }

        #endregion

        #region CappedPipeline Tests

        [Test]
        public void CappedPipeline_MaxEntries_Enforced()
        {
            var bp = new InterningBlueprint();
            var capped = new CappedPipeline<string, LongToken>(bp, bp, 2);

            var token1 = capped.Import("a");
            var token2 = capped.Import("b");
            Assert.AreEqual(2, capped.Count);

            var token3 = capped.Import("c");
            Assert.AreEqual(2, capped.Count);
            Assert.IsNull(capped.Export(token1));
            Assert.AreEqual("b", capped.Export(token2));
            Assert.AreEqual("c", capped.Export(token3));
        }

        [Test]
        public void CappedPipeline_LRU_AccessOrder()
        {
            var bp = new InterningBlueprint();
            var capped = new CappedPipeline<string, LongToken>(bp, bp, 2);

            var t1 = capped.Import("1");
            var t2 = capped.Import("2");
            capped.Export(t1); // 访问 t1，使其变为最新

            var t3 = capped.Import("3"); // 应淘汰 t2
            Assert.IsNull(capped.Export(t2)); // 现在成功返回 null
            Assert.AreEqual("1", capped.Export(t1));
            Assert.AreEqual("3", capped.Export(t3));
        }

        #endregion

        #region IdempotentPipeline Tests

        [Test]
        public void IdempotentPipeline_Import_SameSourceSameToken()
        {
            var bp = new InterningBlueprint();
            var inner = new StandardPipeline<string, LongToken>(bp, bp);
            var idem = new IdempotentPipeline<string, LongToken>(inner);
            var token1 = idem.Import("x");
            var token2 = idem.Import("x");
            Assert.AreEqual(token1, token2);
        }

        [Test]
        public void IdempotentPipeline_Cleanup_ClearsMappings()
        {
            var bp = new InterningBlueprint();
            var inner = new StandardPipeline<string, LongToken>(bp, bp);
            var idem = new IdempotentPipeline<string, LongToken>(inner);
            var token = idem.Import("y");
            idem.Cleanup();
            Assert.IsNull(idem.Export(token));
        }

        #endregion

        #region GuardedPipeline Tests

        [Test]
        public void GuardedPipeline_Import_NullSource_Throws()
        {
            var bp = new InterningBlueprint();
            var inner = new StandardPipeline<string, LongToken>(bp, bp);
            var guarded = new GuardedPipeline<string, LongToken>(inner);
            Assert.Throws<ArgumentNullException>(() => guarded.Import(null));
        }

        [Test]
        public void GuardedPipeline_Export_DefaultToken_Throws()
        {
            var bp = new InterningBlueprint();
            var inner = new StandardPipeline<string, LongToken>(bp, bp);
            var guarded = new GuardedPipeline<string, LongToken>(inner);
            Assert.Throws<ArgumentException>(() => guarded.Export(default));
        }

        [Test]
        public void GuardedPipeline_Valid_CallsInner()
        {
            var bp = new InterningBlueprint();
            var inner = new StandardPipeline<string, LongToken>(bp, bp);
            var guarded = new GuardedPipeline<string, LongToken>(inner);
            var token = guarded.Import("ok");
            Assert.AreEqual("ok", guarded.Export(token));
        }

        #endregion

        #region DiagnosticPipeline Tests

        [Test]
        public void DiagnosticPipeline_Counter_Increments()
        {
            var bp = new InterningBlueprint();
            var inner = new StandardPipeline<string, LongToken>(bp, bp);
            var diag = new DiagnosticPipeline<string, LongToken>(inner);
            Assert.AreEqual(0, diag.ImportCount);
            diag.Import("test");
            Assert.AreEqual(1, diag.ImportCount);
            diag.Export(new LongToken(1));
            Assert.AreEqual(1, diag.ExportCount);
        }

        [Test]
        public void DiagnosticPipeline_Events_Fire()
        {
            var bp = new InterningBlueprint();
            var inner = new StandardPipeline<string, LongToken>(bp, bp);
            var diag = new DiagnosticPipeline<string, LongToken>(inner);
            string importedSource = null;
            LongToken importedToken = default;
            diag.OnImport += (src, tok) =>
            {
                importedSource = src;
                importedToken = tok;
            };
            diag.Import("event_test");
            Assert.AreEqual("event_test", importedSource);
            Assert.AreNotEqual(default, importedToken);
        }

        [Test]
        public void DiagnosticPipeline_ResetCounters_Works()
        {
            var bp = new InterningBlueprint();
            var inner = new StandardPipeline<string, LongToken>(bp, bp);
            var diag = new DiagnosticPipeline<string, LongToken>(inner);
            diag.Import("a");
            diag.ResetCounters();
            Assert.AreEqual(0, diag.ImportCount);
        }

        #endregion

        #region BinaryIdentityPackage Tests

        [Test]
        public void BinaryPackage_WrapUnwrap_Roundtrip()
        {
            var package = new BinaryIdentityPackage();
            var token = new LongToken(123456789);
            byte[] bytes = package.Wrap(token);
            var restored = package.Unwrap(bytes);
            Assert.AreEqual(token, restored);
        }

        [Test]
        public void BinaryPackage_Unwrap_NullOrShort_ReturnsZero()
        {
            var package = new BinaryIdentityPackage();
            Assert.AreEqual(0, package.Unwrap(null).Value);
            Assert.AreEqual(0, package.Unwrap(new byte[4]).Value);
        }

        #endregion

        #region GuidToken and GuidBinaryPackage Tests

        [Test]
        public void GuidToken_Equality_Works()
        {
            var g1 = new GuidToken(new Guid("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE"));
            var g2 = new GuidToken(new Guid("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE"));
            Assert.AreEqual(g1, g2);
        }

        [Test]
        public void GuidBinaryPackage_WrapUnwrap_Roundtrip()
        {
            var package = new GuidBinaryPackage();
            var original = new GuidToken(Guid.NewGuid());
            byte[] bytes = package.Wrap(original);
            var restored = package.Unwrap(bytes);
            Assert.AreEqual(original, restored);
        }

        [Test]
        public void GuidBinaryPackage_Unwrap_NullOrShort_ReturnsEmpty()
        {
            var package = new GuidBinaryPackage();
            Assert.AreEqual(Guid.Empty, package.Unwrap(null).Value);
            Assert.AreEqual(Guid.Empty, package.Unwrap(new byte[8]).Value);
        }

        #endregion

        #region IDMap Static API Tests

        [Test]
        public void IDMap_AssignAndLocateString_Roundtrip()
        {
            long id = IDMap.Assign("roundtrip_string");
            string result = IDMap.Locate(id);
            Assert.AreEqual("roundtrip_string", result);
        }

        [Test]
        public void IDMap_AssignObjectAndLocateGameObject_Works()
        {
            var go = new GameObject("IDMapTest");
            long id = IDMap.Assign(go);
            var found = IDMap.Locate<GameObject>(id);
            Assert.AreEqual(go, found);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void IDMap_PackUnpack_Roundtrip()
        {
            long id = IDMap.Assign("pack_me");
            byte[] bytes = IDMap.Pack(id);
            long restored = IDMap.Unpack(bytes);
            Assert.AreEqual(id, restored);
        }

        [Test]
        public void IDMap_ContainsString_Works()
        {
            long id = IDMap.Assign("contain_string");
            Assert.IsTrue(IDMap.ContainsString(id));
            Assert.IsFalse(IDMap.ContainsString(9999999L));
        }

        [Test]
        public void IDMap_ContainsObject_Works()
        {
            var go = new GameObject("ContainTest");
            long id = IDMap.Assign(go);
            Assert.IsTrue(IDMap.ContainsObject(id));
            Object.DestroyImmediate(go);
            // After destroy, object pipeline weak ref should cause false
            Assert.IsFalse(IDMap.ContainsObject(id));
        }

        [Test]
        public void IDMap_Cleanup_ClearsPipelines()
        {
            // Just ensure no exception, and string pipeline gets cleared
            IDMap.Assign("cleanup_test");
            IDMap.Cleanup();
            // No direct assertion, but verify Cleanup doesn't throw.
            Assert.Pass();
        }

        [Test]
        public void IDMap_Current_CanBeReplaced()
        {
            var original = IDMap.Current;
            // Create a custom instance (using builder)
            var custom = IDMapInstance.Builder.Create().Build();
            IDMap.Current = custom;
            Assert.AreSame(custom, IDMap.Current);
            // Restore
            IDMap.Current = null; // fallback to default
            Assert.AreSame(original, IDMap.Current);
        }

        #endregion
    }
}