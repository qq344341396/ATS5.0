# WinForms 到 WPF 全量迁移计划

更新时间：2026-05-21
状态：Wave 2 已完成自动/半自动收口；Wave 3 WP-06/WP-07/WP-08 已完成代码侧验证并通过双评审；Wave 4 未确认，确认前禁止实施

## 确认口径

请按本文件确认“全量迁移路线、顺序、门禁和验证策略”，不是确认立即全仓开工。

确认本计划后，Controller 仍只能按 Wave 推进：

1. Wave 2 已完成 W2-1..W2-7 自动/半自动收口，后续需补未关闭的人工和高风险门禁。
2. 每个后续 Wave 开始前单独输出实施任务拆分、测试计划、文件 owner 和禁止扩大范围。
3. 每个 Wave 完成后输出验证报告、Spec Compliance Review 和 Code Quality Review。
4. 未满足当前 Wave 门禁时，只能补样板、补验证、补文档或做只读盘点，不能进入下一 Wave。

任何情况下，确认本计划都不等于允许一次性迁移 `ProductTest`、`ATSMes`、`ATSAutoTest`、`ATSDevice`。

## 目标和边界

本计划用于指导 `D:\CODE\ATE\01_code` 原 WinForms 系统向 `D:\CODE\ATE\ATS5.0` WPF 项目迁移。验收标准仍是原 WinForms 行为，不是 WPF “现代化”效果。

核心原则：

- 原 WinForms 行为是唯一验收标准。
- WPF、MVVM、Prism、HandyControl 只是承载手段。
- 盘点必须并行，设计必须集中，样板必须端到端，全量迁移必须过门禁。
- 计划确认前不实施全量迁移，不打开占位按钮为可用，不全量迁移 `ATSMes`、`ATSAutoTest`、`ATSDevice`。
- 不修改原业务保存格式、日志框架、设备时序、MES 上报时机、自动化回调顺序。

## 当前门禁状态

| 门禁项 | 当前状态 | 结论 |
| --- | --- | --- |
| 第一阶段现状盘点 | 已完成，文档在 `D:\CODE\ATE\01_code\docs\winform-to-wpf-migration` | 可作为计划输入 |
| 第二阶段 WPF/Prism 架构设计 | 已完成 | 设计集中到 Controller 统一裁决 |
| 第三阶段流程编辑扩展接口设计 | 已完成，只预留，不实现工步模式 | 脚本模式仍为首发 |
| 第四/五阶段样板选择与计划 | 已完成 | 两个样板已确认 |
| 第六阶段样板首版 | 已完成构建、17 个服务级测试、启动探活；Wave 2 已补 W2-1..W2-7 自动/半自动证据 | 可作为架构模板，不能当作全量完成证据 |
| UI 样板方向 | 主界面、数据查询、流程编辑结构已按用户反馈二次调整；Wave 2 已完成结构、ViewModel/XAML 状态和 smoke 验证 | 仍需人工点击导航/UI 签核 |
| Wave 0/1 | 已完成 Debug 基础设施门禁 | 可作为后续 Wave 前置条件 |
| Wave 2 样板补齐收口 | W2-1..W2-7 已完成并双评审；证据见 `23`..`28` 和 `19-wave2-sample-verification-report.md` | 自动/半自动门禁完成，人工和高风险门禁仍未关闭 |
| Wave 3 执行 | `29-wave3-execution-plan.md` 已同步执行结果 | WP-06/WP-07/WP-08 代码侧完成并通过双评审；人工点击验收仍待关闭 |
| 全量迁移计划 | 本文档更新 | 仍需按 Wave 单独确认 |
| 全量实现 | 未开始 | 禁止在确认前执行 |

全量实现启动条件：

1. 用户明确确认本文计划。
2. Wave 2 自动/半自动证据已完成，但人工点击导航/UI 签核、可读真实 legacy gzip JSON 明细样本、GP12、AutoType 9、设备在线/调试、正式流程覆盖保存、全量迁移确认仍需单独关闭。
3. 当前样板继续保留端到端验证证据，不扩大为未验证的“已迁移”状态。
4. 每个功能进入实现前都有原路径、目标路径、事件映射、验证点和禁止并行范围。
5. 每个实现任务完成后都经过 Spec Compliance Review 和 Code Quality Review。
6. 高风险模块 `ProductTest`、`ATSMes`、`ATSAutoTest`、`ATSDevice` 必须逐 Wave 单独确认，不允许一次性并行铺开。

## 全量实施控制摘要

全量迁移采用“并行盘点、集中设计、样板端到端、全量过门禁”的控制方式。

