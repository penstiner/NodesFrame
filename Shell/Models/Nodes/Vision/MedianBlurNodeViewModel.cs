using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "中值滤波", DefaultTitle = "中值滤波",
          Description = "中值滤波，有效去除椒盐噪声", NodeTypeId = "Vision.MedianBlur")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class MedianBlurNodeViewModel : VisionNodeBase
    {
        public MedianBlurNodeViewModel() : base("中值滤波") { }
        private int _ksize = 5;
        [NodeProperty(Key = "kernelSize", DisplayName = "核大小 (奇数)")] public int KernelSize { get => _ksize; set => SetProperty(ref _ksize, value); }
        protected override byte[] ProcessImage(byte[] input) => VisionAlgorithmService.MedianBlur(input, KernelSize);
    }
}
