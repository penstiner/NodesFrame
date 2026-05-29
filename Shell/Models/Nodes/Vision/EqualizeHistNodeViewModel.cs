using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "直方图均衡", DefaultTitle = "直方图均衡",
          Description = "对灰度图进行直方图均衡化，增强图像对比度", NodeTypeId = "Vision.EqualizeHist")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    public class EqualizeHistNodeViewModel : VisionNodeBase
    {
        public EqualizeHistNodeViewModel() : base("直方图均衡") { }
        protected override byte[] ProcessImage(byte[] input) => VisionAlgorithmService.EqualizeHist(input);
    }
}
