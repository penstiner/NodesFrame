using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Shell.Models.Nodes.Vision
{
    /// <summary>
    /// 视觉节点辅助工具。
    /// </summary>
    public static class VisionHelper
    {
        /// <summary>
        /// 从 PNG byte[] 创建缩略图 BitmapImage（用于预览面板）。
        /// </summary>
        public static BitmapImage? MakePreview(byte[]? pngData, int decodeWidth = 260)
        {
            if (pngData == null || pngData.Length == 0) return null;

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(pngData);
                bmp.DecodePixelWidth = decodeWidth;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 从 ImageData 创建 BitmapSource（零 PNG 编解码，直接从原始像素构建）。
        /// </summary>
        public static BitmapSource? MakePreviewFromImageData(ImageData? img, int decodeWidth = 260)
        {
            if (img == null || img.RawPixels == null || img.RawPixels.Length == 0) return null;

            try
            {
                var format = img.Channels switch
                {
                    1 => PixelFormats.Gray8,
                    3 => PixelFormats.Bgr24,
                    4 => PixelFormats.Bgra32,
                    _ => PixelFormats.Bgr24
                };

                var bmp = BitmapSource.Create(
                    img.Width, img.Height, 96, 96,
                    format, null, img.RawPixels, img.Stride);

                // 缩放预览
                if (decodeWidth > 0 && img.Width > decodeWidth)
                {
                    var scale = (double)decodeWidth / img.Width;
                    var scaled = new System.Windows.Media.Imaging.TransformedBitmap(
                        bmp, new ScaleTransform(scale, scale));
                    scaled.Freeze();
                    return scaled;
                }

                bmp.Freeze();
                return bmp;
            }
            catch
            {
                return null;
            }
        }
    }
}
