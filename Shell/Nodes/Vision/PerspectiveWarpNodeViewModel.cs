using OpenCvSharp;
using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "透视矫正", DefaultTitle = "透视矫正",
          Description = "四点透视变换，矫正倾斜/畸变图像", NodeTypeId = "Vision.PerspectiveWarp")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class PerspectiveWarpNodeViewModel : VisionNodeBase
    {
        public PerspectiveWarpNodeViewModel() : base("透视矫正") { }
        private int _x1 = 0, _y1 = 0, _x2 = 100, _y2 = 0, _x3 = 100, _y3 = 100, _x4 = 0, _y4 = 100;
        private int _dstW = 200, _dstH = 200;
        [NodeProperty(Key = "x1", DisplayName = "左上X", Group = "源四点", Min = 0, Max = 10000)]
        public int X1 { get => _x1; set => SetProperty(ref _x1, value); }
        [NodeProperty(Key = "y1", DisplayName = "左上Y", Group = "源四点", Min = 0, Max = 10000)]
        public int Y1 { get => _y1; set => SetProperty(ref _y1, value); }
        [NodeProperty(Key = "x2", DisplayName = "右上X", Group = "源四点", Min = 0, Max = 10000)]
        public int X2 { get => _x2; set => SetProperty(ref _x2, value); }
        [NodeProperty(Key = "y2", DisplayName = "右上Y", Group = "源四点", Min = 0, Max = 10000)]
        public int Y2 { get => _y2; set => SetProperty(ref _y2, value); }
        [NodeProperty(Key = "x3", DisplayName = "右下X", Group = "源四点", Min = 0, Max = 10000)]
        public int X3 { get => _x3; set => SetProperty(ref _x3, value); }
        [NodeProperty(Key = "y3", DisplayName = "右下Y", Group = "源四点", Min = 0, Max = 10000)]
        public int Y3 { get => _y3; set => SetProperty(ref _y3, value); }
        [NodeProperty(Key = "x4", DisplayName = "左下X", Group = "源四点", Min = 0, Max = 10000)]
        public int X4 { get => _x4; set => SetProperty(ref _x4, value); }
        [NodeProperty(Key = "y4", DisplayName = "左下Y", Group = "源四点", Min = 0, Max = 10000)]
        public int Y4 { get => _y4; set => SetProperty(ref _y4, value); }
        [NodeProperty(Key = "dstW", DisplayName = "目标宽度", Group = "目标尺寸", Min = 1, Max = 10000)]
        public int DstW { get => _dstW; set => SetProperty(ref _dstW, value); }
        [NodeProperty(Key = "dstH", DisplayName = "目标高度", Group = "目标尺寸", Min = 1, Max = 10000)]
        public int DstH { get => _dstH; set => SetProperty(ref _dstH, value); }
        protected override ImageData ProcessImage(ImageData input)
        {
            var srcPts = new Point2f[]
            {
                new Point2f(X1, Y1), new Point2f(X2, Y2),
                new Point2f(X3, Y3), new Point2f(X4, Y4)
            };
            return VisionAlgorithmService.PerspectiveWarp(input, srcPts, DstW, DstH);
        }
    }
}
