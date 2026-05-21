using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ATS5.Application.Authority;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using ATSCommon;
using ATSCore.DBCore;
using ATSModel.DBModel;

namespace ATS5.Infrastructure.LegacyAdapters.Authority
{
    public sealed class LegacyAuthorityRepository : IAuthorityRepository
    {
        private readonly LegacyRuntimeContext _runtimeContext;

        public LegacyAuthorityRepository(LegacyRuntimeContext runtimeContext)
        {
            _runtimeContext = runtimeContext ?? throw new ArgumentNullException(nameof(runtimeContext));
        }

        public IReadOnlyList<AuthorityRole> GetRoles()
        {
            return ExecuteAuthorityQuery(() => RoleCore.GetList().Select(ToAuthorityRole).ToList());
        }

        public IReadOnlyList<AuthorityUser> GetUsers(int roleId)
        {
            return ExecuteAuthorityQuery(() => UserCore.Get(roleId).Select(ToAuthorityUser).ToList());
        }

        public int InsertRole(AuthorityRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return ExecuteAuthorityQuery(() => RoleCore.Insert(ToLegacyRole(role)));
        }

        public int UpdateRole(AuthorityRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return ExecuteAuthorityQuery(() => RoleCore.Update(ToLegacyRole(role)));
        }

        public int DeleteRole(int roleId)
        {
            return ExecuteAuthorityQuery(() => RoleCore.Delete(roleId));
        }

        public int InsertUser(AuthorityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return ExecuteAuthorityQuery(() => UserCore.Insert(ToLegacyUser(user)));
        }

        public int UpdateUser(AuthorityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return ExecuteAuthorityQuery(() => UserCore.Update(ToLegacyUser(user)));
        }

        public int DeleteUser(int userId)
        {
            return ExecuteAuthorityQuery(() => UserCore.Delete(userId));
        }

        private T ExecuteAuthorityQuery<T>(Func<T> action)
        {
            return _runtimeContext.Execute(
                () =>
                {
                    SQLiteHelper.ChangeConnection(_runtimeContext.GetLegacyPath(Path.Combine("AppData", "ats.db")));
                    return action();
                });
        }

        private static AuthorityRole ToAuthorityRole(Role source)
        {
            return new AuthorityRole
            {
                ID = source.ID,
                RoleName = source.RoleName ?? string.Empty,
                Powers = source.Powers ?? "[]",
                Remark = source.Remark ?? string.Empty
            };
        }

        private static AuthorityUser ToAuthorityUser(User source)
        {
            return new AuthorityUser
            {
                ID = source.ID,
                RID = source.RID,
                UserName = source.UserName ?? string.Empty,
                PW = source.PW ?? string.Empty
            };
        }

        private static Role ToLegacyRole(AuthorityRole source)
        {
            return new Role
            {
                ID = source.ID,
                RoleName = source.RoleName,
                Powers = source.Powers,
                Remark = source.Remark
            };
        }

        private static User ToLegacyUser(AuthorityUser source)
        {
            return new User
            {
                ID = source.ID,
                RID = source.RID,
                UserName = source.UserName,
                PW = source.PW
            };
        }
    }
}
