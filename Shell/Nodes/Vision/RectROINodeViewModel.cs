using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "矩形裁剪", DefaultTitle = "矩形裁剪",
          Description = "矩形 ROI 区域裁剪提取", NodeTypeId = "Vision.RectROI")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class RectROINodeViewModel : VisionNodeBase
    {
        public RectROINodeViewModel() : base("矩形裁剪") { }
        private int _x = 0, _y = 0, _w = 200, _h = 200;
        [NodeProperty(Key = "x", DisplayName = "X", Group = "ROI", Min = 0, Max = 100000)]
        public int X { get => _x; set => SetProperty(ref _x, value); }
        [NodeProperty(Key = "y", DisplayName = "Y", Group = "ROI", Min = 0, Max = 100000)]
        public int Y { get => _y; set => SetProperty(ref _y, value); }
        [NodeProperty(Key = "w", DisplayName = "宽度", Group = "ROI", Min = 1, Max = 100000)]
        public int W { get => _w; set => SetProperty(ref _w, value); }
        [NodeProperty(Key = "h", DisplayName = "高度", Group = "ROI", Min = 1, Max = 100000)]
        public int H { get => _h; set => SetProperty(ref _h, value); }
        protected override ImageData ProcessImage(ImageData input)
            => VisionAlgorithmService.RectROI(input, X, Y, W, H);
    }
}
