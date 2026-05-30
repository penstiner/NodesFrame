using OpenCvSharp;
using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "二值化", DefaultTitle = "二值化",
          Description = "将图像按阈值转换为黑白二值图像", NodeTypeId = "Vision.Threshold")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class ThresholdNodeViewModel : VisionNodeBase
    {
        public ThresholdNodeViewModel() : base("二值化") { }
        private double _thresh = 127;
        [NodeProperty(Key = "threshold", DisplayName = "阈值", Group = "二值化")] public double ThresholdValue { get => _thresh; set => SetProperty(ref _thresh, value); }
        private double _maxVal = 255;
        [NodeProperty(Key = "maxValue", DisplayName = "最大值", Group = "二值化")] public double MaxValue { get => _maxVal; set => SetProperty(ref _maxVal, value); }
        private ThresholdTypes _type = ThresholdTypes.Binary;
        [NodeProperty(Key = "thresholdType")] public ThresholdTypes ThresholdType { get => _type; set => SetProperty(ref _type, value); }
        private int _si;
        [NodeProperty(Key = "thresholdTypeIndex", DisplayName = "阈值类型", Options = "Binary,BinaryInv,Otsu")]
        public int SelectedTypeIndex { get => _si; set { if (SetProperty(ref _si, value)) ThresholdType = value switch { 0 => ThresholdTypes.Binary, 1 => ThresholdTypes.BinaryInv, 2 => ThresholdTypes.Otsu, _ => ThresholdTypes.Binary }; } }
        protected override ImageData ProcessImage(ImageData input) => VisionAlgorithmService.Threshold(input, ThresholdValue, MaxValue, ThresholdType);
    }
}