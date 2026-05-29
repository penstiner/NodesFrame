# NodesFrame

基于 WPF 的运控视觉流程编辑器，采用节点图（Node Graph）范式，支持拖拽式构建图像处理与流程控制管线，一键执行并实时预览结果。

## 功能特性

- **可视化节点编辑** — 拖拽创建节点、连接端口、自由布局，支持撤销/重做
- **视觉算法库** — 内置 12+ 种 OpenCV 图像处理算法（高斯模糊、Canny边缘、二值化、形态学、霍夫直线等）
- **流程控制** — 延时、条件判断、循环等流程控制节点
- **图执行引擎** — 基于 Kahn 拓扑排序的自动执行，支持环检测与错误处理
- **实时图像预览** — 右侧面板实时显示处理结果，支持缩放与对比模式
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
| PropertyChanged.Fody | - | MVVM 属性通知 |

## 项目结构

```
Shell.sln
├── Shell/                    # 主应用程序
│   ├── Models/              # 数据模型（节点、属性、工具箱）
│   │   └── Nodes/           # 各类节点 ViewModel
│   ├── Services/            # 服务层（执行引擎、序列化、注册表）
│   │   └── Algorithms/      # 视觉算法实现
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
