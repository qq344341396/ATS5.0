using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;

namespace ATS5.Modules.ProductTest.ViewModels
{
    public sealed class ProductTestChannelViewModel : BindableBase
    {
        private const string PassResult = "PASS";
        private const string TestingStatusText = "Testing";
        private const string PassStatusText = "PASS";
        private const string FailStatusText = "FAIL";

        private string _title;
        private string _flowName = string.Empty;
        private int _barcodeCount = 1;

        public ProductTestChannelViewModel(int channelNumber)
        {
            ChannelNumber = channelNumber;
            _title = $"通道{channelNumber}";
        }

        public int ChannelNumber { get; }

        public string Title
        {
            get => _title;
            private set => SetProperty(ref _title, value);
        }

        public string FlowName
        {
            get => _flowName;
            set => SetProperty(ref _flowName, value ?? string.Empty);
        }

        public int BarcodeCount
        {
            get => _barcodeCount;
            set => SetProperty(ref _barcodeCount, value < 1 ? 1 : value);
        }

        public ObservableCollection<string> Barcodes { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

        public void ApplyBarcodeResult(BarcodeDialogResult result)
        {
            if (result == null || !result.IsConfirmed || string.IsNullOrEmpty(result.FlowName))
            {
                return;
            }

            FlowName = result.FlowName ?? string.Empty;
            BarcodeCount = result.Barcodes.Count;
            Barcodes.Clear();
            foreach (var barcode in result.Barcodes)
            {
                Barcodes.Add(barcode);
            }

            Messages.Add("扫码完成，等待执行。");
        }

        public void UpdateTitle(ProductTestShellStatus testStatus, IReadOnlyList<string> testResults, bool mesResult)
        {
            switch (testStatus)
            {
                case ProductTestShellStatus.Start:
                    Title = $"通道{ChannelNumber}【{TestingStatusText}】";
                    break;
                case ProductTestShellStatus.Complete:
                    var allPass = testResults != null && testResults.All(result => result == PassResult);
                    Title = allPass && mesResult
                        ? $"通道{ChannelNumber}【{PassStatusText}】"
                        : $"通道{ChannelNumber}【{FailStatusText}】";
                    break;
                case ProductTestShellStatus.Stop:
                    Title = $"通道{ChannelNumber}【{FailStatusText}】";
                    break;
                default:
                    Title = $"通道{ChannelNumber}";
                    break;
            }
        }
    }
}
