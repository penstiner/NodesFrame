using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Shell.Models.Nodes.Motion
{
    /// <summary>单轴运动配置项，供 DataGrid 绑定。</summary>
    public sealed class AxisConfig : INotifyPropertyChanged
    {
        private int _axisId;
        public int AxisId
        {
            get => _axisId;
            set { _axisId = value; OnPropertyChanged(); }
        }

        private double _speed = 50;
        public double Speed
        {
            get => _speed;
            set { _speed = value; OnPropertyChanged(); }
        }

        private double _position;
        public double Position
        {
            get => _position;
            set { _position = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
