# Wave 3 Execution Plan

更新时间：2026-05-21
状态：WP-06、WP-07、WP-08 已按本计划单独执行并通过双评审；人工点击验收仍未关闭；禁止扩大到后续 WP。

## Purpose And Scope

Wave 3 只覆盖以下 Work Package：

- WP-06 Authority：权限管理，迁移用户、角色、权限维护 UI 和 CRUD 行为。
- WP-07 Device config management：设备配置管理，只做设备库、设备配置文件读写和配置 UI，不做设备调试 Host。
- WP-08 ProductTest shell + barcode dialogs：产品测试壳、双通道页面框架和扫码弹窗，只做启动前 UI/状态/焦点/字典占位验证。

Wave 3 原本是执行计划；当前 WP-06/WP-07/WP-08 已按本计划完成代码侧实现、验证和双评审。本文不授权 Wave 4、全量迁移或任何新的业务实现。

当前执行状态：

- WP-06 Authority：已完成，报告见 `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\30-wave3-wp06-authority-report.md`。
- WP-07 Device config management：已完成，报告见 `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\32-wave3-wp07-device-config-report.md`。
- WP-08 ProductTest shell + barcode dialogs：已完成，报告见 `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\33-wave3-wp08-product-test-shell-report.md`。
- Wave 3 整体：代码侧已完成并通过双评审；人工点击验收未关闭，不能标记为现场验收通过。

## Hard Prohibitions

- 禁止实现或启用产品测试执行链。
- 禁止 GP12。
- 禁止 AutoType 9。
- 禁止设备调试 Host。
- 禁止真实设备初始化。
- 禁止全量迁移 `ATSMes`、`ATSAutoTest`、`ATSDevice`。
- 禁止修改 `D:\CODE\ATE\01_code`。
- 禁止改变现有日志框架、配置键、数据格式、委托签名、保存时机和设备/MES/自动化时序。

## Preconditions And Open Gates

已满足的 Wave 2 前置证据：

- W2-1..W2-7 已完成并双评审。
- 证据文档：`23-wave2-w2-1-blocker-report.md`、`24-wave2-w2-2-dataquery-viewmodel-ui-report.md`、`25-wave2-w2-3-manual-mes-upload-report.md`、`26-wave2-w2-4-flow-fw-compatibility-report.md`、`27-wave2-w2-5-flow-save-script-validation-report.md`、`28-wave2-w2-6-flow-viewmodel-xaml-state-report.md`、`19-wave2-sample-verification-report.md`。
- target smoke/structure tests 10/10。
- full tests 83/83。
- build 0 warnings/errors。
- startup probe passed。

仍未关闭的门禁：

- 人工点击导航/UI 签核。
- 可读真实 legacy gzip JSON 明细样本。
- GP12。
- AutoType 9。
- 设备在线/调试。
- 正式流程覆盖保存。
- 全量迁移确认。

这些未关闭门禁不得被 Wave 3 计划确认自动视为通过。Wave 3 只允许在用户明确确认后按本计划推进。

## Shared Execution Rules

- 每个 WP 先做只读 WinForms baseline pass，再写 mapping table，再写测试，再实现。
- 每个 WP 必须有 Spec Compliance Review 和 Code Quality Review。
- 所有写入测试必须使用临时目录、测试数据库或副本，禁止污染 legacy Debug 样本。
- 所有 UI 行为以原 WinForms 为唯一验收标准。
- 允许的文件范围必须在实施前再次列出；若目标文件与其他 worker 冲突，停止并回报。
- 禁止修改生产代码以外的未列入 Wave 3 文件；禁止顺手迁移邻近功能。

## WP-06 Authority

### Baseline

原 WinForms 路径：

- `D:\CODE\ATE\01_code\ATS\Authority\UcAuthority.cs`
- `D:\CODE\ATE\01_code\ATS\Authority\FrmUser.cs`
- `D:\CODE\ATE\01_code\ATS\Authority\FrmRole.cs`
- 相关 legacy 表：`User`、`Role` 和权限关联数据。

