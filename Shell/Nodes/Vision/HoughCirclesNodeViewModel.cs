using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "霍夫圆检测", DefaultTitle = "霍夫圆检测",
          Description = "检测图像中的圆形目标（工业零件、孔洞等）", NodeTypeId = "Vision.HoughCircles")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    [NodeConnector(Title = "圆数量", Direction = ConnectorDirection.Output, ExpectedType = "Int32")]
    public class HoughCirclesNodeViewModel : VisionNodeBase
    {
        public HoughCirclesNodeViewModel() : base("霍夫圆检测")
        {
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "圆数量",
                ExpectedType = System.TypeCode.Int32
            });
        }
        private double _dp = 1.5;
        [NodeProperty(Key = "dp", DisplayName = "累加器分辨率", Group = "圆检测参数", Min = 1, Max = 5)]
        public double Dp { get => _dp; set => SetProperty(ref _dp, value); }
        private double _minDist = 50;
        [NodeProperty(Key = "minDist", DisplayName = "最小圆心距", Group = "圆检测参数", Min = 1, Max = 1000)]
        public double MinDist { get => _minDist; set => SetProperty(ref _minDist, value); }
        private double _param1 = 100;
        [NodeProperty(Key = "param1", DisplayName = "Canny高阈值", Group = "圆检测参数", Min = 1, Max = 500)]
        public double Param1 { get => _param1; set => SetProperty(ref _param1, value); }
        private double _param2 = 60;
        [NodeProperty(Key = "param2", DisplayName = "圆心阈值", Group = "圆检测参数", Min = 1, Max = 500)]
        public double Param2 { get => _param2; set => SetProperty(ref _param2, value); }
        private int _minRadius = 10;
        [NodeProperty(Key = "minRadius", DisplayName = "最小半径", Group = "圆检测参数", Min = 1, Max = 1000)]
        public int MinRadius { get => _minRadius; set => SetProperty(ref _minRadius, value); }
        private int _maxRadius = 200;
        [NodeProperty(Key = "maxRadius", DisplayName = "最大半径", Group = "圆检测参数", Min = 1, Max = 2000)]
        public int MaxRadius { get => _maxRadius; set => SetProperty(ref _maxRadius, value); }
        public int CircleCount { get; private set; }
        protected override ImageData ProcessImage(ImageData input)
        {
            var result = VisionAlgorithmService.HoughCircles(input,
                Dp, MinDist, Param1, Param2, MinRadius, MaxRadius, out int count);
            CircleCount = count;
            if (Output.Count > 1) Output[1].Value = VariantValue.FromInt32(count);
            return result;
        }
    }
}
