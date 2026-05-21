# Wave 4 ProductTest Execution And GP12 MES Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement Wave 4 only after explicit user confirmation: WP-09 ProductTest execution chain adapter first, then WP-10 GP12 MES, preserving WinForms execution, data, logging, MES, device, and AutoTest timing.

**Architecture:** Adapter-first migration around legacy `ProductTestCore`, `SysCache`, DBCore, `LogHelper`, and GP12 provider behavior. WPF ViewModels may project state and issue commands, but they must not rewrite test execution, data saving, MES upload, device initialization, or AutoTest callback semantics.

**Tech Stack:** .NET Framework WPF, MVVM, Prism, HandyControl, xUnit, existing legacy assemblies from `D:\CODE\ATE\01_code\ATS\bin\Debug`, log4net through existing `LogHelper`.

---

更新时间：2026-05-21
状态：Wave 4 实施计划草案；本文件不授权实现。用户确认前只能继续只读复核、评审本计划、补文档或补验证脚本设计。

## 0. Current Gate

Wave 3 当前代码侧状态：

- WP-06 权限管理：已完成并双评审。
- WP-07 设备配置管理：已完成并双评审。
- WP-08 产品测试壳和扫码弹窗：已完成并双评审。
- Wave 3 人工点击验收仍未关闭，尤其产品测试导航、双通道 Tab、扫码弹窗焦点/回车、确定/取消、窗口置顶和尺寸。

Wave 4 不等于全量迁移。进入 Wave 4 前必须再次确认：

- 用户确认本文件。
- 用户确认先执行 WP-09，再执行 WP-10。
- 用户确认 GP12 范围只覆盖 `MesType="12"` 的 `UcMain_DB_GP12_EVB` 行为，不迁移其他 MES provider。
- 用户确认本轮仍不进入 AutoType 9、设备调试 Host、其他 AutoTest/MES provider 或全量 `ATSMes`/`ATSAutoTest`/`ATSDevice`。
- 用户确认 WP-08 人工点击验收作为 residual risk 继续跟踪，不作为本次 Wave 4 代码实施前置阻塞。

## 1. Source Baseline

### ProductTest baseline files

- `D:\CODE\ATE\01_code\ATS\ProductTest\UcProductTest.cs`
- `D:\CODE\ATE\01_code\ATS\ProductTest\UcProductTest.Designer.cs`
- `D:\CODE\ATE\01_code\ATS\ProductTest\UcTestMain.cs`
- `D:\CODE\ATE\01_code\ATS\ProductTest\FrmBarcode.cs`
- `D:\CODE\ATE\01_code\ATSCore\ProductTestCore.cs`
- `D:\CODE\ATE\01_code\ATSCore\SysCache.cs`
- `D:\CODE\ATE\01_code\ATSCore\DBCore\IndexInfoCore.cs`
- `D:\CODE\ATE\01_code\ATSCore\DBCore\TestProjectDataCore.cs`
- `D:\CODE\ATE\01_code\ATSCommon\LogHelper.cs`
- `D:\CODE\ATE\01_code\ATSCommon\ConfigHelper.cs`
- `D:\CODE\ATE\01_code\ATSCommon\JsonHelper.cs`

### GP12 baseline files

- `D:\CODE\ATE\01_code\ATSMes\UcMesMain.cs`
- `D:\CODE\ATE\01_code\ATSMes\DB_GP12\UI\UcMain_DB_GP12_EVB.cs`
- `D:\CODE\ATE\01_code\ATSMes\DB_GP12\UI\UcMain_DB_GP12_EVB.Designer.cs`
- `D:\CODE\ATE\01_code\ATSMes\DB_GP12\Model\MesService.cs`
- `D:\CODE\ATE\01_code\ATSMes\DB_GP12\Model\*.cs`
- `D:\CODE\ATE\01_code\ATSModel\*.cs`

### Observed hard sequence from code

The following sequence is a Wave 4 acceptance line. Any implementation or review that changes it must stop.

