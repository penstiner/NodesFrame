using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "高斯模糊", DefaultTitle = "高斯模糊",
          Description = "对图像应用高斯模糊，可调节核大小和 Sigma", NodeTypeId = "Vision.GaussianBlur")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class GaussianBlurNodeViewModel : VisionNodeBase
    {
        public GaussianBlurNodeViewModel() : base("高斯模糊") { }
        private int _kernelSize = 5;
        [NodeProperty(Key = "kernelSize", DisplayName = "核大小", Group = "模糊参数")] public int KernelSize { get => _kernelSize; set => SetProperty(ref _kernelSize, value); }
        private double _sigmaX = 1.5;
        [NodeProperty(Key = "sigmaX", DisplayName = "Sigma X", Group = "模糊参数")] public double SigmaX { get => _sigmaX; set => SetProperty(ref _sigmaX, value); }
        protected override byte[] ProcessImage(byte[] input) => VisionAlgorithmService.GaussianBlur(input, KernelSize, SigmaX);
    }
}