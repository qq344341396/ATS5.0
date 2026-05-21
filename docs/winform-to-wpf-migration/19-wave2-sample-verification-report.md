# Wave 2 W2-7 UI 自动化 Smoke 和人工验收清单

更新时间：2026-05-21
执行范围：仅 QA/verification 文件；不修改生产 View、ViewModel、Service、Repository；不修改 `D:\CODE\ATE\01_code`。

## Smoke 方案

本轮未引入 FlaUI。当前仓库没有稳定可复用的 UI 自动化依赖，W2-7 采用低依赖 fallback：

| 层级 | 覆盖内容 | 证据 |
| --- | --- | --- |
| XAML 结构测试 | Shell 导航 AutomationId、启用样板入口、未迁移入口禁用/隐藏、ContentRegion | `ATS5.Tests\Infrastructure\WpfSmokeTests.cs` |
| 导航结构测试 | `NavigateDataQueryCommand`、`NavigateFlowCommand` 请求 Prism `ContentRegion` 中的样板 View 名 | `ATS5.Tests\Infrastructure\WpfSmokeTests.cs` |
| 输出路径门禁 | WPF 项目保持 `WinExe`、`net461`、`UseWPF=true`，脚本定位 `ATS5.Wpf\bin\<Configuration>\net461\ATS5.Wpf.exe` | `ATS5.Tests\Infrastructure\WpfSmokeTests.cs` |
| 启动探活脚本 | 启动 WPF exe，等待主窗口句柄和标题，随后主动结束进程 | `tests\ui-smoke\Assert-WpfSampleUiSmoke.ps1` |

## 自动或半自动证据命令

| 命令 | 预期 |
| --- | --- |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj --no-restore --filter "FullyQualifiedName~WpfSmokeTests\|FullyQualifiedName~ShellNavigationStateTests\|FullyQualifiedName~DataQueryViewStructureTests\|FullyQualifiedName~FlowEditorViewStructureTests"` | 样板导航、XAML 结构和禁用状态测试通过 |
| `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | WPF 样板解决方案构建通过 |
| `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore` | 全量测试通过 |
| `powershell -ExecutionPolicy Bypass -File D:\CODE\ATE\ATS5.0\tests\ui-smoke\Assert-WpfSampleUiSmoke.ps1 -Configuration Debug -TimeoutSeconds 15` | 进程启动、主窗口句柄和标题探活通过；脚本主动停止进程 |

## 人工验收入口

人工验收清单集中维护在 `docs\winform-to-wpf-migration\14-ui-parity-sample-checklist.md` 的 W2-7 小节。

## 边界确认

- 不点击产品测试、MES、设备、自动测试或正式流程保存路径。
- 不新增业务行为，不启用未迁移按钮。
- 不读取或修改 `D:\CODE\ATE\01_code` 内容。
- 若后续要升级为 FlaUI/E2E 点击测试，应先单独确认依赖、窗口稳定性和业务隔离策略。