目标 WPF 路径：

- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Authority\Views\AuthorityView.xaml`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Authority\ViewModels\AuthorityViewModel.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Authority\Views\UserDialog.xaml`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Authority\Views\RoleDialog.xaml`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\Authority\*`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\Authority\*`

### Mapping Table Requirements

- 用户列表列名、排序、默认选中、刷新时机。
- 角色列表列名、权限树或勾选区域、联动行为。
- 新增、编辑、删除、保存、取消、关闭弹窗的事件映射。
- 删除确认文案、重复名称、空字段、权限缺失等异常路径。
- `User`、`Role` 读写字段、默认值、更新时间和日志分类。

### Allowed And Forbidden

允许目标模块：

- `ATS5.Modules.Authority`
- `ATS5.Application\Authority`
- `ATS5.Infrastructure.LegacyAdapters` 中已存在的只读/最小数据访问包装，前提是实施计划再次确认文件名。
- `ATS5.Tests\Authority`

禁止文件/模块：

- 登录策略、Shell 登录流程、用户认证全局门禁。
- `ProductTest`、`ATSMes`、`ATSAutoTest`、`ATSDevice`。
- `D:\CODE\ATE\01_code`。

### Tests And Verification

- 写 Authority ViewModel 单元测试：加载用户/角色、选择联动、命令启用状态。
- 写 Authority service/repository 测试：使用测试库或临时副本验证 CRUD。
- 数据验证：WinForms 与 WPF 对同一测试副本的 `User`、`Role` 字段读写一致。
- 日志验证：新增、编辑、删除、异常路径仍走原分类，不输出敏感数据。
- UI 验证：导航入口、列表、弹窗、确认框、按钮禁用态、刷新时机。

### Review Gates

- Authority mapping table 已完成并被 reviewer 接受。
- 测试通过且未污染 legacy 数据。
- Spec Compliance Review 确认无登录策略扩大。
- Code Quality Review 确认命名、分层、空值和日志符合规范。

### Execution Result

- 状态：已完成代码侧验证和双评审。
- 报告：`D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\30-wave3-wp06-authority-report.md`。
- 验证：Authority tests 29/29，全量 tests 112/112，build 0 警告 0 错误。
- 未关闭：初始加载异常是否保留 WPF `ex.Message` 提示、权限 CRUD 操作日志分类对照是否后续补齐。
- 边界：未进入 WP-07、WP-08、ProductTest 执行链、GP12、AutoType 9、设备调试 Host 或真实设备初始化。

## WP-07 Device Config Management

### Baseline

原 WinForms 路径：

- `D:\CODE\ATE\01_code\ATS\Device\UcDevice.cs`
- `D:\CODE\ATE\01_code\ATS\Device\FrmAllDev.cs`
- `D:\CODE\ATE\01_code\ATS\Device\FrmDevSave.cs`
- 只读参考：`.dev` 样本、设备库目录、设备启用态和排序规则。

目标 WPF 路径：

- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Device\Views\DeviceManagerView.xaml`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Device\ViewModels\DeviceManagerViewModel.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Device\Views\DeviceLibraryDialog.xaml`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Device\Views\DeviceSaveDialog.xaml`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\DeviceConfig\*`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\DeviceConfig\*`

### Mapping Table Requirements

- 设备库树、搜索、导入、选择、排序、启用态。
- 配置表列名、默认值、编辑控件、校验提示。
- `.dev` 打开、另存、保存副本、取消、覆盖确认行为。
- 文件编码、字段顺序、设备 DLL/XML 引用路径。
- 操作日志、错误日志和异常提示文案。

### Allowed And Forbidden

允许目标模块：

- `ATS5.Modules.Device` 的配置管理文件。
- `ATS5.Application\DeviceConfig`
- `ATS5.Infrastructure.LegacyAdapters` 中已存在的 `.dev` 读写包装，前提是实施计划再次确认文件名。
- `ATS5.Tests\DeviceConfig`

