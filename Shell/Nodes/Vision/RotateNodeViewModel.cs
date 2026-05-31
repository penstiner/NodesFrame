using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "图像旋转", DefaultTitle = "图像旋转",
          Description = "按角度旋转图像", NodeTypeId = "Vision.Rotate")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class RotateNodeViewModel : VisionNodeBase
    {
        public RotateNodeViewModel() : base("图像旋转") { }
        private double _angle = 90;
        [NodeProperty(Key = "angle", DisplayName = "旋转角度", Group = "旋转参数", Min = -360, Max = 360)]
        public double Angle { get => _angle; set => SetProperty(ref _angle, value); }
        protected override ImageData ProcessImage(ImageData input)
            => VisionAlgorithmService.Rotate(input, Angle);
    }
}
