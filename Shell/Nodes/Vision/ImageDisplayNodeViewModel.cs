using System;
using System.Linq;
using Shell.Models.Attributes;

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
        ExpectedType = "Object", Description = "ImageData 原始像素图像数据")]
    [NodeConnector(Title = "完成", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "完成")]
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
                Title = "完成",
                ExpectedType = System.TypeCode.Boolean
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

        /// <summary>高性能图像预览组件（WriteableBitmap 复用 + 节流）。</summary>
        public ImagePreview Preview { get; } = new();

        private bool _hasImage;
        public bool HasImage
        {
            get => _hasImage;
            set => SetProperty(ref _hasImage, value);
        }

        public override void Execute()
        {
            var inputVal = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;

            if (inputVal.TryGetImageData(out var img) && img != null)
            {
                try
                {
                    ImageInfo = $"{img.Width}×{img.Height}, {img.Channels} 通道, {img.DataSize / 1024} KB";
                    Preview.Update(img);
                    HasImage = true;

                    if (Output.Count > 1)
                        Output[1].Value = VariantValue.FromString(ImageInfo);
                    if (Output.Count > 0)
                        Output[0].Value = VariantValue.FromBoolean(true);
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

