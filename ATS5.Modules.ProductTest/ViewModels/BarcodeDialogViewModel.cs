using System;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Commands;
using Prism.Mvvm;

namespace ATS5.Modules.ProductTest.ViewModels
{
    public sealed class BarcodeDialogViewModel : BindableBase
    {
        private readonly string? _previousFlowName;
        private string? _selectedFlowName;
        private string _errorMessage = string.Empty;
        private int _status = -1;
        private int _focusedBarcodeIndex;
        private int _barcodeCount;
        private BarcodeDialogResult _result = BarcodeDialogResult.Cancelled(-1);

        public BarcodeDialogViewModel(BarcodeDialogRequest request)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
            _previousFlowName = request.FlowName;
            _selectedFlowName = request.FlowName;
            _barcodeCount = request.BarcodeCount;
            ConfirmCommand = new DelegateCommand(Confirm);
            CancelCommand = new DelegateCommand(Cancel);
            RebuildBarcodeInputs();
        }

        public BarcodeDialogRequest Request { get; }

        public ObservableCollection<BarcodeInputViewModel> BarcodeInputs { get; } =
            new ObservableCollection<BarcodeInputViewModel>();

        public string? SelectedFlowName
        {
            get => _selectedFlowName;
            set => SetProperty(ref _selectedFlowName, value);
        }

        public int BarcodeCount
        {
            get => _barcodeCount;
            set
            {
                var normalizedValue = Math.Max(1, Math.Min(10, value));
                if (SetProperty(ref _barcodeCount, normalizedValue))
                {
                    RebuildBarcodeInputs();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value ?? string.Empty);
        }

        public int Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }

        public int FocusedBarcodeIndex
        {
            get => _focusedBarcodeIndex;
            private set => SetProperty(ref _focusedBarcodeIndex, value);
        }

        public BarcodeDialogResult Result
        {
            get => _result;
            private set => SetProperty(ref _result, value);
        }

        public DelegateCommand ConfirmCommand { get; }

        public DelegateCommand CancelCommand { get; }

        public BarcodeEnterAction HandleEnter(int barcodeIndex)
        {
            if (barcodeIndex < BarcodeInputs.Count - 1)
            {
                FocusedBarcodeIndex = barcodeIndex + 1;
                return BarcodeEnterAction.FocusNext;
            }

            Confirm();
            return Result.IsConfirmed ? BarcodeEnterAction.Confirmed : BarcodeEnterAction.None;
        }

        private void Confirm()
        {
            ErrorMessage = string.Empty;

            var selectedFlowName = SelectedFlowName ?? string.Empty;
            if (string.IsNullOrEmpty(selectedFlowName))
            {
                ErrorMessage = "流程文件不能为空";
                return;
            }

            var barcodes = BarcodeInputs.Select(input => input.Value.Trim()).ToList();
            if (!Request.BarcodeNullable && barcodes.Any(string.IsNullOrEmpty))
            {
                ErrorMessage = "条码不能为空";
                return;
            }

            Status = 1;
            Result = BarcodeDialogResult.Confirmed(selectedFlowName, barcodes);
        }

        private void Cancel()
        {
            Status = string.Equals(SelectedFlowName, _previousFlowName, StringComparison.Ordinal) ? 2 : 0;
            Result = BarcodeDialogResult.Cancelled(Status);
        }

        private void RebuildBarcodeInputs()
        {
            BarcodeInputs.Clear();
            for (var index = 0; index < BarcodeCount; index++)
            {
                var label = BarcodeCount == 1 ? "条码" : $"条码{index + 1}";
                BarcodeInputs.Add(new BarcodeInputViewModel(index, label));
            }
        }
    }
}
