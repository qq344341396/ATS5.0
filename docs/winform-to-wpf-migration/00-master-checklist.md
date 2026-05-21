# WinForms 到 WPF 迁移总控清单

更新时间：2026-05-21
项目根路径：`D:\CODE\ATE\ATS5.0`
原 WinForms 基线：`D:\CODE\ATE\01_code`
当前阶段：Wave 3 WP-06 权限管理、WP-07 设备配置管理、WP-08 产品测试壳和扫码弹窗均已完成代码侧验证并通过双评审；真实现场明细样本和人工点击验收仍待补验；确认前禁止进入下一任务批次或扩大实现范围

## 当前边界

- 允许：读取代码、读取迁移文档、创建和更新迁移文档、执行已确认 Wave 的小步任务。
- 禁止：一次性铺开全量实现、全量迁移 `ATSMes`、`ATSAutoTest`、`ATSDevice`、修改原业务代码、替换日志框架、改变数据保存格式。
- 当前 WPF 样板只代表主界面、数据查询、流程编辑方向和两个样板的首版验证，不代表全部业务已迁移。

## 已完成阶段

| 阶段 | 状态 | 证据 |
| --- | --- | --- |
| 第一阶段：现状盘点 | 完成 | `D:\CODE\ATE\01_code\docs\winform-to-wpf-migration\01-current-state-inventory.md` 等 |
| 第二阶段：WPF/Prism 架构设计 | 完成 | `D:\CODE\ATE\01_code\docs\winform-to-wpf-migration\06-wpf-prism-architecture.md` |
| 第三阶段：流程编辑扩展接口设计 | 完成 | `D:\CODE\ATE\01_code\docs\winform-to-wpf-migration\07-process-editor-extension-design.md` |
| 第四阶段：样板选择 | 完成 | `D:\CODE\ATE\01_code\docs\winform-to-wpf-migration\08-sample-migration-plan.md` |
| 第五阶段：样板实施计划 | 完成 | `D:\CODE\ATE\01_code\docs\winform-to-wpf-migration\08-sample-migration-plan.md` |
| 第六阶段：样板首版实现 | 完成首版 | `D:\CODE\ATE\01_code\docs\winform-to-wpf-migration\13-sample-verification-report.md`、`D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\14-ui-parity-sample-checklist.md` |
| 第八阶段：全量迁移计划 | 已输出，等待确认；已补实际代码入口附录 | `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\10-full-migration-roadmap.md` |
| 全量计划确认清单 | 已输出，等待确认 | `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\20-full-migration-confirmation-checklist.md` |
| 全量 Work Package 拆分表 | 已输出，等待确认 | `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\21-full-migration-work-packages.md` |
| 全量迁移计划确认稿 | 已输出，等待确认；确认前禁止全量实现 | `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\22-full-migration-plan-confirmation-draft.md` |
| 目标工程未确认问题清单 | 已输出，记录 W2-1 真实现场明细待补验、legacy 查询策略边界、W2-3 UI parity 后续收敛项和 W2-4/W2-5 非阻塞测试维护项 | `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\11-open-questions.md` |
| 全量计划确认 | 待确认 | 最新要求为“计划确认前不要全量实现” |

## 本轮并行盘点摘要

| Agent | 范围 | 结论 |
| --- | --- | --- |
| UI/WinForms | 主框架、登录、产品测试、数据查询、流程、设备、权限、MES、自动化 UI | 高风险集中在流程编辑、设备管理、MES、自动化和产品测试主链路；每个窗体需要页面级 parity checklist |
| MES/Device/AutoTest | `SysCache`、`ProductTestCore`、GP12、AutoType 9、设备插件 | `GP12 + AutoType=9 + ChannelNum=2` 必须单独端到端验收，禁止普通组合迁移 |
| Data/Logging/Verification | SQLite、gzip JSON、`.fw/.dev/.adbc/.xlsx`、`ATS.exe.config`、log4net | 全量前必须补真实资产兼容测试、日志目录格式验证、发布资产清单 |

## 全量迁移总门禁

进入全量实现前必须满足：

