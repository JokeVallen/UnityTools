using System.Collections.Generic;

/// <summary>
/// 哈希码工具
/// </summary>
/// <remarks>
/// <para>提供一组生成和合并哈希码的静态方法，采用乘加混合算法（种子 17，乘数 31），
/// 支持最多 5 个参数的快速合并，以及任意数量参数的合并和顺序相关的哈希计算。</para>
/// <para>所有计算均在 <c>unchecked</c> 上下文中执行，允许静默溢出。</para>
/// </remarks>
public static class HashCodeUtility
{
    /// <summary>
    /// 获取哈希码
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="obj1">对象</param>
    /// <returns>哈希码</returns>
    /// <remarks>
    /// <para>使用 <see cref="EqualityComparer{T}.Default"/> 获取对象的哈希码，并与种子混合。</para>
    /// <para>若 <paramref name="obj1"/> 为 <c>null</c>，对应哈希视为 0。</para>
    /// </remarks>
    public static int GetHashCode<T>(T obj1)
    {
        return GetHashCodeInternal(obj1);
    }

    /// <summary>
    /// 合并两个对象
    /// </summary>
    /// <typeparam name="T1">第一个对象类型</typeparam>
    /// <typeparam name="T2">第二个对象类型</typeparam>
    /// <param name="obj1">第一个对象</param>
    /// <param name="obj2">第二个对象</param>
    /// <returns>合并后的哈希码</returns>
    /// <remarks>
    /// <para>依次将每个对象的哈希码混合到种子中，顺序敏感。</para>
    /// <para>对于 <c>null</c> 对象，其哈希贡献值为 0。</para>
    /// </remarks>
    public static int Combine<T1, T2>(T1 obj1, T2 obj2)
    {
        return CombineInternal(obj1, obj2);
    }

    /// <summary>
    /// 合并三个对象
    /// </summary>
    /// <typeparam name="T1">第一个对象类型</typeparam>
    /// <typeparam name="T2">第二个对象类型</typeparam>
    /// <typeparam name="T3">第三个对象类型</typeparam>
    /// <param name="obj1">第一个对象</param>
    /// <param name="obj2">第二个对象</param>
    /// <param name="obj3">第三个对象</param>
    /// <returns>合并后的哈希码</returns>
    /// <remarks>
    /// <para>依次混合三个对象的哈希码，顺序改变会导致结果不同。</para>
    /// </remarks>
    public static int Combine<T1, T2, T3>(T1 obj1, T2 obj2, T3 obj3)
    {
        return CombineInternal(obj1, obj2, obj3);
    }

    /// <summary>
    /// 合并四个对象
    /// </summary>
    /// <typeparam name="T1">第一个对象类型</typeparam>
    /// <typeparam name="T2">第二个对象类型</typeparam>
    /// <typeparam name="T3">第三个对象类型</typeparam>
    /// <typeparam name="T4">第四个对象类型</typeparam>
    /// <param name="obj1">第一个对象</param>
    /// <param name="obj2">第二个对象</param>
    /// <param name="obj3">第三个对象</param>
    /// <param name="obj4">第四个对象</param>
    /// <returns>合并后的哈希码</returns>
    /// <remarks>
    /// <para>依次混合四个对象的哈希码，顺序敏感。</para>
    /// </remarks>
    public static int Combine<T1, T2, T3, T4>(T1 obj1, T2 obj2, T3 obj3, T4 obj4)
    {
        return CombineInternal(obj1, obj2, obj3, obj4);
    }

    /// <summary>
    /// 合并五个对象
    /// </summary>
    /// <typeparam name="T1">第一个对象类型</typeparam>
    /// <typeparam name="T2">第二个对象类型</typeparam>
    /// <typeparam name="T3">第三个对象类型</typeparam>
    /// <typeparam name="T4">第四个对象类型</typeparam>
    /// <typeparam name="T5">第五个对象类型</typeparam>
    /// <param name="obj1">第一个对象</param>
    /// <param name="obj2">第二个对象</param>
    /// <param name="obj3">第三个对象</param>
    /// <param name="obj4">第四个对象</param>
    /// <param name="obj5">第五个对象</param>
    /// <returns>合并后的哈希码</returns>
    /// <remarks>
    /// <para>依次混合五个对象的哈希码，顺序改变会导致结果不同。</para>
    /// <para>对于超过5个参数的场景，请使用 <see cref="CombineAll{T}"/> 重载。</para>
    /// </remarks>
    public static int Combine<T1, T2, T3, T4, T5>(T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5)
    {
        return CombineInternal(obj1, obj2, obj3, obj4, obj5);
    }