1. `UcProductTest.btnStartAll_ItemClick` checks register and pencil, starts the `ReleaseSingleTest` background thread with the old `flowName`, opens `FrmBarcode`, refreshes flow statistics, runs `CheckSwtichConfig()`, calls `CheckBarcode`, calls `PreTestMesCheck`, waits `thRelease.Join()`, optionally calls `SysCache.StartTest`, then calls `StartTest(flowName, lstBarcode)`.
2. `StartTest` rejects missing `.fw`, rejects open `FrmDevDebug`, sets `SysCache.IsProjectSyncManagerEnable`, updates `SysCache.DicChannelStatus[ch]`, creates `ProductTestCore` on the UI thread, subscribes callbacks, assigns PLC, binds `LstTestInfo` and DBC realtime data, then starts a background test thread.
3. `ProductTestCore.ExecuteTest` creates and compiles dynamic C# with `MakeUpCode()` and `CompileCode(code)`, instantiates `ATSEngine.TestEngine`, assigns DBC/UDS/PLC, calls dynamic `obj.Init()`, sets `CurrentLogguid = yyyyMMddHHmmss`, calls `TestStatusChange(Start)`, `FreshTestRes(Start)`, and `InsertIndexInfo()`.
4. `ExecuteTest` loops projects, invokes generated `RunN` methods, applies NG continue/retest behavior, project sync waits, `DetermineTestResults()`, `UpdateIndexInfo(lstRes)`, `SaveTestData()`, then `AutoExport()`, and finally calls `StopTest()` to close generated devices.
5. After `ExecuteTest()` returns, `UcProductTest.StartTest` calls `SysCache.UploadData(testCore.CurrentLogguid, ch)` only when compile and init succeeded, then calls `IndexInfoCore.UpdateMesInfo(guid, ch, mesEnable && resModel.Status ? 1 : 0)`. If upload delegate is missing or compile/init failed, it calls `IndexInfoCore.UpdateMesInfo(guid, ch, 0)` and treats MES result as true for UI completion.
6. `FreshTestRes(testCore.DetermineTestResults(), TestStatus.Complete)` and `SysCache.AutoTestMESResult?.Invoke(ch, mesUpLoadResult)` happen after upload result handling.
7. Stop path sets `mesUpLoadResult = false`, saves STOP data when `CurrentLogguid != 0` by calling `UpdateIndexInfo(new List<string> { "STOP" })`, `DetermineTestResults()`, `SaveTestData()`, aborts the test thread, and calls `testCore.StopTest()`.

### GP12 delegate registration from code

`ATSMes\UcMesMain.cs` maps `MesType="12"` to `UcMain_DB_GP12_EVB` and registers:

- `SysCache.SendChannelBarcode = RecChannelBarcode`
- `SysCache.BarcodeCheck = BarcodeCheck`
- `SysCache.LoginMes = Login`
- `SysCache.StationCheck = GroupTest`
- `SysCache.UploadData = UploadData`
- `SysCache.DeviceStatusUploadMES = DeviceStatus`
- `SysCache.EquipmentAlarmUploadMES = EquipmentAlarm`
- `SysCache.GetBarcode = GetTaryBarcode`
- `SysCache.GetOrderNo = GetFazit`

## 2. Scope

### In scope after user confirmation

- WP-09 ProductTest execution chain adapter and tests.
- WP-10 GP12 MES adapter/view/service and tests.
- WPF ProductTest shell command wiring only where needed to invoke WP-09 through an Application service.
- GP12 UI surface only for `UcMain_DB_GP12_EVB` parity, including configuration fields, enable/edit/save/cancel behavior, log/message table projection, status controls, and delegate registration.
- Fake/simulator validation for MES and device boundaries before any real provider call.

### Out of scope

- AutoType 9 implementation.
- Device debug Host.
- Full `ATSMes` provider migration.
- Full `ATSAutoTest` provider migration.
- Full `ATSDevice` or plugin migration.
- Flow editor full coverage save.
- Other MES providers: `DQ`, `DB_BDU`, `DB_EVB`, `KST`, `SR`, etc.
- Other AutoTest providers.
- Any change to `D:\CODE\ATE\01_code`.
- Any replacement of log4net or `LogHelper`.
- Any change to SQLite schema, gzip JSON format, `.fw/.dev/.adbc/.xlsx`, `ATS.exe.config`, test data path, log path, or save timing.

## 3. Ownership And Parallel Rules

Wave 4 must be executed serially:

1. WP-09 ProductTest execution chain adapter.
2. WP-09 Spec Compliance Review.
3. WP-09 Code Quality Review.
4. WP-09 report and verification.
5. WP-10 GP12 MES.
6. WP-10 Spec Compliance Review.
7. WP-10 Code Quality Review.
8. Wave 4 summary report and stop.

WP-09 and WP-10 are not parallel tasks because they share:

- `SysCache.UploadData`
- `SysCache.SendChannelBarcode`
- `IndexInfoCore.UpdateMesInfo`
- ProductTest save/upload timing
- GP12 current barcode dictionary semantics
- Log and UI message ordering

### Allowed write areas for WP-09

- `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTest\*`
- `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\ProductTest\*`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\ViewModels\ProductTestShellViewModel.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\ViewModels\ProductTestChannelViewModel.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\Views\ProductTestShellView.xaml`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\ProductTest\*`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\ProductTestShell\*` only for Start/Stop/Pause command wiring tests against existing WP-08 shell behavior; barcode dialog layout/result semantics remain unchanged.
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\*ProductTest*`
- `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\35-wave4-wp09-product-test-execution-report.md`

### Allowed write areas for WP-10

- `D:\CODE\ATE\ATS5.0\ATS5.Application\Mes\*`
- `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\Mes\*`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Mes\*`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\Mes\*`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\*Mes*`
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf\App.xaml.cs` only for Prism module registration, after explicit owner handoff.
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf\Views\Shell.xaml` only for enabling/navigating the existing MES entry, after explicit owner handoff.
- `D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln` and related project files only when adding `ATS5.Modules.Mes`.
- `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\36-wave4-wp10-gp12-mes-report.md`

### Forbidden write areas in Wave 4

- `D:\CODE\ATE\01_code\**`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.AutoTest\**`
- `D:\CODE\ATE\ATS5.0\ATS5.Modules.Device\**` except read-only references; device debug Host is Wave 5.
- `D:\CODE\ATE\ATS5.0\ATS5.Application\Flow\**`
- `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\Flow\**`
- Any shared shell/navigation file not listed above.
- Any legacy `SysCache` delegate signature, data model, database schema, log framework, or serialization helper.