- [ ] 用户确认 `10-full-migration-roadmap.md`。
- [ ] 用户确认 `20-full-migration-confirmation-checklist.md`。
- [ ] 用户确认 `21-full-migration-work-packages.md`。
- [ ] 用户确认 `22-full-migration-plan-confirmation-draft.md`。
- [x] Wave 2 样板端到端补齐完成并通过自动/半自动验证；人工点击验收仍需用户确认。
- [x] 全量计划已列出 ATS WinForms 页面、ATSMes provider、ATSAutoTest provider、ATSDevice 插件入口，后续不得遗漏实际代码目录。
- [ ] 每个 Wave 开始前输出实施任务拆分和测试计划。
- [ ] 每个实现 agent 有单一 owner 和明确文件范围。
- [ ] 不同 agent 不并行修改同一文件或同一核心语义。
- [ ] 每个功能完成后执行构建、测试、数据、日志、UI、设备/MES/自动化验证。
- [ ] 每个实现任务完成后进行 Spec Compliance Review 和 Code Quality Review。

## 当前未满足门禁

- 人工 UI 对照和运行时点击导航尚未全部签核。
- 真实 SQLite/gzip JSON/Excel/CSV 兼容测试尚未全量补齐。
- W2-1 数据查询真实 SQLite 索引和 seeded legacy gzip 明细链路已通过；真实现场 `AppData\TestData` 明细文件仍全部为 3 字节，待用户提供可读样本后补验。
- GP12 真实或模拟 MES 上传未形成完整报告。
- AutoType 9 双通道自动化未端到端验证。
- 设备初始化/关闭/互斥/调试未联机验证。
- 发布包资产清单尚未验证。

## 当前 Wave 0/1 状态

- [x] Wave 0 运行资产基线报告。
- [x] Wave 0 UI 验收缺口报告。
- [x] Wave 0 当前 WPF 样板构建和测试基线。
- [x] Wave 1 基础设施缺口报告。
- [x] Wave 1 首个最小实现任务拆分和测试计划。
- [x] W1-1 输出资产清单检查。
- [x] W1-2 日志通道 smoke。
- [x] W1-3 配置读取/写回兼容 smoke。
- [x] W1-4 RuntimePathProvider 固化。
- [x] W1-5 未迁移入口状态检查。
- [x] Wave 1 Debug 基础设施总验证：`dotnet restore` 成功；`dotnet build --no-restore` 成功，0 警告，0 错误；`dotnet test --no-restore` 成功，23 通过，0 失败，0 跳过。

## Wave 2 样板端到端补齐计划状态

- [x] 输出 Wave 2 实施任务拆分和测试计划：`D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\18-wave2-sample-parity-execution-plan.md`。
- [x] 明确 Wave 2 文件 owner：DataQuery worker、Flow worker、QA/Verification worker、Controller。
- [x] 明确 Wave 2 禁止扩大范围：不启用产品测试/设备/MES/自动化入口，不全量迁移 `ATSMes`、`ATSAutoTest`、`ATSDevice`，不修改 `D:\CODE\ATE\01_code`。
- [x] 用户确认 Wave 2 执行批次。
- [x] W2-1 数据查询真实 SQLite 查询兼容：真实 SQLite 索引分页通过，seeded legacy gzip 明细链路补验通过，Spec Review 通过，Code Quality Review 通过；真实现场 gzip JSON 明细待补验，`Barcode/FlowName` 继续保持 WinForms legacy 查询行为，见 `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\23-wave2-w2-1-blocker-report.md`。
- [x] W2-2 数据查询 ViewModel 行为和 UI 状态门禁：默认日期、PageSize=200、空日期提示、查询页码复位、180 天跨度拦截、无数据统计清理、选择加载明细、旧异步明细防覆盖、旧选择 MES 禁用、原日志异常语义均已覆盖；Spec Review 和 Code Quality Review 通过，见 `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\24-wave2-w2-2-dataquery-viewmodel-ui-report.md`。
- [x] W2-3 手动 MES 上传样板补强：多选上传、确认前置校验、逐条 Info/Warning、legacy `SysCache` 委托桥接和 `UploadMesStatus` 更新时机已覆盖；Spec Review 和 Code Quality Review 通过，见 `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\25-wave2-w2-3-manual-mes-upload-report.md`。
- [x] W2-4 流程 `.fw` 多样本打开和字段往返兼容：七个真实 Debug `.fw` 样本全部覆盖，`Open` 映射和 `Open -> Serialize -> legacy ToObject<ATSModel.Flow>` 结构化往返通过；Spec Review 和 Code Quality Review 通过，见 `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\26-wave2-w2-4-flow-fw-compatibility-report.md`。
- [x] W2-5 流程保存测试副本和脚本校验门禁：样板副本名保护、设备配置门禁、校验失败不写、WinForms `AllCheckCode` 确定性门禁和默认比较符语义已覆盖；Spec Review 和 Code Quality Review 通过，见 `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\27-wave2-w2-5-flow-save-script-validation-report.md`。
- [x] W2-6 流程 ViewModel 和 XAML 状态门禁：未打开流程命令禁用、打开后填充项目/设备配置/首项目、保存仅走测试副本、XAML 未完成按钮禁用、输出/临时变量表格只读、项目启用项只读已覆盖；Spec Review 和 Code Quality Review 通过，见 `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\28-wave2-w2-6-flow-viewmodel-xaml-state-report.md`。
- [x] W2-7 UI 自动化 smoke 和人工验收清单：采用无 FlaUI fallback，新增 Shell/XAML/导航结构 smoke、WPF 启动探活脚本和人工验收表；Spec Review 和 Code Quality Review 通过，见 `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\19-wave2-sample-verification-report.md`。

