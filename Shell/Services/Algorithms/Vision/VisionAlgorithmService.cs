using System.IO;
using OpenCvSharp;

namespace Shell.Services.Algorithms.Vision
{
    /// <summary>
    /// 视觉算法服务 —— 纯静态工具类，零 UI 依赖。
    /// 所有方法输入输出统一使用 PNG 编码的 byte[]，通过 VariantValue 连接器传递。
    /// 中文路径通过 .NET File API 桥接，绕过 OpenCV imread 的中文不兼容问题。
    /// </summary>
    public static class VisionAlgorithmService
    {
        // ──────────── 编解码辅助 ────────────

        /// <summary>Mat → PNG byte[]</summary>
        public static byte[] MatToPngBytes(Mat mat)
        {
            Cv2.ImEncode(".png", mat, out var buf);
            return buf;
        }

        /// <summary>PNG byte[] → Mat</summary>
        public static Mat PngBytesToMat(byte[] pngData)
        {
            return Cv2.ImDecode(pngData, ImreadModes.Unchanged);
        }

        /// <summary>从路径加载图像（支持中文路径）</summary>
        public static Mat LoadImageFromPath(string filePath, ImreadModes mode = ImreadModes.Color)
        {
            var bytes = File.ReadAllBytes(filePath);
            return Cv2.ImDecode(bytes, mode);
        }

        /// <summary>从路径加载为 PNG byte[]（支持中文路径）</summary>
        public static byte[] LoadImageAsPngBytes(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            using var mat = Cv2.ImDecode(bytes, ImreadModes.Unchanged);
            Cv2.ImEncode(".png", mat, out var buf);
            return buf;
        }

        /// <summary>保存图像到路径（支持中文路径）</summary>
        public static void SaveImageToPath(Mat mat, string filePath)
        {
            Cv2.ImEncode(".png", mat, out var bytes);
            File.WriteAllBytes(filePath, bytes);
        }

        // ──────────── 图像信息 ────────────

        public static (int Width, int Height, int Channels) GetImageInfo(byte[] pngData)
        {
            using var mat = PngBytesToMat(pngData);
            return mat.Empty()
                ? (0, 0, 0)
                : (mat.Width, mat.Height, mat.Channels());
        }

        // ──────────── 图像处理算法 ────────────

        /// <summary>高斯模糊</summary>
        public static byte[] GaussianBlur(byte[] pngInput, int kernelSize, double sigmaX)
        {
            using var src = PngBytesToMat(pngInput);
            if (src.Empty()) return pngInput;

            // 确保核大小为奇数
            var ksize = kernelSize % 2 == 0 ? kernelSize + 1 : kernelSize;
            ksize = Math.Max(3, ksize);

            using var dst = new Mat();
            Cv2.GaussianBlur(src, dst, new Size(ksize, ksize), sigmaX);
            return MatToPngBytes(dst);
        }

        /// <summary>色彩空间转换</summary>
        public static byte[] CvtColor(byte[] pngInput, ColorConversionCodes code)
        {
            using var src = PngBytesToMat(pngInput);
            if (src.Empty()) return pngInput;

            using var dst = new Mat();
            Cv2.CvtColor(src, dst, code);
            return MatToPngBytes(dst);
        }

        /// <summary>二值化</summary>
        public static byte[] Threshold(byte[] pngInput, double thresh, double maxval, ThresholdTypes type)
        {
            using var src = PngBytesToMat(pngInput);
            if (src.Empty()) return pngInput;

            // 如果是彩色图，先转灰度
            using var gray = src.Channels() == 1 ? src.Clone() : new Mat();
            if (src.Channels() > 1)
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

            using var dst = new Mat();
            Cv2.Threshold(gray, dst, thresh, maxval, type);
            return MatToPngBytes(dst);
        }

        /// <summary>缩放</summary>
        public static byte[] Resize(byte[] pngInput, double fx, double fy, InterpolationFlags interpolation = InterpolationFlags.Linear)
        {
            using var src = PngBytesToMat(pngInput);
            if (src.Empty()) return pngInput;

            using var dst = new Mat();
            Cv2.Resize(src, dst, new Size(0, 0), fx, fy, interpolation);
            return MatToPngBytes(dst);
        }

        /// <summary>缩放（指定目标尺寸）</summary>
        public static byte[] ResizeToSize(byte[] pngInput, int width, int height, InterpolationFlags interpolation = InterpolationFlags.Linear)
        {
            using var src = PngBytesToMat(pngInput);
            if (src.Empty()) return pngInput;

            using var dst = new Mat();
            Cv2.Resize(src, dst, new Size(width, height), 0, 0, interpolation);
            return MatToPngBytes(dst);
        }

