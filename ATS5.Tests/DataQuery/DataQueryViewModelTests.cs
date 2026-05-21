using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATS5.Application.DataQuery;
using ATS5.Application.Logging;
using ATS5.Modules.DataQuery.ViewModels;
using ATS5.Wpf.Core;
using Xunit;

namespace ATS5.Tests.DataQuery
{
    public sealed class DataQueryViewModelTests
    {
        [Fact]
        public void Constructor_UsesLegacyDefaultDateRange()
        {
            var viewModel = CreateViewModel(new FakeTestRecordRepository());

            Assert.Equal(DateTime.Today.AddDays(-1), viewModel.StartDate?.Date);
            Assert.Equal(DateTime.Today, viewModel.EndDate?.Date);
        }

        [Fact]
        public async Task QueryAsync_ShowsLegacyWarning_AndDoesNotQuery_WhenDateSpanExceeds180Days()
        {
            var repository = new FakeTestRecordRepository();
            var dialog = new FakeMessageDialogService();
            var viewModel = CreateViewModel(repository, dialog);
            viewModel.Records.Add(CreateRecord(20260210000001UL, "SN-OLD"));
            viewModel.Details.Add(CreateDetail("OldProject"));
            viewModel.StartDate = new DateTime(2026, 1, 1);
            viewModel.EndDate = new DateTime(2026, 7, 1);

            await viewModel.QueryAsync();

            Assert.Equal(0, repository.GetTotalCountCallCount);
            Assert.Equal("日期跨度不能大于180天，请调整查询日期", dialog.WarningMessages.Single());
            Assert.Equal("日期跨度不能大于180天，请调整查询日期", viewModel.Message);
            Assert.Single(viewModel.Records);
            Assert.Single(viewModel.Details);
        }

        [Fact]
        public async Task QueryAsync_ShowsLegacyWarning_AndKeepsState_WhenDateIsEmpty()
        {
            var repository = new FakeTestRecordRepository();
            var dialog = new FakeMessageDialogService();
            var viewModel = CreateViewModel(repository, dialog);
            viewModel.CurrentPage = 3;
            viewModel.StartDate = null;
            viewModel.Records.Add(CreateRecord(20260210000001UL, "SN-OLD"));
            viewModel.Details.Add(CreateDetail("OldProject"));

            await viewModel.QueryAsync();

            Assert.Equal(0, repository.GetTotalCountCallCount);
            Assert.Equal("日期不能为空", dialog.WarningMessages.Single());
            Assert.Equal("日期不能为空", viewModel.Message);
            Assert.Equal(3, viewModel.CurrentPage);
            Assert.Single(viewModel.Records);
            Assert.Single(viewModel.Details);
        }

        [Fact]
        public async Task QueryAsync_ResetsPageAndUsesLegacyPageSize_WhenQueryButtonRuns()
        {
            var repository = new FakeTestRecordRepository
            {
                Records = new[] { CreateRecord(20260210010101UL, "SN-NEW") }
            };
            var viewModel = CreateViewModel(repository);
            viewModel.CurrentPage = 4;

            await viewModel.QueryAsync();

            Assert.Equal(1, viewModel.CurrentPage);
            Assert.Equal(1, repository.LastPage);
            Assert.Equal(200, repository.LastPageSize);
        }

        [Fact]
        public async Task QueryAsync_ClearsOldRecordsDetailsAndSelection_BeforeAddingNewRecords()
        {
            var oldRecord = CreateRecord(20260210000001UL, "SN-OLD");
            var repository = new FakeTestRecordRepository
            {
                Records = new[] { CreateRecord(20260210010101UL, "SN-NEW") }
            };
            var viewModel = CreateViewModel(repository);
            viewModel.SelectedRecord = oldRecord;
            await viewModel.LastLoadDetailsTask;
            viewModel.Records.Add(oldRecord);
            viewModel.Details.Add(CreateDetail("OldProject"));

            await viewModel.QueryAsync();

            Assert.Single(viewModel.Records);
            Assert.Equal("SN-NEW", viewModel.Records.Single().Barcode);
            Assert.Empty(viewModel.Details);
            Assert.Null(viewModel.SelectedRecord);
            Assert.True(viewModel.ManualMesUploadCommand.CanExecute());
            Assert.Equal(1, viewModel.TotalCount);
        }

