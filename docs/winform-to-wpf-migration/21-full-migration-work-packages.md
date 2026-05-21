# 全量迁移 Work Package 拆分表

更新时间：2026-05-21
状态：计划产物，等待用户确认；确认前禁止实现

本文件把 `10-full-migration-roadmap.md` 拆成后续可分配的 Work Package。每个 Work Package 执行前仍必须输出更细的实施计划、测试计划、文件 owner 和禁止扩大范围。

## 总规则

- 一个实现 worker 只负责一个 Work Package。
- 同一文件、同一 ViewModel、同一 Service、同一 Adapter 不允许多人并行修改。
- 所有实现必须 TDD：红灯测试、确认红灯、最小绿灯、目标测试、全量测试。
- 每个 Work Package 完成后必须输出 Spec Compliance Review 和 Code Quality Review。
- 高风险模块必须先 fake/simulator，再现场或真实环境验收。
- 未验证功能进入未验证清单，不能标记完成。

## Work Package 总表

| WP | Wave | 名称 | Owner 类型 | 主要目标文件范围 | 原 WinForms/Legacy 范围 | 前置门禁 | 完成证据 | 禁止扩大 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| WP-00 | Wave 0 | 资产和 UI 基线 | 只读盘点 | `docs\*` | `ATS\bin\Debug`、`ATS\*.cs` | 无 | `16-wave0-baseline-report.md` | 禁止改代码 |
| WP-01 | Wave 1 | Shell/导航/未迁移入口门禁 | Shell worker | `ATS5.Wpf\*`、`ATS5.Wpf.Core\*` | `FrmMain`、`FrmLogin` | WP-00 | Shell 测试、未迁移入口禁用测试 | 禁止启用业务入口 |
| WP-02 | Wave 1 | 路径/日志/配置基础设施 | Infrastructure worker | `ATS5.Infrastructure.LegacyAdapters\Runtime`、`Logging`、`DataQuery\LegacyAppConfigService.cs` | `ConfigHelper`、`LogHelper`、`ATS.exe.config`、`log4net.config` | WP-00 | 输出资产、日志、配置 smoke | 禁止替换日志框架 |
| WP-03 | Wave 2 | 数据查询真实数据样板 | DataQuery worker | `ATS5.Application\DataQuery`、`ATS5.Infrastructure.LegacyAdapters\DataQuery`、`ATS5.Modules.DataQuery`、`ATS5.Tests\DataQuery` | `ATS\DataQuery\UcDataQuery.cs` | WP-01/WP-02 | SQLite 查询、明细、手动 MES fake/adapter、UI 状态测试 | 禁止实现 GP12 页面 |
| WP-04 | Wave 2 | 流程脚本模式样板 | Flow worker | `ATS5.Application\Flow`、`ATS5.Infrastructure.LegacyAdapters\Flow`、`ATS5.Modules.Flow`、`ATS5.Tests\Flow` | `ATS\Flow\UcFlow.cs`、流程弹窗 | WP-01/WP-02 | `.fw` 多样本、保存测试副本、脚本校验、UI 状态测试 | 禁止实现工步模式 |
| WP-05 | Wave 2 | UI smoke 和样板验收报告 | QA worker | `ATS5.Tests\Infrastructure`、`docs\*` | Shell/DataQuery/Flow | WP-03/WP-04 | `19-wave2-sample-verification-report.md` | 禁止改业务实现 |
| WP-06 | Wave 3 | 权限管理 | Authority worker | `ATS5.Modules.Authority`、`ATS5.Application\Authority`、`ATS5.Tests\Authority` | `ATS\Authority\UcAuthority.cs`、`FrmUser.cs`、`FrmRole.cs` | Wave 2 完成 | 用户/角色/权限 CRUD 测试和 UI 对照 | 禁止改登录策略 |
| WP-07 | Wave 3 | 设备配置管理 | Device config worker | `ATS5.Modules.Device`、`ATS5.Application\DeviceConfig`、legacy `.dev` adapters | `ATS\Device\UcDevice.cs`、`FrmAllDev.cs`、`FrmDevSave.cs` | Wave 2 完成 | `.dev` WinForms/WPF 互读，设备顺序/启用态一致 | 禁止设备调试 Host |
| WP-08 | Wave 3 | 产品测试壳和扫码弹窗 | ProductTest UI worker | `ATS5.Modules.ProductTest` UI、ViewModel、fake service | `UcTestMain`、`UcProductTest`、`FrmBarcode`、`FrmFlowInfo` | Wave 2 完成 | `ChannelNum=2` UI、扫码焦点、复测弹窗、未执行测试链 | 禁止调用真实 `ProductTestCore.Start` |
| WP-09 | Wave 4 | 产品测试执行链 Adapter | ProductTest core worker | `ATS5.Application\ProductTest`、`ATS5.Infrastructure.LegacyAdapters\ProductTest` | `ATSCore\ProductTestCore`、`UcProductTest.StartTest` | WP-08 | 动态编译、设备初始化顺序、保存、自动导出验证 | 禁止并行改 Device/MES/AutoTest host |
| WP-10 | Wave 4 | GP12 MES | MES GP12 worker | `ATS5.Modules.Mes`、`ATS5.Application\Mes`、`ATS5.Infrastructure.LegacyAdapters\Mes` | `ATSMes\DB_GP12`、`ATSMes\UcMesMain.cs` | WP-09 计划确认 | GP12 登录、条码、工序校验、上传、状态/报警报告 | 禁止迁移其他 MES provider |
| WP-11 | Wave 5 | AutoType 9 双通道自动化 | AutoTest worker | `ATS5.Modules.AutoTest`、AutoType 9 adapter | `ATSAutoTest\DB-XMorZP22`、`AutoTestMain.cs` | WP-09/WP-10 | fake PLC/MES，`AutoTestResult`、`AutoTestMESResult` 顺序 | 禁止顺手迁移其他 AutoTest |
| WP-12 | Wave 5 | 设备调试 Host | Device debug worker | `ATS5.Modules.Device` debug host、device adapter | `ATS\Device\FrmDevDebug.cs`、`ATSDevice\*` | WP-07/WP-09 | 设备初始化/关闭/互斥、动态控件加载、现场或 simulator 报告 | 禁止重命名插件目录 |
| WP-13 | Wave 6 | DBC/UDS/CAN/日志导出/迁移工具 | Tools worker | `ATS5.Modules.Tools` | `ATS\Tool\*` | Wave 5 完成或单独确认低风险工具 | `.adbc/.xlsx`、导出文件、日志目录验证 | 禁止改设备/MES 核心 |
| WP-14 | Wave 6 | 其他 MES provider | MES provider workers | `ATS5.Modules.Mes` provider views/adapters | `ATSMes\*\UI\UcMain_*`、`Model\*` | GP12 模板完成 | 每个 provider 单项报告 | Host 单 owner |
| WP-15 | Wave 6 | 其他 AutoTest provider | AutoTest provider workers | `ATS5.Modules.AutoTest` provider views/adapters | `ATSAutoTest\*\UcMain_*` | AutoType 9 模板完成 | 每个 provider 单项报告 | Host 单 owner |
| WP-16 | Wave 6 | 发布包和关闭过渡入口 | Release/QA worker | WPF csproj、发布脚本、docs | 运行目录资产 | 全部已迁移 WP 验证完成 | Release 资产清单、全回归、未迁移/未验证/差异清单 | 禁止删除未验证入口 |

