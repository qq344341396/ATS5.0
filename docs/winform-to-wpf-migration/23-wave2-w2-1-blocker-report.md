# W2-1 数据查询真实 SQLite 查询兼容阻塞报告

更新时间：2026-05-21
状态：阻塞，禁止进入 W2-2

## 本阶段完成内容

本轮只执行 W2-1：数据查询真实 SQLite 查询兼容。

已验证到的部分：

- WPF `LegacyTestRecordRepository` 可通过 legacy `IndexInfoCore.GetTotalCount` 查询真实 Debug SQLite 数据。
- WPF `LegacyTestRecordRepository` 可通过 legacy `IndexInfoCore.GetIndexInfo` 获取真实分页数据。
- 测试使用临时目录复制 `D:\CODE\ATE\01_code\ATS\bin\Debug`，未直连写入原 Debug 数据。
- 源库 `D:\CODE\ATE\01_code\ATS\bin\Debug\AppData\ats.db` SHA256 未改变。

未完成部分：

- `TestProjectDataCore.GetTestProjectDatas` 的真实明细字段映射无法验收。
- 原因是当前 Debug 数据目录下 131 个 `AppData\TestData\*.json` 文件全部为 3 字节，缺少可反序列化的真实明细内容。

## 修改文件列表

- `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\DataQuery\LegacyTestRecordRepository.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\DataQuery\LegacyTestRecordRepositoryIntegrationTests.cs`
- `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\23-wave2-w2-1-blocker-report.md`

未修改：

- `D:\CODE\ATE\01_code`
- `ATS5.Modules.DataQuery`
- `ATS5.Application\DataQuery\DataQueryService.cs`
- Flow、MES、Device、ProductTest、AutoTest、Shell

## 对应原 WinForms 功能

| WinForms 行为 | 原路径 | 当前验证状态 |
| --- | --- | --- |
| 查询总数 | `D:\CODE\ATE\01_code\ATS\DataQuery\UcDataQuery.cs:438` | 已验证 |
| 查询分页索引 | `D:\CODE\ATE\01_code\ATS\DataQuery\UcDataQuery.cs:440` | 已验证 |
| 选择索引加载明细 | `D:\CODE\ATE\01_code\ATS\DataQuery\UcDataQuery.cs:102` | 阻塞，缺真实明细样本 |