## 4. Target File Structure

### WP-09 target structure

- Create `ATS5.Application\ProductTest\ProductTestSessionRequest.cs`
  - Immutable request values: channel, flow name, barcodes, and flags for fake/simulator use. Wave 4 does not implement single-step execution; any field reserved for future single-step support must remain unused until separately confirmed.
- Create `ATS5.Application\ProductTest\ProductTestSessionResult.cs`
  - Legacy-compatible output: log guid, channel, test results, MES result, messages, final command state, stop/failed flags.
- Create `ATS5.Application\ProductTest\ProductTestStatusMessage.cs`
  - UI-safe projection of `ShowTestMsg`, `TestStatusChange`, `FreshTestData`, and `FreshTestRes`.
- Create `ATS5.Application\ProductTest\IProductTestSessionService.cs`
  - Public API for start, stop, pause/resume, state query, and event subscription abstraction.
- Create `ATS5.Application\ProductTest\IProductTestExecutionAdapter.cs`
  - Abstraction over legacy `ProductTestCore` execution, test data callbacks, save/upload result timing, and stop behavior.
- Create `ATS5.Application\ProductTest\IProductTestMesUploadGateway.cs`
  - Wrapper for `SysCache.UploadData` and `IndexInfoCore.UpdateMesInfo` behavior.
- Create `ATS5.Application\ProductTest\IProductTestAutomationGateway.cs`
  - Wrapper for `SysCache.StartTest`, `AutoTestResult`, `AutoTestMESResult`, and state dictionaries, but do not implement AutoType 9.
- Create `ATS5.Application\ProductTest\ProductTestSessionService.cs`
  - Orchestrates the existing WinForms order without owning legacy static state directly.
- Create `ATS5.Infrastructure.LegacyAdapters\ProductTest\LegacyProductTestExecutionAdapter.cs`
  - Instantiates `ProductTestCore`, wires callbacks, calls `ExecuteTest()`, and exposes deterministic event sequence.
- Create `ATS5.Infrastructure.LegacyAdapters\ProductTest\LegacyProductTestMesUploadGateway.cs`
  - Calls `SysCache.UploadData`, then `IndexInfoCore.UpdateMesInfo` using WinForms status rules.
- Create `ATS5.Infrastructure.LegacyAdapters\ProductTest\LegacyProductTestAutomationGateway.cs`
  - Calls existing delegates when present; fake/test implementation records order.
- Test `ATS5.Tests\ProductTest\ProductTestSessionServiceTests.cs`
- Test `ATS5.Tests\ProductTest\LegacyProductTestExecutionAdapterTests.cs`
- Test `ATS5.Tests\ProductTest\ProductTestUploadOrderingTests.cs`
- Test `ATS5.Tests\Infrastructure\ProductTestForbiddenScopeTests.cs`

### WP-10 target structure

- Create `ATS5.Application\Mes\MesOperationResult.cs`
- Create `ATS5.Application\Mes\Gp12MesSettings.cs`
- Create `ATS5.Application\Mes\Gp12MesMessage.cs`
- Create `ATS5.Application\Mes\IGp12MesService.cs`
- Create `ATS5.Application\Mes\IGp12MesSettingsRepository.cs`
- Create `ATS5.Application\Mes\IGp12MesDelegateRegistrar.cs`
- Create `ATS5.Infrastructure.LegacyAdapters\Mes\LegacyGp12MesSettingsRepository.cs`
  - Reads/writes existing `ConfigHelper` keys only, with original save timing.
- Create `ATS5.Infrastructure.LegacyAdapters\Mes\LegacyGp12MesDelegateRegistrar.cs`
  - Registers only the `MesType="12"` delegate set listed in this plan.
- Create `ATS5.Infrastructure.LegacyAdapters\Mes\LegacyGp12MesGateway.cs`
  - Adapter around GP12 behavior. The first implementation should favor wrapping the legacy provider object where safe; any extraction of GP12 request-building logic must prove identical input/output through tests.
- Create `ATS5.Modules.Mes\ATS5.Modules.Mes.csproj`
- Create `ATS5.Modules.Mes\MesModule.cs`
- Create `ATS5.Modules.Mes\ViewModels\Gp12MesViewModel.cs`
- Create `ATS5.Modules.Mes\Views\Gp12MesView.xaml`
- Create `ATS5.Modules.Mes\Views\Gp12MesView.xaml.cs`
- Test `ATS5.Tests\Mes\Gp12MesSettingsRepositoryTests.cs`
- Test `ATS5.Tests\Mes\Gp12MesDelegateRegistrationTests.cs`
- Test `ATS5.Tests\Mes\Gp12MesUploadDataTests.cs`
- Test `ATS5.Tests\Infrastructure\Gp12MesViewStructureTests.cs`
- Test `ATS5.Tests\Infrastructure\MesForbiddenScopeTests.cs`

## 5. Mapping Tables

### WP-09 ProductTest mapping

