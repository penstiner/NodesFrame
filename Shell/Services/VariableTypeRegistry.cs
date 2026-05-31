using System.Collections.Generic;
using System.Linq;

namespace Shell.Services
{
    /// <summary>
    /// 变量类型注册表——用于扩展变量类型系统。
    /// 程序启动时注册所有 IVariableTypeHandler 实现。
    /// </summary>
    public static class VariableTypeRegistry
    {
        private static readonly Dictionary<string, IVariableTypeHandler> _handlers = new();

        public static void Register(IVariableTypeHandler handler)
            => _handlers[handler.TypeName] = handler;

        public static IVariableTypeHandler? Get(string typeName)
            => _handlers.TryGetValue(typeName, out var h) ? h : null;

        /// <summary>所有已注册的类型名列表。</summary>
        public static IReadOnlyList<string> TypeNames => _handlers.Keys.ToList();
    }
}
