using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
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
        private static readonly string _configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Shell");
        private static readonly string _configPath = Path.Combine(_configDir, "hardware_config.json");

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
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

            // 优先从本地文件加载，文件不存在时从控制卡加载默认配置
            if (File.Exists(_configPath))
            {
                LoadFromFile();
                SaveToCard(); // 将已保存的配置推送回控制卡
            }
            if (Axes.Count == 0 && Inputs.Count == 0 && Outputs.Count == 0)
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
            CardManager.NotifyChanged();
            OnPropertyChanged(nameof(CardVersionDisplay));
        }

        /// <summary>当前配置版本号，保存后自动更新。</summary>
        public string CardVersionDisplay => $"配置版本:  {CardManager.CardVersion}.0.0.0";

        /// <summary>将硬件配置保存到本地 JSON 文件。</summary>
        public void SaveToFile()
        {
            try
            {
                Directory.CreateDirectory(_configDir);
                var data = new
                {
                    Axes = Axes.ToList(),
                    Inputs = Inputs.ToList(),
                    Outputs = Outputs.ToList()
                };
                File.WriteAllText(_configPath, JsonSerializer.Serialize(data, _jsonOptions));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HardwareConfig] 保存失败：{ex.Message}");
            }
        }

        /// <summary>从本地 JSON 文件加载硬件配置。已加载时先清空现有集合。</summary>
        public void LoadFromFile()
        {
            if (!File.Exists(_configPath)) return;

            try
            {
                var json = File.ReadAllText(_configPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                Axes.Clear();
                if (root.TryGetProperty("axes", out var axesEl))
                {
                    var axes = JsonSerializer.Deserialize<List<AxisParameter>>(
                        axesEl.GetRawText(), _jsonOptions);
                    if (axes != null)
                        foreach (var a in axes) Axes.Add(a);
                }

                Inputs.Clear();
                if (root.TryGetProperty("inputs", out var inputsEl))
                {
                    var inputs = JsonSerializer.Deserialize<List<IOParameter>>(
                        inputsEl.GetRawText(), _jsonOptions);
                    if (inputs != null)
                        foreach (var i in inputs) Inputs.Add(i);
                }

                Outputs.Clear();
                if (root.TryGetProperty("outputs", out var outputsEl))
                {
                    var outputs = JsonSerializer.Deserialize<List<IOParameter>>(
                        outputsEl.GetRawText(), _jsonOptions);
                    if (outputs != null)
                        foreach (var o in outputs) Outputs.Add(o);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HardwareConfig] 加载失败：{ex.Message}");
            }
        }

        /// <summary>应用启动时调用：从本地文件恢复配置到控制卡。</summary>
        public static void InitCardFromFile()
        {
            if (!File.Exists(_configPath)) return;
            var vm = new HardwareConfigViewModel();
            // 构造函数已处理 LoadFromFile + SaveToCard
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
