using System.IO;
using OpenCvSharp;
using Shell.Models;

namespace Shell.Services.Algorithms.Vision
{
    /// <summary>
    /// 视觉算法服务 —— 纯静态工具类，零 UI 依赖。
    /// 节点间传递统一使用 ImageData（原始像素），消除 PNG 编解码开销。
    /// PNG 仅在文件 I/O 和 UI 预览时使用。
    /// 中文路径通过 .NET File API 桥接，绕过 OpenCV imread 的中文不兼容问题。
    /// </summary>
    public static class VisionAlgorithmService
    {
        // ──────────── 核心互转方法 ────────────

        /// <summary>Mat → ImageData（提取原始像素，零拷贝 + 安全复制）</summary>
        public static ImageData MatToImageData(Mat mat)
        {
            if (mat == null || mat.Empty()) return null;
            var pixels = new byte[mat.Rows * mat.Cols * mat.Channels()];
            System.Runtime.InteropServices.Marshal.Copy(mat.Data, pixels, 0, pixels.Length);
            return new ImageData(mat.Width, mat.Height, mat.Channels(), pixels);
        }

        /// <summary>ImageData → Mat（从原始像素构建，零拷贝）</summary>
        public static Mat ImageDataToMat(ImageData img)
        {
            if (img == null) return null;
            var type = img.Channels switch
            {
                1 => MatType.CV_8UC1,
                3 => MatType.CV_8UC3,
                4 => MatType.CV_8UC4,
                _ => MatType.CV_8UC3
            };
            return Mat.FromPixelData(img.Height, img.Width, type, img.RawPixels);
        }

        /// <summary>Mat → PNG byte[]（仅用于 UI 预览和文件保存）</summary>
        public static byte[] MatToPngBytes(Mat mat)
        {
            Cv2.ImEncode(".png", mat, out var buf);
            return buf;
        }

        /// <summary>PNG byte[] → Mat（仅用于文件加载和向后兼容）</summary>
        public static Mat PngBytesToMat(byte[] pngData)
        {
            return Cv2.ImDecode(pngData, ImreadModes.Unchanged);
        }

        /// <summary>PNG byte[] → ImageData（向后兼容桥接）</summary>
        public static ImageData PngBytesToImageData(byte[] pngData)
        {
            using var mat = PngBytesToMat(pngData);
            return MatToImageData(mat);
        }

        /// <summary>ImageData → PNG byte[]（仅用于 UI 预览）</summary>
        public static byte[] ImageDataToPngBytes(ImageData img)
        {
            using var mat = ImageDataToMat(img);
            return MatToPngBytes(mat);
        }

        // ──────────── 文件 I/O ────────────

        /// <summary>从路径加载为 ImageData（支持中文路径）</summary>
        public static ImageData LoadImageAsImageData(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            using var mat = Cv2.ImDecode(bytes, ImreadModes.Unchanged);
            return MatToImageData(mat);
        }

        /// <summary>从路径加载图像（支持中文路径）</summary>
        public static Mat LoadImageFromPath(string filePath, ImreadModes mode = ImreadModes.Color)
        {
            var bytes = File.ReadAllBytes(filePath);
            return Cv2.ImDecode(bytes, mode);
        }

        /// <summary>从路径加载为 PNG byte[]（向后兼容）</summary>
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

        /// <summary>从 ImageData 获取图像信息（零开销，直接读取元数据）</summary>
        public static (int Width, int Height, int Channels) GetImageInfo(ImageData img)
        {
            return img == null ? (0, 0, 0) : (img.Width, img.Height, img.Channels);
        }

        /// <summary>从 PNG byte[] 获取图像信息（向后兼容，需解码）</summary>
        public static (int Width, int Height, int Channels) GetImageInfo(byte[] pngData)
        {
            using var mat = PngBytesToMat(pngData);
            return mat.Empty()
                ? (0, 0, 0)
                : (mat.Width, mat.Height, mat.Channels());
        }

        // ──────────── 图像处理算法（统一 ImageData 输入输出）────────────

        /// <summary>高斯模糊</summary>
        public static ImageData GaussianBlur(ImageData input, int kernelSize, double sigmaX)
        {
            using var src = ImageDataToMat(input);
            if (src.Empty()) return input;
            var ksize = kernelSize % 2 == 0 ? kernelSize + 1 : kernelSize;
            ksize = Math.Max(3, ksize);
            using var dst = new Mat();
            Cv2.GaussianBlur(src, dst, new Size(ksize, ksize), sigmaX);
            return MatToImageData(dst);
        }

        /// <summary>色彩空间转换</summary>
        public static ImageData CvtColor(ImageData input, ColorConversionCodes code)
        {
            using var src = ImageDataToMat(input);
            if (src.Empty()) return input;
            using var dst = new Mat();
            Cv2.CvtColor(src, dst, code);
            return MatToImageData(dst);
        }

        /// <summary>二值化</summary>
        public static ImageData Threshold(ImageData input, double thresh, double maxval, ThresholdTypes type)
        {
            using var src = ImageDataToMat(input);
            if (src.Empty()) return input;
            using var gray = src.Channels() == 1 ? src.Clone() : new Mat();
            if (src.Channels() > 1)
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            using var dst = new Mat();
            Cv2.Threshold(gray, dst, thresh, maxval, type);
            return MatToImageData(dst);
        }

