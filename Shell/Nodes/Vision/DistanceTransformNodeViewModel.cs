using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "距离变换", DefaultTitle = "距离变换",
          Description = "计算前景像素到背景的距离图", NodeTypeId = "Vision.DistanceTransform")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class DistanceTransformNodeViewModel : VisionNodeBase
    {
        public DistanceTransformNodeViewModel() : base("距离变换") { }
        private int _distType = 2;
        [NodeProperty(Key = "distType", DisplayName = "距离类型", Group = "参数",
            Options = "L1 曼哈顿,L2 欧几里得,C 棋盘")]
        public int DistType { get => _distType; set => SetProperty(ref _distType, value); }
        protected override ImageData ProcessImage(ImageData input)
        {
            var type = _distType switch
            {
                0 => OpenCvSharp.DistanceTypes.L1,
                2 => OpenCvSharp.DistanceTypes.C,
                _ => OpenCvSharp.DistanceTypes.L2,
            };
            return VisionAlgorithmService.DistanceTransform(input, type);
        }
    }
}
