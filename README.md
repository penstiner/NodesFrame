# NodesFrame

基于 WPF 的运控视觉流程编辑器，采用节点图（Node Graph）范式，支持拖拽式构建图像处理、流程控制与运动控制管线，一键执行并实时预览结果。

## 功能特性

- **可视化节点编辑** — 拖拽创建节点、连接端口、自由布局，支持撤销/重做
- **视觉算法库** — 内置 12+ 种 OpenCV 图像处理算法（高斯模糊、Canny边缘、二值化、形态学、霍夫直线等）
- **硬件采集** — 海康相机集成（初始化→触发拍照→关闭），支持动态枚举已连接设备
- **运动控制** — 控制卡初始化 → 电机运动 / 多轴定位 → 等待输入信号 → 输出信号，完整闭环
- **流程控制** — 延时、条件判断、重复N次循环、While循环、等待信号 ⏳ 等节点，支持回环连线与外部触发式循环
- **多回路分支** — 单个输出可连接多个下游，主流程走回环路径，旁路 BFS 递归执行
- **FlowExecutor 引擎** — 5 步流水线（TryExecute → PropagateOutputs → ManageLoopStack → ResolveNextNode → TryLoopBack），ILoopNode 统一循环管理
- **高性能图像传递** — 节点间使用 ImageData 原始像素直传，零 PNG 编解码开销
- **WriteableBitmap 预览** — 复用式位图 + UI 写入节流，支持高频刷新场景
- **实时图像预览** — 右侧面板实时显示处理结果，支持缩放与对比模式
- **动态属性系统** — 基于 `[NodeProperty]` 反射扫描，支持动态下拉选项、数值范围限制、变量绑定
- **变量系统** — `IVariableTypeHandler` 可扩展类型注册（Bool/Double/Int32/String），变量管理器弹窗 + 面板实时查看
- **三种主题** — Dark / Light / Nodify 主题一键切换，HandyControl 暗色主题集成
- **序列化与持久化** — JSON 格式保存/加载完整流程图
- **执行日志** — 带级别着色的实时日志面板，支持折叠与清空
- **自动布局** — 一键自动整理节点位置

## 技术栈

| 技术 | 版本 | 用途 |
|------|------|------|
| .NET | 8.0 | 运行时框架 |
| WPF | - | UI 框架 |
| Nodify | 7.3.0 | 节点图编辑器控件 |
| OpenCvSharp4 | 4.13.0 | 图像处理算法 |
| Prism.Unity | 8.1.97 | 依赖注入 |
| ReactiveUI | 23.2.27 | 响应式 MVVM + WriteableBitmap 预览 |
| MvCamCtrl.NET | - | 海康相机 SDK |
| HandyControl | - | WPF 控件库 + 暗色主题 |

## 项目结构

```
Shell.sln
├── Shell/                    # 主应用程序
│   ├── Attributes/           # 节点属性标记（[Node], [NodeProperty], [NodeConnector]）
│   ├── Hardware/             # 硬件驱动封装（海康相机 SDK）
│   ├── Models/               # 数据模型与框架类型
│   │   ├── Nodes/            # 节点数据模型（SignalConfig, OutputSignalConfig, AxisConfig）
│   │   ├── Variable.cs       # 变量模型（ValueString, IVariableTypeHandler）
│   │   └── PropertyItem.cs   # 属性编辑器模型（反射驱动，支持 Number/Boolean/Enum/Text）
│   ├── Nodes/                # 节点 ViewModel
│   │   ├── Flow/             # 流程开始/结束、While循环、等待信号 ⏳ 节点
│   │   ├── Hardware/         # 相机初始化/拍照/关闭节点
│   │   ├── Motion/           # 电机运动 / 多轴定位 / 控制卡初始化 / 等待输入信号 / 输出信号
│   │   └── Vision/           # 视觉算法节点（VisionNodeBase 基类，31 种算法）
│   ├── Services/             # 服务层
│   │   ├── FlowExecutor.cs           # 流水线执行引擎（回环 + 旁路 + ILoopNode）
│   │   ├── VariableManager.cs        # 全局变量管理器
│   │   ├── VariableTypeRegistry.cs   # IVariableTypeHandler 类型注册中心
│   │   ├── VariableTypeHandlers.cs   # Bool/Double/Int32/String 处理器
│   │   ├── NodeFactory.cs            # 节点反射工厂 + JSON 序列化
│   │   ├── CardManager.cs            # 控制卡全局访问点
│   │   └── Algorithms/      # 视觉算法实现（ImageData 输入输出）
│   ├── ViewModels/           # 主窗口与工具箱 ViewModel
│   ├── Views/                # XAML 视图
│   │   ├── NodeEditorWindow.xaml     # 节点属性编辑弹窗
│   │   ├── VariableManagerDialog.xaml # 变量管理弹窗
│   │   └── HardwareConfigWindow.xaml # 硬件参数配置窗口
│   └── Style/                # 样式与模板资源
│       ├── NodeTemplates.xaml        # 画布节点渲染模板
│       └── NodeEditTemplates.xaml    # 节点编辑模板选择器（含通用反射 + 专用模板）
├── Core.UI/                  # 通用 UI 组件库
│   ├── Controls/             # 自定义控件（NumericTextBox, ImageViewer）
│   ├── Converter/            # 值转换器
│   ├── Font/                 # 字体资源（FontAwesome + iconfont）
│   └── Style/                # 基础样式
├── Hardware.Card/            # 运动控制卡抽象层
│   ├── Interface/            # IControlCard 接口 + ControlCardBase 基类
│   ├── Models/               # IOParameter, AxisParameter 模型
│   └── Enum/                 # IO_STATUS 枚举
└── Nodify.Shared/            # 共享基础设施
    ├── Controls/             # 自定义控件
    ├── Converters/           # 通用转换器
    ├── Themes/               # 主题资源（Dark/Light）
    └── UndoRedo/             # 撤销重做框架
```

