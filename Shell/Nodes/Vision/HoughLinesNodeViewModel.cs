using System.Linq;
using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "霍夫直线", DefaultTitle = "霍夫直线",
          Description = "霍夫变换检测图像中的直线段并绘制", NodeTypeId = "Vision.HoughLines")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class HoughLinesNodeViewModel : VisionNodeBase
    {
        public HoughLinesNodeViewModel() : base("霍夫直线")
        {
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "线段数",
                ExpectedType = System.TypeCode.Int32
            });
        }
        private double _rho = 1;
        [NodeProperty(Key = "rho", DisplayName = "ρ 分辨率", Group = "霍夫参数")] public double Rho { get => _rho; set => SetProperty(ref _rho, value); }
        private double _theta = Math.PI / 180;
        [NodeProperty(Key = "theta", DisplayName = "θ 角度 (弧度)", Group = "霍夫参数")] public double Theta { get => _theta; set => SetProperty(ref _theta, value); }
        private int _threshold = 80;
        [NodeProperty(Key = "houghThreshold", DisplayName = "投票阈值", Group = "检测参数")] public int HoughThreshold { get => _threshold; set => SetProperty(ref _threshold, value); }
        private double _minLen = 50;
        [NodeProperty(Key = "minLineLength", DisplayName = "最小线段长度", Group = "检测参数")] public double MinLineLength { get => _minLen; set => SetProperty(ref _minLen, value); }
        private double _maxGap = 10;
        [NodeProperty(Key = "maxLineGap", DisplayName = "最大间隙", Group = "检测参数")] public double MaxLineGap { get => _maxGap; set => SetProperty(ref _maxGap, value); }

        private int _lineCount;
        /// <summary>检测到的线段数量。</summary>
        public int LineCount
        {
            get => _lineCount;
            set => SetProperty(ref _lineCount, value);
        }

        protected override ImageData ProcessImage(ImageData input)
        {
            var result = VisionAlgorithmService.HoughLinesP(
                input, Rho, Theta, HoughThreshold, MinLineLength, MaxLineGap, out var count);
            LineCount = count;
            if (Output.Count > 1)
                Output[1].Value = VariantValue.FromInt32(count);
            return result;
        }

        public override void Execute()
        {
            var inputVal = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            if (!inputVal.TryGetImageData(out var imageData) || imageData == null)
            {
                ImageInfo = "等待输入图像...";
                return;
            }

            var result = ProcessImage(imageData);
            if (result != null)
            {
                if (Output.Count > 0)
                    Output[0].Value = VariantValue.FromImageData(result);

                ImageInfo = $"{result.InfoText}, {LineCount} 条线";
                Preview.Update(result);
            }
        }
    }
}
