using MvCamCtrl.NET;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shell.Hardware.Camera
{
    public class HikCamera
    {
        private MyCamera _camera;
        private bool _isGrabbing;
        private IntPtr _frameBuffer = IntPtr.Zero;
        private int _frameBufferSize = 0;
        private IntPtr _convertBuffer = IntPtr.Zero;
        private int _convertBufferSize = 0;

        public bool IsConnected { get; private set; }

        public HikCamera()
        {
            _camera = new MyCamera();
        }

        // 枚举设备
        public List<MyCamera.MV_CC_DEVICE_INFO> EnumDevices()
        {
            var deviceList = new List<MyCamera.MV_CC_DEVICE_INFO>();
            int nRet;
            var stDevList = new MyCamera.MV_CC_DEVICE_INFO_LIST();

            // 枚举 GigE 和 USB 相机
            nRet = MyCamera.MV_CC_EnumDevices_NET(
                MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE,
                ref stDevList);

            if (nRet != MyCamera.MV_OK)
                throw new Exception($"Enum Devices failed: {nRet:X}");

            for (int i = 0; i < stDevList.nDeviceNum; i++)
            {
                var device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(
                    stDevList.pDeviceInfo[i],
                    typeof(MyCamera.MV_CC_DEVICE_INFO));
                deviceList.Add(device);
            }

            return deviceList;
        }

        public void Open(MyCamera.MV_CC_DEVICE_INFO deviceInfo)
        {
            if (IsConnected) Close();

            _camera = new MyCamera();

            // Use CreateDevice as per standard/user pattern
            int nRet = _camera.MV_CC_CreateDevice_NET(ref deviceInfo);
            if (nRet != MyCamera.MV_OK) throw new Exception($"Create Device failed: {nRet:X}");

            nRet = _camera.MV_CC_OpenDevice_NET();
            if (nRet != MyCamera.MV_OK)
            {
                _camera.MV_CC_DestroyDevice_NET();
                throw new Exception($"Open Device failed: {nRet:X}");
            }

            // 探测网络最佳包大小(只对GigE有效, USB会报错但忽略)
            if (deviceInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            {
                int packetSize = _camera.MV_CC_GetOptimalPacketSize_NET();
                if (packetSize > 0)
                    _camera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)packetSize);
            }

            IsConnected = true;
        }

        public void StartGrabbing()
        {
            if (!IsConnected) return;

            int nRet = _camera.MV_CC_StartGrabbing_NET();
            if (nRet != MyCamera.MV_OK) throw new Exception($"Start Grabbing failed: {nRet:X}");

            _isGrabbing = true;
        }

        public void StopGrabbing()
        {
            if (!IsConnected || !_isGrabbing) return;
            _camera.MV_CC_StopGrabbing_NET();
            _isGrabbing = false;
        }

        public void Close()
        {
            if (_isGrabbing) StopGrabbing();
            if (IsConnected)
            {
                _camera.MV_CC_CloseDevice_NET();
                _camera.MV_CC_DestroyDevice_NET();
            }
            IsConnected = false;

            if (_frameBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_frameBuffer);
                _frameBuffer = IntPtr.Zero;
            }

            if (_convertBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_convertBuffer);
                _convertBuffer = IntPtr.Zero;
                _convertBufferSize = 0;
            }
        }

        // 获取一帧并转换为 Mat (OpenCvSharp)
        public Mat? GetFrame()
        {
            if (!IsConnected || !_isGrabbing) return null;

            var stFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();

            // 确保缓冲区足够大 (假设最大 20MB)
            if (_frameBuffer == IntPtr.Zero)
            {
                _frameBufferSize = 3072 * 2048 * 3 + 2048; // 预留大一点
                _frameBuffer = Marshal.AllocHGlobal(_frameBufferSize);
            }

            // 获取一帧图像（查询方式）
            int nRet = _camera.MV_CC_GetOneFrameTimeout_NET(
                _frameBuffer,
                (uint)_frameBufferSize,
                ref stFrameInfo,
                1000);

            if (nRet == MyCamera.MV_OK)
            {
                // 成功拿到数据，数据在 _frameBuffer，格式在 stFrameInfo.enPixelType

                // 1. 如果是 Mono8，直接构建 Mat (不复制)
                if (stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
                {
                    return Mat.FromPixelData(stFrameInfo.nHeight, stFrameInfo.nWidth, MatType.CV_8UC1, _frameBuffer);
                }
                // 2. 如果是 BGR8 Packed，直接构建 Mat (不复制)
                else if (stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_BGR8_Packed)
                {
                    return Mat.FromPixelData(stFrameInfo.nHeight, stFrameInfo.nWidth, MatType.CV_8UC3, _frameBuffer);
                }
                // 3. 其他格式需要转换 (如 BayerRG -> BGR)
                else
                {
                    int nConvertSize = stFrameInfo.nWidth * stFrameInfo.nHeight * 3;
                    if (_convertBuffer == IntPtr.Zero || _convertBufferSize < nConvertSize)
                    {
                        if (_convertBuffer != IntPtr.Zero) Marshal.FreeHGlobal(_convertBuffer);
                        _convertBuffer = Marshal.AllocHGlobal(nConvertSize);
                        _convertBufferSize = nConvertSize;
                    }

                    var stConvertParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();
                    stConvertParam.nWidth = stFrameInfo.nWidth;
                    stConvertParam.nHeight = stFrameInfo.nHeight;
                    stConvertParam.pSrcData = _frameBuffer;
                    stConvertParam.nSrcDataLen = stFrameInfo.nFrameLen;
                    stConvertParam.enSrcPixelType = stFrameInfo.enPixelType;
                    stConvertParam.enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_BGR8_Packed;
                    stConvertParam.pDstBuffer = _convertBuffer;
                    stConvertParam.nDstBufferSize = (uint)nConvertSize;

                    nRet = _camera.MV_CC_ConvertPixelType_NET(ref stConvertParam);
                    if (nRet == MyCamera.MV_OK)
                    {
                        // Wrap 转换后的 buffer
                        return Mat.FromPixelData(stFrameInfo.nHeight, stFrameInfo.nWidth, MatType.CV_8UC3, _convertBuffer);
                    }
                }
            }

            return null;
        }

        // 设置曝光时间
        public void SetExposureTime(float exposureTime)
        {
            _camera.MV_CC_SetFloatValue_NET("ExposureTime", exposureTime);
        }

        // 设置增益
        public void SetGain(float gain)
        {
            _camera.MV_CC_SetFloatValue_NET("Gain", gain);
        }

        // 设置触发模式
        public void SetTriggerMode(bool isTriggered)
        {
            if (!IsConnected) return;

            if (isTriggered)
            {
                // 打开硬件触发
                // TriggerMode: 1-On
                _camera.MV_CC_SetEnumValue_NET("TriggerMode", 1);
                // 设置触发源为软件
                // TriggerSource: 7-Software
                // Try catch to handle different camera models where Enum values might differ
                try { _camera.MV_CC_SetEnumValue_NET("TriggerSource", 7); }
                catch { _camera.MV_CC_SetEnumValueByString_NET("TriggerSource", "Software"); }
            }
            else
            {
                // 关闭硬件触发，切换到自由行模式
                // TriggerMode: 0-Off
                _camera.MV_CC_SetEnumValue_NET("TriggerMode", 0);
            }
        }

        // 执行一次软触发
        public void SoftTrigger()
        {
            if (!IsConnected) return;
            int nRet = _camera.MV_CC_SetCommandValue_NET("TriggerSoftware");
            if (nRet != MyCamera.MV_OK) throw new Exception($"Soft Trigger failed: {nRet:X}");
        }

        // 设置帧率控制
        public void SetFrameRate(bool enable, float frameRate)
        {
            if (!IsConnected) return;
            _camera.MV_CC_SetBoolValue_NET("AcquisitionFrameRateEnable", enable);
            if (enable)
            {
                _camera.MV_CC_SetFloatValue_NET("AcquisitionFrameRate", frameRate);
            }
        }
    }
}
