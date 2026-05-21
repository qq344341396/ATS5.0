using System.Windows;
using ATS5.Application.Authority;

namespace ATS5.Modules.Authority.Views
{
    public partial class UserDialog : Window
    {
        private AuthorityUser _user = new AuthorityUser();

        public UserDialog()
        {
            InitializeComponent();
        }

        public AuthorityUser User
        {
            get
            {
                _user.UserName = UserNameTextBox.Text;
                _user.PW = PasswordTextBox.Password;
                return _user;
            }
            set
            {
                _user = value ?? new AuthorityUser();
                DataContext = _user;
                PasswordTextBox.Password = _user.PW;
            }
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
