# Wave 1 基础设施验证报告

更新时间：2026-05-21
状态：W1-1 输出资产清单检查完成；W1-2 日志通道 smoke 完成；W1-3 配置读取/写回兼容 smoke 完成；W1-4 RuntimePathProvider 固化完成；W1-5 未迁移入口状态检查完成；Wave 1 当前 Debug 基础设施门禁完成

## 范围

Wave 1 只稳定基础设施，不进入产品测试、GP12 MES、AutoType 9 自动化、设备调试全量迁移。

已完成任务：

- `W1-1 输出资产清单检查`
- `W1-2 日志通道 smoke`
- `W1-3 配置读取/写回兼容 smoke`
- `W1-4 RuntimePathProvider 固化`
- `W1-5 未迁移入口状态检查`

写入范围：

- 测试：`D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\WpfOutputAssetTests.cs`
- 测试：`D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\LegacyLogServiceTests.cs`
- 测试：`D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\LegacyAppConfigServiceTests.cs`
- 测试：`D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\DebugRuntimePathProviderTests.cs`
- 测试：`D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\ShellNavigationStateTests.cs`
- 代码：`D:\CODE\ATE\ATS5.0\ATS5.Application\Logging\ILogService.cs`
- 代码：`D:\CODE\ATE\ATS5.0\ATS5.Application\DataQuery\IAppConfigService.cs`
- 代码：`D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\Logging\LegacyLogService.cs`
- 代码：`D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\DataQuery\LegacyAppConfigService.cs`
- 代码：`D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\Runtime\DebugRuntimePathProvider.cs`
- 代码：`D:\CODE\ATE\ATS5.0\ATS5.Wpf\App.xaml.cs`
- 代码：`D:\CODE\ATE\ATS5.0\ATS5.Wpf\ATS5.Wpf.csproj`
- 代码：`D:\CODE\ATE\ATS5.0\ATS5.Wpf\Views\Shell.xaml`

禁止范围：

- 不改 `D:\CODE\ATE\01_code` 原源码。
- 不迁移 `ATSMes`、`ATSAutoTest` 业务。
- 不启用 MES/自动化 UI 占位按钮。
- 不替换 log4net。

## W1-1 TDD 记录

### RED

新增测试：

`ATS5.Tests.Infrastructure.WpfOutputAssetTests.DebugOutput_ContainsLegacyRuntimeAssetsRequiredByWave1`

验证 WPF Debug 输出目录必须包含：

- `ATS.exe.config`
- `log4net.config`
- `ATSCommon.dll`
- `ATSCore.dll`
- `ATSModel.dll`
- `ATSMes.dll`
- `ATSAutoTest.dll`
- `AppDll\Dapper.dll`
- `AppDll\log4net.dll`
- `AppDll\Newtonsoft.Json.dll`
- `AppDll\sqlite\System.Data.SQLite.dll`
- `AppDll\sqlite\x86\SQLite.Interop.dll`
- `AppDll\sqlite\x64\SQLite.Interop.dll`
- `SysCache\Devices\DeviceHelper.dll`

红灯命令：

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~WpfOutputAssetTests
```

红灯结果：

- 失败：1
- 通过：0
- 缺失：`ATSMes.dll`、`ATSAutoTest.dll`

结论：测试有效暴露当前 WPF 输出资产缺口。

### GREEN

修复文件：

- `D:\CODE\ATE\ATS5.0\ATS5.Wpf\ATS5.Wpf.csproj`

修复内容：

- 将 `D:\CODE\ATE\01_code\ATS\bin\Debug\ATSMes.dll` 复制到 WPF 输出目录。
- 将 `D:\CODE\ATE\01_code\ATS\bin\Debug\ATSAutoTest.dll` 复制到 WPF 输出目录。

说明：

- 仅复制 legacy DLL 作为运行资产。
- 未引用 `ATSMes` 或 `ATSAutoTest` 源码。
- 未迁移 MES/AutoTest 页面。
- 未启用 MES/AutoTest 占位按钮。

GREEN 验证：

| 命令 | 结果 |
| --- | --- |
| `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 成功；0 警告，0 错误 |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~WpfOutputAssetTests` | 成功；1 通过，0 失败 |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 成功；18 通过，0 失败，0 跳过 |

输出目录复核：

- `ATSMes.dll` 存在，大小 584,192 bytes。
- `ATSAutoTest.dll` 存在，大小 458,240 bytes。

## 后续验证

W1-1 修复后必须执行：

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~WpfOutputAssetTests
dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
```

