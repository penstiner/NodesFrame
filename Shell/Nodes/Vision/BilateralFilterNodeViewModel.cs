using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "双边滤波", DefaultTitle = "双边滤波",
          Description = "保边去噪滤波，类似美颜效果", NodeTypeId = "Vision.BilateralFilter")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class BilateralFilterNodeViewModel : VisionNodeBase
    {
        public BilateralFilterNodeViewModel() : base("双边滤波") { }
        private int _d = 9;
        [NodeProperty(Key = "d", DisplayName = "直径", Group = "滤波参数", Min = 1, Max = 50)]
        public int D { get => _d; set => SetProperty(ref _d, value); }
        private double _sigmaColor = 75;
        [NodeProperty(Key = "sigmaColor", DisplayName = "Sigma颜色", Group = "滤波参数", Min = 1, Max = 200)]
        public double SigmaColor { get => _sigmaColor; set => SetProperty(ref _sigmaColor, value); }
        private double _sigmaSpace = 75;
        [NodeProperty(Key = "sigmaSpace", DisplayName = "Sigma空间", Group = "滤波参数", Min = 1, Max = 200)]
        public double SigmaSpace { get => _sigmaSpace; set => SetProperty(ref _sigmaSpace, value); }
        protected override ImageData ProcessImage(ImageData input)
            => VisionAlgorithmService.BilateralFilter(input, D, SigmaColor, SigmaSpace);
    }
}
