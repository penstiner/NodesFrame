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

        /// <summary>当前激活的控制卡实例。</summary>
        public static IControlCard? Card => _card;

        /// <summary>控制卡是否已就绪。</summary>
        public static bool IsReady => _card != null && _card.Initialized;

        /// <summary>
        /// 注册控制卡实例（通常在 App 启动 / 初始化时调用一次）。
        /// </summary>
        public static void Register(IControlCard card)
        {
            _card = card;
        }
    }
}
