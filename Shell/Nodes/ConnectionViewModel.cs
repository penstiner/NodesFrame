using System;
using Nodify;

namespace Shell.Models
{
    public class ConnectionViewModel : ObservableObject
    {
        public Guid Id { get; } = Guid.NewGuid();

        public ConnectorViewModel Source { get; }
        public ConnectorViewModel Target { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// 连接是否类型兼容。不兼容时仍可建立连接但会标记警告。
        /// </summary>
        public bool IsTypeCompatible { get; }

        public ConnectionViewModel(ConnectorViewModel source, ConnectorViewModel target)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target ?? throw new ArgumentNullException(nameof(target));

            Source.IsConnected = true;
            Target.IsConnected = true;

            // 检查类型兼容性
            IsTypeCompatible = VariantValue.AreTypesCompatible(
                source.ExpectedType, target.ExpectedType);

            Source.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ConnectorViewModel.Value))
                {
                    Target.Value = Source.Value;
                }
            };
            Target.Value = Source.Value;
        }
    }
}