## 验证命令和结果

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~LegacyTestRecordRepositoryIntegrationTests
```

结果：

- 通过：1
- 失败：1
- 失败项：`GetDetails_MapsRealTestProjectDataFields_ForFirstRecordWithDetails`
- 失败原因：`Assert.NotNull() Failure`，未找到可读明细样本。

```powershell
dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
```

结果：

- 0 warning
- 0 error

```powershell
Get-ChildItem -Path 'D:\CODE\ATE\01_code\ATS\bin\Debug\AppData\TestData' -Recurse -Filter '*.json' | Measure-Object -Property Length -Minimum -Maximum -Sum
```

结果：

- Count：131
- Minimum：3
- Maximum：3
- Sum：393

```powershell
Get-FileHash -Algorithm SHA256 'D:\CODE\ATE\01_code\ATS\bin\Debug\AppData\ats.db'
```

结果：

- `0E4FD32446672A069ED0E7A54296E3AE5EF36F0EEDFD1D8770945952F3C589C0`

## 数据兼容结果

- 真实 SQLite 旧数据可读取。
- `IndexInfoCore.GetTotalCount` 与 `IndexInfoCore.GetIndexInfo` 走 legacy 查询链路。
- 宽日期范围 `20251201000000..20260430235959` 可查询到真实记录。
- 源 `ats.db` 未被修改。
- gzip JSON 明细兼容未通过，原因是当前样本缺少可读明细内容。

## 日志验证结果

W2-1 未触发日志写入；日志验证不在本小任务范围内。

## UI/交互验证结果

W2-1 未修改 UI；UI/交互验证不在本小任务范围内。

## 设备/MES/自动测试影响

无影响。本轮未触发 MES、设备、自动化、产品测试。

## 未解决问题

1. 当前 Debug `AppData\TestData` 明细文件全部只有 3 字节，无法验证 `GetDetails` 字段映射。
2. W2-1 不能判定完成，因此不能进入 W2-2。
3. 需要用户提供至少一个真实可读 `AppData\TestData\yyyy\M\d\{LogGuid}-CH{channel}.json` 明细样本，或确认允许改用其他实际运行目录做 W2-1 明细验证。

## 追加只读根因诊断

已复核 legacy 明细路径，当前阻塞不是测试目录写错。

| 核对点 | 结论 |
| --- | --- |
| `D:\CODE\ATE\01_code\ATSCore\SysCache.cs` | `PathTestData = "AppData/TestData/"` |
| `D:\CODE\ATE\01_code\ATSCore\DBCore\TestProjectDataCore.cs` | 明细读取路径为 `{SysCache.PathTestData}/{year}/{month}/{day}/{LogGuid}-CH{Channel}.json` |
| `D:\CODE\ATE\01_code\ATSCore\ProductTestCore.cs` | 明细写入同样使用 `SysCache.PathTestData` |
| `D:\CODE\ATE\01_code\ATSCommon\JsonHelper.cs` | `ReadJsonFileEx` 和 `WriteJsonFileEx` 使用 gzip 压缩/解压 |
| `D:\CODE\ATE\01_code\ATS\App.config` | `AutoExportPath = "TestData"`，这是自动导出目录，不是 `GetTestProjectDatas` 明细读取目录 |

磁盘扫描结论：

- `D:\CODE\ATE\01_code\ATS\bin\Debug\AppData\TestData` 存在 131 个 `.json`，全部长度为 3 字节，内容为 gzip 头 `1F 8B 08`。
- `D:\CODE\ATE\01_code\ATS\bin\Debug\TestData` 下没有 `.json` 明细，只有自动导出的 `.csv/.xlsx` 报表。

因此不建议修改 W2-1 生产代码路径。当前代码按 legacy 规则读取 `AppData\TestData` 是正确方向，阻塞原因是缺少可解压、可反序列化的真实明细样本。

## 追加样本搜索证据

已在本机项目相关目录做只读搜索，仍未找到可用于 W2-1 明细字段映射验收的 legacy 明细样本。

```powershell
Get-ChildItem -Path 'D:\CODE\ATE' -Recurse -Directory -Filter 'TestData'
```

结果只发现：

- `D:\CODE\ATE\01_code\ATS\bin\Debug\TestData`
- `D:\CODE\ATE\01_code\ATS\bin\Debug\AppData\TestData`

其中 `Debug\TestData` 为自动导出 CSV/XLSX 报表目录，不含 `*-CH*.json` 明细。

```powershell
Get-ChildItem -Path 'D:\CODE' -Recurse -Filter '*-CH*.json' | Where-Object { $_.Length -gt 3 }
```

结果：未找到可读明细 JSON。

已只读打开 `D:\CODE\ATE\01_code\ATS\bin\Debug\AppData\ats.db` 检查旧明细表：

| 表 | 状态 |
| --- | --- |
| `TestData` | 不存在 |
| `TestProject` | 不存在 |
| `TestDataBackup` | 不存在 |

因此当前机器上没有可替代 `AppData\TestData` 的 legacy 明细源。

## 下一步建议

建议暂停 W2-1 后续实现，等待用户确认以下任一方案：

1. 提供或指定包含真实可读明细 JSON 的 Debug/现场运行目录。
2. 允许只把 W2-1 标记为“SQLite 索引查询部分通过、明细验证阻塞”，暂不进入 W2-2。
3. 允许先补一个只读明细样本发现脚本，在用户指定的运行目录中查找可读明细，再恢复 W2-1。

未解决前，不进入 W2-2，不启动 Flow，不启动 QA。

## Code Quality Review 返工记录

初次 Code Quality Review 结论为 Not approved，主要问题：

- `LegacyTestRecordRepository` 构造函数和公开方法缺少 null/input fail-fast。
- 测试清理失败时原先会吞异常或清理不明确。
- SQLite native interop 拷贝会写入测试输出目录，需降低重复写入和交叉影响。
- 测试中 `Assert.Equal(200, records.Count)` 对真实数据过于刚性。

已在 W2-1 范围内修复：

- 构造函数、`GetTotalCount`、`GetRecords`、`GetDetails`、`UpdateIndexInfo` 增加 `ArgumentNullException` 校验。
- 增加 null/input 保护测试。
- `GetRecords` 增加 `page < 1`、`pageSize < 1` 的 `ArgumentOutOfRangeException` 校验。
- 临时目录删除前清理 SQLite 连接池并重置文件属性，删除失败时显式抛出上下文异常。
- SQLite native 目录仅在测试输出目录不存在时复制。
- 分页记录数量断言改为 `Assert.InRange(records.Count, 1, 200)`。

返工后验证：

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter "FullyQualifiedName~LegacyTestRecordRepositoryIntegrationTests&FullyQualifiedName~RejectsNull"
```