| WinForms source | WPF target | Rule |
| --- | --- | --- |
| `btnStartAll_ItemClick` | `ProductTestChannelViewModel.StartCommand` -> `IProductTestSessionService.StartAsync` | Preserve register/pencil/barcode/pre-MES/automation/start order. |
| `FrmBarcode` result | existing `IBarcodeDialogService` | WP-09 may reuse WP-08 dialog only; no dialog behavior changes without tests. |
| `ReleaseSingleTest` thread | `IProductTestSessionService` pre-start step | Start after pencil check with old flow name and wait with `Join()` after pre-MES succeeds and before `SysCache.StartTest`. |
| `CheckSwtichConfig()` | `IProductTestSessionService` pre-start step | Run after barcode dialog updates flow/test info and before `CheckBarcode`. |
| `CheckBarcode` | `IProductTestSessionService` + `IProductTestMesUploadGateway` | Call `SendChannelBarcode` before `BarcodeCheck`, then refresh barcode UI projection. |
| `PreTestMesCheck` | `IProductTestSessionService` | Call `LoginMes`, `GetOrderNo`, `OrderNoCheck`, `StationCheck`, `GetMESDownloadPara` in original order. |
| `StartTest(flow, barcodes)` | `ProductTestSessionService.StartAsync` | Reject duplicate running state, missing `.fw`, and open device debug state before execution. |
| `new ProductTestCore(flow, barcodes, ch)` | `LegacyProductTestExecutionAdapter` | Create on UI dispatcher if legacy binding requires it; callbacks must marshal to UI/EventAggregator. |
| `testCore.ExecuteTest()` | `IProductTestExecutionAdapter.ExecuteAsync` | Keep dynamic compile, init, project loop, save and auto export inside legacy core. |
| `SysCache.UploadData(logGuid, ch)` | `LegacyProductTestMesUploadGateway.UploadAfterLocalSave` | Must happen after `ExecuteTest()` returned and only when compile/init succeeded. |
| `IndexInfoCore.UpdateMesInfo` | `LegacyProductTestMesUploadGateway` | Status rule: `mesEnable && uploadStatus ? 1 : 0`; missing upload delegate path uses `0`. |
| `FreshTestData`, `FreshTestRes`, `TestStatusChange` | ProductTest events -> ViewModel state | Preserve message order; UI may be projected but not business-reordered. |
| `StopTest` | `StopCommand` -> `IProductTestSessionService.StopAsync` | Save STOP data when `CurrentLogguid != 0`, then abort/stop/close in legacy order. |
| `btnPause` suspend/resume | `PauseCommand` | Preserve caption/state semantics; implementation must not introduce async deadlock. |
| `AutoTestResult`, `AutoTestMESResult` | `IProductTestAutomationGateway` | Do not implement AutoType 9; only preserve callback slots and order. |

### WP-10 GP12 mapping

| WinForms source | WPF target | Rule |
| --- | --- | --- |
| `UcMesMain` `MesType="12"` switch | `IGp12MesDelegateRegistrar.Register` | Register only GP12 delegate set, no other provider. |
| `SetUI(false/true)` | `Gp12MesViewModel.IsEditing` | Preserve edit/save/cancel visibility and operator disabled fields. |
| `btnSave_Click` | `SaveCommand` -> settings repository | Preserve config keys and save timing. |
| `RecChannelBarcode` | `IGp12MesService.RecChannelBarcode` | Preserve per-channel dictionary and message text. |
| `BarcodeCheck` | `IGp12MesService.BarcodeCheck` | Preserve `*` wildcard rule and disabled-rule success message. |
| `Login` | `IGp12MesService.Login` | Preserve disabled-MES success and request fields. |
| `GroupTest` | `IGp12MesService.GroupTest` | Preserve GetMain/GetMAC then new/old MES check-in branch. |
| `UploadData` | `IGp12MesService.UploadData` | Preserve local `IndexInfoCore.QueryFirst`, upload type gate, test data conversion, equipment parameter upload, and result message. |
| `DeviceStatus` | `IGp12MesService.DeviceStatus` | Preserve status code map and 30s standby behavior only when explicitly implementing status loop. |
| `EquipmentAlarm` | `IGp12MesService.EquipmentAlarm` | Preserve delegate shape and disabled-MES result. |
| `ShowMsg` | `Gp12MesMessage` -> WPF message grid/log | Preserve `LogHelper.Mes` use and message text. |

## 6. WP-09 Tasks

### Task 1: Read-only behavior lock

**Files:**
- Read: `D:\CODE\ATE\01_code\ATS\ProductTest\UcProductTest.cs`
- Read: `D:\CODE\ATE\01_code\ATSCore\ProductTestCore.cs`
- Read: `D:\CODE\ATE\01_code\ATSCore\SysCache.cs`
- Read: `D:\CODE\ATE\01_code\ATSCore\DBCore\IndexInfoCore.cs`
- Read: `D:\CODE\ATE\01_code\ATSCore\DBCore\TestProjectDataCore.cs`
- Modify: `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\35-wave4-wp09-product-test-execution-report.md`

- [ ] **Step 1: Extract exact ProductTest sequence**

  Record start, pause, stop, pre-MES, upload, save, auto export, callback, and AutoTest paths with method names and line references.

