# Wave 4 WP-09 ProductTest Execution Report

更新时间：2026-05-21

## 1. Status

- 当前任务：WP-09 Task 1 Read-only behavior lock。
- 执行范围：只读盘点 ProductTest 执行链，锁定后续 WPF adapter/service 必须复刻的 WinForms 行为。
- 修改范围：仅创建本报告。
- 未执行内容：未写 C#、未写 XAML、未写测试、未实现 ProductTest execution adapter、未进入 WP-10 GP12、未进入 AutoType 9、未进入设备调试 Host。
- 结论：ProductTest 主线、预检、保存、上传、停止、导出、AutoTest 回调均能从代码追踪；本任务未发现必须暂停询问的行为空洞。

## 2. Viewed Files

| 文件 | 用途 |
| --- | --- |
| `D:\CODE\ATE\01_code\ATS\ProductTest\UcProductTest.cs` | WinForms ProductTest UI、开始/暂停/停止、扫码、预检、MES 前置、上传后 UI/AutoTest 回调、PLC 边界 |
| `D:\CODE\ATE\01_code\ATSCore\ProductTestCore.cs` | 动态脚本编译执行、设备初始化/关闭、项目循环、判定、索引保存、明细保存、自动导出 |
| `D:\CODE\ATE\01_code\ATSCore\SysCache.cs` | 路径、MES 委托、AutoTest 字典、项目同步状态、设备/MES 集成总线 |
| `D:\CODE\ATE\01_code\ATSCore\DBCore\IndexInfoCore.cs` | SQLite 索引表结构、插入、更新、MES 状态更新 |
| `D:\CODE\ATE\01_code\ATSCore\DBCore\TestProjectDataCore.cs` | gzip JSON 明细读取路径和投影 |

## 3. Exact ProductTest Sequence

### 3.1 Load And Delegate Registration

`UcProductTest_Load` 初始化消息、DBC、线束、最近一次测试数据、UI、PLC 和语言后调用 `RegisterMethod()` 并订阅消息：`SysCache.PublishMsg`、`SysCache.PublishSysInfo`、`MessageHelper.Subscribe<ProductTestCore>(..., ReleaseSingleTest)`，见 `UcProductTest.cs:76-127`。

`RegisterMethod()` 必须按通道注册以下委托，且只在字典不存在当前通道时加入：`SysCache.CheckRegister`、`AutoTestGetBarcodeDic[ch]`、`AutoTestCheckBarcodeDic[ch]`、`AutoTestPreTestMesCheckDic[ch]`、`AutoTestStartTestDic[ch]`、`AutoTestStopTestDic[ch]`、`AutoTestStateDic[ch]`，见 `UcProductTest.cs:204-240`。

后续 WPF 不得改变这些 delegate key、通道号语义或注册时机。AutoType 9 不在 WP-09 实现范围内，只保留 ProductTest callback 边界。

### 3.2 Manual Start Button Flow

`btnStartAll_ItemClick` 的顺序是 WP-09 的硬验收线，见 `UcProductTest.cs:714-799`：

1. `CheckRegister()`，失败直接返回。
2. `CheckPencil()`，失败直接返回。
3. 启动 `ReleaseSingleTest` 后台线程，参数是旧 `flowName`，线程名为 `ReleaseSingleTest`。
4. 打开 `FrmBarcode(flowName, barcodeNum, ch)`。
5. `DialogResult.OK && frm.status == 1` 时禁用开始按钮，更新 `flowName`、`barcodeNum`、`lstBarcode`。
6. 读取 `SysCache.PathTemp + test_info{ch}_{flowName}.tmp` 并刷新 Total/Pass/Fail/Pencil 统计。
7. 调用 `CheckSwtichConfig()`。
8. 调用 `CheckBarcode(lstBarcode)`；失败时 `SetUI()` 并返回。
9. 后台调用 `PreTestMesCheck(lstBarcode)`；失败时 `SetUI()` 并返回。
10. `thRelease.Join()` 等待单步测试资源释放完成。
11. 如 `SysCache.StartTest != null`，后台调用 `SysCache.StartTest(ch, lstBarcode)`；异常或 false 时提示并 `SetUI()` 返回。
12. 调用 `StartTest(flowName, lstBarcode)`。

取消扫码弹窗时仅更新 flow/barcode/test_info 预览，不启动测试，见 `UcProductTest.cs:801-810` 及后续同段逻辑。

### 3.3 Barcode And Pre-MES Checks

`CheckBarcode` 顺序固定，见 `UcProductTest.cs:1509-1549`：

1. 如果 `SysCache.SendChannelBarcode != null`，先调用 `SendChannelBarcode(ch, lstBarcode)`，消息文本为传入通道号和条码成功/失败。
2. 如果 `SysCache.BarcodeCheck != null`，再调用 `BarcodeCheck(lstBarcode)`，消息文本为条码规则校验成功/失败。
3. 两者任一失败直接返回 false。
4. 成功后调用 `InitBarcodeUI(lstBarcode)`。

`GetBarcode` 是 AutoTest 边界，调用 `SysCache.GetBarcode(string.Join(";", lstBarcode))` 后刷新条码 UI，见 `UcProductTest.cs:1563-1578`。注意源码把成功返回拆到局部 `lstBarcode` 变量，不能假定会回写调用者集合。

`PreTestMesCheck` 顺序固定，见 `UcProductTest.cs:1447-1495`：

