using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Core.UI.Controls;

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
        private readonly Type _propertyType;

        public string Name { get; }
        public string Key { get; }
        public string Group { get; }

        /// <summary>属性编辑器类型：Number / Text / Boolean / Enum。</summary>
        public string EditorType { get; }

        /// <summary>NumericTextBox 数值类型：Int 禁止小数点，Float 允许小数点。</summary>
        public NumberKinds NumericKind =>
            _propertyType == typeof(double) || _propertyType == typeof(float)
                ? NumberKinds.Float
                : NumberKinds.Int;

        /// <summary>数值最小值（来自 [NodeProperty].Min，未设置则为 double.MinValue）。</summary>
        public string MinValue { get; }

        /// <summary>数值最大值（来自 [NodeProperty].Max，未设置则为 double.MaxValue）。</summary>
        public string MaxValue { get; }

        /// <summary>范围提示文本，如 "0 ~ 255"；未设置范围时为空。</summary>
        public string RangeHint { get; }

        /// <summary>枚举选项（仅 Enum 类型有效）。</summary>
        public IReadOnlyList<EnumOption> EnumOptions { get; }

        /// <summary>该属性是否允许绑定到变量（来自 [NodeProperty] 标记）。</summary>
        public bool BindableToVariable { get; }

        /// <summary>属性绑定信息（来自节点的 PropertyBindings 字典）。</summary>
        public PropertyBinding Binding { get; }

        /// <summary>可用变量名列表（从全局 VariableManager 获取）。</summary>
        public IReadOnlyList<string> VariableNames
        {
            get
            {
                if (!BindableToVariable) return Array.Empty<string>();
                return NodeViewModel.GlobalVariableManager?.GetVariableNames() ?? Array.Empty<string>();
            }
        }

        public PropertyItem(NodeViewModel owner, PropertyInfo prop, Attributes.NodePropertyAttribute attr)
        {
            _owner = owner;
            _property = prop;
            _propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            Key = attr.Key ?? prop.Name;
            Name = attr.DisplayName ?? prop.Name;
            Group = attr.Group ?? "";
            BindableToVariable = attr.BindableToVariable;

            // NumericTextBox 范围限制（仅对 Number 类型有意义）
            if (attr.Min > double.MinValue)
                MinValue = attr.Min.ToString(CultureInfo.InvariantCulture);
            else
                MinValue = _propertyType == typeof(double) || _propertyType == typeof(float)
                    ? double.MinValue.ToString(CultureInfo.InvariantCulture)
                    : int.MinValue.ToString();
            if (attr.Max < double.MaxValue)
                MaxValue = attr.Max.ToString(CultureInfo.InvariantCulture);
            else
                MaxValue = _propertyType == typeof(double) || _propertyType == typeof(float)
                    ? double.MaxValue.ToString(CultureInfo.InvariantCulture)
                    : int.MaxValue.ToString();

            // 范围提示（如 "0 ~ 255"），用于 UI 标签展示
            bool hasMin = attr.Min > double.MinValue;
            bool hasMax = attr.Max < double.MaxValue;
            bool isInt = _propertyType == typeof(int) || _propertyType == typeof(long)
                      || _propertyType == typeof(short);
            if (hasMin && hasMax)
                RangeHint = $"{Fmt(attr.Min)} ~ {Fmt(attr.Max)}";
            else if (hasMin)
                RangeHint = $"≥ {Fmt(attr.Min)}";
            else if (hasMax)
                RangeHint = $"≤ {Fmt(attr.Max)}";
            else
                RangeHint = "";
            string Fmt(double v) => isInt ? ((long)v).ToString() : v.ToString("G", CultureInfo.InvariantCulture);

            // 获取或创建 PropertyBinding（确保绑定对象始终可用）
            if (BindableToVariable)
            {
                if (!owner.PropertyBindings.TryGetValue(Key, out var binding))
                {
                    binding = new PropertyBinding();
                    owner.PropertyBindings[Key] = binding;
                }
                Binding = binding;

                // 监听绑定状态变化，刷新 VariableNames 列表
                Binding.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(PropertyBinding.IsBound))
                        OnPropertyChanged(nameof(VariableNames));
                };
            }
            else
            {
                Binding = new PropertyBinding(); // 空占位
            }

            var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            // ── 优先级 1：DynamicOptionsSource → 调用节点方法获取动态选项 ──
            if (!string.IsNullOrEmpty(attr.DynamicOptionsSource))
            {
                EditorType = "Enum";
                var list = new List<EnumOption>();
                try
                {
                    var method = owner.GetType().GetMethod(attr.DynamicOptionsSource,
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (method != null)
                    {
                        var result = method.Invoke(owner, null);
                        if (result is IEnumerable<string> labels)
                        {
                            int idx = 0;
                            foreach (var label in labels)
                                list.Add(new EnumOption(label, idx++));
                        }
                    }
                }
                catch { }
                // 若动态查询无结果，提供默认提示项
                if (list.Count == 0)
                    list.Add(new EnumOption("未发现设备", 0));
                EnumOptions = list;
            }
            // ── 优先级 2：Options 静态逗号分隔标签 → ComboBox ──
            else if (!string.IsNullOrEmpty(attr.Options))
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


        /// <summary>以 "." 结尾时暂存于此，防止绑定解析后回退 TextBox 显示。</summary>
        private string _pendingText = "";
        private bool _hasPendingText;

        public string Value
        {
            get
            {
                if (_hasPendingText)
                    return _pendingText;
                var val = _property.GetValue(_owner);
                return val?.ToString() ?? "";
            }
            set
            {
                // 以 "." 结尾是正在输入小数的中间状态，暂存到影子避免绑定回退
                if ((_propertyType == typeof(double) || _propertyType == typeof(float))
                    && value.Length > 0 && value[value.Length - 1] == '.'
                    && !value.StartsWith("-."))
                {
                    _pendingText = value;
                    _hasPendingText = true;
                    OnPropertyChanged();
                    return;
                }

                _hasPendingText = false;
                // NumericTextBox 已处理输入过滤（Int 模式禁止小数点等），直接解析
                try
                {
                    object converted = _propertyType switch
                    {
                        _ when _propertyType == typeof(double) => double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0.0,
                        _ when _propertyType == typeof(float)  => float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f) ? f : 0f,
                        _ when _propertyType == typeof(int)    => int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var i) ? i : 0,
                        _ when _propertyType == typeof(long)   => long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var l) ? l : 0L,
                        _ when _propertyType == typeof(short)  => short.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var s) ? s : (short)0,
                        _ when _propertyType == typeof(bool)   => bool.TryParse(value, out var b) && b,
                        _ when _propertyType.IsEnum            => int.TryParse(value, out var ei) ? Enum.ToObject(_propertyType, ei) : Activator.CreateInstance(_propertyType),
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
                if (val is int i) return i;
                if (val is string s && EnumOptions != null)
                {
                    for (int idx = 0; idx < EnumOptions.Count; idx++)
                        if (EnumOptions[idx].Label == s) return idx;
                }
                try { return Convert.ToInt32(val); }
                catch { return 0; }
            }
            set
            {
                var type = Nullable.GetUnderlyingType(_property.PropertyType) ?? _property.PropertyType;
                if (type == typeof(string) && EnumOptions != null && value >= 0 && value < EnumOptions.Count)
                {
                    _property.SetValue(_owner, EnumOptions[value].Label);
                }
                else
                {
                    Value = value.ToString();
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(Value));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
