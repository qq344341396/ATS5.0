using System.Windows.Controls;
using System.Windows.Input;

namespace ATS5.Modules.Device.Views
{
    public partial class DeviceManagerView : UserControl
    {
        public DeviceManagerView()
        {
            InitializeComponent();
        }

        private void DeviceLibrary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not ViewModels.DeviceManagerViewModel viewModel || !viewModel.AddDeviceFromLibraryCommand.CanExecute())
            {
                return;
            }

            viewModel.AddDeviceFromLibraryCommand.Execute();
        }

        private void DeviceLibrary_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is ViewModels.DeviceManagerViewModel viewModel)
            {
                viewModel.SelectedLibraryItem = (e.NewValue as ViewModels.DeviceLibraryNodeViewModel)?.Item;
            }
        }
    }
}
