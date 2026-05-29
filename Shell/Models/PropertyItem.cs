using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Shell.Models
{
    /// <summary>枚举选项，提供 ComboBox 可绑定的 Label / Value 属性。</summary>
    public sealed class EnumOption
    {
        public string Label { get; }
        public int Value { get; }
        public EnumOption(string label, int value) { Label = label; Value = value; }
    }

    /// <summary>
    /// 反射发现的节点可编辑属性描述项，支持双向绑定。
    /// 通过反射读写宿主节点上的实际属性值。
    /// </summary>
    public sealed class PropertyItem : INotifyPropertyChanged
    {
        private readonly NodeViewModel _owner;
        private readonly PropertyInfo _property;

        public string Name { get; }
        public string Key { get; }
        public string Group { get; }

        /// <summary>属性编辑器类型：Number / Text / Boolean / Enum。</summary>
        public string EditorType { get; }

        /// <summary>枚举选项（仅 Enum 类型有效）。</summary>
        public IReadOnlyList<EnumOption> EnumOptions { get; }

        public PropertyItem(NodeViewModel owner, PropertyInfo prop, Attributes.NodePropertyAttribute attr)
        {
            _owner = owner;
            _property = prop;
            Key = attr.Key ?? prop.Name;
            Name = attr.DisplayName ?? prop.Name;
            Group = attr.Group ?? "";

            var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            // ── Options 优先：逗号分隔的标签 → ComboBox ──
            if (!string.IsNullOrEmpty(attr.Options))
            {
                EditorType = "Enum";
                var labels = attr.Options.Split(',');
                var list = new List<EnumOption>();
                for (int i = 0; i < labels.Length; i++)
                    list.Add(new EnumOption(labels[i].Trim(), i));
                EnumOptions = list;
            }
            else if (type == typeof(double) || type == typeof(float) || type == typeof(int)
                || type == typeof(long) || type == typeof(short))
                EditorType = "Number";
            else if (type == typeof(bool))
                EditorType = "Boolean";
            else if (type.IsEnum)
            {
                EditorType = "Enum";
                var values = Enum.GetValues(type);
                var list = new List<EnumOption>();
                foreach (var v in values)
                    list.Add(new EnumOption(v.ToString(), (int)v));
                EnumOptions = list;
            }
            else
                EditorType = "Text";
        }

        public string Value
        {
            get
            {
                var val = _property.GetValue(_owner);
                return val?.ToString() ?? "";
            }
            set
            {
                var type = Nullable.GetUnderlyingType(_property.PropertyType) ?? _property.PropertyType;
                try
                {
                    object converted = type switch
                    {
                        _ when type == typeof(double) => double.TryParse(value, out var d) ? d : 0.0,
                        _ when type == typeof(float)  => float.TryParse(value, out var f) ? f : 0f,
                        _ when type == typeof(int)    => int.TryParse(value, out var i) ? i : 0,
                        _ when type == typeof(long)   => long.TryParse(value, out var l) ? l : 0L,
                        _ when type == typeof(short)  => short.TryParse(value, out var s) ? s : (short)0,
                        _ when type == typeof(bool)   => bool.TryParse(value, out var b) && b,
                        _ when type.IsEnum            => int.TryParse(value, out var ei) ? Enum.ToObject(type, ei) : Activator.CreateInstance(type),
                        _ => value
                    };
                    _property.SetValue(_owner, converted);
                }
                catch { }
                OnPropertyChanged();
            }
        }

        public bool BoolValue
        {
            get => bool.TryParse(Value, out var b) && b;
            set => Value = value.ToString();
        }

        public int EnumIndex
        {
            get
            {
                var val = _property.GetValue(_owner);
                return val is int i ? i : Convert.ToInt32(val);
            }
            set => Value = value.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
