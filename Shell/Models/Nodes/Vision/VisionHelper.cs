using System.IO;
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
    }
}
