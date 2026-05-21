using ATS5.Modules.DataQuery.Views;
using ATS5.Modules.Flow.Views;
using ATS5.Modules.Authority.Views;
using ATS5.Modules.Device.Views;
using ATS5.Modules.ProductTest.Views;
using ATS5.Wpf.Core;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace ATS5.Wpf.ViewModels
{
    public sealed class ShellViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        public ShellViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            NavigateDataQueryCommand = new DelegateCommand(NavigateDataQuery);
            NavigateFlowCommand = new DelegateCommand(NavigateFlow);
            NavigateAuthorityCommand = new DelegateCommand(NavigateAuthority);
            NavigateDeviceCommand = new DelegateCommand(NavigateDevice);
            NavigateProductTestCommand = new DelegateCommand(NavigateProductTest);
        }

        public DelegateCommand NavigateProductTestCommand { get; }

        public DelegateCommand NavigateDataQueryCommand { get; }

        public DelegateCommand NavigateFlowCommand { get; }

        public DelegateCommand NavigateAuthorityCommand { get; }

        public DelegateCommand NavigateDeviceCommand { get; }

        private void NavigateProductTest()
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(ProductTestShellView));
        }

        private void NavigateDataQuery()
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(DataQueryView));
        }

        private void NavigateFlow()
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(FlowEditorView));
        }

        private void NavigateAuthority()
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(AuthorityView));
        }

        private void NavigateDevice()
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(DeviceManagerView));
        }
    }
}
