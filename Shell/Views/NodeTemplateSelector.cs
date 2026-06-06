using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Shell.Models;
using Shell.Models.Nodes.Vision;
using Shell.Models.Nodes.Motion;
using Shell.Services;

namespace Shell.Views
{
    /// <summary>
    /// 根据节点 ViewModel 类型选择不同的 DataTemplate 渲染。
    /// 支持硬编码类型映射 + 外部注册模板（通过 RegisterTemplate 方法）。
    /// </summary>
    public class NodeTemplateSelector : DataTemplateSelector
    {
        // ── 模板注册表：CLR Type → DataTemplate ──
        private static readonly Dictionary<Type, DataTemplate> _templateRegistry = new();

        public DataTemplate ConstantTemplate { get; set; }
        public DataTemplate FunctionTemplate { get; set; }
        public DataTemplate DisplayTemplate { get; set; }
        public DataTemplate DelayTemplate { get; set; }
        public DataTemplate ConditionTemplate { get; set; }
        public DataTemplate LoopTemplate { get; set; }
        public DataTemplate ImageDisplayTemplate { get; set; }
        public DataTemplate MotionControlTemplate { get; set; }
        public DataTemplate DefaultTemplate { get; set; }

        /// <summary>
        /// 注册自定义节点类型的模板。
        /// </summary>
        public static void RegisterTemplate(Type nodeType, DataTemplate template)
        {
            _templateRegistry[nodeType] = template;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null) return DefaultTemplate;

            var itemType = item.GetType();

            // 1. 从注册表查找
            if (_templateRegistry.TryGetValue(itemType, out var registered))
                return registered ?? DefaultTemplate;

            // 2. 硬编码类型匹配
            return item switch
            {
                ConstantNodeViewModel => ConstantTemplate ?? DefaultTemplate,
                FunctionNodeViewModel => FunctionTemplate ?? DefaultTemplate,
                DisplayNodeViewModel => DisplayTemplate ?? DefaultTemplate,
                DelayNodeViewModel => DelayTemplate ?? DefaultTemplate,
                ConditionNodeViewModel => ConditionTemplate ?? DefaultTemplate,
                LoopNodeViewModel => LoopTemplate ?? DefaultTemplate,
                ImageDisplayNodeViewModel => ImageDisplayTemplate ?? DefaultTemplate,
                MultiAxisMoveNodeViewModel => MotionControlTemplate ?? DefaultTemplate,
                MotorMoveNodeViewModel => MotionControlTemplate ?? DefaultTemplate,
                ControlCardInitViewModel => MotionControlTemplate ?? DefaultTemplate,
                _ => DefaultTemplate
            };
        }
    }
}
