using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "形态学梯度", DefaultTitle = "形态学梯度",
          Description = "形态学梯度（膨胀-腐蚀），突出物体边缘", NodeTypeId = "Vision.MorphGradient")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class MorphGradientNodeViewModel : VisionNodeBase
    {
        public MorphGradientNodeViewModel() : base("形态学梯度") { }
        private int _shapeIdx = 1;
        [NodeProperty(Key = "shape", DisplayName = "核形状", Group = "梯度参数",
            Options = "矩形,椭圆,十字")]
        public int ShapeIdx { get => _shapeIdx; set => SetProperty(ref _shapeIdx, value); }
        private int _ksize = 3;
        [NodeProperty(Key = "ksize", DisplayName = "核大小", Group = "梯度参数", Min = 3, Max = 31)]
        public int KSize { get => _ksize; set => SetProperty(ref _ksize, value); }
        protected override ImageData ProcessImage(ImageData input)
        {
            var shape = (OpenCvSharp.MorphShapes)(_shapeIdx switch
            {
                0 => 0, 2 => 2, _ => 1
            });
            return VisionAlgorithmService.MorphGradient(input, shape, KSize);
        }
    }
}
