using System.Collections.Generic;

namespace ATS5.Application.DeviceConfig
{
    public interface IDeviceConfigRepository
    {
        IReadOnlyList<DeviceConfigName> GetConfigNames();

        DeviceConfigOpenResult Open(string configName);

        IReadOnlyList<DeviceLibraryItem> GetDeviceLibrary();

        string Serialize(IReadOnlyList<DeviceConfigItem> devices);

        void Write(string configName, IReadOnlyList<DeviceConfigItem> devices);
    }
}