结果：

- 通过：5
- 失败：0

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter "FullyQualifiedName~LegacyTestRecordRepositoryIntegrationTests&FullyQualifiedName~InvalidPagination"
```

结果：

- 通过：4
- 失败：0

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~LegacyTestRecordRepositoryIntegrationTests
```

结果：

- 通过：6
- 失败：1
- 唯一失败仍为 `GetDetails_MapsRealTestProjectDataFields_ForFirstRecordWithDetails`，原因仍是缺少可读真实明细样本。

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

- 通过：29
- 失败：1
- 唯一失败仍为 W2-1 明细样本缺失。

仍需确认的质量/兼容冲突：

- `GetRecords` 的 legacy 路径 `IndexInfoCore.GetIndexInfo` 内部使用字符串拼接处理 `Barcode LIKE '%{barcode}%'` 和 `ProcessName = '{processName}'`。
- 若在 WPF adapter 中禁止特殊字符，会改变原 WinForms 条码/流程名筛选行为。
- 若在 WPF adapter 中改为参数化查询，需要重写或绕过 legacy `IndexInfoCore.GetIndexInfo` 的查询实现，可能改变分页、表扫描、排序和兼容行为。
- 因此该项不能擅自修复，需用户确认：保持 legacy 行为并把输入限制放在 UI/服务层提示，还是允许 adapter 重写参数化查询以满足安全规范。

## 2026-05-21 本轮执行总控复核

本轮按执行总控提示词只复核 W2-1，未进入 W2-2，未修改生产代码，未启动 Flow、MES、Device、AutoTest。

新鲜验证命令：

```powershell
Get-ChildItem -LiteralPath 'D:\CODE\ATE\01_code\ATS\bin\Debug\AppData\TestData' -Recurse -Filter '*.json' |
    Measure-Object -Property Length -Minimum -Maximum -Sum
```

结果：

- Count：131
- Minimum：3
- Maximum：3
- Sum：393

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~LegacyTestRecordRepositoryIntegrationTests
```

结果：

- 通过：10
- 失败：1
- 唯一失败：`GetDetails_MapsRealTestProjectDataFields_ForFirstRecordWithDetails`
- 失败位置：`D:\CODE\ATE\ATS5.0\ATS5.Tests\DataQuery\LegacyTestRecordRepositoryIntegrationTests.cs:104`
- 失败原因：`Assert.NotNull() Failure`，仍未找到可读真实明细样本。

```powershell
Get-ChildItem -Path 'D:\CODE\ATE' -Recurse -Filter '*-CH*.json' -ErrorAction SilentlyContinue |
    Where-Object { $_.Length -gt 3 }