1. `SysCache.LoginMes()`。
2. `SysCache.GetOrderNo(string.Join(";", lstBarcode))`。
3. `SysCache.OrderNoCheck(string.Join(";", lstBarcode))`。
4. `SysCache.StationCheck(string.Join(";", lstBarcode))`。
5. `SysCache.GetMESDownloadPara(barcodes, SysCache.PathFlows + flowName + ".fw")`；如果 `resModel.MesData != null`，用 `JsonHelper.WriteJsonFile((ATSModel.Flow)resModel.MesData, path)` 写回当前 `.fw`。

任一失败返回 false；异常提示为 `测试前MES数据校验异常：{ex.Message}`，见 `UcProductTest.cs:1497-1500`。

### 3.4 Matrix, Pencil, And UI State

`CheckPencil` 受 `PencilEnable` 控制，读取 `SysCache.PathTemp/test_info{ch}_{flowName}.tmp`，超过 `PencilAlarmNum` 时阻断开始，超过 `PencilEarlyAlarmNum` 时只预警不阻断，见 `UcProductTest.cs:1277-1299`。

`CheckSwtichConfig` 读取当前 `.fw` 和对应 `.dev`，如果 flow、dev config 或 mutex list 解析为 null 会直接返回；只有在启用矩阵设备且互斥文件不存在、为空或没有有效互斥项时才 `ShowMsg(false, ...)` 警告，不阻断测试，见 `UcProductTest.cs:1304-1329`。

`SetUI` 中 `CmdUI.Default` 启用开始、禁用暂停/停止、按流程名决定导出按钮；`CmdUI.Testing` 禁用开始、启用暂停/停止、禁用导出，且操作员禁用 reset 和 pencil reset，见 `UcProductTest.cs:396-433`。

### 3.5 StartTest Guard And Setup

`StartTest(flow, lstBarcode)` 的前置顺序固定，见 `UcProductTest.cs:1587-1665`：

1. 重复启动保护：`currentCmd == CmdUI.Testing || (td != null && td.IsAlive)` 时提示 `通道{ch} 已在测试中，忽略重复启动流程：{flow}`，尝试 `LogHelper.Info(msg)`，再 `SetUI()` 返回。
2. 如果 `SysCache.PathFlows + flow + ".fw"` 不存在，提示 `流程文件：{flow}不存在，请检查！` 并返回。
3. 如果 `Application.OpenForms` 中存在 `FrmDevDebug`，提示并 `MsgBoxHelper.ShowWarning("当前设备调试界面还未关闭！")`，返回。
4. 读取 `IsProjectSyncManagerEnable`，设置 `SysCache.DicChannelStatus[ch].Status = 0`。
5. 默认 `testResult = FAIL`，调用 `InitBarcodeUI(lstBarcode)`。
6. 未注册时写 `CacheTime`。
7. 更新 `flowName`，设置 `currentCmd = CmdUI.Testing`，调用 `SetUI()`。
8. 在 UI 线程创建 `new ProductTestCore(flow, lstBarcode, ch)`，绑定 `ShowTestMsg`、`TestStatusChange`、`FreshTestData`、`FreshTestRes`，并设置 `testCore.PLC = plc`。
9. 从 `testCore.LstTestInfo` 和 `testCore.LstDbcRealTime` 绑定 `gcProject`、`gcDBCSignal`。
10. 输出开始测试消息，创建后台线程 `td`。

WPF adapter 如果不能证明 UI dispatcher 创建、PLC 注入、绑定数据源和回调顺序一致，必须暂停。

### 3.6 Background Execution Thread

`td` 线程顺序固定，见 `UcProductTest.cs:1666-1765`：

1. 读取 `mesEnable = ConfigHelper.GetValueApp("MesEnable") == "1"`。
2. 如果 `PLC_M2_Enable == "1" && plcEnable == 1 && plc != null`，先 `plc.SetYOrM("M2", true)`。
3. 如果 `testCore == null`，弹错并返回。
4. 调用 `testCore.ExecuteTest()`。
5. 读取 `guid = testCore.CurrentLogguid`。
6. 仅当 `SysCache.UploadData != null && isCompileAndInit && testCore != null` 时调用 `SysCache.UploadData(testCore.CurrentLogguid, ch)`。
7. 上传分支中先 `ShowMsg(resModel.Status, resModel.Msg)`，再 `mesUpLoadResult = resModel.Status`，再 `IndexInfoCore.UpdateMesInfo(guid, ch, mesEnable && resModel.Status ? 1 : 0)`。
8. 否则调用 `IndexInfoCore.UpdateMesInfo(guid, ch, 0)`，并设置 `mesUpLoadResult = true`。
9. 调用 `FreshTestRes(testCore.DetermineTestResults(), TestStatus.Complete)`；在 `FreshTestRes(Complete)` 内部，非初始化/非单步时会调用 `FreshTestInfo(lstRes)`，进而根据原始测试结果发送 `SysCache.AutoTestResult?.Invoke(ch, PASS/FAIL)`。
10. 调用 `SendMESResult(mesUpLoadResult)`，即 `SysCache.AutoTestMESResult?.Invoke(ch, mesResult)`。因此 WinForms 完成分支的回调顺序是 `FreshTestRes(Complete) -> AutoTestResult -> AutoTestMESResult`。
11. 异常时显示 `执行测试异常：{ex.Message}`，并 `LogHelper.Error("执行测试异常", ex)`。
12. finally 中设置 `SysCache.DicChannelStatus[ch].Status = -1`；如果所有通道 `Status == -1`，调用 `DevicePool.ClearObj()`。
13. 调用 `TestStatusChange(TestStatus.Complete)`。
14. 输出结束测试消息；`AutoType == "14"` 分支显示 MES 上传结果中文成功/失败，其它分支显示 PASS/FAIL/未启用。
15. 清空 `testCore` 并清理内存。
16. 按 `AutoShowMesResultDialog`、`MesEnable`、`SNUpload` 决定弹窗。
17. `AutoShowScanDialog == "1"` 时 `BeginInvoke(btnStartAll_ItemClick)` 自动再次打开扫码。
18. 如果 `plcType == 0 && plcEnable == 1 && plc != null && !plc.OpenTheDoor()`，执行 `plc.SetYOrM("M2", false)`。

