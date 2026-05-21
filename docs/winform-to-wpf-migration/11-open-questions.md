# 未确认问题清单

更新时间：2026-05-21
状态：Wave 3 WP-06 权限管理、WP-07 设备配置管理、WP-08 产品测试壳和扫码弹窗均已完成代码侧验证并通过双评审；WP-08 仍等待人工点击验收

## 当前待补验/边界问题

| 编号 | 问题 | 影响 | 当前证据 | 需要确认 |
| --- | --- | --- | --- | --- |
| W2-1-Q1 | 当前机器缺少可读 legacy 明细 JSON | 真实现场 gzip JSON 明细字段映射仍待补验；下一任务不得宣称真实现场明细已完全验收 | `D:\CODE\ATE\01_code\ATS\bin\Debug\AppData\TestData` 下 131 个 `.json` 全部 3 字节；已用 seeded runtime gzip JSON 补验 `TestProjectDataCore.GetTestProjectDatas` legacy 链路，W2-1 测试 11/11 通过，整套测试 34/34 通过 | 后续请指定包含可读 `AppData\TestData\...\{LogGuid}-CH{channel}.json` 的运行目录，用于补验真实现场明细 |
| W2-1-Q2 | `Barcode/FlowName` 查询安全规范与 WinForms legacy 兼容冲突 | W2-1 当前按“原 WinForms 行为为验收标准”保留 legacy 查询；若未来改参数化，需单独确认并做对照测试 | `IndexInfoCore.GetTotalCount` 已参数化，`GetIndexInfo` 仍拼接 `Barcode LIKE '%{barcode}%'` 和 `ProcessName = '{processName}'`；WPF adapter 未重写该行为 | 默认保持 legacy 行为；如需安全参数化重写，必须另行确认 |
| W2-3-Q1 | WPF DataQuery 右键菜单仍包含 `MES上传`，WinForms `popupMenu3` 只有数据导出 | 不影响 W2-3 工具栏手动 MES 上传样板验收，但后续 UI parity 收敛时需要决定是否移除右键 MES 上传入口 | Spec Review 明确将其列为非阻塞边界；W2-3 未扩大处理右键菜单 | 后续 UI parity 或 DataQuery 完整页面迁移时，以原 WinForms Designer 为准收敛 |
| W2-3-Q2 | 手动 MES 上传结果状态和弹窗分类仍依赖 legacy 中文消息文本 | 当前为保持 WinForms 文案和行为而接受；未来若结构化结果级别，必须确认不改变弹窗文案、顺序和判定 | Code Quality Review 仅列为非阻塞维护性提示；W2-3 测试已覆盖当前文案路径 | 默认保持 legacy 文本语义；结构化优化需单独确认 |
| W2-4-Q1 | `.fw` 兼容测试中的 `AssemblyResolve` 为进程级事件 | 不影响当前 W2-4 通过结果；后续 corpus 扩大或测试并行化时可改进隔离性 | Code Quality Review 标记为非阻塞；全量测试 58/58 通过 | 暂不处理，后续测试维护批次可优化 |
| W2-4-Q2 | `.fw` 兼容测试 helper 的部分上下文没有直接进入 `Assert.Equal` 失败消息 | 不影响当前字段校验覆盖；失败定位时可能需要从调用栈反查样本上下文 | Code Quality Review 标记为非阻塞；W2-4 七个样本 2/2 测试通过 | 暂不处理，后续 corpus 扩大时优化断言消息 |
| W2-5-Q1 | `LegacyFlowScriptValidator` 未穷尽 WinForms `AllCheckCode` 所有潜在分支 | W2-5 已覆盖保存门禁所需的确定性高风险规则，但完整流程编辑迁移时仍需继续对照验证 | W2-5 覆盖重复启用项目名、输出/临时变量底层命名、重复变量、double/double[] 数值和长度、string 最大/最小值、默认 `ComparisonOperator`；目标测试 20/20，全量 71/71 | W2-6/Wave 后续如继续扩展脚本编辑，应逐项补完整 AllCheckCode parity |
| W2-5-Q2 | `LegacyFlowScriptValidatorTests` 使用真实 Debug root | 当前机器可运行且测试避开真实编译成功依赖；CI 或换机时可能需要 legacy runtime fixture | Code Quality Review 标记为非阻塞；测试使用 `D:\CODE\ATE\01_code\ATS\bin\Debug` | 后续如需要 CI 化或跨机运行，设计可复制 fixture |
| W2-5-Q3 | W2-5 暂未实现正式覆盖保存、另存为、备份和保存日志 | 不影响“保存测试副本”样板门禁；全量流程编辑迁移时仍需处理正式保存语义 | public `Save(...)` 在样板阶段统一拒绝，只允许 `SaveSampleCopy`；W2-5 报告记录范围 | 后续正式流程编辑迁移前单独确认覆盖保存和备份行为 |
| W2-6-Q1 | W2-6 仅做 XAML 结构测试，未执行运行时 UI 自动化 smoke | 不影响 W2-6 状态门禁；但不能把结构测试等同于截图/运行时交互验收 | W2-6 目标测试 9/9、全量测试 80/80；`FlowEditorViewStructureTests` 锁定 XAML 声明状态 | W2-7 执行前需确认 UI 自动化 smoke 和人工验收清单范围 |
| W2-7-Q1 | W2-7 未使用 FlaUI 做真实点击导航 | 自动/半自动证据只证明启动、窗口标题、XAML/导航结构和未迁移入口禁用；不能宣称已完成真实点击 E2E | W2-7 目标 smoke/结构测试 10/10，启动探活脚本识别 `测试系统` 并停止进程；人工点击项在 `14-ui-parity-sample-checklist.md` 标为待人工确认 | 如要升级到 FlaUI 或完整点击 E2E，需单独确认依赖、窗口稳定性和业务隔离策略 |
| WP-06-Q1 | 权限初始加载异常是否必须完全照搬 WinForms 静默失败 | WinForms `UcAuthority_Load` 对初始加载异常为空 catch；WPF 当前初始加载异常会显示 `ex.Message`，更利于诊断但不是完全静默 | Spec Compliance Review 将其列为非阻断差异；WP-06 目标测试 29/29、全量测试 112/112，通过双评审 | 请确认保持当前 WPF 诊断提示，还是改为仅记录/静默以完全复刻 WinForms 初始加载异常行为 |
| WP-06-Q2 | 权限 CRUD 是否需要补原 `LogHelper` 操作日志分类对照 | 当前未替换日志框架，也未新增敏感日志；但权限新增/修改/删除是否需要落原操作日志分类仍需完整页面验收时确认 | WP-06 报告记录为待确认；本轮范围以 UI 提示和数据库兼容为主 | 后续权限完整收口或日志验收批次确认是否补操作日志对照 |
| WP-07-Q1 | 设备管理页面需要人工点击验收 | 自动测试已覆盖 ViewModel、结构、`.dev` 兼容和禁区扫描，但未执行真实 UI 点击 | WP-07 目标测试 23/23、结构测试 2/2、全量测试 132/132，Spec/Code Review 通过 | 请人工确认导航进入、设备库搜索、TreeView 展开、双击添加、打开/保存/另存为、排序/删除、右侧参数说明 |
| WP-07-Q2 | `FrmDevSave` 右键物理删除 `.dev` 是否迁移 | WinForms 保存弹窗可右键删除 `.dev`，删除后不可恢复；本轮为避免扩大到高风险物理删除未实现 | WP-07 报告记录范围；设备配置读写与保存兼容已完成 | 后续完整设备管理收口时确认是否迁移右键删除入口 |
| WP-07-Q3 | 隐藏的底层驱动导入/删除按钮是否迁移 | WinForms Designer 中默认隐藏；迁移会触碰 `SysCache/Devices` DLL/XML 写删 | WP-07 未迁移该隐藏入口，保持不扩大范围 | 如后续需要启用，必须单独确认驱动导入/删除的权限、备份和验证策略 |
| WP-07-Q4 | 设备库 DLL/XML 全量质量和损坏场景未联机验收 | `DeviceCore.GetDevices()` 读取元数据，未验证所有插件 XML 注释和损坏 DLL/XML 行为 | WP-07 使用真实 Debug root 做样本读取和设备库元数据路径，未实例化设备 | 后续设备管理完整验收或联机前补设备库 corpus/异常场景 |
| WP-07-Q5 | WP-07 legacy integration test 依赖本机 Debug runtime | 测试读取 `D:\CODE\ATE\01_code\ATS\bin\Debug`，跨机/CI 可能不可运行 | Code Quality Re-review 接受为 residual risk；当前机器验证通过 | 跨机/CI 时提供 fixture 或 runtime root 配置 |
| WP-08-Q1 | 产品测试壳和扫码弹窗需要人工点击验收 | 自动测试覆盖 ViewModel 和 XAML 结构，但未执行真实窗口点击和焦点验证 | WP-08 目标/smoke 测试 15/15、全量测试 143/143、build 0 警告 0 错误，Spec Compliance Review 和 Code Quality Review 均通过 | 请人工确认产品测试导航、双通道 Tab、扫码弹窗焦点/回车、确定/取消、窗口置顶和尺寸 |
| WP-08-Q2 | `FrmFlowInfo` 完整流程查看窗口未迁移 | 本轮只做产品测试壳，流程详情窗口如启用需承载或复刻 `UcFlow.OpenFlow(flowName, true)` | WP-08 报告记录未实现，不影响壳和扫码验收 | 后续流程详情按钮启用前单独确认 |
| WP-08-Q3 | NG 复测密码、MES 未启用确认、`MesType=16` BDU 条码规则匹配未实现 | 这些逻辑靠近真实测试链、MES 和客户专项规则，贸然实现会扩大范围 | WP-08 仅实现普通扫码必填、回车和取消状态语义 | 随 ProductTest 执行链或客户专项批次单独设计 |
| WP-08-Q4 | 本轮没有注册 AutoTest 字典 | `SysCache.AutoTest*Dic` 是自动化入口，注册后可能被外部流程调用 | WP-08 禁区扫描确认无 AutoTest 字典注册 | AutoType 9 双通道自动化接入前单独计划和验证 |

