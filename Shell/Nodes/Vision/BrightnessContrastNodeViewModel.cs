using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "亮度对比度", DefaultTitle = "亮度对比度",
          Description = "调整图像亮度和对比度 (alpha × 像素 + beta)", NodeTypeId = "Vision.BrightnessContrast")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class BrightnessContrastNodeViewModel : VisionNodeBase
    {
        public BrightnessContrastNodeViewModel() : base("亮度对比度") { }
        private double _alpha = 1.2;
        [NodeProperty(Key = "alpha", DisplayName = "对比度 (alpha)", Group = "调整参数", Min = 0, Max = 5)] public double Alpha { get => _alpha; set => SetProperty(ref _alpha, value); }
        private double _beta = 10;
        [NodeProperty(Key = "beta", DisplayName = "亮度 (beta)", Group = "调整参数", Min = -255, Max = 255)]  public double Beta { get => _beta; set => SetProperty(ref _beta, value); }
        protected override ImageData ProcessImage(ImageData input) => VisionAlgorithmService.BrightnessContrast(input, Alpha, Beta);
    }
}
