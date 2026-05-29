using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    /// <summary>
    /// 图像显示节点：显示输入图像的缩略图预览和信息。
    /// </summary>
    [Node(
        Category = "输入输出",
        DisplayName = "图像显示",
        DefaultTitle = "图像显示",
        Description = "显示输入图像并提供尺寸信息",
        NodeTypeId = "Vision.ImageDisplay")]
    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input,
        ExpectedType = "Object", Description = "PNG 编码的 byte[] 图像数据")]
    [NodeConnector(Title = "尺寸信息", Direction = ConnectorDirection.Output,
        ExpectedType = "String", Description = "图像尺寸描述")]
    public class ImageDisplayNodeViewModel : NodeViewModel
    {
        public ImageDisplayNodeViewModel()
        {
            AddInputConnector(new ConnectorViewModel
            {
                Title = "输入图像",
                ExpectedType = System.TypeCode.Object
            });
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "尺寸信息",
                ExpectedType = System.TypeCode.String
            });
        }

        private string _imageInfo = "等待输入...";
        public string ImageInfo
        {
            get => _imageInfo;
            set => SetProperty(ref _imageInfo, value);
        }

        private BitmapImage? _previewImage;
        /// <summary>预览图像（WPF Image.Source 可绑定）</summary>
        public BitmapImage? PreviewImage
        {
            get => _previewImage;
            set => SetProperty(ref _previewImage, value);
        }

        private bool _hasImage;
        public bool HasImage
        {
            get => _hasImage;
            set => SetProperty(ref _hasImage, value);
        }

        public override void Execute()
        {
            var inputVal = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;

            if (inputVal.TryGetBytes(out var imageData) && imageData.Length > 0)
            {
                try
                {
                    var info = VisionAlgorithmService.GetImageInfo(imageData);
                    ImageInfo = $"{info.Width}×{info.Height}, {info.Channels} 通道, {imageData.Length / 1024} KB";

                    // 创建缩略图预览
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.StreamSource = new MemoryStream(imageData);
                    bmp.DecodePixelWidth = 160;
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze(); // 允许跨线程访问
                    PreviewImage = bmp;
                    HasImage = true;

                    if (Output.Count > 0)
                        Output[0].Value = VariantValue.FromString(ImageInfo);
                }
                catch (Exception ex)
                {
                    ImageInfo = $"解码失败: {ex.Message}";
                    HasImage = false;
                }
            }
            else
            {
                ImageInfo = "无图像数据";
                HasImage = false;
            }
        }
    }
}