禁止文件/模块：

- `FrmDevDebug` 对应的设备调试 Host。
- 真实设备初始化、连接、关闭、调试控件加载。
- `ATSDevice` 插件目录迁移、重命名、合并或重排。
- `ProductTest`、`ATSMes`、`ATSAutoTest`。
- `D:\CODE\ATE\01_code`。

### Tests And Verification

- 写 `.dev` 兼容测试：WinForms 样本 WPF 可读，WPF 测试副本 WinForms 可读。
- 写 ViewModel 测试：设备选择、启用态、排序、命令状态。
- 数据验证：字段、顺序、路径、默认值和编码不漂移。
- 日志验证：打开、保存、另存、失败路径输出分类一致。
- UI 验证：设备库树、搜索、导入、配置表、保存/另存弹窗。

### Review Gates

- Device config mapping table 已完成并被 reviewer 接受。
- `.dev` 互读验证通过。
- Spec Compliance Review 确认未进入设备调试 Host 或真实设备初始化。
- Code Quality Review 确认没有改动插件协议、路径规则或保存格式。

### Execution Result

- 状态：已完成代码侧验证和双评审。
- 报告：`D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\32-wave3-wp07-device-config-report.md`。
- 验证：DeviceConfig tests 23/23，DeviceManagerViewStructureTests 2/2，全量 tests 132/132，build 0 警告 0 错误。
- 未关闭：人工点击验收、`FrmDevSave` 右键物理删除 `.dev`、隐藏底层驱动导入/删除、设备库 DLL/XML 全量质量和跨机 fixture。
- 边界：未进入 WP-08、ProductTest 执行链、GP12、AutoType 9、设备调试 Host、真实设备初始化、ATSMes、ATSAutoTest 或 ATSDevice 插件迁移。

## WP-08 ProductTest Shell And Barcode Dialogs

### Baseline

原 WinForms 路径：

- `D:\CODE\ATE\01_code\ATS\ProductTest\UcTestMain.cs`
- `D:\CODE\ATE\01_code\ATS\ProductTest\UcProductTest.cs`
- `D:\CODE\ATE\01_code\ATS\ProductTest\FrmBarcode.cs`
- `D:\CODE\ATE\01_code\ATS\ProductTest\FrmFlowInfo.cs`

目标 WPF 路径：

- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\Views\ProductTestShellView.xaml`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\Views\ProductTestChannelView.xaml`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\Views\BarcodeDialog.xaml`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\ViewModels\ProductTestShellViewModel.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\ViewModels\ProductTestChannelViewModel.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTestShell\*`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\ProductTestShell\*`

### Mapping Table Requirements

- `ChannelNum=2` Tab、通道标题、状态文本、按钮启用态。
- 扫码弹窗焦点、回车行为、取消行为、重复条码和空条码提示。
- 流程名、条码、测试前状态显示，不落库或仅写测试副本。
- 开始、暂停、停止、清空、复测等按钮在 Wave 3 的禁用/占位规则。
- AutoTest 字典注册占位验证，不触发真实自动化、设备或 MES。

### Allowed And Forbidden

允许目标模块：

- `ATS5.Modules.ProductTest` 的 shell、channel、barcode dialog 文件。
- `ATS5.Application\ProductTestShell`
- `ATS5.Tests\ProductTestShell`

禁止文件/模块：

- `ProductTestCore.Start` 或任何真实产品测试执行链。
- 设备初始化、MES 上传、AutoTest 启动、GP12、AutoType 9。
- `ATS5.Application\ProductTest` 核心执行 adapter，除非 Wave 3 实施计划重新明确为只读接口。
- `ATSMes`、`ATSAutoTest`、`ATSDevice` 全量迁移。
- `D:\CODE\ATE\01_code`。

### Tests And Verification

- 写 ViewModel 测试：双通道状态隔离、按钮状态、扫码结果分发。
- 写 BarcodeDialog 交互测试或 smoke：焦点、回车、取消、错误提示。
- 数据验证：Wave 3 不写正式测试结果；如需状态副本，必须使用临时测试目录。
- 日志验证：扫码取消、空条码、重复条码、占位操作的日志分类一致。
- UI 验证：Shell 导航、双通道 Tab、状态栏、扫码弹窗、复测弹窗占位。

### Review Gates

- ProductTest shell mapping table 已完成并被 reviewer 接受。
- Spec Compliance Review 确认未进入产品测试执行链、GP12、AutoType 9 或设备调试。
- Code Quality Review 确认通道状态隔离，不共享可变状态导致串通道。

### Execution Result

- 状态：已完成代码侧验证和双评审。
- 报告：`D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\33-wave3-wp08-product-test-shell-report.md`。
- 验证：ProductTestShell/ShellNavigation/WpfSmoke 目标测试 15/15，全量 tests 143/143，build 0 警告 0 错误。
- 未关闭：人工点击验收、`FrmFlowInfo` 完整流程查看、NG 复测密码、MES 未启用确认、`MesType=16` BDU 条码规则、AutoTest 字典注册。
- 边界：未进入 ProductTestCore 真实执行链、设备初始化、MES 上传、GP12、AutoType 9、ATSMes、ATSAutoTest 或 ATSDevice 全量迁移。

## Parallelism

- WP-06 Authority 和 WP-07 Device config 当时在文件集互不重叠后可并行；当前两项均已完成。
- WP-08 ProductTest shell 已作为单独实施批次串行完成，因为它触碰 Shell/navigation，并且靠近高风险产品测试语义。
- 任何 worker 发现需要修改同一 XAML、ViewModel、Service、Adapter 或共享 legacy gateway，必须停止并交由 Controller 重新分配 owner。

## Required Roles And Reviews

- Controller：维护 Wave 3 边界、文件 owner、禁止范围和最终验收。
- Authority worker：只负责 WP-06。
- Device config worker：只负责 WP-07。
- ProductTest UI worker：只负责 WP-08。
- QA worker：维护测试清单、UI smoke、数据/日志验证证据。
- Spec reviewer：逐 WP 检查是否符合 WinForms parity 和 Wave 禁止范围。
- Code quality reviewer：逐 WP 检查命名、分层、空值安全、日志、安全和测试质量。

## Completion Criteria

Wave 3 完成必须同时满足：

- WP-06、WP-07、WP-08 均有 baseline notes、mapping table、测试结果、数据/日志/UI 验证记录。代码侧已满足，人工点击验收仍待关闭。
- 权限管理 CRUD 与 WinForms 行为一致。
- `.dev` 测试副本可由 WPF/WinForms 互读，排序和启用态一致。
- `ChannelNum=2` 产品测试壳 UI、通道状态和扫码弹窗行为一致，且未触发真实执行链。
- 未修改 `D:\CODE\ATE\01_code`。
- 未进入 GP12、AutoType 9、设备调试 Host、真实设备初始化、全量 ATSMes/ATSAutoTest/ATSDevice 迁移。
- Spec Compliance Review 和 Code Quality Review 均通过。
- Wave 2 遗留门禁状态已同步更新，未关闭项继续保留。

## Validation Commands

实施 Wave 3 后至少执行：

```powershell
dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln
dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
rg -n "ProductTestCore\.Start|GP12|AutoType\s*=?\s*9|FrmDevDebug|DeviceDebug|D:\\CODE\\ATE\\01_code" D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest D:\CODE\ATE\ATS5.0\ATS5.Modules.Device D:\CODE\ATE\ATS5.0\ATS5.Modules.Authority D:\CODE\ATE\ATS5.0\ATS5.Application D:\CODE\ATE\ATS5.0\ATS5.Tests
```

文档计划确认阶段只需运行 stale phrase 搜索，不要求 build。

## Confirmation Required

Wave 3 代码侧已完成。等待用户明确确认后，才能进入下一任务批次、Wave 4 或全量实现；确认前本文件不授权任何后续业务实现。
