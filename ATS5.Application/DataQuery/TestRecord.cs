using System;

namespace ATS5.Application.DataQuery
{
    public sealed class TestRecord
    {
        public ulong LogGuid { get; set; }

        public ushort Channel { get; set; }

        public string Barcode { get; set; } = string.Empty;

        public string ProcessName { get; set; } = string.Empty;

        public string ProcessInfo { get; set; } = string.Empty;

        public DateTime TestStartTime { get; set; }

        public DateTime TestEndTime { get; set; }

        public double TakeTime { get; set; }

        public short UploadMesStatus { get; set; } = -1;

        public short UploadFtpStatus { get; set; } = -1;

        public string TestResult { get; set; } = string.Empty;

        public DateTime CreateTime { get; set; }

        public string Reserve1 { get; set; } = string.Empty;

        public string Reserve2 { get; set; } = string.Empty;
    }
}