## 每个 Work Package 必须输出

- 原 WinForms 路径。
- WPF 目标路径。
- 控件/事件/命令映射。
- 数据读写对照。
- 日志记录对照。
- UI/交互对照。
- 设备/MES/自动化边界。
- 单元测试结果。
- 兼容测试结果。
- UI smoke 或人工验收表。
- Spec Compliance Review。
- Code Quality Review。

## 可并行建议

可以并行：

- WP-03 数据查询 与 WP-04 流程样板，前提是不修改同一基础 adapter。
- WP-06 权限 与 WP-07 设备配置，前提是不修改 Shell/登录。
- WP-13 工具模块中 DBC/UDS/CAN 可分子包，只要不共享同一文件。
- WP-14 不同 MES provider 只读盘点可并行；实现时 provider host 单 owner。
- WP-15 不同 AutoTest provider 只读盘点可并行；实现时 provider host 单 owner。

不建议并行：

- WP-09 产品测试执行链 与 WP-10 GP12 MES。
- WP-09 产品测试执行链 与 WP-12 设备调试 Host。
- WP-10 GP12 MES 与 WP-11 AutoType 9。

禁止并行：

- 同时修改 `SysCache` gateway/委托签名。
- 同时修改 `ProductTestCore` adapter 和 Device/MES/AutoTest host。
- 同时修改 `.fw` 序列化和产品测试流程执行。
- 同时修改日志 adapter 和日志导出目录规则。
- 同时修改配置 adapter 和 MES/AutoTest 配置键。

## 确认前状态

本文件只是 Work Package 拆分，不授权任何实现。确认前只能继续补文档、做只读盘点或调整计划。
