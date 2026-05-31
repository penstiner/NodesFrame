using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "伽马校正", DefaultTitle = "伽马校正",
          Description = "非线性亮度调整，补偿光照不均匀", NodeTypeId = "Vision.GammaCorrection")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class GammaCorrectionNodeViewModel : VisionNodeBase
    {
        public GammaCorrectionNodeViewModel() : base("伽马校正") { }
        private double _gamma = 1.0;
        [NodeProperty(Key = "gamma", DisplayName = "Gamma 值", Group = "伽马参数", Description = "<1 变亮，>1 变暗", Min = 0.1, Max = 5)]
        public double Gamma { get => _gamma; set => SetProperty(ref _gamma, value); }
        protected override ImageData ProcessImage(ImageData input)
            => VisionAlgorithmService.GammaCorrection(input, Gamma);
    }
}
