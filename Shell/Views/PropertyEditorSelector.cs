using System.Windows;
using System.Windows.Controls;
using Shell.Models;

namespace Shell.Views
{
    public class PropertyEditorSelector : DataTemplateSelector
    {
        public DataTemplate NumberTemplate { get; set; }
        public DataTemplate BooleanTemplate { get; set; }
        public DataTemplate EnumTemplate { get; set; }
        public DataTemplate TextTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is PropertyItem p)
            {
                return p.EditorType switch
                {
                    "Number"  => NumberTemplate,
                    "Boolean" => BooleanTemplate,
                    "Enum"    => EnumTemplate,
                    _         => TextTemplate
                };
            }
            return base.SelectTemplate(item, container);
        }
    }
}