        [Fact]
        public async Task QueryAsync_ShowsLegacyInfo_WhenNoRecordsFound()
        {
            var repository = new FakeTestRecordRepository { TotalCount = 0 };
            var dialog = new FakeMessageDialogService();
            var viewModel = CreateViewModel(repository, dialog);
            viewModel.Records.Add(CreateRecord(20260210000001UL, "SN-OLD"));
            viewModel.Details.Add(CreateDetail("OldProject"));
            viewModel.StatisticsFlowName = "Flow-A";
            viewModel.StatisticsProjectName = "Project-A";
            viewModel.StatisticsStartTime = new DateTime(2026, 2, 10, 0, 0, 0);
            viewModel.StatisticsEndTime = new DateTime(2026, 2, 10, 23, 59, 59);

            await viewModel.QueryAsync();

            Assert.Empty(viewModel.Records);
            Assert.Empty(viewModel.Details);
            Assert.Equal(string.Empty, viewModel.StatisticsFlowName);
            Assert.Equal(string.Empty, viewModel.StatisticsProjectName);
            Assert.Null(viewModel.StatisticsStartTime);
            Assert.Null(viewModel.StatisticsEndTime);
            Assert.Equal("未查询到相关数据", dialog.InfoMessages.Single());
            Assert.Equal("未查询到相关数据", viewModel.Message);
        }

        [Fact]
        public async Task SelectedRecord_LoadsDetails_AndNullSelectionClearsDetails()
        {
            var record = CreateRecord(20260210010101UL, "SN-001");
            var repository = new FakeTestRecordRepository
            {
                Details = new[] { CreateDetail("VoltageProject") }
            };
            var viewModel = CreateViewModel(repository);

            viewModel.SelectedRecord = record;
            await viewModel.LastLoadDetailsTask;

            Assert.Single(viewModel.Details);
            Assert.Equal("VoltageProject", viewModel.Details.Single().ProjectName);
            Assert.True(viewModel.ManualMesUploadCommand.CanExecute());

            viewModel.SelectedRecord = null;
            await viewModel.LastLoadDetailsTask;

            Assert.Empty(viewModel.Details);
            Assert.True(viewModel.ManualMesUploadCommand.CanExecute());
        }

        [Fact]
        public async Task SelectedRecord_DoesNotRestoreStaleDetails_WhenSelectionIsClearedBeforeLoadReturns()
        {
            using (var staleLoadStarted = new ManualResetEventSlim(false))
            using (var releaseStaleLoad = new ManualResetEventSlim(false))
            {
                var staleRecord = CreateRecord(20260210010101UL, "SN-STALE");
                var repository = new FakeTestRecordRepository
                {
                    GetDetailsHandler = record =>
                    {
                        if (record.Barcode == staleRecord.Barcode)
                        {
                            staleLoadStarted.Set();
                            releaseStaleLoad.Wait(TimeSpan.FromSeconds(5));
                            return new[] { CreateDetail("StaleProject") };
                        }

                        return Array.Empty<TestDetailRecord>();
                    }
                };
                var viewModel = CreateViewModel(repository);

                viewModel.SelectedRecord = staleRecord;
                var staleLoadTask = viewModel.LastLoadDetailsTask;
                Assert.True(staleLoadStarted.Wait(TimeSpan.FromSeconds(5)));

                viewModel.SelectedRecord = null;
                await viewModel.LastLoadDetailsTask;
                releaseStaleLoad.Set();
                await staleLoadTask;

                Assert.Empty(viewModel.Details);
                Assert.Null(viewModel.SelectedRecord);
                Assert.True(viewModel.ManualMesUploadCommand.CanExecute());
            }
        }

        [Fact]
        public async Task QueryAsync_LogsLegacyErrorAndShowsLegacyErrorMessage_WhenRepositoryThrows()
        {
            var repository = new FakeTestRecordRepository
            {
                GetTotalCountException = new InvalidOperationException("boom")
            };
            var dialog = new FakeMessageDialogService();
            var logService = new FakeLogService();
            var viewModel = CreateViewModel(repository, dialog, logService);

            await viewModel.QueryAsync();

            Assert.Equal("数据查询异常", logService.ErrorMessages.Single());
            Assert.Equal("boom", logService.Exceptions.Single().Message);
            Assert.Equal("数据查询异常：boom", dialog.ErrorMessages.Single());
            Assert.Equal("数据查询异常：boom", viewModel.Message);
        }

        [Fact]
        public async Task ManualMesUploadAsync_LogsLegacyErrorAndShowsLegacyErrorMessage_WhenGatewayThrows()
        {
            var repository = new FakeTestRecordRepository();
            var dialog = new FakeMessageDialogService();
            var logService = new FakeLogService();
            var mesGateway = new FakeManualMesGateway
            {
                LoginException = new InvalidOperationException("mes boom")
            };
            var viewModel = CreateViewModel(repository, dialog, logService, mesGateway);
            viewModel.SelectedRecord = CreateRecord(20260210010101UL, "SN-001");
            await viewModel.LastLoadDetailsTask;

            await viewModel.ManualMesUploadAsync();

            Assert.Equal("MES上传异常", logService.ErrorMessages.Single());
            Assert.Equal("mes boom", logService.Exceptions.Single().Message);
            Assert.Equal("MES上传异常：mes boom", dialog.ErrorMessages.Single());
            Assert.Equal("MES上传异常：mes boom", viewModel.Message);
        }

