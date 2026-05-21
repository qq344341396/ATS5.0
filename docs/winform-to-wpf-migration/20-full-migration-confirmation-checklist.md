# 全量迁移计划确认清单

更新时间：2026-05-21
状态：Wave 2 自动/半自动收口已完成；Wave 3 WP-06/WP-07/WP-08 已完成代码侧验证并通过双评审；Wave 4 未确认，确认前禁止全量实现

本清单用于确认 `10-full-migration-roadmap.md` 是否可以作为后续全量迁移总计划。确认本清单不等于立即全仓开工。

## 确认含义

请确认的是：

- 全量迁移路线。
- Wave 执行顺序。
- 每个 Wave 的前置门禁。
- 验证策略。
- 并行和禁止并行规则。
- 高风险模块单独确认规则。

不是确认：

- 立即全量迁移所有模块。
- 立即启用产品测试、MES、自动化、设备调试入口。
- 跳过 Wave 2 遗留人工和高风险门禁。
- 跳过每个 Wave 的实施计划、测试计划和评审。

## 必须保持的总原则

- [ ] 原 WinForms 行为是唯一验收标准。
- [ ] WPF、MVVM、Prism、HandyControl 只作为落地手段。
- [ ] 不为了现代化 UI 改变操作习惯、保存时机、弹窗逻辑、设备/MES/自动化时序。
- [ ] 保留现有 log4net 和 `LogHelper` 语义。
- [ ] 保留旧数据格式、路径、编码、字段、默认值和时间格式。
- [ ] 设备、MES、自动测试协议和调用顺序不擅自改变。
- [ ] 当前脚本编辑模式不受影响，未来工步模式只预留接口。

## 当前计划产物

- [x] 全量主路线图：`D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\10-full-migration-roadmap.md`
- [x] 总控清单：`D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\00-master-checklist.md`
- [x] Wave 0 基线报告：`D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\16-wave0-baseline-report.md`
- [x] Wave 1 基础设施报告：`D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\17-wave1-infrastructure-report.md`
- [x] Wave 2 样板补齐计划：`D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\18-wave2-sample-parity-execution-plan.md`
- [x] Wave 2 W2-1..W2-7 证据报告：`23`..`28` 和 `19-wave2-sample-verification-report.md`
- [x] Wave 3 执行计划和结果：`D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\29-wave3-execution-plan.md`

## 全量主计划必须覆盖项

- [x] 所有 WinForms 功能清单。
- [x] 每个功能对应的新 WPF 目标。
- [x] 迁移顺序。
- [x] 每个功能的风险等级。
- [x] 每个功能的数据保存验证点。
- [x] 每个功能的日志验证点。
- [x] 每个功能的 UI/交互验证点。
- [x] 设备/MES/自动测试边界验证点。
- [x] 每个功能完成标准。
- [x] 每个功能可分配的 subagent 范围。
- [x] 禁止并行修改的文件或模块。
- [x] 回归测试策略。
- [x] 实际代码入口附录：ATS 页面、ATSMes provider、ATSAutoTest provider、ATSDevice 插件。

## Wave 门禁确认

- [x] Wave 0：只读基线已完成。
- [x] Wave 1：Debug 基础设施门禁已完成。
- [x] Wave 2：W2-1..W2-7 已完成并双评审；target smoke/structure tests 10/10、full tests 83/83、build 0 warnings/errors、startup probe passed。
- [ ] Wave 2 遗留门禁：人工点击导航/UI 签核、可读真实 legacy gzip JSON 明细样本、GP12、AutoType 9、设备在线/调试、正式流程覆盖保存、全量迁移确认仍未关闭。
- [x] Wave 3：权限、设备配置、产品测试壳已完成代码侧验证和双评审；人工点击验收仍未关闭。
- [ ] Wave 4：产品测试执行链和 GP12 MES，需 Wave 3 完成后单独确认。
- [ ] Wave 5：AutoType 9 和设备调试，需 Wave 4 完成后单独确认。
- [ ] Wave 6：工具、其他 provider、发布包，需前置 Wave 完成后单独确认。

## 高风险专项确认

- [ ] `ProductTest` 不允许在 Wave 2 前实现。
- [ ] `ATSMes\DB_GP12` 不允许在 Wave 4 前实现。
- [ ] `ATSAutoTest\DB-XMorZP22` AutoType 9 不允许在 Wave 5 前实现。
- [ ] `ATSDevice` 设备调试 Host 不允许在 Wave 5 前实现。
- [ ] 任何 provider 不允许跳过单独验证报告。

## 计划确认后下一步

如果用户确认全量主计划，下一步仍然不是全量实现，而是二选一：

1. 确认是否补 Wave 3 人工点击验收，关闭权限、设备配置、产品测试壳的现场 UI 签核。
2. 单独确认是否进入 Wave 4 产品测试执行链和 GP12 MES；确认前不得实施。
3. 或继续补 Wave 2 遗留人工/高风险门禁、补文档或调整计划。

## 等待用户确认

请确认是否接受 `10-full-migration-roadmap.md` 作为后续全量迁移总计划，并单独确认下一步是补人工验收还是进入 Wave 4。确认前，停止在当前门禁，不执行全量实现。