| 控制点 | 要求 | 产物 | 未满足时允许动作 |
| --- | --- | --- | --- |
| 并行盘点 | UI、Core、Data、Logging、MES/Device/AutoTest、Process Editor 分开只读盘点 | 页面级 parity checklist、调用链、风险清单 | 只能继续盘点和补文档 |
| 集中设计 | Controller 统一裁决 Prism、Module、Adapter、数据兼容、日志、设备/MES/自动化边界 | 架构约束、模块边界、禁止并行清单 | 只能调整设计，不实现 |
| 样板端到端 | 数据查询和流程编辑两个样板已完成 W2-1..W2-7 自动/半自动验证；人工 UI 和真实现场/高风险门禁继续单独补充 | Wave 2 验证报告和 W2-1..W2-7 报告 | 只能补未关闭门禁或确认下一 Wave 计划，不能进入全量 |
| 全量门禁 | 每个 Wave 开始前有实施计划，结束后有验证报告和两类评审 | Wave 计划、测试结果、评审记录 | 不能进入下一 Wave |
| 高风险确认 | 产品测试、GP12、AutoType 9、设备调试单独确认 | 专项计划和验收清单 | 只能做只读盘点和 fake/simulator 验证 |

确认本文档只代表允许按 Wave 机制继续，不代表允许跳过未关闭门禁或一次性迁移所有模块。

## 并行盘点规则

全量迁移期间所有新增盘点仍按并行只读 agent 执行，不能在盘点阶段改代码。

| 盘点 Agent | 范围 | 输出 |
| --- | --- | --- |
| UI/WinForms Agent | `ATS`、`ATSMes`、`ATSAutoTest` 所有窗体、控件、菜单、弹窗、快捷键、右键菜单 | 页面级 parity checklist |
| Core/Business Agent | `ATSCore`、`ATSCommon`、`ATSModel` 调用链和跨层耦合 | 用例调用链和风险 |
| Data/Storage Agent | SQLite、gzip JSON、`.fw/.dev/.adbc/.xlsx`、`ATS.exe.config`、编码和保存时机 | 数据兼容矩阵 |
| Logging Agent | `log4net`、`LogHelper`、目录、格式、等级、导出 | 日志验证矩阵 |
| MES/Device/AutoTest Agent | `ATSMes`、`ATSDevice`、`ATSAutoTest`、`SysCache` 委托、双通道时序 | 边界和时序矩阵 |
| Process Editor Agent | `UcFlow`、`.fw`、脚本动态编译、未来工步扩展点 | 流程编辑迁移清单 |

## 集中设计规则

所有设计决策由 Controller 集中维护到文档和架构约束中，不允许实现 agent 自行改变架构。

集中裁决范围：

- WPF Solution/Project 划分。
- Prism Module、Region、Navigation、DialogService、EventAggregator、DI 生命周期。
- Legacy Adapter 边界。
- `SysCache`、`ProductTestCore`、`LogHelper`、`ConfigHelper`、DBCore 包装策略。
- `.fw/.dev/.adbc/.xlsx`、SQLite、gzip JSON、`ATS.exe.config` 兼容策略。
- MES、设备、自动化委托和时序。
- 流程编辑脚本模式和未来工步模式扩展边界。

实现 agent 不得自行：

- 修改 legacy 委托签名。
- 改变保存时机。
- 替换 log4net。
- 重写设备插件协议。
- 将样板占位功能标记为已完成。

## 端到端样板要求

已确认样板：

1. 数据查询 + 手动 MES 上传。
2. 流程管理脚本模式打开、保存测试副本、编译校验。

Wave 2 已完成的自动/半自动证据：

- W2-1 阻塞项处理报告：`23-wave2-w2-1-blocker-report.md`。
- W2-2 数据查询 ViewModel/UI 报告：`24-wave2-w2-2-dataquery-viewmodel-ui-report.md`。
- W2-3 手动 MES 上传报告：`25-wave2-w2-3-manual-mes-upload-report.md`。
- W2-4 流程 `.fw` 兼容报告：`26-wave2-w2-4-flow-fw-compatibility-report.md`。
- W2-5 流程保存/脚本校验报告：`27-wave2-w2-5-flow-save-script-validation-report.md`。
- W2-6 流程 ViewModel/XAML 状态报告：`28-wave2-w2-6-flow-viewmodel-xaml-state-report.md`。
- W2-7 汇总验证报告：`19-wave2-sample-verification-report.md`。
- 代码侧/自动门禁：target smoke/structure tests 10/10、full tests 83/83、build 0 warnings/errors、startup probe passed。

仍需继续补齐：

- 人工点击导航/UI 签核：按钮、右键、弹窗、禁用态、列名、布局。
- 可读真实 legacy gzip JSON 明细样本对账。
- GP12 可用环境下的真实/模拟上传专项报告。
- AutoType 9 双通道自动化专项验证。
- 设备在线/调试 Host 联机或 simulator 验证。
- 正式流程覆盖保存验证。
- 全量迁移确认。

上述门禁关闭前，允许补验证、补文档或确认下一 Wave 的计划边界，不允许宣称全量门禁已完全通过。

## 目标 WPF 模块图

