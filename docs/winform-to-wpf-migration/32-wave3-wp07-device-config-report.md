# Wave 3 WP-07 Device Config Management Report

更新时间：2026-05-21
状态：WP-07 设备配置管理代码侧已实现、已验证、已通过 Spec Compliance Review 和 Code Quality Review；未进入设备调试 Host、真实设备初始化、ProductTest、MES、AutoTest、GP12、AutoType 9

## 1. 本阶段完成内容

- 迁移设备配置管理样板：设备库树、搜索、`.dev` 打开、保存、另存为、取消、排序、删除、初始化参数说明。
- 复用 legacy `JsonHelper`、`DeviceCore.GetDevices()`、`ATSModel.Device` 和 `SysCache.PathDevCfg`。
- 保持 `.dev` 为 UTF-8 BOM JSON 数组，字段包括 `DevType`、`DevName`、`DevCode`、`InitPars`、`InitRemark`、`DevClasss`、`IsEnable`、`Remark`、`IsGlobal`。
- WPF Shell 启用“设备管理”导航，但本阶段只进入配置管理，不启用设备调试 Host 或真实设备生命周期。
- 增加设备配置 Application service、legacy repository、Prism module、ViewModel、View、弹窗服务、结构测试和兼容测试。

## 2. 修改文件列表

主要新增/修改文件：

- `D:\CODE\ATE\ATS5.0\ATS5.Application\DeviceConfig\DeviceConfigItem.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\DeviceConfig\DeviceConfigName.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\DeviceConfig\DeviceConfigNameValidator.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\DeviceConfig\DeviceConfigOpenResult.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\DeviceConfig\DeviceConfigOperationResult.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\DeviceConfig\DeviceConfigSaveMode.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\DeviceConfig\DeviceConfigService.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\DeviceConfig\DeviceLibraryItem.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\DeviceConfig\ICurrentUserContext.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\DeviceConfig\IDeviceConfigRepository.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\DeviceConfig\LegacyCurrentUserContext.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\DeviceConfig\LegacyDeviceConfigRepository.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Device\ATS5.Modules.Device.csproj`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Device\DeviceModule.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Device\ViewModels\DeviceLibraryNodeViewModel.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Device\ViewModels\DeviceManagerViewModel.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Device\ViewModels\IDeviceConfigDialogService.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Device\Views\DeviceConfigDialogService.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Device\Views\DeviceManagerView.xaml`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Device\Views\DeviceManagerView.xaml.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\DeviceConfig\*.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\DeviceManagerViewStructureTests.cs`

Shell/solution/test 集成文件：

- `D:\CODE\ATE\ATS5.0\ATS5.Wpf\App.xaml.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf\ViewModels\ShellViewModel.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf\Views\Shell.xaml`
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf\ATS5.Wpf.csproj`
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\ShellNavigationStateTests.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\WpfSmokeTests.cs`

## 3. 对应原 WinForms 功能

原 WinForms 基线：

- `D:\CODE\ATE\01_code\ATS\Device\UcDevice.cs`
- `D:\CODE\ATE\01_code\ATS\Device\UcDevice.Designer.cs`
- `D:\CODE\ATE\01_code\ATS\Device\FrmAllDev.cs`
- `D:\CODE\ATE\01_code\ATS\Device\FrmAllDev.Designer.cs`
- `D:\CODE\ATE\01_code\ATS\Device\FrmDevSave.cs`
- `D:\CODE\ATE\01_code\ATS\Device\FrmDevSave.Designer.cs`
- `D:\CODE\ATE\01_code\ATSModel\Device.cs`
- `D:\CODE\ATE\01_code\ATSCore\DeviceCore.cs`
- `D:\CODE\ATE\01_code\ATSCommon\JsonHelper.cs`

已对齐行为：

