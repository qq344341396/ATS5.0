# Wave 2 W2-6 流程 ViewModel 和 XAML 状态门禁报告

更新时间：2026-05-21
执行范围：仅 FlowEditor ViewModel 命令状态、打开后绑定状态、保存测试副本入口、XAML 未完成操作禁用门禁

## 原 WinForms 基线

- 原窗体：`D:\CODE\ATE\01_code\ATS\Flow\UcFlow.cs`
- 原 Designer：`D:\CODE\ATE\01_code\ATS\Flow\UcFlow.Designer.cs`
- 关键方法：`SetUI`、`OpenFlow`、保存入口、脚本校验入口。
- 本轮只锁定样板 UI 状态：未打开流程前保存和脚本校验不可执行；打开流程后展示项目、首项目、设备配置名；未完成的编辑/导入/导出/移动/复制/格式化等操作继续禁用。

## 本轮完成内容

- `FlowEditorViewModel` 构造函数增加 `FlowEditorService` 空值门禁。
- `SaveCommand` 和 `CheckScriptsCommand` 通过 `CurrentFlow != null` 控制可执行状态，未打开流程时不可执行。
- `OpenCommand` 调用 `FlowEditorService.Open` 后填充 `CurrentFlow`、`DevConfigName`、`Projects`，并选择第一个项目。
- `SaveCommand` 只调用 `FlowEditorService.SaveSampleCopy`，仍不启用正式覆盖保存、另存为或备份行为。
- `Open`、`CheckScripts`、`Save` 不捕获顶层 `Exception`，避免把系统异常详情写到用户消息区。
- XAML 保留流程编辑样板结构：顶部工具条、左侧测试项目、项目脚本页、右侧设备控制区、输出项、临时变量、底部 DBC/UDS/公共变量页签。
- 未完成按钮继续禁用：新建、编辑、导出、导入、另存为、取消、编辑公共变量、修改流程名称、格式化、导入设备、输出/临时变量增删改移动复制、项目工具条占位按钮。
- 输出项和临时变量表格通过共享 `GridStyle` 保持只读。
- 项目启用 `CheckBox` 保持禁用，只允许查看，不启用隐藏编辑路径。
- 结构测试强化为同名按钮全部禁用：`新增/删除/上移/下移/复制` 同时覆盖输出项和临时变量两个工具条。

## 修改文件

| 文件 | 说明 |
| --- | --- |
| `D:\CODE\ATE\ATS5.0\ATS5.Modules.Flow\ViewModels\FlowEditorViewModel.cs` | 接入打开、脚本校验、保存测试副本命令状态；移除顶层异常吞吐；构造参数 fail-fast |
| `D:\CODE\ATE\ATS5.0\ATS5.Modules.Flow\Views\FlowEditorView.xaml` | 锁定流程编辑样板结构、未完成按钮禁用、输出/临时变量表格只读、项目启用项只读 |
| `D:\CODE\ATE\ATS5.0\ATS5.Tests\Flow\FlowEditorViewModelTests.cs` | 覆盖命令可执行状态、打开后绑定状态、保存测试副本、异常传播和构造参数门禁 |
| `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\FlowEditorViewStructureTests.cs` | 覆盖 XAML 结构、禁用按钮、只读表格、项目启用项只读 |
| `D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj` | 测试项目直接引用 `ATS5.Modules.Flow`，移除 brittle bin/reflection 路径需求 |

## TDD 和修复证据

- 初始目标测试覆盖：未打开流程时保存和脚本校验命令不可执行。
- 初始目标测试覆盖：打开流程后 `CurrentFlow`、设备配置名、项目集合、首项目选择正确。
- 初始目标测试覆盖：保存只写 `{sourceFlowName}-WpfSample-*` 样板副本，不写源流程名。
- Code Quality Review 第一次阻塞后补强：不再捕获顶层 `Exception`，异常不写入 `Message`；测试改为直接项目引用；项目启用 `CheckBox` 禁用。
- Code Quality Review 第二次阻塞后补强：XAML 结构测试不再只检查“任意一个同名按钮禁用”，而是检查所有同名按钮均禁用；输出项和临时变量的重复按钮至少各覆盖两个。

## 验证命令和结果

