# Wave 0/1 实施拆分与测试计划

更新时间：2026-05-21
状态：全量计划已确认；本文件用于启动 Wave 0 只读基线和 Wave 1 基础设施稳定

## 边界

本轮只允许：

- Wave 0：只读资产基线、UI 验收缺口、当前 WPF 样板验证。
- Wave 1：基础设施缺口分析，随后按最小任务实现 Shell/路径/日志/配置/消息桥接等基础能力。

本轮禁止：

- 全量迁移 `ATSMes`、`ATSAutoTest`、`ATSDevice`。
- 改 `D:\CODE\ATE\01_code` 原 WinForms、业务、MES、设备、自动化源码。
- 改变 SQLite、gzip JSON、`.fw/.dev/.adbc/.xlsx`、`ATS.exe.config` 格式。
- 替换 log4net 或 `LogHelper`。
- 打开未迁移占位按钮为可用状态。

## Wave 0：只读基线任务

| 任务 | 文件范围 | 输出 | 验证 |
| --- | --- | --- | --- |
| W0-1 运行资产基线 | 只读 `D:\CODE\ATE\01_code\ATS\bin\Debug` | `16-wave0-baseline-report.md` | 统计 `ats.db`、gzip JSON、`.fw/.dev/.adbc/.xlsx`、设备 DLL/XML、配置、日志配置 |
| W0-2 UI 验收缺口 | 只读 `Shell.xaml`、`DataQueryView.xaml`、`FlowEditorView.xaml`、`14-ui-parity-sample-checklist.md` | `16-wave0-baseline-report.md` | 列出人工验收项、占位按钮、不能宣称完成项 |
| W0-3 当前 WPF 样板验证 | `ATS5.Wpf.sln` | `16-wave0-baseline-report.md` | `dotnet restore/build/test`，必要时启动探活 |
| W0-4 文档门禁更新 | `00-master-checklist.md`、`16-wave0-baseline-report.md` | 更新清单 | 明确 Wave 0 是否完成 |

Wave 0 不写业务代码。

## Wave 1：基础设施稳定任务

Wave 1 在 W0-1 到 W0-3 完成后开始实现。每个实现任务必须测试先行、最小改动、完成后两类评审。

| 任务 | 目标文件范围 | 目标 | 测试计划 | 禁止扩大 |
| --- | --- | --- | --- | --- |
| W1-1 输出资产清单检查 | `ATS5.Tests`、必要的 WPF csproj | 验证输出目录包含 `ATS.exe.config`、`log4net.config`、`AppDll`、SQLite native DLL、必要 legacy DLL | 新增输出资产测试，先失败后修复 | 不复制完整 `SysCache/AppData` 到源码，不污染原 Debug 数据 |
| W1-2 日志通道 smoke | `ATS5.Application`、`ATS5.Infrastructure.LegacyAdapters\Logging`、`ATS5.Tests` | 验证 `LogHelper.Init` 后 6 个日志通道可按原目录格式生成 | 用临时运行目录测试 `Info/Error/Test/Mes/Operate/CanTool` | 不替换 log4net，不改 `log4net.config` 格式 |
| W1-3 配置读取/写回兼容 smoke | `ATS5.Application`、`ATS5.Infrastructure.LegacyAdapters\DataQuery` 或新增 Config 目录、`ATS5.Tests` | 读取和写回临时 `ATS.exe.config` 测试键 | 用临时目录复制配置，写非敏感测试键，确认 XML 节点不丢 | 不写真实 Debug 配置，不改配置键 |
| W1-4 RuntimePathProvider 固化 | `ATS5.Infrastructure.LegacyAdapters\Runtime`、`ATS5.Wpf` | 明确 Debug 基线和 WPF 输出目录的运行根策略 | 单元测试路径解析和当前目录串行锁 | 不改变 legacy 相对路径语义 |
| W1-5 未迁移入口状态检查 | `Shell.xaml`、`ShellViewModel`、`ATS5.Tests` | 未迁移入口禁用/占位状态可测试 | ViewModel 或 UI 状态测试 | 不启用产品测试/MES/设备/自动化占位按钮 |

## 首个推荐实现任务

首个实现任务建议选 W1-1“输出资产清单检查”，原因：

- 风险低，不碰业务逻辑。
- 能立刻暴露 WPF 运行目录是否缺 `ATS.exe.config`、`log4net.config`、`AppDll`、SQLite native DLL、legacy DLL。
- 是后续数据、日志、流程编译、设备/MES 的共同前置门禁。

## 通用验证命令

每个实现小任务完成后执行：

```powershell
dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln
dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
```

若涉及 WPF 启动或输出资产，还需执行启动探活并记录输出目录检查结果。

## 评审要求

每个实现任务完成后必须输出：

- Spec Compliance Review：是否严格符合 WinForms 迁移计划，不扩大范围。
- Code Quality Review：是否符合 C#、WPF、MVVM、Prism、Adapter、测试和可维护性要求。

## Wave 1 完成门禁

- `dotnet restore/build/test` 全通过。
- 输出资产检查通过。
- 日志通道 smoke 通过。
- 配置读取/写回临时副本 smoke 通过。
- 未迁移入口保持禁用或明确占位。
- 总控清单更新。

Wave 1 完成前不进入 Wave 2。