关键验收点：本地 `ProductTestCore.ExecuteTest()` 内部保存完成后，ProductTest UI 层才做 `SysCache.UploadData` 和 `IndexInfoCore.UpdateMesInfo`。

### 3.7 ProductTestCore Constructor

`ProductTestCore` 构造函数读取当前 `.fw`、输出项、DBC、UDS、`.dev` 和配置，见 `ProductTestCore.cs:64-155`：

- `.fw` 路径：`SysCache.PathFlows + flowName + ".fw"`。
- enabled 项目：非单步时只取 `flow.LstProject.Where(p => p.IsEnable)`。
- 输出项展开到 `LstTestInfo`，数组输出展开为 `TestName1..n` / `VarName1..n`。
- DBC 路径：`SysCache.PathDBC + dbc.DbcFileName + ".adbc"`。
- UDS 路径：`SysCache.PathUDS + uds.UdsFileName + ".xlsx"`。
- 设备配置路径：`SysCache.PathDevCfg + flow.DevCfgName + ".dev"`，只取启用设备。
- `NgContinue` 为 `"0"` 时不继续，`RetestNum` 从配置读取。

这些文件格式、路径、字段、默认值和读取时机不得改。

### 3.8 ProductTestCore ExecuteTest

`ExecuteTest()` 执行顺序固定，见 `ProductTestCore.cs:161-282`：

1. 为全局设备调用 `DevicePool.GetOrAddLockObj($"{DevClasss}|{InitPars}")`。
2. `MakeUpCode()` 动态拼接 C#。
3. `CompileCode(code)` 编译。
4. `Activator.CreateInstance` 创建 `ATSEngine.TestEngine`。
5. 绑定 `obj.ShowMsg += ShowTestMsg`。
6. 注入 DBC、UDS、`obj.Plc = PLC`。
7. 输出 `脚本编译校验通过`。
8. 调用动态 `obj.Init()`。
9. 初始化失败时输出 `设备初始化失败，终止测试`，`TestStatusChange(Stop)`、`FreshTestRes(null, Stop)`，返回 false。
10. 成功后 `isCompileAndInit = true`，写 `obj.LstBarcode`。
11. `startTime = DateTime.Now`，`CurrentLogguid = yyyyMMddHHmmss`。
12. 输出 `Logguid：{CurrentLogguid}`。
13. `TestStatusChange(Start)`。
14. `FreshTestRes(null, Start)`。
15. `InsertIndexInfo()`。
16. 遍历项目，项目同步开启时先更新 `SysCache.DicChannelStatus[channel]` 并 `WaitForPhaseStartSmart`。
17. `Run(type, obj, i, isOK)` 执行动态 `RunN`。
18. 如果 NG 且 `NgContinue == "0"`，终止项目循环；如果 NG 且继续，按 `RetestNum` 复测。
19. 项目同步开启时设置 `Status = 1`，`WaitForPhaseEndSmart`，输出等待耗时，`Thread.Sleep(50)`。
20. `DetermineTestResults()`。
21. `UpdateIndexInfo(lstRes)`。
22. `SaveTestData()`。
23. `AutoExport()`。
24. catch 中非 `ThreadAbortException` 记录 `LogHelper.Error("运行脚本引擎出现异常", ex)`，推送 `ShowTestMsg(false, ex.Message)`。
25. finally 中 `StopTest()` 关闭动态对象设备。
26. 返回 `isCompileAndInit`。

`Run` 调用动态 `Run{i+1}`，刷新数组/非数组输出，复测时不覆盖已 PASS 结果，项目结束后写项目耗时和结果，见 `ProductTestCore.cs:397-468`。

### 3.9 Data Save And Upload Timing

SQLite 索引表结构包含 `LogGuid`、`Channel`、`Barcode`、`ProcessName`、`ProcessInfo`、`TestStartTime`、`TestEndTime`、`TakeTime`、`UploadMesStatus`、`UploadFtpStatus`、`TestResult`、`Reserve1`、`Reserve2`，见 `IndexInfoCore.cs:72-90`。

`InsertIndexInfo()` 在测试开始后立即插入索引，字段为 `LogGuid`、`Channel`、`;` 拼接条码、`ProcessName`、`.fw` 原文 `ProcessInfo`、秒级 `TestStartTime`，见 `ProductTestCore.cs:961-973` 和 `IndexInfoCore.cs:96-120`。

`UpdateIndexInfo(lstRes)` 在项目循环完成后、本地明细保存前执行，字段包括秒级 `TestEndTime`、一位小数 `TakeTime`、`;` 拼接 `TestResult`、`UploadFtpStatus=-1`、`UploadMesStatus=-1`，见 `ProductTestCore.cs:982-998` 和 `IndexInfoCore.cs:127-139`。