- [ ] **Step 2: Record risks**

  Include duplicate start, `ReleaseSingleTest`, `CheckSwtichConfig`, `thRelease.Join`, open `FrmDevDebug`, missing `.fw`, compile failure, init failure, stopped test, missing upload delegate, MES disabled, upload fail, NG continue/retest, project sync, DBC/UDS, PLC, PLC M2 gate, and `DevicePool.ClearObj`.

- [ ] **Step 3: Stop if behavior is unclear**

  If any required branch cannot be traced from code, stop and ask the user before writing implementation.

### Task 2: Write failing service and ordering tests

**Files:**
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Tests\ProductTest\ProductTestSessionServiceTests.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Tests\ProductTest\ProductTestUploadOrderingTests.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\ProductTestForbiddenScopeTests.cs`

- [ ] **Step 1: Write test for start order**

  Expected sequence:

  ```text
  CheckRegister
  CheckPencil
  ReleaseSingleTest thread starts with previous flowName
  BarcodeDialog
  Load flow test info
  CheckSwtichConfig
  CheckBarcode
  PreTestMesCheck
  ReleaseSingleTest Join
  SysCache.StartTest when present
  CreateProductTestCore
  ExecuteTest
  UploadData after ExecuteTest only when compile/init succeeded
  UpdateMesInfo
  FreshTestRes Complete
  AutoTestMESResult
  ```

- [ ] **Step 2: Write test for local save before upload**

  Use a fake execution adapter that records `InsertIndexInfo`, `UpdateIndexInfo`, `SaveTestData`, `AutoExport`, then a fake MES gateway. Assert upload happens after local save markers.

- [ ] **Step 3: Write test for compile/init failure**

  Fake execution returns `IsCompileAndInit=false`; assert `UploadData` is not called and MES info update follows WinForms fallback.

- [ ] **Step 4: Write test for stop path**

  Fake current logguid nonzero; assert STOP update/save then adapter stop.

- [ ] **Step 5: Write forbidden-scope scan**

  Scan WP-09 implementation paths and assert no GP12 provider implementation, no AutoType 9, no device debug Host, no `ATSAutoTest`, no `ATSDevice` provider migration.

- [ ] **Step 6: Run tests to verify red**

  Run:

  ```powershell
  dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter "ProductTestSessionService|ProductTestUploadOrdering|ProductTestForbiddenScope"
  ```

  Expected before implementation: FAIL because target ProductTest application services and adapters do not exist.

### Task 3: Implement ProductTest application abstractions

**Files:**
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTest\ProductTestSessionRequest.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTest\ProductTestSessionResult.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTest\ProductTestStatusMessage.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTest\IProductTestSessionService.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTest\IProductTestExecutionAdapter.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTest\IProductTestMesUploadGateway.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTest\IProductTestAutomationGateway.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTest\ProductTestSessionService.cs`

- [ ] **Step 1: Define DTOs and interfaces**

  Keep DTOs small and explicit. Public API must have XML comments.

- [ ] **Step 2: Implement orchestration with fake-friendly dependencies**

  The service must not reference `SysCache`, `ProductTestCore`, DBCore, `LogHelper`, WPF controls, or HandyControl directly.

- [ ] **Step 3: Run target tests**

  Run the same ProductTest filter. Expected: service-level tests pass, legacy adapter tests may still fail until Task 4.

### Task 4: Implement legacy ProductTest adapters

