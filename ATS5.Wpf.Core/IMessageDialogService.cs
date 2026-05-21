namespace ATS5.Wpf.Core
{
    public interface IMessageDialogService
    {
        bool Confirm(string message);

        void ShowInfo(string message);

        void ShowWarning(string message);

        void ShowError(string message);
    }
}
