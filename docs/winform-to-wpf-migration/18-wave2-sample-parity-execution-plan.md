# Wave 2 样板端到端补齐实施计划

更新时间：2026-05-21
状态：计划完成，等待用户确认；确认前不实现

> **For agentic workers:** REQUIRED: Use subagent-driven-development if implementation subagents are available, otherwise use executing-plans. All implementation tasks must follow TDD: write failing test, verify red, implement minimum green, run target test and full verification.

## 目标

Wave 2 只把两个已确认样板补齐到可验收的端到端迁移模板：

- 数据查询 + 手动 MES 上传。
- 流程管理脚本模式：打开、保存测试副本、脚本校验、数据兼容。

Wave 2 不迁移产品测试执行、GP12 全量 MES、AutoType 9 自动化、设备调试和其他 provider。

## 架构边界

沿用 Wave 1 已完成基础设施：

- `D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln`
- `D:\CODE\ATE\ATS5.0\ATS5.Application`
- `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.DataQuery`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Flow`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests`

原 WinForms 验收基线：

- 数据查询：`D:\CODE\ATE\01_code\ATS\DataQuery\UcDataQuery.cs`
- 数据查询 Designer：`D:\CODE\ATE\01_code\ATS\DataQuery\UcDataQuery.Designer.cs`
- 流程编辑：`D:\CODE\ATE\01_code\ATS\Flow\UcFlow.cs`
- 流程编辑 Designer：`D:\CODE\ATE\01_code\ATS\Flow\UcFlow.Designer.cs`
- Debug 运行数据：`D:\CODE\ATE\01_code\ATS\bin\Debug`

## 禁止扩大范围

- 不启用 Shell 中产品测试、设备管理、权限管理、MES、自动化、统计信息入口。
- 不实现 GP12 全量配置页和自动上传。
- 不迁移 `ATSMes`、`ATSAutoTest`、`ATSDevice`。
- 不修改 `D:\CODE\ATE\01_code` 下源码、配置、数据库和真实流程文件。
- 不覆盖真实 `.fw`、`.dev`、`.adbc`、`.xlsx`、`ats.db`。
- 不替换 log4net 或 `LogHelper`。
- 不改变 `SysCache.UploadData`、`IndexInfoCore`、`TestProjectDataCore`、`JsonHelper`、`ConfigHelper` 语义。
- 不把结构占位功能标记为完成，尤其是数据导出、日志导出、展开/收合、完整脚本编辑器、DBC/UDS 插入、设备命令树真实联动。

## 文件 Owner

| Owner | 可写文件范围 | 禁止写入 |
| --- | --- | --- |
| DataQuery worker | `ATS5.Application\DataQuery\*`、`ATS5.Infrastructure.LegacyAdapters\DataQuery\*`、`ATS5.Modules.DataQuery\*`、`ATS5.Tests\DataQuery\*` | `ATS5.Modules.Flow`、`ATS5.Modules.Mes`、`ATS5.Modules.Device`、legacy 源码 |
| Flow worker | `ATS5.Application\Flow\*`、`ATS5.Infrastructure.LegacyAdapters\Flow\*`、`ATS5.Modules.Flow\*`、`ATS5.Tests\Flow\*` | `ATS5.Modules.DataQuery`、产品测试、MES、AutoTest、Device |
| QA/Verification worker | `docs\winform-to-wpf-migration\*`、`ATS5.Tests\Infrastructure\*`，只允许新增验证测试 | 业务实现文件，除非 Controller 明确批准 |
| Controller | 总清单、计划、评审、合并冲突裁决 | 不直接扩大业务功能 |

禁止多个实现 worker 同时修改同一文件。

## 旧代码对照点

### 数据查询

| WinForms 行为 | 原路径 |
| --- | --- |
| 页面 Load 初始化日期、流程列表、事件 | `UcDataQuery.cs:32` |
| 条码、流程、通道筛选勾选启用/禁用输入框 | `UcDataQuery.cs:44`、`:53`、`:62` |
| 查询按钮 | `UcDataQuery.cs:71` |
| 日期跨度大于 180 天提示 | `UcDataQuery.cs:420` |
| `IndexInfoCore.GetTotalCount/GetIndexInfo` 查询 | `UcDataQuery.cs:438`、`:440` |
| 查询前清空索引和明细数据源 | `UcDataQuery.cs:443`、`:444` |
| 未查到数据提示 | `UcDataQuery.cs:449` |
| 选择索引加载 `TestProjectDataCore.GetTestProjectDatas` | `UcDataQuery.cs:102` |
| MES 上传确认框 | `UcDataQuery.cs:505` |
| 手动 MES 上传调用 `SysCache.UploadData` | `UcDataQuery.cs:542` |
| 上传成功更新 `UploadMesStatus=1` 并 `IndexInfoCore.UpdateIndexInfo` | `UcDataQuery.cs:548` 到 `:555` |
| 查询异常和 MES 上传异常写 `LogHelper.Error` | `UcDataQuery.cs:87`、`:560` |
| `gcIndexInfo/gcTestData`、`UploadMesStatus` 列和右键菜单 | `UcDataQuery.Designer.cs:201`、`:354`、`:799`、`:970` |

