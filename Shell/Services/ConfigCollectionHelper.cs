using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Nodify;
using Shell.Models.Nodes.Motion;

namespace Shell.Services
{
    /// <summary>
    /// 管理可过滤配置集合的标准模式：初始化首项、增删命令、Siblings 注入、延迟刷新。
    /// </summary>
    public static class ConfigCollectionHelper
    {
        public static void Initialize<TItem, TConfig>(
            ObservableCollection<TConfig> configs,
            Func<TConfig> newConfig,
            out DelegateCommand addCmd,
            out DelegateCommand<TConfig> removeCmd)
            where TConfig : FilteredConfigBase<TItem, TConfig>, new()
            where TItem : class
        {
            AddOne<TItem, TConfig>(configs, newConfig());

            addCmd = new DelegateCommand(() => AddOne<TItem, TConfig>(configs, newConfig()));
            removeCmd = new DelegateCommand<TConfig>(cfg =>
            {
                if (cfg != null) { configs.Remove(cfg); foreach (var c in configs) c.NotifyFilteredChanged(); }
            });

            configs.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (TConfig item in e.NewItems) item.Siblings = configs;
                foreach (var c in configs) c.NotifyFilteredChanged();
            };
        }

        /// <summary>添加一项并自动选择第一个可用项，避免空白行。</summary>
        private static void AddOne<TItem, TConfig>(ObservableCollection<TConfig> configs, TConfig cfg)
            where TConfig : FilteredConfigBase<TItem, TConfig>
            where TItem : class
        {
            configs.Add(cfg);

            // 添加后 FilteredItems 已由 CollectionChanged → RefreshAll 重建，
            // 此时选中第一个可用项，避免空行导致的保存错误
            if (cfg.FilteredItems.Count > 0)
                cfg.SelectedItem = cfg.FilteredItems[0];
        }

        public static TConfig CreateConfig<TItem, TConfig>(
            ObservableCollection<TConfig> configs,
            Action scheduleRefresh)
            where TConfig : FilteredConfigBase<TItem, TConfig>, new()
            where TItem : class
        {
            var cfg = new TConfig { Siblings = configs };
            cfg.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Id")
                    scheduleRefresh();
            };
            return cfg;
        }

        public static void ScheduleRefresh(Action refresh)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background, refresh);
        }
    }
}

