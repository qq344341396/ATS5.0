using System.Collections.Generic;
using ATS5.Application.DeviceConfig;

namespace ATS5.Modules.Device.ViewModels
{
    public interface IDeviceConfigDialogService
    {
        string? SelectConfigName(IReadOnlyList<DeviceConfigName> configNames);

        string? RequestSaveConfigName(IReadOnlyList<DeviceConfigName> configNames);

        bool ConfirmDeleteDevice();

        bool ConfirmCancelEdit();
    }
}