### 流程编辑

| WinForms 行为 | 原路径 |
| --- | --- |
| 打开流程文件 `JsonHelper.GetJsonFile(...).ToObject<Flow>()` | `UcFlow.cs:231` 到 `:270` |
| 打开后填充项目树、选中状态、设备配置标签 | `UcFlow.cs:247` 到 `:264` |
| 保存前校验项目、设备配置名、`.dev` 存在且非空 | `UcFlow.cs:368` 到 `:388` |
| 保存前执行 `AllCheckCode()` | `UcFlow.cs:395` |
| 首次保存弹出 `FrmFlowSave` 并写 `.fw` | `UcFlow.cs:399` 到 `:404` |
| 覆盖保存先写备份再写正式文件 | `UcFlow.cs:411` 到 `:421` |
| 另存为写新 `.fw` 并记录操作日志 | `UcFlow.cs:435` 到 `:463` |
| 单项脚本校验写临时流程并调用编译 | `UcFlow.cs:1451` 到 `:1527` |
| 全流程脚本校验只校验启用项目，带公共变量 | `UcFlow.cs:1532` 到 `:1641` |
| 设备控制树读取 `.dev` | `UcFlow.cs:1669`、`:1750`、`:2029` |
| 顶部按钮可见/可用状态随查看/编辑模式变化 | `UcFlow.cs:93` 到 `:141` |

## Wave 2 任务拆分

### W2-1 数据查询真实 SQLite 查询兼容

**目标：** 用实际 Debug `ats.db` 证明 WPF adapter 走原 `IndexInfoCore` 查询语义，且不会写库。

**Files:**

- Test: `D:\CODE\ATE\ATS5.0\ATS5.Tests\DataQuery\LegacyTestRecordRepositoryIntegrationTests.cs`
- Modify if needed: `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\DataQuery\LegacyTestRecordRepository.cs`
- Modify if needed: `D:\CODE\ATE\ATS5.0\ATS5.Application\DataQuery\DataQueryService.cs`

**Steps:**

- [ ] 写红灯测试：用 `DebugRuntimePathProvider` 指向 `D:\CODE\ATE\01_code\ATS\bin\Debug`，查询一个宽日期范围，断言 `GetTotalCount` 与 `GetRecords` 返回一致页数据。
- [ ] 运行目标测试，预期失败点必须是当前 adapter/query 行为缺口，不接受测试路径拼写错误。
- [ ] 最小修复 adapter 或 service，不改变 `IndexInfoCore` 入参和分页语义。
- [ ] 增加明细加载测试：选择首条有明细的 `TestRecord`，调用 `GetDetails`，只读验证字段映射不丢。
- [ ] 验证查询前后 `ats.db` SHA256 不变；如果 SQLite WAL/SHM 有运行态变化，记录原因并改用只读副本测试。
- [ ] 运行：
  - `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~LegacyTestRecordRepositoryIntegrationTests`
  - `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore`
  - `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore`

**完成标准：**

- 真实 SQLite 旧数据 WPF 可读。
- 查询条件、分页、索引字段、明细字段映射与 WinForms 对齐。
- 原 Debug 数据不被污染。

### W2-2 数据查询 ViewModel 行为和 UI 状态门禁

**目标：** 锁定 WinForms 工具条、筛选、查询、选择索引加载明细、手动 MES 入口的用户操作顺序。

**Files:**

- Test: `D:\CODE\ATE\ATS5.0\ATS5.Tests\DataQuery\DataQueryViewModelTests.cs`
- Test: `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\DataQueryViewStructureTests.cs`
- Modify: `D:\CODE\ATE\ATS5.0\ATS5.Modules.DataQuery\ViewModels\DataQueryViewModel.cs`
- Modify: `D:\CODE\ATE\ATS5.0\ATS5.Modules.DataQuery\Views\DataQueryView.xaml`

**Steps:**

- [ ] 写红灯测试：日期跨度超过 180 天时 ViewModel 显示原提示，不查询 repository。
- [ ] 写红灯测试：查询成功后先清空旧索引/明细，再填充新索引，总数和消息对齐。
- [ ] 写红灯测试：SelectedRecord 变化后加载明细，未选中时明细清空，MES 上传命令不可执行。
- [ ] 写 XAML 结构测试：左索引、右明细、左下统计区、右键 `MES上传`、`UploadMesStatus` 列存在。
- [ ] 最小实现，保持 `DataQueryViewModel` 不直接依赖 legacy 类型。
- [ ] 对仍未实现按钮保持禁用：日志导出、数据导出、全部展开、全部收合。
- [ ] 运行目标测试和全量测试。

