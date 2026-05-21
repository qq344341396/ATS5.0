using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ATS5.Application.ProductTest;
using Xunit;

namespace ATS5.Tests.ProductTest
{
    public sealed class ProductTestSessionServiceTests
    {
        [Fact]
        public async Task StartAsync_PreservesWinFormsStartOrder()
        {
            var calls = new List<string>();
            var service = CreateService(calls);
            var request = CreateRequest();

            await service.StartAsync(request, CancellationToken.None);

            Assert.Equal(new[]
            {
                "CheckRegister",
                "CheckPencil",
                "ReleaseSingleTest.Start:旧流程",
                "LoadFlowTestInfo:1:流程A",
                "CheckSwtichConfig:流程A",
                "CheckBarcode:1:SN001|SN002",
                "PreTestMesCheck:流程A:SN001|SN002",
                "ReleaseSingleTest.Join",
                "SysCache.StartTest:1:SN001|SN002",
                "IsTestRunning:1",
                "FlowExists:流程A",
                "IsDeviceDebugOpen",
                "CreateProductTestCore:流程A",
                "ExecuteTest",
                "InsertIndexInfo",
                "UpdateIndexInfo",
                "SaveTestData",
                "AutoExport",
                "ProductTestCore.StopTest",
                "UploadData:20260521010101:1",
                "UpdateMesInfo:20260521010101:1:1",
                "FreshTestRes.Complete:PASS",
                "AutoTestResult:1:PASS",
                "AutoTestMESResult:1:True"
            }, calls);
        }

        [Fact]
        public async Task StartAsync_DoesNotCreateCoreOrUpload_WhenBarcodeCheckFails()
        {
            var calls = new List<string>();
            var automationGateway = new RecordingProductTestAutomationGateway(calls)
            {
                IsBarcodeValid = false
            };
            var service = CreateService(calls, automationGateway);

            await service.StartAsync(CreateRequest(), CancellationToken.None);

            Assert.Contains("CheckBarcode:1:SN001|SN002", calls);
            Assert.Contains("RestoreUi", calls);
            Assert.DoesNotContain("PreTestMesCheck:流程A:SN001|SN002", calls);
            Assert.DoesNotContain("CreateProductTestCore:流程A", calls);
            Assert.DoesNotContain("UploadData:20260521010101:1", calls);
        }

        [Fact]
        public async Task StartAsync_DoesNotCreateCoreOrUpload_WhenPreTestMesCheckFails()
        {
            var calls = new List<string>();
            var automationGateway = new RecordingProductTestAutomationGateway(calls)
            {
                IsPreTestMesValid = false
            };
            var service = CreateService(calls, automationGateway);

            await service.StartAsync(CreateRequest(), CancellationToken.None);

            Assert.Contains("PreTestMesCheck:流程A:SN001|SN002", calls);
            Assert.Contains("RestoreUi", calls);
            Assert.DoesNotContain("ReleaseSingleTest.Join", calls);
            Assert.DoesNotContain("CreateProductTestCore:流程A", calls);
            Assert.DoesNotContain("UploadData:20260521010101:1", calls);
        }

        [Fact]
        public async Task StartAsync_DoesNotCreateCoreOrUpload_WhenAutomationStartFails()
        {
            var calls = new List<string>();
            var automationGateway = new RecordingProductTestAutomationGateway(calls)
            {
                IsAutomationStartAccepted = false
            };
            var service = CreateService(calls, automationGateway);

            await service.StartAsync(CreateRequest(), CancellationToken.None);

            Assert.Contains("ReleaseSingleTest.Join", calls);
            Assert.Contains("SysCache.StartTest:1:SN001|SN002", calls);
            Assert.Contains("RestoreUi", calls);
            Assert.DoesNotContain("CreateProductTestCore:流程A", calls);
            Assert.DoesNotContain("UploadData:20260521010101:1", calls);
        }

        [Fact]
        public async Task StartAsync_DoesNotCreateCoreOrUpload_WhenAutomationStartThrows()
        {
            var calls = new List<string>();
            var automationGateway = new RecordingProductTestAutomationGateway(calls)
            {
                AutomationStartException = new InvalidOperationException("PLC busy")
            };
            var service = CreateService(calls, automationGateway);

            await service.StartAsync(CreateRequest(), CancellationToken.None);

            Assert.Contains("ReleaseSingleTest.Join", calls);
            Assert.Contains("SysCache.StartTest:1:SN001|SN002", calls);
            Assert.Contains("SysCache.StartTest.Exception:PLC busy", calls);
            Assert.Contains("RestoreUi", calls);
            Assert.DoesNotContain("CreateProductTestCore:流程A", calls);
            Assert.DoesNotContain("UploadData:20260521010101:1", calls);
        }

        [Fact]
        public async Task StartAsync_DoesNotCreateCoreOrUpload_WhenFlowFileIsMissing()
        {
            var calls = new List<string>();
            var automationGateway = new RecordingProductTestAutomationGateway(calls)
            {
                DoesFlowExist = false
            };
            var service = CreateService(calls, automationGateway);

            await service.StartAsync(CreateRequest(), CancellationToken.None);

            Assert.Contains("FlowExists:流程A", calls);
            Assert.Contains("RestoreUi", calls);
            Assert.DoesNotContain("CreateProductTestCore:流程A", calls);
            Assert.DoesNotContain("ExecuteTest", calls);
            Assert.DoesNotContain("UploadData:20260521010101:1", calls);
        }

