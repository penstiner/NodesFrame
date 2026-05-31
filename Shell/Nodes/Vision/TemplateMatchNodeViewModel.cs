using System.Linq;
using OpenCvSharp;
using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    /// <summary>
    /// 模板匹配节点：2 个图像输入（源图 + 模板），输出标注匹配位置的图像。
    /// </summary>
    [Node(Category = "视觉算法", DisplayName = "模板匹配", DefaultTitle = "模板匹配",
          Description = "在源图像中定位模板位置，用于工业定位、缺陷对齐", NodeTypeId = "Vision.TemplateMatch")]
    [NodeConnector(Title = "源图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "模板图", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    [NodeConnector(Title = "匹配度", Direction = ConnectorDirection.Output, ExpectedType = "Double")]
    public class TemplateMatchNodeViewModel : NodeViewModel
    {
        public ImagePreview Preview { get; } = new();

        public TemplateMatchNodeViewModel()
        {
            Title = "模板匹配";
            AddInputConnector(new ConnectorViewModel
            {
                Title = "源图像",
                ExpectedType = System.TypeCode.Object
            });
            AddInputConnector(new ConnectorViewModel
            {
                Title = "模板图",
                ExpectedType = System.TypeCode.Object
            });
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "输出图像",
                ExpectedType = System.TypeCode.Object
            });
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "匹配度",
                ExpectedType = System.TypeCode.Double
            });
        }

        private int _modeIndex = 5;
        [NodeProperty(Key = "mode", DisplayName = "匹配方法", Group = "匹配参数",
            Options = "平方差,归一化平方差,相关系数,归一化相关系数,互相关,归一化互相关")]
        public int ModeIndex { get => _modeIndex; set => SetProperty(ref _modeIndex, value); }

        private string _imageInfo = "等待输入...";
        public string ImageInfo { get => _imageInfo; set => SetProperty(ref _imageInfo, value); }

        public double MatchScore { get; private set; }

        public override void Execute()
        {
            var srcVal = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            var tplVal = Input.ElementAtOrDefault(1)?.Value ?? VariantValue.Null;
            if (!srcVal.TryGetImageData(out var src) || src == null
                || !tplVal.TryGetImageData(out var tpl) || tpl == null)
            { ImageInfo = "等待源图像和模板..."; return; }

            var mode = (TemplateMatchModes)(_modeIndex switch
            {
                0 => 0, 1 => 1, 2 => 2, 3 => 3, 4 => 4, _ => 5
            });

            var result = VisionAlgorithmService.TemplateMatch(src, tpl, mode,
                out double minVal, out double maxVal, out var minLoc, out var maxLoc);

            MatchScore = (mode == TemplateMatchModes.SqDiff || mode == TemplateMatchModes.SqDiffNormed)
                ? 1.0 - minVal : maxVal;

            Output[0].Value = VariantValue.FromImageData(result);
            Output[1].Value = VariantValue.FromDouble(MatchScore);
            ImageInfo = $"匹配度: {MatchScore:F3}";
            Preview.Update(result);
        }
    }
}
