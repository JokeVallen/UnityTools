using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

public class ComparerUtilityTests
{
    // ------- 辅助类型 --------
    private class MyIntComparer : IEqualityComparer<int>, IComparer<int>, IEqualityComparer, IComparer
    {
        public bool Equals(int x, int y) => x == y;
        public int GetHashCode(int obj) => obj.GetHashCode();
        public int Compare(int x, int y) => x.CompareTo(y);
        bool IEqualityComparer.Equals(object x, object y) => Equals((int)x, (int)y);
        int IEqualityComparer.GetHashCode(object obj) => GetHashCode((int)obj);
        int IComparer.Compare(object x, object y) => Compare((int)x, (int)y);
    }

    private class MyStringComparer : IEqualityComparer<string>, IComparer<string>
    {
        private readonly StringComparison _comparison;
        public MyStringComparer(StringComparison comparison) => _comparison = comparison;
        public bool Equals(string x, string y) => string.Equals(x, y, _comparison);
        public int GetHashCode(string obj) => obj?.GetHashCode() ?? 0;
        public int Compare(string x, string y) => string.Compare(x, y, _comparison);
    }

    private class MyNonGenericComparer : IEqualityComparer, IComparer
    {
        public new bool Equals(object x, object y) => x?.Equals(y) ?? y == null;
        public int GetHashCode(object obj) => obj?.GetHashCode() ?? 0;
        public int Compare(object x, object y) => ((IComparable)x)?.CompareTo(y) ?? -1;
    }

    [SetUp]
    public void SetUp()
    {
        ComparerUtility.ClearAll();
    }

    // ============================================================
    // 第一部分：IEqualityComparer 测试
    // ============================================================

