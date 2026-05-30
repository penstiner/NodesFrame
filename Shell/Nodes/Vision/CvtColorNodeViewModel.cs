using OpenCvSharp;
using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "色彩转换", DefaultTitle = "色彩转换",
          Description = "转换图像色彩空间（灰度/HSV/Lab 等）", NodeTypeId = "Vision.CvtColor")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class CvtColorNodeViewModel : VisionNodeBase
    {
        public CvtColorNodeViewModel() : base("色彩转换") { }
        private ColorConversionCodes _code = ColorConversionCodes.BGR2GRAY;
        [NodeProperty(Key = "conversionCode")] public ColorConversionCodes ConversionCode { get => _code; set => SetProperty(ref _code, value); }
        private int _sel;
        [NodeProperty(Key = "selectedIndex", DisplayName = "转换类型", Options = "灰度 BGR→GRAY,HSV BGR→HSV,Lab BGR→Lab")]
        public int SelectedIndex { get => _sel; set { if (SetProperty(ref _sel, value)) ConversionCode = value switch { 0 => ColorConversionCodes.BGR2GRAY, 1 => ColorConversionCodes.BGR2HSV, 2 => ColorConversionCodes.BGR2Lab, _ => ColorConversionCodes.BGR2GRAY }; } }
        protected override ImageData ProcessImage(ImageData input) => VisionAlgorithmService.CvtColor(input, ConversionCode);
    }
}