using OpenCvSharp;
using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "形态学", DefaultTitle = "形态学",
          Description = "图像的膨胀/腐蚀/开运算/闭运算", NodeTypeId = "Vision.Morphology")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class MorphologyNodeViewModel : VisionNodeBase
    {
        public MorphologyNodeViewModel() : base("形态学") { }
        private MorphTypes _op = MorphTypes.Dilate;
        [NodeProperty(Key = "morphOp")] public MorphTypes MorphOp { get => _op; set => SetProperty(ref _op, value); }
        private int _ks = 5;
        [NodeProperty(Key = "kernelSize", DisplayName = "核大小", Group = "形态学参数", Min = 3, Max = 31)] public int KernelSize { get => _ks; set => SetProperty(ref _ks, value); }
        private int _iter = 1;
        [NodeProperty(Key = "iterations", DisplayName = "迭代次数", Group = "形态学参数", Min = 1, Max = 20)] public int Iterations { get => _iter; set => SetProperty(ref _iter, value); }
        private int _si;
        [NodeProperty(Key = "selectedOpIndex", DisplayName = "操作类型", Options = "膨胀 (Dilate),腐蚀 (Erode),开运算 (Open),闭运算 (Close)")]
        public int SelectedOpIndex { get => _si; set { if (SetProperty(ref _si, value)) MorphOp = value switch { 0 => MorphTypes.Dilate, 1 => MorphTypes.Erode, 2 => MorphTypes.Open, 3 => MorphTypes.Close, _ => MorphTypes.Dilate }; } }
        protected override ImageData ProcessImage(ImageData input) => VisionAlgorithmService.MorphologyOp(input, MorphOp, KernelSize, Iterations);
    }
}