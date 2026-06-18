using System;
using Hardware.Card.Interface;

namespace Shell.Services
{
    /// <summary>
    /// 控制卡管理器 —— 全局静态访问点。
    /// 在应用启动时注入 IControlCard 实例后，所有运动节点均可通过此类访问硬件。
    /// </summary>
    public static class CardManager
    {
        private static IControlCard? _card;
        private static readonly object _lock = new();
        private static int _cardVersion;

        /// <summary>配置版本号：硬件参数变更时递增，节点通过此值检测是否需要刷新下拉列表。</summary>
        public static int CardVersion
        {
            get { lock (_lock) return _cardVersion; }
        }

        /// <summary>当前激活的控制卡实例。</summary>
        public static IControlCard? Card
        {
            get { lock (_lock) return _card; }
        }

        /// <summary>控制卡是否已就绪。</summary>
        public static bool IsReady
        {
            get { lock (_lock) return _card != null && _card.Initialized; }
        }

        /// <summary>通知配置已变更（不更换卡实例时调用），使节点下拉列表重新加载。</summary>
        public static void NotifyChanged()
        {
            lock (_lock) _cardVersion++;
        }

        /// <summary>注册或更新控制卡实例。</summary>
        public static void Register(IControlCard card)
        {
            lock (_lock) { _card = card ?? throw new ArgumentNullException(nameof(card)); _cardVersion++; }
        }

        /// <summary>注销控制卡实例，释放引用。</summary>
        public static void Unregister()
        {
            lock (_lock)
            {
                if (_card is IDisposable d) d.Dispose();
                _card = null;
                _cardVersion++;
            }
        }
    }
}
