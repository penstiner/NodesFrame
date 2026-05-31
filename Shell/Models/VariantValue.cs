using System;
using System.Collections.Generic;
using System.Globalization;

namespace Shell.Models
{
    /// <summary>
    /// 表示连接器之间传递的通用值类型。
    /// 不可变 struct，内部用 object 存储数据 + TypeCode 标记类型。
    /// 支持 double, int, bool, string, byte[] (图像/二进制), double[] (向量) 等。
    /// </summary>
    public readonly struct VariantValue : IEquatable<VariantValue>
    {
        /// <summary>表示未连接/空值的单例。</summary>
        public static readonly VariantValue Null = default;

        private readonly object _value;
        private readonly TypeCode _typeCode;

        /// <summary>值的运行时类型。</summary>
        public TypeCode TypeCode => _typeCode;

        /// <summary>是否为 null（未连接或空值）。</summary>
        public bool IsNull => _typeCode == TypeCode.Empty;

        /// <summary>是否为数值类型（double 或 int）。</summary>
        public bool IsNumeric => _typeCode == TypeCode.Double || _typeCode == TypeCode.Int32;

        internal VariantValue(object value, TypeCode typeCode)
        {
            _value = typeCode == TypeCode.Empty ? null : value;
            _typeCode = typeCode;
        }

        // ──────────── 工厂方法 ────────────

        public static VariantValue FromDouble(double value) => new(value, TypeCode.Double);
        public static VariantValue FromInt32(int value) => new(value, TypeCode.Int32);
        public static VariantValue FromBoolean(bool value) => new(value, TypeCode.Boolean);
        public static VariantValue FromString(string value) => new(value ?? string.Empty, TypeCode.String);
        public static VariantValue FromBytes(byte[] value) => new(value, TypeCode.Object); // Object 代表 byte[]
        public static VariantValue FromDoubleArray(double[] value) => new(value, TypeCode.Object);
        public static VariantValue FromImageData(ImageData value) => new(value, TypeCode.Object);

        // ──────────── 隐式转换 (从基础类型 → VariantValue) ────────────

        public static implicit operator VariantValue(double value) => FromDouble(value);
        public static implicit operator VariantValue(double? value) => value.HasValue ? FromDouble(value.Value) : Null;
        public static implicit operator VariantValue(int value) => FromInt32(value);
        public static implicit operator VariantValue(int? value) => value.HasValue ? FromInt32(value.Value) : Null;
        public static implicit operator VariantValue(bool value) => FromBoolean(value);
        public static implicit operator VariantValue(bool? value) => value.HasValue ? FromBoolean(value.Value) : Null;
        public static implicit operator VariantValue(string value) => value != null ? FromString(value) : Null;
        public static implicit operator VariantValue(byte[] value) => value != null ? FromBytes(value) : Null;

        // ──────────── TryGet 类型安全取值 ────────────

        public bool TryGetDouble(out double result)
        {
            if (_typeCode == TypeCode.Double)
            {
                result = (double)_value;
                return true;
            }
            if (_typeCode == TypeCode.Int32)
            {
                result = (int)_value;
                return true;
            }
            result = 0;
            return false;
        }

        public bool TryGetInt32(out int result)
        {
            if (_typeCode == TypeCode.Int32)
            {
                result = (int)_value;
                return true;
            }
            if (_typeCode == TypeCode.Double)
            {
                result = (int)(double)_value;
                return true;
            }
            result = 0;
            return false;
        }

        public bool TryGetBoolean(out bool result)
        {
            if (_typeCode == TypeCode.Boolean)
            {
                result = (bool)_value;
                return true;
            }
            if (_typeCode == TypeCode.Int32)
            {
                result = (int)_value != 0;
                return true;
            }
            if (_typeCode == TypeCode.Double)
            {
                result = (double)_value != 0;
                return true;
            }
            result = false;
            return false;
        }

        public bool TryGetString(out string result)
        {
            if (_typeCode == TypeCode.String)
            {
                result = (string)_value;
                return true;
            }
            result = null;
            return false;
        }

        public bool TryGetBytes(out byte[] result)
        {
            if (_typeCode == TypeCode.Object && _value is byte[] bytes)
            {
                result = bytes;
                return true;
            }
            result = null;
            return false;
        }

        /// <summary>
        /// 尝试获取 ImageData。若值是 ImageData 直接返回；
        /// 若值是 PNG byte[] 则自动解码（向后兼容）。
        /// </summary>
        public bool TryGetImageData(out ImageData result)
        {
            if (_typeCode == TypeCode.Object && _value is ImageData img)
            {
                result = img;
                return true;
            }
            // 向后兼容：如果是 PNG byte[]，解码为 ImageData
            if (_typeCode == TypeCode.Object && _value is byte[] bytes && bytes.Length > 0)
            {
                result = Services.Algorithms.Vision.VisionAlgorithmService.PngBytesToImageData(bytes);
                return result != null;
            }
            result = null;
            return false;
        }

        public bool TryGetDoubleArray(out double[] result)
        {
            if (_typeCode == TypeCode.Object && _value is double[] arr)
            {
                result = arr;
                return true;
            }
            result = null;
            return false;
        }

        // ──────────── 获取原始值 ────────────

        /// <summary>获取原始装箱值，若为空则返回 null。</summary>
        public object GetRawValue() => IsNull ? null : _value;

        /// <summary>
        /// 根据目标类型尝试获取适配的值。用于变量绑定时将 VariantValue 转换为属性所需类型。
        /// </summary>
        public object GetValueForType(Type targetType)
        {
            if (targetType == typeof(double) && TryGetDouble(out var d)) return d;
            if (targetType == typeof(int) && TryGetDouble(out var i)) return (int)i;
            if (targetType == typeof(float) && TryGetDouble(out var f)) return (float)f;
            if (targetType == typeof(bool) && TryGetBoolean(out var b)) return b;
            if (targetType == typeof(string) && TryGetString(out var s)) return s;
            return null;
        }

        // ──────────── Equals / GetHashCode / ToString ────────────

        public bool Equals(VariantValue other)
        {
            if (_typeCode != other._typeCode) return false;
            if (IsNull) return true;
            if (_value is byte[] b1 && other._value is byte[] b2)
                return ByteArrayEquals(b1, b2);
            if (_value is double[] d1 && other._value is double[] d2)
                return DoubleArrayEquals(d1, d2);
            return EqualityComparer<object>.Default.Equals(_value, other._value);
        }

        public override bool Equals(object obj) => obj is VariantValue other && Equals(other);

        public override int GetHashCode()
        {
            if (IsNull) return 0;
            return HashCode.Combine(_typeCode, _value);
        }

        public override string ToString()
        {
            return _typeCode switch
            {
                TypeCode.Empty => "(null)",
                TypeCode.Double => ((double)_value).ToString(CultureInfo.InvariantCulture),
                TypeCode.Int32 => ((int)_value).ToString(),
                TypeCode.Boolean => ((bool)_value).ToString(),
                TypeCode.String => (string)_value,
                TypeCode.Object when _value is ImageData img => $"[Image {img.Width}×{img.Height} {img.Channels}ch]",
                TypeCode.Object when _value is byte[] bytes => $"[byte[{bytes.Length}]]",
                TypeCode.Object when _value is double[] arr => $"[double[{arr.Length}]]",
                _ => _value?.ToString() ?? "(null)"
            };
        }

        public static bool operator ==(VariantValue left, VariantValue right) => left.Equals(right);
        public static bool operator !=(VariantValue left, VariantValue right) => !left.Equals(right);

        // ──────────── 类型兼容性检查 ────────────

        /// <summary>
        /// 检查 source 类型是否可以隐式传递给 target 类型。
        /// 规则：相同类型兼容；int→double 兼容；null 总是兼容。
        /// </summary>
        public static bool AreTypesCompatible(TypeCode sourceType, TypeCode targetType)
        {
            if (sourceType == TypeCode.Empty || targetType == TypeCode.Empty) return true;
            if (sourceType == targetType) return true;
            // int 可以传给 double
            if (sourceType == TypeCode.Int32 && targetType == TypeCode.Double) return true;
            // 数值 → 布尔
            if ((sourceType == TypeCode.Int32 || sourceType == TypeCode.Double) && targetType == TypeCode.Boolean) return true;
            // 数值 → 字符串
            if ((sourceType == TypeCode.Int32 || sourceType == TypeCode.Double || sourceType == TypeCode.Boolean) && targetType == TypeCode.String) return true;
            // Object 类型（byte[], double[]）间不自动兼容，需显式匹配
            if (sourceType == TypeCode.Object && targetType == TypeCode.Object) return true;
            return false;
        }

        private static bool ByteArrayEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }

        private static bool DoubleArrayEquals(double[] a, double[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (Math.Abs(a[i] - b[i]) > 1e-10) return false;
            return true;
        }
    }
}
