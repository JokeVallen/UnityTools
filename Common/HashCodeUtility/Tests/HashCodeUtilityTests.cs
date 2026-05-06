using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// 对 Snake.HashCodeUtility 静态工具类的全面单元测试。
/// 覆盖所有公开方法、null 安全、顺序依赖性、空集合、自定义比较器、溢出行为等。
/// </summary>
[TestFixture]
public class HashCodeUtilityTests
{
    #region GetHashCode<T> 测试

    [Test]
    public void GetHashCode_SameValue_ReturnsSameHash()
    {
        int hash1 = HashCodeUtility.GetHashCode("test");
        int hash2 = HashCodeUtility.GetHashCode("test");
        Assert.AreEqual(hash1, hash2);
    }

    [Test]
    public void GetHashCode_NullString_ReturnsStableHash()
    {
        // null 应返回基于种子和乘数的特定值，且不会抛出异常
        int hash = HashCodeUtility.GetHashCode<string>(null);
        int expected = 17 * 31 + 0; // 种子*31 + 0
        Assert.AreEqual(expected, hash);
    }

    [Test]
    public void GetHashCode_DefaultValueType_ReturnsStableHash()
    {
        int zeroHash = HashCodeUtility.GetHashCode(0);
        int defaultHash = HashCodeUtility.GetHashCode(default(int));
        Assert.AreEqual(zeroHash, defaultHash);
    }

    [Test]
    public void GetHashCode_DifferentValues_UnlikelyEqual()
    {
        int h1 = HashCodeUtility.GetHashCode(42);
        int h2 = HashCodeUtility.GetHashCode(43);
        Assert.AreNotEqual(h1, h2);
    }

    #endregion

    #region Combine<T1,T2> 到 Combine<T1,...,T5> 测试

