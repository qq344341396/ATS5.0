using System.Windows;
using ATS5.Modules.ProductTest.ViewModels;

namespace ATS5.Modules.ProductTest.Views
{
    public sealed class BarcodeDialogService : IBarcodeDialogService
    {
        public BarcodeDialogResult ShowBarcodeDialog(BarcodeDialogRequest request)
        {
            var viewModel = new BarcodeDialogViewModel(request);
            var dialog = new BarcodeDialog
            {
                DataContext = viewModel,
                Owner = System.Windows.Application.Current?.MainWindow
            };

            var result = dialog.ShowDialog();
            if (result == true && viewModel.Result.IsConfirmed)
            {
                return viewModel.Result;
            }

            if (viewModel.Status == -1)
            {
                viewModel.CancelCommand.Execute();
            }

            return BarcodeDialogResult.Cancelled(viewModel.Status);
        }
    }
}
