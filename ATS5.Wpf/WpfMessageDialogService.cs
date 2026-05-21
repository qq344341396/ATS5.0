using System.Windows;
using ATS5.Wpf.Core;
using HandyMessageBox = HandyControl.Controls.MessageBox;

namespace ATS5.Wpf
{
    public sealed class WpfMessageDialogService : IMessageDialogService
    {
        public bool Confirm(string message)
        {
            return HandyMessageBox.Show(message, "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK;
        }

        public void ShowInfo(string message)
        {
            HandyMessageBox.Show(message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowWarning(string message)
        {
            HandyMessageBox.Show(message, "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public void ShowError(string message)
        {
            HandyMessageBox.Show(message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