`SaveTestData()` 在 `UpdateIndexInfo` 之后执行，写 `TestProjectDataInfo` 到 `SysCache.PathTestData/{year}/{month}/{day}/{CurrentLogguid}-CH{channel}.json`，使用 `JsonHelper.WriteJsonFileEx`，见 `ProductTestCore.cs:1038-1083`。读取路径完全相同，使用 `JsonHelper.ReadJsonFileEx<TestProjectDataInfo>`，见 `TestProjectDataCore.cs:20-27`。

`IndexInfoCore.UpdateMesInfo(logGuid, channel, uploadMesStatus)` 在 UI 层上传分支后执行；`logGuid == 0` 时直接返回，只更新 `UploadMesStatus`，见 `IndexInfoCore.cs:145-157`。

严格顺序为：

```text
InsertIndexInfo
Run projects
DetermineTestResults
UpdateIndexInfo(UploadMesStatus=-1)
SaveTestData(gzip JSON)
AutoExport
ProductTestCore.StopTest
SysCache.UploadData if delegate exists and compile/init succeeded
IndexInfoCore.UpdateMesInfo(1 or 0)
FreshTestRes Complete
AutoTestResult callback
AutoTestMESResult callback
```

### 3.10 AutoExport

`AutoExport()` 只在 `AutoExportExcel == "1"` 时执行，见 `ProductTestCore.cs:1090-1097`。它依赖 `IndexInfoCore.GetLastIndexInfo(channel)` 和 `TestProjectDataCore.GetTestProjectDataInfo(last)`，因此后续实现不得把导出移到明细保存前。

导出类型：

- type 0：`AutoExportPath\{LogGuid}-【{barcode}】-result.xlsx`，见 `ProductTestCore.cs:1146-1151`。
- type 1：`AutoExportPath/{RemoveChannelSuffix(ProcessName)}/{yyyy-MM-dd}-{Online|Offline}/{TestResult}/...xlsx`，并维护通道汇总 CSV，见 `ProductTestCore.cs:1153-1250`。
- type 2：`AutoExportPath/yyyyMMdd/{CurrentLogguid}【{barcode}】.xlsx`，见 `ProductTestCore.cs:1251-1267`。
- type 3：`AutoExportPath/{RemoveChannelSuffix(ProcessName)}/yyyy/MM/dd/{Online|Offline}/{TestResult}/...xlsx`，并维护同目录通道汇总 CSV，见 `ProductTestCore.cs:1268-1379`。

异常仅记录 `LogHelper.Error($"自动导出异常：{ex}")`，见 `ProductTestCore.cs:1382-1385`。

### 3.11 Result, Status, And AutoTest Callbacks

`TestStatusChange(Start)` 设置开始时间、`currentCmd = Testing`、`SetUI()` 并启动耗时线程；非 Start 时 `ShowMsg("执行测试{testStatus}。")`、`isStop = true`、`currentCmd = Default`，非 Stop 时 `SetUI()`，见 `UcProductTest.cs:1036-1095`。

`FreshTestRes(Start)` 设置条码状态为 Testing、绿灯、更新线束次数并 `SendDeviceStatus(Running)`；`FreshTestRes(Complete)` 根据 `lstRes[i] == PASS && mesUpLoadResult` 设置 UI 标签 PASS/FAIL，非初始化/非单步时写统计和临时 `test_info`，并 `SendDeviceStatus(Standby)`；`FreshTestRes(Stop)` 设置 Stop、`SendTestResult(FAIL)`、`SendMESResult(false)`、`SendDeviceStatus(Stop)`，见 `UcProductTest.cs:1146-1213`。

`FreshTestInfo` 每次完整测试完成都会更新 Total/Pass/Fail/PencilNum 并写回 `SysCache.PathTemp/test_info{ch}_{flowName}.tmp`，见 `UcProductTest.cs:1220-1252`。这里的 `SendTestResult` 只依据 `lstRes.All(PASS)`，不包含 `mesUpLoadResult`；UI 标签是否 PASS 则包含 `mesUpLoadResult`。后续 WPF 不能把 `AutoTestResult` 改成受 MES 上传结果影响。

`SendTestResult` 调用 `SysCache.AutoTestResult?.Invoke(ch, testResult)`；`SendMESResult` 调用 `SysCache.AutoTestMESResult?.Invoke(ch, mesResult)`；`SendDeviceStatus` 调用 `SysCache.DeviceStatusUploadMES?.Invoke(status)`，见 `UcProductTest.cs:1843-1863`。

### 3.12 Pause And Stop

暂停按钮保留 WinForms 线程语义，见 `UcProductTest.cs:862-875`：

- caption 为 `暂停` 时 `td?.Suspend()`，caption 改 `接续`，提示 `测试暂停。`。
- caption 为 `接续` 时 `td?.Resume()`，caption 改 `暂停`，提示 `测试继续。`。

停止顺序固定，见 `UcProductTest.cs:883-922`：

1. 后台调用 `StopTest()`。
2. `isStopDev = true`。
3. 禁用 pause/stop。
4. `mesUpLoadResult = false`。
5. 如果处于暂停状态，先 `td?.Resume()` 并把 caption 改回 `暂停`。
6. 如果 `testCore?.CurrentLogguid != 0`：
   - `testCore.UpdateIndexInfo(new List<string> { "STOP" })`。
   - `testCore.DetermineTestResults()`。
   - `testCore.SaveTestData()`。
7. 停止保存异常记录 `LogHelper.Error("停止测试时保存数据异常", ex)`。
8. `td?.Abort()`。
9. `testCore?.StopTest()`。
10. 清空 `td`、`testCore`，清内存，黄灯。

