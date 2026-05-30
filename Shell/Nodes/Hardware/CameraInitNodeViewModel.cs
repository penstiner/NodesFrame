using System.Collections.Generic;
using Shell.Models.Attributes;
using Shell.Services;

namespace Shell.Models.Nodes.Hardware
{
    [Node(
        Category = "硬件采集",
        DisplayName = "相机初始化",
        DefaultTitle = "相机初始化",
        Description = "连接并初始化海康相机，设置基本参数",
        NodeTypeId = "Hardware.CameraInit")]
    [NodeConnector(Title = "触发", Direction = ConnectorDirection.Input,
        ExpectedType = "Boolean", Description = "接收上游触发信号")]
    [NodeConnector(Title = "状态", Direction = ConnectorDirection.Output,
        ExpectedType = "Boolean", Description = "初始化是否成功")]
    public class CameraInitNodeViewModel : NodeViewModel
    {
        public CameraInitNodeViewModel()
        {
            Title = "相机初始化";

            // 触发输入
            AddInputConnector(new ConnectorViewModel
            {
                Title = "触发",
                ExpectedType = System.TypeCode.Boolean
            });

            // 状态输出
            AddOutputConnector(new ConnectorViewModel
            {
                Title = "状态",
                ExpectedType = System.TypeCode.Boolean
            });
        }

        /// <summary>
        /// 动态获取当前连接的相机设备列表（供 PropertyItem 反射调用）。
        /// </summary>
        public IReadOnlyList<string> GetAvailableDevices()
        {
            var devices = CameraManager.EnumDeviceNames();
            if (devices.Count == 0)
                return new List<string> { "未发现相机设备" };
            return devices;
        }

        private int _deviceIndex = 0;
        [NodeProperty(Key = "deviceIndex", DisplayName = "选择相机", Group = "相机设置",
            DynamicOptionsSource = nameof(GetAvailableDevices))]
        public int DeviceIndex
        {
            get => _deviceIndex;
            set => SetProperty(ref _deviceIndex, value);
        }

        private string _triggerMode = "软触发";
        [NodeProperty(Key = "triggerMode", DisplayName = "触发模式", Group = "相机设置",
            Options = "连续采集,软触发")]
        public string TriggerMode
        {
            get => _triggerMode;
            set => SetProperty(ref _triggerMode, value);
        }

        private float _exposureTime = 10000;
        [NodeProperty(Key = "exposureTime", DisplayName = "曝光时间(us)", Group = "相机参数")]
        public float ExposureTime
        {
            get => _exposureTime;
            set => SetProperty(ref _exposureTime, value);
        }

        public override void Execute()
        {
            var (success, message) = CameraManager.Initialize(DeviceIndex);
            if (success)
            {
                // 设置触发模式
                CameraManager.SetTriggerMode(TriggerMode == "软触发");
                // 设置曝光
                CameraManager.SetExposureTime(ExposureTime);
                ExecutionLogger.Success("相机初始化", message);
            }
            else
            {
                ExecutionLogger.Error("相机初始化", message);
            }
            Output[0].Value = VariantValue.FromBoolean(success);
        }
    }
}