## UI 特性

- **工具箱** — 卡片化节点列表，左侧彩色类型标识条 + iconfont 专属图标，支持实时搜索过滤
- **多字体图标** — FontAwesome（通用UI图标）+ iconfont（视觉算法专属图标）动态切换
- **主题系统** — 18 个颜色参数全局主题切换 + HandyControl SkinDark 暗色主题
- **变量管理** — 弹窗式变量管理（Name / Type / Value / Description），类型色条 + 专用编辑器
- **节点编辑** — 分组折叠、属性编辑器选择器（NumericTextBox / 开关 / 下拉 / 文本框）、输出变量绑定
- **NumericTextBox** — 自定义数值输入控件，支持 Int/Float 模式、范围限制、虚拟数字键盘
- **图像预览** — 单图/对比双模式，鼠标滚轮缩放、拖拽平移，自适应缩放
- **执行日志** — 级别着色（信息/警告/错误/成功），底部微分隔线，可折叠
- **硬件参数配置** — Tab 页形式配置轴参数 / 输入信号 / 输出信号（DataGrid 编辑）

## 节点速览

| 类别 | 节点 | 说明 |
|------|------|------|
| 流程 | 开始 / 结束 | 流程入口与出口 |
| 流程 | 延时 | 等待指定时间 |
| 流程 | 条件判断 | 数值比较（> / < / == / ≥ / ≤） |
| 流程 | While 循环 | 带条件的重复循环 |
| 流程 | 等待信号 ⏳ | 外部信号触发放行 |
| 运动 | 控制卡初始化 | 初始化运动控制板卡 |
| 运动 | 电机运动 | 单轴绝对/相对定位 |
| 运动 | 多轴定位 | 多轴同步运动配置（DataGrid） |
| 运动 | 等待输入信号 | 等待指定输入 IO 信号为 ON/OFF |
| 运动 | 输出信号 | 批量设置输出 IO 信号为 ON/OFF |
| 视觉 | 31 种算法 | 高斯模糊、Canny、二值化、形态学、颜色转换、缩放等 |

## 快速开始

### 环境要求

- Windows 10/11
- .NET 8.0 SDK
- Visual Studio 2022 或 Rider

### 构建与运行

```bash
# 克隆项目
git clone <repo-url>

# 还原依赖
dotnet restore Shell.sln

# 构建
dotnet build Shell.sln

# 运行
dotnet run --project Shell/Shell.csproj
```

### 基本使用

1. 从左侧工具箱拖拽节点到画布
2. 连接节点端口建立数据流
3. 双击节点编辑参数
4. 点击「执行」按钮运行流程
5. 在右侧面板查看图像处理结果

## 文档导航

- [开发者指南](Developer-Guide.md) — 节点开发、属性系统、执行引擎详解
- [Nodify 入门](docs/Getting-Started.md) — 底层节点图控件使用教程
- [API 参考](docs/api/API-Reference.md) — Nodify 控件 API 文档

## 许可证

MIT License
