using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Nodify;
using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    /// <summary>
    /// 图像源节点：从文件加载图像并输出 ImageData。
    /// 中文路径通过 .NET File API 桥接，安全绕过 OpenCV 的中文限制。
    /// </summary>
    [Node(
        Category = "输入输出",
        DisplayName = "图像源",
        DefaultTitle = "图像源",
        Description = "从本地文件加载图像，输出 ImageData 格式图像数据",
        NodeTypeId = "Vision.ImageSource")]
    [NodeConnector(Title = "触发", Direction = ConnectorDirection.Input,
        ExpectedType = "Object", Description = "触发")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output,
        ExpectedType = "Object", Description = "ImageData 原始像素图像数据")]
    public class ImageSourceNodeViewModel : NodeViewModel
    {
        public ImageSourceNodeViewModel()
        {
            AddInputConnector(new ConnectorViewModel
            {
                Title = "触发",
                ExpectedType = System.TypeCode.Object
            });
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "输出图像",
                ExpectedType = System.TypeCode.Object
            });

            BrowseImageCommand = new DelegateCommand(BrowseImageFile);
        }

        private string _filePath = string.Empty;
        [NodeProperty(Key = "filePath", DisplayName = "文件路径")]
        public string FilePath
        {
            get => _filePath;
            set
            {
                if (SetProperty(ref _filePath, value))
                {
                    OnPropertyChanged(nameof(FileName));
                    OnPropertyChanged(nameof(HasFile));
                    // 路径变更时自动尝试加载
                    TryLoadImage();
                }
            }
        }

        /// <summary>仅文件名（用于显示）</summary>
        public string FileName => string.IsNullOrEmpty(FilePath)
            ? "未选择"
            : Path.GetFileName(FilePath);

        /// <summary>是否已选择文件</summary>
        public bool HasFile => !string.IsNullOrEmpty(FilePath) && File.Exists(FilePath);

        private string _imageInfo = "未加载";
        public string ImageInfo
        {
            get => _imageInfo;
            set => SetProperty(ref _imageInfo, value);
        }

        /// <summary>高性能图像预览组件（WriteableBitmap 复用）。</summary>
        public ImagePreview Preview { get; } = new();

        /// <summary>尝试加载图像</summary>
        public void TryLoadImage()
        {
            if (!HasFile) return;
            try
            {
                var imageData = VisionAlgorithmService.LoadImageAsImageData(FilePath);
                if (imageData != null)
                {
                    if (Output.Count > 0)
                        Output[0].Value = VariantValue.FromImageData(imageData);
                    ImageInfo = $"{imageData.Width}×{imageData.Height}, {imageData.Channels} 通道";
                    Preview.UpdateSync(imageData);
                }
            }
            catch (Exception ex)
            {
                ImageInfo = $"加载失败: {ex.Message}";
            }
        }

        /// <summary>浏览图像文件命令（用于 UI 绑定，替代 Click 事件）。</summary>
        public ICommand BrowseImageCommand { get; }

        private void BrowseImageFile()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "选择图像文件",
                Filter = "图像文件|*.png;*.jpg;*.jpeg;*.bmp;*.tiff;*.webp|所有文件|*.*",
                Multiselect = false
            };
            if (dlg.ShowDialog() == true)
            {
                FilePath = dlg.FileName;
            }
        }

        public override void Execute()
        {
            TryLoadImage();
        }
    }
}
