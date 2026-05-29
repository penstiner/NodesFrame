using OpenCvSharp;
using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "缩放", DefaultTitle = "缩放",
          Description = "按比例或指定尺寸缩放图像", NodeTypeId = "Vision.Resize")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class ResizeNodeViewModel : VisionNodeBase
    {
        public ResizeNodeViewModel() : base("缩放") { }
        private bool _useScale = true;
        [NodeProperty(Key = "useScaleMode", DisplayName = "比例缩放模式")] public bool UseScaleMode { get => _useScale; set => SetProperty(ref _useScale, value); }
        private double _sx = 0.5;
        [NodeProperty(Key = "scaleX", DisplayName = "X 缩放比例", Group = "缩放参数")] public double ScaleX { get => _sx; set => SetProperty(ref _sx, value); }
        private double _sy = 0.5;
        [NodeProperty(Key = "scaleY", DisplayName = "Y 缩放比例", Group = "缩放参数")] public double ScaleY { get => _sy; set => SetProperty(ref _sy, value); }
        private int _tw = 320;
        [NodeProperty(Key = "targetWidth", DisplayName = "目标宽度", Group = "缩放参数")] public int TargetWidth { get => _tw; set => SetProperty(ref _tw, value); }
        private int _th = 240;
        [NodeProperty(Key = "targetHeight", DisplayName = "目标高度", Group = "缩放参数")] public int TargetHeight { get => _th; set => SetProperty(ref _th, value); }
        protected override byte[] ProcessImage(byte[] input) => UseScaleMode
            ? VisionAlgorithmService.Resize(input, ScaleX, ScaleY)
            : VisionAlgorithmService.ResizeToSize(input, TargetWidth, TargetHeight);
    }
}