| 命令 | 结果 |
| --- | --- |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter "FullyQualifiedName~FlowEditorViewModelTests|FullyQualifiedName~FlowEditorViewStructureTests"` | 9 passed, 0 failed |
| `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 成功，0 警告，0 错误 |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 80 passed, 0 failed |

## 数据兼容结果

- 本轮不写真实 `SysCache\Flows`，不覆盖原 `.fw`。
- 保存命令仍只走 W2-5 已锁定的 `SaveSampleCopy` 测试副本路径。
- 未改变 `.fw`、`.dev`、SQLite、gzip JSON、Excel/CSV 或配置文件格式、路径、编码、字段、默认值、时间格式。

## 日志验证结果

- 本轮不替换日志框架。
- 本轮未新增或改变 `LogHelper`/log4net 输出位置、格式、等级。
- 异常路径不再把系统异常详情写入 `Message`；具体日志策略仍按后续正式流程编辑保存/执行任务处理。

## UI/交互验证结果

- 未打开流程时保存和脚本校验不可执行。
- 打开流程后显示项目、首项目和设备配置名。
- 顶部工具条、左项目区、项目脚本区、右设备方法区、输出项、临时变量和底部页签结构已由 XAML 结构测试锁定。
- 未完成的编辑类按钮继续禁用，避免把样板 UI 误验收为完整流程编辑器。
- 输出项、临时变量、项目启用勾选均保持只读，不开放隐藏编辑路径。
- 本轮没有执行 W2-7 的 UI 自动化 smoke；运行时截图/人工验收清单仍在 W2-7 范围。

## 设备/MES/自动测试影响

- 本轮未触达 `ATSMes`、`ATSAutoTest`、`ATSDevice` 或 ProductTest 主流程。
- 本轮不初始化设备、不关闭设备、不改变设备互斥、不触发 MES 上报、不执行自动测试。
- 右侧设备控制树仍是样板展示结构，导入设备和设备方法执行入口保持禁用。

## Subagent 评审

| 评审 | 结论 | 说明 |
| --- | --- | --- |
| Spec Compliance Review 第一次 | Approved | 确认命令状态、打开后绑定、保存测试副本、XAML 结构、未完成按钮禁用、只读表格和项目启用只读符合 W2-6 |
| Code Quality Review 第一次 | Blocked | 指出重复按钮文案只检查任意一个禁用，不能可靠锁定所有同名按钮状态；另提示保存测试名容易误解 |
| Code Quality Review 复审 | Approved | 确认同名按钮全部禁用门禁已补强，测试名已澄清，无 Critical/Important/Minor |
| Spec Compliance Review 复审 | Approved | 确认测试补强不偏离 W2-6 范围，未进入 W2-7 或正式保存 |

## 非阻塞维护性提示

- `FlowEditorViewStructureTests` 当前通过 XML/字符串结构锁定 XAML 状态，不等同于运行时 UI 自动化；W2-7 需要补 smoke 和人工验收清单。
- `FlowEditorViewStructureTests` 使用向上查找 solution root 的方式定位 XAML，当前避免了 hardcoded `bin\Debug`，后续若结构测试继续扩大，可考虑统一测试根目录 fixture。

## 未解决问题和边界

- 本轮不实现流程正式覆盖保存、另存为、备份和完整保存日志。
- 本轮不实现项目新增/编辑/删除/移动/复制、输出项编辑、临时变量编辑、格式化、导入设备、设备方法插入或公共变量编辑。
- 本轮不启动 W2-7 UI 自动化 smoke，不做截图验收。
- W2-1 真实现场 gzip JSON 明细样本缺失问题仍按既有 open question 跟踪。
- W2-5 未穷尽 `AllCheckCode` 所有潜在分支的问题仍按既有 open question 跟踪。

## 结论

W2-6 流程 ViewModel 和 XAML 状态门禁已完成本轮确认范围，已通过目标测试、全量构建、全量测试、Spec Compliance Review 和 Code Quality Review。下一步候选为 W2-7 UI 自动化 smoke 和人工验收清单，开始前仍需确认本轮任务范围，禁止自动扩大到产品测试、GP12、设备、自动测试、完整正式保存或全量迁移。