**Files:**
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\ProductTest\LegacyProductTestExecutionAdapter.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\ProductTest\LegacyProductTestMesUploadGateway.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\ProductTest\LegacyProductTestAutomationGateway.cs`
- Modify: `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\ATS5.Infrastructure.LegacyAdapters.csproj`

- [ ] **Step 1: Add compile references only if required**

  Reference existing legacy DLLs from Debug output. Do not reference source projects under `D:\CODE\ATE\01_code`.

- [ ] **Step 2: Wrap `ProductTestCore`**

  Instantiate with flow, barcode list, and channel. Wire `ShowTestMsg`, `TestStatusChange`, `FreshTestData`, `FreshTestRes`. `ProductTestCore.PLC` must receive the same PLC instance or PLC boundary as WinForms. If production implementation cannot preserve PLC assignment, `PLC_M2_Enable` gate behavior, and dynamic engine `Plc` propagation, stop and ask. A null PLC path is allowed only in explicitly named simulator tests and must not be reported as production parity.

- [ ] **Step 3: Preserve upload and `UpdateMesInfo` semantics**

  Implement WinForms fallback rules exactly.

- [ ] **Step 4: Preserve predictable exception handling**

  Handle known legacy failures only where WinForms does; do not swallow unexpected adapter errors silently.

- [ ] **Step 5: Run target tests**

  Run:

  ```powershell
  dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter "ProductTest"
  ```

### Task 5: Wire ProductTest UI commands minimally

**Files:**
- Modify: `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\ViewModels\ProductTestShellViewModel.cs`
- Modify: `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\ViewModels\ProductTestChannelViewModel.cs`
- Modify: `D:\CODE\ATE\ATS5.0\ATS5.Modules.ProductTest\Views\ProductTestShellView.xaml`
- Modify tests under: `D:\CODE\ATE\ATS5.0\ATS5.Tests\ProductTestShell\*`

- [ ] **Step 1: Connect `StartCommand` to service**

  Reuse WP-08 barcode dialog behavior. Do not change barcode dialog layout or result semantics.

- [ ] **Step 2: Connect stop and pause state**

  Preserve WinForms caption/state meaning in ViewModel properties.

- [ ] **Step 3: Project legacy callbacks to UI state**

  Messages and test data updates must preserve order. If UI thread dispatch is needed, centralize it.

- [ ] **Step 4: Run ProductTest shell and execution tests**

  Run:

  ```powershell
  dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter "ProductTest|ProductTestShell"
  ```

### Task 6: WP-09 verification and report

**Files:**
- Modify: `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\35-wave4-wp09-product-test-execution-report.md`
- Modify: `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\00-master-checklist.md`
- Modify: `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\11-open-questions.md`

- [ ] **Step 1: Run restore**

  ```powershell
  dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln
  ```

- [ ] **Step 2: Run build**

  ```powershell
  dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
  ```

- [ ] **Step 3: Run targeted tests**

  ```powershell
  dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter "ProductTest|ProductTestShell"
  ```

- [ ] **Step 4: Run full tests**

  ```powershell
  dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
  ```

- [ ] **Step 5: Run forbidden scans**

  ```powershell
  rg -n -e 'AutoType\s*9' -e 'ATSAutoTest' -e 'ATSDevice' -e 'FrmDevDebug' -e 'UcMain_DB_GP12_EVB' -e 'DB_GP12' -- ATS5.Application\ProductTest ATS5.Infrastructure.LegacyAdapters\ProductTest ATS5.Modules.ProductTest
  ```

  Expected: no production WP-09 implementation hit except allowed string assertions in tests or documentation.

- [ ] **Step 6: Write WP-09 report**

  Include original paths, WPF paths, mapping, data compatibility, log validation, UI validation, device/MES/AutoTest impact, tests, screenshots/manual checklist if used, known differences, and whether WP-09 can be used as template.

- [ ] **Step 7: Run Spec Compliance Review**

  Reviewer must focus on original WinForms behavior, sequence, data saving, log semantics, device/MES/AutoTest boundary, and scope.

- [ ] **Step 8: Run Code Quality Review**

  Reviewer must focus on C# naming, nullable, async/cancellation, MVVM/Prism separation, adapter boundaries, tests, and maintainability.

- [ ] **Step 9: Fix review findings and re-review**

  Do not proceed to WP-10 with Critical or Important findings open.

## 7. WP-10 Tasks

### Task 1: Read-only GP12 behavior lock

**Files:**
- Read: `D:\CODE\ATE\01_code\ATSMes\UcMesMain.cs`
- Read: `D:\CODE\ATE\01_code\ATSMes\DB_GP12\UI\UcMain_DB_GP12_EVB.cs`
- Read: `D:\CODE\ATE\01_code\ATSMes\DB_GP12\UI\UcMain_DB_GP12_EVB.Designer.cs`
- Read: `D:\CODE\ATE\01_code\ATSMes\DB_GP12\Model\MesService.cs`
- Modify: `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\36-wave4-wp10-gp12-mes-report.md`

- [ ] **Step 1: Extract GP12 controls and settings**

  Capture all visible fields, hidden/conditional fields, save keys, edit/cancel state, operator disabled fields, status radio behavior, and message grid behavior.

- [ ] **Step 2: Extract delegate and API sequence**

  Capture `RecChannelBarcode`, `BarcodeCheck`, `Login`, `GroupTest`, `UploadData`, `DeviceStatus`, `EquipmentAlarm`, `GetTaryBarcode`, `GetFazit`, and `EquipmentParameters`.

- [ ] **Step 3: Stop if behavior is unclear**

  If any config key or GP12 request branch cannot be traced, stop and ask before implementing.

### Task 2: Write failing GP12 tests

**Files:**
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Tests\Mes\Gp12MesSettingsRepositoryTests.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Tests\Mes\Gp12MesDelegateRegistrationTests.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Tests\Mes\Gp12MesUploadDataTests.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\Gp12MesViewStructureTests.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\MesForbiddenScopeTests.cs`

- [ ] **Step 1: Settings tests**

  Verify `MesUrl`, `BarcodeRuleEnable`, `IsUploadCode`, `BarcodeRule`, `TrayCodeRule`, `MesEnable`, `MesUserName`, `MesPassword`, `DevNo`, `OrderNo`, `GroupCode`, `Position`, `UploadType`, `MacEnable`, `GetMainCode`, `isFirstStation`, `isPanelEnable`, `IsGetSepBarcode`, and `IsNewMes`.
  Lock the legacy `IsGetSepBarcode` / `IsNewMes` behavior exactly: source code saves `IsGetSepBarcode` from `chkGetSN`, saves `IsNewMes` from `ckNewMES`, but loads `chkGetSN` from `IsNewMes`. If this appears inconsistent during implementation, stop and ask before deciding whether to preserve or correct it.

- [ ] **Step 2: Delegate registration tests**

  Assert `MesType="12"` registers only GP12 delegates listed in section 1.

- [ ] **Step 3: Barcode and login tests**

  Cover disabled rule success, wildcard rule behavior, disabled MES login success, and failed post response mapping through fake transport.

