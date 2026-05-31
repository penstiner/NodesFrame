using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "CLAHE 均衡", DefaultTitle = "CLAHE 均衡",
          Description = "自适应直方图均衡，增强局部对比度，抑制噪声放大", NodeTypeId = "Vision.CLAHE")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class CLAHENodeViewModel : VisionNodeBase
    {
        public CLAHENodeViewModel() : base("CLAHE 均衡") { }
        private double _clipLimit = 2.0;
        [NodeProperty(Key = "clipLimit", DisplayName = "对比度限制", Group = "CLAHE参数", Min = 0.1, Max = 10)]
        public double ClipLimit { get => _clipLimit; set => SetProperty(ref _clipLimit, value); }
        private int _tileGridSize = 8;
        [NodeProperty(Key = "tileGridSize", DisplayName = "网格大小", Group = "CLAHE参数", Min = 2, Max = 32)]
        public int TileGridSize { get => _tileGridSize; set => SetProperty(ref _tileGridSize, value); }
        protected override ImageData ProcessImage(ImageData input)
            => VisionAlgorithmService.CLAHE(input, ClipLimit, TileGridSize);
    }
}