- 三栏布局：左侧“设备库”、中间“设备配置信息”、右侧“设备初始化参数说明”。
- 顶部按钮：`新建`、`编辑`、`打开`、`保存`、`另存为`、`取消`、`配置设备调试`、`清除全局参数`。
- `配置设备调试` 与 `清除全局参数` 可见但禁用，不绑定真实命令。
- 设备库用 TreeView 展示父/子节点，默认展开，搜索按 `DevName` contains 并带父/子节点。
- 设备库叶子节点双击添加配置，默认 `DevCode` 为类名最后段小写加 `_`，`IsEnable=true`。
- 配置表列：`启用`、`设备类型`、`设备名称`、`底层类名`、`设备编码`、`初始化参数`、`备注`、`全局参数`。
- 选中配置行显示 `InitRemark`。
- 默认态表格只读；新增/编辑态可编辑；`另存为` 仅编辑态可用。
- 保存前空配置提示 `无设备配置信息，无法保存`。
- 启用设备 `DevCode` 重复提示 `存在多个相同设备编码【{code}】\r\n请检查修改！`。
- 保存成功提示 `保存成功`。
- 覆盖保存日志：`[{RoleName}:{UserName}]设备配置{cfgName}.dev修改，并保存覆盖`。
- 另存为日志：`[{RoleName}:{UserName}]设备配置另存为，原配置：{old}.dev, 新配置：{new}.dev`。
- 0 字节或损坏 `.dev` 提示 `设备配置文件已损坏，请重新选择！`。

## 4. 已验证内容

- WP-07 目标测试：23/23 通过。
- DeviceManagerView 结构测试：2/2 通过。
- 全量测试：132/132 通过。
- 构建：0 警告，0 错误。
- 越界扫描未命中 `FrmDevDebug`、`DevicePool.ClearObj`、`ProductTestCore.Start`、`GP12`、`AutoType 9` 或真实设备 `Init(`。
- Spec Compliance Review：首次 CHANGES_REQUESTED，修复设备库搜索/tree 和保存名 trim 后 re-review APPROVED。
- Code Quality Review：首次 CHANGES_REQUESTED，修复配置名路径边界和 null guard 后 re-review APPROVED。

## 5. 验证命令和结果

```powershell
dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln
# 所有项目均是最新的，无法还原。

dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
# 已成功生成。0 个警告，0 个错误。

dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter DeviceConfig
# 已通过：失败 0，通过 23，跳过 0，总计 23。

dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter DeviceManagerViewStructureTests
# 已通过：失败 0，通过 2，跳过 0，总计 2。

dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
# 已通过：失败 0，通过 132，跳过 0，总计 132。

rg -n "FrmDevDebug|DevicePool\.ClearObj|ProductTestCore\.Start|GP12|AutoType\s*9|\.Init\(" `
  D:\CODE\ATE\ATS5.0\ATS5.Application\DeviceConfig `
  D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\DeviceConfig `
  D:\CODE\ATE\ATS5.0\ATS5.Modules.Device `
  D:\CODE\ATE\ATS5.0\ATS5.Tests\DeviceConfig `
  D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\DeviceManagerViewStructureTests.cs