| WPF 项目/模块 | 目标职责 | 对应 WinForms/Legacy |
| --- | --- | --- |
| `ATS5.Wpf` | Prism Shell、启动、导航、主题、登录门禁、状态栏 | `ATS\Program.cs`、`FrmLogin`、`FrmMain` |
| `ATS5.Wpf.Core` | Region 常量、Dialog 抽象、基础 ViewModel、UI 事件 | WinForms 公共 UI 行为 |
| `ATS5.Application` | 用例服务、DTO、领域编排接口 | 原 UI 事件中的业务编排 |
| `ATS5.Infrastructure.LegacyAdapters` | 包装 `SysCache`、`ProductTestCore`、DBCore、`LogHelper`、`ConfigHelper`、`JsonHelper` | `ATSCore`、`ATSCommon`、`ATSModel` |
| `ATS5.Modules.ProductTest` | 产品测试壳、通道页、扫码、测试状态和结果显示 | `UcTestMain`、`UcProductTest`、`FrmBarcode` |
| `ATS5.Modules.DataQuery` | 数据查询、明细、导出、手动 MES 上传 | `UcDataQuery` |
| `ATS5.Modules.Flow` | 流程脚本模式 UI、流程弹窗、校验、保存 | `UcFlow`、流程相关 `Frm*` |
| `ATS5.Modules.Device` | 设备库、设备配置、设备调试外壳 | `UcDevice`、`FrmDevDebug` |
| `ATS5.Modules.Authority` | 用户、角色、权限 | `UcAuthority`、`FrmUser`、`FrmRole` |
| `ATS5.Modules.Mes` | MES Host、GP12 和其他客户 MES 配置/日志 | `ATSMes\UcMesMain`、各 `UcMain_*` |
| `ATS5.Modules.AutoTest` | 自动化 Host、AutoType 9 双通道自动化 | `ATSAutoTest\AutoTestMain`、各自动化控件 |
| `ATS5.Modules.Tools` | DBC、UDS、CAN、日志导出、数据库迁移、第三方工具入口 | `ATS\Tool\*`、第三方工具目录 |
| `ATS5.Tests` | 单元、兼容、服务级、UI smoke 测试 | 新测试工程 |

## 全功能迁移矩阵

