using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Hardware.Card.Interface;
using Hardware.Card.Models;
using Nodify;
using Shell.Services;

namespace Shell.ViewModels
{
    /// <summary>
    /// 硬件配置窗口 ViewModel。管理轴参数和 IO 参数的表格编辑。
    /// </summary>
    public class HardwareConfigViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<AxisParameter> Axes { get; } = new();
        public ObservableCollection<IOParameter> Inputs { get; } = new();
        public ObservableCollection<IOParameter> Outputs { get; } = new();

        public ICommand AddAxisCommand { get; }
        public ICommand RemoveAxisCommand { get; }
        public ICommand AddInputCommand { get; }
        public ICommand RemoveInputCommand { get; }
        public ICommand AddOutputCommand { get; }
        public ICommand RemoveOutputCommand { get; }

        public HardwareConfigViewModel()
        {
            AddAxisCommand = new DelegateCommand(() =>
                Axes.Add(new AxisParameter { RegID = Axes.Count, Name = $"轴{Axes.Count}" }));
            RemoveAxisCommand = new DelegateCommand<AxisParameter>(p => { if (p != null) Axes.Remove(p); });

            AddInputCommand = new DelegateCommand(() =>
                Inputs.Add(new IOParameter { RegID = Inputs.Count, Name = $"输入{Inputs.Count}" }));
            RemoveInputCommand = new DelegateCommand<IOParameter>(p => { if (p != null) Inputs.Remove(p); });

            AddOutputCommand = new DelegateCommand(() =>
                Outputs.Add(new IOParameter { RegID = Outputs.Count, Name = $"输出{Outputs.Count}" }));
            RemoveOutputCommand = new DelegateCommand<IOParameter>(p => { if (p != null) Outputs.Remove(p); });

            // 从已注册的控制卡加载现有配置
            LoadFromCard();
        }

        private void LoadFromCard()
        {
            var card = CardManager.Card;
            if (card == null) return;

            if (card.AxisList != null)
                foreach (var a in card.AxisList)
                    Axes.Add(new AxisParameter
                    {
                        RegID = a.RegID, Name = a.Name, CardID = a.CardID, AxisID = a.AxisID,
                        AccTime = a.AccTime, StartSpeed = a.StartSpeed,
                        HomeSpeed = a.HomeSpeed, HomeDis = a.HomeDis, Equiv = a.Equiv
                    });

            if (card.InBitList != null)
                foreach (var i in card.InBitList)
                    Inputs.Add(new IOParameter { RegID = i.RegID, Name = i.Name, Cardno = i.Cardno, Nodeno = i.Nodeno, Bitno = i.Bitno });

            if (card.OutBitList != null)
                foreach (var o in card.OutBitList)
                    Outputs.Add(new IOParameter { RegID = o.RegID, Name = o.Name, Cardno = o.Cardno, Nodeno = o.Nodeno, Bitno = o.Bitno });
        }

        /// <summary>将编辑后的数据写回控制卡。</summary>
        public void SaveToCard()
        {
            var card = CardManager.Card;
            if (card == null) return;

            card.AxisList = Axes.ToList();
            card.InBitList = Inputs.ToList();
            card.OutBitList = Outputs.ToList();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
