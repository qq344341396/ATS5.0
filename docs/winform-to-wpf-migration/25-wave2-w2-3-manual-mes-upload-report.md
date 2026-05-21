# Wave 2 W2-3 手动 MES 上传样板补强报告

更新时间：2026-05-21
执行范围：仅 DataQuery 手动 MES 上传样板补强

## 原 WinForms 基线

- 原窗体：`D:\CODE\ATE\01_code\ATS\DataQuery\UcDataQuery.cs`
- 入口事件：`btnMesUpload_ItemClick`
- Designer 基线：`D:\CODE\ATE\01_code\ATS\DataQuery\UcDataQuery.Designer.cs`
- 工具栏按钮：`btnMesUpload`
- 主表多选：`gvIndexInfo.OptionsSelection.MultiSelect = true`
- 右键菜单边界：`popupMenu3` 仅包含数据导出，右键 MES 上传不作为 W2-3 扩展目标。

## WinForms 行为摘录

| 行为 | WinForms 基线 |
| --- | --- |
| 无数据 | `lstIndexInfo == null || lstIndexInfo.Count == 0` 时提示 `未选中数据!` |
| MES 禁用 | `ConfigHelper.GetValueApp("MesEnable") != "1"` 时提示 `未启用MES!` |
| 确认框 | 前置检查通过后提示 `是否对选中的数据进行MES上传?` |
| 多选来源 | `gvIndexInfo.GetSelectedRows().ToList()` |
| LoginMes 失败 | 提示 `MES登录失败:{Msg}` 并返回 |
| SendChannelBarcode 失败 | 提示 `传入通道号和条码:失败{Msg}` 并返回 |
| StationCheck 失败 | 提示 `MES工序校验失败:{Msg}` 并返回 |
| UploadData 成功 | 提示 `条码为：{Barcode},MES上传成功!`，设置 `UploadMesStatus = 1`，调用 `IndexInfoCore.UpdateIndexInfo(item)` |
| UploadData 失败 | 提示 `条码为：{Barcode},MES上传失败:{Msg}`，不更新，继续下一条 |
| UploadData 委托缺失 | 跳过更新和提示，继续下一条 |
| 异常 | `LogHelper.Error("MES上传异常", ex)`，提示 `MES上传异常：{ex.Message}` |

## 本轮完成内容

- `ManualMesUploadService` 增加确认框前置校验，保持顺序为：空数据 -> `MesEnable` -> 确认框 -> `LoginMes` -> 逐条上传。
- `DataQueryViewModel` 增加 `SelectedUploadRecords`，手动 MES 上传优先使用多选记录，单选记录仅作为兜底。
- `DataQueryViewModel` 的 `ManualMesUploadCommand` 保持可执行，空选择时由逻辑提示 `未选中数据!`，避免工具栏入口被禁用后无法复刻 WinForms 提示。
- `DataQueryView.xaml` 保持主表 `SelectionMode="Extended"`，并通过页面内薄 code-behind 同步 `DataGrid.SelectedItems` 到 ViewModel。
- 上传结果按 WinForms 语义逐条反馈：成功消息走 `ShowInfo`，失败和前置失败消息走 `ShowWarning`。
- 保持异常日志通过既有 `ILogService`/legacy `LogHelper` 适配路径，不替换日志框架。

## 修改文件

| 文件 | 说明 |
| --- | --- |
| `D:\CODE\ATE\ATS5.0\ATS5.Application\DataQuery\ManualMesUploadService.cs` | 增加 `ValidateBeforeConfirmation`，复用 legacy 前置校验；保留上传调用顺序和更新时机 |
| `D:\CODE\ATE\ATS5.0\ATS5.Modules.DataQuery\ViewModels\DataQueryViewModel.cs` | 增加多选上传集合、前置校验、逐条弹窗分类，命令始终可执行 |
| `D:\CODE\ATE\ATS5.0\ATS5.Modules.DataQuery\Views\DataQueryView.xaml` | 主索引表命名并绑定 `SelectionChanged`，保持多选 |
| `D:\CODE\ATE\ATS5.0\ATS5.Modules.DataQuery\Views\DataQueryView.xaml.cs` | 仅做 `SelectedItems` 到 `SelectedUploadRecords` 的 UI 桥接 |
| `D:\CODE\ATE\ATS5.0\ATS5.Tests\DataQuery\DataQueryViewModelTests.cs` | 覆盖多选上传、逐条 Info/Warning、未启用 MES/无选择不弹确认、命令可执行性 |
| `D:\CODE\ATE\ATS5.0\ATS5.Tests\DataQuery\ManualMesUploadServiceTests.cs` | 覆盖服务层多条上传、失败继续、委托缺失等 legacy 行为 |
| `D:\CODE\ATE\ATS5.0\ATS5.Tests\DataQuery\LegacyManualMesGatewayTests.cs` | 覆盖 legacy `SysCache` 委托桥接 |
| `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\DataQueryViewStructureTests.cs` | 覆盖 DataQuery 页面入口和多选同步结构 |

