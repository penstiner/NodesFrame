using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MvCamCtrl.NET;
using OpenCvSharp;
using Shell.Hardware.Camera;
using Shell.Services.Algorithms.Vision;
using Shell.Models;

namespace Shell.Services
{
    /// <summary>
    /// 相机管理器 —— 静态服务类，管理 HikCamera 实例的生命周期。
    /// 线程安全，通过 lock 保护内部状态。
    /// </summary>
    public static class CameraManager
    {
        private static readonly object _lock = new object();
        private static HikCamera _camera;

        /// <summary>
        /// 枚举当前连接的所有相机设备名称（用于 UI 下拉选择）。
        /// 返回格式如 "[0] Hikvision MV-xxx (192.168.1.100)"。
        /// </summary>
        public static List<string> EnumDeviceNames()
        {
            var names = new List<string>();
            try
            {
                var stDevList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
                int nRet = MyCamera.MV_CC_EnumDevices_NET(
                    MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE,
                    ref stDevList);

                if (nRet != MyCamera.MV_OK || stDevList.nDeviceNum == 0)
                    return names;

                for (int i = 0; i < stDevList.nDeviceNum; i++)
                {
                    var device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(
                        stDevList.pDeviceInfo[i],
                        typeof(MyCamera.MV_CC_DEVICE_INFO));

                    string displayName = GetDeviceDisplayName(device, i);
                    names.Add(displayName);
                }
            }
            catch
            {
                // 枚举失败返回空列表
            }
            return names;
        }

        /// <summary>获取设备友好显示名称。</summary>
        private static string GetDeviceDisplayName(MyCamera.MV_CC_DEVICE_INFO device, int index)
        {
            try
            {
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    var gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(
                        device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    string name = gigeInfo.chUserDefinedName ?? gigeInfo.chModelName ?? "GigE Camera";
                    uint ip = gigeInfo.nCurrentIp;
                    string ipStr = $"{(ip >> 24) & 0xFF}.{(ip >> 16) & 0xFF}.{(ip >> 8) & 0xFF}.{ip & 0xFF}";
                    return $"[{index}] {name} ({ipStr})";
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    var usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(
                        device.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    string name = usbInfo.chUserDefinedName ?? usbInfo.chModelName ?? "USB Camera";
                    return $"[{index}] {name} (USB)";
                }
            }
            catch { }
            return $"[{index}] 未知设备";
        }

        /// <summary>相机是否已连接。</summary>
        public static bool IsConnected
        {
            get
            {
                lock (_lock)
                {
                    return _camera != null && _camera.IsConnected;
                }
            }
        }

        /// <summary>
        /// 初始化相机：枚举设备、打开指定索引的设备、开始采集。
        /// </summary>
        /// <param name="deviceIndex">设备索引（从 0 开始）。</param>
        /// <returns>初始化结果元组：(是否成功, 消息)。</returns>
        public static (bool Success, string Message) Initialize(int deviceIndex)
        {
            lock (_lock)
            {
                try
                {
                    // 如果已有连接，先关闭
                    if (_camera != null && _camera.IsConnected)
                    {
                        _camera.Close();
                    }

                    _camera = new HikCamera();

                    // 枚举设备
                    var devices = _camera.EnumDevices();
                    if (devices == null || devices.Count == 0)
                        return (false, "未发现相机设备");

                    if (deviceIndex < 0 || deviceIndex >= devices.Count)
                        return (false, $"设备索引 {deviceIndex} 超出范围，共发现 {devices.Count} 台设备");

                    // 打开设备
                    _camera.Open(devices[deviceIndex]);

                    // 开始采集
                    _camera.StartGrabbing();

                    return (true, $"相机初始化成功，设备索引: {deviceIndex}");
                }
                catch (Exception ex)
                {
                    return (false, $"相机初始化失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 设置触发模式。
        /// </summary>
        /// <param name="isSoftTrigger">true = 软触发模式，false = 连续采集模式。</param>
        public static void SetTriggerMode(bool isSoftTrigger)
        {
            lock (_lock)
            {
                if (_camera == null || !_camera.IsConnected) return;
                _camera.SetTriggerMode(isSoftTrigger);
            }
        }

        /// <summary>
        /// 设置曝光时间。
        /// </summary>
        /// <param name="exposureTime">曝光时间（微秒）。</param>
        public static void SetExposureTime(float exposureTime)
        {
            lock (_lock)
            {
                if (_camera == null || !_camera.IsConnected) return;
                _camera.SetExposureTime(exposureTime);
            }
        }

        /// <summary>
        /// 设置增益。
        /// </summary>
        /// <param name="gain">增益值。</param>
        public static void SetGain(float gain)
        {
            lock (_lock)
            {
                if (_camera == null || !_camera.IsConnected) return;
                _camera.SetGain(gain);
            }
        }

        /// <summary>
        /// 触发拍照：执行软触发 → 获取帧 → 返回 ImageData。
        /// 零 PNG 编解码开销，直接从 Mat 提取原始像素。
        /// </summary>
        /// <returns>ImageData 原始像素数据；失败返回 null。</returns>
        public static ImageData CaptureImageData()
        {
            lock (_lock)
            {
                if (_camera == null || !_camera.IsConnected) return null;

                try
                {
                    _camera.SoftTrigger();
                    using Mat frame = _camera.GetFrame();
                    if (frame == null || frame.Empty()) return null;
                    return VisionAlgorithmService.MatToImageData(frame);
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 触发拍照（向后兼容）：执行软触发 → 获取帧 → 转换为 PNG byte[]。
        /// </summary>
        public static byte[] Capture()
        {
            var img = CaptureImageData();
            return img != null ? VisionAlgorithmService.ImageDataToPngBytes(img) : null;
        }

        /// <summary>
        /// 关闭相机：停止采集、关闭设备、释放资源。
        /// </summary>
        public static void Close()
        {
            lock (_lock)
            {
                if (_camera == null) return;

                try
                {
                    _camera.Close();
                }
                catch
                {
                    // 忽略关闭时的异常
                }
                finally
                {
                    _camera = null;
                }
            }
        }
    }
}
