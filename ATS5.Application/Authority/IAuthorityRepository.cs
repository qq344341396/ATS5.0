using System.Collections.Generic;

namespace ATS5.Application.Authority
{
    public interface IAuthorityRepository
    {
        IReadOnlyList<AuthorityRole> GetRoles();

        IReadOnlyList<AuthorityUser> GetUsers(int roleId);

        int InsertRole(AuthorityRole role);

        int UpdateRole(AuthorityRole role);

        int DeleteRole(int roleId);

        int InsertUser(AuthorityUser user);

        int UpdateUser(AuthorityUser user);

        int DeleteUser(int userId);
    }
}
