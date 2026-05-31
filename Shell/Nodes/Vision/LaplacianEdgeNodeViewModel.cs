using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "Laplacian 边缘", DefaultTitle = "Laplacian 边缘",
          Description = "拉普拉斯二阶微分边缘检测", NodeTypeId = "Vision.LaplacianEdge")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class LaplacianEdgeNodeViewModel : VisionNodeBase
    {
        public LaplacianEdgeNodeViewModel() : base("Laplacian 边缘") { }
        private int _ksize = 3;
        [NodeProperty(Key = "ksize", DisplayName = "核大小", Group = "Laplacian参数", Min = 1, Max = 7)]
        public int KSize { get => _ksize; set => SetProperty(ref _ksize, value); }
        protected override ImageData ProcessImage(ImageData input)
            => VisionAlgorithmService.LaplacianEdge(input, KSize);
    }
}