        [Fact]
        public async Task StartAsync_DoesNotCreateCoreOrUpload_WhenDuplicateStartIsDetected()
        {
            var calls = new List<string>();
            var automationGateway = new RecordingProductTestAutomationGateway(calls)
            {
                IsTestAlreadyRunning = true
            };
            var service = CreateService(calls, automationGateway);

            await service.StartAsync(CreateRequest(), CancellationToken.None);

            Assert.Contains("SysCache.StartTest:1:SN001|SN002", calls);
            Assert.Contains("IsTestRunning:1", calls);
            Assert.Contains("RestoreUi", calls);
            Assert.DoesNotContain("FlowExists:流程A", calls);
            Assert.DoesNotContain("CreateProductTestCore:流程A", calls);
            Assert.DoesNotContain("UploadData:20260521010101:1", calls);
        }

        [Fact]
        public async Task StartAsync_DoesNotCreateCoreOrUpload_WhenDeviceDebugWindowIsOpen()
        {
            var calls = new List<string>();
            var automationGateway = new RecordingProductTestAutomationGateway(calls)
            {
                IsDeviceDebugOpened = true
            };
            var service = CreateService(calls, automationGateway);

            await service.StartAsync(CreateRequest(), CancellationToken.None);

            Assert.Contains("IsDeviceDebugOpen", calls);
            Assert.Contains("RestoreUi", calls);
            Assert.DoesNotContain("CreateProductTestCore:流程A", calls);
            Assert.DoesNotContain("ExecuteTest", calls);
            Assert.DoesNotContain("UploadData:20260521010101:1", calls);
        }

        private static ProductTestSessionRequest CreateRequest()
        {
            return new ProductTestSessionRequest(
                channel: 1,
                previousFlowName: "旧流程",
                flowName: "流程A",
                barcodes: new[] { "SN001", "SN002" });
        }

        private static IProductTestSessionService CreateService(
            List<string> calls,
            RecordingProductTestAutomationGateway? automationGateway = null,
            RecordingProductTestExecutionAdapter? executionAdapter = null,
            RecordingProductTestMesUploadGateway? mesUploadGateway = null)
        {
            return new ProductTestSessionService(
                executionAdapter ?? new RecordingProductTestExecutionAdapter(calls),
                mesUploadGateway ?? new RecordingProductTestMesUploadGateway(calls),
                automationGateway ?? new RecordingProductTestAutomationGateway(calls));
        }

        private sealed class RecordingProductTestAutomationGateway : IProductTestAutomationGateway
        {
            private readonly List<string> _calls;

            public RecordingProductTestAutomationGateway(List<string> calls)
            {
                _calls = calls;
            }

            public bool IsRegisterValid { get; set; } = true;

            public bool IsPencilValid { get; set; } = true;

            public bool IsBarcodeValid { get; set; } = true;

            public bool IsPreTestMesValid { get; set; } = true;

            public bool IsAutomationStartAccepted { get; set; } = true;

            public Exception? AutomationStartException { get; set; }

            public bool DoesFlowExist { get; set; } = true;

            public bool IsTestAlreadyRunning { get; set; }

            public bool IsDeviceDebugOpened { get; set; }

            public bool CheckRegister()
            {
                _calls.Add("CheckRegister");
                return IsRegisterValid;
            }

            public bool CheckPencil(ProductTestSessionRequest request)
            {
                _calls.Add("CheckPencil");
                return IsPencilValid;
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
                return IsBarcodeValid;
            }

            public Task<bool> PreTestMesCheckAsync(string flowName, IReadOnlyList<string> barcodes, CancellationToken cancellationToken)
            {
                _calls.Add($"PreTestMesCheck:{flowName}:{string.Join("|", barcodes)}");
                return Task.FromResult(IsPreTestMesValid);
            }

            public void JoinReleaseSingleTest()
            {
                _calls.Add("ReleaseSingleTest.Join");
            }

            public Task<bool> StartAutomationAsync(ushort channel, IReadOnlyList<string> barcodes, CancellationToken cancellationToken)
            {
                _calls.Add($"SysCache.StartTest:{channel}:{string.Join("|", barcodes)}");
                if (AutomationStartException != null)
                {
                    _calls.Add($"SysCache.StartTest.Exception:{AutomationStartException.Message}");
                    throw AutomationStartException;
                }

                return Task.FromResult(IsAutomationStartAccepted);
            }

            public bool IsTestRunning(ushort channel)
            {
                _calls.Add($"IsTestRunning:{channel}");
                return IsTestAlreadyRunning;
            }

            public bool FlowExists(string flowName)
            {
                _calls.Add($"FlowExists:{flowName}");
                return DoesFlowExist;
            }

            public bool IsDeviceDebugOpen()
            {
                _calls.Add("IsDeviceDebugOpen");
                return IsDeviceDebugOpened;
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
}