## 评审记录

### Spec Compliance Review

通过。

- 符合 W1-1 目标：自动验证 WPF 输出目录包含 Wave 1 所需 legacy 运行资产。
- 修复范围只包含输出资产复制，不改变业务逻辑。
- 未修改 `D:\CODE\ATE\01_code` 原源码。
- 未迁移、启用或重写 `ATSMes`、`ATSAutoTest`。
- 未替换 log4net，未改变数据格式。
- 未扩大到产品测试、GP12、AutoType 9、设备调试等后续 Wave。

### Code Quality Review

通过。

- 新测试集中在 `ATS5.Tests\Infrastructure\WpfOutputAssetTests.cs`，职责单一。
- 测试以相对仓库根定位 WPF Debug 输出目录，避免硬编码用户临时目录。
- `ATS5.Wpf.csproj` 只新增两个 Content 复制项，沿用现有 `PreserveNewest` 模式。
- 没有引入新依赖、公共字段、跨层调用或业务重构。
- 残余风险：测试依赖 WPF 项目已构建生成输出目录；后续可在发布验证中补 Release 输出资产检查。

## W1-2 日志通道 Smoke

### RED

新增测试：

`ATS5.Tests.Infrastructure.LegacyLogServiceTests.LogService_WritesAllLegacyChannels_ToOriginalDirectories`

初始红灯：

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~LegacyLogServiceTests
```

红灯结果：

- 编译失败：缺少 `ATS5.Application.Logging.ILogService`。
- 测试工程未引用 `ATS5.Infrastructure.LegacyAdapters`。
- `LegacyLogService` 只实现局部 `IOperationLogger`，没有 6 个 legacy 日志通道。

### GREEN

修复文件：

- `D:\CODE\ATE\ATS5.0\ATS5.Application\Logging\ILogService.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\Logging\LegacyLogService.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj`
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf\App.xaml.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\LegacyLogServiceTests.cs`

修复内容：

- 新增 `ILogService`，公开 `Init`、`Info`、`Error`、`Test`、`Mes`、`Operate`、`CanTool`。
- `LegacyLogService` 仍调用原 `ATSCommon.LogHelper`，没有替换 log4net。
- WPF DI 注册 `ILogService -> LegacyLogService`。
- 测试工程引用 legacy adapter 项目，以验证真实 adapter。
- 测试复制 legacy `log4net.config` 到测试输出目录，验证原 `Log/*` 相对目录语义。

调试根因记录：

- 第一次行为红灯发现日志没有落到人工指定的 runtime 临时目录，而是按 log4net appender 相对路径落到测试进程输出目录，这是原配置语义。
- 第二次红灯发现测试输出目录缺 `log4net.config`，因此必须在测试环境复制原配置。
- 第三次红灯发现日志文件被 log4net 持有，测试读取需用 `FileShare.ReadWrite`。
- 第四次红灯发现 `Error` 通道等级是原始 `ERROR`，其余通道是 `INFO`，测试已按原等级语义区分。

验证：

| 命令 | 结果 |
| --- | --- |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~LegacyLogServiceTests` | 成功；1 通过，0 失败 |
| `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 成功；0 警告，0 错误 |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 成功；19 通过，0 失败，0 跳过 |

日志输出复核：

- `Log\Info\20260521.log`
- `Log\Error\20260521.log`
- `Log\Test\20260521.log`
- `Log\MES\20260521.log`
- `Log\Operate\20260521.log`
- `Log\Cantool\20260521.log`

### Spec Compliance Review

通过。

- 保留原 `LogHelper` 和 log4net。
- 保留原 6 个日志目录和通道名称。
- `Error` 仍走 `ERROR` 等级，其余通道走 `INFO`。
- 未更改 `log4net.config`。
- 未引入 Serilog、NLog、Microsoft.Extensions.Logging 或其他替代框架。
- 未改变业务日志内容，只新增统一适配接口和 smoke 测试。

### Code Quality Review

