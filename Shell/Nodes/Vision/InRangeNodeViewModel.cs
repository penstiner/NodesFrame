using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "颜色提取", DefaultTitle = "颜色提取",
          Description = "HSV 颜色范围过滤提取", NodeTypeId = "Vision.InRange")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class InRangeNodeViewModel : VisionNodeBase
    {
        public InRangeNodeViewModel() : base("颜色提取") { }
        private int _hMin = 0, _sMin = 0, _vMin = 0;
        private int _hMax = 180, _sMax = 255, _vMax = 255;
        [NodeProperty(Key = "hMin", DisplayName = "H 最小值", Group = "HSV范围", Min = 0, Max = 180)]
        public int HMin { get => _hMin; set => SetProperty(ref _hMin, value); }
        [NodeProperty(Key = "sMin", DisplayName = "S 最小值", Group = "HSV范围", Min = 0, Max = 255)]
        public int SMin { get => _sMin; set => SetProperty(ref _sMin, value); }
        [NodeProperty(Key = "vMin", DisplayName = "V 最小值", Group = "HSV范围", Min = 0, Max = 255)]
        public int VMin { get => _vMin; set => SetProperty(ref _vMin, value); }
        [NodeProperty(Key = "hMax", DisplayName = "H 最大值", Group = "HSV范围", Min = 0, Max = 180)]
        public int HMax { get => _hMax; set => SetProperty(ref _hMax, value); }
        [NodeProperty(Key = "sMax", DisplayName = "S 最大值", Group = "HSV范围", Min = 0, Max = 255)]
        public int SMax { get => _sMax; set => SetProperty(ref _sMax, value); }
        [NodeProperty(Key = "vMax", DisplayName = "V 最大值", Group = "HSV范围", Min = 0, Max = 255)]
        public int VMax { get => _vMax; set => SetProperty(ref _vMax, value); }
        protected override ImageData ProcessImage(ImageData input)
            => VisionAlgorithmService.InRange(input, HMin, SMin, VMin, HMax, SMax, VMax);
    }
}
