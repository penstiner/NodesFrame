using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "图像锐化", DefaultTitle = "图像锐化",
          Description = "Laplacian 锐化增强图像边缘", NodeTypeId = "Vision.Sharpen")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class SharpenNodeViewModel : VisionNodeBase
    {
        public SharpenNodeViewModel() : base("图像锐化") { }
        private double _strength = 1.5;
        [NodeProperty(Key = "strength", DisplayName = "锐化强度", Group = "锐化参数", Min = 0, Max = 5)]
        public double Strength { get => _strength; set => SetProperty(ref _strength, value); }
        protected override ImageData ProcessImage(ImageData input)
            => VisionAlgorithmService.Sharpen(input, Strength);
    }
}