通过。

- `ILogService` 位于应用层，供后续模块依赖抽象。
- `LegacyLogService` 仍在 legacy adapter 层，集中包装静态 `LogHelper`。
- 兼容既有 `IOperationLogger`，未破坏流程样板。
- 测试覆盖真实 adapter、真实 legacy `log4net.config` 和真实文件输出。
- 测试读取日志文件使用共享读，适配 log4net 文件锁。
- 残余风险：日志配置在同一测试进程中为全局状态，后续如增加更多日志测试需避免并行互相干扰。

## W1-3 配置读取/写回兼容 Smoke

### RED

新增测试：

`ATS5.Tests.Infrastructure.LegacyAppConfigServiceTests`

初始红灯命令：

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~LegacyAppConfigServiceTests
```

红灯结果：

- 编译失败：`IAppConfigService` 没有 `SetValue`。
- 当前 `LegacyAppConfigService` 只能读，不能证明 `ATS.exe.config` 写回兼容。

### GREEN

修复文件：

- `D:\CODE\ATE\ATS5.0\ATS5.Application\DataQuery\IAppConfigService.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\DataQuery\LegacyAppConfigService.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\DataQuery\ManualMesUploadServiceTests.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\LegacyAppConfigServiceTests.cs`

修复内容：

- `IAppConfigService` 新增 `SetValue(string key, string value)`，返回 bool。
- `LegacyAppConfigService.SetValue` 直接包装原 `ConfigHelper.SetValueApp`。
- 测试只复制 `ATS.exe.config` 到临时目录，在临时副本写 `Language` 键。
- 缺失 key 仍返回 false，不新增节点，保持 legacy 语义。

验证：

| 命令 | 结果 |
| --- | --- |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~LegacyAppConfigServiceTests` | 成功；2 通过，0 失败 |
| `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 成功；0 警告，0 错误 |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 成功；21 通过，0 失败，0 跳过 |
| `Get-FileHash D:\CODE\ATE\01_code\ATS\bin\Debug\ATS.exe.config` | SHA256 仍为 `AD84D68828905480FEED4FFDC3F444277EE65B2479CD2F2D439D6413FF5A0632` |

### Spec Compliance Review

通过。

- 配置写回走原 `ConfigHelper.SetValueApp`，不重写 XML 规则。
- 只写临时副本，不污染 Debug 实际运行配置。
- 保留缺失 key 返回 false 的 legacy 行为。
- 不改变配置键、不新增节点、不自动保存。

### Code Quality Review

通过。

- 接口扩展最小，满足后续配置页面依赖抽象。
- Adapter 仍集中在 legacy infrastructure 层。
- 现有 fake 配置服务同步补齐接口，测试保持清晰。
- 使用原配置哈希证明未污染真实配置。
- 残余风险：`IAppConfigService` 当前位于 `DataQuery` 命名空间，后续可在不改行为的前提下迁到通用 Config 命名空间，但不应在本任务扩大范围。

## W1-4 RuntimePathProvider 固化

### RED

新增测试：

`ATS5.Tests.Infrastructure.DebugRuntimePathProviderTests.DefaultProvider_ResolvesToOriginalWinFormsDebugRuntime`

红灯命令：

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~DebugRuntimePathProviderTests
```

红灯结果：

- Expected：`D:\CODE\ATE\01_code\ATS\bin\Debug`
- Actual：`D:\CODE\ATE\ATS5.0\ATS\bin\Debug`

### GREEN

修复文件：

- `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\Runtime\DebugRuntimePathProvider.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\DebugRuntimePathProviderTests.cs`

修复内容：

- 默认路径解析从当前 AppBase 向上查找 `01_code\ATS\bin\Debug`。
- 找到后作为 legacy runtime root。
- 找不到时保留旧推断路径作为回退。

验证：

| 命令 | 结果 |
| --- | --- |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~DebugRuntimePathProviderTests` | 成功；1 通过，0 失败 |
| `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 成功；0 警告，0 错误 |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 成功；22 通过，0 失败，0 跳过 |

### Spec Compliance Review

通过。

- 默认运行根对齐用户已确认的 Debug 基线。
- 没有改 legacy 相对路径语义。
- 没有复制或修改 `D:\CODE\ATE\01_code` 运行资产。
- 保留回退路径，降低非当前目录布局下启动失败风险。

