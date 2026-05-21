# Wave 0 基线报告

更新时间：2026-05-21
状态：Wave 0 只读基线完成；当前进入 Wave 1 首个最小实现任务

## 范围

本阶段只读检查：

- 原 WinForms 运行基线：`D:\CODE\ATE\01_code\ATS\bin\Debug`
- 当前 WPF 目标：`D:\CODE\ATE\ATS5.0`
- 当前 WPF 样板：Shell、数据查询、流程编辑

未修改 `D:\CODE\ATE\01_code` 下任何业务、数据、配置、MES、设备、自动化源码或运行资产。

## 运行资产基线

| 资产 | 状态 | 数量/大小 | 风险 |
| --- | --- | --- | --- |
| `AppData\ats.db` | 存在 | 140,439,552 bytes | 高：主业务数据库，迁移前需备份和哈希锁定 |
| `AppData\ats.db-wal` | 存在 | 0 bytes | 中：需随库纳入基线 |
| `AppData\ats.db-shm` | 存在 | 32,768 bytes | 中：运行态残留，验证时要注意 |
| `AppData\TestData` | 目录存在 | `*.json` 131 个，总 393 bytes；无 `.json.gz`/`.gz` | 高：实际资产与“gzip json”口径不一致，后续必须按代码和文件内容双重判断 |
| `SysCache\Flows` | 存在 | `.fw` 132 个，9,243,227 bytes | 高：流程定义核心资产 |
| `SysCache\Flows\BackupFlow` | 存在 | `.fw` 90 个，6,049,030 bytes | 中：历史备份和追溯资产 |
| `SysCache\DevCfg` | 存在 | `.dev` 55 个，523,953 bytes | 高：设备配置 |
| `SysCache\DBC` | 存在 | `.adbc` 10 个，2,073,298 bytes | 中高：CAN/DBC 资产 |
| `SysCache\UDS` | 存在 | `.xlsx` 23 个，344,100 bytes | 中：Excel/UDS 资产 |
| `SysCache\Devices` DLL | 存在 | `.dll` 101 个，4,728,320 bytes | 高：设备插件二进制 |
| `SysCache\Devices` XML | 存在 | `.xml` 101 个，1,392,218 bytes | 中高：需与 DLL 成对校验 |
| `ATS.exe.config` | 存在 | 15,266 bytes | 高：运行配置源 |
| `log4net.config` | 存在 | 6,636 bytes | 高：日志框架配置，必须保留 |

关键哈希：

| 文件 | SHA256 |
| --- | --- |
| `AppData\ats.db` | `0E4FD32446672A069ED0E7A54296E3AE5EF36F0EEDFD1D8770945952F3C589C0` |
| `AppData\ats.db-wal` | `E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855` |
| `AppData\ats.db-shm` | `FD4C9FDA9CD3F9AE7C962B0DDF37232294D55580E1AA165AA06129B8549389EB` |
| `ATS.exe.config` | `AD84D68828905480FEED4FFDC3F444277EE65B2479CD2F2D439D6413FF5A0632` |
| `log4net.config` | `8484311D627D02110573A3BFA386A471E59BA42D94B6E5497E96C99DFE91E26F` |

## UI 验收缺口

当前只能按“UI 结构样板”验收，不能宣称主界面、数据查询、流程编辑功能完整迁移。

### 主界面

已具备结构走查基础：

- 顶部类 Ribbon 区。
- `工具`、`系统`、`admin【管理员】`。
- 版本 `GeekTest ATS V5.0.0`。
- Tile 导航顺序。
- Prism 内容 Region。
- 底部版权。

仍需人工验收或后续实现：

- `工具` 菜单下 DBC、UDS、CAN、第三方工具、数据迁移工具行为。
- `系统` 菜单下设置、帮助、关于、注册、语言切换。
- 用户菜单更改密码、注销。
- 隐藏/显示菜单栏实际折叠逻辑。
- 状态栏 MES/自动化/用户/版本动态状态。

必须保持禁用，不能宣称完成：

- 产品测试。
- 设备管理。
- 权限管理。
- MES。
- 自动化。
- 统计信息当前折叠，不纳入完成口径。

### 数据查询

已具备结构走查基础：

