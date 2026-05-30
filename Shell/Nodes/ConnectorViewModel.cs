using System;
using System.Windows;
using Nodify;

namespace Shell.Models
{
    public class ConnectorViewModel : ObservableObject
    {
        public NodeViewModel ParentNode { get; internal set; }

        public ConnectorViewModel()
        {
            PropertyChangedDispatcher = action => action();
        }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>连接器的通用值（支持 double/int/bool/string/byte[] 等多种类型）。</summary>
        private VariantValue _value = VariantValue.Null;
        public VariantValue Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        /// <summary>此连接器期望的数据类型。连接时校验 Source 类型是否与此兼容。</summary>
        private TypeCode _expectedType = TypeCode.Double;
        public TypeCode ExpectedType
        {
            get => _expectedType;
            set => SetProperty(ref _expectedType, value);
        }

        /// <summary>便捷属性：以 double 形式获取/设置值（向后兼容）。</summary>
        public double? DoubleValue
        {
            get => Value.TryGetDouble(out var d) ? d : null;
            set => Value = value.HasValue ? VariantValue.FromDouble(value.Value) : VariantValue.Null;
        }

        private Point _anchor;
        public Point Anchor
        {
            get => _anchor;
            set => SetProperty(ref _anchor, value);
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }
    }
}
