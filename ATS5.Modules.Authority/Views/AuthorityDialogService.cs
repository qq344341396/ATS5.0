using System;
using System.Linq;
using System.Windows;
using ATS5.Application.Authority;
using ATS5.Modules.Authority.ViewModels;

namespace ATS5.Modules.Authority.Views
{
    public sealed class AuthorityDialogService : IAuthorityDialogService
    {
        public AuthorityDialogResult<AuthorityRole> ShowRoleDialog(string title, AuthorityRole? role)
        {
            var dialog = new RoleDialog();
            AssignOwner(dialog);
            dialog.Title = title;
            dialog.Role = role?.Clone() ?? new AuthorityRole();
            var isOk = dialog.ShowDialog() == true;
            return new AuthorityDialogResult<AuthorityRole>(isOk, dialog.Role);
        }

        public AuthorityDialogResult<AuthorityUser> ShowUserDialog(string title, AuthorityUser? user)
        {
            var dialog = new UserDialog();
            AssignOwner(dialog);
            dialog.Title = title;
            dialog.User = user?.Clone() ?? new AuthorityUser();
            var isOk = dialog.ShowDialog() == true;
            return new AuthorityDialogResult<AuthorityUser>(isOk, dialog.User);
        }

        private static void AssignOwner(Window dialog)
        {
            var owner = ResolveOwner();
            if (owner != null && !ReferenceEquals(owner, dialog))
            {
                dialog.Owner = owner;
            }
        }

        private static Window? ResolveOwner()
        {
            return System.Windows.Application.Current?.Windows
                .OfType<Window>()
                .FirstOrDefault(window => window.IsActive) ??
                System.Windows.Application.Current?.MainWindow;
        }
    }
}
