using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "高斯噪声", DefaultTitle = "高斯噪声",
          Description = "添加高斯噪声，模拟工业相机噪声测试算法鲁棒性", NodeTypeId = "Vision.GaussianNoise")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class GaussianNoiseNodeViewModel : VisionNodeBase
    {
        public GaussianNoiseNodeViewModel() : base("高斯噪声") { }
        private double _mean = 0;
        [NodeProperty(Key = "mean", DisplayName = "均值", Group = "噪声参数", Min = -100, Max = 100)]
        public double Mean { get => _mean; set => SetProperty(ref _mean, value); }
        private double _stddev = 25;
        [NodeProperty(Key = "stddev", DisplayName = "标准差", Group = "噪声参数", Min = 0, Max = 255)]
        public double Stddev { get => _stddev; set => SetProperty(ref _stddev, value); }
        protected override ImageData ProcessImage(ImageData input)
            => VisionAlgorithmService.AddGaussianNoise(input, Mean, Stddev);
    }
}