## 当前禁止动作

- 未经用户确认，不自动进入下一任务批次、下一 Wave 或全量实现。
- 禁止启动产品测试、GP12、AutoType 9、设备联机、正式覆盖保存或全量迁移。
- 禁止迁移 ProductTest、GP12 MES、AutoType 9、ATSDevice。
- 禁止修改 `D:\CODE\ATE\01_code`。

## WP-06 结论

- WP-06 权限管理已通过 Spec Compliance Review 和 Code Quality Review。
- WP-06 覆盖角色/用户表格列、按钮、默认选中首角色、角色用户联动、权限 tag、删除确认、重复/失败文案、异常提示、legacy `User`/`Role` 表读写和 `Role.Powers` JSON string array。
- WP-06 新鲜验证：`dotnet restore` 成功；`dotnet build --no-restore` 成功，0 警告，0 错误；`dotnet test --no-restore --filter Authority` 29/29 通过；`dotnet test --no-restore` 112/112 通过。
- WP-06 不验收 WP-07/WP-08、ProductTest 执行链、GP12、AutoType 9、设备调试 Host、真实设备初始化或全量 ATSMes/ATSAutoTest/ATSDevice。
- 下一步必须由用户确认：处理 WP-06-Q1/Q2，补人工点击验收，或进入下一个已确认任务批次。

