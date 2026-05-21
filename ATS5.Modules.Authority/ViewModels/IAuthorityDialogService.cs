using ATS5.Application.Authority;

namespace ATS5.Modules.Authority.ViewModels
{
    public interface IAuthorityDialogService
    {
        AuthorityDialogResult<AuthorityRole> ShowRoleDialog(string title, AuthorityRole? role);

        AuthorityDialogResult<AuthorityUser> ShowUserDialog(string title, AuthorityUser? user);
    }
}
