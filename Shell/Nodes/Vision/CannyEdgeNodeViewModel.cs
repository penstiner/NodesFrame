using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "Canny 边缘", DefaultTitle = "Canny 边缘",
          Description = "Canny 边缘检测，提取图像中的边缘线条", NodeTypeId = "Vision.CannyEdge")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class CannyEdgeNodeViewModel : VisionNodeBase
    {
        public CannyEdgeNodeViewModel() : base("Canny 边缘") { }
        private double _t1 = 50;
        [NodeProperty(Key = "threshold1", DisplayName = "低阈值", Group = "边缘检测", Min = 0, Max = 500)] public double Threshold1 { get => _t1; set => SetProperty(ref _t1, value); }
        private double _t2 = 150;
        [NodeProperty(Key = "threshold2", DisplayName = "高阈值", Group = "边缘检测", Min = 0, Max = 500)] public double Threshold2 { get => _t2; set => SetProperty(ref _t2, value); }
        protected override ImageData ProcessImage(ImageData input) => VisionAlgorithmService.CannyEdge(input, Threshold1, Threshold2);
    }
}