**完成标准：**

- 数据查询页面行为从“结构样板”提升为“查询/选中/明细/MES 手动入口可测样板”。
- 未实现导出/展开类功能仍不可用或明确占位。

### W2-3 手动 MES 上传样板补强

**目标：** 只补数据查询样板中的手动 MES 上传，不进入 GP12 配置页和自动上传迁移。

**Files:**

- Test: `D:\CODE\ATE\ATS5.0\ATS5.Tests\DataQuery\ManualMesUploadServiceTests.cs`
- Test: `D:\CODE\ATE\ATS5.0\ATS5.Tests\DataQuery\LegacyManualMesGatewayTests.cs`
- Modify if needed: `D:\CODE\ATE\ATS5.0\ATS5.Application\DataQuery\ManualMesUploadService.cs`
- Modify if needed: `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\DataQuery\LegacyManualMesGateway.cs`

**Steps:**

- [ ] 补红灯测试：用户取消确认框时，不调用 `LoginMes/SendChannelBarcode/StationCheck/UploadData`。
- [ ] 补红灯测试：`UploadData` 返回失败时，保持 WinForms 提示 `条码为：...,MES上传失败:...`，不更新为已上传。
- [ ] 补红灯测试：多选记录按 WinForms 顺序逐条处理，成功项更新，失败项保留。
- [ ] 若接入 legacy gateway，测试 `SysCache.UploadData == null` 时与 WinForms 一样跳过更新。
- [ ] 不写真实 MES，不调用外部网络；真实 GP12 留到 Wave 4。
- [ ] 运行目标测试和全量测试。

**完成标准：**

- 手动 MES 上传调用顺序、确认框、成功/失败提示、`UploadMesStatus` 更新与 WinForms 对齐。
- 仍不实现 GP12 页面和自动上传链。

### W2-4 流程 `.fw` 多样本打开和字段往返兼容

**目标：** 用真实 Debug `.fw` corpus 证明 WPF flow adapter 可以打开、转换、序列化，关键字段不丢。

**Files:**

- Test: `D:\CODE\ATE\ATS5.0\ATS5.Tests\Flow\LegacyFlowRepositoryCompatibilityTests.cs`
- Modify if needed: `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\Flow\LegacyFlowRepository.cs`
- Modify if needed: `D:\CODE\ATE\ATS5.0\ATS5.Application\Flow\ProcessFlowDefinition.cs`
- Modify if needed: `D:\CODE\ATE\ATS5.0\ATS5.Application\Flow\ProcessProjectDefinition.cs`

**样本优先级：**

- `测试1.fw`
- `测试2.fw`
- `超充重卡PDU-临工（三支路）.fw`
- `大秦800Y-BMS.fw`
- `徐工528子母车 扩展帧-EOL测试.fw`
- `DCDC-EOL测试.fw`
- `Maserati M189 成品测试（主控）-通道2.fw`

**Steps:**

- [ ] 写红灯测试：逐个打开样本，断言 `DevCfgName`、`MESParamName`、项目数量、启用状态、脚本、输出项、临时变量、全局变量、DBC、UDS 不丢。
- [ ] 写往返测试：`Open -> Serialize -> ToObject<ATSModel.Flow>`，对关键字段做结构化语义对比。
- [ ] 不要求 JSON 文本字节级完全一致，但字段、顺序和 WinForms 可读语义必须一致。
- [ ] 禁止写回真实 `SysCache\Flows`；如需写文件，写到测试临时目录。
- [ ] 运行目标测试和全量测试。

**完成标准：**

- 多个真实 `.fw` 样本可读可序列化。
- 字段映射缺口全部补齐或进入 open questions。

### W2-5 流程保存测试副本和脚本校验门禁

**目标：** 保持当前脚本模式，验证保存测试副本、设备配置存在性、脚本校验消息，不覆盖原流程。

**Files:**

- Test: `D:\CODE\ATE\ATS5.0\ATS5.Tests\Flow\FlowEditorServiceTests.cs`
- Test: `D:\CODE\ATE\ATS5.0\ATS5.Tests\Flow\LegacyFlowScriptValidatorTests.cs`
- Modify if needed: `D:\CODE\ATE\ATS5.0\ATS5.Application\Flow\FlowEditorService.cs`
- Modify if needed: `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\Flow\LegacyFlowScriptValidator.cs`
- Modify if needed: `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\Flow\LegacyFlowRepository.cs`

