# Wave 2 W2-4 流程 .fw 多样本打开和字段往返兼容报告

更新时间：2026-05-21
执行范围：仅 Flow `.fw` 打开、转换、序列化字段兼容验证

## 原 WinForms 基线

- 原窗体：`D:\CODE\ATE\01_code\ATS\Flow\UcFlow.cs`
- 打开入口：`OpenFlow(string name, bool isDetail = false)`
- 旧读取方式：`JsonHelper.GetJsonFile($"{SysCache.PathFlows}{name}.fw").ToObject<ATSModel.Flow>()`
- 打开后行为：填充项目树、项目启用状态、DBC/UDS、全局变量、设备配置标签，并调用 `SetUI()`
- 旧模型：`D:\CODE\ATE\01_code\ATSModel\Flow.cs`
- 真实样本目录：`D:\CODE\ATE\01_code\ATS\bin\Debug\SysCache\Flows`

## 样本覆盖

本轮 W2-4 使用真实 Debug `.fw` 样本，测试要求七个优先样本全部存在，缺一即失败：

| 样本 | 覆盖结果 |
| --- | --- |
| `测试1.fw` | 已覆盖 |
| `测试2.fw` | 已覆盖 |
| `超充重卡PDU-临工（三支路）.fw` | 已覆盖 |
| `大秦800Y-BMS.fw` | 已覆盖 |
| `徐工528子母车 扩展帧-EOL测试.fw` | 已覆盖 |
| `DCDC-EOL测试.fw` | 已覆盖 |
| `Maserati M189 成品测试（主控）-通道2.fw` | 已覆盖 |

## 本轮完成内容

- 新增 `LegacyFlowRepositoryCompatibilityTests`，通过反射调用原 `ATSCommon.JsonHelper.GetJsonFile(...).ToObject<ATSModel.Flow>()` 作为验收基线。
- 覆盖 `Open` 映射：`DevCfgName`、`MESParamName`、`Reserve2`、项目数量和顺序、启用状态、脚本、输出项、临时变量、全局变量、DBC、UDS。
- 覆盖 `Open -> Serialize -> legacy ToObject<ATSModel.Flow>` 结构化语义往返，不要求 JSON 字节级一致，但要求旧模型可读且关键字段不丢。
- 保留真实 corpus 中已证明可为 `null` 的字段语义，避免 WPF DTO 把旧 `null` 写成空字符串。
- 本轮未调用 `WriteFlow` 和 `WriteBackup`，没有写回真实 `SysCache\Flows`。

## 修改文件

| 文件 | 说明 |
| --- | --- |
| `D:\CODE\ATE\ATS5.0\ATS5.Tests\Flow\LegacyFlowRepositoryCompatibilityTests.cs` | 新增真实 `.fw` corpus 打开和序列化往返兼容测试 |
| `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\Flow\LegacyFlowRepository.cs` | 保持 legacy `JsonHelper`/`ATSModel.Flow` 转换路径，补齐 nullable 字段映射 |
| `D:\CODE\ATE\ATS5.0\ATS5.Application\Flow\ProcessFlowDefinition.cs` | `MesParamName` 保留 legacy nullable 语义 |
| `D:\CODE\ATE\ATS5.0\ATS5.Application\Flow\ProcessOutputDefinition.cs` | `Length`、`Unit`、`ComparisonOperator` 保留 legacy nullable 语义 |
| `D:\CODE\ATE\ATS5.0\ATS5.Application\Flow\ProcessVariableDefinition.cs` | `TempUnit`、`TempRemark` 保留 legacy nullable 语义 |

## TDD 证据

- 红灯测试首次失败点为 legacy `null` 语义丢失：期望 `null`，实际为 `""`。
- 根因是 adapter 对真实 corpus 中可为 `null` 的字段使用了 `?? string.Empty`。
- 修复策略只放宽真实 corpus 证明可空的字段，不扩大到 `DevCfgName`、`ProjectName`、`Script`、DBC/UDS 文件名等核心非空展示字段。

## 验证命令和结果

| 命令 | 结果 |
| --- | --- |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~LegacyFlowRepositoryCompatibilityTests` | 2 passed, 0 failed |
| `dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln` | 所有项目均是最新的，无法还原 |
| `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 成功，0 警告，0 错误 |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 58 passed, 0 failed |

## 数据兼容结果

- 旧 WinForms `.fw` 文件由 WPF adapter 通过原 `JsonHelper` 和原 `ATSModel.Flow` 语义读取。
- WPF 序列化结果可再次被 legacy `JsonHelper.ToObject<ATSModel.Flow>()` 读取。
- 本轮验证的是结构化字段语义一致，不要求 JSON 文本字节级完全一致。
- 本轮不写真实流程文件，不覆盖 `.fw`，不改变保存路径、编码、字段名或旧模型结构。

## 日志验证结果

- 本轮不替换日志框架。
- 本轮未新增运行时日志路径，也未改变 `LogHelper`/log4net 语义。
- 流程打开失败消息仍由 adapter 返回给上层，W2-5/W2-6 再验证 UI 弹窗和日志记录边界。

## UI/交互验证结果

- 本轮只验证 `.fw` 打开和字段往返兼容，不声明流程编辑 UI 已完成。
- WinForms 打开后项目树、DBC/UDS、公共变量、设备配置标签等 UI 填充行为已作为 W2-4 字段覆盖依据。
- 流程编辑 ViewModel/XAML 状态、按钮启用、脚本页和设备控制区仍属于 W2-6。

## 设备/MES/自动测试影响

- 本轮不触达 `ATSDevice`、`ATSMes`、`ATSAutoTest` 或 ProductTest。
- 仅读取流程定义中的设备配置名、MES 参数名、DBC/UDS 文件名等字段，不初始化设备、不上传 MES、不执行自动测试。
- 设备配置存在性、脚本校验和保存测试副本属于 W2-5。

## Subagent 评审

| 评审 | 结论 | 说明 |
| --- | --- | --- |
| Spec Compliance Review | Approved | 确认七个真实 `.fw` 样本全部覆盖，打开映射和序列化往返字段符合 W2-4 计划 |
| Code Quality Review | Approved | 无阻塞问题；确认 Application DTO 不依赖 legacy 程序集，legacy 引用留在 adapter/tests |

## 非阻塞维护性提示

- `LegacyFlowRepositoryCompatibilityTests` 的 `AssemblyResolve` 为进程级事件，当前测试场景可接受；后续可考虑 scoped cleanup 提升隔离性。
- 部分 helper 构造了 `context`，但内层 `Assert.Equal` 失败不会直接打印上下文；后续若 corpus 扩大，可优化断言消息便于定位。

## 未解决问题和边界

- W2-4 无新增阻塞问题。
- W2-4 不覆盖保存、备份、脚本校验、设备配置文件存在性和流程 UI 状态。
- W2-1 真实现场 gzip JSON 明细样本缺失问题仍按既有 open question 跟踪，不由 W2-4 解决。

## 结论

W2-4 流程 `.fw` 多样本打开和字段往返兼容已完成本轮范围，已通过目标测试、全量构建、全量测试、Spec Compliance Review 和 Code Quality Review。下一步候选为 W2-5 流程保存测试副本和脚本校验门禁，开始前仍需确认本轮任务范围，禁止自动扩大到产品测试、GP12、设备、自动测试或完整流程编辑器高级行为。
