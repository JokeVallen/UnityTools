using System;
using System.Runtime.CompilerServices;
using FNV1A.x64;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

internal static class FNV1AUtilityExtension
{
    public static ulong AppendFor<T>(this ulong hash, T value)
    {
        if (value == null) return FNV1AUtility.AppendByte(hash, 0);

#if UNITY_5_3_OR_NEWER
        if (IsUnityEngineType<T>.Value)
            return FNV1AUtility.AppendForUnity(hash, value);
#endif

#if ENABLE_UNSAFE
        if (typeof(T) == typeof(System.Guid))
            return FNV1AUtility.AppendForUnsafe(hash, value);
#endif
        return FNV1AUtility.AppendForNET(hash, value);
    }

    public static ulong AppendFor(this ulong hash, object value)
    {
        if (value == null) return FNV1AUtility.AppendByte(hash, 0);
        Type type = value.GetType();

#if UNITY_5_3_OR_NEWER
        if (type.Namespace?.StartsWith("UnityEngine") == true || value is UnityEngine.Object)
            return FNV1AUtility.AppendForUnity(hash, value);
#endif

#if ENABLE_UNSAFE
        if (value is Guid guid)
            return FNV1AUtility.AppendForUnsafe(hash, guid);
#endif
        return FNV1AUtility.AppendForNET(hash, value);
    }

#if UNITY_5_3_OR_NEWER
    /// <summary>
    /// 确定指定类型是否为 UnityEngine命名空间下的相关类型
    /// </summary>
    public static class IsUnityEngineType<T>
    {
        public static readonly bool Value = Compute();

        private static bool Compute()
        {
            Type type = typeof(T);
            if (type.Namespace?.StartsWith("UnityEngine") == true)
                return true;
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                return true;
            return false;
        }
    }
#endif

    // ===============================
    // .NET 基础类型重载（链式调用）
    // ===============================

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Append(this ulong hash, byte value) => FNV1AUtility.AppendByte(hash, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Append(this ulong hash, int value) => FNV1AUtility.AppendInt32(hash, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Append(this ulong hash, long value) => FNV1AUtility.AppendInt64(hash, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Append(this ulong hash, float value) => FNV1AUtility.AppendFloat(hash, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Append(this ulong hash, double value) => FNV1AUtility.AppendDouble(hash, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Append(this ulong hash, bool value) => FNV1AUtility.AppendBool(hash, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Append(this ulong hash, string value) => FNV1AUtility.AppendString(hash, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Append(this ulong hash, DateTime value) => FNV1AUtility.AppendDateTime(hash, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Append<TEnum>(this ulong hash, TEnum value) where TEnum : Enum
        => FNV1AUtility.AppendEnum(hash, value);

    // Guid 在未启用 Unsafe 时走安全版本
#if !ENABLE_UNSAFE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Append(this ulong hash, Guid value) => FNV1AUtility.AppendGuid(hash, value);
#else
    // 若启用 Unsafe，Guid 可通过统一入口自动走 AppendForUnsafe，
    // 但仍可提供显式重载以保持一致性（可选）
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Append(this ulong hash, Guid value) => FNV1AUtility.AppendForUnsafe(hash, value);
#endif

    // ===============================
    // Unity 类型重载（链式调用）
    // ===============================

#if UNITY_5_3_OR_NEWER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Append(this ulong hash, Vector2 value) => FNV1AUtility.AppendVector2(hash, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Append(this ulong hash, Vector3 value) => FNV1AUtility.AppendVector3(hash, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Append(this ulong hash, Vector4 value) => FNV1AUtility.AppendVector4(hash, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Append(this ulong hash, Quaternion value) => FNV1AUtility.AppendQuaternion(hash, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Append(this ulong hash, Color value) => FNV1AUtility.AppendColor(hash, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Append(this ulong hash, Rect value) => FNV1AUtility.AppendRect(hash, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Append(this ulong hash, UnityEngine.Object value) => FNV1AUtility.AppendUnityObject(hash, value);
#endif

    // ===============================
    // 集合类型重载（需显式传入元素哈希器）
    // ===============================

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AppendArray<T>(this ulong hash, T[] array, Func<ulong, T, ulong> elementHasher)
        => FNV1AUtility.AppendArray(hash, array, elementHasher);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AppendList<T>(this ulong hash, System.Collections.Generic.List<T> list, Func<ulong, T, ulong> elementHasher)
        => FNV1AUtility.AppendList(hash, list, elementHasher);
}