## Wave 3 当前状态

- [x] WP-06 权限管理：角色/用户/权限 CRUD、弹窗、删除确认、重复校验、失败提示、`Role.Powers` JSON string array 和 legacy `User`/`Role` 表读写已完成；Spec Compliance Review 和 Code Quality Review 通过，见 `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\30-wave3-wp06-authority-report.md`。
- [x] WP-07 设备配置管理：设备库树/搜索、`.dev` 打开/保存/另存为、排序、删除、初始化参数说明、legacy JSON/UTF-8 BOM 兼容、禁用设备调试/清全局入口已完成；Spec Compliance Review 和 Code Quality Review 通过，见 `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\32-wave3-wp07-device-config-report.md`。
- [x] WP-08 产品测试壳和扫码弹窗：Shell 产品测试导航、`ChannelNum` 通道壳、通道标题状态规则、扫码弹窗流程列表/数量/回车/确定/取消/必填语义已完成；Spec Compliance Review 和 Code Quality Review 通过；禁止产品测试执行链、GP12、AutoType 9。
- [ ] Wave 3 整体完成：代码侧 WP-06/WP-07/WP-08 已完成并通过双评审；人工点击验收未关闭，不能标记为现场验收通过。

WP-06 新鲜验证证据：

- `dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln`：所有项目均是最新的，无法还原。
- `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore`：0 警告，0 错误。
- `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter Authority`：29/29 通过。
- `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore`：112/112 通过。
- 禁止范围关键词扫描：无 ProductTestCore.Start、GP12、AutoType 9、设备调试命中。

WP-07 新鲜验证证据：

- `dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln`：所有项目均是最新的，无法还原。
- `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore`：0 警告，0 错误。
- `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter DeviceConfig`：23/23 通过。
- `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter DeviceManagerViewStructureTests`：2/2 通过。
- `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore`：132/132 通过。
- WP-07 禁止范围关键词扫描：无 FrmDevDebug、DevicePool.ClearObj、ProductTestCore.Start、GP12、AutoType 9、真实设备 Init 命中。

WP-08 当前验证证据：

- RED：`dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter ProductTestShell` 初次失败，缺少 `ATS5.Modules.ProductTest` 和目标 ViewModel/Dialog 类型。
- `dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln`：所有项目均是最新的，无法还原。
- `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore`：0 警告，0 错误。
- `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter "ProductTestShell|ProductTestShellViewStructureTests|ShellNavigationStateTests|WpfSmokeTests"`：15/15 通过。
- `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore`：143/143 通过。
- WP-08 禁止范围关键词扫描：生产 WP-08 模块和 ProductTestShell 测试无 ProductTestCore、SysCache.StartTest、SysCache.UploadData、AutoTestStartTestDic、AutoTestStopTestDic、DevicePool、真实 Init、GP12、AutoType 9、ATSMes、ATSAutoTest、ATSDevice 或 ExecuteTest 命中。

## 下一步

WP-06、WP-07 和 WP-08 已通过自动验证和双评审。Wave 2 样板补齐的代码侧门禁已完成，但人工点击导航、真实现场明细样本、GP12、AutoType 9、设备联机、正式覆盖保存和全量迁移仍未验收；任何后续 Wave 或任务开始前必须由用户重新确认任务范围和门禁。
