using System;

namespace ATS5.Application.DataQuery
{
    public sealed class DataQueryFilter
    {
        private const int MaxDateSpanDays = 180;
        private const string DateSpanWarningMessage = "日期跨度不能大于180天，请调整查询日期";

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsBarcodeFilterEnabled { get; set; }

        public string? Barcode { get; set; }

        public bool IsChannelFilterEnabled { get; set; }

        public ushort Channel { get; set; }

        public bool IsFlowFilterEnabled { get; set; }

        public string? FlowName { get; set; }

        public ValidationResult Validate()
        {
            return EndDate.Subtract(StartDate).TotalDays > MaxDateSpanDays
                ? ValidationResult.Fail(DateSpanWarningMessage)
                : ValidationResult.Success();
        }

        public DataQueryCriteria ToLegacyCriteria()
        {
            var start = ulong.Parse($"{StartDate:yyyyMMdd}000000");
            var end = ulong.Parse($"{EndDate:yyyyMMdd}235959");
            var barcode = IsBarcodeFilterEnabled && !string.IsNullOrEmpty(Barcode) ? Barcode! : string.Empty;
            var channel = IsChannelFilterEnabled ? Channel : (ushort)0;
            var flowName = IsFlowFilterEnabled && !string.IsNullOrEmpty(FlowName) ? FlowName! : string.Empty;

            return new DataQueryCriteria(start, end, barcode, channel, flowName);
        }
    }
}