        /// <summary>缩放</summary>
        public static ImageData Resize(ImageData input, double fx, double fy, InterpolationFlags interpolation = InterpolationFlags.Linear)
        {
            using var src = ImageDataToMat(input);
            if (src.Empty()) return input;
            using var dst = new Mat();
            Cv2.Resize(src, dst, new Size(0, 0), fx, fy, interpolation);
            return MatToImageData(dst);
        }

        /// <summary>缩放（指定目标尺寸）</summary>
        public static ImageData ResizeToSize(ImageData input, int width, int height, InterpolationFlags interpolation = InterpolationFlags.Linear)
        {
            using var src = ImageDataToMat(input);
            if (src.Empty()) return input;
            using var dst = new Mat();
            Cv2.Resize(src, dst, new Size(width, height), 0, 0, interpolation);
            return MatToImageData(dst);
        }

        /// <summary>Canny 边缘检测</summary>
        public static ImageData CannyEdge(ImageData input, double threshold1, double threshold2)
        {
            using var src = ImageDataToMat(input);
            if (src.Empty()) return input;
            using var gray = src.Channels() == 1 ? src.Clone() : new Mat();
            if (src.Channels() > 1)
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            using var edges = new Mat();
            Cv2.Canny(gray, edges, threshold1, threshold2);
            return MatToImageData(edges);
        }

        /// <summary>形态学操作（膨胀/腐蚀/开运算/闭运算）</summary>
        public static ImageData MorphologyOp(ImageData input, MorphTypes op, int kernelSize, int iterations = 1)
        {
            using var src = ImageDataToMat(input);
            if (src.Empty()) return input;
            var ksize = Math.Max(3, kernelSize % 2 == 0 ? kernelSize + 1 : kernelSize);
            using var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(ksize, ksize));
            using var dst = new Mat();
            Cv2.MorphologyEx(src, dst, op, kernel, iterations: iterations);
            return MatToImageData(dst);
        }

        // ──────────── 新增算法 ────────────

        /// <summary>亮度对比度调整：dst = src * alpha + beta</summary>
        public static ImageData BrightnessContrast(ImageData input, double alpha, double beta)
        {
            using var src = ImageDataToMat(input);
            if (src.Empty()) return input;
            using var dst = new Mat();
            src.ConvertTo(dst, -1, alpha, beta);
            return MatToImageData(dst);
        }

        /// <summary>直方图均衡化（仅灰度图）</summary>
        public static ImageData EqualizeHist(ImageData input)
        {
            using var src = ImageDataToMat(input);
            if (src.Empty()) return input;
            using var gray = src.Channels() == 1 ? src.Clone() : new Mat();
            if (src.Channels() > 1)
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            using var dst = new Mat();
            Cv2.EqualizeHist(gray, dst);
            return MatToImageData(dst);
        }

        /// <summary>图像翻转</summary>
        public static ImageData Flip(ImageData input, FlipMode mode)
        {
            using var src = ImageDataToMat(input);
            if (src.Empty()) return input;
            using var dst = new Mat();
            Cv2.Flip(src, dst, mode);
            return MatToImageData(dst);
        }

        /// <summary>自适应二值化</summary>
        public static ImageData AdaptiveThreshold(ImageData input, double maxValue,
            AdaptiveThresholdTypes adaptiveMethod, ThresholdTypes thresholdType,
            int blockSize, double c)
        {
            using var src = ImageDataToMat(input);
            if (src.Empty()) return input;
            using var gray = src.Channels() == 1 ? src.Clone() : new Mat();
            if (src.Channels() > 1)
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            var bs = blockSize % 2 == 0 ? blockSize + 1 : blockSize;
            bs = Math.Max(3, bs);
            using var dst = new Mat();
            Cv2.AdaptiveThreshold(gray, dst, maxValue, adaptiveMethod, thresholdType, bs, c);
            return MatToImageData(dst);
        }

        /// <summary>中值滤波（去椒盐噪声）</summary>
        public static ImageData MedianBlur(ImageData input, int ksize)
        {
            using var src = ImageDataToMat(input);
            if (src.Empty()) return input;
            var ks = ksize % 2 == 0 ? ksize + 1 : ksize;
            ks = Math.Max(3, ks);
            using var dst = new Mat();
            Cv2.MedianBlur(src, dst, ks);
            return MatToImageData(dst);
        }

        /// <summary>霍夫直线检测（在图像上绘制检测到的直线）</summary>
        public static ImageData HoughLinesP(ImageData input, double rho, double theta,
            int threshold, double minLineLength, double maxLineGap,
            out int lineCount)
        {
            lineCount = 0;
            using var src = ImageDataToMat(input);
            if (src.Empty()) return input;
            using var gray = src.Channels() == 1 ? src.Clone() : new Mat();
            if (src.Channels() > 1)
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            using var edges = new Mat();
            Cv2.Canny(gray, edges, 50, 150);
            var lines = Cv2.HoughLinesP(edges, rho, theta, threshold, minLineLength, maxLineGap);
            using var colorSrc = src.Channels() == 3 ? src.Clone() : new Mat();
            if (src.Channels() != 3)
                Cv2.CvtColor(src, colorSrc, ColorConversionCodes.GRAY2BGR);
            if (lines != null && lines.Length > 0)
            {
                lineCount = lines.Length;
                foreach (var line in lines)
                    Cv2.Line(colorSrc, line.P1, line.P2, Scalar.Red, 2);
            }
            return MatToImageData(colorSrc);
        }
    }
}