WP-09 如用 async/await 包装停止命令，仍必须保留 STOP 索引/明细保存先于 abort/close 的顺序。

### 3.13 Single Test Boundary

单步测试不是 WP-09 实现目标，但释放逻辑影响开始前资源。`ReleaseSingleTest` 只有在 `obj != null && testCore != null && isSingleTest` 且 `ShowSingleTest == "1"` 时生效；根据传入对象解析 flow 名，匹配 `FrmSysConfig` 或当前 `testCore.flowName` 时提示 `单步测试设备已复位!`，调用 `testCore.StopTest()` 并清空 `testCore`，见 `UcProductTest.cs:1957-1996`。

`StartSingleTest` 会创建或复用 `ProductTestCore`，设置 `isSingleTest = true`，调用 `ExecuteSingleTest(singleTestName)`，finally 中 `TestStatusChange(Complete)`、清内存、`isSingleTest = false`，见 `UcProductTest.cs:1773-1833`。

WP-09 只需保留主测试开始前的 `ReleaseSingleTest` 顺序和资源释放边界，不实现新的单步 UI。

### 3.14 PLC And Device Boundary

`InitPlc()` 根据 `PLCType` 反射加载 `SysCache.PathDevices/IO_CORX_5216E.Dll` 或 `SysCache.PathDevices/PLC_LK3U.Dll`，按通道读取 `PLCConfig`、`PLCEnable`，启用时后台初始化 PLC，监听急停、上传设备状态、触发停止、控制三色灯和门禁 M2，见 `UcProductTest.cs:245-335`。

`ProductTestCore.MakeUpCode()` 生成动态 `TestEngine`，注入 `dynamic Plc`。源码会执行 `obj.Plc = PLC`；生成代码中从 `Plc` 赋给设备变量的有效分支主要是 `IO_CORX_5216E_NET && plcType == 1`，原 PLC 类赋值分支是注释状态。全局设备仍使用 `DevicePool.GetDeviceInstance` 和 `DevicePool.AddDeviceInstance`，动态 `Close()` 关闭非全局设备并解绑消息，相关入口见 `ProductTestCore.cs:511-676`。

WP-09 不能真实重写设备初始化、互斥、关闭或 PLC 时序；只能 adapter 包装 legacy `ProductTestCore` 和已有 `DevicePool` 行为。

## 4. Risk Matrix

| 风险 | 原代码依据 | 后续实现约束 |
| --- | --- | --- |
| 重复启动 | `UcProductTest.cs:1592-1599` | 必须阻断并恢复 UI；不能并发创建多个 `ProductTestCore` |
| `ReleaseSingleTest` 顺序 | `UcProductTest.cs:724-728`, `UcProductTest.cs:776`, `UcProductTest.cs:1957-1996` | 必须在扫码前启动，pre-MES 成功后 join，不能移到 `StartTest` 后 |
| `CheckSwtichConfig` 只警告不阻断 | `UcProductTest.cs:1304-1329` | 不能把矩阵互斥缺失改成阻断 |
| `thRelease.Join()` | `UcProductTest.cs:776` | 不能省略等待，否则单步设备释放和新测试可能交叠 |
| `FrmDevDebug` 打开保护 | `UcProductTest.cs:1614-1621` | WPF 需要等价设备调试占用 guard；若无法判定必须暂停 |
| 缺失 `.fw` | `UcProductTest.cs:1607-1612` | 必须提示并 `SetUI()` 返回 |
| 编译失败 | `ProductTestCore.cs:171-176`, `ProductTestCore.cs:266-275` | 由 legacy core 记录并返回；不能吞异常 |
| 编译/初始化失败 | `ProductTestCore.cs:190-198`, `ProductTestCore.cs:203-204`, `UcProductTest.cs:1682-1699`, `IndexInfoCore.cs:145-148` | 若失败发生在 `CurrentLogguid` 赋值前，`guid=0`，`UpdateMesInfo(0,...)` no-op；Stop 回调先发 FAIL/false，UI fallback 后可能再发 Complete/true，必须用测试锁住 |
| STOP 保存 | `UcProductTest.cs:903-915` | STOP 索引和明细保存必须先于 abort/close |
| 缺失上传委托 | `UcProductTest.cs:1685-1697` | 不调用上传，`UpdateMesInfo(...,0)`，UI MES result 视为 true |
| MES disabled | `UcProductTest.cs:1669`, `UcProductTest.cs:1691` | 即使上传成功，`MesEnable != 1` 时 `UploadMesStatus=0` |
| 上传失败 | `UcProductTest.cs:1688-1699` | `mesUpLoadResult=false`，`UploadMesStatus=0`，Complete UI 判 FAIL |
| NG continue/retest | `ProductTestCore.cs:225-244` | 由 legacy core 保留，不在 WPF service 中重写 |
| 项目同步 | `ProductTestCore.cs:215-254`, `ProductTestCore.cs:1437-1518` | 必须保留 `DicChannelStatus`、Start/End wait 和 50ms sleep |
| DBC/UDS | `ProductTestCore.cs:125-149` | `.adbc/.xlsx` 路径和解析保持 legacy |
| PLC M2 gate | `UcProductTest.cs:1672-1675`, `UcProductTest.cs:1758-1760` | 后续 production adapter 必须保留 PLC 注入和 M2 时序 |
| `DevicePool.ClearObj` | `UcProductTest.cs:1709-1713` | 只有所有通道 `Status == -1` 时调用 |
| AutoShow dialogs | `UcProductTest.cs:1731-1756` | 自动 MES 结果弹窗和自动扫码重入不能丢 |
| 自动导出路径 | `ProductTestCore.cs:1090-1385` | 不能改变导出类型、文件名、目录和 CSV 表头行为 |