| 顺序 | 功能 | 原 WinForms 路径 | WPF 目标 | 风险 | 数据验证 | 日志验证 | UI/交互验证 | 设备/MES/自动化验证 | 完成标准 | 可分配 subagent 范围 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | 迁移门禁和资产基线 | `docs`、`ATS\bin\Debug` | `docs`、测试基线 | 高 | 统计 `ats.db`、gzip JSON、`.fw/.dev/.adbc/.xlsx` | 记录 log4net 配置快照 | 建立页面验收表 | 记录 GP12、AutoType 9、ChannelNum 2 目标 | 基线清单完成，确认后才实现 | 只读盘点 agent |
| 1 | Shell/启动/登录 | `Program.cs`、`FrmLogin`、`FrmMain` | `ATS5.Wpf`、`LoginView`、`Shell` | 高 | `ATS.exe.config` 用户名、皮肤、语言读取 | `LogHelper.Init` 保持 | Ribbon/TileBar/状态栏/用户菜单复刻 | 不注册设备/MES/AutoTest 业务委托 | 启动、登录、注销、退出确认、菜单禁用态一致 | Shell worker |
| 2 | 配置/路径/日志基础设施 | `ConfigHelper`、`SysCache`、`LogHelper` | Legacy adapters | 高 | 相对路径、XML 节点、敏感配置不丢 | `Log/*` 目录和格式一致 | 配置保存按钮触发，不实时保存 | 不改变委托签名 | 读写旧配置、日志生成、输出清单通过 | Infrastructure worker |
| 3 | 数据兼容测试基座 | `SQLiteHelper`、`JsonHelper`、DBCore | Repository/Storage tests | 高 | SQLite/WAL、gzip JSON、普通 JSON、Excel/CSV 对账 | 错误日志分类 | 无 UI 或测试 UI | 不触发真实设备/MES | 可在临时目录跑兼容测试，不污染原数据 | Data worker |
| 4 | 数据查询完整迁移 | `ATS\DataQuery\UcDataQuery.cs` | `DataQueryView` | 中高 | `IndexInfo*`、明细、导出、分页一致 | 查询/导出/MES 异常日志 | 左索引右明细、左下统计、右键、展开/收合 | 手动 MES 只走 `SysCache.UploadData` | 与 WinForms 查询和导出一致 | DataQuery worker |
| 5 | 流程编辑脚本模式完整迁移 | `ATS\Flow\UcFlow.cs`、流程弹窗 | `FlowEditorView`、dialogs | 极高 | `.fw` 字段、脚本、项目顺序、备份、`.zfw` | `Operate` 日志 | 工具条、项目树、脚本、输出/临时变量、DBC/UDS、设备控制 | 只预检，不启动设备/MES/AutoTest | 代表流程可打开、编辑、校验、保存副本、WinForms 反开 | Flow worker |
| 6 | 权限管理 | `Authority\UcAuthority.cs`、`FrmUser`、`FrmRole` | `AuthorityView`、dialogs | 中 | `User`、`Role` 表 CRUD | 操作/异常日志 | 角色用户联动、权限勾选、删除确认 | 无 | 用户/角色/权限读写与 WinForms 一致 | Authority worker |
| 7 | 设备配置管理 | `Device\UcDevice.cs`、`FrmAllDev`、`FrmDevSave` | `DeviceManagerView` | 极高 | `.dev` 读写、设备顺序、启用态 | `Operate/Error` | 设备库树、搜索、导入、配置表、保存/另存 | 只做配置管理，不全量调试设备 | `.dev` WinForms/WPF 互读，排序和启用态一致 | Device config worker |
| 8 | 产品测试壳和扫码弹窗 | `UcTestMain`、`UcProductTest`、`FrmBarcode` | `ProductTestShellView`、`ProductTestChannelView`、`BarcodeDialog` | 极高 | 流程名、条码、通道状态不落库或用测试副本 | Test/Error 日志 | 双通道 Tab、按钮、扫码焦点、复测弹窗 | 注册 AutoTest 字典但先用 fake | `ChannelNum=2` UI 和启动前链路一致 | ProductTest UI worker |
| 9 | 产品测试执行链 | `ProductTestCore.cs`、`UcProductTest.StartTest` | `ProductTestSessionService`、adapter | 极高 | `IndexInfo`、`ProcessInfo`、gzip JSON、自动导出一致 | Test/Error/Operate | 开始/暂停/停止/单步/清日志/统计 | 设备初始化、关闭、锁、复用顺序不变 | 单通道和双通道端到端测试完成 | ProductTest core worker |
| 10 | GP12 MES 优先迁移 | `ATSMes\UcMesMain.cs`、`DB_GP12` | `MesHostView`、`Gp12MesView` | 极高 | 读取本地结果后上传，`UploadMesStatus` 一致 | `Log/MES`、`Log/Error` | MES 配置、启用、保存、日志表 | `LoginMes`、`SendChannelBarcode`、`StationCheck`、`UploadData`、状态/报警 | GP12 手动和自动上传闭环通过 | MES GP12 worker |
| 11 | AutoType 9 双通道自动化 | `ATSAutoTest\AutoTestMain.cs`、`DB-XMorZP22` | `AutoTestHostView`、AutoType9 view | 极高 | 流程名、条码、结果不改格式 | Test/Error | 自动化状态、连接、启动/停止、报警 | `AutoTestStartTestDic`、`AutoTestResult`、`AutoTestMESResult` 顺序不变 | fake PLC/MES 和现场验收通过 | AutoTest worker |
| 12 | 设备调试和插件 Host | `FrmDevDebug`、`ATSDevice\*` | `DeviceDebugView`、legacy host/adapter | 极高 | `.dev`、设备 XML/DLL 引用一致 | 设备 Info/Error | 调试树、动态控件、关闭释放 | `DeviceCore`、`DevicePool`、`BaseDevice` 生命周期不变 | 实际设备或模拟器初始化/关闭顺序通过 | Device debug worker |
| 13 | 工具模块 | `Tool\DBC`、`UDS`、`CAN`、日志导出、DB 迁移、第三方工具 | `Tools` module | 中高 | `.adbc/.xlsx`、导出文件、日志目录 | CanTool/Operate/Error | 工具菜单动态扫描、弹窗、文件选择 | CAN 工具按原行为；CAN 自检低优先级 | 常用工具可用，低优先工具保留入口状态 | Tools worker |
| 14 | 其他 MES providers | `ATSMes\*\UI\UcMain_*` | Provider views/adapters | 极高 | 各配置键和上传字段一致 | MES/Error | 每个客户页面保存/取消/日志 | 各委托返回 `MesRes` 语义一致 | 按实际使用优先级逐个验收 | MES provider workers |
| 15 | 其他 AutoTest providers | `ATSAutoTest\*\UcMain_*` | Provider views/adapters | 极高 | 条码、流程、结果回调一致 | Test/Error | PLC/扫码/报警/状态 UI | 不改变轮询、Sleep、状态码 | 按实际使用优先级逐个验收 | AutoTest provider workers |
| 16 | 统计信息和低优先入口 | `barStatisticalInfo`、CAN 自检等 | `StatisticalInfoView`、Tool entries | 中 | 按代码确认 | 按原分类 | 入口、占位、隐藏规则 | 以代码为准 | 未使用项可低优先但不删除 | Low-priority UI worker |
| 17 | 清理过渡层和发布验收 | WinFormsHost、占位入口 | Release package | 高 | 发布包资产完整 | 全日志通道生成 | 所有菜单可用/禁用态明确 | 现场端到端回归 | 无未确认占位被误标完成 | Release/QA worker |

## 迁移顺序

