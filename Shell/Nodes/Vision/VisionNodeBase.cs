using System.Linq;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    /// <summary>
    /// 视觉算法节点基类 —— 封装通用的输入/输出处理、预览生成、图像信息提取。
    /// 节点间传递 ImageData（原始像素），零 PNG 编解码开销。
    /// 预览使用 WriteableBitmap 复用 + UI 节流，子类无需关心显示细节。
    /// </summary>
    public abstract class VisionNodeBase : NodeViewModel
    {
        /// <summary>高性能图像预览组件（WriteableBitmap 复用 + 节流）。</summary>
        public ImagePreview Preview { get; } = new();

        protected VisionNodeBase(string title = "视觉节点")
        {
            Title = title;
            AddInputConnector(new ConnectorViewModel
            {
                Title = "输入图像",
                ExpectedType = System.TypeCode.Object
            });
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "输出图像",
                ExpectedType = System.TypeCode.Object
            });
        }

        private string _imageInfo = string.Empty;
        public string ImageInfo
        {
            get => _imageInfo;
            set => SetProperty(ref _imageInfo, value);
        }

        /// <summary>子类实现具体的图像处理逻辑（ImageData → ImageData，零 PNG 开销）。</summary>
        protected abstract ImageData? ProcessImage(ImageData input);

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

                ImageInfo = result.InfoText;
                Preview.Update(result);
            }
        }
    }
}