    /// <summary>
    /// 合并任意数量对象（泛型）
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="objects">对象数组</param>
    /// <returns>合并后的哈希码</returns>
    /// <remarks>
    /// <para>按顺序混合 <paramref name="objects"/> 中每个元素的哈希码，使用 <see cref="EqualityComparer{T}.Default"/> 获取各元素哈希。</para>
    /// <para>若参数为 <c>null</c>，返回 0；空数组返回初始种子值（17）。</para>
    /// <para>顺序敏感，例如 <c>CombineAll("a","b")</c> 与 <c>CombineAll("b","a")</c> 结果不同。</para>
    /// </remarks>
    public static int CombineAll<T>(params T[] objects)
    {
        return CombineAllInternal(objects);
    }

    /// <summary>
    /// 合并任意数量对象（非泛型）
    /// </summary>
    /// <param name="objects">对象数组</param>
    /// <returns>合并后的哈希码</returns>
    /// <remarks>
    /// <para>按顺序混合 <paramref name="objects"/> 中每个元素的哈希码，通过 <see cref="object.GetHashCode()"/> 获取哈希值。</para>
    /// <para>注意：值类型将被装箱，其哈希值可能与泛型版本不同（依赖于运行时的 <c>ValueType.GetHashCode</c> 实现）。</para>
    /// <para>若参数为 <c>null</c>，返回 0；空数组返回初始种子值（17）。</para>
    /// </remarks>
    public static int CombineAll(params object[] objects)
    {
        return CombineAllInternal(objects);
    }

    /// <summary>
    /// 获取顺序依赖哈希码（数组，默认比较器）
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="array">元素数组</param>
    /// <returns>顺序依赖的哈希码</returns>
    /// <remarks>
    /// <para>使用 <see cref="EqualityComparer{T}.Default"/> 计算各元素的哈希，并根据元素顺序依次混合。</para>
    /// <para>若 <paramref name="array"/> 为 <c>null</c>，返回 0；空数组返回种子值（17）。</para>
    /// </remarks>
    public static int GetOrderDependentHashCode<T>(T[] array)
    {
        return GetOrderDependentHashCodeInternal(array, EqualityComparer<T>.Default);
    }

    /// <summary>
    /// 获取顺序依赖哈希码（数组，自定义比较器）
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="array">元素数组</param>
    /// <param name="comparer">相等比较器</param>
    /// <returns>顺序依赖的哈希码</returns>
    /// <remarks>
    /// <para>若 <paramref name="comparer"/> 为 <c>null</c>，将回退为 <see cref="EqualityComparer{T}.Default"/>。</para>
    /// <para>允许通过自定义比较器控制哈希行为（例如不区分大小写的字符串哈希）。</para>
    /// </remarks>
    public static int GetOrderDependentHashCode<T>(T[] array, IEqualityComparer<T> comparer)
    {
        return GetOrderDependentHashCodeInternal(array, comparer);
    }

    /// <summary>
    /// 获取顺序依赖哈希码（可枚举序列，默认比较器）
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="enumerable">元素序列</param>
    /// <returns>顺序依赖的哈希码</returns>
    /// <remarks>
    /// <para>遍历序列，使用默认比较器计算各元素哈希，按顺序混合。</para>
    /// <para>适用于 <see cref="IEnumerable{T}"/> 实现，如列表、数组等。</para>
    /// </remarks>
    public static int GetOrderDependentHashCode<T>(IEnumerable<T> enumerable)
    {
        return GetOrderDependentHashCodeInternal(enumerable, EqualityComparer<T>.Default);
    }

    /// <summary>
    /// 获取顺序依赖哈希码（可枚举序列，自定义比较器）
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="enumerable">元素序列</param>
    /// <param name="comparer">相等比较器</param>
    /// <returns>顺序依赖的哈希码</returns>
    /// <remarks>
    /// <para>若 <paramref name="comparer"/> 为 <c>null</c>，则使用默认比较器。</para>
    /// <para>通过自定义比较器可调整不同相等语义下的哈希结果。</para>
    /// </remarks>
    public static int GetOrderDependentHashCode<T>(IEnumerable<T> enumerable, IEqualityComparer<T> comparer)
    {
        return GetOrderDependentHashCodeInternal(enumerable, comparer);
    }

    private const int Seed = 17;
    private const int Multiplier = 31;

    private static int GetHashCodeInternal<T>(T obj1)
    {
        var comparer = EqualityComparer<T>.Default;

        unchecked
        {
            int hash = Seed;
            return hash * Multiplier + (obj1 == null ? 0 : comparer.GetHashCode(obj1));
        }
    }

