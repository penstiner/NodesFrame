using System.Linq;
using Shell.Models.Attributes;

namespace Shell.Models
{
    [Node(Category = "输出", DisplayName = "显示", DefaultTitle = "显示",
          Description = "显示输入值（支持多类型格式化）", NodeTypeId = "Display")]
    [NodeConnector(Title = "In", Direction = ConnectorDirection.Input, ExpectedType = "Double")]
    public class DisplayNodeViewModel : NodeViewModel
    {
        public DisplayNodeViewModel()
        {
            AddInputConnector(new ConnectorViewModel { Title = "In" });
        }

        /// <summary>显示值（支持多种类型格式化显示）。</summary>
        public VariantValue DisplayValue =>
            Input.Count > 0 ? Input[0].Value : VariantValue.Null;

        /// <summary>显示值的格式化文本。</summary>
        public string DisplayText => DisplayValue.ToString();

        /// <summary>是否有值可以显示。</summary>
        public bool HasValue => !DisplayValue.IsNull;

        public override void Execute()
        {
            OnPropertyChanged(nameof(DisplayValue));
            OnPropertyChanged(nameof(DisplayText));
            OnPropertyChanged(nameof(HasValue));
        }
    }
}
