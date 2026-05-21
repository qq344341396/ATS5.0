# WP-07 Device Config Management Plan

更新时间：2026-05-21
状态：执行中，当前只允许 WP-07 设备配置管理，不允许扩大到设备调试 Host、真实设备初始化、ProductTest、MES、AutoTest、GP12、AutoType 9。

## Scope Lock

允许新增或最小修改：

- `D:\CODE\ATE\ATS5.0\ATS5.Application\DeviceConfig\*`
- `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\DeviceConfig\*`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Device\*`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\DeviceConfig\*`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\DeviceManagerViewStructureTests.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf\App.xaml.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf\ViewModels\ShellViewModel.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf\Views\Shell.xaml`
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf\ATS5.Wpf.csproj`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj`
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln`

禁止修改或实现：

- `D:\CODE\ATE\01_code`
- `FrmDevDebug` 对应设备调试 Host
- `DevicePool.ClearObj()`
- `ATSDevice` 插件迁移、重命名、合并或重排
- `ProductTestCore.Start` 和任何真实产品测试执行链
- `ATSMes`、`ATSAutoTest`
- GP12、AutoType 9

## WinForms Baseline

原 WinForms 位置：

- `D:\CODE\ATE\01_code\ATS\Device\UcDevice.cs`
- `D:\CODE\ATE\01_code\ATS\Device\UcDevice.Designer.cs`
- `D:\CODE\ATE\01_code\ATS\Device\FrmAllDev.cs`
- `D:\CODE\ATE\01_code\ATS\Device\FrmAllDev.Designer.cs`
- `D:\CODE\ATE\01_code\ATS\Device\FrmDevSave.cs`
- `D:\CODE\ATE\01_code\ATS\Device\FrmDevSave.Designer.cs`
- `D:\CODE\ATE\01_code\ATSModel\Device.cs`
- `D:\CODE\ATE\01_code\ATSCore\DeviceCore.cs`
- `D:\CODE\ATE\01_code\ATSCommon\JsonHelper.cs`

布局基线：

- 左侧“设备库”：检索框占位“请输入检索内容”、设备树。
- 中间“设备配置信息”：配置表。
- 右侧“设备初始化参数说明”：只读参数说明。
- 顶部工具栏：`新建`、`编辑`、`打开`、`保存`、`另存为`、`取消`、`配置设备调试`、标题 `【设备配置：{cfgName}】`、`清除全局参数`。
- 行操作：`下移`、`上移`、`置顶`、`删除`。

`.dev` 基线：

- 路径：`SysCache/DevCfg/`
- Debug 样本：`D:\CODE\ATE\01_code\ATS\bin\Debug\SysCache\DevCfg\*.dev`
- 格式：UTF-8 BOM 单行 JSON 数组，元素字段为 `DevType`、`DevName`、`DevCode`、`InitPars`、`InitRemark`、`DevClasss`、`IsEnable`、`Remark`、`IsGlobal`。
- 0 字节文件按旧 UI 视为损坏，提示 `设备配置文件已损坏，请重新选择！`。

## Mapping Table

| WinForms | WPF |
| --- | --- |
| `UcDevice` | `DeviceManagerView` |
| `FrmAllDev` | `IDeviceConfigDialogService.SelectConfigName` |
| `FrmDevSave` 保存/覆盖确认 | `IDeviceConfigDialogService.RequestSaveConfigName` |
| `btnAdd_ItemClick` | `NewCommand` |
| `btnEdit_ItemClick` | `EditCommand` |
| `btnOpen_ItemClick` | `OpenCommand` |
| `btnSave_ItemClick` | `SaveCommand` |
| `btnSaveOther_ItemClick` | `SaveAsCommand` |
| `btnCancel_ItemClick` | `CancelCommand` |
| `treeDev_DoubleClick` | `AddDeviceFromLibraryCommand` |
| `btnUp/btnDown/btnTop/btnDel` | `MoveUpCommand`、`MoveDownCommand`、`MoveTopCommand`、`DeleteCommand` |
| `MsgBoxHelper.ShowWarning/Info/Error/Question` | `IMessageDialogService` 或 `IDeviceConfigDialogService` |
| `JsonHelper.GetJsonFile/WriteJsonFile` | `LegacyDeviceConfigRepository` 通过 legacy adapter 复用 |
| `LogHelper.Operate` | `ILogService.Operate` 复用原 log4net 语义 |

## Test Plan

先写并验证红灯：

- `DeviceConfigServiceTests`
  - 重复启用 `DevCode` 时返回旧提示文案。
  - 正常保存使用 repository 写入指定名称。
  - 编辑覆盖只有 JSON 变化时写操作日志。
  - 另存为写新名称并记录旧日志文案。
- `DeviceManagerViewModelTests`
  - 默认态按钮状态：新建/打开可用，保存/取消不可用，行操作不可用。
  - 新建进入编辑态，取消清空列表和配置名。
  - 打开加载配置名、标题和参数说明。
  - 添加设备库叶子项生成旧默认 `DevCode`、`InitPars`、`InitRemark`。
  - 上移/下移/置顶/删除保持 WinForms 顺序语义。
- `LegacyDeviceConfigRepositoryTests`
  - 真实 Debug `.dev` 样本可读。
  - 测试副本写出 UTF-8 BOM，并可按 legacy JSON 字段读回。
  - 0 字节 `.dev` 返回损坏结果，不污染源目录。
- `DeviceManagerViewStructureTests`
  - 三栏结构、旧按钮文字、配置表列名、设备调试/清除全局参数可见但禁用。
- forbidden-scope scan
  - 不出现 `FrmDevDebug`、`DevicePool.ClearObj`、`ProductTestCore.Start`、`GP12`、`AutoType 9`、真实 `Init(` 调用。

## Review Gates

- Spec Compliance Review 必须确认：
  - 不进入设备调试 Host。
  - 不进入真实设备初始化。
  - `.dev` 字段、路径、编码、保存/另存/取消/重复校验文案符合 WinForms。
  - 设备调试和清除全局参数入口保留禁用，不执行。
- Code Quality Review 必须确认：
  - C# 命名、可空、成员排序、最小访问权限符合 `AGENTS.md`。
  - ViewModel 依赖 service/repository/dialog 抽象，不直接访问 legacy 静态类。
  - Legacy adapter 隔离 `ATSCommon/ATSCore/ATSModel`。
  - 测试不写入 `D:\CODE\ATE\01_code`。

## Open WP-07 Questions

- 旧 `JsonHelper.GetJsonFile()` 对缺失文件会创建空文件。WP-07 当前计划只在用户选择已存在配置时打开，避免额外创建缺失文件；若后续要求完全复刻缺失副作用，需要单独确认。
- `FrmDevSave` 旧版允许未校验非法文件名。WP-07 当前计划保持旧文案和行为边界，但测试不主动覆盖非法字符扩展校验。
- `btnCancel_ItemClick` 编辑态读取失败会删除当前 `.dev`。WP-07 当前计划只对测试副本复刻可验证行为，不对真实 Debug 样本执行删除。
