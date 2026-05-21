using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ATS5.Application.DataQuery
{
    public sealed class ManualMesUploadService
    {
        private const string MesEnableKey = "MesEnable";
        private const string MesEnabledValue = "1";

        private readonly IAppConfigService _appConfigService;
        private readonly IManualMesGateway _manualMesGateway;
        private readonly ITestRecordRepository _testRecordRepository;

        public ManualMesUploadService(
            IAppConfigService appConfigService,
            IManualMesGateway manualMesGateway,
            ITestRecordRepository testRecordRepository)
        {
            _appConfigService = appConfigService ?? throw new ArgumentNullException(nameof(appConfigService));
            _manualMesGateway = manualMesGateway ?? throw new ArgumentNullException(nameof(manualMesGateway));
            _testRecordRepository = testRecordRepository ?? throw new ArgumentNullException(nameof(testRecordRepository));
        }

        /// <summary>
        /// Validates the legacy manual MES upload preconditions before the confirmation prompt is shown.
        /// </summary>
        public ManualMesUploadResult ValidateBeforeConfirmation(IReadOnlyList<TestRecord> selectedRecords)
        {
            if (selectedRecords == null || selectedRecords.Count == 0)
            {
                return Fail("未选中数据!");
            }

            if (!IsMesEnabled())
            {
                return Fail("未启用MES!");
            }

            return new ManualMesUploadResult(true, Array.Empty<string>());
        }

        public Task<ManualMesUploadResult> UploadAsync(
            IReadOnlyList<TestRecord> selectedRecords,
            bool isConfirmed,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var validationResult = ValidateBeforeConfirmation(selectedRecords);
            if (!validationResult.IsSuccess)
            {
                return Task.FromResult(validationResult);
            }

            if (!isConfirmed)
            {
                return Task.FromResult(new ManualMesUploadResult(false, Array.Empty<string>()));
            }

            var messages = new List<string>();
            var loginResult = _manualMesGateway.LoginMes();
            if (!loginResult.Status)
            {
                return Task.FromResult(Fail($"MES登录失败:{loginResult.Message}"));
            }

            foreach (var item in selectedRecords)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var barcodeItems = item.Barcode.Split(new[] { ';' }, StringSplitOptions.None).ToList();
                var channelBarcodeResult = _manualMesGateway.SendChannelBarcode(item.Channel, barcodeItems);
                if (!channelBarcodeResult.Status)
                {
                    return Task.FromResult(Fail($"传入通道号和条码:失败{channelBarcodeResult.Message}"));
                }

                var stationCheckResult = _manualMesGateway.StationCheck(item.Barcode);
                if (!stationCheckResult.Status)
                {
                    return Task.FromResult(Fail($"MES工序校验失败:{stationCheckResult.Message}"));
                }

                var uploadResult = _manualMesGateway.UploadData(item.LogGuid, item.Channel);
                if (!uploadResult.IsHandled)
                {
                    continue;
                }

                if (uploadResult.Status)
                {
                    messages.Add($"条码为：{item.Barcode},MES上传成功!");
                    item.UploadMesStatus = 1;
                    _testRecordRepository.UpdateIndexInfo(item);
                }
                else
                {
                    messages.Add($"条码为：{item.Barcode},MES上传失败:{uploadResult.Message}");
                }
            }

            return Task.FromResult(new ManualMesUploadResult(messages.All(m => m.Contains("成功")), messages));
        }

        public string GetConfirmMessage()
        {
            return "是否对选中的数据进行MES上传?";
        }

        private static ManualMesUploadResult Fail(string message)
        {
            return new ManualMesUploadResult(false, new[] { message });
        }

        private bool IsMesEnabled()
        {
            return _appConfigService.GetValue(MesEnableKey) == MesEnabledValue;
        }
    }
}
