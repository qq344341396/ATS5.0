using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ATS5.Application.Authority;

namespace ATS5.Modules.Authority.Views
{
    public partial class RoleDialog : Window
    {
        private AuthorityRole _role = new AuthorityRole();

        public RoleDialog()
        {
            InitializeComponent();
        }

        public AuthorityRole Role
        {
            get
            {
                _role.RoleName = RoleNameTextBox.Text;
                _role.Remark = RemarkTextBox.Text;
                _role.SelectedPermissionTags = GetSelectedPermissionTags();
                return _role;
            }
            set
            {
                _role = value ?? new AuthorityRole();
                DataContext = _role;
                SetSelectedPermissionTags(AuthorityPermissionSerializer.ParseTags(_role.Powers));
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

        private IReadOnlyList<string> GetSelectedPermissionTags()
        {
            return FindPermissionCheckBoxes()
                .Where(checkBox => checkBox.IsChecked == true)
                .Select(checkBox => checkBox.Tag?.ToString() ?? string.Empty)
                .Where(tag => !string.IsNullOrEmpty(tag))
                .ToList();
        }

        private void SetSelectedPermissionTags(IReadOnlyList<string> tags)
        {
            foreach (var checkBox in FindPermissionCheckBoxes())
            {
                checkBox.IsChecked = tags.Contains(checkBox.Tag?.ToString() ?? string.Empty);
            }
        }

        private IReadOnlyList<CheckBox> FindPermissionCheckBoxes()
        {
            return FindVisualChildren<CheckBox>(this).ToList();
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent)
            where T : DependencyObject
        {
            for (var index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); index++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, index);
                if (child is T typedChild)
                {
                    yield return typedChild;
                }

                foreach (var descendant in FindVisualChildren<T>(child))
                {
                    yield return descendant;
                }
            }
        }
    }
}