# 无命中。
```

说明：验证期间曾在并行执行多个 WPF build/test 命令时遇到临时 baml/obj 文件锁；改为串行后构建和测试均通过。后续 WPF 验证应避免并行构建同一 solution。

## 6. 数据兼容结果

- `LegacyDeviceConfigRepository` 调用原 `JsonHelper.GetJsonFile` / `WriteJsonFile`，保持 UTF-8 BOM JSON。
- 真实 Debug 样本 `测试.dev` 可读，WPF repository 全量读取设备列表，不截断。
- WPF 在临时 runtime 写出的 `.dev` 可重新读回，字段和数量保持一致。
- 0 字节样本 `DeviceA.dev` 返回 legacy 损坏提示。
- 配置名进入路径前通过 `DeviceConfigNameValidator` 限制为叶子文件名，拦截 `..\`、目录分隔符和非法文件名字符。
- 所有写入测试使用临时目录，未写入 `D:\CODE\ATE\01_code`。

## 7. 日志验证结果

- 未替换现有日志框架。
- `ILogService` 仍由 `LegacyLogService` 对接原 `LogHelper` / log4net。
- 覆盖保存只在 JSON 变化时记录 legacy 操作日志。
- 另存为记录 legacy 操作日志。
- 本轮未新增设备调试、设备初始化、MES 或自动测试日志路径。

## 8. UI/交互验证结果

- XAML 结构测试锁定三栏布局、设备库 TreeView、搜索框占位、配置表列、右侧初始化参数说明和禁用设备调试/清全局入口。
- 设备库搜索 ViewModel 测试覆盖 leaf 命中带父节点、category 命中带全部子项、空搜索还原全树。
- 打开和保存弹窗为最小 WPF 交互实现，包含 `设备配置清单`、`设备配置类别`、空名提示和覆盖确认。
- Shell `NavDevice` 已从未迁移禁用状态切换为 `NavigateDeviceCommand`，对应 smoke 测试已同步。
- 本轮未执行人工点击验收；运行时 UI 点击、视觉细节和真实设备库展开效果仍需现场人工验收。

## 9. 设备/MES/自动测试影响

- 未调用 `FrmDevDebug`。
- 未调用 `DevicePool.ClearObj()`。
- 未调用真实设备 `Init`、连接、关闭或调试 UI。
- 未修改 `ATSDevice` 插件目录或插件协议。
- 未进入 ProductTest、MES、AutoTest、GP12、AutoType 9。
- `DeviceCore.GetDevices()` 仅用于读取设备 DLL/XML 元数据，这与 WinForms 设备库初始化路径一致。

## 10. 评审结果

Spec Compliance Review：

- 首轮结论：CHANGES_REQUESTED。
- 修复项：
  - 设备库搜索由只读 placeholder 改为 `SearchText` 绑定和 WinForms 风格过滤。
  - 设备库由 flat ListBox 改为 TreeView + HierarchicalDataTemplate。
  - 保存名不再 `.Trim()`，保留 WinForms 原样输入语义。
- Re-review 结论：APPROVED。

Code Quality Review：

- 首轮结论：CHANGES_REQUESTED。
- 修复项：
  - 增加 `DeviceConfigNameValidator`，service 和 repository 均拦截路径逃逸和非法叶子文件名。
  - 补 `LegacyCurrentUserContext` 和 `DeviceConfigDialogService` DI null guard。
- Re-review 结论：APPROVED。

## 11. 未解决问题

- WP-07-Q1：本轮未执行人工点击验收。设备库 TreeView、打开/保存弹窗、双击添加、排序、保存等需要用户现场点击确认。
- WP-07-Q2：本轮只做配置管理，不做 `FrmDevSave` 右键物理删除 `.dev` 文件能力；后续完整设备管理收口时需确认是否迁移该高风险删除入口。
- WP-07-Q3：本轮未迁移隐藏的“设备底层驱动导入/删除底层库”按钮；WinForms Designer 中默认隐藏，后续如需要启用需单独确认。
- WP-07-Q4：本轮未联机验证设备 DLL/XML 损坏、缺失或所有设备类别的完整显示质量。
- WP-07-Q5：legacy integration test 仍依赖 `D:\CODE\ATE\01_code\ATS\bin\Debug`，跨机/CI 时需提供 fixture 或 runtime root 配置。

## 12. 下一步建议

- 暂停在 WP-07 收口点，不进入 WP-08。
- 请用户人工验收设备管理页面：导航进入、设备库搜索、设备库双击添加、打开 `.dev`、保存测试副本、另存为、排序、删除、右侧参数说明、设备调试和清全局是否禁用。
- 若确认 WP-07 通过，再按执行总控提示词单独确认是否进入 WP-08 产品测试壳和扫码弹窗。
