using System;
using System.Globalization;

namespace Shell.Models
{
    [Attributes.Node(
        Category = "输入",
        DisplayName = "常量",
        DefaultTitle = "常量",
        Description = "输出一个固定值（支持 double/int/bool/string 等多种类型）",
        NodeTypeId = "Constant")]
    [Attributes.NodeConnector(Title = "Value", Direction = Attributes.ConnectorDirection.Output,
        ExpectedType = "Double", Description = "常量输出值")]
    public class ConstantNodeViewModel : NodeViewModel
    {
        public ConstantNodeViewModel()
        {
            AddOutputConnector(new ConnectorViewModel { Title = "Value" });
        }

        /// <summary>常量值（支持 double/int/bool/string 等多种类型）。</summary>
        [Attributes.NodeProperty(Key = "constant")]
        public VariantValue Constant
        {
            get => Output.Count > 0 ? Output[0].Value : VariantValue.Null;
            set
            {
                if (Output.Count > 0)
                {
                    Output[0].Value = value;
                    OnPropertyChanged(nameof(Constant));
                    OnPropertyChanged(nameof(ConstantDisplay));
                    OnPropertyChanged(nameof(ConstantString));
                }
            }
        }

        /// <summary>常量值的显示文本。</summary>
        public string ConstantDisplay => Constant.ToString();

        /// <summary>
        /// 常量值的字符串表示（用于 TextBox 双向绑定）。
        /// 设置时将自动根据当前类型解析字符串。
        /// </summary>
        public string ConstantString
        {
            get
            {
                if (Constant.IsNull) return string.Empty;
                if (Constant.TryGetDouble(out var d))
                    return d.ToString(CultureInfo.InvariantCulture);
                if (Constant.TryGetInt32(out var i))
                    return i.ToString();
                if (Constant.TryGetBoolean(out var b))
                    return b.ToString();
                if (Constant.TryGetString(out var s))
                    return s;
                return ConstantDisplay;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Constant = VariantValue.Null;
                    return;
                }

                var typeCode = Output.Count > 0 ? Output[0].ExpectedType : TypeCode.Double;
                switch (typeCode)
                {
                    case TypeCode.Double:
                        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                            Constant = VariantValue.FromDouble(d);
                        break;
                    case TypeCode.Int32:
                        if (int.TryParse(value, out var i))
                            Constant = VariantValue.FromInt32(i);
                        break;
                    case TypeCode.Boolean:
                        if (bool.TryParse(value, out var b))
                            Constant = VariantValue.FromBoolean(b);
                        else if (value == "1") Constant = VariantValue.FromBoolean(true);
                        else if (value == "0") Constant = VariantValue.FromBoolean(false);
                        break;
                    case TypeCode.String:
                        Constant = VariantValue.FromString(value);
                        break;
                    default:
                        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var fallbackD))
                            Constant = VariantValue.FromDouble(fallbackD);
                        break;
                }
            }
        }

        /// <summary>常量值的类型码（用于 UI 绑定）。</summary>
        public TypeCode ConstantType
        {
            get => Output.Count > 0 ? Output[0].ExpectedType : TypeCode.Double;
            set
            {
                if (Output.Count > 0)
                {
                    Output[0].ExpectedType = value;
                    OnPropertyChanged(nameof(ConstantType));
                    OnPropertyChanged(nameof(ConstantString));
                    OnPropertyChanged(nameof(ConstantTypeTag));
                }
            }
        }

        /// <summary>常量类型标签（用于 ComboBox SelectedValue 双向绑定，替代 SelectionChanged 事件）。</summary>
        public string ConstantTypeTag
        {
            get => ConstantType switch
            {
                TypeCode.Double => "Double",
                TypeCode.Int32 => "Int32",
                TypeCode.Boolean => "Boolean",
                TypeCode.String => "String",
                _ => "Double"
            };
            set
            {
                var newType = value switch
                {
                    "Double" => TypeCode.Double,
                    "Int32" => TypeCode.Int32,
                    "Boolean" => TypeCode.Boolean,
                    "String" => TypeCode.String,
                    _ => TypeCode.Double
                };
                var currentVal = Constant;
                var currentDVal = currentVal.TryGetDouble(out var d) ? d : 0;
                ConstantType = newType;
                Constant = newType switch
                {
                    TypeCode.Double => VariantValue.FromDouble(currentDVal),
                    TypeCode.Int32 => VariantValue.FromInt32((int)currentDVal),
                    TypeCode.Boolean => VariantValue.FromBoolean(currentDVal != 0),
                    TypeCode.String => VariantValue.FromString(currentVal.ToString()),
                    _ => VariantValue.FromDouble(currentDVal)
                };
            }
        }

        public override void Execute()
        {
            // 常量节点无需计算，值已在设置时写入输出连接器
        }
    }
}
