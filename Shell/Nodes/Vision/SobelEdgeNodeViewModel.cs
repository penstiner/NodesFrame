using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "Sobel 边缘", DefaultTitle = "Sobel 边缘",
          Description = "Sobel 梯度边缘检测", NodeTypeId = "Vision.SobelEdge")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class SobelEdgeNodeViewModel : VisionNodeBase
    {
        public SobelEdgeNodeViewModel() : base("Sobel 边缘") { }
        private int _ksize = 3;
        [NodeProperty(Key = "ksize", DisplayName = "核大小", Group = "Sobel参数", Min = 1, Max = 7)]
        public int KSize { get => _ksize; set => SetProperty(ref _ksize, value); }
        private double _scale = 1.0;
        [NodeProperty(Key = "scale", DisplayName = "缩放", Group = "Sobel参数", Min = 0.1, Max = 5)]
        public double Scale { get => _scale; set => SetProperty(ref _scale, value); }
        protected override ImageData ProcessImage(ImageData input)
            => VisionAlgorithmService.SobelEdge(input, KSize, Scale);
    }
}