## Wave 执行总表

| Wave | 实施范围 | 前置门禁 | 本 Wave 禁止范围 | 完成证据 | 是否可并行 |
| --- | --- | --- | --- | --- | --- |
| Wave 0 | 只读资产基线、UI 缺口、样板当前状态 | 用户确认开始盘点 | 禁止业务代码修改 | `16-wave0-baseline-report.md` | 可并行只读 |
| Wave 1 | Shell/路径/日志/配置/输出资产/未迁移入口状态 | Wave 0 完成 | 禁止产品测试、MES、设备、自动化实现 | `17-wave1-infrastructure-report.md`、23 个测试通过 | 小范围单 owner |
| Wave 2 | 数据查询和流程编辑样板端到端补齐 | Wave 1 完成，用户确认 `18-wave2...` | 禁止启用产品测试/MES/设备/自动化入口 | `19-wave2-sample-verification-report.md`、`23`..`28` W2 报告、10/10 target smoke/structure、83/83 full tests、0 warning/error build、startup probe | 已完成自动/半自动收口；遗留人工和高风险门禁单独补 |
| Wave 3 | 权限、设备配置、产品测试壳和扫码弹窗 | Wave 2 自动/半自动收口完成，`29-wave3-execution-plan.md` 经用户确认；遗留门禁不得被视为 Wave 3 授权 | 禁止产品测试执行、真实设备调试、GP12 自动上传 | WP-06/WP-07/WP-08 报告，权限/.dev/双通道 UI 代码侧验证通过 | 已完成代码侧验证和双评审；人工点击验收仍待关闭 |
| Wave 4 | 产品测试执行链、GP12 MES | Wave 3 完成，单独确认 GP12 | 禁止 AutoType 9 和设备调试全量铺开 | 产品测试数据保存、GP12 手动/自动上传报告 | 不建议并行核心链路 |
| Wave 5 | AutoType 9 双通道自动化、设备调试 Host | Wave 4 完成，现场或 simulator 条件确认 | 禁止其他 provider 顺手迁移 | AutoType 9 回调顺序、设备初始化/关闭报告 | AutoType 9 与设备调试可分 owner，但共享接口单 owner |
| Wave 6 | 工具、其他 MES/AutoTest provider、发布包 | Wave 5 完成 | 禁止删除未验证入口 | 发布资产清单、provider 单项报告、全回归 | provider 可并行，Host 单 owner |

### Wave 0：计划确认和只读基线

- 更新总控清单、功能清单、风险清单。
- 用只读脚本统计 Debug 资产：`ats.db`、gzip JSON、`.fw/.dev/.adbc/.xlsx`。
- 输出页面级 parity checklist。
- 不写业务代码。

门禁：

- 本文计划经用户确认。
- 资产清单和并行盘点报告入文档。

### Wave 1：基础设施稳定

- Prism Shell、Region、Navigation、Dialog、EventAggregator、DI。
- `IRuntimePathProvider`、`ILogService`、`IAppConfigService`、`ILegacySysCacheGateway`、`ILegacyMessageBridge`。
- 运行目录、`ATS.exe.config`、`log4net.config`、`AppDll`、SQLite native DLL 输出策略。

门禁：

- `dotnet restore/build/test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln` 通过。
- `Log/Info/Error/Test/MES/Operate/Cantool` 可按原格式生成。
- 未迁移菜单保持禁用或明确占位。

### Wave 2：数据和流程样板补齐

已完成：

- 数据查询 ViewModel/UI、手动 MES 上传、target smoke/structure。
- 流程 `.fw` 兼容、保存/脚本校验、ViewModel/XAML 状态。
- W2-1..W2-7 均已输出报告并双评审。
- target smoke/structure tests 10/10、full tests 83/83、build 0 warnings/errors、startup probe passed。

门禁：

- 自动/半自动门禁已通过。
- 人工点击导航/UI 签核、可读真实 legacy gzip JSON 明细样本、GP12、AutoType 9、设备在线/调试、正式流程覆盖保存、全量迁移确认仍需单独关闭。

### Wave 3：权限、设备配置、产品测试壳

- 权限管理 CRUD。
- 设备配置管理，不先全量设备调试。
- 产品测试双通道 UI 和扫码弹窗。
- 已按 `29-wave3-execution-plan.md` 完成 WP-06/WP-07/WP-08 代码侧实现、验证和双评审；人工点击验收仍待关闭。

门禁：

- 权限数据库写回一致。
- `.dev` 读写互通。
- `ChannelNum=2` 下 Tab、状态、AutoTest 字典注册不串通道。
- 未进入产品测试执行链、真实设备调试、GP12 自动上传、AutoType 9 或 ATSMes/ATSAutoTest/ATSDevice 全量迁移。

### Wave 4：产品测试执行和 GP12 MES

- `ProductTestCore` adapter 保持动态编译、设备初始化、项目循环、保存、自动导出。
- GP12 MES Host 和配置/上传边界。
- 自动上传与手动上传统一 `SysCache.UploadData`。