### Code Quality Review

通过。

- 解析逻辑集中在 `DebugRuntimePathProvider`。
- 测试直接锁定用户确认路径，防止后续误退回错误路径。
- 没有引入外部配置或硬编码到业务服务。
- 残余风险：发布环境最终运行根策略还需在 Release/部署 Wave 中单独设计，当前只固化开发 Debug 基线。

## W1-5 未迁移入口状态检查

### RED

新增测试：

`ATS5.Tests.Infrastructure.ShellNavigationStateTests.ShellNavigation_KeepsUnmigratedEntriesDisabled`

红灯命令：

```powershell
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~ShellNavigationStateTests
```

红灯结果：

- 失败：1
- 通过：0
- 失败信息：`Missing navigation button AutomationId: NavProductTest`
- 根因：测试按错误的 XML 命名空间读取 `AutomationProperties.AutomationId`，没有读取到 XAML 实际 attached property 属性名。

### GREEN

修复文件：

- `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\ShellNavigationStateTests.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf\Views\Shell.xaml`

修复内容：

- `Shell.xaml` 中导航按钮保留 AutomationId：
  - `NavProductTest`
  - `NavDataQuery`
  - `NavFlow`
  - `NavDevice`
  - `NavAuthority`
  - `NavMes`
  - `NavAutoTest`
  - `NavStatisticalInfo`
- 测试改为按 XAML 实际属性名读取 `AutomationProperties.AutomationId`。
- 验证仅 `数据查询` 和 `流程管理` 入口绑定导航命令。
- 验证 `产品测试`、`设备管理`、`权限管理`、`MES`、`自动化` 入口保持 `IsEnabled="False"`。
- 验证 `统计信息` 保持 `Visibility="Collapsed"`。

验证：

| 命令 | 结果 |
| --- | --- |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter FullyQualifiedName~ShellNavigationStateTests` | 成功；1 通过，0 失败 |
| `dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln` | 成功；`ATS5.Tests` 已还原，6 个项目最新 |
| `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 成功；0 警告，0 错误 |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 成功；23 通过，0 失败，0 跳过 |

### Spec Compliance Review

通过。

- 未启用产品测试、设备、权限、MES、自动化等未迁移入口。
- 仅数据查询和流程管理两个样板入口可导航，符合当前样板边界。
- 统计信息仍隐藏，避免用户误认为已迁移。
- 没有改业务逻辑、数据格式、日志框架、MES/设备/自动化流程。
- AutomationId 只用于后续 UI 自动化验收，不改变用户操作行为。

### Code Quality Review

通过。

- 导航状态测试集中在 `ShellNavigationStateTests`，职责单一。
- 测试直接读取 XAML 结构，适合作为未迁移入口状态的轻量门禁。
- 可空返回已显式标注为 `string?`，无新增编译警告。
- `Shell.xaml` 仅增加 AutomationId，不影响现有 Prism Region、命令绑定和样式。
- 残余风险：该测试验证静态 XAML 状态，不替代后续 FlaUI 真实点击/不可点击 UI 验证。

## Wave 1 总验证

| 命令 | 结果 |
| --- | --- |
| `dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln` | 成功 |
| `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 成功；0 警告，0 错误 |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 成功；23 通过，0 失败，0 跳过 |

## Wave 1 门禁结论

当前 Debug 基础设施门禁完成：

- 输出资产检查通过。
- 日志通道 smoke 通过。
- 配置读取/写回临时副本 smoke 通过。
- Debug RuntimePathProvider 对齐 `D:\CODE\ATE\01_code\ATS\bin\Debug`。
- 未迁移入口保持禁用或隐藏。

仍不允许进入全量铺开迁移，下一 Wave 需要单独确认实施范围、测试计划和文件 owner。

已知残余风险：

- Prism DialogService/EventAggregator 尚未形成完整项目级封装门禁。
- `ILegacySysCacheGateway` 和 `ILegacyMessageBridge` 尚未正式抽象。
- Release/发布输出资产清单尚未验证。
- `IAppConfigService` 当前仍在 `DataQuery` 命名空间，后续可做不改变行为的命名空间归位。
