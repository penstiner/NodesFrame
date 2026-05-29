using System.Linq;
using System.Windows.Media.Imaging;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    /// <summary>
    /// 视觉算法节点基类 —— 封装通用的输入/输出处理、预览生成、图像信息提取。
    /// 子类只需重写 ProcessImage(byte[] input) → byte[] output 即可。
    /// </summary>
    public abstract class VisionNodeBase : NodeViewModel
    {
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

        private BitmapImage? _previewImage;
        public BitmapImage? PreviewImage
        {
            get => _previewImage;
            set => SetProperty(ref _previewImage, value);
        }

        private string _imageInfo = string.Empty;
        public string ImageInfo
        {
            get => _imageInfo;
            set => SetProperty(ref _imageInfo, value);
        }

        /// <summary>子类实现具体的图像处理逻辑。</summary>
        protected abstract byte[]? ProcessImage(byte[] input);

        public override void Execute()
        {
            var inputVal = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
            if (!inputVal.TryGetBytes(out var pngData) || pngData.Length == 0)
            {
                ImageInfo = "等待输入图像...";
                return;
            }

            var result = ProcessImage(pngData);
            if (result != null && result.Length > 0)
            {
                if (Output.Count > 0)
                    Output[0].Value = VariantValue.FromBytes(result);

                var info = VisionAlgorithmService.GetImageInfo(result);
                ImageInfo = $"{info.Width}×{info.Height}, {info.Channels}ch";
                PreviewImage = VisionHelper.MakePreview(result);
            }
        }
    }
}
