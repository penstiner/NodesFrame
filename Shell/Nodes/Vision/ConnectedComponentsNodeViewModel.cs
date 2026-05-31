using System.Linq;
using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(Category = "视觉算法", DisplayName = "连通区域", DefaultTitle = "连通区域",
          Description = "连通区域分析，染色标记每个独立区域", NodeTypeId = "Vision.ConnectedComponents")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input, ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output, ExpectedType = "Object")]
    [NodeConnector(Title = "区域数", Direction = ConnectorDirection.Output, ExpectedType = "Int32")]
    public class ConnectedComponentsNodeViewModel : VisionNodeBase
    {
        public ConnectedComponentsNodeViewModel() : base("连通区域")
        {
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "区域数",
                ExpectedType = System.TypeCode.Int32
            });
        }
        public int LabelCount { get; private set; }
        protected override ImageData ProcessImage(ImageData input)
        {
            var result = VisionAlgorithmService.ConnectedComponents(input, out int count);
            LabelCount = count;
            if (Output.Count > 1) Output[1].Value = VariantValue.FromInt32(count);
            ImageInfo = $"{count} 个连通区域";
            return result;
        }
    }
}
