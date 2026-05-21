using ATS5.Modules.Device.ViewModels;
using ATS5.Modules.Device.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace ATS5.Modules.Device
{
    public sealed class DeviceModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IDeviceConfigDialogService, DeviceConfigDialogService>();
            containerRegistry.RegisterForNavigation<DeviceManagerView>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