        /// <summary>Canny 边缘检测</summary>
        public static byte[] CannyEdge(byte[] pngInput, double threshold1, double threshold2)
        {
            using var src = PngBytesToMat(pngInput);
            if (src.Empty()) return pngInput;

            using var gray = src.Channels() == 1 ? src.Clone() : new Mat();
            if (src.Channels() > 1)
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

            using var edges = new Mat();
            Cv2.Canny(gray, edges, threshold1, threshold2);
            return MatToPngBytes(edges);
        }

        /// <summary>形态学操作（膨胀/腐蚀/开运算/闭运算）</summary>
        public static byte[] MorphologyOp(byte[] pngInput, MorphTypes op, int kernelSize, int iterations = 1)
        {
            using var src = PngBytesToMat(pngInput);
            if (src.Empty()) return pngInput;

            var ksize = Math.Max(3, kernelSize % 2 == 0 ? kernelSize + 1 : kernelSize);
            using var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(ksize, ksize));
            using var dst = new Mat();
            Cv2.MorphologyEx(src, dst, op, kernel, iterations: iterations);
            return MatToPngBytes(dst);
        }

        // ──────────── 新增算法 ────────────

        /// <summary>亮度对比度调整：dst = src * alpha + beta</summary>
        public static byte[] BrightnessContrast(byte[] pngInput, double alpha, double beta)
        {
            using var src = PngBytesToMat(pngInput);
            if (src.Empty()) return pngInput;
            using var dst = new Mat();
            src.ConvertTo(dst, -1, alpha, beta);
            return MatToPngBytes(dst);
        }

        /// <summary>直方图均衡化（仅灰度图）</summary>
        public static byte[] EqualizeHist(byte[] pngInput)
        {
            using var src = PngBytesToMat(pngInput);
            if (src.Empty()) return pngInput;
            using var gray = src.Channels() == 1 ? src.Clone() : new Mat();
            if (src.Channels() > 1)
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            using var dst = new Mat();
            Cv2.EqualizeHist(gray, dst);
            return MatToPngBytes(dst);
        }

        /// <summary>图像翻转</summary>
        public static byte[] Flip(byte[] pngInput, FlipMode mode)
        {
            using var src = PngBytesToMat(pngInput);
            if (src.Empty()) return pngInput;
            using var dst = new Mat();
            Cv2.Flip(src, dst, mode);
            return MatToPngBytes(dst);
        }

        /// <summary>自适应二值化</summary>
        public static byte[] AdaptiveThreshold(byte[] pngInput, double maxValue,
            AdaptiveThresholdTypes adaptiveMethod, ThresholdTypes thresholdType,
            int blockSize, double c)
        {
            using var src = PngBytesToMat(pngInput);
            if (src.Empty()) return pngInput;
            using var gray = src.Channels() == 1 ? src.Clone() : new Mat();
            if (src.Channels() > 1)
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            // blockSize 必须是奇数
            var bs = blockSize % 2 == 0 ? blockSize + 1 : blockSize;
            bs = Math.Max(3, bs);
            using var dst = new Mat();
            Cv2.AdaptiveThreshold(gray, dst, maxValue, adaptiveMethod, thresholdType, bs, c);
            return MatToPngBytes(dst);
        }

        /// <summary>中值滤波（去椒盐噪声）</summary>
        public static byte[] MedianBlur(byte[] pngInput, int ksize)
        {
            using var src = PngBytesToMat(pngInput);
            if (src.Empty()) return pngInput;
            var ks = ksize % 2 == 0 ? ksize + 1 : ksize;
            ks = Math.Max(3, ks);
            using var dst = new Mat();
            Cv2.MedianBlur(src, dst, ks);
            return MatToPngBytes(dst);
        }

        /// <summary>霍夫直线检测（在图像上绘制检测到的直线）</summary>
        public static byte[] HoughLinesP(byte[] pngInput, double rho, double theta,
            int threshold, double minLineLength, double maxLineGap,
            out int lineCount)
        {
            lineCount = 0;
            using var src = PngBytesToMat(pngInput);
            if (src.Empty()) return pngInput;
            using var gray = src.Channels() == 1 ? src.Clone() : new Mat();
            if (src.Channels() > 1)
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            using var edges = new Mat();
            Cv2.Canny(gray, edges, 50, 150);
            var lines = Cv2.HoughLinesP(edges, rho, theta, threshold, minLineLength, maxLineGap);
            // 在原图上绘制直线
            using var colorSrc = src.Channels() == 3 ? src.Clone() : new Mat();
            if (src.Channels() != 3)
                Cv2.CvtColor(src, colorSrc, ColorConversionCodes.GRAY2BGR);
            if (lines != null && lines.Length > 0)
            {
                lineCount = lines.Length;
                foreach (var line in lines)
                    Cv2.Line(colorSrc, line.P1, line.P2, Scalar.Red, 2);
            }
            return MatToPngBytes(colorSrc);
        }
    }
}