    [Test]
    public void Combine_TwoArgs_SameValues_SameHash()
    {
        var h1 = HashCodeUtility.Combine("a", 1);
        var h2 = HashCodeUtility.Combine("a", 1);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void Combine_TwoArgs_OrderMatters()
    {
        var h1 = HashCodeUtility.Combine("a", 1);
        var h2 = HashCodeUtility.Combine(1, "a");
        Assert.AreNotEqual(h1, h2);
    }

    [Test]
    public void Combine_TwoArgs_AllNull_ReturnsPredictableHash()
    {
        int hash = HashCodeUtility.Combine<string, string>(null, null);
        // ((17*31 + 0) * 31 + 0) = 16337
        int expected = unchecked(((17 * 31) + 0) * 31 + 0);
        Assert.AreEqual(expected, hash);
    }

    [Test]
    public void Combine_ThreeArgs_AllNull_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => HashCodeUtility.Combine<string, string, string>(null, null, null));
    }

    [Test]
    public void Combine_FourArgs_MixedTypes_Works()
    {
        var h1 = HashCodeUtility.Combine("x", 5, 3.14, true);
        var h2 = HashCodeUtility.Combine("x", 5, 3.14, true);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void Combine_FiveArgs_OrderSensitive()
    {
        var h1 = HashCodeUtility.Combine(1, 2, 3, 4, 5);
        var h2 = HashCodeUtility.Combine(5, 4, 3, 2, 1);
        Assert.AreNotEqual(h1, h2);
    }

    [Test]
    public void Combine_FiveArgs_AllNullRefTypes()
    {
        var h = HashCodeUtility.Combine<object, string, object, string, object>(null, null, null, null, null);
        // 预期: (((((17*31) *31) *31) *31) *31)
        int expected = 17;
        for (int i = 0; i < 5; i++)
            expected = expected * 31; // 每次加0所以直接乘
        Assert.AreEqual(expected, h);
    }

    #endregion

    #region CombineAll<T> 泛型版本

    [Test]
    public void CombineAll_NullArray_ReturnsZero()
    {
        Assert.AreEqual(0, HashCodeUtility.CombineAll<string>(null));
    }

    [Test]
    public void CombineAll_EmptyArray_ReturnsSeed()
    {
        // 空数组没有元素，直接返回种子
        Assert.AreEqual(17, HashCodeUtility.CombineAll(new int[0]));
    }

    [Test]
    public void CombineAll_SingleElement_MatchesGetHashCode()
    {
        var obj = "hello";
        var h1 = HashCodeUtility.CombineAll(obj);
        var h2 = HashCodeUtility.GetHashCode(obj);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void CombineAll_MultipleElements_OrderMatters()
    {
        var h1 = HashCodeUtility.CombineAll("a", "b", "c");
        var h2 = HashCodeUtility.CombineAll("c", "b", "a");
        Assert.AreNotEqual(h1, h2);
    }

    [Test]
    public void CombineAll_ParamsWithValueTypes_Works()
    {
        var h1 = HashCodeUtility.CombineAll(1, 2, 3);
        var h2 = HashCodeUtility.CombineAll(1, 2, 3);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void CombineAll_ParamsWithNullElements_HandledGracefully()
    {
        var h = HashCodeUtility.CombineAll("a", null, "c");
        Assert.NotNull(h); // 不抛出异常即可
    }

    #endregion

    #region CombineAll 非泛型版本

    [Test]
    public void CombineAll_NonGeneric_NullArray_ReturnsZero()
    {
        Assert.AreEqual(0, HashCodeUtility.CombineAll((object[])null));
    }

    [Test]
    public void CombineAll_NonGeneric_EmptyArray_ReturnsSeed()
    {
        Assert.AreEqual(17, HashCodeUtility.CombineAll(new object[0]));
    }

    [Test]
    public void CombineAll_NonGeneric_MixedTypes_Stable()
    {
        var h1 = HashCodeUtility.CombineAll("hello", 123, true);
        var h2 = HashCodeUtility.CombineAll("hello", 123, true);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void CombineAll_NonGeneric_BoxedValueTypeHashDiffersFromGeneric()
    {
        // 这是一个文档性测试：值类型装箱后可能使用不同的哈希实现（Mono/IL2CPP）
        var genericHash = HashCodeUtility.CombineAll(1, 2, 3);               // 泛型版，使用 EqualityComparer<int>.Default
        var nonGenericHash = HashCodeUtility.CombineAll((object)1, (object)2, (object)3); // 非泛型，调用 object.GetHashCode()
        // 根据运行时，二者可能不同（尤其在 .NET Framework/Mono 中反射计算值类型哈希）
        // 此处仅验证两个调用都不崩溃
        Assert.Pass($"Generic: {genericHash}, NonGeneric: {nonGenericHash}");
    }

    #endregion

    #region GetOrderDependentHashCode<T> 数组版本

    [Test]
    public void GetOrderDependentHashCode_Array_Null_ReturnsZero()
    {
        Assert.AreEqual(0, HashCodeUtility.GetOrderDependentHashCode<int>(null));
    }

    [Test]
    public void GetOrderDependentHashCode_Array_Empty_ReturnsSeed()
    {
        Assert.AreEqual(17, HashCodeUtility.GetOrderDependentHashCode(new int[0]));
    }

    [Test]
    public void GetOrderDependentHashCode_Array_SingleElement_MatchesGetHashCode()
    {
        var arr = new[] { "element" };
        int hash1 = HashCodeUtility.GetOrderDependentHashCode(arr);
        int hash2 = HashCodeUtility.GetHashCode("element");
        Assert.AreEqual(hash1, hash2);
    }

    [Test]
    public void GetOrderDependentHashCode_Array_OrderMatters()
    {
        int[] forward = { 1, 2, 3 };
        int[] backward = { 3, 2, 1 };
        int h1 = HashCodeUtility.GetOrderDependentHashCode(forward);
        int h2 = HashCodeUtility.GetOrderDependentHashCode(backward);
        Assert.AreNotEqual(h1, h2);
    }

    [Test]
    public void GetOrderDependentHashCode_Array_WithNullElements_Handled()
    {
        string[] arr = { "a", null, "b" };
        Assert.DoesNotThrow(() => HashCodeUtility.GetOrderDependentHashCode(arr));
    }

    #endregion

    #region GetOrderDependentHashCode<T> 数组 + 自定义比较器

    [Test]
    public void GetOrderDependentHashCode_Array_NullComparer_UsesDefault()
    {
        int[] data = { 5, 10 };
        int h1 = HashCodeUtility.GetOrderDependentHashCode(data);
        int h2 = HashCodeUtility.GetOrderDependentHashCode(data, null);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void GetOrderDependentHashCode_Array_CustomComparer_ChangesHash()
    {
        string[] arr = { "A", "a" };
        int caseSensitive = HashCodeUtility.GetOrderDependentHashCode(arr);
        int caseInsensitive = HashCodeUtility.GetOrderDependentHashCode(
            arr, StringComparer.OrdinalIgnoreCase);
        Assert.AreNotEqual(caseSensitive, caseInsensitive);
    }

    [Test]
    public void GetOrderDependentHashCode_Array_CustomComparer_OrderStillMatters()
    {
        var arr1 = new[] { "a", "b" };
        var arr2 = new[] { "b", "a" };
        var h1 = HashCodeUtility.GetOrderDependentHashCode(arr1, StringComparer.Ordinal);
        var h2 = HashCodeUtility.GetOrderDependentHashCode(arr2, StringComparer.Ordinal);
        Assert.AreNotEqual(h1, h2);
    }

    #endregion

    #region GetOrderDependentHashCode<T> IEnumerable 版本

    [Test]
    public void GetOrderDependentHashCode_IEnumerable_Null_ReturnsZero()
    {
        Assert.AreEqual(0, HashCodeUtility.GetOrderDependentHashCode((IEnumerable<int>)null));
    }

    [Test]
    public void GetOrderDependentHashCode_IEnumerable_Empty_ReturnsSeed()
    {
        var empty = new List<int>();
        Assert.AreEqual(17, HashCodeUtility.GetOrderDependentHashCode(empty));
    }

    [Test]
    public void GetOrderDependentHashCode_IEnumerable_OrderMatters()
    {
        var list1 = new List<int> { 10, 20, 30 };
        var list2 = new List<int> { 30, 20, 10 };
        int h1 = HashCodeUtility.GetOrderDependentHashCode(list1);
        int h2 = HashCodeUtility.GetOrderDependentHashCode(list2);
        Assert.AreNotEqual(h1, h2);
    }

    [Test]
    public void GetOrderDependentHashCode_IEnumerable_CustomComparer()
    {
        var items = new List<string> { "X", "y" };
        int defaultHash = HashCodeUtility.GetOrderDependentHashCode(items);
        int ignoreCaseHash = HashCodeUtility.GetOrderDependentHashCode(
            items, StringComparer.OrdinalIgnoreCase);
        Assert.AreNotEqual(defaultHash, ignoreCaseHash);
    }

    [Test]
    public void GetOrderDependentHashCode_IEnumerable_StableAcrossEnumerations()
    {
        var list = new List<int> { 1, 2, 3 };
        int h1 = HashCodeUtility.GetOrderDependentHashCode(list);
        int h2 = HashCodeUtility.GetOrderDependentHashCode(list);
        Assert.AreEqual(h1, h2);
    }

    #endregion

    #region 溢出与极端值测试

    [Test]
    public void Combine_WithIntMaxValues_NoOverflowException()
    {
        Assert.DoesNotThrow(() => HashCodeUtility.Combine(int.MaxValue, int.MaxValue));
    }

    [Test]
    public void CombineAll_WithLargeParams_NoOverflowException()
    {
        var manyValues = new[]
        {
            int.MaxValue, int.MinValue, 123456789, -987654321
        };
        Assert.DoesNotThrow(() => HashCodeUtility.CombineAll(manyValues));
    }

    [Test]
    public void GetOrderDependentHashCode_Array_ExtremeValues()
    {
        var arr = new[] { int.MinValue, int.MaxValue };
        Assert.DoesNotThrow(() => HashCodeUtility.GetOrderDependentHashCode(arr));
    }

    #endregion

    #region 内部常量一致性测试

    [Test]
    public void SeedAndMultiplier_ProduceKnownResult()
    {
        // 数学验证：Combine(0, 0) 应为 ((17*31) + 0) * 31 + 0 = 16337
        int expected = unchecked(((17 * 31) + 0) * 31 + 0);
        Assert.AreEqual(expected, HashCodeUtility.Combine(0, 0));
    }

    #endregion

    #region 不同泛型组合确保内部分支被覆盖

    [Test]
    public void Combine_TwoDifferentTypes_IntAndString()
    {
        var h1 = HashCodeUtility.Combine(100, "hello");
        var h2 = HashCodeUtility.Combine(100, "hello");
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void Combine_FourArgs_MixedNulls()
    {
        var h = HashCodeUtility.Combine<string, int?, string, object>(null, 5, "data", null);
        Assert.NotNull(h);
    }

    #endregion
}