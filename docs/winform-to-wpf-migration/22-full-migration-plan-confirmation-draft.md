# 全量迁移计划确认稿

更新时间：2026-05-21
状态：确认稿，Wave 2 自动/半自动收口已完成；Wave 3 WP-06/WP-07/WP-08 已完成代码侧验证并通过双评审；Wave 4 未确认，确认前禁止全量实现

## 确认范围

本确认稿用于确认 `D:\CODE\ATE\ATS5.0` 后续全量迁移的路线、顺序、门禁、Work Package 拆分和验证策略。

确认本文件不等于授权一次性全量实现。确认后 Controller 仍必须按 Wave 推进，每个 Wave 开始前单独输出实施计划、测试计划、文件 owner 和禁止扩大范围。

## 不变验收标准

- 原 WinForms 行为是唯一验收标准。
- WPF、MVVM、Prism、HandyControl 只是落地手段。
- 主界面、数据查询、流程编辑、产品测试、设备、MES、自动化的操作习惯必须按原代码复刻。
- 保留现有 log4net 和 `LogHelper` 语义。
- 保留 SQLite、gzip JSON、`.fw/.dev/.adbc/.xlsx`、`ATS.exe.config` 的格式、路径、字段、编码、默认值、时间格式和保存时机。
- 不改变设备调用顺序、MES 上报时机、自动化回调顺序、测试时序。
- 当前流程编辑仍以脚本模式为准，未来工步模式只预留接口，不在本计划确认阶段实现。

## 全量迁移 Wave 顺序

| Wave | 范围 | 前置门禁 | 本 Wave 禁止范围 | 完成证据 |
| --- | --- | --- | --- | --- |
| Wave 0 | 只读资产基线、UI 缺口、样板当前状态 | 已完成 | 禁止业务代码修改 | `16-wave0-baseline-report.md` |
| Wave 1 | Shell、路径、日志、配置、输出资产、未迁移入口门禁 | 已完成 | 禁止产品测试、MES、设备、自动化实现 | `17-wave1-infrastructure-report.md`、23 个测试通过 |
| Wave 2 | 数据查询和流程编辑样板端到端补齐 | 已完成 W2-1..W2-7 自动/半自动收口 | 禁止启用产品测试/MES/设备/自动化入口 | `19-wave2-sample-verification-report.md`、`23`..`28` W2 报告、10/10 target smoke/structure、83/83 full tests、0 warning/error build、startup probe |
| Wave 3 | 权限、设备配置、产品测试壳和扫码弹窗 | 已完成代码侧验证和双评审；遗留门禁继续跟踪 | 禁止产品测试执行、真实设备调试、GP12 自动上传 | `30`、`32`、`33` Wave 3 报告 |
| Wave 4 | 产品测试执行链、GP12 MES | Wave 3 通过，GP12 单独确认 | 禁止 AutoType 9 和设备调试全量铺开 | 产品测试数据保存、GP12 手动/自动上传报告 |
| Wave 5 | AutoType 9 双通道自动化、设备调试 Host | Wave 4 通过，现场或 simulator 条件确认 | 禁止其他 provider 顺手迁移 | AutoType 9 回调顺序、设备初始化/关闭报告 |
| Wave 6 | 工具、其他 MES/AutoTest provider、发布包 | Wave 5 通过 | 禁止删除未验证入口 | 发布资产清单、provider 单项报告、全回归 |

## Work Package 拆分

