using System.Windows;
using System.Windows.Controls;
using Shell.Models;
using Shell.Models.Nodes.Vision;
using Shell.Models.Nodes.Motion;

namespace Shell.Views
{
    public class NodeEditTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ConstantEditTemplate { get; set; }
        public DataTemplate FunctionEditTemplate { get; set; }
        public DataTemplate DisplayEditTemplate { get; set; }
        public DataTemplate DelayEditTemplate { get; set; }
        public DataTemplate ConditionEditTemplate { get; set; }
        public DataTemplate LoopEditTemplate { get; set; }
        public DataTemplate ImageSourceEditTemplate { get; set; }
        public DataTemplate GaussianBlurEditTemplate { get; set; }
        public DataTemplate CvtColorEditTemplate { get; set; }
        public DataTemplate ThresholdEditTemplate { get; set; }
        public DataTemplate ResizeEditTemplate { get; set; }
        public DataTemplate CannyEdgeEditTemplate { get; set; }
        public DataTemplate MorphologyEditTemplate { get; set; }
        public DataTemplate ImageDisplayEditTemplate { get; set; }
        public DataTemplate MultiAxisMoveEditTemplate { get; set; }
        public DataTemplate AwaitInputEditTemplate { get; set; }
        public DataTemplate OutputSignalEditTemplate { get; set; }
        public DataTemplate GenericEditTemplate { get; set; }
        public DataTemplate DefaultEditTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                ConstantNodeViewModel => ConstantEditTemplate ?? DefaultEditTemplate,
                FunctionNodeViewModel => FunctionEditTemplate ?? DefaultEditTemplate,
                DisplayNodeViewModel => DisplayEditTemplate ?? DefaultEditTemplate,
                DelayNodeViewModel => DelayEditTemplate ?? DefaultEditTemplate,
                ConditionNodeViewModel => ConditionEditTemplate ?? DefaultEditTemplate,
                LoopNodeViewModel => LoopEditTemplate ?? DefaultEditTemplate,
                ImageSourceNodeViewModel => ImageSourceEditTemplate ?? DefaultEditTemplate,
                GaussianBlurNodeViewModel => GaussianBlurEditTemplate ?? DefaultEditTemplate,
                CvtColorNodeViewModel => CvtColorEditTemplate ?? DefaultEditTemplate,
                ThresholdNodeViewModel => ThresholdEditTemplate ?? DefaultEditTemplate,
                ResizeNodeViewModel => ResizeEditTemplate ?? DefaultEditTemplate,
                CannyEdgeNodeViewModel => CannyEdgeEditTemplate ?? DefaultEditTemplate,
                MorphologyNodeViewModel => MorphologyEditTemplate ?? DefaultEditTemplate,
                ImageDisplayNodeViewModel => ImageDisplayEditTemplate ?? DefaultEditTemplate,
                MultiAxisMoveNodeViewModel => MultiAxisMoveEditTemplate ?? DefaultEditTemplate,
                AwaitInPutNodeViewModel => AwaitInputEditTemplate ?? DefaultEditTemplate,
                OutputSignalNodeViewModel => OutputSignalEditTemplate ?? DefaultEditTemplate,
                // ── 通用反射编辑器：所有带 [NodeProperty] 的节点自动使用 ──
                VisionNodeBase => GenericEditTemplate ?? DefaultEditTemplate,
                _ => GenericEditTemplate ?? DefaultEditTemplate
            };
        }
    }
}
