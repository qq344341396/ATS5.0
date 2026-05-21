using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ATS5.Application.ProductTest;
using Xunit;

namespace ATS5.Tests.ProductTest
{
    public sealed class ProductTestUploadOrderingTests
    {
        [Fact]
        public async Task StartAsync_UploadsMesOnlyAfterLocalSaveAndAutoExport()
        {
            var calls = new List<string>();
            var executionAdapter = new RecordingProductTestExecutionAdapter(calls);
            var service = new ProductTestSessionService(
                executionAdapter,
                new RecordingProductTestMesUploadGateway(calls),
                new AlwaysReadyAutomationGateway(calls));

            await service.StartAsync(CreateRequest(), CancellationToken.None);

            AssertBefore(calls, "InsertIndexInfo", "UploadData:20260521010101:1");
            AssertBefore(calls, "UpdateIndexInfo", "UploadData:20260521010101:1");
            AssertBefore(calls, "SaveTestData", "UploadData:20260521010101:1");
            AssertBefore(calls, "AutoExport", "UploadData:20260521010101:1");
            AssertBefore(calls, "ProductTestCore.StopTest", "UploadData:20260521010101:1");
        }

        [Fact]
        public async Task StartAsync_DoesNotUploadMes_WhenCompileOrInitFailsBeforeLogGuid()
        {
            var calls = new List<string>();
            var executionAdapter = new RecordingProductTestExecutionAdapter(calls)
            {
                IsCompileAndInit = false,
                LogGuid = 0,
                TestResults = new[] { "FAIL" },
                EmitsStopCallbacks = true
            };
            var service = new ProductTestSessionService(
                executionAdapter,
                new RecordingProductTestMesUploadGateway(calls),
                new AlwaysReadyAutomationGateway(calls));

            await service.StartAsync(CreateRequest(), CancellationToken.None);

            Assert.DoesNotContain("UploadData:0:1", calls);
            Assert.Contains("UpdateMesInfo:0:1:0", calls);
            Assert.Contains("UpdateMesInfo.NoOp", calls);
            AssertBefore(calls, "FreshTestRes.Stop", "FreshTestRes.Complete:FAIL");
            AssertBefore(calls, "AutoTestMESResult:1:False", "AutoTestMESResult:1:True");
        }

        [Fact]
        public async Task StartAsync_DoesNotUploadMes_WhenCompileOrInitFailsAfterLogGuid()
        {
            var calls = new List<string>();
            var executionAdapter = new RecordingProductTestExecutionAdapter(calls)
            {
                IsCompileAndInit = false,
                LogGuid = 20260521010101UL,
                TestResults = new[] { "FAIL" }
            };
            var service = new ProductTestSessionService(
                executionAdapter,
                new RecordingProductTestMesUploadGateway(calls),
                new AlwaysReadyAutomationGateway(calls));

            await service.StartAsync(CreateRequest(), CancellationToken.None);

            Assert.DoesNotContain("UploadData:20260521010101:1", calls);
            Assert.Contains("UpdateMesInfo:20260521010101:1:0", calls);
            Assert.Contains("FreshTestRes.Complete:FAIL", calls);
            Assert.Contains("AutoTestMESResult:1:True", calls);
        }

        [Fact]
        public async Task StartAsync_KeepsAutoTestResultRawPass_WhenMesUploadFails()
        {
            var calls = new List<string>();
            var executionAdapter = new RecordingProductTestExecutionAdapter(calls)
            {
                TestResults = new[] { "PASS" }
            };
            var mesGateway = new RecordingProductTestMesUploadGateway(calls)
            {
                UploadStatus = false
            };
            var service = new ProductTestSessionService(
                executionAdapter,
                mesGateway,
                new AlwaysReadyAutomationGateway(calls));

            await service.StartAsync(CreateRequest(), CancellationToken.None);

            Assert.Contains("UploadData:20260521010101:1", calls);
            Assert.Contains("UpdateMesInfo:20260521010101:1:0", calls);
            Assert.Contains("FreshTestRes.Complete:PASS", calls);
            Assert.Contains("AutoTestResult:1:PASS", calls);
            Assert.Contains("AutoTestMESResult:1:False", calls);
            AssertBefore(calls, "FreshTestRes.Complete:PASS", "AutoTestResult:1:PASS");
            AssertBefore(calls, "AutoTestResult:1:PASS", "AutoTestMESResult:1:False");
        }

        [Fact]
        public async Task StartAsync_SetsUploadMesStatusZero_WhenMesDisabledButUploadSucceeds()
        {
            var calls = new List<string>();
            var executionAdapter = new RecordingProductTestExecutionAdapter(calls)
            {
                TestResults = new[] { "PASS" }
            };
            var mesGateway = new RecordingProductTestMesUploadGateway(calls)
            {
                IsMesEnabled = false,
                UploadStatus = true
            };
            var service = new ProductTestSessionService(
                executionAdapter,
                mesGateway,
                new AlwaysReadyAutomationGateway(calls));

            await service.StartAsync(CreateRequest(), CancellationToken.None);

            Assert.Contains("UploadData:20260521010101:1", calls);
            Assert.Contains("UpdateMesInfo:20260521010101:1:0", calls);
            Assert.Contains("FreshTestRes.Complete:PASS", calls);
            Assert.Contains("AutoTestResult:1:PASS", calls);
            Assert.Contains("AutoTestMESResult:1:True", calls);
        }

