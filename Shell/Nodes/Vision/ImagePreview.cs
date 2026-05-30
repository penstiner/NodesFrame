using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ReactiveUI;

namespace Shell.Models.Nodes.Vision
{
    /// <summary>
    /// 高性能图像预览组件 —— 基于 WriteableBitmap 复用 + UI 写入节流。
    /// <para>
    /// 核心优势：
    /// 1. WriteableBitmap 按尺寸缓存复用，避免每帧 new 大对象；
    /// 2. Interlocked 节流：高频调用时只保留最新帧，防止 Dispatcher 队列积压；
    /// 3. 线程安全：后台线程调用 Update()，自动调度到 UI 线程写入像素。
    /// </para>
    /// </summary>
    public sealed class ImagePreview : ReactiveObject
    {
        private WriteableBitmap? _wbGray;
        private WriteableBitmap? _wbBgr;

        private ImageSource? _imageSource;
        /// <summary>当前显示的图像源（WPF Image.Source 直接绑定此属性）。</summary>
        public ImageSource? ImageSource
        {
            get => _imageSource;
            private set => this.RaiseAndSetIfChanged(ref _imageSource, value);
        }

        // ── UI 写入节流：避免高频 BeginInvoke 排队导致 UI 卡顿 ──
        private int _uiWriteScheduled;
        private ImageData? _pendingFrame;
        private readonly object _pendingLock = new();

        /// <summary>
        /// 更新预览图像。可从任意线程调用（包括后台执行线程）。
        /// 内部自动调度到 UI 线程写入像素，并通过节流只保留最新帧。
        /// </summary>
        public void Update(ImageData? img)
        {
            if (img == null || img.RawPixels == null || img.RawPixels.Length == 0)
                return;

            // 保存最新帧
            lock (_pendingLock)
            {
                _pendingFrame = img;
            }

            // 如果已有调度在排队，跳过（最新帧会在下次回调时被读取）
            if (Interlocked.CompareExchange(ref _uiWriteScheduled, 1, 0) != 0)
                return;

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null) return;

            dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    ImageData? frame;
                    lock (_pendingLock)
                    {
                        frame = _pendingFrame;
                    }
                    if (frame == null || frame.RawPixels == null) return;

                    WriteToBitmap(frame);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ImagePreview] UI写入失败: {ex.Message}");
                }
                finally
                {
                    Interlocked.Exchange(ref _uiWriteScheduled, 0);
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        /// <summary>
        /// 同步更新预览（仅在 UI 线程调用时使用，如文件浏览器选择图像）。
        /// </summary>
        public void UpdateSync(ImageData? img)
        {
            if (img == null || img.RawPixels == null || img.RawPixels.Length == 0)
                return;

            WriteToBitmap(img);
        }

        private void WriteToBitmap(ImageData img)
        {
            if (img.Channels == 1)
            {
                EnsureBitmap(ref _wbGray, img.Width, img.Height, PixelFormats.Gray8);
                _wbGray!.WritePixels(
                    new Int32Rect(0, 0, img.Width, img.Height),
                    img.RawPixels, img.Stride, 0);
                ImageSource = _wbGray;
            }
            else // BGR24 / BGRA32
            {
                var format = img.Channels >= 4 ? PixelFormats.Bgra32 : PixelFormats.Bgr24;
                EnsureBitmap(ref _wbBgr, img.Width, img.Height, format);
                _wbBgr!.WritePixels(
                    new Int32Rect(0, 0, img.Width, img.Height),
                    img.RawPixels, img.Stride, 0);
                ImageSource = _wbBgr;
            }
        }

        private static void EnsureBitmap(ref WriteableBitmap? wb, int w, int h, PixelFormat format)
        {
            if (wb == null || wb.PixelWidth != w || wb.PixelHeight != h)
                wb = new WriteableBitmap(w, h, 96d, 96d, format, null);
        }
    }
}