    private static int CombineInternal<T1, T2>(T1 obj1, T2 obj2)
    {
        var c1 = EqualityComparer<T1>.Default;
        var c2 = EqualityComparer<T2>.Default;

        unchecked
        {
            int hash = Seed;
            hash = hash * Multiplier + (obj1 == null ? 0 : c1.GetHashCode(obj1));
            hash = hash * Multiplier + (obj2 == null ? 0 : c2.GetHashCode(obj2));
            return hash;
        }
    }

    private static int CombineInternal<T1, T2, T3>(T1 obj1, T2 obj2, T3 obj3)
    {
        var c1 = EqualityComparer<T1>.Default;
        var c2 = EqualityComparer<T2>.Default;
        var c3 = EqualityComparer<T3>.Default;

        unchecked
        {
            int hash = Seed;
            hash = hash * Multiplier + (obj1 == null ? 0 : c1.GetHashCode(obj1));
            hash = hash * Multiplier + (obj2 == null ? 0 : c2.GetHashCode(obj2));
            hash = hash * Multiplier + (obj3 == null ? 0 : c3.GetHashCode(obj3));
            return hash;
        }
    }

    private static int CombineInternal<T1, T2, T3, T4>(T1 obj1, T2 obj2, T3 obj3, T4 obj4)
    {
        var c1 = EqualityComparer<T1>.Default;
        var c2 = EqualityComparer<T2>.Default;
        var c3 = EqualityComparer<T3>.Default;
        var c4 = EqualityComparer<T4>.Default;

        unchecked
        {
            int hash = Seed;
            hash = hash * Multiplier + (obj1 == null ? 0 : c1.GetHashCode(obj1));
            hash = hash * Multiplier + (obj2 == null ? 0 : c2.GetHashCode(obj2));
            hash = hash * Multiplier + (obj3 == null ? 0 : c3.GetHashCode(obj3));
            hash = hash * Multiplier + (obj4 == null ? 0 : c4.GetHashCode(obj4));
            return hash;
        }
    }

    private static int CombineInternal<T1, T2, T3, T4, T5>(T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5)
    {
        var c1 = EqualityComparer<T1>.Default;
        var c2 = EqualityComparer<T2>.Default;
        var c3 = EqualityComparer<T3>.Default;
        var c4 = EqualityComparer<T4>.Default;
        var c5 = EqualityComparer<T5>.Default;

        unchecked
        {
            int hash = Seed;
            hash = hash * Multiplier + (obj1 == null ? 0 : c1.GetHashCode(obj1));
            hash = hash * Multiplier + (obj2 == null ? 0 : c2.GetHashCode(obj2));
            hash = hash * Multiplier + (obj3 == null ? 0 : c3.GetHashCode(obj3));
            hash = hash * Multiplier + (obj4 == null ? 0 : c4.GetHashCode(obj4));
            hash = hash * Multiplier + (obj5 == null ? 0 : c5.GetHashCode(obj5));
            return hash;
        }
    }

    private static int CombineAllInternal<T>(params T[] objects)
    {
        if (objects == null) return 0;

        var comparer = EqualityComparer<T>.Default;

        unchecked
        {
            int hash = Seed;
            foreach (var obj in objects)
            {
                hash = hash * Multiplier + (obj == null ? 0 : comparer.GetHashCode(obj));
            }
            return hash;
        }
    }

    private static int CombineAllInternal(params object[] objects)
    {
        if (objects == null) return 0;

        unchecked
        {
            int hash = Seed;
            foreach (var obj in objects)
            {
                hash = hash * Multiplier + (obj?.GetHashCode() ?? 0);
            }
            return hash;
        }
    }

    private static int GetOrderDependentHashCodeInternal<T>(T[] array, IEqualityComparer<T> comparer)
    {
        if (array == null) return 0;

        var com = comparer ?? EqualityComparer<T>.Default;

        unchecked
        {
            int hash = Seed;
            for (int i = 0; i < array.Length; i++)
            {
                T item = array[i];
                hash = hash * Multiplier + (item == null ? 0 : com.GetHashCode(item));
            }
            return hash;
        }
    }

    private static int GetOrderDependentHashCodeInternal<T>(IEnumerable<T> enumerable, IEqualityComparer<T> comparer)
    {
        if (enumerable == null) return 0;

        var com = comparer ?? EqualityComparer<T>.Default;

        unchecked
        {
            int hash = Seed;
            foreach (T item in enumerable)
            {
                int itemHash = (item == null) ? 0 : com.GetHashCode(item);
                hash = hash * Multiplier + itemHash;
            }
            return hash;
        }
    }
}