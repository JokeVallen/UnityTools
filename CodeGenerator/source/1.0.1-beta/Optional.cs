using System;
using System.Collections.Generic;

namespace CodeGenerator
{
    /// <summary>
    /// 可选值包装器
    /// </summary>
    public readonly struct Optional<T> : IEquatable<Optional<T>>
    {
        /// <summary>
        /// 是否已赋值
        /// </summary>
        public bool HasValue
        {
            get
            {
                return hasValue;
            }
        }

        /// <summary>
        /// 值
        /// </summary>
        public T Value
        {
            get
            {
                if (!hasValue)
                    throw new InvalidOperationException("[CodeGenerator] Optional has no value");
                return value;
            }
        }

        private readonly T value;
        private readonly bool hasValue;

        private Optional(T value, bool hasValue)
        {
            this.value = value;
            this.hasValue = hasValue;
        }

        /// <inheritdoc/>
        public bool Equals(Optional<T> other)
        {
            if (hasValue != other.hasValue) return false;
            if (!hasValue) return true;
            return EqualityComparer<T>.Default.Equals(value, other.value);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!(obj is Optional<T>)) return false;
            return Equals(((Optional<T>)obj));
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (!hasValue) return 0;
            if (value == null) return 0;
            return value.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (!hasValue) return "<none>";
            if (value == null) return "null";
            return value.ToString();
        }

        /// <summary>
        /// 获取一个无值实例
        /// </summary>
        public static readonly Optional<T> None = new Optional<T>();

        /// <inheritdoc/>
        public static implicit operator Optional<T>(T value)
        {
            return new Optional<T>(value, true);
        }

        /// <inheritdoc/>
        public static explicit operator T(Optional<T> optional) 
        { 
            return optional.Value;
        }

        /// <inheritdoc/>
        public static bool operator ==(Optional<T> left, Optional<T> right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(Optional<T> left, Optional<T> right) 
        { 
            return !left.Equals(right);
        }
    }
}
