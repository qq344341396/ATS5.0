using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ATS5.Modules.ProductTest.ViewModels;

namespace ATS5.Modules.ProductTest.Views
{
    public partial class BarcodeDialog : Window
    {
        public BarcodeDialog()
        {
            InitializeComponent();
        }

        private BarcodeDialogViewModel? ViewModel => DataContext as BarcodeDialogViewModel;

        private void Barcode_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || ViewModel == null)
            {
                return;
            }

            if (sender is FrameworkElement element &&
                element.DataContext is BarcodeInputViewModel input)
            {
                var action = ViewModel.HandleEnter(input.Index);
                if (action == BarcodeEnterAction.Confirmed)
                {
                    DialogResult = true;
                    Close();
                }
                else if (action == BarcodeEnterAction.FocusNext)
                {
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            }

            e.Handled = true;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.ConfirmCommand.Execute();
            if (ViewModel?.Result.IsConfirmed == true)
            {
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.CancelCommand.Execute();
            DialogResult = false;
            Close();
        }
    }
}