门禁：

- 本地保存先于 MES 上传。
- `UploadMesStatus` 更新语义一致。
- GP12 `AutoType=9` 不误套 `AutoType=4` 的过滤逻辑。
- 失败/停止/异常路径有日志和 UI 提示。

### Wave 5：AutoType 9 和设备调试

- AutoType 9 双通道自动化。
- fake PLC/MES 验证扫码、预检、启动、停止、结果、MES 结果回调。
- 设备调试 Host、实际设备或模拟器验证初始化/关闭。

门禁：

- `AutoTestResult` 和 `AutoTestMESResult` 顺序不变。
- `DevicePool` 锁、全局复用、关闭顺序不变。
- PLC/扫码枪/上位机通信节奏不被 async 重写改变。

### Wave 6：工具、其他 provider、发布

- DBC/UDS/CAN/日志导出/DB 迁移/第三方工具。
- 其他 MES provider。
- 其他 AutoTest provider。
- 发布包、升级包、运行目录资产完整性。

门禁：

- 每个 provider 单独验收。
- 未使用或低优先功能保留入口状态，不删除。
- 全回归通过后才移除过渡 WinFormsHost 或占位。

## 并行实现规则

可以并行：

- UI parity checklist 补充与数据兼容测试补充。
- 权限模块与工具入口迁移。
- 不同 MES provider 的只读盘点。
- 不同 AutoTest provider 的只读盘点。
- 测试用例编写和 UI 实现，前提是目标文件不重叠。

禁止并行：

- `SysCache` gateway 与 ProductTest/MES/AutoTest 同时改委托签名。
- `ProductTestCoreAdapter` 与 DevicePool/设备插件运行逻辑同时改。
- 数据查询手动上传与产品测试自动上传同时改 `UploadData` 语义。
- 流程编辑保存逻辑与产品测试流程执行逻辑同时改 `.fw` 序列化。
- `LogHelper` adapter 与日志导出同时改目录/格式。
- `ConfigHelper` adapter 与 MES/AutoTest 配置页面同时改配置键名。
- 同一 XAML/ViewModel/Service 文件不能由多个实现 agent 同时修改。

## 禁止并行修改的核心文件/模块

| 文件/模块 | 规则 |
| --- | --- |
| `ATS5.Infrastructure.LegacyAdapters\SysCache*` | 单 owner，任何签名变更必须 Controller 批准 |
| `ATS5.Application\ProductTest`、`ProductTestCoreAdapter` | 单 owner，设备/MES/AutoTest 不得并行改 |
| `ATS5.Application\Flow`、`LegacyFlowRepository`、`LegacyFlowSerializer` | 单 owner，保存格式不允许并行改 |
| `ATS5.Application\DataQuery`、`ManualMesUploadService` | 单 owner，上传语义与 MES owner 协调 |
| `ATS5.Infrastructure.LegacyAdapters\Logging` | 单 owner，不改 logger 名和目录 |
| `ATS5.Infrastructure.LegacyAdapters\Config` | 单 owner，不改配置键 |
| `ATS5.Modules.Mes` provider host | GP12 owner 与其他 provider owner 分开，但 host 单 owner |
| `ATS5.Modules.AutoTest` provider host | AutoType 9 owner 与其他 provider owner 分开，但 host 单 owner |
| `ATS5.Modules.Device` | 配置管理与调试 Host 分阶段，不能同文件并行 |

## 验证策略

### 构建验证

每个实现批次至少执行：

- `dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln`
- `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore`
- `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore`

必要时补充 legacy 项目构建顺序：

1. `D:\CODE\ATE\01_code\ATSModel\ATSModel.csproj`
2. `D:\CODE\ATE\01_code\ATSCommon\ATSCommon.csproj`
3. `D:\CODE\ATE\01_code\ATSCore\ATSCore.csproj`
4. `D:\CODE\ATE\01_code\ATSDevice\DeviceHelper\DeviceHelper.csproj`
5. `D:\CODE\ATE\01_code\ATSMes\ATSMes.csproj`
6. `D:\CODE\ATE\01_code\ATSAutoTest\ATSAutoTest.csproj`
7. `D:\CODE\ATE\01_code\ATS\ATS.csproj`

已知：原 `ATS5.0.sln` 异常已忽略，`ATS.csproj.user` 无效 XML 已记录。

### 功能一致性验证

每个页面必须逐项对照：

- 菜单、按钮、快捷键、右键菜单。
- 默认值、启用/禁用、可见/隐藏。
- 弹窗文案、按钮、默认按钮、阻塞时机。
- 操作顺序、保存时机、刷新时机。
- 权限控制。
- 表格列、排序、筛选、选择行为、分页。

### 数据兼容验证

必须覆盖：

