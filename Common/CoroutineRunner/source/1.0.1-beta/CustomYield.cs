using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoroutineRunner
{
    /// <summary>
    /// 自定义可控指令静态池化工厂
    /// </summary>
    /// <remarks>
    /// <para>若扩展的自定义指令需要接入池化工厂：请实现具体的池化扩展接口 <see cref="IPoolableYieldInstruction"/> 或 <see cref="IPoolableYieldInstruction{T}"/>。</para>
    /// </remarks>
    public static class CustomYield
    {
        private class Pooler
        {
            private readonly Type type;
            private readonly Queue<CustomYieldInstructionBase> pool = new Queue<CustomYieldInstructionBase>();
            private Func<CustomYieldInstructionBase> factory;
            private readonly int capacity;

            public Pooler(Type type, int capacity, Func<CustomYieldInstructionBase> factory) 
            { 
                this.type = type;
                this.capacity = capacity;
                this.factory = factory;
                Application.quitting -= OnApplicationQuit;
                Application.quitting += OnApplicationQuit;
            }

            public CustomYieldInstructionBase Get() 
            {
                if (pool.Count > 0) return pool.Dequeue();
                return factory();
            }

            public void Release(CustomYieldInstructionBase item) 
            {
                if (item == null) return;
                if (pool.Count >= capacity) return;
                if (type != item.GetType()) return;
                pool.Enqueue(item);
            }

            private void OnApplicationQuit()
            {
                pool.Clear();
                factory = null;
            }
        }

        private static readonly Dictionary<Type, Pooler> poolDict = new Dictionary<Type, Pooler>();
        private const int DEFAULT_CAPACITY = 128;

        /// <summary>
        /// 获取指定类型的池化指令实例
        /// </summary>
        /// <param name="instructionType">指令类型</param>
        /// <returns>池化的指令实例</returns>
        /// <remarks>
        /// <para>指令类型需要提供公开的无参构造函数，内部将通过反射创建实例。</para>
        /// <para>适用于无参数依赖的指令。</para>
        /// </remarks>
        public static CustomYieldInstructionBase Yield(Type instructionType) 
        {
            if (instructionType == null) 
                throw new ArgumentNullException(nameof(instructionType));
            if(!typeof(IPoolable).IsAssignableFrom(instructionType)) 
                throw new ArgumentException($"[CoroutineRunner] The paramter '{nameof(instructionType)}' is not poolable.");
            return YieldInternal(instructionType, null, false);
        }

        /// <summary>
        /// 获取指定类型的附带重置参数的池化指令实例
        /// </summary>
        /// <param name="instructionType">指令类型</param>
        /// <param name="arg">重置参数</param>
        /// <returns>池化的指令实例</returns>
        /// <remarks>
        /// <para>指令类型需要提供公开的无参构造函数，内部将通过反射创建实例。</para>
        /// <para>指令类型需要实现 <see cref="IPoolableYieldInstruction"/> 接口以支持非泛型参数重置。</para>
        /// <para>适用于有参数依赖的指令。</para>
        /// </remarks>
        public static CustomYieldInstructionBase Yield(Type instructionType, object arg) 
        {
            if (instructionType == null)
                throw new ArgumentNullException(nameof(instructionType));
            if (!typeof(IPoolable).IsAssignableFrom(instructionType))
                throw new ArgumentException($"[CoroutineRunner] The paramter '{nameof(instructionType)}' is not poolable.");
            return YieldInternal(instructionType, arg, true);
        }

        /// <summary>
        /// 获取指定类型的附带重置参数的池化指令实例
        /// </summary>
        /// <param name="instructionType">指令类型</param>
        /// <param name="arg">重置参数</param>
        /// <typeparam name="T">参数类型</typeparam>
        /// <returns>池化的指令实例</returns>
        /// <remarks>
        /// <para>指令类型需要提供公开的无参构造函数，内部将通过反射创建实例。</para>
        /// <para>指令类型需要实现 <see cref="IPoolableYieldInstruction{T}"/> 接口以支持泛型参数重置。</para>
        /// <para>适用于有参数依赖的指令。</para>
        /// </remarks>
        public static CustomYieldInstructionBase Yield<T>(Type instructionType, T arg) 
        {
            if (instructionType == null)
                throw new ArgumentNullException(nameof(instructionType));
            if (!typeof(IPoolable).IsAssignableFrom(instructionType))
                throw new ArgumentException($"[CoroutineRunner] The paramter '{nameof(instructionType)}' is not poolable.");
            return YieldInternal(instructionType,arg);
        }

        /// <summary>
        /// 获取指定类型的池化指令实例
        /// </summary>
        /// <typeparam name="T">指令类型</typeparam>
        /// <returns>池化的指令实例</returns>
        /// <remarks>
        /// <para>指令类型需要提供公开的无参构造函数，内部将通过 <c>new()</c> 创建实例。</para>
        /// <para>适用于无参数依赖的指令。</para>
        /// </remarks>
        public static T Yield<T>() where T : CustomYieldInstructionBase, IPoolable, new () 
        {
            return (T)YieldInternal<T>(null, false);
        }

        /// <summary>
        /// 获取指定类型的附带重置参数的池化指令实例
        /// </summary>
        /// <param name="arg">重置参数</param>
        /// <typeparam name="T">指令类型</typeparam>
        /// <returns>池化的指令实例</returns>
        /// <remarks>
        /// <para>指令类型需要提供公开的无参构造函数，内部将通过 <c>new()</c> 创建实例。</para>
        /// <para>指令类型需要实现 <see cref="IPoolableYieldInstruction"/> 接口以支持非泛型参数重置。</para>
        /// <para>适用于有参数依赖的指令。</para>
        /// </remarks>
        public static T Yield<T>(object arg) where T : CustomYieldInstructionBase, IPoolable, new() 
        {
            return (T)YieldInternal<T>(arg, true);
        }

        /// <summary>
        /// 获取指定类型的附带重置参数的池化指令实例
        /// </summary>
        /// <param name="arg">重置参数</param>
        /// <typeparam name="T1">指令类型</typeparam>
        /// <typeparam name="T2">参数类型</typeparam>
        /// <returns>池化的指令实例</returns>
        /// <remarks>
        /// <para>指令类型需要提供公开的无参构造函数，内部将通过 <c>new()</c> 创建实例。</para>
        /// <para>指令类型可以实现 <see cref="IPoolableYieldInstruction{T}"/> 接口以支持泛型参数重置。</para>
        /// <para>指令类型可以实现 <see cref="IPoolableYieldInstruction"/> 接口以支持非泛型参数重置。</para>
        /// <para>两种参数实现接口需要至少实现一种，推荐优先实现泛型接口，可以避免值类型装箱和引用类型多余的类型转换。</para>
        /// <para>适用于有参数依赖的指令。</para>
        /// </remarks>
        public static T1 Yield<T1, T2>(T2 arg) where T1 : CustomYieldInstructionBase, IPoolable, new() 
        {
            return (T1)YieldInternal<T1, T2>(arg, true);
        }

        /// <summary>
        /// 将指令实例回收到池中
        /// </summary>
        /// <param name="instruction">待回收的指令实例</param>
        /// <remarks>
        /// <para>如果池中容量未满且类型匹配，则回收以供复用；否则丢弃。</para>
        /// <para>框架内部自动回收通过 <c>yield return</c> 使用的指令，一般情况下无需手动调用。</para>
        /// </remarks>
        public static void Release(CustomYieldInstructionBase instruction) 
        {
            ReleaseInternal(instruction);
        }

        private static CustomYieldInstructionBase YieldInternal(Type instructionType, object arg, bool hasArg) 
        {
            var pooler = GetOrCreatePooler(instructionType);
            var item = pooler.Get();
            if (hasArg && item is IPoolableYieldInstruction poolable)
                poolable.Reset(arg);
            return item;
        }

        private static CustomYieldInstructionBase YieldInternal<T>(Type instructionType, T arg)
        {
            var pooler = GetOrCreatePooler(instructionType);
            var item = pooler.Get();
            if (item is IPoolableYieldInstruction<T> poolable)
                poolable.Reset(arg);
            return item;
        }

        private static CustomYieldInstructionBase YieldInternal<T>(object arg, bool hasArg) where T : CustomYieldInstructionBase, new()
        {
            var pooler = GetOrCreatePooler<T>();
            var item = pooler.Get();
            if(hasArg && item is IPoolableYieldInstruction poolable)
                poolable.Reset(arg);
            return item;
        }

        private static CustomYieldInstructionBase YieldInternal<T1, T2>(T2 arg, bool hasArg) where T1 : CustomYieldInstructionBase, new()
        {
            var pooler = GetOrCreatePooler<T1>();
            var item = pooler.Get();
            if (hasArg)
            {
                if(item is IPoolableYieldInstruction<T2> poolable)
                    poolable.Reset(arg);
                else if(item is IPoolableYieldInstruction poolable2)
                    poolable2.Reset(arg);
            }
            return item;
        }

        private static void ReleaseInternal(CustomYieldInstructionBase instruction) 
        {
            if (instruction == null) return;
            if (poolDict.TryGetValue(instruction.GetType(), out var pooler))
                pooler.Release(instruction);
        }

        private static Pooler GetOrCreatePooler(Type instructionType) 
        {
            if (!poolDict.TryGetValue(instructionType, out var pooler)) 
            {
                pooler = new Pooler(instructionType, DEFAULT_CAPACITY, () => (CustomYieldInstructionBase)Activator.CreateInstance(instructionType));
                poolDict[instructionType] = pooler;
            }

            return pooler;
        }

        private static Pooler GetOrCreatePooler<T>() where T : CustomYieldInstructionBase, new()
        {
            var instructionType = typeof(T);
            if (!poolDict.TryGetValue(instructionType, out var pooler))
            {
                pooler = new Pooler(instructionType, DEFAULT_CAPACITY, () => new T());
                poolDict[instructionType] = pooler;
            }

            return pooler;
        }
    }
}