        [Fact]
        public async Task StartAsync_SkipsUploadAndUsesFallback_WhenUploadDelegateIsMissing()
        {
            var calls = new List<string>();
            var executionAdapter = new RecordingProductTestExecutionAdapter(calls)
            {
                TestResults = new[] { "PASS" }
            };
            var mesGateway = new RecordingProductTestMesUploadGateway(calls)
            {
                HasUploadDelegate = false
            };
            var service = new ProductTestSessionService(
                executionAdapter,
                mesGateway,
                new AlwaysReadyAutomationGateway(calls));

            await service.StartAsync(CreateRequest(), CancellationToken.None);

            Assert.DoesNotContain("UploadData:20260521010101:1", calls);
            Assert.Contains("UploadData.Missing", calls);
            Assert.Contains("UpdateMesInfo:20260521010101:1:0", calls);
            Assert.Contains("FreshTestRes.Complete:PASS", calls);
            Assert.Contains("AutoTestResult:1:PASS", calls);
            Assert.Contains("AutoTestMESResult:1:True", calls);
        }

        [Fact]
        public async Task StopAsync_SavesStopResultBeforeAbortAndClose()
        {
            var calls = new List<string>();
            var executionAdapter = new RecordingProductTestExecutionAdapter(calls)
            {
                LogGuid = 20260521010101UL
            };
            var service = new ProductTestSessionService(
                executionAdapter,
                new RecordingProductTestMesUploadGateway(calls),
                new AlwaysReadyAutomationGateway(calls));

            await service.StopAsync(channel: 1, CancellationToken.None);

            Assert.Equal(new[]
            {
                "UpdateIndexInfo:STOP",
                "DetermineTestResults",
                "SaveTestData",
                "AbortTestThread",
                "ProductTestCore.StopTest"
            }, calls);
        }

        [Fact]
        public async Task StopAsync_SkipsStopSave_WhenCurrentLogGuidIsZero()
        {
            var calls = new List<string>();
            var executionAdapter = new RecordingProductTestExecutionAdapter(calls)
            {
                LogGuid = 0
            };
            var service = new ProductTestSessionService(
                executionAdapter,
                new RecordingProductTestMesUploadGateway(calls),
                new AlwaysReadyAutomationGateway(calls));

            await service.StopAsync(channel: 1, CancellationToken.None);

            Assert.Equal(new[]
            {
                "AbortTestThread",
                "ProductTestCore.StopTest"
            }, calls);
        }

        private static ProductTestSessionRequest CreateRequest()
        {
            return new ProductTestSessionRequest(
                channel: 1,
                previousFlowName: "旧流程",
                flowName: "流程A",
                barcodes: new[] { "SN001", "SN002" });
        }

        private static void AssertBefore(IReadOnlyList<string> calls, string first, string second)
        {
            var firstIndex = IndexOf(calls, first);
            var secondIndex = IndexOf(calls, second);
            Assert.True(firstIndex >= 0, $"未记录调用: {first}");
            Assert.True(secondIndex >= 0, $"未记录调用: {second}");
            Assert.True(firstIndex < secondIndex, $"{first} 必须早于 {second}");
        }

        private static int IndexOf(IReadOnlyList<string> calls, string value)
        {
            for (var i = 0; i < calls.Count; i++)
            {
                if (calls[i] == value)
                {
                    return i;
                }
            }

            return -1;
        }
    }

    internal sealed class RecordingProductTestExecutionAdapter : IProductTestExecutionAdapter
    {
        private readonly List<string> _calls;

        public RecordingProductTestExecutionAdapter(List<string> calls)
        {
            _calls = calls;
        }

        public bool IsCompileAndInit { get; set; } = true;

        public ulong LogGuid { get; set; } = 20260521010101UL;

        public IReadOnlyList<string> TestResults { get; set; } = new[] { "PASS" };

        public bool EmitsStopCallbacks { get; set; }

        public Task<ProductTestSessionResult> ExecuteAsync(ProductTestSessionRequest request, CancellationToken cancellationToken)
        {
            _calls.Add($"CreateProductTestCore:{request.FlowName}");
            _calls.Add("ExecuteTest");

            if (EmitsStopCallbacks)
            {
                _calls.Add("FreshTestRes.Stop");
                _calls.Add($"AutoTestResult:{request.Channel}:FAIL");
                _calls.Add($"AutoTestMESResult:{request.Channel}:False");
            }

            if (IsCompileAndInit)
            {
                _calls.Add("InsertIndexInfo");
                _calls.Add("UpdateIndexInfo");
                _calls.Add("SaveTestData");
                _calls.Add("AutoExport");
            }

            _calls.Add("ProductTestCore.StopTest");
            return Task.FromResult(new ProductTestSessionResult(LogGuid, request.Channel, TestResults, IsCompileAndInit));
        }