- 旧 WinForms 数据 WPF 可读。
- WPF 保存 legacy 文件 WinForms 可读。
- SQLite `IndexInfo*`、`User`、`Role` 查询一致。
- gzip JSON 明细路径和内容一致。
- `.fw/.dev/.adbc/.xlsx` 字段、编码、文件名、目录不变。
- `ATS.exe.config` XML 结构和配置键不变。
- Excel/CSV 导出格式不漂移。

### 日志验证

必须覆盖：

- 仍使用 log4net 和 `LogHelper`。
- 输出目录：`Log/Info`、`Log/Error`、`Log/Test`、`Log/MES`、`Log/Operate`、`Log/Cantool`。
- 格式：`%d [%t] %-5p - %m%n`。
- 关键操作日志无遗漏。
- 异常不吞、不新增敏感信息泄露。
- 日志导出仍按原目录规则工作。

### 设备/MES/自动化验证

必须覆盖：

- `SysCache` 委托签名和注册顺序。
- GP12 `LoginMes`、`SendChannelBarcode`、`StationCheck`、`UploadData`、设备状态/报警上传。
- 本地保存先于 MES 上传。
- `AutoType=9`、`ChannelNum=2` 双通道自动化启动、停止、结果、MES 结果回调。
- `DeviceCore`、`DevicePool`、`BaseDevice` 初始化、锁、复用、关闭顺序。
- PLC、扫码枪、上位机、设备断线/超时场景。

### UI 自动化和人工验收

自动化建议：

- ViewModel 单元测试。
- Service 单元测试。
- Repository/Storage 兼容测试。
- 数据保存兼容测试。
- UI smoke：启动、登录、导航、数据查询、流程打开、配置保存。
- 关键端到端：数据查询手动 MES、流程保存副本、产品测试双通道、GP12 上传、AutoType 9。

人工验收：

- 用户不强制每项截图，但每个页面必须有验收表。
- 高风险页面建议保留截图或录屏：流程编辑、产品测试、MES、自动化、设备调试。

## 回滚和兼容策略

- 每个功能默认先复用 legacy adapter，不直接重写业务算法。
- 写入测试默认使用临时目录或副本，禁止污染 `D:\CODE\ATE\01_code\ATS\bin\Debug` 原始样例。
- `.fw` 正式覆盖前必须有备份和 WinForms 反开验证。
- MES、AutoTest、Device 先使用 fake/simulator 验证，再进现场。
- 未完成页面保持禁用占位或 legacy host，不能伪装成已完成。
- 若 WPF 行为与 WinForms 不一致，优先回退到旧 adapter 或临时 legacy host，而不是改变验收标准。

## 全量完成标准

全量迁移完成必须同时满足：

- 所有 WinForms 入口均有 WPF 目标、完成状态和验收证据。
- 所有已迁移功能通过构建、测试、UI 对照、数据对照、日志对照。
- GP12、AutoType 9、ChannelNum 2 端到端通过。
- `ATSMes`、`ATSAutoTest`、`ATSDevice` 的已迁移 provider 均有单独报告。
- 未迁移功能清单、未验证功能清单、有差异功能清单均更新。
- 用户确认可以关闭对应 WinForms 入口或过渡 host。

## 实际代码入口附录

本附录用于防止全量迁移时遗漏实际目录。每个入口进入实现前仍必须补页面级或 provider 级实施计划。

### ATS WinForms 页面和工具入口

| 类别 | 原路径 | 全量迁移目标 |
| --- | --- | --- |
| 主框架/登录/注册/指纹 | `D:\CODE\ATE\01_code\ATS\FrmMain.cs`、`FrmLogin.cs`、`FrmRegister.cs`、`FrmFingerprintLogin.cs`、`FrmFingerprintManagement.cs` | `ATS5.Wpf` Shell/Login/Register/Fingerprint dialogs |
| 公共弹窗 | `FrmBase.cs`、`FrmAlarm.cs`、`FrmWait.cs`、`FrmReTest.cs` | WPF DialogService/HandyControl Dialog，文案和阻塞时机对齐 |
| 数据查询 | `D:\CODE\ATE\01_code\ATS\DataQuery\UcDataQuery.cs` | `ATS5.Modules.DataQuery` |
| 流程编辑 | `D:\CODE\ATE\01_code\ATS\Flow\UcFlow.cs`、`FrmAllFlow.cs`、`FrmFlowSave.cs`、`FrmAddProject.cs`、`FrmEditGlobalVar.cs`、`FrmInputPars.cs`、`FrmRefer.cs`、`FrmDemoScript.cs` | `ATS5.Modules.Flow`，脚本模式优先，工步模式只预留接口 |
| 设备配置/调试 | `D:\CODE\ATE\01_code\ATS\Device\UcDevice.cs`、`FrmAllDev.cs`、`FrmDevSave.cs`、`FrmDevDebug.cs` | `ATS5.Modules.Device`，先配置管理，后调试 Host |
| 权限 | `D:\CODE\ATE\01_code\ATS\Authority\UcAuthority.cs`、`FrmUser.cs`、`FrmRole.cs` | `ATS5.Modules.Authority` |
| 产品测试 | `D:\CODE\ATE\01_code\ATS\ProductTest\UcTestMain.cs`、`UcProductTest.cs`、`FrmBarcode.cs`、`FrmFlowInfo.cs` | `ATS5.Modules.ProductTest` |
| 系统管理 | `D:\CODE\ATE\01_code\ATS\SysManage\FrmSysConfig.cs`、`FrmChangePW.cs`、`FrmAbout.cs` | Shell/System dialogs |
| DBC 工具 | `D:\CODE\ATE\01_code\ATS\Tool\DBCTool\*` | `ATS5.Modules.Tools` |
| UDS 工具 | `D:\CODE\ATE\01_code\ATS\Tool\UDSTool\*` | `ATS5.Modules.Tools` |
| CAN 工具/CAN Monitor | `D:\CODE\ATE\01_code\ATS\Tool\CANTool\UI\*`、`Tool\CANMonitor\*` | `ATS5.Modules.Tools`，CAN 自检低优先但保留入口 |
| 日志导出/升级/数据库迁移 | `Tool\ExportLog\FrmExportLog.cs`、`Tool\FrmUpgradeTool.cs`、`Tool\FrmMigrateDBTool.cs` | `ATS5.Modules.Tools` |