```

结果：未找到长度大于 3 字节的 legacy 明细 JSON。

```powershell
dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
```

结果：

- 0 warning
- 0 error

只读 Spec Compliance Review subagent 结论：

- W2-1 原验收点仅部分满足。
- `GetTotalCount` 和 `GetIndexInfo` 真实 SQLite 索引侧已满足。
- `TestProjectDataCore.GetTestProjectDatas` 明细 gzip JSON 字段映射未满足。
- 当前仍阻塞。
- 不允许进入 W2-2。

当前执行结论：

- W2-1 不能关闭。
- 不能进入 W2-2。
- 需要用户提供可读 legacy 明细运行目录，或确认其他验收策略。
- `Barcode/FlowName` legacy SQL 拼接与参数化安全规范冲突仍需用户确认。

## 2026-05-21 本轮扩展样本发现

本轮仍只执行 W2-1 阻塞诊断，未修改业务代码，未进入 W2-2。由于用户未提供新的运行目录，主控在本机常见目录做只读扩展搜索，范围包括：

- `D:\CODE`
- `D:\TOOL`
- `C:\Users\34434\Desktop`
- `C:\Users\34434\Documents`
- `C:\Users\34434\Downloads`
- `C:\Users\34434\OneDrive\Desktop`

只读搜索命令摘要：

```powershell
Get-ChildItem -LiteralPath $root -Recurse -Filter '*-CH*.json' -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Length -gt 3 }
```

结果：未找到长度大于 3 字节的 `*-CH*.json` legacy 明细文件。

发现的相关运行目录：

- `D:\CODE\01_code\ATS\bin\Debug\AppData\TestData`
- `D:\CODE\ATE\01_code\ATS\bin\Debug\AppData\TestData`
- `D:\CODE\ATS5.0\src\ATS.Wpf\bin\Debug\net461\AppData\TestData`

目录统计：

| 目录 | JSON 数量 | Minimum | Maximum | Sum | 结论 |
| --- | ---: | ---: | ---: | ---: | --- |
| `D:\CODE\01_code\ATS\bin\Debug\AppData\TestData` | 131 | 3 | 3 | 393 | 只有 gzip 头，不能作为明细验收 |
| `D:\CODE\ATE\01_code\ATS\bin\Debug\AppData\TestData` | 131 | 3 | 3 | 393 | 只有 gzip 头，不能作为明细验收 |
| `D:\CODE\ATS5.0\src\ATS.Wpf\bin\Debug\net461\AppData\TestData` | 0 | - | - | - | 无明细 JSON |

另外发现 `D:\CODE\01_code\ATS\bin\Debug\TestData` 与 `D:\CODE\ATE\01_code\ATS\bin\Debug\TestData` 各有 5 个 `.csv`、29 个 `.xlsx`，该目录对应自动导出报表，不是 `TestProjectDataCore.GetTestProjectDatas` 的 legacy 明细读取源。

本轮最小验证：

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~LegacyTestRecordRepositoryIntegrationTests
```

结果：

- 通过：10
- 失败：1
- 唯一失败：`GetDetails_MapsRealTestProjectDataFields_ForFirstRecordWithDetails`
- 失败原因：`Assert.NotNull() Failure`，仍没有可读真实明细样本。

本轮结论：

- W2-1-Q1 未解除。
- W2-1 仍不能关闭。
- 仍不允许进入 W2-2。

只读复核 subagent 结论：

- 已复核 `PathTestData`、`GetTestProjectDatas`、`ReadJsonFileEx`、`WriteJsonFileEx`、`ProductTestCore` 写入路径。
- 已在 `D:\CODE`、`D:\TOOL`、`D:\Knowledge`、桌面、文档、下载、用户 `source/Source`、用户 `AppData Local/Roaming` 等位置做只读扫描。
- 严格 legacy 文件名 `^\d{14}-CH\d+\.json$` 且长度大于 3 字节的候选数为 0。
- 严格 `AppData\TestData\yyyy\M\d\{LogGuid}-CH{Channel}.json` 布局候选数为 0。
- W2-1-Q1 不能解除。
- 不允许进入 W2-2。

## 2026-05-21 W2-1 seeded gzip 明细链路补验

本轮按执行总控只在 W2-1 范围内补强验证，不修改生产 adapter，不修改 `D:\CODE\ATE\01_code` 原始运行数据，不进入 W2-2。

变更范围：

- `D:\CODE\ATE\ATS5.0\ATS5.Tests\DataQuery\LegacyTestRecordRepositoryIntegrationTests.cs`

验证策略：

- 保持 `LegacyTestRecordRepository.GetDetails` 生产代码不变。
- 继续通过 legacy `TestProjectDataCore.GetTestProjectDatas` 读取明细。
- 在测试复制出来的 `%TEMP%\ATS5.DataQuery.*` runtime 目录中，按 legacy 路径 `AppData\TestData\yyyy\M\d\{LogGuid}-CH{Channel}.json` 写入一份 gzip JSON 明细。
- 该 JSON 使用 legacy `TestProjectDataInfo` 可反序列化的字段结构：`LstTestProject` 与 `LstTestData`。
- 只验证 adapter 到 legacy 明细读取/字段映射链路，不把该测试解释为真实现场明细已验收。

