using System;
using System.Collections.Generic;
using System.Linq;

namespace ATS5.Application.Authority
{
    public sealed class AuthorityService
    {
        private const string DeleteRoleConfirmationMessage = "确定要删除该角色吗？";
        private const string DeleteUserConfirmationMessage = "确定要删除该用户吗？";

        private static readonly IReadOnlyList<AuthorityPermissionItem> PermissionItems = new[]
        {
            new AuthorityPermissionItem("1001", "产品测试"),
            new AuthorityPermissionItem("1002", "数据查询"),
            new AuthorityPermissionItem("1003", "流程管理"),
            new AuthorityPermissionItem("1004", "设备管理"),
            new AuthorityPermissionItem("1005", "权限管理"),
            new AuthorityPermissionItem("1006", "MES"),
            new AuthorityPermissionItem("1008", "工具"),
            new AuthorityPermissionItem("1009", "系统")
        };

        private readonly IAuthorityRepository _authorityRepository;

        public AuthorityService(IAuthorityRepository authorityRepository)
        {
            _authorityRepository = authorityRepository ?? throw new ArgumentNullException(nameof(authorityRepository));
        }

        public static IReadOnlyList<AuthorityPermissionItem> GetPermissionItems()
        {
            return PermissionItems;
        }

        public IReadOnlyList<AuthorityRole> GetRoles()
        {
            return _authorityRepository.GetRoles();
        }

        public IReadOnlyList<AuthorityUser> GetUsers(int roleId)
        {
            return _authorityRepository.GetUsers(roleId);
        }

        public AuthorityOperationResult AddRole(AuthorityRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (_authorityRepository.GetRoles().Any(item => item.RoleName == role.RoleName))
            {
                return AuthorityOperationResult.Failed($"新增角色失败：角色【{role.RoleName}】已存在");
            }

            var saveRole = role.Clone();
            saveRole.Powers = AuthorityPermissionSerializer.SerializeTags(saveRole.SelectedPermissionTags);
            return _authorityRepository.InsertRole(saveRole) > 0
                ? AuthorityOperationResult.Success()
                : AuthorityOperationResult.Failed("新增失败");
        }

        public AuthorityOperationResult UpdateRole(AuthorityRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (_authorityRepository.GetRoles().Any(item => item.RoleName == role.RoleName && item.ID != role.ID))
            {
                return AuthorityOperationResult.Failed($"修改角色失败：角色【{role.RoleName}】已存在");
            }

            var saveRole = role.Clone();
            saveRole.Powers = AuthorityPermissionSerializer.SerializeTags(saveRole.SelectedPermissionTags);
            return _authorityRepository.UpdateRole(saveRole) > 0
                ? AuthorityOperationResult.Success()
                : AuthorityOperationResult.Failed("修改失败");
        }

        public AuthorityOperationResult DeleteRole(int roleId, bool isConfirmed)
        {
            if (!isConfirmed)
            {
                return AuthorityOperationResult.Cancelled(DeleteRoleConfirmationMessage);
            }

            return _authorityRepository.DeleteRole(roleId) > 0
                ? AuthorityOperationResult.Success()
                : AuthorityOperationResult.Failed("删除失败");
        }

        public AuthorityOperationResult AddUser(AuthorityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (_authorityRepository.GetUsers(user.RID).Any(item => item.UserName == user.UserName))
            {
                return AuthorityOperationResult.Failed($"新增用户失败：用户【{user.UserName}】已存在");
            }

            return _authorityRepository.InsertUser(user.Clone()) > 0
                ? AuthorityOperationResult.Success()
                : AuthorityOperationResult.Failed("新增失败");
        }

        public AuthorityOperationResult UpdateUser(AuthorityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (_authorityRepository.GetUsers(user.RID).Any(item => item.UserName == user.UserName && item.ID != user.ID))
            {
                return AuthorityOperationResult.Failed($"修改用户失败：用户【{user.UserName}】已存在");
            }

            return _authorityRepository.UpdateUser(user.Clone()) > 0
                ? AuthorityOperationResult.Success()
                : AuthorityOperationResult.Failed("修改失败");
        }

        public AuthorityOperationResult DeleteUser(int userId, bool isConfirmed)
        {
            if (!isConfirmed)
            {
                return AuthorityOperationResult.Cancelled(DeleteUserConfirmationMessage);
            }

            return _authorityRepository.DeleteUser(userId) > 0
                ? AuthorityOperationResult.Success()
                : AuthorityOperationResult.Failed("删除失败");
        }

    }
}
