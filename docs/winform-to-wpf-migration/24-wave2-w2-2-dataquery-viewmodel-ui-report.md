# W2-2 数据查询 ViewModel 行为和 UI 状态门禁报告

更新时间：2026-05-21
状态：已完成，Spec Compliance Review 通过，Code Quality Review 通过

## 本阶段完成内容

本轮只执行 W2-2：数据查询 ViewModel 行为和 UI 状态门禁。

完成项：

- 复刻 WinForms 数据查询默认日期：开始日期为当天前一日，结束日期为当天。
- 复刻 WinForms runtime `PageSize = 200`。
- 复刻空日期提示：`日期不能为空`，不触发查询、不清旧索引/明细、不重置页码。
- 复刻查询按钮非空日期时先将当前页重置为 1。
- 复刻 180 天跨度提示：`日期跨度不能大于180天，请调整查询日期`，不触发仓储查询、不清旧索引/明细。
- 新查询成功或无数据时清理旧选择、索引、明细，避免旧记录继续执行手动 MES 上传。
- 无数据时提示 `未查询到相关数据`，并清理统计筛选状态。
- 选择索引后加载明细；清空选择后清空明细并禁用手动 MES 命令。
- 增加异步明细加载版本控制，避免旧选择的慢请求覆盖新状态。
- 查询异常和 MES 上传异常经 `ILogService` 进入 legacy `LogHelper`/log4net 语义，错误提示保留 WinForms 前缀。
- UI 结构保留左索引、右明细、下方统计区域、右键 MES 上传入口、`上传MES状态` 列和未实现按钮禁用态。

## 修改文件列表

- `D:\CODE\ATE\ATS5.0\ATS5.Modules.DataQuery\ViewModels\DataQueryViewModel.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.DataQuery\Views\DataQueryView.xaml`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\DataQuery\DataQueryViewModelTests.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\DataQueryViewStructureTests.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj`
- `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\00-master-checklist.md`
- `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\11-open-questions.md`
- `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\24-wave2-w2-2-dataquery-viewmodel-ui-report.md`

未修改：

- `D:\CODE\ATE\01_code`
- Flow、Device、AutoTest、ProductTest、GP12 配置页
- W2-3 手动 MES 上传深层实现

## 对应原 WinForms 功能

| WinForms 行为 | 原路径 | WPF 对应 |
| --- | --- | --- |
| 默认日期和 `PageSize=200` | `D:\CODE\ATE\01_code\ATS\DataQuery\UcDataQuery.cs:32-36` | `DataQueryViewModel` 默认值和 `DefaultPageSize` |
| 空日期提示 | `UcDataQuery.cs:75-78` | `QueryAsync` 空日期拦截 |
| 查询按钮重置页码 | `UcDataQuery.cs:80-82` | `QueryAsync` 非空日期后设置 `CurrentPage = 1` |
| 180 天跨度提示 | `UcDataQuery.cs:416-421` | `DataQueryFilter` + `QueryAsync` 返回处理 |
| 查询后清索引/明细 | `UcDataQuery.cs:443-445` | `Records.Clear()`、`Details.Clear()`、`SelectedRecord = null` |
| 无数据提示和统计清理 | `UcDataQuery.cs:449-456` | `ClearStatisticsState()` |
| 选中索引加载明细 | `UcDataQuery.cs:96-103` | `SelectedRecord` + `LoadDetailsCoreAsync` |
| 查询异常日志 | `UcDataQuery.cs:85-88` | `ILogService.Error("数据查询异常", ex)` |
| MES 上传异常日志 | `UcDataQuery.cs:558-561` | `ILogService.Error("MES上传异常", ex)` |

## 验证命令和结果

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter "FullyQualifiedName~DataQueryViewModelTests|FullyQualifiedName~DataQueryViewStructureTests"
```

结果：

- 通过：11
- 失败：0

```powershell
dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
```

结果：

- 0 warning
- 0 error

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
```

结果：

- 通过：45
- 失败：0
- 跳过：0

## 数据兼容结果

- W2-2 未改变 SQLite、gzip JSON、配置文件或保存格式。
- 查询仍经 W2-1 已接入的 legacy 数据查询服务和仓储路径。
- W2-1 的真实现场 gzip JSON 明细样本缺失仍未在 W2-2 内解决，继续在 `11-open-questions.md` 跟踪。

## 日志验证结果

- ViewModel 不直接引用 `LogHelper`，而是依赖应用层 `ILogService` 抽象。
- WPF DI 中既有 `ILogService -> LegacyLogService`，该实现继续调用 `ATSCommon.LogHelper` 和原 log4net。
- 单元测试覆盖查询异常前缀 `数据查询异常` 和 MES 上传异常前缀 `MES上传异常`。
- 未替换日志框架，未改变日志目录或 log4net 配置。

## UI/交互验证结果

- 结构测试确认数据查询页面包含索引表、明细表、统计区域、`MES上传` 入口、`上传MES状态` 列。
- 结构测试确认 `日志导出`、`数据导出`、`全部展开`、`全部收合` 仍为禁用状态。
- XAML 将下方统计筛选与顶部查询筛选拆分，避免无数据清理统计条件时影响顶部查询条件。

## 设备/MES/自动测试影响

- W2-2 只验证手动 MES 上传命令入口和异常日志语义。
- 未进入 W2-3 手动 MES 上传深层调用顺序。
- 未修改 ATSDevice、ATSMes provider、ATSAutoTest、ProductTest。

## 评审结果

### Spec Compliance Review

结论：Approved。

覆盖点：

- 默认日期、`PageSize=200`。
- 空日期、180 天跨度、无数据、查询页码复位。
- 查询成功/无数据状态清理。
- 选择明细加载和旧异步请求防覆盖。
- UI 左索引右明细下统计、右键 MES、未实现按钮禁用。

### Code Quality Review

结论：Approved。

覆盖点：

- `ILogService` 抽象保留原日志框架语义。
- `SelectedRecord` 异步加载版本控制。
- 结构测试不再硬编码 `D:\CODE\ATE\ATS5.0` 路径。
- ViewModel 未直接引用 UI 控件，未扩大到其他模块。

## 未解决问题

1. W2-1 的真实现场 gzip JSON 明细样本缺失仍待补验。
2. W2-3 手动 MES 上传深层行为尚未执行，不能宣称手动 MES 上传完整 parity 已完成。
3. 尚未做 WPF 实机 UI 点击验收；当前为 ViewModel/结构测试和人工代码对照。

## 下一步建议

等待用户确认是否进入 W2-3。确认前不启动 Flow W2-4、QA W2-7、ProductTest、GP12、AutoType 9 或设备调试。
