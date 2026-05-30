namespace Shell.Models
{
    /// <summary>
    /// 轻量级图像数据容器，在节点间传递原始像素数据。
    /// 替代 PNG byte[] 方案，消除节点间传递时的 PNG 编解码开销。
    /// PNG 编解码仅保留在输入源节点和输出终点节点。
    /// </summary>
    public sealed class ImageData
    {
        /// <summary>图像宽度（像素）。</summary>
        public int Width { get; }

        /// <summary>图像高度（像素）。</summary>
        public int Height { get; }

        /// <summary>通道数（1=灰度, 3=BGR, 4=BGRA）。</summary>
        public int Channels { get; }

        /// <summary>原始像素数据（BGR/Gray/BGRA 排列，行优先，无填充）。</summary>
        public byte[] RawPixels { get; }

        public ImageData(int width, int height, int channels, byte[] rawPixels)
        {
            Width = width;
            Height = height;
            Channels = channels;
            RawPixels = rawPixels;
        }

        /// <summary>每像素字节数。</summary>
        public int BytesPerPixel => Channels;

        /// <summary>每行字节数（无填充）。</summary>
        public int Stride => Width * Channels;

        /// <summary>像素数据总大小。</summary>
        public int DataSize => RawPixels.Length;

        /// <summary>图像信息摘要文本。</summary>
        public string InfoText => $"{Width}×{Height}, {Channels}ch, {DataSize / 1024} KB";
    }
}