        [Fact]
        public async Task ManualMesUploadAsync_UploadsSelectedUploadRecordsInOrder_AndUpdatesSuccessfulStatuses()
        {
            var firstRecord = CreateRecord(20260210010101UL, "SN-001");
            var secondRecord = CreateRecord(20260210010102UL, "SN-002");
            var mesGateway = new FakeManualMesGateway();
            var viewModel = CreateViewModel(new FakeTestRecordRepository(), mesGateway: mesGateway);
            viewModel.SelectedRecord = CreateRecord(20260210019999UL, "SN-DETAIL");
            viewModel.SelectedUploadRecords.Add(firstRecord);
            viewModel.SelectedUploadRecords.Add(secondRecord);
            await viewModel.LastLoadDetailsTask;

            await viewModel.ManualMesUploadAsync();

            Assert.Equal(new[]
            {
                "LoginMes",
                "SendChannelBarcode:1:SN-001",
                "StationCheck:SN-001",
                "UploadData:20260210010101:1",
                "SendChannelBarcode:1:SN-002",
                "StationCheck:SN-002",
                "UploadData:20260210010102:1"
            }, mesGateway.Calls);
            Assert.Equal((short)1, firstRecord.UploadMesStatus);
            Assert.Equal((short)1, secondRecord.UploadMesStatus);
            Assert.Equal("SN-DETAIL", viewModel.SelectedRecord?.Barcode);
        }

        [Fact]
        public async Task ManualMesUploadAsync_ShowsSuccessMessagesAsInfo_AndFailureMessagesAsWarning()
        {
            var successRecord = CreateRecord(20260210010101UL, "SN-OK");
            var failedRecord = CreateRecord(20260210010102UL, "SN-NG");
            var dialog = new FakeMessageDialogService();
            var mesGateway = new FakeManualMesGateway
            {
                UploadDataHandler = (logGuid, channel) =>
                    logGuid == failedRecord.LogGuid
                        ? MesCallResult.Fail("UploadNg")
                        : MesCallResult.Success()
            };
            var viewModel = CreateViewModel(new FakeTestRecordRepository(), dialog, mesGateway: mesGateway);
            viewModel.SelectedUploadRecords.Add(successRecord);
            viewModel.SelectedUploadRecords.Add(failedRecord);

            await viewModel.ManualMesUploadAsync();

            Assert.Equal("条码为：SN-OK,MES上传成功!", dialog.InfoMessages.Single());
            Assert.Equal("条码为：SN-NG,MES上传失败:UploadNg", dialog.WarningMessages.Single());
            Assert.Equal(
                "条码为：SN-OK,MES上传成功!" + Environment.NewLine + "条码为：SN-NG,MES上传失败:UploadNg",
                viewModel.Message);
        }

        [Fact]
        public async Task ManualMesUploadAsync_ShowsMesDisabledWarning_AndDoesNotConfirm_WhenMesDisabled()
        {
            var dialog = new FakeMessageDialogService();
            var viewModel = CreateViewModel(
                new FakeTestRecordRepository(),
                dialog,
                mesEnable: "0");
            viewModel.SelectedRecord = CreateRecord(20260210010101UL, "SN-001");
            await viewModel.LastLoadDetailsTask;

            await viewModel.ManualMesUploadAsync();

            Assert.Equal("未启用MES!", dialog.WarningMessages.Single());
            Assert.Equal(0, dialog.ConfirmCallCount);
            Assert.Equal("未启用MES!", viewModel.Message);
        }

        [Fact]
        public async Task ManualMesUploadAsync_ShowsNoSelectionWarning_AndDoesNotConfirm_WhenNoRecordSelected()
        {
            var dialog = new FakeMessageDialogService();
            var viewModel = CreateViewModel(new FakeTestRecordRepository(), dialog);

            await viewModel.ManualMesUploadAsync();

            Assert.Equal("未选中数据!", dialog.WarningMessages.Single());
            Assert.Equal(0, dialog.ConfirmCallCount);
            Assert.Equal("未选中数据!", viewModel.Message);
        }

        private static DataQueryViewModel CreateViewModel(
            FakeTestRecordRepository repository,
            FakeMessageDialogService? dialog = null,
            FakeLogService? logService = null,
            FakeManualMesGateway? mesGateway = null,
            string mesEnable = "1")
        {
            var appConfig = new FakeAppConfigService(mesEnable);
            var dataQueryService = new DataQueryService(repository);
            var manualMesUploadService = new ManualMesUploadService(appConfig, mesGateway ?? new FakeManualMesGateway(), repository);
            return new DataQueryViewModel(
                dataQueryService,
                manualMesUploadService,
                dialog ?? new FakeMessageDialogService(),
                logService ?? new FakeLogService());
        }