    [Test]
    public void GetEqualityComparer_KeyNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ComparerUtility.GetEqualityComparer<int, string>(null));
    }

    [Test]
    public void GetEqualityComparer_NotRegistered_ReturnsNull()
    {
        var result = ComparerUtility.GetEqualityComparer<int, string>("key1");
        Assert.IsNull(result);
    }

    [Test]
    public void GetEqualityComparerOrDefault_NotRegistered_ReturnsDefault()
    {
        var result = ComparerUtility.GetEqualityComparerOrDefault<int, string>("key1");
        Assert.AreEqual(EqualityComparer<int>.Default, result);
    }

    [Test]
    public void SetAndGetEqualityComparer_Generic_Works()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetEqualityComparer<int, string>("key1", comparer);
        var result = ComparerUtility.GetEqualityComparer<int, string>("key1");
        Assert.AreSame(comparer, result);
    }

    [Test]
    public void SetAndGetEqualityComparer_NonGeneric_Works()
    {
        var comparer = new MyNonGenericComparer();
        ComparerUtility.SetEqualityComparer<string>("key1", comparer);
        var result = ComparerUtility.GetEqualityComparer<string>("key1", typeof(MyNonGenericComparer));
        Assert.AreSame(comparer, result);
    }

    [Test]
    public void GenericSet_SyncsToNonGenericStorage_WhenComparerImplementsIEqualityComparer()
    {
        var comparer = new MyIntComparer(); // implements IEqualityComparer
        ComparerUtility.SetEqualityComparer<int, string>("key1", comparer);
        var result = ComparerUtility.GetEqualityComparer<string>("key1", typeof(MyIntComparer));
        Assert.AreSame(comparer, result);
    }

    [Test]
    public void GetEqualityComparer_NonGeneric_TypeMismatch_Throws()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetEqualityComparer<int, string>("key1", comparer);
        Assert.Throws<InvalidCastException>(() =>
            ComparerUtility.GetEqualityComparer<string>("key1", typeof(string))
        );
    }

    [Test]
    public void GetEqualityComparer_NonGeneric_TypeMatches_Returns()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetEqualityComparer<int, string>("key1", comparer);
        var result = ComparerUtility.GetEqualityComparer<string>("key1", typeof(MyIntComparer));
        Assert.AreSame(comparer, result);
    }

    [Test]
    public void GetEqualityComparerOrDefault_NonGeneric_StorageMissing_ReturnsDefault()
    {
        var result = ComparerUtility.GetEqualityComparerOrDefault<string>("key1", typeof(int));
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<IEqualityComparer>(result);
    }

    [Test]
    public void RemoveEqualityComparer_Generic_RemovesBothStorages()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetEqualityComparer<int, string>("key1", comparer);
        var removed = ComparerUtility.RemoveEqualityComparer<int, string>("key1");
        Assert.IsTrue(removed);
        Assert.IsNull(ComparerUtility.GetEqualityComparer<int, string>("key1"));
    }

    [Test]
    public void RemoveEqualityComparer_NonGeneric_OnlyRemovesNonGenericStorage()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetEqualityComparer<int, string>("key1", comparer);
        var removed = ComparerUtility.RemoveEqualityComparer<string>("key1");
        Assert.IsTrue(removed);
        Assert.IsNotNull(ComparerUtility.GetEqualityComparer<int, string>("key1"));
        Assert.IsNull(ComparerUtility.GetEqualityComparer<string>("key1", typeof(MyIntComparer)));
    }

    [Test]
    public void ClearAll_ResetsEverything_Equality()
    {
        ComparerUtility.SetEqualityComparer<int, string>("key1", new MyIntComparer());
        var _ = ComparerUtility.GetEqualityComparerOrDefault<string>("any", typeof(int));
        ComparerUtility.ClearAll();
        Assert.IsNull(ComparerUtility.GetEqualityComparer<int, string>("key1"));
        Assert.IsNotNull(ComparerUtility.GetEqualityComparerOrDefault<string>("any", typeof(int))); // 缓存已清但重新生成
    }

    // ============================================================
    // 第二部分：IComparer 测试（完全对称，全部覆盖）
    // ============================================================

    [Test]
    public void GetComparer_KeyNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ComparerUtility.GetComparer<int, string>(null));
    }

    [Test]
    public void GetComparer_NotRegistered_ReturnsNull()
    {
        var result = ComparerUtility.GetComparer<int, string>("key1");
        Assert.IsNull(result);
    }

    [Test]
    public void GetComparerOrDefault_NotRegistered_ReturnsDefault()
    {
        var result = ComparerUtility.GetComparerOrDefault<int, string>("key1");
        Assert.AreEqual(Comparer<int>.Default, result);
    }

    [Test]
    public void SetAndGetComparer_Generic_Works()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetComparer<int, string>("key1", comparer);
        var result = ComparerUtility.GetComparer<int, string>("key1");
        Assert.AreSame(comparer, result);
    }

    [Test]
    public void SetAndGetComparer_NonGeneric_Works()
    {
        var comparer = new MyNonGenericComparer();
        ComparerUtility.SetComparer<string>("key1", comparer);
        var result = ComparerUtility.GetComparer<string>("key1", typeof(MyNonGenericComparer));
        Assert.AreSame(comparer, result);
    }

    [Test]
    public void GenericSet_SyncsToNonGenericStorage_WhenComparerImplementsIComparer()
    {
        var comparer = new MyIntComparer(); // implements IComparer
        ComparerUtility.SetComparer<int, string>("key1", comparer);
        var result = ComparerUtility.GetComparer<string>("key1", typeof(MyIntComparer));
        Assert.AreSame(comparer, result);
    }

    [Test]
    public void GetComparer_NonGeneric_TypeMismatch_Throws()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetComparer<int, string>("key1", comparer);
        Assert.Throws<InvalidCastException>(() =>
            ComparerUtility.GetComparer<string>("key1", typeof(string))
        );
    }

    [Test]
    public void GetComparer_NonGeneric_TypeMatches_Returns()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetComparer<int, string>("key1", comparer);
        var result = ComparerUtility.GetComparer<string>("key1", typeof(MyIntComparer));
        Assert.AreSame(comparer, result);
    }

    [Test]
    public void GetComparerOrDefault_NonGeneric_StorageMissing_ReturnsDefault()
    {
        var result = ComparerUtility.GetComparerOrDefault<string>("key1", typeof(int));
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<IComparer>(result);
    }

    [Test]
    public void RemoveComparer_Generic_RemovesBothStorages()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetComparer<int, string>("key1", comparer);
        var removed = ComparerUtility.RemoveComparer<int, string>("key1");
        Assert.IsTrue(removed);
        Assert.IsNull(ComparerUtility.GetComparer<int, string>("key1"));
    }

    [Test]
    public void RemoveComparer_NonGeneric_OnlyRemovesNonGenericStorage()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetComparer<int, string>("key1", comparer);
        var removed = ComparerUtility.RemoveComparer<string>("key1");
        Assert.IsTrue(removed);
        Assert.IsNotNull(ComparerUtility.GetComparer<int, string>("key1"));
        Assert.IsNull(ComparerUtility.GetComparer<string>("key1", typeof(MyIntComparer)));
    }

    [Test]
    public void ClearAll_ResetsEverything_Comparer()
    {
        ComparerUtility.SetComparer<int, string>("key1", new MyIntComparer());
        var _ = ComparerUtility.GetComparerOrDefault<string>("any", typeof(int));
        ComparerUtility.ClearAll();
        Assert.IsNull(ComparerUtility.GetComparer<int, string>("key1"));
        Assert.IsNotNull(ComparerUtility.GetComparerOrDefault<string>("any", typeof(int)));
    }

    [Test]
    public void GetEqualityComparer_NonGeneric_NoTypeCheck_ReturnsComparer()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetEqualityComparer<int, string>("key1", comparer);
        var result = ComparerUtility.GetEqualityComparer<string>("key1");
        Assert.AreSame(comparer, result);
    }

    [Test]
    public void GetEqualityComparer_NonGeneric_NoTypeCheck_NotRegistered_ReturnsNull()
    {
        var result = ComparerUtility.GetEqualityComparer<string>("key1");
        Assert.IsNull(result);
    }

    [Test]
    public void GetComparer_NonGeneric_NoTypeCheck_ReturnsComparer()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetComparer<int, string>("key1", comparer);
        var result = ComparerUtility.GetComparer<string>("key1");
        Assert.AreSame(comparer, result);
    }

    [Test]
    public void GetComparer_NonGeneric_NoTypeCheck_NotRegistered_ReturnsNull()
    {
        var result = ComparerUtility.GetComparer<string>("key1");
        Assert.IsNull(result);
    }

    [Test]
    public void GetEqualityComparerOrDefault_NonGeneric_WithTypeCheck_TypeMatches_Returns()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetEqualityComparer<int, string>("key1", comparer);
        var result = ComparerUtility.GetEqualityComparerOrDefault<string>("key1", typeof(int), typeof(MyIntComparer));
        Assert.AreSame(comparer, result);
    }

    [Test]
    public void GetEqualityComparerOrDefault_NonGeneric_WithTypeCheck_TypeMismatch_ReturnsDefault()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetEqualityComparer<int, string>("key1", comparer);

        // 类型不匹配时，应返回默认比较器，而非抛出异常
        var result = ComparerUtility.GetEqualityComparerOrDefault<string>("key1", typeof(int), typeof(string));
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<IEqualityComparer>(result);
        // 验证返回的是默认比较器（非注册的那个）
        Assert.AreNotSame(comparer, result);
        // 可以进一步验证它确实是 EqualityComparer<int>.Default（如果是 int 类型的话）
        // 但由于类型擦除，这里验证它不等于注册的比较器即可
    }

    [Test]
    public void GetEqualityComparerOrDefault_NonGeneric_WithTypeCheck_NotRegistered_ReturnsDefault()
    {
        var result = ComparerUtility.GetEqualityComparerOrDefault<string>("key1", typeof(int), typeof(MyIntComparer));
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<IEqualityComparer>(result);
    }

    [Test]
    public void GetComparerOrDefault_NonGeneric_WithTypeCheck_TypeMatches_Returns()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetComparer<int, string>("key1", comparer);
        var result = ComparerUtility.GetComparerOrDefault<string>("key1", typeof(int), typeof(MyIntComparer));
        Assert.AreSame(comparer, result);
    }

    [Test]
    public void GetComparerOrDefault_NonGeneric_WithTypeCheck_TypeMismatch_ReturnsDefault()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetComparer<int, string>("key1", comparer);

        // 类型不匹配时，应返回默认比较器，而非抛出异常
        var result = ComparerUtility.GetComparerOrDefault<string>("key1", typeof(int), typeof(string));
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<IComparer>(result);
        Assert.AreNotSame(comparer, result);
    }

    [Test]
    public void GetComparerOrDefault_NonGeneric_WithTypeCheck_NotRegistered_ReturnsDefault()
    {
        var result = ComparerUtility.GetComparerOrDefault<string>("key1", typeof(int), typeof(MyIntComparer));
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<IComparer>(result);
    }

    [Test]
    public void GetEqualityComparerOrDefault_Generic_StorageMissing_ReturnsDefault()
    {
        var result = ComparerUtility.GetEqualityComparerOrDefault<int, string>("key1");
        Assert.AreEqual(EqualityComparer<int>.Default, result);
    }

    [Test]
    public void GetComparerOrDefault_Generic_StorageMissing_ReturnsDefault()
    {
        var result = ComparerUtility.GetComparerOrDefault<int, string>("key1");
        Assert.AreEqual(Comparer<int>.Default, result);
    }

    [Test]
    public void TryGetEqualityComparer_Generic_Success()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetEqualityComparer<int, string>("key1", comparer);

        bool success = ComparerUtility.TryGetEqualityComparer<int, string>("key1", out var result);
        Assert.IsTrue(success);
        Assert.AreSame(comparer, result);
    }

    [Test]
    public void TryGetEqualityComparer_Generic_KeyNotFound_ReturnsFalse()
    {
        bool success = ComparerUtility.TryGetEqualityComparer<int, string>("key1", out var result);
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    [Test]
    public void TryGetEqualityComparer_Generic_KeyNull_ReturnsFalse()
    {
        bool success = ComparerUtility.TryGetEqualityComparer<int, string>(null, out var result);
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    [Test]
    public void TryGetEqualityComparer_NonGeneric_NoTypeCheck_Success()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetEqualityComparer<int, string>("key1", comparer);

        bool success = ComparerUtility.TryGetEqualityComparer<string>("key1", out var result);
        Assert.IsTrue(success);
        Assert.AreSame(comparer, result);
    }

    [Test]
    public void TryGetEqualityComparer_NonGeneric_NoTypeCheck_KeyNotFound_ReturnsFalse()
    {
        bool success = ComparerUtility.TryGetEqualityComparer<string>("key1", out var result);
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    [Test]
    public void TryGetEqualityComparer_NonGeneric_NoTypeCheck_KeyNull_ReturnsFalse()
    {
        bool success = ComparerUtility.TryGetEqualityComparer<string>(null, out var result);
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    [Test]
    public void TryGetEqualityComparer_NonGeneric_WithTypeCheck_Success()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetEqualityComparer<int, string>("key1", comparer);

        bool success = ComparerUtility.TryGetEqualityComparer<string>("key1", typeof(MyIntComparer), out var result);
        Assert.IsTrue(success);
        Assert.AreSame(comparer, result);
    }

    [Test]
    public void TryGetEqualityComparer_NonGeneric_WithTypeCheck_TypeMismatch_ReturnsFalse()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetEqualityComparer<int, string>("key1", comparer);

        bool success = ComparerUtility.TryGetEqualityComparer<string>("key1", typeof(string), out var result);
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    [Test]
    public void TryGetEqualityComparer_NonGeneric_WithTypeCheck_KeyNotFound_ReturnsFalse()
    {
        bool success = ComparerUtility.TryGetEqualityComparer<string>("key1", typeof(MyIntComparer), out var result);
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    [Test]
    public void TryGetEqualityComparer_NonGeneric_WithTypeCheck_KeyNull_ReturnsFalse()
    {
        bool success = ComparerUtility.TryGetEqualityComparer<string>(null, typeof(MyIntComparer), out var result);
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    [Test]
    public void TryGetEqualityComparer_NonGeneric_WithTypeCheck_ComparerTypeNull_ReturnsFalse()
    {
        bool success = ComparerUtility.TryGetEqualityComparer<string>("key1", null, out var result);
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    // ---- Comparer 对称测试 ----
    [Test]
    public void TryGetComparer_Generic_Success()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetComparer<int, string>("key1", comparer);

        bool success = ComparerUtility.TryGetComparer<int, string>("key1", out var result);
        Assert.IsTrue(success);
        Assert.AreSame(comparer, result);
    }

    [Test]
    public void TryGetComparer_Generic_KeyNotFound_ReturnsFalse()
    {
        bool success = ComparerUtility.TryGetComparer<int, string>("key1", out var result);
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    [Test]
    public void TryGetComparer_Generic_KeyNull_ReturnsFalse()
    {
        bool success = ComparerUtility.TryGetComparer<int, string>(null, out var result);
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    [Test]
    public void TryGetComparer_NonGeneric_NoTypeCheck_Success()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetComparer<int, string>("key1", comparer);

        bool success = ComparerUtility.TryGetComparer<string>("key1", out var result);
        Assert.IsTrue(success);
        Assert.AreSame(comparer, result);
    }

    [Test]
    public void TryGetComparer_NonGeneric_NoTypeCheck_KeyNotFound_ReturnsFalse()
    {
        bool success = ComparerUtility.TryGetComparer<string>("key1", out var result);
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    [Test]
    public void TryGetComparer_NonGeneric_NoTypeCheck_KeyNull_ReturnsFalse()
    {
        bool success = ComparerUtility.TryGetComparer<string>(null, out var result);
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    [Test]
    public void TryGetComparer_NonGeneric_WithTypeCheck_Success()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetComparer<int, string>("key1", comparer);

        bool success = ComparerUtility.TryGetComparer<string>("key1", typeof(MyIntComparer), out var result);
        Assert.IsTrue(success);
        Assert.AreSame(comparer, result);
    }

    [Test]
    public void TryGetComparer_NonGeneric_WithTypeCheck_TypeMismatch_ReturnsFalse()
    {
        var comparer = new MyIntComparer();
        ComparerUtility.SetComparer<int, string>("key1", comparer);

        bool success = ComparerUtility.TryGetComparer<string>("key1", typeof(string), out var result);
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    [Test]
    public void TryGetComparer_NonGeneric_WithTypeCheck_KeyNotFound_ReturnsFalse()
    {
        bool success = ComparerUtility.TryGetComparer<string>("key1", typeof(MyIntComparer), out var result);
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    [Test]
    public void TryGetComparer_NonGeneric_WithTypeCheck_KeyNull_ReturnsFalse()
    {
        bool success = ComparerUtility.TryGetComparer<string>(null, typeof(MyIntComparer), out var result);
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    [Test]
    public void TryGetComparer_NonGeneric_WithTypeCheck_ComparerTypeNull_ReturnsFalse()
    {
        bool success = ComparerUtility.TryGetComparer<string>("key1", null, out var result);
        Assert.IsFalse(success);
        Assert.IsNull(result);
    }

    // ============================================================
    // 新增：OrDefault 方法对 null key 的行为测试（应返回默认值，不抛异常）
    // ============================================================

    [Test]
    public void GetEqualityComparerOrDefault_Generic_KeyNull_ReturnsDefault()
    {
        var result = ComparerUtility.GetEqualityComparerOrDefault<int, string>(null);
        Assert.AreEqual(EqualityComparer<int>.Default, result);
    }

    [Test]
    public void GetComparerOrDefault_Generic_KeyNull_ReturnsDefault()
    {
        var result = ComparerUtility.GetComparerOrDefault<int, string>(null);
        Assert.AreEqual(Comparer<int>.Default, result);
    }

    [Test]
    public void GetEqualityComparerOrDefault_NonGeneric_KeyNull_ReturnsDefault()
    {
        var result = ComparerUtility.GetEqualityComparerOrDefault<string>(null, typeof(int));
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<IEqualityComparer>(result);
    }

    [Test]
    public void GetComparerOrDefault_NonGeneric_KeyNull_ReturnsDefault()
    {
        var result = ComparerUtility.GetComparerOrDefault<string>(null, typeof(int));
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<IComparer>(result);
    }

    [Test]
    public void GetEqualityComparerOrDefault_NonGeneric_WithTypeCheck_KeyNull_ReturnsDefault()
    {
        var result = ComparerUtility.GetEqualityComparerOrDefault<string>(null, typeof(int), typeof(MyIntComparer));
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<IEqualityComparer>(result);
    }

    [Test]
    public void GetComparerOrDefault_NonGeneric_WithTypeCheck_KeyNull_ReturnsDefault()
    {
        var result = ComparerUtility.GetComparerOrDefault<string>(null, typeof(int), typeof(MyIntComparer));
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<IComparer>(result);
    }

    [Test]
    public void GetEqualityComparerOrDefault_NonGeneric_WithTypeCheck_ComparerTypeNull_ReturnsDefault()
    {
        // 当 comparerType 为 null 时，应返回默认值，不抛异常
        var result = ComparerUtility.GetEqualityComparerOrDefault<string>("key1", typeof(int), null);
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<IEqualityComparer>(result);
    }

    [Test]
    public void GetComparerOrDefault_NonGeneric_WithTypeCheck_ComparerTypeNull_ReturnsDefault()
    {
        var result = ComparerUtility.GetComparerOrDefault<string>("key1", typeof(int), null);
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<IComparer>(result);
    }
}