## 5. Task 2 Test Implications

WP-09 Task 2 的失败测试应至少锁定以下顺序和分支：

- Start order：`CheckRegister -> CheckPencil -> ReleaseSingleTest start(old flow) -> BarcodeDialog -> load test_info -> CheckSwtichConfig -> CheckBarcode -> PreTestMesCheck -> ReleaseSingleTest Join -> SysCache.StartTest -> CreateProductTestCore -> ExecuteTest -> UploadData -> UpdateMesInfo -> FreshTestRes Complete -> AutoTestResult -> AutoTestMESResult`。
- Local save before upload：fake execution adapter 必须记录 `InsertIndexInfo -> UpdateIndexInfo -> SaveTestData -> AutoExport` 全部先于 fake MES upload。
- AutoTest result semantics：`AutoTestResult` 根据原始测试结果 `lstRes` 判定，不受 `mesUpLoadResult` 影响；UI 标签 PASS/FAIL 才受 MES 上传结果影响。
- Compile/init false before `CurrentLogguid`：不得调用 `UploadData`；`guid` 可能为 `0`，`UpdateMesInfo(0, ch, 0)` 会 no-op；`ProductTestCore` 先通过 `FreshTestRes(Stop)` 发送 `AutoTestResult(FAIL)` 和 `AutoTestMESResult(false)`，UI 线程 fallback 后还会设置 `mesUpLoadResult=true` 并执行 Complete/true 路径。这个 WinForms 现状虽然别扭，但 WP-09 不能自行纠正。
- Compile/init false after `CurrentLogguid`：不得调用 `UploadData`；必须走 `UpdateMesInfo(guid, ch, 0)` 和 WinForms `mesUpLoadResult=true` UI completion fallback。
- Early return branches：`CheckBarcode` false、`PreTestMesCheck` false、`SysCache.StartTest` false/exception、缺失 `.fw`、打开 `FrmDevDebug` 时，必须断言不创建或不继续执行 `ProductTestCore`、不上传 MES，并恢复 UI/state 到 WinForms 等价结果。
- Stop path：`UpdateIndexInfo(STOP) -> DetermineTestResults -> SaveTestData -> Abort -> StopTest`。
- Forbidden scope：WP-09 implementation 不得出现 GP12 provider、AutoType 9、设备调试 Host、`ATSAutoTest`、`ATSDevice` provider migration。

## 6. Open Questions

本只读行为锁定任务未发现必须暂停的问题。Spec Review 已指出 compile/init failure 的双回调语义较怪异，但它能从代码确认，后续实现必须先测试锁定，不能擅自优化。

后续实现前仍有两个执行门禁：

- 若 production adapter 无法保留 UI dispatcher 创建 `ProductTestCore`、`testCore.PLC = plc`、`PLC_M2_Enable`、动态 engine `Plc` 传递，需要暂停确认。
- 若无法用 fake/simulator 证明本地保存先于 MES 上传、STOP 保存先于线程 abort，需要暂停确认。

## 7. Verification Evidence

本报告的验证范围是文档与只读代码追踪，不包含编译/测试通过声明。

已执行只读检查：

- 读取 `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\34-wave4-implementation-plan.md`。
- 读取 `D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\00-master-checklist.md`。
- 读取并摘录 `UcProductTest.cs`、`ProductTestCore.cs`、`SysCache.cs`、`IndexInfoCore.cs`、`TestProjectDataCore.cs` 的关键方法和行号。

已执行文档验证：

- `rg -n "ReleaseSingleTest|CheckSwtichConfig|thRelease\\.Join|PLC_M2_Enable|SysCache\\.UploadData|UpdateMesInfo|SaveTestData|AutoExport|DevicePool\\.ClearObj|AutoTestMESResult|FrmDevDebug|ProductTestCore|STOP|GetMESDownloadPara|SendChannelBarcode|BarcodeCheck|PreTestMesCheck" D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\35-wave4-wp09-product-test-execution-report.md`：关键风险词均命中。
- `git status --short -- docs/winform-to-wpf-migration/35-wave4-wp09-product-test-execution-report.md`：仅显示本报告为新增文件。
- `Get-FileHash -Algorithm SHA256 D:\CODE\ATE\ATS5.0\docs\winform-to-wpf-migration\35-wave4-wp09-product-test-execution-report.md` 已执行；因本节写入 hash 会改变文件 hash，最终 hash 以阶段汇报中的最后一次命令输出为准。

已执行 subagent 评审：

- Spec Compliance Review：首次 `PASS_WITH_FINDINGS`，无 Critical；Important 要求补强 `AutoTestResult` 顺序、compile/init failure 的 `guid=0`/双回调语义、早退分支测试要求。本报告已按这些 findings 修订。
- Code Quality / Execution Review：`PASS_WITH_FINDINGS`，无 Critical/Important；建议把“待执行”验证改为实际验证结果。本节已修订。

## 8. Task 3 Application Abstractions Evidence

更新时间：2026-05-21

### 8.1 Scope

本节记录 WP-09 Task 3：只实现 ProductTest application abstractions。未进入 legacy adapters、WPF command wiring、GP12、AutoType 9、设备调试 Host、`ATSDevice`、`ATSAutoTest` 或 WP-10。

