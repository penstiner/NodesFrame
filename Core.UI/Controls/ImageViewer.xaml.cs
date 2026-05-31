using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Core.UI.Controls
{
    /// <summary>
    /// 可缩放、可拖动的图像查看器。
    /// 将缩放和拖拽逻辑完全封装在控件内部，符合 MVVM 模式。
    /// </summary>
    public partial class ImageViewer : UserControl
    {
        // ── 依赖属性 ──

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(ImageSource), typeof(ImageViewer),
                new PropertyMetadata(null, OnSourceChanged));

        public ImageSource? Source
        {
            get => (ImageSource?)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register(nameof(Scale), typeof(double), typeof(ImageViewer),
                new PropertyMetadata(1.0, OnScaleChanged));

        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        public static readonly DependencyProperty OffsetXProperty =
            DependencyProperty.Register(nameof(OffsetX), typeof(double), typeof(ImageViewer),
                new PropertyMetadata(0.0));

        public double OffsetX
        {
            get => (double)GetValue(OffsetXProperty);
            set => SetValue(OffsetXProperty, value);
        }

        public static readonly DependencyProperty OffsetYProperty =
            DependencyProperty.Register(nameof(OffsetY), typeof(double), typeof(ImageViewer),
                new PropertyMetadata(0.0));

        public double OffsetY
        {
            get => (double)GetValue(OffsetYProperty);
            set => SetValue(OffsetYProperty, value);
        }

        // ── 缩放范围 ──
        public double MinScale { get; set; } = 0.1;
        public double MaxScale { get; set; } = 5.0;
        public double ZoomStep { get; set; } = 1.25;

        // ── 内部拖拽状态 ──
        private bool _isDragging;
        private Point _dragStart;
        private readonly ScaleTransform _scaleTransform;
        private readonly TranslateTransform _translateTransform;

        // ── 构造函数 ──

        public ImageViewer()
        {
            InitializeComponent();

            _scaleTransform = new ScaleTransform(1, 1);
            _translateTransform = new TranslateTransform(0, 0);
            var group = new TransformGroup();
            group.Children.Add(_scaleTransform);
            group.Children.Add(_translateTransform);
            ImageBox.RenderTransform = group;
        }

        // ── 公开方法（供外部按钮调用）──

        public void ZoomIn()
        {
            ApplyZoom(ZoomStep);
        }

        public void ZoomOut()
        {
            ApplyZoom(1.0 / ZoomStep);
        }

        public void ResetView()
        {
            Scale = 1.0;
            OffsetX = 0;
            OffsetY = 0;
        }

        // ── Source 变更时重置视图 ──

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageViewer viewer)
            {
                viewer.ResetView();
            }
        }

        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageViewer viewer)
            {
                var s = (double)e.NewValue;
                viewer._scaleTransform.ScaleX = s;
                viewer._scaleTransform.ScaleY = s;
            }
        }

        // ── 鼠标事件 ──

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var factor = e.Delta > 0 ? ZoomStep : 1.0 / ZoomStep;
            ApplyZoom(factor);
            e.Handled = true;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _dragStart = e.GetPosition(ViewportBorder);
            ViewportBorder.CaptureMouse();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ViewportBorder.ReleaseMouseCapture();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;
            var current = e.GetPosition(ViewportBorder);
            var delta = current - _dragStart;
            OffsetX += delta.X;
            OffsetY += delta.Y;
            _translateTransform.X = OffsetX;
            _translateTransform.Y = OffsetY;
            _dragStart = current;
        }

        private void ApplyZoom(double factor)
        {
            var newScale = Scale * factor;
            if (newScale < MinScale || newScale > MaxScale) return;
            Scale = newScale;
        }
    }
}
