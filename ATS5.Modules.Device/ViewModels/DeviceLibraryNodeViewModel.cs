using System.Collections.ObjectModel;
using ATS5.Application.DeviceConfig;

namespace ATS5.Modules.Device.ViewModels
{
    public sealed class DeviceLibraryNodeViewModel
    {
        public DeviceLibraryNodeViewModel(DeviceLibraryItem item)
        {
            Item = item;
            Children = new ObservableCollection<DeviceLibraryNodeViewModel>();
        }

        public DeviceLibraryItem Item { get; }

        public string? Name => Item.DevName;

        public bool CanAddDevice => !Item.IsCategory;

        public ObservableCollection<DeviceLibraryNodeViewModel> Children { get; }
    }
}