本轮新增/更新范围：

- `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTest\ProductTestSessionRequest.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTest\ProductTestSessionResult.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTest\ProductTestStatusMessage.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTest\IProductTestSessionService.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTest\IProductTestExecutionAdapter.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTest\IProductTestMesUploadGateway.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTest\IProductTestAutomationGateway.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Application\ProductTest\ProductTestSessionService.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\ProductTest\ProductTestSessionServiceTests.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\ProductTest\ProductTestUploadOrderingTests.cs`

### 8.2 Locked Behaviors Covered

- `StartAsync` 顺序：`CheckRegister -> CheckPencil -> ReleaseSingleTest.Start -> LoadFlowTestInfo -> CheckSwtichConfig -> CheckBarcode -> PreTestMesCheck -> ReleaseSingleTest.Join -> StartAutomation -> IsTestRunning -> FlowExists -> IsDeviceDebugOpen -> ExecuteAsync -> UploadAfterLocalSaveAsync -> FreshTestResComplete -> AutoTestResult -> AutoTestMESResult`。
- 重复启动保护：`IsTestRunning(channel)` 在自动化开始后、`.fw` 存在检查和 execution adapter 前阻断，并调用 `RestoreUi()`。
- 早退分支：条码失败、pre-MES 失败、自动化开始 false/`InvalidOperationException`、流程缺失、设备调试占用均不创建 execution core、不上传 MES，并按测试约束恢复 UI。
- 上传时机：MES upload 只在 execution adapter 返回后发生，由 adapter 保留 `InsertIndexInfo -> UpdateIndexInfo -> SaveTestData -> AutoExport -> StopTest` 的本地保存/导出时机。
- compile/init false：仍调用 upload gateway，由 gateway 负责 `UpdateMesInfo(...,0)` fallback 和 WinForms 的 UI completion 语义。
- AutoTest 回调：`AutoTestResult` 只根据原始 `PASS/FAIL` 测试结果判定，不受 MES 结果影响；`AutoTestMESResult` 使用 upload gateway 返回值。
- Stop 顺序：`SaveStopAsync -> AbortAsync -> StopCoreAsync`，即 STOP 保存先于 abort/close。

### 8.3 Verification Commands

- `dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln`：通过；所有项目均是最新。
- `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore`：通过；0 warning，0 error。
- `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore`：通过；160/160。
- `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter "ProductTestSessionService|ProductTestUploadOrdering|ProductTestForbiddenScope"`：通过；17/17。
- `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter "ProductTest"`：通过；28/28。
- `rg -n -e 'UcMain_DB_GP12_EVB' -e 'DB_GP12' -e 'MesType\s*=\s*"12"' -e 'AutoType\s*9' -e 'ATSAutoTest' -e 'ATSDevice' -e 'FrmDevDebug' -e 'DeviceDebugHost' -e 'ATS5\.Modules\.DeviceDebug' -- ATS5.Application\ProductTest`：无命中。
- `rg -n -e 'SysCache' -e 'ProductTestCore' -e 'DBCore' -e 'LogHelper' -e 'HandyControl' -e 'System\.Windows' -- ATS5.Application\ProductTest`：无命中。

### 8.4 Review Results

- Spec Compliance Review：首次 `CHANGES_REQUIRED`，指出缺少 WinForms duplicate-start guard seam。已用 RED 测试 `StartAsync_DoesNotCreateCoreOrUpload_WhenDuplicateStartIsDetected` 复现并补充 `IProductTestAutomationGateway.IsTestRunning`；复审 `APPROVED`。
- Code Quality Review：首次 `APPROVED`，Minor 建议 DTO 成员顺序按 AGENTS.md 调整。已修正 `ProductTestSessionRequest`、`ProductTestSessionResult`、`ProductTestStatusMessage` 的属性/构造函数顺序；复审 `APPROVED`。

### 8.5 Residual Risks

- 真实 `currentCmd == Testing || td.IsAlive` 到 `IsTestRunning(channel)` 的映射留给 WP-09 Task 4 legacy adapter，Task 3 不直接引用 legacy 静态状态。
- 真实 UI dispatcher 创建 execution core、PLC 注入、设备初始化/关闭、日志文本、MES delegate 和数据库更新仍是 WP-09 Task 4 风险；Task 3 只提供 application-layer seam。
- 本轮未做 UI 人工验收、数据文件对比、真实设备/MES/AutoTest 联调；这些均不属于 Task 3。

## 9. Task 4A Safe Legacy Boundary Adapter Evidence

更新时间：2026-05-21

### 9.1 Scope

本节记录 WP-09 Task 4A：只实现 ProductTest legacy boundary adapters 的安全边界层和测试隔离。未进入真实 `ProductTestCore.ExecuteTest()` production execution、WPF command wiring、GP12/WP-10、AutoType 9、`ATSDevice`、`ATSAutoTest` 或 DeviceDebug Host。

本轮新增/更新范围：

- `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\ProductTest\LegacyProductTestMesUploadGateway.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\ProductTest\LegacyProductTestAutomationGateway.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Infrastructure.LegacyAdapters\ProductTest\LegacyProductTestExecutionAdapter.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\ProductTest\LegacyProductTestAdapterTests.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\Infrastructure\LegacySysCacheCollection.cs`
- `D:\CODE\ATE\ATS5.0\ATS5.Tests\DataQuery\LegacyManualMesGatewayTests.cs`

