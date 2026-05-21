# Wave 3 WP-08 ProductTest Shell And Barcode Dialog Report

更新时间：2026-05-21
状态：WP-08 产品测试壳和扫码弹窗已完成最小实现、代码侧验证、Spec Compliance Review 和 Code Quality Review，且双评审 re-review 均通过；未进入产品测试执行链、真实设备初始化、MES 上传、GP12、AutoType 9、ATSMes/ATSAutoTest/ATSDevice 全量迁移

## 1. 本阶段完成内容

- 迁移产品测试壳最小样板：Shell 导航入口、按 `ChannelNum` 动态通道、通道标题状态规则、产品测试工具栏外观、测试信息/消息/当前测试/系统消息区域。
- 迁移扫码弹窗基础行为：流程文件、单通道条码数量、动态条码框、回车跳焦、最后一个条码回车确认、确定/取消、必填校验、取消 `status` 语义。
- 扫码弹窗流程文件列表通过 legacy adapter 读取 `SysCache.PathFlows` 下 `.fw` 文件名；条码数量支持 1..10 动态调整并重建条码输入框。
- 可编辑流程下拉绑定手输文本；legacy 流程目录读取处理可预期 I/O 异常并安全返回空列表。
- 保持 WP-08 范围为 UI shell 和 dialog 行为，不实例化 `ProductTestCore`，不启动设备、MES、自动化或真实测试线程。
- 新增 ProductTest Prism module，并接入 WPF Shell 的 `NavProductTest` 导航。
- 增加 ViewModel 测试、XAML 结构测试和禁区扫描测试。

## 2. 修改文件列表

新增/修改文件：

- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\ATS5.Modules.ProductTest.csproj`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\ProductTestModule.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\ViewModels\BarcodeDialogRequest.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\ViewModels\BarcodeDialogResult.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\ViewModels\BarcodeDialogViewModel.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\ViewModels\BarcodeEnterAction.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\ViewModels\BarcodeInputViewModel.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\ViewModels\IBarcodeDialogService.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\ViewModels\ProductTestChannelViewModel.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\ViewModels\ProductTestShellStatus.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\ViewModels\ProductTestShellViewModel.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\Views\BarcodeDialog.xaml`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\Views\BarcodeDialog.xaml.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\Views\BarcodeDialogService.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\Views\ProductTestShellView.xaml`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\Views\ProductTestShellView.xaml.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTestShell\IProductTestFlowCatalog.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\ProductTestShell\LegacyProductTestFlowCatalog.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\ProductTestShell\ProductTestShellViewModelTests.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\ProductTestShellViewStructureTests.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf\App.xaml.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf\ViewModels\ShellViewModel.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf\Views\Shell.xaml`
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf\ATS5.Wpf.csproj`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj`
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln`

## 3. 对应原 WinForms 功能

原 WinForms 基线：

- `D:\CODE\ATE\01_code\ATS\ProductTest\UcTestMain.cs`
- `D:\CODE\ATE\01_code\ATS\ProductTest\UcTestMain.Designer.cs`
- `D:\CODE\ATE\01_code\ATS\ProductTest\UcProductTest.cs`
- `D:\CODE\ATE\01_code\ATS\ProductTest\UcProductTest.Designer.cs`
- `D:\CODE\ATE\01_code\ATS\ProductTest\FrmBarcode.cs`
- `D:\CODE\ATE\01_code\ATS\ProductTest\FrmBarcode.Designer.cs`
- `D:\CODE\ATE\01_code\ATS\ProductTest\FrmFlowInfo.cs`
- `D:\CODE\ATE\01_code\ATS\ProductTest\FrmFlowInfo.Designer.cs`

已对齐行为：

- `UcTestMain` 的动态通道壳：读取 `ChannelNum` 后创建通道页；WP-08 测试覆盖 `ChannelNum=2` 生成 `通道1`、`通道2`。
- 通道标题规则：`Start` -> `通道N【Testing】`；`Complete` 且所有结果 PASS 且 MES 结果 true -> `通道N【PASS】`；否则 `通道N【FAIL】`；`Stop` -> `通道N【FAIL】`。
- `UcProductTest` 工具栏文字：`执行`、`暂停`、`停止`、`清除报警`、`【流程：】`、`导出`、`CAN监控`。
- 页面区域：`测试信息`、`消息`、`当前测试`、`系统消息`，测试表列包含 `测试项目`、`测试点`、`数据类型`、`下限`、`上限`、`单位`、`测试值`、`结果`、`判定符号`。
- `FrmBarcode` 弹窗文字：`通道：{ch} 条码输入`、`流程文件`、`单通道条码数量`、`确定`、`取消`。
- 扫码回车行为：非最后一个条码框回车切到下一个，最后一个条码框回车执行确认。
- 普通扫码确认校验：流程为空提示 `流程文件不能为空`；条码不允许为空时按 WinForms `Trim()` 后提示 `条码不能为空`。
- 条码数量按 WinForms 语义支持 1..10，改变数量后重建 `条码` / `条码1..N` 动态输入框。
- 取消语义：流程未变化 `status=2`，流程变化 `status=0`；确认 `status=1`。

## 4. 明确未实现和禁止范围

- 未调用 `ProductTestCore`。
- 未调用 `ProductTestCore.ExecuteTest()` 或任何真实测试执行链。
- 未调用 `SysCache.StartTest`。
- 未调用 `SysCache.UploadData`。
- 未注册或触发 `AutoTestStartTestDic`、`AutoTestStopTestDic`。
- 未初始化 PLC、设备或调用任何 `.Init(`。
- 未进入 GP12、AutoType 9、ATSMes、ATSAutoTest、ATSDevice 全量迁移。
- 未实现 `FrmFlowInfo` 的完整流程查看/编辑承载，只保留产品测试壳可扩展位置。
- 未实现 NG 复测密码、MES 未启用确认、`MesType=16` BDU 条码规则匹配，这些属于后续产品测试执行链或客户专项验证范围。

## 5. 验证命令和结果

RED 证据：

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter ProductTestShell
# 失败：ATS5.Modules.ProductTest、ProductTestShellViewModel、BarcodeDialogViewModel 等类型不存在。
```

新增模块后首次验证：

```powershell
dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln
# 成功，新增 ATS5.Modules.ProductTest project.assets.json。
```

当前新鲜验证：

```powershell
dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln
# 所有项目均是最新的，无法还原。

dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
# 已成功生成。0 个警告，0 个错误。

dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter "ProductTestShell|ProductTestShellViewStructureTests|ShellNavigationStateTests|WpfSmokeTests"
# Code Quality Re-review 通过后新鲜验证：失败 0，通过 15，跳过 0，总计 15。

dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
# Code Quality Re-review 通过后新鲜验证：失败 0，通过 143，跳过 0，总计 143。

rg -n -e 'ProductTestCore' -e 'SysCache\.StartTest' -e 'SysCache\.UploadData' -e 'AutoTestGetBarcodeDic' -e 'AutoTestCheckBarcodeDic' -e 'AutoTestPreTestMesCheckDic' -e 'AutoTestStartTestDic' -e 'AutoTestStopTestDic' -e 'AutoTestStateDic' -e 'DevicePool' -e '\.Init\(' -e 'GP12' -e 'AutoType\s*9' -e 'ATSMes' -e 'ATSAutoTest' -e 'ATSDevice' -e 'ExecuteTest' -- ATS5.Modules.ProductTest ATS5.Application\ProductTestShell ATS5.Infrastructure.LegacyAdapters\ProductTestShell
# exit code 1，无生产 WP-08 实现侧禁区命中。
```

补充扫描：同一禁区扫描包含 `ATS5.Tests\Infrastructure\ProductTestShellViewStructureTests.cs` 时，仅命中测试中的 `Assert.DoesNotContain(...)` 禁止断言文本，属于保护网，不是实现越界。

## 6. 数据兼容结果

- WP-08 不写正式测试结果、SQLite `IndexInfo`、gzip JSON 明细或自动导出文件。
- WP-08 不保存 `.fw/.dev/.adbc/.xlsx`。
- 扫码结果仅保存在 WPF ViewModel 内存状态中，用于壳层显示和后续执行链接入点。
- 未污染 `D:\CODE\ATE\01_code`。

## 7. 日志验证结果

- 未替换现有日志框架。
- WP-08 当前不新增真实测试日志、MES 日志、设备日志或自动化日志。
- 因未进入真实执行链，本阶段没有生成 `Log/Test`、`Log/MES`、`Log/Error` 的新增行为；后续进入 ProductTest 执行链时必须补日志对照。

## 8. UI/交互验证结果

- XAML 结构测试锁定产品测试导航、工具栏、测试信息表列、消息区、当前测试区、系统消息区和扫码弹窗关键文案。
- ViewModel 测试覆盖双通道隔离、通道标题规则、扫码结果回填、回车跳焦、必填校验和取消状态语义。
- 本轮未执行人工点击验收；产品测试导航、扫码弹窗焦点、实际窗口大小和视觉细节需要用户现场确认。

## 9. 设备/MES/自动测试影响

- 未触发真实设备、PLC、MES、AutoTest。
- 未改变设备调用顺序、MES 上报时机、自动测试回调顺序。
- 未注册 `SysCache.AutoTest*Dic`。
- 未进入 GP12 或 AutoType 9。

## 10. 评审结果

- Spec Compliance Review：
  - 首轮结论：CHANGES_REQUESTED。
  - 修复项：流程列表改为通过 `IProductTestFlowCatalog` / `LegacyProductTestFlowCatalog` 读取 legacy `.fw` 文件名；条码数量改为 1..10 可调整并动态重建；必填条码按 WinForms `Trim()` 后校验；禁区扫描测试补充 AutoTest 其它字典入口。
  - Re-review：APPROVED。
- Code Quality Review：
  - 首轮结论：CHANGES_REQUESTED。
  - 修复项：可编辑流程 ComboBox 绑定 `Text`；legacy `.fw` 文件扫描处理 `IOException` / `UnauthorizedAccessException`；新增公共 API XML 注释；产品测试配置键和标题状态文本常量化。
  - Re-review：APPROVED。
  - 复审 residual risk：部分 WPF/ViewModel public classes 未全部加 XML 注释，但属于 UI-facing implementation surface，不作为 WP-08 门禁阻塞；XAML 结构测试仍以字符串断言为主，后续完整 UI 自动化可继续增强。

## 11. 未解决问题

- WP-08-Q1：产品测试壳和扫码弹窗需要人工点击验收，尤其是双通道 Tab、扫码弹窗焦点/回车、确定/取消、窗口置顶和尺寸。
- WP-08-Q2：`FrmFlowInfo` 完整流程查看窗口未迁移，本轮只做产品测试壳，后续若启用流程详情按钮需单独确认。
- WP-08-Q3：NG 复测密码、MES 未启用确认、`MesType=16` BDU 条码规则匹配未实现，需随产品测试执行链或客户专项批次处理。
- WP-08-Q4：本轮没有注册 AutoTest 字典；后续 AutoType 9 双通道自动化接入前必须单独计划和验证。

## 12. 下一步建议

- WP-08 已通过双评审和新鲜自动验证；下一步只能在用户确认后进入下一任务批次。
- 禁止直接进入 Wave 4 产品测试执行链、GP12、AutoType 9、真实设备、MES 上传或 ATSMes/ATSAutoTest/ATSDevice 全量迁移。
- 建议先做 WP-08 人工点击验收：产品测试导航、双通道 Tab、扫码弹窗焦点/回车、确定/取消、窗口置顶和尺寸。