## TDD 证据

- 多选 ViewModel 红灯：新增测试首次失败，错误为 `DataQueryViewModel` 未包含 `SelectedUploadRecords`。
- UI 选择同步红灯：`DataQueryViewStructureTests` 首次失败，缺少 `SelectionChanged="RecordsGrid_OnSelectionChanged"` 和 code-behind 同步方法。
- 确认框顺序红灯：`MesEnable="0"` 时 `ConfirmCallCount` 实际为 1，期望 0。
- 无选择入口红灯：无选择时没有 Warning，`WarningMessages.Single()` 为空。
- 工具栏入口可执行红灯：清空选择后 `ManualMesUploadCommand.CanExecute()` 仍为 false，期望 true。

## 验证命令和结果

| 命令 | 结果 |
| --- | --- |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter "FullyQualifiedName~ManualMesUploadServiceTests|FullyQualifiedName~LegacyManualMesGatewayTests|FullyQualifiedName~DataQueryViewModelTests|FullyQualifiedName~DataQueryViewStructureTests"` | 28 passed, 0 failed |
| `dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln` | 所有项目均是最新的，无法还原 |
| `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 成功，0 警告，0 错误 |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 56 passed, 0 failed |

## 数据兼容结果

- 本轮不改变数据保存格式、路径、编码、字段或数据库结构。
- 成功上传仍通过 legacy repository 更新索引记录的 `UploadMesStatus = 1`。
- `UploadData` 失败不更新记录，保持 WinForms 行为。
- `UploadData` 委托缺失时不更新、不提示，保持 WinForms 行为。

## 日志验证结果

- 本轮不替换日志框架。
- `ManualMesUploadAsync` 异常仍通过 `ILogService.Error("MES上传异常", ex)` 进入 legacy log4net/`LogHelper` 适配路径。
- 异常提示保持 `MES上传异常：{ex.Message}`。
- 未启用 MES、无选择、单条上传失败等 legacy 弹窗消息不新增日志，保持当前 WinForms 事件行为。

## UI/交互验证结果

- 工具栏 `MES上传` 入口保留并可执行。
- 主索引表保留 `SelectionMode="Extended"`，支持多选记录进入手动 MES 上传。
- 确认框只在空数据和 MES 启用检查通过后显示。
- 成功消息使用信息弹窗，失败和前置失败使用警告弹窗。
- 右键菜单仍存在 `MES上传`，该项不是 W2-3 新增目标；后续 UI parity 任务可按原 WinForms `popupMenu3` 另行收敛。

## 设备/MES/自动测试影响

- 仅触达手动 MES 上传边界。
- 未迁移或扩展完整 `ATSMes` provider。
- 未进入 GP12 配置页、自动 MES 上传、`ATSAutoTest`、`ATSDevice` 或 ProductTest 主流程。
- 保持 `SysCache` 委托调用顺序：`LoginMes` 一次，随后每条 `SendChannelBarcode -> StationCheck -> UploadData`。

## Subagent 评审

| 评审 | 结论 | 说明 |
| --- | --- | --- |
| Spec Compliance Review 第一次 | Blocked | 指出确认框早于 `MesEnable` 检查、无选择可能静默返回 |
| Spec Compliance Review 第二次 | Approved | 确认前置校验、多选上传、逐条弹窗、调用顺序均符合 W2-3 基线 |
| Code Quality Review | Approved | 无阻塞问题；提出 localized message substring 分类较脆弱等非阻塞维护性提示 |

## 未解决问题和边界

- W2-1 真实现场 gzip JSON 明细文件仍待用户提供可读样本后补验，不由 W2-3 解决。
- 右键菜单 MES 上传与 WinForms `popupMenu3` 不完全一致，暂按本轮范围记录为后续 UI parity 收敛项。
- Code Quality 非阻塞提示：当前 `IsSuccess` 和 Info/Warning 分类依赖 legacy 中文消息文本；若后续要结构化结果级别，必须先确认不改变 WinForms 弹窗语义。

## 结论

W2-3 手动 MES 上传样板补强已完成本轮范围，已通过目标测试、全量构建、全量测试、Spec Compliance Review 和 Code Quality Review。下一步只能在用户确认后进入 W2-4，禁止自动扩大到 Flow、GP12、设备、自动测试或产品测试主链路。