- [ ] **Step 4: Upload tests**

  Cover no index, no result, upload type PASS gate, FAIL confirmation gate, normal upload conversion, `AutoType="4"` filtering behavior, `AutoType="9"` not inheriting unverified `AutoType="4"` filtering, panel/non-panel branches if both are implemented.

- [ ] **Step 5: View structure tests**

  Assert WPF GP12 view contains original settings, edit/save/cancel, status/message area, and does not expose other MES providers.

- [ ] **Step 6: Run tests to verify red**

  ```powershell
  dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter "Gp12|MesForbiddenScope"
  ```

  Expected before implementation: FAIL because GP12 WPF module/service does not exist.

### Task 3: Implement GP12 application layer

**Files:**
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Application\Mes\MesOperationResult.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Application\Mes\Gp12MesSettings.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Application\Mes\Gp12MesMessage.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Application\Mes\IGp12MesService.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Application\Mes\IGp12MesSettingsRepository.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Application\Mes\IGp12MesDelegateRegistrar.cs`

- [ ] **Step 1: Define interfaces and DTOs**

  Keep provider-specific GP12 types isolated under `Mes`. Do not mix with ProductTest service except through `SysCache` delegate adapter.

- [ ] **Step 2: Add XML comments**

  Required for public APIs.

- [ ] **Step 3: Run GP12 target tests**

  Expected: application-level compile passes; infrastructure tests remain red until adapters are implemented.

### Task 4: Implement GP12 legacy adapters

**Files:**
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\Mes\LegacyGp12MesSettingsRepository.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\Mes\LegacyGp12MesDelegateRegistrar.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\Mes\LegacyGp12MesGateway.cs`
- Modify: `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\ATS5.Infrastructure.LegacyAdapters.csproj`

- [ ] **Step 1: Add ATSMes references if required**

  Reference `ATSMes.dll` and its dependent legacy DLLs from `D:\CODE\ATE\01_code\ATS\bin\Debug` only if needed. Do not copy source.

- [ ] **Step 2: Implement config read/write through `ConfigHelper`**

  Preserve save timing: values are written on Save command, not on each edit.

- [ ] **Step 3: Implement delegate registration**

  Register only `MesType="12"` GP12 delegates. Dispose/unregister strategy must prevent duplicate registration on repeated navigation.

- [ ] **Step 4: Implement GP12 service**

  Prefer wrapping the legacy provider behavior. If direct WinForms control wrapping is not viable, extract only behavior proven by tests against the legacy code path.

- [ ] **Step 5: Run GP12 target tests**

  ```powershell
  dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter "Gp12"
  ```

### Task 5: Implement GP12 WPF module

