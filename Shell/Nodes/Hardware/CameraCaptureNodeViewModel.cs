using System.Linq;
using Shell.Models.Attributes;
using Shell.Models.Nodes.Vision;
using Shell.Services;

namespace Shell.Models.Nodes.Hardware
{
    [Node(
        Category = "硬件采集",
        DisplayName = "触发拍照",
        DefaultTitle = "触发拍照",
        Description = "接收上游触发信号，触发相机拍照，输出 ImageData 图像数据和状态",
        NodeTypeId = "Hardware.CameraCapture")]
    [NodeConnector(Title = "触发", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "接收上游触发信号")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output,
        ExpectedType = "Object", Description = "ImageData 原始像素图像数据")]
    [NodeConnector(Title = "状态", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "拍照是否成功")]
    public class CameraCaptureNodeViewModel : NodeViewModel
    {
        public CameraCaptureNodeViewModel()
        {
            Title = "触发拍照";

            // 触发输入（接收上游完成信号）
            AddInputConnector(new ConnectorViewModel
            {
                Title = "触发",
                ExpectedType = System.TypeCode.Boolean
            });

            // 输出图像
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "输出图像",
                ExpectedType = System.TypeCode.Object
            });

            // 状态输出（拍照成功/失败）
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "状态",
                ExpectedType = System.TypeCode.Boolean
            });
        }

        /// <summary>高性能图像预览组件（WriteableBitmap 复用 + 节流）。</summary>
        public ImagePreview Preview { get; } = new();

        private string _imageInfo = string.Empty;
        public string ImageInfo
        {
            get => _imageInfo;
            set => SetProperty(ref _imageInfo, value);
        }

        public override void Execute()
        {
            var imgData = CameraManager.CaptureImageData();
            if (imgData != null)
            {
                Output[0].Value = VariantValue.FromImageData(imgData);
                Output[1].Value = VariantValue.FromBoolean(true);
                ImageInfo = imgData.InfoText;
                Preview.Update(imgData);
                ExecutionLogger.Success("触发拍照", $"拍照成功，{ImageInfo}");
            }
            else
            {
                Output[0].Value = VariantValue.Null;
                Output[1].Value = VariantValue.FromBoolean(false);
                ImageInfo = "拍照失败";
                ExecutionLogger.Error("触发拍照", "拍照失败，请检查相机连接");
            }
        }
    }
}
