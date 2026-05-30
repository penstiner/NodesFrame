using OpenCvSharp;
using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "图像翻转", DefaultTitle = "图像翻转",
          Description = "水平/垂直/双向翻转图像", NodeTypeId = "Vision.Flip")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class FlipNodeViewModel : VisionNodeBase
    {
        public FlipNodeViewModel() : base("图像翻转") { }
        private FlipMode _mode = FlipMode.X;
        [NodeProperty(Key = "flipMode")] public FlipMode Mode { get => _mode; set => SetProperty(ref _mode, value); }
        private int _si;
        [NodeProperty(Key = "flipMode", DisplayName = "翻转方向", Options = "水平翻转,垂直翻转,双向翻转")]
        public int SelectedModeIndex { get => _si; set { if (SetProperty(ref _si, value)) Mode = value switch { 0 => FlipMode.X, 1 => FlipMode.Y, 2 => FlipMode.XY, _ => FlipMode.X }; } }
        protected override ImageData ProcessImage(ImageData input) => VisionAlgorithmService.Flip(input, Mode);
    }
}