**Files:**
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Modules.Mes\ATS5.Modules.Mes.csproj`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Modules.Mes\MesModule.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Modules.Mes\ViewModels\Gp12MesViewModel.cs`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Modules.Mes\Views\Gp12MesView.xaml`
- Create: `D:\CODE\ATE\ATS5.0\ATS5.Modules.Mes\Views\Gp12MesView.xaml.cs`
- Modify: `D:\CODE\ATE\ATS5.0\ATS5.Wpf\App.xaml.cs`
- Modify: `D:\CODE\ATE\ATS5.0\ATS5.Wpf\ATS5.Wpf.csproj`
- Modify: `D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln`
- Modify: `D:\CODE\ATE\ATS5.0\ATS5.Tests\ATS5.Tests.csproj`

- [ ] **Step 1: Create Prism module**

  Register GP12 view and services. Do not register other MES provider views.

- [ ] **Step 2: Build GP12 UI parity**

  Match original GP12 layout structure: settings panel, edit/save/cancel, MES enable, barcode rule, tray rule, username/password/device/order/group/position/url, toggles, status selection, message area.

- [ ] **Step 3: Wire commands**

  Save, edit, cancel, manual checks if present, and status command must call Application service only.

- [ ] **Step 4: Run structure and GP12 tests**

  ```powershell
  dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter "Gp12|Mes"
  ```

### Task 6: ProductTest and GP12 integration verification

**Files:**
- Modify: `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\36-wave4-wp10-gp12-mes-report.md`
- Modify: `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\00-master-checklist.md`
- Modify: `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\11-open-questions.md`

- [ ] **Step 1: Verify delegate registration before ProductTest**

  Use fake/simulator GP12 provider to confirm `SendChannelBarcode`, `BarcodeCheck`, `LoginMes`, `StationCheck`, and `UploadData` are registered and invoked in original order.

- [ ] **Step 2: Verify local save before MES upload**

  ProductTest fake execution must show data save markers before GP12 upload markers.

- [ ] **Step 3: Verify `UploadMesStatus`**

  PASS upload -> `1`; MES disabled or upload skipped/fail as WinForms rule -> `0`.

- [ ] **Step 4: Verify logging**

  Use existing `LogHelper` categories. GP12 messages must preserve `LogHelper.Mes` semantics. ProductTest execution errors must preserve `LogHelper.Error` or `LogHelper.Info` usage according to original branch.

- [ ] **Step 5: Verify no AutoType 9 implementation**

  AutoType 9 remains a Wave 5 boundary. Only verify GP12 does not incorrectly reuse AutoType 4 data filtering for AutoType 9 unless original GP12 code proves it.

- [ ] **Step 6: Write WP-10 report and review**

  Same report structure as WP-09, with GP12-specific data/log/UI/MES evidence.

- [ ] **Step 7: Run Spec Compliance Review**

  Reviewer must focus on GP12 WinForms behavior, delegate registration, config keys, `UploadData` branches, `UploadMesStatus`, log categories, UI parity, ProductTest integration order, and Wave 4 scope.

- [ ] **Step 8: Run Code Quality Review**

  Reviewer must focus on C# naming, nullable annotations, XML comments, MVVM/Prism boundaries, adapter isolation, deterministic fake/simulator tests, async/cancellation correctness, no `Result`/`Wait`, no new broad exception handling, and no sensitive logging.

- [ ] **Step 9: Fix review findings and re-review**

  Do not report WP-10 or Wave 4 complete with Critical or Important findings open.

## 8. Required Verification Commands

Run these after each implemented WP, not at plan-writing time unless only validating docs.

```powershell
dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln
dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter "ProductTest|ProductTestShell"
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter "Gp12|Mes"
dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore
rg -n -e 'AutoType\s*9' -e 'ATSAutoTest' -e 'ATSDevice' -e 'FrmDevDebug' -- ATS5.Application ATS5.Infrastructure.LegacyAdapters ATS5.Modules.ProductTest ATS5.Modules.Mes ATS5.Tests
```

Expected implementation-stage results:

- restore: exit 0.
- build: exit 0, 0 errors. Any warnings must be explained.
- target tests: all pass after red/green evidence.
- full tests: all pass.
- forbidden scans: only approved test assertions or documentation hits; no production implementation scope violation.

## 9. Review Prompts

### Spec Compliance Review prompt

Reviewer must inspect diff and reports against:

- `D:\CODE\ATE\01_code\ATS\ProductTest\UcProductTest.cs`
- `D:\CODE\ATE\01_code\ATSCore\ProductTestCore.cs`
- `D:\CODE\ATE\01_code\ATSMes\UcMesMain.cs`
- `D:\CODE\ATE\01_code\ATSMes\DB_GP12\UI\UcMain_DB_GP12_EVB.cs`
- this plan

Findings must prioritize:

- changed operation order,
- changed save/upload timing,
- changed `UploadMesStatus`,
- changed log category,
- missing dialog/error behavior,
- changed device initialization/close boundary,
- accidental AutoType 9/device debug/other provider implementation,
- missing tests for high-risk branches.

### Code Quality Review prompt

Reviewer must inspect diff against:

- user AGENTS.md C# rules,
- MVVM/Prism boundaries,
- adapter boundaries,
- nullable safety,
- async/cancellation correctness,
- no `Result`/`Wait`,
- no broad top-level `Exception` catch in new code unless preserving a legacy branch behind an adapter with tests,
- no public fields,
- no magic strings without constants for new code,
- public API XML comments,
- test quality and deterministic fake/simulator design.

## 10. Stop Conditions

Stop and ask before continuing if any of the following occurs:

- Need to modify `D:\CODE\ATE\01_code`.
- Need to change `ProductTestCore`, `SysCache`, DBCore, `LogHelper`, `ConfigHelper`, or legacy model signatures.
- Need to change test data path, gzip JSON format, SQLite schema, `.fw/.dev/.adbc/.xlsx`, or `ATS.exe.config` keys.
- Cannot prove ProductTest local save happens before GP12 upload.
- Cannot prove `UploadMesStatus` matches WinForms.
- Need to instantiate real devices or touch `ATSDevice`.
- Cannot preserve the WinForms PLC boundary, `PLC_M2_Enable` gate, or dynamic engine `Plc` propagation.
- Cannot preserve the `ReleaseSingleTest` / `CheckSwtichConfig` / `thRelease.Join` pre-start order.
- Need to correct the GP12 `IsGetSepBarcode` / `IsNewMes` load-save inconsistency instead of preserving or explicitly asking.
- Need to implement AutoType 9 callbacks.
- Need to migrate more than GP12 MES.
- Tests require real MES endpoint, real PLC, or live device and no simulator/fake path exists.
- Spec Compliance Review or Code Quality Review returns Critical or Important findings.

## 11. Completion Criteria For Wave 4

Wave 4 can be reported as code-side complete only when:

- WP-09 report exists and is reviewed.
- WP-10 report exists and is reviewed.
- restore/build/target/full tests have fresh passing evidence.
- Data compatibility report proves local save path and `IndexInfo`/gzip JSON are unchanged or explicitly notes unverified real data gaps.
- Log report proves original `LogHelper` categories are used.
- MES report proves GP12 delegate registration, disabled-MES branch, upload gate, upload status update, and fake/simulator upload sequence.
- UI report proves ProductTest and GP12 WPF surfaces match original structure enough for user manual acceptance.
- Forbidden-scope scan proves no AutoType 9, device debug Host, other MES provider, full AutoTest, or full ATSDevice migration entered Wave 4.
- User is asked whether to enter Wave 5; Wave 5 must not start automatically.
