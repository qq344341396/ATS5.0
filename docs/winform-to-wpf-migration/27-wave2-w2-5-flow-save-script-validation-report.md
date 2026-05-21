# Wave 2 W2-5 流程保存测试副本和脚本校验门禁报告

更新时间：2026-05-21
执行范围：仅 Flow 保存测试副本、设备配置门禁、脚本校验门禁

## 原 WinForms 基线

- 原窗体：`D:\CODE\ATE\01_code\ATS\Flow\UcFlow.cs`
- 保存入口：`btnSave_ItemClick`
- 全流程脚本校验：`AllCheckCode`
- 关键保存顺序：无测试项目信息 -> 设备配置名为空 -> `.dev` 不存在 -> `.dev` 空文件 -> `AllCheckCode()` -> 写流程文件。
- `AllCheckCode` 关键行为：只校验启用项目；重复启用项目名直接提示；保存前对空 `ComparisonOperator` 补默认值；输出项和临时变量做确定性数据校验；确定性校验通过后才写临时 `.fw` 并调用 `ProductTestCore.MakeUpCode/CompileCode`。

## 本轮完成内容

- `FlowEditorService.Save(...)` 在样板阶段统一拒绝，避免 public SaveAs 入口绕过样板副本名保护覆盖源 `.fw`。
- `FlowEditorService.SaveSampleCopy(...)` 只允许写 `{sourceFlowName}-WpfSample-*` 形式的测试副本。
- `SaveSampleCopy` 使用源流程名执行脚本校验，写入时使用测试副本名。
- 设备配置门禁文案对齐 WinForms：无项目、设备配置名为空、`.dev` 不存在、`.dev` 空文件。
- 校验失败不写文件、不发布保存消息。
- `LegacyFlowScriptValidator` 在进入真实 `ProductTestCore` 编译前补齐确定性门禁：重复启用项目名、输出/临时变量底层命名、输出与临时变量重复、double/double[] 数值和长度、string 最大/最小值一致性。
- `LegacyFlowScriptValidator` 使用原 `ATSCommon.Checker.IsValidVariableName`，不重新定义变量命名规则。
- `ComparisonOperator` 默认值按 WinForms 保存前校验补齐：`string/var -> ==,&&,==`，`double/double[] -> >=,&&,<=`，禁用输出项也会先补默认值再跳过校验。

## 修改文件

| 文件 | 说明 |
| --- | --- |
| `D:\CODE\ATE\ATS5.0\ATS5.Application\Flow\FlowEditorService.cs` | 保护样板保存入口、限制 public `Save`、确保保存测试副本顺序 |
| `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\Flow\LegacyFlowScriptValidator.cs` | 补 WinForms `AllCheckCode` 确定性门禁和默认比较符语义 |
| `D:\CODE\ATE\ATS5.0\ATS5.Tests\Flow\FlowEditorServiceTests.cs` | 覆盖设备配置门禁、保存测试副本名保护、校验失败不写 |
| `D:\CODE\ATE\ATS5.0\ATS5.Tests\Flow\LegacyFlowScriptValidatorTests.cs` | 覆盖重复项目名、确定性校验文案、默认比较符补齐 |

## TDD 和修复证据

- 初始红灯：`SaveSampleCopy` 校验时使用测试副本名而非源流程名；校验失败路径同样流程名不对；仓储返回源名时仍可能保存成功。
- Spec Review 第一次阻塞后红灯：public `Save(..., FlowSaveMode.SaveAs)` 可写源流程名；legacy validator 漏掉输出/临时变量确定性门禁。
- Spec Review 第二次阻塞后红灯：禁用输出项没有按 WinForms 顺序补默认 `ComparisonOperator`。
- 修复后目标测试从 W2-4 后的 12 个相关测试增长到 20 个相关测试，全部通过。

## 验证命令和结果

| 命令 | 结果 |
| --- | --- |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter "FullyQualifiedName~FlowEditorServiceTests|FullyQualifiedName~LegacyFlowScriptValidatorTests"` | 20 passed, 0 failed |
| `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 成功，0 警告，0 错误 |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 71 passed, 0 failed |