- 顶部工具条。
- 日期、条码、通道、流程筛选。
- 查询、MES 上传入口。
- 左索引、右明细。
- 左下统计区域。
- 右键 `MES上传`。

仍需人工验收或后续实现：

- 真实数据查询与 WinForms 对照。
- 条码/通道/流程筛选启用规则。
- 分页、选中索引后加载明细。
- 列宽、排序、只读、分组、右键区域。
- `UploadMesStatus=-1/0/1` 显示规则。
- 左下统计图仍是占位。

必须保持禁用，不能宣称完成：

- 日志导出。
- 全部展开。
- 全部收合。
- 数据导出。
- 右键数据导出。

### 流程编辑

已具备结构走查基础：

- 顶部工具条。
- 左测试项目列表。
- 项目脚本/输出项/临时变量页签。
- 脚本区。
- 方法说明和设备控制树。
- DBC/UDS/公共变量等底部页签。

仍需人工验收或后续实现：

- 当前脚本区是 WPF `TextBox` 样板，不是完整 `ICSharpCode.TextEditorControl` 体验。
- 脚本高亮、右键、剪切/复制/粘贴/全选、参数插入、方法说明联动未完成。
- 左侧项目增删改、上下移动、导入、全选等未能证明可用。
- 真实设备命令加载、搜索、方法插入脚本未完成。
- DBC/UDS 信号完整列表、右键插入/清空未完成。
- 输出项/临时变量行级编辑规则、增删上移复制、保存联动未完成。

必须保持禁用，不能宣称完成：

- 新建。
- 编辑。
- 导出。
- 导入。
- 另存为。
- 取消。
- 编辑公共变量。
- 修改流程名称。
- 格式化。
- 导入设备。
- 输出项/临时变量增删上移下移复制。

## 当前 WPF 样板验证

本轮执行：

| 命令 | 结果 |
| --- | --- |
| `dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln` | 成功；所有项目最新 |
| `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 成功；0 警告，0 错误 |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 成功；17 通过，0 失败，0 跳过 |

输出目录已观察到：

- `ATS.exe.config`
- `ATS5.Wpf.exe.config`
- `log4net.config`
- `ATSCommon.dll`
- `ATSCore.dll`
- `ATSModel.dll`
- `Dapper.dll`
- `log4net.dll`
- `Newtonsoft.Json.dll`
- `System.Data.SQLite.dll`
- Prism/HandyControl/DevExpress 等依赖 DLL

仍需测试锁定：

- SQLite native `x86/x64` DLL 是否完整复制。
- `DeviceHelper.dll` 是否在 `SysCache\Devices` 下完整输出。
- 必要 legacy DLL 和配置是否能通过自动测试验证，而不是人工观察。

## Wave 1 基础设施缺口

当前已有：

- PrismApplication + DryIoc。
- Shell、ContentRegion、数据查询/流程模块。
- `IRuntimePathProvider`、`DebugRuntimePathProvider`、`LegacyRuntimeContext`。
- 局部 `LegacyLogService`、`LegacyAppConfigService`、`LegacyManualMesGateway`、`LegacyFlowChangePublisher`。
- WPF csproj 已复制 `ATS.exe.config`、`log4net.config`、`AppDll/**`、`DeviceHelper.dll`。

当前缺口：

- 统一 `ILogService` 未完成，日志通道只覆盖 `Operate/Error` 的局部场景。
- `ILegacySysCacheGateway` 未完成，当前只有局部手动 MES gateway。
- `ILegacyMessageBridge` 未完成，当前只有流程保存点状 publisher。
- Prism DialogService 和 EventAggregator 应用级约定未完整建立。
- 配置只有读取 smoke，没有临时副本写回兼容 smoke。
- 默认运行根从输出目录反推 `ATS\bin\Debug`，与实际基线 `D:\CODE\ATE\01_code\ATS\bin\Debug` 存在风险。
- 输出资产策略缺自动化测试。

## Wave 0 结论

Wave 0 只读基线已完成，可以进入 Wave 1 首个最小实现任务：

`W1-1 输出资产清单检查`

该任务只新增/调整测试和必要的输出资产配置，不触碰原 WinForms 业务代码，不进入 MES、设备、自动化全量实现。