### ATSMes provider 入口

全量计划覆盖以下实际 provider 目录：

- `BL_EVB`
- `BL_EVB_Ex`
- `BS`
- `DB_BDU`
- `DB_EVB`
- `DB_GP12`
- `DFD`
- `DisableMes`
- `DQ`
- `GCXJ`
- `HN`
- `HY`
- `KST`
- `NC_EVB`
- `NC_EVB_Ex`
- `NJ_EVB`
- `SR`
- `YFMS`
- `ZHNY`

迁移规则：

- `DB_GP12` 优先，但必须按 Wave 4 单独确认。
- 每个 provider 的 `UI\UcMain_*`、`Model\*`、配置键、协议入参、返回处理、日志和上传时机单独验收。
- `DisableMes` 保留原禁用语义，不删除。
- provider host 单 owner，禁止多个 worker 同时修改 host 委托注册。

### ATSAutoTest provider 入口

全量计划覆盖以下实际 provider 目录：

- `BL`
- `BS`
- `DB`
- `DB-BDU`
- `DB-DCDC-EOL`
- `DB-GP12`
- `DB-MX068`
- `DB-PDU`
- `DB-Print`
- `DB-XMorZP22`
- `Deafault`
- `HY`
- `NC`
- `ZHNY48200`

迁移规则：

- `DB-XMorZP22` 的 AutoType 9 双通道优先，但必须按 Wave 5 单独确认。
- 每个 provider 的 UI、PLC/扫码/报警/状态轮询、`AutoTestStartTestDic`、`AutoTestResult`、`AutoTestMESResult` 调用顺序单独验收。
- 目录名 `Deafault` 按原拼写保留兼容，不擅自改名。

### ATSDevice 插件入口

全量计划覆盖 `D:\CODE\ATE\01_code\ATSDevice` 下所有设备插件目录。当前实际目录包括但不限于：

- 采集类：`AcquisitionCard_ZS18CH`、`AnalogAcquisition_*`、`DataAcq_*`
- 扫码/打印：`CodeScan`、`CodeScan_HF811`、`CodeScan_PJVT66`、`BarcodePrinter_Zebra_ZT231`
- 电源/负载/电阻：`DCSouce*`、`DCSource_*`、`ELoadBoard_*`、`Resistor_*`
- 安规/泄漏/等电位/内阻/万用表：`SafetyAnalyzer_*`、`SafeAnalyzer_*`、`Leak_*`、`EquipotentialMeter_*`、`InRes_*`、`Multimeter_*`
- 通信/板卡/PLC：`CAN_BMS`、`VBT_ZY`、`PLC_LK3U`、`IO_*`、`LIN`、`SENT`、`Switch_ZY`
- 其他专用设备：`BMS_*`、`BTM_ZY_C2`、`PDU_SDF`、`SunwodaBMS`、`TempHumiditySensor_JYWS2`、`Trans*`

迁移规则：

- `DeviceHelper`、设备 XML/DLL 加载规则、动态调试控件、初始化/关闭/互斥顺序不变。
- 设备配置管理和设备调试 Host 分 Wave 实施。
- 设备插件目录不重命名、不重排、不合并。
- 每个设备族有单独 smoke，现场设备或 simulator 条件不足时必须进入未验证清单，不能标记完成。

## 待用户确认

请确认是否接受本文作为后续迁移控制路线。确认后仍按 Wave 分批执行，每批开始前输出实施任务拆分和测试计划，每批完成后输出验证报告与两类评审结果；任何实现仍需对应 Wave 的单独确认。

确认前停止在计划阶段，不执行全量实现。
