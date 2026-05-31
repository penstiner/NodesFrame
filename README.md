# NodesFrame

基于 WPF 的运控视觉流程编辑器，采用节点图（Node Graph）范式，支持拖拽式构建图像处理与流程控制管线，一键执行并实时预览结果。

## 功能特性

- **可视化节点编辑** — 拖拽创建节点、连接端口、自由布局，支持撤销/重做
- **视觉算法库** — 内置 12+ 种 OpenCV 图像处理算法（高斯模糊、Canny边缘、二值化、形态学、霍夫直线等）
- **硬件采集** — 海康相机集成（初始化→触发拍照→关闭），支持动态枚举已连接设备
- **流程控制** — 延时、条件判断、重复N次循环、等待信号⏳ 等节点，支持回环连线与外部触发式循环
- **多回路分支** — 单个输出可连接多个下游，主流程走回环路径，旁路 BFS 递归执行
- **双执行引擎** — FlowExecutor 流式执行（循环栈 + 轮询等待 + 旁路分支）+ GraphExecutor 拓扑排序
- **高性能图像传递** — 节点间使用 ImageData 原始像素直传，零 PNG 编解码开销
- **WriteableBitmap 预览** — 复用式位图 + UI 写入节流，支持高频刷新场景
- **实时图像预览** — 右侧面板实时显示处理结果，支持缩放与对比模式
- **动态属性系统** — 基于 Attribute 反射扫描，支持动态下拉选项（如相机设备列表）
- **三种主题** — Dark / Light / Nodify 主题一键切换
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

## 项目结构

```
Shell.sln
├── Shell/                    # 主应用程序
│   ├── Attributes/          # 节点属性标记（[Node], [NodeProperty], [NodeConnector]）
│   ├── Hardware/            # 硬件驱动封装（海康相机 SDK）
│   ├── Models/              # 数据模型与框架类型（VariantValue, ImageData, PropertyItem）
│   ├── Nodes/               # 节点 ViewModel
│   │   ├── Flow/            # 流程开始/结束、循环判断、等待信号⏳ 节点
│   │   ├── Hardware/        # 相机初始化/拍照/关闭节点
│   │   ├── Motion/          # 电机运动/序列/传感器节点
│   │   └── Vision/          # 视觉算法节点 + ImagePreview + VisionHelper
│   ├── Services/            # 服务层（FlowExecutor、GraphExecutor、序列化、VariableManager、CameraManager）
│   │   └── Algorithms/      # 视觉算法实现（ImageData 输入输出）
│   ├── ViewModels/          # 主窗口与工具箱 ViewModel
│   ├── Views/               # XAML 视图
│   └── Style/               # 样式与模板资源
├── Core.UI/                  # 通用 UI 组件库
│   ├── Converter/           # 值转换器
│   ├── Font/                # 字体资源（FontAwesome + iconfont）
│   └── Style/               # 基础样式
└── Nodify.Shared/            # 共享基础设施
    ├── Controls/            # 自定义控件
    ├── Converters/          # 通用转换器
    ├── Themes/              # 主题资源（Dark/Light）
    └── UndoRedo/            # 撤销重做框架
```

## UI 特性

- **工具箱** — 卡片化节点列表，左侧彩色类型标识条 + iconfont 专属图标，支持实时搜索过滤
- **多字体图标** — FontAwesome（通用UI图标）+ iconfont（视觉算法专属图标）动态切换
- **主题系统** — 18 个颜色参数全局主题切换，运行时动态更新无需重启
- **节点模板** — 8 种专用节点模板 + 1 种默认扩展模板，带执行状态动画
- **图像预览** — 单图/对比双模式，鼠标滚轮缩放，占位提示
- **执行日志** — 级别着色（信息/警告/错误/成功），底部微分隔线，可折叠

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
