using System;
using System.Collections.ObjectModel;
using ATS5.Application.DataQuery;
using ATS5.Application.ProductTestShell;
using Prism.Commands;
using Prism.Mvvm;

namespace ATS5.Modules.ProductTest.ViewModels
{
    public sealed class ProductTestShellViewModel : BindableBase
    {
        private const string ChannelNumKey = "ChannelNum";
        private const string BarcodeNullableKey = "BarcodeNullable";

        private readonly IAppConfigService _appConfigService;
        private readonly IProductTestFlowCatalog _flowCatalog;
        private readonly IBarcodeDialogService _barcodeDialogService;
        private ProductTestChannelViewModel? _selectedChannel;
        private ProductTestShellStatus _currentStatus = ProductTestShellStatus.Default;

        public ProductTestShellViewModel(
            IAppConfigService appConfigService,
            IProductTestFlowCatalog flowCatalog,
            IBarcodeDialogService barcodeDialogService)
        {
            _appConfigService = appConfigService ?? throw new ArgumentNullException(nameof(appConfigService));
            _flowCatalog = flowCatalog ?? throw new ArgumentNullException(nameof(flowCatalog));
            _barcodeDialogService = barcodeDialogService ?? throw new ArgumentNullException(nameof(barcodeDialogService));
            ExecuteCommand = new DelegateCommand(Execute, CanExecute);
            PauseCommand = new DelegateCommand(Pause, CanPause);
            StopCommand = new DelegateCommand(Stop, CanStop);
            LoadChannels();
        }

        public ObservableCollection<ProductTestChannelViewModel> Channels { get; } =
            new ObservableCollection<ProductTestChannelViewModel>();

        public ProductTestChannelViewModel? SelectedChannel
        {
            get => _selectedChannel;
            set => SetProperty(ref _selectedChannel, value);
        }

        public DelegateCommand ExecuteCommand { get; }

        public DelegateCommand PauseCommand { get; }

        public DelegateCommand StopCommand { get; }

        public void LoadChannels()
        {
            Channels.Clear();
            var channelCount = GetChannelCount();
            for (var channelNumber = 1; channelNumber <= channelCount; channelNumber++)
            {
                Channels.Add(new ProductTestChannelViewModel(channelNumber));
            }

            SelectedChannel = Channels.Count > 0 ? Channels[0] : null;
        }

        private void Execute()
        {
            if (SelectedChannel == null)
            {
                return;
            }

            var request = new BarcodeDialogRequest(
                SelectedChannel.ChannelNumber,
                SelectedChannel.FlowName,
                SelectedChannel.BarcodeCount,
                IsBarcodeNullable(),
                _flowCatalog.GetFlowNames());
            var result = _barcodeDialogService.ShowBarcodeDialog(request);
            SelectedChannel.ApplyBarcodeResult(result);
            _currentStatus = ProductTestShellStatus.Default;
            RaiseCommandStates();
        }

        private void Pause()
        {
        }

        private void Stop()
        {
            if (SelectedChannel != null)
            {
                SelectedChannel.UpdateTitle(ProductTestShellStatus.Stop, Array.Empty<string>(), mesResult: false);
            }

            _currentStatus = ProductTestShellStatus.Default;
            RaiseCommandStates();
        }

        private bool CanExecute()
        {
            return _currentStatus == ProductTestShellStatus.Default && SelectedChannel != null;
        }

        private bool CanPause()
        {
            return _currentStatus == ProductTestShellStatus.Start;
        }

        private bool CanStop()
        {
            return _currentStatus == ProductTestShellStatus.Start;
        }

        private int GetChannelCount()
        {
            var value = _appConfigService.GetValue(ChannelNumKey);
            return int.TryParse(value, out var count) && count > 0 ? count : 1;
        }

        private bool IsBarcodeNullable()
        {
            return _appConfigService.GetValue(BarcodeNullableKey) != "0";
        }

        private void RaiseCommandStates()
        {
            ExecuteCommand.RaiseCanExecuteChanged();
            PauseCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
        }
    }
}
