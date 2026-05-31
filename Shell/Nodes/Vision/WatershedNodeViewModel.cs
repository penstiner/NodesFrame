using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "分水岭分割", DefaultTitle = "分水岭分割",
          Description = "基于标记的分水岭分割，分离粘连物体", NodeTypeId = "Vision.Watershed")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class WatershedNodeViewModel : VisionNodeBase
    {
        public WatershedNodeViewModel() : base("分水岭分割") { }
        private double _fgThresh = 0.4;
        [NodeProperty(Key = "fgThresh", DisplayName = "前景阈值", Group = "分割参数", Min = 0, Max = 1)]
        public double FgThresh { get => _fgThresh; set => SetProperty(ref _fgThresh, value); }
        private double _bgThresh = 0.3;
        [NodeProperty(Key = "bgThresh", DisplayName = "背景阈值", Group = "分割参数", Min = 0, Max = 1)]
        public double BgThresh { get => _bgThresh; set => SetProperty(ref _bgThresh, value); }
        protected override ImageData ProcessImage(ImageData input)
            => VisionAlgorithmService.Watershed(input, FgThresh * 255, BgThresh * 255);
    }
}