验证命令：

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~LegacyTestRecordRepositoryIntegrationTests
```

结果：

- 通过：11
- 失败：0
- 跳过：0

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

- 通过：34
- 失败：0
- 跳过：0

Spec Compliance Review 结论：

- Approved。
- SQLite 索引查询与 seeded gzip 明细链路补验可通过。
- 真实现场明细样本仍缺失，不能宣称真实现场明细字段映射已完全验收。
- 允许 W2-1 在“真实现场明细待补验”的条件下进入下一小任务。
- W2-2 不得依赖“真实明细字段映射已完全验收”的结论。

当前 W2-1 精确状态：

- `GetTotalCount` 真实 SQLite 查询：通过。
- `GetIndexInfo` 真实 SQLite 分页：通过。
- `GetTestProjectDatas` legacy gzip 读取链路：seeded runtime 补验通过。
- 真实现场 gzip JSON 明细：仍待用户提供可读样本后补验。
- `Barcode/FlowName` 查询：仍保持 WinForms legacy 行为，未擅自参数化重写。

## 2026-05-21 W2-1 Code Quality Review 返工

初次 Code Quality Review 结论为 Not approved，问题包括：

- 测试强依赖本机绝对 legacy 路径和固定数据窗口，缺少显式 integration/parity 标识。
- 当前仓库存在大量 untracked 文件，Git 视角无法证明本轮只改测试文件。
- 临时 runtime 创建过程中若复制失败，可能残留 `%TEMP%\ATS5.DataQuery.*`。
- seeded legacy JSON 字段值和分页参数过多散落在测试逻辑中。

已在 W2-1 测试范围内返工：

- 添加 `Trait("Category", "LegacyIntegration")` 和 `Trait("Category", "DataQueryParity")`。
- 新增 `ATS5_LEGACY_RUNTIME_ROOT` 环境变量，可覆盖默认 `D:\CODE\ATE\01_code\ATS\bin\Debug`。
- 抽取页码、分页大小、查询条件、seeded 明细字段值为命名常量。
- `CopiedDebugRuntime.Create()` 增加失败清理逻辑，复制或 SQLite interop 准备失败时删除已创建的临时目录。
- 生产 `LegacyTestRecordRepository` 仍未修改。

返工后验证：

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~LegacyTestRecordRepositoryIntegrationTests
```

结果：

- 通过：11
- 失败：0
- 跳过：0

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

- 通过：34
- 失败：0
- 跳过：0

剩余边界：

- 仓库内 WPF 样板文件整体仍为 untracked，提交/归档时必须按任务范围选择文件。
- 真实现场 gzip JSON 明细样本仍待补验。
- `Barcode/FlowName` 仍保持 legacy 行为，安全参数化策略未切换。

## 2026-05-21 W2-1 未跟踪文件审计边界

Code Quality Re-review 仍为 Not approved，剩余 Important 问题为 Git 未跟踪范围风险：当前 WPF 样板工程整体尚未纳入版本控制，无法通过 `git diff` 证明生产代码相对主线未变化。

只读核查命令：

```powershell
git status --short -- `
  'ATS5.Infrastructure.LegacyAdapters/DataQuery/LegacyTestRecordRepository.cs' `
  'ATS5.Tests/DataQuery/LegacyTestRecordRepositoryIntegrationTests.cs' `
  'ATS5.Tests/ATS5.Tests.csproj' `
  'docs/winform-to-wpf-migration/23-wave2-w2-1-blocker-report.md'
```

结果：

- `?? ATS5.Infrastructure.LegacyAdapters/DataQuery/LegacyTestRecordRepository.cs`
- `?? ATS5.Tests/ATS5.Tests.csproj`
- `?? ATS5.Tests/DataQuery/LegacyTestRecordRepositoryIntegrationTests.cs`
- `?? docs/winform-to-wpf-migration/23-wave2-w2-1-blocker-report.md`

```powershell
git ls-files -- `
  'ATS5.Infrastructure.LegacyAdapters/DataQuery/LegacyTestRecordRepository.cs' `
  'ATS5.Tests/DataQuery/LegacyTestRecordRepositoryIntegrationTests.cs' `
  'ATS5.Tests/ATS5.Tests.csproj'
