# Shell 流程编辑器 — 开发者指南

> 版本: 3.0 | 目标框架: .NET 8.0 + WPF | 更新: 2026-06-11

## 目录

1. [架构概览](#1-架构概览)
2. [快速上手：视觉节点](#2-快速上手视觉节点)
3. [快速上手：运动控制节点](#3-快速上手运动控制节点)
4. [属性系统详解](#4-属性系统详解)
5. [FilteredConfigBase 泛型基类](#5-filteredconfigbase-泛型基类)
6. [通用属性编辑器](#6-通用属性编辑器)
7. [视觉算法节点开发](#7-视觉算法节点开发)
8. [序列化与持久化](#8-序列化与持久化)
9. [工具箱注册与分类](#9-工具箱注册与分类)
10. [主题与样式](#10-主题与样式)
11. [执行引擎](#11-执行引擎)
12. [常见问题](#12-常见问题)

---

## 1. 架构概览

```
Shell/
├── Models/
│   ├── Nodes/
│   │   ├── NodeViewModel.cs          ← 所有节点的抽象基类
│   │   ├── Vision/VisionNodeBase.cs   ← 视觉算法节点基类
│   │   └── Vision/*NodeViewModel.cs   ← 具体视觉算法节点
│   ├── Attributes/
│   │   └── NodeAttribute.cs          ← [Node] / [NodeProperty] / [NodeConnector]
│   ├── PropertyItem.cs               ← 反射属性描述项（通用编辑器核心）
│   ├── PropertyGroup.cs              ← 属性分组模型
│   ├── EnumOption.cs                 ← 下拉选项模型
│   └── VariantValue.cs               ← 连接器值类型系统
├── Services/
│   ├── Algorithms/Vision/
│   │   └── VisionAlgorithmService.cs ← 图像处理算法（纯静态，零 UI 依赖）
│   ├── GraphExecutor.cs              ← 拓扑排序执行引擎
│   ├── GraphSerializer.cs            ← JSON 序列化/反序列化
│   └── NodeRegistry.cs              ← [Node] 反射自动发现
├── ViewModels/
│   ├── MainWindowViewModel.cs        ← 主窗口逻辑
│   └── ToolBoxViewModel.cs           ← 工具箱分类
├── Views/
│   ├── MainWindow.xaml               ← 主界面
│   ├── NodeEditorWindow.xaml         ← 节点编辑弹窗（可滚动+可缩放）
│   ├── NodeEditTemplateSelector.cs   ← 编辑模板选择器
│   └── PropertyEditorSelector.cs     ← 通用属性编辑器选择器
└── Style/
    ├── NodeTemplates.xaml            ← 节点渲染模板（画布显示）
    └── NodeEditTemplates.xaml        ← 节点编辑模板（弹窗编辑）
```

### 核心设计原则

| 原则 | 说明 |
|------|------|
| **模板方法模式** | 视觉节点重写 `ProcessImage`，运动节点继承 `MotionNodeBase` |
| **属性驱动** | `[Node]` / `[NodeProperty]` 标记 + 反射自动生成 UI |
| **泛型配置基类** | `FilteredConfigBase<TItem, TSelf>` 封装下拉选择、过滤、缓存、安全快照 |
| **集合管理模式** | `ConfigCollectionHelper` 统一管理多行配置的增删刷新逻辑 |
| **流式执行** | FlowExecutor 5 步流水线 + ILoopNode 统一循环管理 |
| **硬件抽象** | `IControlCard` 接口 + `CardManager` 版本号同步，解耦硬件与 UI |

---

## 2. 快速上手：视觉节点

### 示例：创建一个「图像锐化」节点

#### 步骤 1：在 VisionAlgorithmService 中添加算法

```csharp
// 文件: Services/Algorithms/Vision/VisionAlgorithmService.cs

/// <summary>图像锐化 (Laplacian 叠加)</summary>
public static byte[] Sharpen(byte[] pngInput, double strength)
{
    using var src = PngBytesToMat(pngInput);
    if (src.Empty()) return pngInput;
    using var blurred = new Mat();
    Cv2.GaussianBlur(src, blurred, new Size(3, 3), 1);
    using var dst = new Mat();
    Cv2.AddWeighted(src, 1 + strength, blurred, -strength, 0, dst);
    return MatToPngBytes(dst);
}
```

#### 步骤 2：创建 NodeViewModel（唯一需要的新文件）

```csharp
// 新建文件: Models/Nodes/Vision/SharpenNodeViewModel.cs

using Shell.Models.Attributes;
using Shell.Services.Algorithms.Vision;

namespace Shell.Models.Nodes.Vision
{
    [Node(
        Category = "视觉算法",
        DisplayName = "图像锐化",
        DefaultTitle = "图像锐化",
        Description = "Laplacian 锐化增强图像边缘",
        NodeTypeId = "Vision.Sharpen")]

    [NodeConnector(Title = "输入图像", Direction = ConnectorDirection.Input,
        ExpectedType = "Object")]
    [NodeConnector(Title = "输出图像", Direction = ConnectorDirection.Output,
        ExpectedType = "Object")]

    public class SharpenNodeViewModel : VisionNodeBase
    {
        public SharpenNodeViewModel() : base("图像锐化") { }

        // 可配置参数 — 加 [NodeProperty] 即可自动出现在编辑弹窗
        private double _strength = 1.5;
        [NodeProperty(Key = "strength",
                       DisplayName = "锐化强度",
                       Group = "锐化参数")]
        public double Strength
        {
            get => _strength;
            set => SetProperty(ref _strength, value);
        }

        // 核心算法 — 只需实现这个方法
        protected override byte[] ProcessImage(byte[] input)
            => VisionAlgorithmService.Sharpen(input, Strength);
    }
}
```

**完成！** 无需修改任何 XAML 或其他文件。新节点自动：
- ✅ 出现在工具箱「视觉算法」分类中
- ✅ 支持拖放到画布
- ✅ 双击弹出参数编辑窗口（自动生成 UI）
- ✅ 支持 JSON 序列化/反序列化

---

## 3. 快速上手：运动控制节点

### 示例：创建一个「停止轴」节点

运动控制节点遵循统一的模式：**配置模型** + **节点 ViewModel** + **编辑模板**。

#### 步骤 1：创建配置模型（继承 FilteredConfigBase）

```csharp
// 新建文件: Models/Nodes/Motion/StopConfig.cs

using Hardware.Card.Models;
using Shell.Services;

namespace Shell.Models.Nodes.Motion
{
    public sealed class StopConfig : FilteredConfigBase<AxisParameter, StopConfig>
    {
        [JsonPropertyName("AxisId")]
        public int AxisId { get => Id; set => Id = value; }

        // 只需实现 3 个抽象方法，基类自动提供下拉绑定/过滤/缓存/安全快照
        protected override IList<AxisParameter>? GetAllItems() => CardManager.Card?.AxisList;
        protected override int GetItemId(AxisParameter item) => item.RegID;
        protected override string? GetItemName(AxisParameter item) => item.Name;
    }
}
```

#### 步骤 2：创建节点 ViewModel

```csharp
// 新建文件: Nodes/Motion/StopNodeViewModel.cs

[Node(Category = "运动控制", DisplayName = "停止轴",
      DefaultTitle = "停止轴", NodeTypeId = "Motion.Stop")]
public class StopNodeViewModel : MotionNodeBase
{
    public StopNodeViewModel()
    {
        ConfigCollectionHelper.Initialize<AxisParameter, StopConfig>(
            AxisConfigs,
            () => ConfigCollectionHelper.CreateConfig<AxisParameter, StopConfig>(...),
            out var add, out var remove);
        AddAxisCommand = add; RemoveAxisCommand = remove;
    }

    [NodeProperty(Key = "axisConfigs", DisplayName = "停止轴列表")]
    public ObservableCollection<StopConfig> AxisConfigs { get; set; } = new();

    public override void Execute()
    {
        foreach (var cfg in AxisConfigs) Card?.Stop(cfg.AxisId);
        SetOutputBool(true);
    }
}
```

#### 步骤 3：注册编辑模板并创建 XAML

在 `NodeEditTemplateSelector.cs` 添加 `StopEditTemplate` 属性，在 `NodeEditTemplates.xaml` 中添加 ItemsControl + ComboBox 模板（参考现有 `VMoveEditTemplate`）。

**完成！** 新节点自动支持：
- ✅ 下拉选择轴（过滤已选、自动排除兄弟行选择）
- ✅ 多行添加/删除
- ✅ 硬件配置变更自动同步（CardManager 版本号机制）
- ✅ JSON 序列化/反序列化

### 现有运动控制节点速览

| 节点 | 配置模型 | 硬件 API | 特点 |
|------|---------|---------|------|
| 轴运动 | `MotorMoveConfig` | `AbsMove`/`RelMove` | 多轴 + 定位方式 |
| 电机复位 | `ResetAxisConfig` | `ProcessHomeMove` | 多轴 + 速度 |
| 等待输入 | `SignalConfig` | `ReadIn` | 多信号 + 判断条件 |
| 输出信号 | `OutputSignalConfig` | `WriteState` | 批量 ON/OFF |
| 停止轴 | `StopConfig` | `Stop` | 多轴停止 |
| 连续运动 | `VMoveConfig` | `VMove` | 匀速不停 |
| 轴IO检测 | `SensorCheckConfig` | `GetORGStatus`/`GetPEL`/`GetNEL` | 多轴 + ON/OFF |
| 控制卡初始化 | `AxisInitConfig` | `Init` + 参数配置 | 脉冲/传感器/使能 |
| 控制卡关闭 | — | `Close` | 释放资源 |
| 同步 | — | — | 多入1出，动态端口 |

---

## 4. 属性系统详解

### 3.1 `[Node]` — 节点注册

```csharp
[Node(
    Category    = "视觉算法",        // 工具箱分类名
    DisplayName = "高斯模糊",        // 工具箱显示文本
    DefaultTitle = "高斯模糊",       // 新建节点的默认标题
    Description  = "对图像应用...",   // 鼠标悬停提示
    NodeTypeId   = "Vision.GaussianBlur"  // 全局唯一标识符（序列化用）
)]
```

> **`NodeTypeId` 命名规范：** `{领域}.{功能}`，如 `Vision.GaussianBlur`。

### 3.2 `[NodeProperty]` — 可配置参数

```csharp
[NodeProperty(
    Key         = "kernelSize",    // 序列化键名（必须唯一）
    DisplayName = "核大小",         // 弹窗编辑器的标签文字
    Group       = "模糊参数",      // 分组名（相同 Group 合并为一个可折叠区域）
    Options     = "选项1,选项2"    // 逗号分隔的下拉选项（渲染为 ComboBox）
)]
public int KernelSize { get; set; }
```

**支持的属性类型与自动生成的编辑器控件：**

| C# 类型 | 编辑器控件 | 说明 |
|---------|-----------|------|
| `double` / `float` / `int` / `long` | `NumericTextBox` | 支持 Min/Max 范围限制，Float/Int 模式 |
| `bool` | `CheckBox` | |
| `string` | `TextBox` | |
| 任何 `enum` | `ComboBox` | 自动枚举所有值作为选项 |
| 任意类型 + `Options="..."` | `ComboBox` | 强制下拉，选项由 Options 指定 |
| 任意类型 + `DynamicOptionsSource="方法名"` | `ComboBox` | 运行时调用节点方法获取动态选项 |

**数值属性增强 (`Min` / `Max` / `DynamicOptionsSource`)：**

```csharp
// 带范围限制的数值输入
[NodeProperty(Key = "speed", DisplayName = "速度", Min = 1, Max = 500)]
public double Speed { get; set; }

// 动态下拉：调用节点上的 GetCameraList 方法获取选项
[NodeProperty(Key = "device", DisplayName = "相机",
    DynamicOptionsSource = "GetCameraList")]
public string DeviceId { get; set; }

// 节点上需要定义对应方法
public IEnumerable<string> GetCameraList() => CameraManager.GetDeviceNames();
```

**`Options` 用法详解：**

```csharp
// 将 int 属性渲染为下拉选择框（0=水平翻转, 1=垂直翻转, 2=双向翻转）
[NodeProperty(Key = "flipMode", DisplayName = "翻转方向",
    Options = "水平翻转,垂直翻转,双向翻转")]
public int SelectedModeIndex { get; set; }
```

> `Options` 会**覆盖**属性类型检测。即使属性是 `int`，设置 Options 后也会渲染为 ComboBox。

### 3.3 `[NodeConnector]` — 连接器声明

```csharp
[NodeConnector(
    Title        = "输入图像",
    Direction    = ConnectorDirection.Input,
    ExpectedType = "Object",       // Double / String / Object / Boolean
    Description  = "PNG 编码的 byte[] 图像数据")]
```

> ⚠️ `[NodeConnector]` 仅用于工具箱提示和文档生成。**实际连接器必须在构造函数中创建**（VisionNodeBase 已自动创建输入/输出图像连接器）。

## 5. FilteredConfigBase 泛型基类

所有需要"下拉选择硬件资源 + 排除兄弟行已选项"的配置模型都继承此类。

### 基类提供的功能

| 功能 | 实现 |
|------|------|
| **下拉数据源** | `FilteredItems`（缓存 + 版本检测，硬件变更自动重建） |
| **选择绑定** | `SelectedItem`（回查硬件原始列表，setter 自动解析 Id/Name） |
| **过滤已选** | `Siblings` 集合，BuildFiltered 排除兄弟行 `Id>=0` 的项 |
| **安全快照** | `try { new List<TItem>(all); }` 防止硬件线程并发修改 |
| **未选保护** | `Id` 默认 `-1`，过滤时排除 `<0` 的值，避免误伤真实 `RegID=0` |
| **版本同步** | `_lastCardVersion` 与 `CardManager.CardVersion` 比较，配置变更自动刷新 |

### 子类契约

```csharp
public sealed class MyConfig : FilteredConfigBase<AxisParameter, MyConfig>
{
    // 1. 序列化属性（映射到基类 Id）
    [JsonPropertyName("AxisId")]
    public int AxisId { get => Id; set => Id = value; }

    // 2. 自定义参数
    public double Speed { get; set; }

    // 3. 抽象方法实现
    protected override IList<AxisParameter>? GetAllItems() => CardManager.Card?.AxisList;
    protected override int GetItemId(AxisParameter item) => item.RegID;
    protected override string? GetItemName(AxisParameter item) => item.Name;
}
```

### ConfigCollectionHelper 辅助类

封装了四个运动节点中重复的模式：

```csharp
// 一行代码完成初始化：首项 + 增删命令 + Siblings 注入 + 刷新调度
ConfigCollectionHelper.Initialize<AxisParameter, MyConfig>(
    configs,
    () => ConfigCollectionHelper.CreateConfig<AxisParameter, MyConfig>(configs, ScheduleRefresh),
    out var addCmd, out var removeCmd);
```

- `CreateConfig` 自动订阅 PropertyChanged → Id 变化 → ScheduleRefresh
- `ScheduleRefresh` 使用 `Dispatcher.BeginInvoke(Background, ...)` 防止重入
- 添加时自动选择第一个可用项（`AddOne` 方法）

---

## 6. 通用属性编辑器

### 运作流程

```
双击节点
  → NodeEditorWindow 打开 (SizeToContent=Height, MaxHeight=700, 可缩放)
  → EditTemplateSelector 匹配节点类型
  ├─ ConstantNodeViewModel? → ConstantEditTemplate (专用)
  ├─ FunctionNodeViewModel? → FunctionEditTemplate (专用)
  ├─ VisionNodeBase 子类?   → GenericEditTemplate (通用)  ← 新增节点走这里
  └─ 其他?                  → DefaultEditTemplate (仅标题编辑)

GenericEditTemplate:
  1. 显示 "⚙ {节点名称}" 标题 + "节点名称" 编辑框
  2. 反射扫描所有 [NodeProperty] 属性
  3. 按 Group 分组 → 生成可折叠区域 (ToggleButton 标题)
  4. 每个属性根据类型选择编辑器控件 (PropertyEditorSelector)
  5. 全部包裹在 ScrollViewer 中（参数多时可滚动）
```

### 编辑器控件选择逻辑

```
PropertyItem.EditorType 确定顺序:
  1. attr.DynamicOptionsSource 非空? → "Enum" → ComboBox（动态选项）
  2. attr.Options 非空?           → "Enum" → ComboBox（静态选项）
  3. 属性类型是 double/int 等?    → "Number" → NumericTextBox（支持 Min/Max）
  4. 属性类型是 bool?             → "Boolean" → CheckBox
  5. 属性类型是 enum?             → "Enum" → ComboBox
  6. 其他                        → "Text" → TextBox
```

### 何时需要专用模板？

仅当节点需要**复杂交互 UI** 时才写专用模板（如图像预览、文件浏览按钮、多列布局）。大多数参数简单的节点无需。

---

## 7. 视觉算法节点开发

### VisionNodeBase 契约

```csharp
public abstract class VisionNodeBase : NodeViewModel
{
    // 基类已提供:
    //   Input[0]   — "输入图像" 连接器 (ExpectedType = Object)
    //   Output[0]  — "输出图像" 连接器 (ExpectedType = Object)
    //   PreviewImage — 自动生成的缩略图 BitmapImage
    //   ImageInfo    — 自动提取的尺寸信息字符串
    //   Execute()    — 读取 Input[0] → 调用 ProcessImage → 写入 Output[0]

    // 子类只需实现:
    protected abstract byte[] ProcessImage(byte[] input);
}
```

### 多输出节点 (重写 Execute)

```csharp
public override void Execute()
{
    var inputVal = Input.ElementAtOrDefault(0)?.Value ?? VariantValue.Null;
    if (!inputVal.TryGetBytes(out var pngData) || pngData.Length == 0)
    {
        ImageInfo = "等待输入图像...";
        return;
    }

    var result = ProcessImage(pngData);
    if (result != null && result.Length > 0)
    {
        Output[0].Value = VariantValue.FromBytes(result);      // 输出图像
        Output[1].Value = VariantValue.FromInt32(lineCount);    // 额外输出

        var info = VisionAlgorithmService.GetImageInfo(result);
        ImageInfo = $"{info.Width}×{info.Height}, {info.Channels}ch, {lineCount} 条线";
        PreviewImage = VisionHelper.MakePreview(result);
    }
}
```

### VisionAlgorithmService 开发规范

```csharp
// 算法方法签名规范:
//   输入: byte[] pngInput  (PNG 编码的图像数据)
//   输出: byte[]            (PNG 编码的结果图像)

public static byte[] YourAlgorithm(byte[] pngInput, ...)
{
    using var src = PngBytesToMat(pngInput);
    if (src.Empty()) return pngInput;  // 空图直接返回原图

    using var dst = new Mat();
    // ... OpenCV 处理 ...
    return MatToPngBytes(dst);
}
```

### VariantValue 类型系统

```csharp
// 基本类型
VariantValue.FromDouble(3.14)
VariantValue.FromInt32(42)
VariantValue.FromBoolean(true)
VariantValue.FromString("hello")

// 二进制 (自动 Base64 序列化)
VariantValue.FromBytes(pngData)
VariantValue.FromDoubleArray(arr)

// 读取值
inputVal.TryGetDouble(out var d);
inputVal.TryGetBytes(out var bytes);
```

---

## 8. 序列化与持久化

所有 `[NodeProperty]` 标记的属性**自动参与 JSON 序列化**：

```csharp
// 保存时自动生成: { "kernelSize": 5, "sigmaX": 1.5 }
[NodeProperty(Key = "kernelSize")] public int KernelSize { get; set; }
[NodeProperty(Key = "sigmaX")]     public double SigmaX { get; set; }
```

### 保存/加载流程

```
保存: Nodes + Connections → GraphSerializer.Serialize() → JSON 文件
加载: JSON 文件 → GraphSerializer.Deserialize() → NodeFactory 还原
```

### 改名兼容

改名后旧存档无法加载？在 `[Node]` 中添加 `LegacyTypeIds`：

```csharp
[Node(
    Category = "运动控制",
    NodeTypeId = "Motion.Stop",           // 新 ID
    LegacyTypeIds = new[] { "Motion.LegacyStop" }  // 旧 ID 别名
)]
```

`NodeFactory` 启动时会同时注册新旧两种 ID 到同一个工厂函数。

---

## 9. 工具箱注册与分类

### 自动注册

启动时 `NodeRegistry.Initialize()` 反射扫描所有 `[Node]` 属性类 → 自动生成工具箱。

### 分类排除

在 `ToolBoxViewModel` 中可排除特定 Category：

```csharp
var excluded = new HashSet<string> { "算术", "运动控制" };
// ... 过滤 ...
```

### Category 命名约定

| Category | 用途 | 示例 |
|----------|------|------|
| `输入` | 数据源 | Constant |
| `流程控制` | 执行逻辑 | Delay, Condition, Loop, Sync |
| `视觉算法` | 图像处理 | GaussianBlur, Canny... |
| `运动控制` | 硬件运动 | MotorMove, ResetAxis, Stop, VMove... |
| `硬件采集` | 相机 | HikCamera |

---

## 10. 主题与样式

### 图标按钮

```xml
<!-- 中性色 -->
<Button Style="{StaticResource IconBtnStyle}">&#xf0c7;</Button>

<!-- 颜色变体 -->
IconBtnAccentStyle   <!-- 蓝 — 保存/加载 -->
IconBtnDangerStyle   <!-- 红 — 清空/删除 -->
IconBtnSuccessStyle  <!-- 绿 — 执行 -->
IconBtnWarnStyle     <!-- 橙 — 主题切换 -->
IconBtnPurpleStyle   <!-- 紫 — 自动布局 -->
```

### 节点状态指示

节点标题左侧执行状态图标（Path 矢量绘制，不受字体影响）：

| Idle | Running | Success | Error |
|------|---------|---------|-------|
| 无图标 | 蓝色实心圆 | 绿底白色对勾 | 红底白色叉号 |

---

## 11. 执行引擎

系统有双执行引擎：

### 9.1 FlowExecutor（流式执行器）— 主引擎

5 步流水线：`TryExecuteNode → PropagateOutputs → ManageLoopStack → ResolveNextNode → TryLoopBack`

统一循环管理接口 `ILoopNode`：

```csharp
public interface ILoopNode
{
    bool IsLooping { get; }
    void OnLoopEnter();
    void OnLoopExit();
    string LoopDescription { get; }
}
```

`WhileNode`、`LoopNode`、`WaitSignalNode` 均实现此接口，`ManageLoopStack` 统一处理压栈/出栈。

| 节点类型 | 接口 | Output[0] | Output[1] | 执行器行为 |
|---------|------|----------|----------|-----------|
| `FlowStart` | — | 触发 | — | 流程入口 |
| `FlowEnd` | — | — | — | 命中即结束 |
| `WhileNode` | `IBranchNode` | 循环体 | 退出 | 循环栈：真→压栈走[0]，假→出栈走[1] |
| `LoopNode` | `IBranchNode` | 循环体 | 完成 | 有限次：未达→压栈走[0]，完成→出栈走[1] |
| `WaitSignalNode` | `IBranchNode` | 收到信号 | 超时/停止 | 不参与循环栈，由回环连线显式控制循环 |
| `ConditionNode` | `IBranchNode` | 满足 | 不满足 | 一次性分支 |
| 普通节点 | — | — | — | 默认跟随 Output[0] |

> ⚠️ **关键约定：所有 `IBranchNode` 的 `Output[1]` 是退出/停止/异常路径。**
> 正常流程（旁路 BFS）**永远不执行** Output[1]，它只在流程取消时由
> `ExecuteCleanupPathAsync` 单独遍历。连线时请把 CameraClose 等清理节点连到 Output[1]。

**多回路旁路执行：** 活跃输出连多个下游时，回环节点走主流程，其余 BFS 递归执行。

**停止与清理：** 点击停止 → `CancellationToken` 取消 → 轮询节点抛出异常 → FlowExecutor 捕获 → `ExecuteCleanupPathAsync` 沿 **IBranchNode.Output[1]**（优先）或 Output[0]（兜底）链式执行到 FlowEnd。最后兜底执行所有未触发的 `CameraCloseNode`。

### 9.2 等待信号节点 (WaitSignalNode)

`NodeTypeId = "Flow.WaitSignal"`，用于外部触发式循环：

```
属性：信号变量（下拉选择 Boolean 变量，SkipBindingResolve=true）
      轮询间隔(ms) 默认 50

端口：触发 (Input) / 回环 (Input) / 收到信号 (Output[0]) / 超时停止 (Output[1])

行为：ExecuteAsync → while (!EvaluateSignal()) { await Task.Delay; }
      收到信号 → Execute（走 Output[0]）→ 复位变量（SetValueAndNotify）
      回环连线存在 → 流程自动回到等待；不存在 → 单次结束
```

### 9.3 变量系统

- `Variable.SetValueAndNotify()` — 直写字段 + 通知 UI 刷新
- `[NodeProperty(SkipBindingResolve = true)]` — 属性存变量名而非值
- `StringToBoolConverter` — 变量管理器面板 Bool 用 CheckBox 切换

### 9.4 GraphExecutor（拓扑执行器）

基于 Kahn 拓扑排序的 DAG 执行器，用于无环图批量计算。

---

## 12. 常见问题

**Q: 新节点没出现在工具箱？**
→ 检查是否有 `[Node]` 属性、Category 是否被排除、重新构建确保 DLL 更新。

**Q: 双击弹窗空白？**
→ 检查 `NodeEditTemplateSelector` 是否有该类型 case。视觉节点走 `VisionNodeBase => GenericEditTemplate`。

**Q: 参数在弹窗中不显示？**
→ 检查属性是否标记了 `[NodeProperty]`，是否有 public getter/setter。

**Q: 运动控制节点的下拉框空白？**
→ 确认硬件配置窗口已保存（底部版本号会递增）。FilteredConfigBase 通过 `CardManager.CardVersion` 自动检测变更。若仍空白，检查 `GetAllItems()` 返回的列表是否正确。

**Q: 新增配置行后第一项消失了？**
→ 旧版默认 `Id=0` 会与真实 `RegID=0` 冲突。现已修复为 `Id=-1` 表示"未选择"，且 `BuildFiltered` 排除 `Id<0` 的兄弟行。

**Q: 运动控制节点如何新增？**
→ 三步：创建继承 `FilteredConfigBase<T>` 的配置模型 → 创建继承 `MotionNodeBase` 的 ViewModel → 在 `NodeEditTemplates.xaml` 中添加 ItemsControl+ComboBox 模板。参考 `StopNodeViewModel` 或 `VMoveNodeViewModel`。

**Q: 硬件配置修改后节点下拉不更新？**
→ 硬件配置窗口保存时调用 `CardManager.NotifyChanged()` → 版本号递增 → 节点下次访问 `FilteredItems` 时检测版本差异自动重建。

**Q: 序列化后加载失败？**
→ 检查 `NodeTypeId` 唯一性，是否与 JSON 中 `type` 字段一致。改名后可通过 `LegacyTypeIds` 保持兼容。

**Q: 如何创建纯逻辑节点？**
→ 继承 `NodeViewModel`（而非 `VisionNodeBase`），手动创建连接器，标记 `[Node]` + `[NodeProperty]` 即可。参考 `SyncNodeViewModel`（动态端口）。

**Q: 等待信号节点收到信号但变量没复位？**
→ 检查 SignalVariable 是否正确绑定（属性面板 🔗 下拉选择变量，勿手动输入）。

**Q: 下拉框空白（通用）？**
→ 检查 `Options` 格式（逗号分隔），确认 `EnumOption` 使用属性而非字段。

**Q: 数值输入如何限制范围？**
→ 使用 `[NodeProperty(Min = 0, Max = 255)]` 或 `DynamicOptionsSource` 动态选项。

---

> 📝 **黄金法则：视觉节点写 `[Node]` + `[NodeProperty]` + `ProcessImage()`；运动节点写 `FilteredConfigBase<T>` + `MotionNodeBase` + XAML 模板。其余自动生成。**