## 数据兼容结果

- 本轮不写真实 legacy `SysCache\Flows`，不覆盖原 `.fw`。
- 样板保存只允许生成 `{sourceFlowName}-WpfSample-*` 测试副本名。
- 保存路径、旧模型、旧 JSON 格式仍由 legacy repository/`JsonHelper` 负责；本轮未改变字段名、编码或旧文件结构。
- `ComparisonOperator` 默认值补齐行为按 WinForms 保存前校验保留，属于旧保存语义的一部分。

## 日志验证结果

- 本轮不替换日志框架。
- 本轮未新增或改变 `LogHelper`/log4net 输出位置、格式、等级。
- 覆盖保存和脚本校验门禁时没有引入新的日志调用；完整覆盖保存日志属于后续非样板覆盖保存任务。

## UI/交互验证结果

- 本轮仅实现 service/adapter 层保存测试副本门禁，不修改 Flow View/ViewModel/XAML。
- W2-6 仍负责流程编辑页面按钮状态、打开后 UI 填充和保存命令接入行为。
- public `Save(...)` 统一拒绝是样板阶段边界，避免误用覆盖保存入口。

## 设备/MES/自动测试影响

- 本轮只读取设备配置名并通过 repository 检查 `.dev` 是否存在且非空，不初始化设备。
- 未触达 `ATSMes`、`ATSAutoTest`、`ATSDevice` 或 ProductTest 主流程执行。
- `LegacyFlowScriptValidator` 仍保留 legacy `ProductTestCore.MakeUpCode/CompileCode` 编译路径，但新增测试只覆盖确定性门禁，避免依赖真实编译成功。

## Subagent 评审

| 评审 | 结论 | 说明 |
| --- | --- | --- |
| Spec Compliance Review 第一次 | Blocked | 指出 public `SaveAs` 可绕过测试副本名保护、legacy validator 漏 WinForms 确定性门禁 |
| Spec Compliance Review 第二次 | Blocked | 指出禁用输出项也应先补默认 `ComparisonOperator` 再跳过校验 |
| Spec Compliance Review 第三次 | Approved | 确认样板副本保护、保存顺序、确定性门禁和默认比较符顺序符合 W2-5 |
| Code Quality Review | Approved | 无阻塞问题；确认 Application 依赖抽象，legacy 实现在 adapter/tests，测试避开不稳定编译成功路径 |

## 非阻塞维护性提示

- `LegacyFlowScriptValidator` 构造函数可后续补 `ArgumentNullException` fail-fast，与其他 adapter 风格保持一致。
- legacy 数据类型和比较符 literal 当前集中在 adapter 且为复刻 WinForms 所需；后续若继续扩展，可提取私有常量降低漂移。
- `LegacyFlowScriptValidatorTests` 使用本机真实 Debug root 是 adapter 契约的一部分，当前测试避免依赖真实 `ProductTestCore` 编译成功；若未来要 CI 化，需要设计可复制的 legacy runtime fixture。

## 未解决问题和边界

- 本轮没有穷尽 WinForms `AllCheckCode` 的所有潜在分支，只覆盖 W2-5 保存门禁所需的确定性高风险规则和默认比较符语义。
- 本轮不实现覆盖保存、另存为正式流程、备份文件、完整保存日志。
- 本轮不接入流程编辑 UI 状态，W2-6 才验证按钮和 ViewModel 命令。
- W2-1 真实现场 gzip JSON 明细样本缺失问题仍按既有 open question 跟踪。

## 结论

W2-5 流程保存测试副本和脚本校验门禁已完成本轮范围，已通过目标测试、全量构建、全量测试、Spec Compliance Review 和 Code Quality Review。下一步候选为 W2-6 流程 ViewModel 和 XAML 状态门禁，开始前仍需确认本轮任务范围，禁止自动扩大到产品测试、GP12、设备、自动测试或完整正式保存行为。