        private static TestRecord CreateRecord(ulong logGuid, string barcode)
        {
            return new TestRecord
            {
                LogGuid = logGuid,
                Channel = 1,
                Barcode = barcode,
                TestResult = "PASS"
            };
        }

        private static TestDetailRecord CreateDetail(string projectName)
        {
            return new TestDetailRecord
            {
                ProjectName = projectName,
                TestName = "VoltageCheck",
                TestResult = "PASS",
                ProjectTestResult = "PASS"
            };
        }

        private sealed class FakeTestRecordRepository : ITestRecordRepository
        {
            public int TotalCount { get; set; } = 1;

            public IReadOnlyList<TestRecord> Records { get; set; } = Array.Empty<TestRecord>();

            public IReadOnlyList<TestDetailRecord> Details { get; set; } = Array.Empty<TestDetailRecord>();

            public Func<TestRecord, IReadOnlyList<TestDetailRecord>>? GetDetailsHandler { get; set; }

            public Exception? GetTotalCountException { get; set; }

            public int GetTotalCountCallCount { get; private set; }

            public int LastPage { get; private set; }

            public int LastPageSize { get; private set; }

            public int GetTotalCount(DataQueryCriteria criteria)
            {
                if (GetTotalCountException != null)
                {
                    throw GetTotalCountException;
                }

                GetTotalCountCallCount++;
                return TotalCount;
            }

            public IReadOnlyList<TestRecord> GetRecords(DataQueryCriteria criteria, int page, int pageSize)
            {
                LastPage = page;
                LastPageSize = pageSize;
                return Records;
            }

            public IReadOnlyList<TestDetailRecord> GetDetails(TestRecord record)
            {
                if (GetDetailsHandler != null)
                {
                    return GetDetailsHandler(record);
                }

                return Details;
            }

            public void UpdateIndexInfo(TestRecord record)
            {
            }
        }

        private sealed class FakeAppConfigService : IAppConfigService
        {
            private readonly string _mesEnable;

            public FakeAppConfigService(string mesEnable)
            {
                _mesEnable = mesEnable;
            }

            public string GetValue(string key)
            {
                return key == "MesEnable" ? _mesEnable : string.Empty;
            }

            public bool SetValue(string key, string value)
            {
                return false;
            }
        }

        private sealed class FakeManualMesGateway : IManualMesGateway
        {
            public List<string> Calls { get; } = new List<string>();

            public Exception? LoginException { get; set; }

            public Func<ulong, ushort, MesCallResult>? UploadDataHandler { get; set; }

            public MesCallResult LoginMes()
            {
                Calls.Add("LoginMes");
                if (LoginException != null)
                {
                    throw LoginException;
                }

                return MesCallResult.Success();
            }

            public MesCallResult SendChannelBarcode(ushort channel, IReadOnlyList<string> barcodes)
            {
                Calls.Add($"SendChannelBarcode:{channel}:{string.Join("|", barcodes)}");
                return MesCallResult.Success();
            }

            public MesCallResult StationCheck(string barcode)
            {
                Calls.Add($"StationCheck:{barcode}");
                return MesCallResult.Success();
            }

            public MesCallResult UploadData(ulong logGuid, ushort channel)
            {
                Calls.Add($"UploadData:{logGuid}:{channel}");
                if (UploadDataHandler != null)
                {
                    return UploadDataHandler(logGuid, channel);
                }

                return MesCallResult.Success();
            }
        }

        private sealed class FakeMessageDialogService : IMessageDialogService
        {
            public List<string> InfoMessages { get; } = new List<string>();

            public List<string> WarningMessages { get; } = new List<string>();

            public List<string> ErrorMessages { get; } = new List<string>();

            public int ConfirmCallCount { get; private set; }

            public bool Confirm(string message)
            {
                ConfirmCallCount++;
                return true;
            }

            public void ShowInfo(string message)
            {
                InfoMessages.Add(message);
            }

            public void ShowWarning(string message)
            {
                WarningMessages.Add(message);
            }

            public void ShowError(string message)
            {
                ErrorMessages.Add(message);
            }
        }

        private sealed class FakeLogService : ILogService
        {
            public List<string> ErrorMessages { get; } = new List<string>();

            public List<Exception> Exceptions { get; } = new List<Exception>();

            public void Init()
            {
            }

            public void Info(string message)
            {
            }

            public void Error(string message, Exception exception)
            {
                ErrorMessages.Add(message);
                Exceptions.Add(exception);
            }

            public void Test(string message, string key = "")
            {
            }

            public void Mes(string message)
            {
            }

            public void Operate(string message)
            {
            }

            public void CanTool(string message)
            {
            }
        }
    }
}
