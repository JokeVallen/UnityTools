using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using FNV1A.x64;

public class FNV1AUtilityTests
{
    #region 基础类型测试

    [Test]
    public void AppendByte_ConsistentHash()
    {
        ulong hash1 = FNV1AUtility.Start();
        hash1 = FNV1AUtility.AppendByte(hash1, 42);
        ulong hash2 = FNV1AUtility.Start();
        hash2 = FNV1AUtility.AppendByte(hash2, 42);
        Assert.AreEqual(hash1, hash2, "相同输入应产生相同哈希");
    }

    [Test]
    public void AppendByte_DifferentValues_ProduceDifferentHash()
    {
        ulong h1 = FNV1AUtility.AppendByte(FNV1AUtility.Start(), 0);
        ulong h2 = FNV1AUtility.AppendByte(FNV1AUtility.Start(), 1);
        Assert.AreNotEqual(h1, h2, "不同输入应产生不同哈希");
    }

    [Test]
    public void AppendInt32_Consistent()
    {
        ulong h1 = FNV1AUtility.AppendInt32(FNV1AUtility.Start(), 123456);
        ulong h2 = FNV1AUtility.AppendInt32(FNV1AUtility.Start(), 123456);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void AppendInt64_Consistent()
    {
        long value = 0x123456789ABCDEF;
        ulong h1 = FNV1AUtility.AppendInt64(FNV1AUtility.Start(), value);
        ulong h2 = FNV1AUtility.AppendInt64(FNV1AUtility.Start(), value);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void AppendFloat_ZeroAndNegativeZero_DifferentHash()
    {
        ulong hPosZero = FNV1AUtility.AppendFloat(FNV1AUtility.Start(), 0.0f);
        ulong hNegZero = FNV1AUtility.AppendFloat(FNV1AUtility.Start(), -0.0f);
        Assert.AreNotEqual(hPosZero, hNegZero, "二进制位不同，哈希应不同");
    }

    [Test]
    public void AppendDouble_Consistent()
    {
        double value = 3.141592653589793;
        ulong h1 = FNV1AUtility.AppendDouble(FNV1AUtility.Start(), value);
        ulong h2 = FNV1AUtility.AppendDouble(FNV1AUtility.Start(), value);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void AppendString_Null_ReturnsSameAsNullByte()
    {
        ulong hNull = FNV1AUtility.AppendString(FNV1AUtility.Start(), null);
        ulong hNullByte = FNV1AUtility.AppendByte(FNV1AUtility.Start(), 0);
        Assert.AreEqual(hNullByte, hNull);
    }

    [Test]
    public void AppendString_SameString_SameHash()
    {
        string s = "Hello, 世界!";
        ulong h1 = FNV1AUtility.AppendString(FNV1AUtility.Start(), s);
        ulong h2 = FNV1AUtility.AppendString(FNV1AUtility.Start(), s);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void AppendString_DifferentCase_DifferentHash()
    {
        ulong h1 = FNV1AUtility.AppendString(FNV1AUtility.Start(), "abc");
        ulong h2 = FNV1AUtility.AppendString(FNV1AUtility.Start(), "ABC");
        Assert.AreNotEqual(h1, h2);
    }

    [Test]
    public void AppendBool_TrueFalse_DifferentHash()
    {
        ulong hTrue = FNV1AUtility.AppendBool(FNV1AUtility.Start(), true);
        ulong hFalse = FNV1AUtility.AppendBool(FNV1AUtility.Start(), false);
        Assert.AreNotEqual(hTrue, hFalse);
    }

    [Test]
    public void AppendEnum_Consistent()
    {
        TestEnum e = TestEnum.ValueB;
        ulong h1 = FNV1AUtility.AppendEnum(FNV1AUtility.Start(), e);
        ulong h2 = FNV1AUtility.AppendEnum(FNV1AUtility.Start(), e);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void AppendDateTime_Consistent()
    {
        DateTime dt = new DateTime(2026, 4, 19, 12, 0, 0, DateTimeKind.Utc);
        ulong h1 = FNV1AUtility.AppendDateTime(FNV1AUtility.Start(), dt);
        ulong h2 = FNV1AUtility.AppendDateTime(FNV1AUtility.Start(), dt);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void AppendGuid_Consistent()
    {
        Guid g = Guid.NewGuid();
        ulong h1 = FNV1AUtility.AppendGuid(FNV1AUtility.Start(), g);
        ulong h2 = FNV1AUtility.AppendGuid(FNV1AUtility.Start(), g);
        Assert.AreEqual(h1, h2);
    }

    #endregion

    #region Unity 类型测试

    [Test]
    public void AppendVector2_Consistent()
    {
        Vector2 v = new Vector2(1.23f, -4.56f);
        ulong h1 = FNV1AUtility.AppendVector2(FNV1AUtility.Start(), v);
        ulong h2 = FNV1AUtility.AppendVector2(FNV1AUtility.Start(), v);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void AppendVector3_Consistent()
    {
        Vector3 v = new Vector3(1, 2, 3);
        ulong h1 = FNV1AUtility.AppendVector3(FNV1AUtility.Start(), v);
        ulong h2 = FNV1AUtility.AppendVector3(FNV1AUtility.Start(), v);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void AppendVector4_Consistent()
    {
        Vector4 v = new Vector4(1, 2, 3, 4);
        ulong h1 = FNV1AUtility.AppendVector4(FNV1AUtility.Start(), v);
        ulong h2 = FNV1AUtility.AppendVector4(FNV1AUtility.Start(), v);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void AppendQuaternion_Consistent()
    {
        Quaternion q = Quaternion.Euler(30, 45, 60);
        ulong h1 = FNV1AUtility.AppendQuaternion(FNV1AUtility.Start(), q);
        ulong h2 = FNV1AUtility.AppendQuaternion(FNV1AUtility.Start(), q);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void AppendColor_Consistent()
    {
        Color c = new Color(0.1f, 0.2f, 0.3f, 0.4f);
        ulong h1 = FNV1AUtility.AppendColor(FNV1AUtility.Start(), c);
        ulong h2 = FNV1AUtility.AppendColor(FNV1AUtility.Start(), c);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void AppendRect_Consistent()
    {
        Rect r = new Rect(10, 20, 100, 200);
        ulong h1 = FNV1AUtility.AppendRect(FNV1AUtility.Start(), r);
        ulong h2 = FNV1AUtility.AppendRect(FNV1AUtility.Start(), r);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void AppendUnityObject_Null_Handled()
    {
        UnityEngine.Object obj = null;
        ulong hNull = FNV1AUtility.AppendUnityObject(FNV1AUtility.Start(), obj);
        ulong hZero = FNV1AUtility.AppendByte(FNV1AUtility.Start(), 0);
        Assert.AreEqual(hZero, hNull);
    }

    [Test]
    public void AppendUnityObject_UsesInstanceID()
    {
        // 模拟一个对象，无法直接创建 UnityEngine.Object，通过派生测试
        var go = new GameObject("Test");
        try
        {
            ulong h = FNV1AUtility.AppendUnityObject(FNV1AUtility.Start(), go);
            Assert.AreNotEqual(FNV1AUtility.Start(), h);
        }
        finally
        {
            GameObject.DestroyImmediate(go);
        }
    }

    #endregion

    #region 集合类型测试

    [Test]
    public void AppendArray_IntArray_Consistent()
    {
        int[] arr = { 1, 2, 3, 4 };
        ulong h1 = FNV1AUtility.AppendArray(FNV1AUtility.Start(), arr, FNV1AUtility.AppendInt32);
        ulong h2 = FNV1AUtility.AppendArray(FNV1AUtility.Start(), arr, FNV1AUtility.AppendInt32);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void AppendArray_Null_ReturnsNullHash()
    {
        int[] arr = null;
        ulong h = FNV1AUtility.AppendArray(FNV1AUtility.Start(), arr, FNV1AUtility.AppendInt32);
        ulong expected = FNV1AUtility.AppendByte(FNV1AUtility.Start(), 0);
        Assert.AreEqual(expected, h);
    }

    [Test]
    public void AppendList_Consistent()
    {
        List<string> list = new List<string> { "a", "b", "c" };
        ulong h1 = FNV1AUtility.AppendList(FNV1AUtility.Start(), list, FNV1AUtility.AppendString);
        ulong h2 = FNV1AUtility.AppendList(FNV1AUtility.Start(), list, FNV1AUtility.AppendString);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void AppendIListGeneric_SameAsArray()
    {
        int[] arr = { 10, 20, 30 };
        IList<int> ilist = arr;
        ulong hArray = FNV1AUtility.AppendArray(FNV1AUtility.Start(), arr, FNV1AUtility.AppendInt32);
        ulong hIList = FNV1AUtility.AppendIListGeneric(FNV1AUtility.Start(), ilist, FNV1AUtility.AppendInt32);
        Assert.AreEqual(hArray, hIList);
    }

    [Test]
    public void AppendForCollection_Generic_CacheWorks()
    {
        // 第一次调用会初始化缓存
        int[] arr = { 1, 2, 3 };
        ulong h1 = FNV1AUtility.AppendForCollection<int[], int>(FNV1AUtility.Start(), arr, FNV1AUtility.AppendInt32);
        ulong h2 = FNV1AUtility.AppendForCollection<int[], int>(FNV1AUtility.Start(), arr, FNV1AUtility.AppendInt32);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void AppendForCollection_NonGeneric_IList()
    {
        ArrayList list = new ArrayList { 1, "two", 3.0f };
        Func<ulong, object, ulong> objectHasher = (h, obj) =>
        {
            if (obj is int i) return FNV1AUtility.AppendInt32(h, i);
            if (obj is string s) return FNV1AUtility.AppendString(h, s);
            if (obj is float f) return FNV1AUtility.AppendFloat(h, f);
            return h;
        };
        ulong h1 = FNV1AUtility.AppendIList(FNV1AUtility.Start(), list, objectHasher);
        ulong h2 = FNV1AUtility.AppendIList(FNV1AUtility.Start(), list, objectHasher);
        Assert.AreEqual(h1, h2);
    }

    #endregion

    #region 自定义哈希接口测试

    class HashableTest : IFNVHashable
    {
        public int A;
        public string B;

        public ulong AppendHash(ulong hash)
        {
            hash = FNV1AUtility.AppendInt32(hash, A);
            hash = FNV1AUtility.AppendString(hash, B);
            return hash;
        }
    }

    [Test]
    public void IFNVHashable_WorksWithGenericCache()
    {
        var obj = new HashableTest { A = 42, B = "hello" };
        ulong h1 = FNV1AUtility.AppendForNET(FNV1AUtility.Start(), obj);
        ulong h2 = obj.AppendHash(FNV1AUtility.Start());
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void IFNVHashable_Null_Handled()
    {
        HashableTest obj = null;
        ulong h = FNV1AUtility.AppendForNET(FNV1AUtility.Start(), obj);
        ulong expected = FNV1AUtility.AppendByte(FNV1AUtility.Start(), 0);
        Assert.AreEqual(expected, h);
    }

    #endregion

    #region 缓存与注册测试

    [Test]
    public void RegisterHasherForNET_OverridesDefault()
    {
        // 为 string 类型注册一个简单哈希器（只取长度）
        FNV1AUtility.RegisterHasherForNET<string>((hash, s) => FNV1AUtility.AppendInt32(hash, s?.Length ?? 0));

        ulong h1 = FNV1AUtility.AppendForNET(FNV1AUtility.Start(), "test");
        ulong h2 = FNV1AUtility.AppendInt32(FNV1AUtility.Start(), 4);
        Assert.AreEqual(h2, h1);

        // 恢复默认（或再次注册正常哈希器），避免影响其他测试
        FNV1AUtility.RegisterHasherForNET<string>(null);
    }

    [Test]
    public void RegisterHasherForCollection_Works()
    {
        // 注册一个自定义数组哈希器：只哈希长度
        FNV1AUtility.RegisterHasherForCollection<int[], int>((hash, arr, _) => FNV1AUtility.AppendInt32(hash, arr?.Length ?? 0));
        int[] arr = { 1, 2, 3, 4, 5 };
        ulong h = FNV1AUtility.AppendForCollection<int[], int>(FNV1AUtility.Start(), arr, null);
        ulong expected = FNV1AUtility.AppendInt32(FNV1AUtility.Start(), 5);
        Assert.AreEqual(expected, h);

        // 恢复
        FNV1AUtility.RegisterHasherForCollection<int[], int>(null);
    }

    #endregion

    #region 新增：Unsafe API 测试（条件编译）

#if ENABLE_UNSAFE
    [Test]
    public void AppendGuidUnsafe_Consistent()
    {
        Guid g = Guid.NewGuid();
        ulong h1 = FNV1AUtility.AppendGuidUnsafe(FNV1AUtility.Start(), g);
        ulong h2 = FNV1AUtility.AppendGuidUnsafe(FNV1AUtility.Start(), g);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void AppendGuidFastUnsafe_Consistent()
    {
        Guid g = Guid.NewGuid();
        ulong h1 = FNV1AUtility.AppendGuidFastUnsafe(FNV1AUtility.Start(), g);
        ulong h2 = FNV1AUtility.AppendGuidFastUnsafe(FNV1AUtility.Start(), g);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void AppendGuidUnsafe_And_AppendGuidFastUnsafe_ProduceDifferentHash()
    {
        // 两种方法因处理顺序不同，产生的哈希值应不相同（符合预期）
        Guid g = Guid.NewGuid();
        ulong h1 = FNV1AUtility.AppendGuidUnsafe(FNV1AUtility.Start(), g);
        ulong h2 = FNV1AUtility.AppendGuidFastUnsafe(FNV1AUtility.Start(), g);
        Assert.AreNotEqual(h1, h2, "逐字节循环与两次 ulong 迭代产生的哈希值不同，此为预期行为，请勿混用");
    }

    [Test]
    public void AppendGuidUnsafe_Matches_AppendGuid_WhenUnsafeEnabled()
    {
        // 启用 Unsafe 后，AppendGuid 内部也是逐字节指针循环，应与 AppendGuidUnsafe 结果一致
        Guid g = Guid.NewGuid();
        ulong h1 = FNV1AUtility.AppendGuidUnsafe(FNV1AUtility.Start(), g);
        ulong h2 = FNV1AUtility.AppendGuid(FNV1AUtility.Start(), g);
        Assert.AreEqual(h1, h2, "AppendGuidUnsafe 应与 AppendGuid 的 unsafe 分支产生相同哈希");
    }

    [Test]
    public void AppendForUnsafe_Guid_CallsFastPath()
    {
        Guid g = Guid.NewGuid();
        ulong h1 = FNV1AUtility.AppendForUnsafe(FNV1AUtility.Start(), g);
        ulong h2 = FNV1AUtility.AppendGuidFastUnsafe(FNV1AUtility.Start(), g);
        Assert.AreEqual(h1, h2, "AppendForUnsafe<Guid> 应调用 AppendGuidFastUnsafe");
    }

    [Test]
    public void AppendForUnsafe_Object_Guid()
    {
        object obj = Guid.NewGuid();
        ulong h1 = FNV1AUtility.AppendForUnsafe(FNV1AUtility.Start(), obj);
        ulong h2 = FNV1AUtility.AppendGuidFastUnsafe(FNV1AUtility.Start(), (Guid)obj);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void RegisterHasherForUnsafe_Overrides()
    {
        var originalHasher = FNV1AUtility.UnsafeFNVHasherCache<Guid>.Hasher;
        try
        {
            FNV1AUtility.RegisterHasherForUnsafe<Guid>((hash, g) =>
                FNV1AUtility.AppendByte(hash, g.ToByteArray()[0]));

            Guid g = Guid.NewGuid();
            ulong h1 = FNV1AUtility.AppendForUnsafe(FNV1AUtility.Start(), g);
            ulong h2 = FNV1AUtility.AppendByte(FNV1AUtility.Start(), g.ToByteArray()[0]);
            Assert.AreEqual(h2, h1);
        }
        finally
        {
            FNV1AUtility.RegisterHasherForUnsafe<Guid>(originalHasher);
        }
    }
#endif

    #endregion

    #region 新增：Unity 类型注册测试

    [Test]
    public void RegisterHasherForUnity_OverridesDefault()
    {
        FNV1AUtility.RegisterHasherForUnity<Vector3>((hash, v) =>
            FNV1AUtility.AppendFloat(hash, v.x + v.y + v.z));

        Vector3 v = new Vector3(1, 2, 3);
        ulong h1 = FNV1AUtility.AppendForUnity(FNV1AUtility.Start(), v);
        ulong h2 = FNV1AUtility.AppendFloat(FNV1AUtility.Start(), 6f);
        Assert.AreEqual(h2, h1);

        // 恢复
        FNV1AUtility.RegisterHasherForUnity<Vector3>(null);
    }

    #endregion

    #region 新增：边界条件和组合测试

    [Test]
    public void AppendArray_EmptyArray_AddsLengthZero()
    {
        int[] arr = new int[0];
        ulong h = FNV1AUtility.AppendArray(FNV1AUtility.Start(), arr, FNV1AUtility.AppendInt32);
        ulong expected = FNV1AUtility.AppendInt32(FNV1AUtility.Start(), 0);
        Assert.AreEqual(expected, h);
    }

    [Test]
    public void AppendList_EmptyList_AddsCountZero()
    {
        List<int> list = new List<int>();
        ulong h = FNV1AUtility.AppendList(FNV1AUtility.Start(), list, FNV1AUtility.AppendInt32);
        ulong expected = FNV1AUtility.AppendInt32(FNV1AUtility.Start(), 0);
        Assert.AreEqual(expected, h);
    }

    [Test]
    public void AppendIListGeneric_Null_Handled()
    {
        IList<int> list = null;
        ulong h = FNV1AUtility.AppendIListGeneric(FNV1AUtility.Start(), list, FNV1AUtility.AppendInt32);
        ulong expected = FNV1AUtility.AppendByte(FNV1AUtility.Start(), 0);
        Assert.AreEqual(expected, h);
    }

    [Test]
    public void AppendIList_Null_Handled()
    {
        IList list = null;
        ulong h = FNV1AUtility.AppendIList(FNV1AUtility.Start(), list, (hsh, obj) => hsh);
        ulong expected = FNV1AUtility.AppendByte(FNV1AUtility.Start(), 0);
        Assert.AreEqual(expected, h);
    }

    [Test]
    public void CombinedHash_MultipleTypes_Deterministic()
    {
        ulong h1 = FNV1AUtility.Start();
        h1 = FNV1AUtility.AppendInt32(h1, 100);
        h1 = FNV1AUtility.AppendString(h1, "test");
        h1 = FNV1AUtility.AppendVector3(h1, new Vector3(1, 2, 3));

        ulong h2 = FNV1AUtility.Start();
        h2 = FNV1AUtility.AppendInt32(h2, 100);
        h2 = FNV1AUtility.AppendString(h2, "test");
        h2 = FNV1AUtility.AppendVector3(h2, new Vector3(1, 2, 3));

        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void AppendForCollection_NonIList_Object_FallsBackToGetHashCode()
    {
        object obj = new object();
        ulong h = FNV1AUtility.AppendForCollection(FNV1AUtility.Start(), obj, (hsh, o) => hsh);
        ulong expected = FNV1AUtility.AppendInt32(FNV1AUtility.Start(), obj.GetHashCode());
        Assert.AreEqual(expected, h);
    }

    [Test]
    public void AppendForCollection_Null_Handled()
    {
        ulong h = FNV1AUtility.AppendForCollection(FNV1AUtility.Start(), null, (hsh, o) => hsh);
        ulong expected = FNV1AUtility.AppendByte(FNV1AUtility.Start(), 0);
        Assert.AreEqual(expected, h);
    }

    #endregion

    #region 新增：非泛型 AppendForNET 覆盖更多类型

    [Test]
    public void AppendForNET_Object_HandlesVariousTypes()
    {
        // 每个类型独立测试，避免累积干扰
        ulong hash, expected;

        // byte
        object b = (byte)5;
        hash = FNV1AUtility.AppendForNET(FNV1AUtility.Start(), b);
        expected = FNV1AUtility.AppendByte(FNV1AUtility.Start(), 5);
        Assert.AreEqual(expected, hash, "byte 处理不一致");

        // long
        object l = 123456789L;
        hash = FNV1AUtility.AppendForNET(FNV1AUtility.Start(), l);
        expected = FNV1AUtility.AppendInt64(FNV1AUtility.Start(), 123456789L);
        Assert.AreEqual(expected, hash, "long 处理不一致");

        // double
        object d = 3.14;
        hash = FNV1AUtility.AppendForNET(FNV1AUtility.Start(), d);
        expected = FNV1AUtility.AppendDouble(FNV1AUtility.Start(), 3.14);
        Assert.AreEqual(expected, hash, "double 处理不一致");

        // bool
        object bl = true;
        hash = FNV1AUtility.AppendForNET(FNV1AUtility.Start(), bl);
        expected = FNV1AUtility.AppendBool(FNV1AUtility.Start(), true);
        Assert.AreEqual(expected, hash, "bool 处理不一致");

        // Enum
        object en = TestEnum.ValueC;
        hash = FNV1AUtility.AppendForNET(FNV1AUtility.Start(), en);
        expected = FNV1AUtility.AppendEnum(FNV1AUtility.Start(), TestEnum.ValueC);
        Assert.AreEqual(expected, hash, "Enum 处理不一致");

        // DateTime
        object dt = new DateTime(2025, 1, 1);
        hash = FNV1AUtility.AppendForNET(FNV1AUtility.Start(), dt);
        expected = FNV1AUtility.AppendDateTime(FNV1AUtility.Start(), (DateTime)dt);
        Assert.AreEqual(expected, hash, "DateTime 处理不一致");

        // 特殊：Guid 在非 Unsafe 下调用 AppendGuid，会产生分配但应一致
        object g = Guid.NewGuid();
        hash = FNV1AUtility.AppendForNET(FNV1AUtility.Start(), g);
        expected = FNV1AUtility.AppendGuid(FNV1AUtility.Start(), (Guid)g);
        Assert.AreEqual(expected, hash, "Guid 处理不一致");
    }

    #endregion

    #region 辅助枚举

    enum TestEnum
    {
        ValueA,
        ValueB,
        ValueC
    }

    #endregion
}