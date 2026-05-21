using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATS5.Application.DataQuery;
using Xunit;

namespace ATS5.Tests.DataQuery
{
    public sealed class ManualMesUploadServiceTests
    {
        [Fact]
        public async Task UploadAsync_StopsWithLegacyMessage_WhenNoRecordSelected()
        {
            var service = new ManualMesUploadService(
                new FakeAppConfigService("1"),
                new FakeManualMesGateway(),
                new FakeTestRecordRepository());

            var result = await service.UploadAsync(Array.Empty<TestRecord>(), true, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("未选中数据!", result.Messages.Single());
        }

        [Fact]
        public async Task UploadAsync_StopsWithLegacyMessage_WhenMesDisabled()
        {
            var service = new ManualMesUploadService(
                new FakeAppConfigService("0"),
                new FakeManualMesGateway(),
                new FakeTestRecordRepository());

            var result = await service.UploadAsync(new[] { CreateRecord() }, true, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("未启用MES!", result.Messages.Single());
        }

        [Fact]
        public async Task UploadAsync_PreservesLegacyCallOrder_AndUpdatesSuccessfulRecords()
        {
            var gateway = new FakeManualMesGateway();
            var repository = new FakeTestRecordRepository();
            var service = new ManualMesUploadService(
                new FakeAppConfigService("1"),
                gateway,
                repository);
            var record = CreateRecord();

            var result = await service.UploadAsync(new[] { record }, true, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(new[]
            {
                "LoginMes",
                "SendChannelBarcode:2:SN1|SN2",
                "StationCheck:SN1;SN2",
                "UploadData:20260520010101:2"
            }, gateway.Calls);
            Assert.Equal((short)1, record.UploadMesStatus);
            Assert.Single(repository.UpdatedRecords);
            Assert.Equal("条码为：SN1;SN2,MES上传成功!", result.Messages.Single());
        }

        [Fact]
        public async Task UploadAsync_DoesNotCallMesDelegates_WhenUserCancelsConfirmation()
        {
            var gateway = new FakeManualMesGateway();
            var repository = new FakeTestRecordRepository();
            var service = new ManualMesUploadService(
                new FakeAppConfigService("1"),
                gateway,
                repository);

            var result = await service.UploadAsync(new[] { CreateRecord() }, false, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Empty(result.Messages);
            Assert.Empty(gateway.Calls);
            Assert.Empty(repository.UpdatedRecords);
        }

        [Fact]
        public async Task UploadAsync_UsesLegacyFailureMessage_WhenStationCheckFails()
        {
            var gateway = new FakeManualMesGateway { StationCheckResult = MesCallResult.Fail("NG") };
            var repository = new FakeTestRecordRepository();
            var service = new ManualMesUploadService(
                new FakeAppConfigService("1"),
                gateway,
                repository);

            var result = await service.UploadAsync(new[] { CreateRecord() }, true, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("MES工序校验失败:NG", result.Messages.Single());
            Assert.Empty(repository.UpdatedRecords);
        }

        [Fact]
        public async Task UploadAsync_UsesLegacyFailureMessage_AndDoesNotUpdateRecord_WhenUploadDataFails()
        {
            var gateway = new FakeManualMesGateway { UploadDataResult = MesCallResult.Fail("NG") };
            var repository = new FakeTestRecordRepository();
            var service = new ManualMesUploadService(
                new FakeAppConfigService("1"),
                gateway,
                repository);
            var record = CreateRecord();

            var result = await service.UploadAsync(new[] { record }, true, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("条码为：SN1;SN2,MES上传失败:NG", result.Messages.Single());
            Assert.Equal((short)-1, record.UploadMesStatus);
            Assert.Empty(repository.UpdatedRecords);
        }

        [Fact]
        public async Task UploadAsync_ProcessesMultipleRecordsInOrder_AndKeepsFailedRecordUnchanged()
        {
            var firstRecord = CreateRecord(20260520010101UL, 1, "SN-OK");
            var failedRecord = CreateRecord(20260520010102UL, 2, "SN-NG");
            var thirdRecord = CreateRecord(20260520010103UL, 3, "SN-OK2");
            var gateway = new FakeManualMesGateway
            {
                UploadDataHandler = (logGuid, channel) =>
                    logGuid == failedRecord.LogGuid
                        ? MesCallResult.Fail("UploadNg")
                        : MesCallResult.Success()
            };
            var repository = new FakeTestRecordRepository();
            var service = new ManualMesUploadService(
                new FakeAppConfigService("1"),
                gateway,
                repository);

            var result = await service.UploadAsync(
                new[] { firstRecord, failedRecord, thirdRecord },
                true,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(new[]
            {
                "LoginMes",
                "SendChannelBarcode:1:SN-OK",
                "StationCheck:SN-OK",
                "UploadData:20260520010101:1",
                "SendChannelBarcode:2:SN-NG",
                "StationCheck:SN-NG",
                "UploadData:20260520010102:2",
                "SendChannelBarcode:3:SN-OK2",
                "StationCheck:SN-OK2",
                "UploadData:20260520010103:3"
            }, gateway.Calls);
            Assert.Equal((short)1, firstRecord.UploadMesStatus);
            Assert.Equal((short)-1, failedRecord.UploadMesStatus);
            Assert.Equal((short)1, thirdRecord.UploadMesStatus);
            Assert.Equal(new[] { firstRecord, thirdRecord }, repository.UpdatedRecords);
            Assert.Equal(new[]
            {
                "条码为：SN-OK,MES上传成功!",
                "条码为：SN-NG,MES上传失败:UploadNg",
                "条码为：SN-OK2,MES上传成功!"
            }, result.Messages);
        }

        [Fact]
        public async Task UploadAsync_DoesNotUpdateRecord_WhenUploadDelegateIsMissing()
        {
            var gateway = new FakeManualMesGateway { UploadDataResult = MesCallResult.Skipped() };
            var repository = new FakeTestRecordRepository();
            var service = new ManualMesUploadService(
                new FakeAppConfigService("1"),
                gateway,
                repository);

            var result = await service.UploadAsync(new[] { CreateRecord() }, true, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Messages);
            Assert.Empty(repository.UpdatedRecords);
        }

        [Fact]
        public void GetConfirmMessage_ReturnsLegacyConfirmText()
        {
            var service = new ManualMesUploadService(
                new FakeAppConfigService("1"),
                new FakeManualMesGateway(),
                new FakeTestRecordRepository());

            Assert.Equal("是否对选中的数据进行MES上传?", service.GetConfirmMessage());
        }

        private static TestRecord CreateRecord()
        {
            return CreateRecord(20260520010101UL, 2, "SN1;SN2");
        }

        private static TestRecord CreateRecord(ulong logGuid, ushort channel, string barcode)
        {
            return new TestRecord
            {
                LogGuid = logGuid,
                Channel = channel,
                Barcode = barcode
            };
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

            public MesCallResult LoginResult { get; set; } = MesCallResult.Success();

            public MesCallResult SendChannelBarcodeResult { get; set; } = MesCallResult.Success();

            public MesCallResult StationCheckResult { get; set; } = MesCallResult.Success();

            public MesCallResult UploadDataResult { get; set; } = MesCallResult.Success();

            public Func<ulong, ushort, MesCallResult>? UploadDataHandler { get; set; }

            public MesCallResult LoginMes()
            {
                Calls.Add("LoginMes");
                return LoginResult;
            }

            public MesCallResult SendChannelBarcode(ushort channel, IReadOnlyList<string> barcodes)
            {
                Calls.Add($"SendChannelBarcode:{channel}:{string.Join("|", barcodes)}");
                return SendChannelBarcodeResult;
            }

            public MesCallResult StationCheck(string barcode)
            {
                Calls.Add($"StationCheck:{barcode}");
                return StationCheckResult;
            }

            public MesCallResult UploadData(ulong logGuid, ushort channel)
            {
                Calls.Add($"UploadData:{logGuid}:{channel}");
                if (UploadDataHandler != null)
                {
                    return UploadDataHandler(logGuid, channel);
                }

                return UploadDataResult;
            }
        }

        private sealed class FakeTestRecordRepository : ITestRecordRepository
        {
            public List<TestRecord> UpdatedRecords { get; } = new List<TestRecord>();

            public int GetTotalCount(DataQueryCriteria criteria)
            {
                return 0;
            }

            public IReadOnlyList<TestRecord> GetRecords(DataQueryCriteria criteria, int page, int pageSize)
            {
                return Array.Empty<TestRecord>();
            }

            public IReadOnlyList<TestDetailRecord> GetDetails(TestRecord record)
            {
                return Array.Empty<TestDetailRecord>();
            }

            public void UpdateIndexInfo(TestRecord record)
            {
                UpdatedRecords.Add(record);
            }
        }
    }
}
