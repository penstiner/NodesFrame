using OpenCvSharp;
using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "自适应二值化", DefaultTitle = "自适应二值化",
          Description = "根据局部区域自动计算阈值进行二值化", NodeTypeId = "Vision.AdaptiveThreshold")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class AdaptiveThresholdNodeViewModel : VisionNodeBase
    {
        public AdaptiveThresholdNodeViewModel() : base("自适应二值化") { }
        private double _maxVal = 255;
        [NodeProperty(Key = "maxValue", DisplayName = "最大值", Group = "阈值参数")] public double MaxValue { get => _maxVal; set => SetProperty(ref _maxVal, value); }
        private AdaptiveThresholdTypes _adaptiveMethod = AdaptiveThresholdTypes.GaussianC;
        [NodeProperty(Key = "adaptiveMethod")] public AdaptiveThresholdTypes AdaptiveMethod { get => _adaptiveMethod; set => SetProperty(ref _adaptiveMethod, value); }
        private int _blockSize = 11;
        [NodeProperty(Key = "blockSize", DisplayName = "块大小 (奇数)", Group = "阈值参数")] public int BlockSize { get => _blockSize; set => SetProperty(ref _blockSize, value); }
        private double _c = 2;
        [NodeProperty(Key = "c", DisplayName = "常数 C", Group = "阈值参数")] public double C { get => _c; set => SetProperty(ref _c, value); }
        protected override byte[] ProcessImage(byte[] input) =>
            VisionAlgorithmService.AdaptiveThreshold(input, MaxValue, AdaptiveMethod, ThresholdTypes.Binary, BlockSize, C);
    }
}
