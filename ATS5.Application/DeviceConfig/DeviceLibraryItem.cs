namespace ATS5.Application.DeviceConfig
{
    public sealed class DeviceLibraryItem
    {
        public string? Id { get; set; }

        public string? DevType { get; set; }

        public string? DevName { get; set; }

        public string? ClassName { get; set; }

        public string? InitParExample { get; set; }

        public string? InitParRemark { get; set; }

        public bool IsCategory => string.IsNullOrEmpty(DevType);
    }
}