        public Task SaveStopAsync(ushort channel, CancellationToken cancellationToken)
        {
            if (LogGuid != 0)
            {
                _calls.Add("UpdateIndexInfo:STOP");
                _calls.Add("DetermineTestResults");
                _calls.Add("SaveTestData");
            }

            return Task.CompletedTask;
        }

        public Task AbortAsync(ushort channel, CancellationToken cancellationToken)
        {
            _calls.Add("AbortTestThread");
            return Task.CompletedTask;
        }

        public Task StopCoreAsync(ushort channel, CancellationToken cancellationToken)
        {
            _calls.Add("ProductTestCore.StopTest");
            return Task.CompletedTask;
        }
    }

    internal sealed class RecordingProductTestMesUploadGateway : IProductTestMesUploadGateway
    {
        private readonly List<string> _calls;

        public RecordingProductTestMesUploadGateway(List<string> calls)
        {
            _calls = calls;
        }

        public bool IsMesEnabled { get; set; } = true;

        public bool UploadStatus { get; set; } = true;

        public bool HasUploadDelegate { get; set; } = true;

        public Task<bool> UploadAfterLocalSaveAsync(ulong logGuid, ushort channel, bool isCompileAndInit, CancellationToken cancellationToken)
        {
            if (isCompileAndInit)
            {
                if (!HasUploadDelegate)
                {
                    _calls.Add("UploadData.Missing");
                    _calls.Add($"UpdateMesInfo:{logGuid}:{channel}:0");
                    return Task.FromResult(true);
                }

                _calls.Add($"UploadData:{logGuid}:{channel}");
                var uploadMesStatus = IsMesEnabled && UploadStatus ? 1 : 0;
                _calls.Add($"UpdateMesInfo:{logGuid}:{channel}:{uploadMesStatus}");
                return Task.FromResult(UploadStatus);
            }

            _calls.Add($"UpdateMesInfo:{logGuid}:{channel}:0");
            if (logGuid == 0)
            {
                _calls.Add("UpdateMesInfo.NoOp");
            }

            return Task.FromResult(true);
        }
    }

    internal sealed class AlwaysReadyAutomationGateway : IProductTestAutomationGateway
    {
        private readonly List<string> _calls;

        public AlwaysReadyAutomationGateway(List<string> calls)
        {
            _calls = calls;
        }

        public bool CheckRegister()
        {
            _calls.Add("CheckRegister");
            return true;
        }

        public bool CheckPencil(ProductTestSessionRequest request)
        {
            _calls.Add("CheckPencil");
            return true;
        }

        public void StartReleaseSingleTest(string previousFlowName)
        {
            _calls.Add($"ReleaseSingleTest.Start:{previousFlowName}");
        }

        public void LoadFlowTestInfo(ushort channel, string flowName)
        {
            _calls.Add($"LoadFlowTestInfo:{channel}:{flowName}");
        }

        public void CheckSwtichConfig(string flowName)
        {
            _calls.Add($"CheckSwtichConfig:{flowName}");
        }

        public bool CheckBarcode(ushort channel, IReadOnlyList<string> barcodes)
        {
            _calls.Add($"CheckBarcode:{channel}:{string.Join("|", barcodes)}");
            return true;
        }

        public Task<bool> PreTestMesCheckAsync(string flowName, IReadOnlyList<string> barcodes, CancellationToken cancellationToken)
        {
            _calls.Add($"PreTestMesCheck:{flowName}:{string.Join("|", barcodes)}");
            return Task.FromResult(true);
        }

        public void JoinReleaseSingleTest()
        {
            _calls.Add("ReleaseSingleTest.Join");
        }

        public Task<bool> StartAutomationAsync(ushort channel, IReadOnlyList<string> barcodes, CancellationToken cancellationToken)
        {
            _calls.Add($"SysCache.StartTest:{channel}:{string.Join("|", barcodes)}");
            return Task.FromResult(true);
        }

        public bool FlowExists(string flowName)
        {
            _calls.Add($"FlowExists:{flowName}");
            return true;
        }

        public bool IsTestRunning(ushort channel)
        {
            _calls.Add($"IsTestRunning:{channel}");
            return false;
        }

        public bool IsDeviceDebugOpen()
        {
            _calls.Add("IsDeviceDebugOpen");
            return false;
        }

        public void RestoreUi()
        {
            _calls.Add("RestoreUi");
        }

        public void FreshTestResComplete(IReadOnlyList<string> testResults, bool mesResult)
        {
            _calls.Add($"FreshTestRes.Complete:{string.Join(";", testResults)}");
        }

        public void SendAutoTestResult(ushort channel, string testResult)
        {
            _calls.Add($"AutoTestResult:{channel}:{testResult}");
        }

        public void SendAutoTestMesResult(ushort channel, bool mesResult)
        {
            _calls.Add($"AutoTestMESResult:{channel}:{mesResult}");
        }
    }
}