### 9.2 Locked Behaviors Covered

- `LegacyProductTestMesUploadGateway.UploadAfterLocalSaveAsync` 保持 WinForms 上传 fallback：`isCompileAndInit && SysCache.UploadData != null` 时先上传，再按 `MesEnable == "1" && uploadStatus ? 1 : 0` 更新 `IndexInfoCore.UpdateMesInfo`；缺失上传委托或编译/初始化失败时只更新 `UploadMesStatus=0` 并返回 UI completion fallback 的 `true`。
- `LegacyProductTestAutomationGateway.CheckBarcode` 保持 `SysCache.SendChannelBarcode` 先于 `SysCache.BarcodeCheck`，`SendChannelBarcode` 失败时不继续条码规则校验。
- `LegacyProductTestAutomationGateway.PreTestMesCheckAsync` 保持 `LoginMes -> GetOrderNo -> OrderNoCheck -> StationCheck -> GetMESDownloadPara` 顺序；`GetMESDownloadPara.MesData` 非空时仍写回 `.fw`。
- `CheckBarcode`、`PreTestMesCheckAsync`、`StartAutomationAsync` 在 legacy delegate 边界发生异常时返回 `false`，匹配 `UcProductTest.cs` 的异常早退语义，让上层服务恢复 UI 并阻断 core 创建。
- `LegacyProductTestExecutionAdapter` 仍是 guarded placeholder：所有执行/停止入口抛出 `InvalidOperationException`，直到后续任务提供 UI-thread session owner、PLC 注入、线程状态和设备调试占用 parity。
- `LegacySysCacheCollection` 串行化会修改 `SysCache` 静态委托的 ProductTest/DataQuery 测试，修复全量测试中的静态委托并发串扰。

### 9.3 Verification Commands

- RED 验证：`dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter "AutomationGateway_CheckBarcode_ReturnsFalse_WhenLegacyDelegateThrows|AutomationGateway_PreTestMesCheck_ReturnsFalse_WhenLegacyDelegateThrows|AutomationGateway_StartAutomation_ReturnsFalse_WhenLegacyDelegateThrowsUnexpectedException"`：修复前失败 3/3，分别暴露 `InvalidCastException` 和 `FormatException` 外抛。
- GREEN 验证：同上命令修复后通过；失败 0，通过 3。
- `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter "LegacyProductTestAdapterTests|LegacyManualMesGatewayTests|ProductTestForbiddenScope"`：通过；失败 0，通过 17。
- `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore --filter "ProductTest"`：通过；失败 0，通过 41。
- `dotnet restore D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln`：通过；所有项目均是最新。
- `dotnet build D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore`：通过；0 warning，0 error。
- `dotnet test D:\CODE\ATE\ATS5.0\ATS5.Wpf.sln --no-restore`：通过；失败 0，通过 173。
- `rg -n -e 'UcMain_DB_GP12_EVB' -e 'DB_GP12' -e 'MesType\s*=\s*"12"' -e 'AutoType\s*9' -e 'ATSAutoTest' -e 'ATSDevice' -e 'FrmDevDebug' -e 'DeviceDebugHost' -e 'ATS5\.Modules\.DeviceDebug' -- ATS5.Application\ProductTest ATS5.Infrastructure.LegacyAdapters\ProductTest ATS5.Modules.ProductTest`：无命中。
- `rg -n -e 'Application\.OpenForms' -e 'DevicePool\.ClearObj' -e '\.Init\(' -e 'ExecuteTest\(' -e 'Thread\.Abort' -e 'Thread\.Resume' -e 'new Thread' -e 'ATSAutoTest' -e 'ATSDevice' -- ATS5.Infrastructure.LegacyAdapters\ProductTest`：无命中。

### 9.4 Review Results

- Spec Compliance Review：首次 `CHANGES_REQUIRED`，Important 指出 `StartAutomationAsync` 只捕获 `InvalidOperationException`，以及 `CheckBarcode`/`PreTestMesCheckAsync` 未保持 WinForms broad exception -> false 早退语义。已按 RED-GREEN 修复并补充三条异常 parity 测试；复审 `PASS`，无 Critical/Important/Minor，明确 Task 4A 可进入文档收口且不进入 Task 5。
- Code Quality Review：首次 `APPROVED`；修复后复审 `APPROVED`，无 Critical/Important。评审确认 broad catch 仅限 legacy delegate/pre-MES 边界且有 parity 测试覆盖；提醒 `CheckPencil`、`StartReleaseSingleTest`、`CheckSwtichConfig`、`JoinReleaseSingleTest`、`IsTestRunning`、`IsDeviceDebugOpen` 仍是 Task 4A 安全占位，后续不能当作完整 WinForms parity。

### 9.5 Residual Risks

- 真实 `ProductTestCore.ExecuteTest()`、`ProductTestCore` UI dispatcher 创建、`testCore.PLC = plc`、`PLC_M2_Enable`/`plc.SetYOrM("M2", true)` 时序、`currentCmd/td.IsAlive`、`Application.OpenForms.OfType<FrmDevDebug>()` 等仍未实现，必须保留后续门禁。
- 本轮未接入 WPF `StartCommand`/`StopCommand`，未改变 UI、数据格式、日志框架、设备初始化、MES 上报时机或自动测试 provider。
- 本轮未做真实设备、真实 MES、真实 AutoTest 联调；当前证据仅覆盖安全 boundary adapter、委托顺序、异常早退、上传 fallback、scope scan、构建与自动化测试。