## WP-07 结论

- WP-07 设备配置管理已通过 Spec Compliance Review 和 Code Quality Review。
- WP-07 覆盖设备库树和搜索、`.dev` 打开/保存/另存为、UTF-8 BOM JSON 字段兼容、排序/删除、右侧初始化参数说明、保存/另存日志、配置名路径边界校验。
- WP-07 新鲜验证：`dotnet restore` 成功；`dotnet build --no-restore` 成功，0 警告，0 错误；`dotnet test --no-restore --filter DeviceConfig` 23/23 通过；`dotnet test --no-restore --filter DeviceManagerViewStructureTests` 2/2 通过；`dotnet test --no-restore` 132/132 通过。
- WP-07 不验收设备调试 Host、真实设备初始化、设备插件迁移、ProductTest、MES、AutoTest、GP12、AutoType 9。
- 下一步必须由用户确认：补 WP-07 人工点击验收、处理 WP-07-Q2/Q3，或进入下一个明确任务批次。

## WP-08 结论

- WP-08 产品测试壳和扫码弹窗已完成代码侧实现、自动验证、Spec Compliance Review 和 Code Quality Review。
- WP-08 覆盖 Shell 产品测试导航、`ChannelNum=2` 通道壳、通道标题状态规则、工具栏和核心区域结构、普通扫码弹窗回车/确定/取消/必填语义。
- WP-08 新鲜验证：RED 初次失败证明缺少模块；`dotnet restore` 成功；`dotnet build --no-restore` 成功，0 警告，0 错误；WP-08 目标/smoke 测试 15/15 通过；`dotnet test --no-restore` 143/143 通过；生产 WP-08 禁区扫描无真实执行链命中。
- WP-08 不验收 ProductTestCore 真实执行链、设备初始化、MES 上传、GP12、AutoType 9、ATSMes/ATSAutoTest/ATSDevice、NG 复测密码、MES 未启用确认或 BDU 条码规则匹配。
- 下一步必须由用户确认：补 WP-08 人工点击验收，或进入下一个已确认任务批次；禁止直接进入 Wave 4 产品测试执行链或全量迁移。

## W2-7 结论

- W2-7 UI 自动化 smoke 和人工验收清单已通过 Spec Compliance Review 和 Code Quality Review。
- W2-7 采用 fallback 方案：结构 smoke 测试 + WPF 启动探活脚本 + 人工验收清单；未引入 FlaUI。
- W2-7 覆盖 Shell 启动探活、窗口标题、样板导航 AutomationId/Command、未迁移入口禁用/隐藏、人工验收表。
- W2-7 未执行真实点击导航、不验收 ProductTest、GP12、AutoType 9、设备联机、正式覆盖保存、完整 `ATSMes` provider、`ATSAutoTest`、`ATSDevice` 或 ProductTest。
- W2-7 不解决 W2-1 真实现场 gzip JSON 明细样本缺失问题，该补验仍按 W2-1-Q1 跟踪。
- 下一步必须由用户确认：补人工点击验收、补真实现场明细样本、进入下一 Wave，或进入全量实现门禁复核。