| WP | Wave | 名称 | Owner 类型 | 禁止扩大 |
| --- | --- | --- | --- | --- |
| WP-00 | Wave 0 | 资产和 UI 基线 | 只读盘点 | 禁止改代码 |
| WP-01 | Wave 1 | Shell/导航/未迁移入口门禁 | Shell worker | 禁止启用业务入口 |
| WP-02 | Wave 1 | 路径/日志/配置基础设施 | Infrastructure worker | 禁止替换日志框架 |
| WP-03 | Wave 2 | 数据查询真实数据样板 | DataQuery worker | 禁止实现 GP12 页面 |
| WP-04 | Wave 2 | 流程脚本模式样板 | Flow worker | 禁止实现工步模式 |
| WP-05 | Wave 2 | UI smoke 和样板验收报告 | QA worker | 禁止改业务实现 |
| WP-06 | Wave 3 | 权限管理 | Authority worker | 禁止改登录策略 |
| WP-07 | Wave 3 | 设备配置管理 | Device config worker | 禁止设备调试 Host |
| WP-08 | Wave 3 | 产品测试壳和扫码弹窗 | ProductTest UI worker | 禁止调用真实 `ProductTestCore.Start` |
| WP-09 | Wave 4 | 产品测试执行链 Adapter | ProductTest core worker | 禁止并行改 Device/MES/AutoTest host |
| WP-10 | Wave 4 | GP12 MES | MES GP12 worker | 禁止迁移其他 MES provider |
| WP-11 | Wave 5 | AutoType 9 双通道自动化 | AutoTest worker | 禁止顺手迁移其他 AutoTest |
| WP-12 | Wave 5 | 设备调试 Host | Device debug worker | 禁止重命名插件目录 |
| WP-13 | Wave 6 | DBC/UDS/CAN/日志导出/迁移工具 | Tools worker | 禁止改设备/MES 核心 |
| WP-14 | Wave 6 | 其他 MES provider | MES provider workers | Host 单 owner |
| WP-15 | Wave 6 | 其他 AutoTest provider | AutoTest provider workers | Host 单 owner |
| WP-16 | Wave 6 | 发布包和关闭过渡入口 | Release/QA worker | 禁止删除未验证入口 |

## 每个 Work Package 的必交付物

- 原 WinForms 路径。
- WPF 目标路径。
- 控件、事件、命令映射。
- 数据读写对照。
- 日志记录对照。
- UI 和交互对照。
- 设备、MES、自动化边界验证。
- 单元测试结果。
- 兼容测试结果。
- UI smoke 或人工验收表。
- Spec Compliance Review。
- Code Quality Review。

## 禁止并行修改

- 禁止同时修改 `SysCache` gateway/委托签名。
- 禁止同时修改 `ProductTestCore` adapter 和 Device/MES/AutoTest host。
- 禁止同时修改 `.fw` 序列化和产品测试流程执行。
- 禁止同时修改日志 adapter 和日志导出目录规则。
- 禁止同时修改配置 adapter 和 MES/AutoTest 配置键。
- 禁止多个 worker 同时修改同一 XAML、ViewModel、Service 或 Adapter。

## 全量实现启动条件

全量实现必须同时满足：

- 用户确认 `10-full-migration-roadmap.md`。
- 用户确认 `20-full-migration-confirmation-checklist.md`。
- 用户确认 `21-full-migration-work-packages.md`。
- 用户确认本确认稿。
- Wave 2 样板自动/半自动收口已完成；遗留人工和高风险门禁继续跟踪。
- 每个 Wave 开始前输出实施任务拆分和测试计划。
- 每个实现 agent 有单一 owner 和明确文件范围。
- 每个实现任务完成后执行构建、测试、数据、日志、UI、设备/MES/自动化验证。
- 每个实现任务完成后进行 Spec Compliance Review 和 Code Quality Review。

## 当前未满足门禁

- 人工 UI 对照尚未全部签核。
- 可读真实 legacy gzip JSON 明细样本尚未补齐。
- GP12 真实或模拟 MES 上传尚未形成完整报告。
- AutoType 9 双通道自动化尚未端到端验证。
- 设备初始化、关闭、互斥、调试尚未联机验证。
- 正式流程覆盖保存尚未验收。
- Wave 3 代码侧已完成并通过双评审；人工点击验收尚未关闭。
- 发布包资产清单尚未验证。

## 计划确认后的下一步

计划确认后，下一步不是全量实现，而是：

1. 确认是否补 Wave 3 人工点击验收：权限、设备配置、产品测试壳和扫码弹窗。
2. 或继续补充 Wave 2 遗留门禁：人工点击导航/UI 签核、可读真实 legacy gzip JSON 明细样本、GP12、AutoType 9、设备在线/调试、正式流程覆盖保存、全量迁移确认。
3. 或单独确认是否进入 Wave 4 产品测试执行链和 GP12 MES。
4. Wave 4 未经用户单独确认前，GP12、AutoType 9、设备调试 Host 和产品测试执行链继续禁止。

## 等待确认

请确认是否接受本确认稿作为后续全量迁移的控制路线，并单独确认下一步是补人工验收还是进入 Wave 4。确认前，本项目继续停留在当前门禁，不执行全量实现。