```

结果：空，说明上述文件当前均未被 Git 跟踪。

当前文件 SHA256：

| 文件 | SHA256 |
| --- | --- |
| `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\DataQuery\LegacyTestRecordRepository.cs` | `238C1AE4FBDE5D9C0CBB9CCD2FB695EE82E144D89D944ED3263815E3E69FED53` |
| `D:\CODE\ATE\ATS5.0\ATS5.Tests\DataQuery\LegacyTestRecordRepositoryIntegrationTests.cs` | `8BB8C9FA16B3107DE82DAE362F0212655295E66BAA1EECAA7CF7AE2A21D4999E` |
| `D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj` | `B1D966F9E7169D7B6905A2F31A7A3DC3A32D627CFDC462757D8151590E13516F` |

W2-1 当前纳入范围：

- `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\DataQuery\LegacyTestRecordRepository.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\DataQuery\LegacyTestRecordRepositoryIntegrationTests.cs`
- `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\23-wave2-w2-1-blocker-report.md`

W2-1 当前排除范围：

- `D:\CODE\ATE\01_code` 全部原 WinForms/legacy 代码和运行数据。
- Flow、MES、Device、AutoTest、ProductTest、Shell。
- `ATS5.Tests\ATS5.Tests.csproj` 本轮未修改，仅因项目整体未跟踪而列入审计风险。

门禁结论：

- 在 WPF 样板工程未纳入版本控制前，Code Quality Review 无法完全消除“生产代码是否被本轮改动”的审计风险。
- 继续下一小任务前，需要用户确认是否接受当前 untracked 样板工程状态，或先进行版本控制基线收敛。

## 2026-05-21 W2-1 intent-to-add 审计收敛

为处理 Code Quality Review 中的 untracked 审计风险，本轮只对 W2-1 相关文件执行 `git add -N`，即 intent-to-add。该操作不提交、不正式暂存内容，但允许 `git diff` 展示新增文件全文，便于 subagent 和主控审查。

执行命令：

```powershell
git add -N -- `
  'ATS5.Infrastructure.LegacyAdapters/DataQuery/LegacyTestRecordRepository.cs' `
  'ATS5.Tests/DataQuery/LegacyTestRecordRepositoryIntegrationTests.cs' `
  'ATS5.Tests/ATS5.Tests.csproj' `
  'docs/winform-to-wpf-migration/23-wave2-w2-1-blocker-report.md'
```

审计状态：

```powershell
git status --short -- `
  'ATS5.Infrastructure.LegacyAdapters/DataQuery/LegacyTestRecordRepository.cs' `
  'ATS5.Tests/DataQuery/LegacyTestRecordRepositoryIntegrationTests.cs' `
  'ATS5.Tests/ATS5.Tests.csproj' `
  'docs/winform-to-wpf-migration/23-wave2-w2-1-blocker-report.md'
```

结果：

- ` A ATS5.Infrastructure.LegacyAdapters/DataQuery/LegacyTestRecordRepository.cs`
- ` A ATS5.Tests/ATS5.Tests.csproj`
- ` A ATS5.Tests/DataQuery/LegacyTestRecordRepositoryIntegrationTests.cs`
- ` A docs/winform-to-wpf-migration/23-wave2-w2-1-blocker-report.md`

```powershell
git diff --stat -- `
  'ATS5.Infrastructure.LegacyAdapters/DataQuery/LegacyTestRecordRepository.cs' `
  'ATS5.Tests/DataQuery/LegacyTestRecordRepositoryIntegrationTests.cs' `
  'ATS5.Tests/ATS5.Tests.csproj' `
  'docs/winform-to-wpf-migration/23-wave2-w2-1-blocker-report.md'
```

结果：

- `LegacyTestRecordRepository.cs`：160 行新增。
- `ATS5.Tests.csproj`：20 行新增。
- `LegacyTestRecordRepositoryIntegrationTests.cs`：393 行新增。
- `23-wave2-w2-1-blocker-report.md`：539 行新增。
- 合计：4 个文件，1112 行新增。

验证命令：

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~LegacyTestRecordRepositoryIntegrationTests
```

结果：

- 通过：11
- 失败：0
- 跳过：0

当前边界：

- 上述 4 个文件已具备 `git diff` 审计能力。
- 仍未提交任何文件。
- 仍未修改 `D:\CODE\ATE\01_code`。
- 仍未进入 W2-2。
