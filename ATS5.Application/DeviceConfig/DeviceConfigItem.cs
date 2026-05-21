namespace ATS5.Application.DeviceConfig
{
    public sealed class DeviceConfigItem
    {
        public string? DevType { get; set; }

        public string? DevName { get; set; }

        public string? DevCode { get; set; }

        public string? InitPars { get; set; }

        public string? InitRemark { get; set; }

        public string? DevClasss { get; set; }

        public bool IsEnable { get; set; }

        public string? Remark { get; set; }

        public bool IsGlobal { get; set; }
    }
}