**Steps:**

- [ ] 补红灯测试：设备配置名为空、`.dev` 不存在、`.dev` 空文件时，提示与 WinForms 对齐。
- [ ] 补红灯测试：保存测试副本前必须执行全流程脚本校验；校验失败不写文件。
- [ ] 补红灯测试：保存样板副本只能写 `*-WpfSample-*.fw`，不能覆盖源文件。
- [ ] 脚本校验只使用当前脚本模式，不引入工步模式实现。
- [ ] 如需写文件，写测试临时目录并用 legacy `JsonHelper`/模型反开。
- [ ] 运行目标测试和全量测试。

**完成标准：**

- 流程保存测试副本可以作为后续页面迁移模板。
- 当前脚本模式行为不被工步模式抽象破坏。

### W2-6 流程 ViewModel 和 XAML 状态门禁

**目标：** 锁定流程编辑样板 UI 中哪些可用、哪些仍是占位，避免误验收。

**Files:**

- Test: `D:\CODE\ATE\ATS5.0\ATS5.Tests\Flow\FlowEditorViewModelTests.cs`
- Test: `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\FlowEditorViewStructureTests.cs`
- Modify: `D:\CODE\ATE\ATS5.0\ATS5.Modules.Flow\ViewModels\FlowEditorViewModel.cs`
- Modify: `D:\CODE\ATE\ATS5.0\ATS5.Modules.Flow\Views\FlowEditorView.xaml`

**Steps:**

- [ ] 写红灯测试：未打开流程时，保存和脚本校验命令不可执行。
- [ ] 写红灯测试：打开流程后填充项目、选择首项目、显示设备配置。
- [ ] 写红灯测试：保存时只调用 SaveSampleCopy，不调用覆盖保存。
- [ ] 写 XAML 结构测试：顶部工具条、左项目区、脚本页、输出项、临时变量、右设备控制区、底部 DBC/UDS/公共变量页签存在。
- [ ] 未完成按钮继续禁用：新建、编辑、导出、导入、另存为、取消、编辑公共变量、修改流程名称、格式化、导入设备、输出/临时变量增删改移动复制。
- [ ] 运行目标测试和全量测试。

**完成标准：**

- 流程编辑页面从“截图结构像”推进到“打开/校验/保存测试副本的样板行为可测”。
- 未实现编辑器高级行为仍明确不在完成口径。

### W2-7 UI 自动化 smoke 和人工验收清单

**目标：** 为两个样板建立最低限度 UI 验收证据。

**Files:**

- Test or script: `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\WpfSmokeTests.cs` 或 `D:\CODE\ATE\ATS5.0\tests\ui-smoke\*`
- Docs: `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\19-wave2-sample-verification-report.md`
- Docs: `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\14-ui-parity-sample-checklist.md`

**Steps:**

- [ ] 选择可在本机稳定运行的 UI smoke 方案：优先 FlaUI；如包或环境受限，先用启动探活 + XAML 结构测试 + 人工验收表。
- [ ] 验证 Shell 启动、导航到数据查询、导航到流程管理。
- [ ] 验证未迁移导航按钮不可点击或保持禁用。
- [ ] 输出人工验收清单：数据查询、流程编辑逐项勾选。
- [ ] 不要求用户每项截图；但高风险区域建议截图或录屏。

**完成标准：**

- 有自动或半自动 UI smoke 证据。
- 有人工验收表，明确哪些已完成、哪些仍占位。

## Wave 2 总验证命令

每个小任务完成后至少执行：

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~<TargetTestClass>
dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
```

Wave 2 完成时执行：

```powershell
dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln
dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
```

必要时启动：

```powershell
D:\CODE\ATE\ATS5.0\ATS5.Wpf\bin\Debug\net461\ATS5.Wpf.exe
```

## Wave 2 文档产出

- `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\19-wave2-sample-verification-report.md`
- 更新 `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\00-master-checklist.md`
- 更新 `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\14-ui-parity-sample-checklist.md`
- 如发现原行为不明确，更新 `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\11-open-questions.md` 或创建对应问题清单。

## Wave 2 完成门禁

- 数据查询真实 SQLite 查询/明细读取通过。
- 手动 MES 上传 fake/adapter 调用顺序和提示通过。
- 流程 `.fw` 多样本打开和字段往返通过。
- 流程保存测试副本不覆盖原文件，并可用 legacy 模型反开。
- 日志仍使用 `LogHelper` 和 log4net。
- 未实现按钮保持禁用或明确占位。
- `dotnet restore/build/test` 全通过。
- 输出 Spec Compliance Review 和 Code Quality Review。

## 等待确认

请确认是否按本计划进入 Wave 2 实现。确认后才允许按 W2-1 到 W2-7 小步实施。
