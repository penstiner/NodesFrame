using System;
using System.Globalization;
using Nodify;
using Shell.Services;

namespace Shell.Models
{
    public class Variable : ObservableObject
    {
        private string _name;
        private VariantValue _value;
        private string _typeName;
        private string _description;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        
        /// <summary>变量的当前值。变更时同步通知 ValueString 绑定刷新。</summary>
        public VariantValue Value
        {
            get => _value;
            set
            {
                if (SetProperty(ref _value, value))
                    OnPropertyChanged(nameof(ValueString));
            }
        }
        public string TypeName { get => _typeName; set => SetProperty(ref _typeName, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value); }

        // ── 影子文本：尾部带 "." 时暂存，防止绑定解析后回退 ──
        private string _pendingText = "";
        private bool _hasPendingText;

        /// <summary>用于 UI 编辑的字符串形式值，通过 VariableTypeRegistry 委托给类型处理器。</summary>
        public string ValueString
        {
            get
            {
                if (_hasPendingText) return _pendingText;
                var handler = VariableTypeRegistry.Get(TypeName);
                return handler != null ? handler.GetDisplayText(Value) : Value.ToString();
            }
            set
            {
                // 尾部 "." 是正在输入小数的中间态，暂存到影子避免绑定回退
                if ((TypeName == "Double" || TypeName == "Float")
                    && value.Length > 0 && value[value.Length - 1] == '.')
                {
                    _pendingText = value;
                    _hasPendingText = true;
                    OnPropertyChanged(nameof(ValueString));
                    return;
                }

                _hasPendingText = false;
                if (GetDisplayValue() == value) return;
                var handler = VariableTypeRegistry.Get(TypeName);
                if (handler != null)
                    Value = handler.Parse(value, Value);
                OnPropertyChanged(nameof(ValueString));
            }
        }

        /// <summary>获取当前显示文本（不含影子）。</summary>
        private string GetDisplayValue()
        {
            var handler = VariableTypeRegistry.Get(TypeName);
            return handler != null ? handler.GetDisplayText(Value) : Value.ToString();
        }

        /// <summary>
        /// 直接设值并通知 Value / ValueString 绑定刷新（供代码调用，无需反射）。
        /// </summary>
        public void SetValueAndNotify(VariantValue newValue)
        {
            _value = newValue;
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(ValueString));
        }

        public Variable() { }
        public Variable(string name, string typeName, VariantValue defaultValue = default)
        {
            Name = name;
            TypeName = typeName;
            Value = defaultValue;
        }
    }
}
