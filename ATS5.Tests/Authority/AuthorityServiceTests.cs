using System;
using System.Collections.Generic;
using System.Linq;
using ATS5.Application.Authority;
using Xunit;

namespace ATS5.Tests.Authority
{
    public sealed class AuthorityServiceTests
    {
        [Fact]
        public void GetPermissionItems_ReturnsLegacyTagsAndNamesInOrder()
        {
            var items = AuthorityService.GetPermissionItems();

            Assert.Equal(new[]
            {
                "1001:产品测试",
                "1002:数据查询",
                "1003:流程管理",
                "1004:设备管理",
                "1005:权限管理",
                "1006:MES",
                "1008:工具",
                "1009:系统"
            }, items.Select(item => $"{item.Tag}:{item.Name}"));
        }

        [Fact]
        public void AddRole_ReturnsLegacyDuplicateMessage_AndDoesNotInsert_WhenNameExists()
        {
            var repository = new FakeAuthorityRepository
            {
                Roles = new[]
                {
                    new AuthorityRole { ID = 1, RoleName = "管理员", Remark = "旧角色" }
                }
            };
            var service = new AuthorityService(repository);

            var result = service.AddRole(new AuthorityRole { RoleName = "管理员", Remark = "新角色" });

            Assert.False(result.IsSuccess);
            Assert.Equal("新增角色失败：角色【管理员】已存在", result.Message);
            Assert.Equal(0, repository.InsertRoleCallCount);
        }

        [Fact]
        public void UpdateRole_ReturnsLegacyDuplicateMessage_AndDoesNotUpdate_WhenOtherRoleHasSameName()
        {
            var repository = new FakeAuthorityRepository
            {
                Roles = new[]
                {
                    new AuthorityRole { ID = 1, RoleName = "管理员" },
                    new AuthorityRole { ID = 2, RoleName = "操作员" }
                }
            };
            var service = new AuthorityService(repository);

            var result = service.UpdateRole(new AuthorityRole { ID = 2, RoleName = "管理员" });

            Assert.False(result.IsSuccess);
            Assert.Equal("修改角色失败：角色【管理员】已存在", result.Message);
            Assert.Equal(0, repository.UpdateRoleCallCount);
        }

        [Fact]
        public void AddRole_SerializesSelectedPermissionTagsAsJson()
        {
            var repository = new FakeAuthorityRepository();
            var service = new AuthorityService(repository);

            var result = service.AddRole(new AuthorityRole
            {
                RoleName = "工艺",
                Remark = "remark",
                SelectedPermissionTags = new[] { "1001", "1005" }
            });

            Assert.True(result.IsSuccess);
            Assert.Equal("[\"1001\",\"1005\"]", repository.LastInsertedRole?.Powers);
        }

        [Fact]
        public void DeleteRole_UsesLegacyConfirmationText_AndSkipsRepositoryWhenCancelled()
        {
            var repository = new FakeAuthorityRepository();
            var service = new AuthorityService(repository);

            var result = service.DeleteRole(9, isConfirmed: false);

            Assert.False(result.IsSuccess);
            Assert.Equal("确定要删除该角色吗？", result.ConfirmationMessage);
            Assert.Equal(0, repository.DeleteRoleCallCount);
        }

        [Fact]
        public void AddUser_ReturnsLegacyDuplicateMessageWithinCurrentRoleOnly()
        {
            var repository = new FakeAuthorityRepository
            {
                Users = new[]
                {
                    new AuthorityUser { ID = 1, RID = 3, UserName = "alice", PW = "old" },
                    new AuthorityUser { ID = 2, RID = 4, UserName = "alice", PW = "other" }
                }
            };
            var service = new AuthorityService(repository);

            var duplicateResult = service.AddUser(new AuthorityUser { RID = 3, UserName = "alice", PW = "new" });
            var otherRoleResult = service.AddUser(new AuthorityUser { RID = 4, UserName = "bob", PW = "pw" });

            Assert.False(duplicateResult.IsSuccess);
            Assert.Equal("新增用户失败：用户【alice】已存在", duplicateResult.Message);
            Assert.True(otherRoleResult.IsSuccess);
            Assert.Equal("pw", repository.LastInsertedUser?.PW);
        }

        [Fact]
        public void UpdateUser_DoesNotExposePasswordInFailureMessage()
        {
            var repository = new FakeAuthorityRepository
            {
                Users = new[]
                {
                    new AuthorityUser { ID = 1, RID = 8, UserName = "alice", PW = "secret-1" },
                    new AuthorityUser { ID = 2, RID = 8, UserName = "bob", PW = "secret-2" }
                }
            };
            var service = new AuthorityService(repository);

            var result = service.UpdateUser(new AuthorityUser { ID = 2, RID = 8, UserName = "alice", PW = "new-secret" });

            Assert.False(result.IsSuccess);
            Assert.Equal("修改用户失败：用户【alice】已存在", result.Message);
            Assert.DoesNotContain("new-secret", result.Message);
        }

        private sealed class FakeAuthorityRepository : IAuthorityRepository
        {
            public IReadOnlyList<AuthorityRole> Roles { get; set; } = Array.Empty<AuthorityRole>();

            public IReadOnlyList<AuthorityUser> Users { get; set; } = Array.Empty<AuthorityUser>();

            public AuthorityRole? LastInsertedRole { get; private set; }

            public AuthorityUser? LastInsertedUser { get; private set; }

            public int InsertRoleCallCount { get; private set; }

            public int UpdateRoleCallCount { get; private set; }

            public int DeleteRoleCallCount { get; private set; }

            public IReadOnlyList<AuthorityRole> GetRoles()
            {
                return Roles;
            }

            public IReadOnlyList<AuthorityUser> GetUsers(int roleId)
            {
                return Users.Where(user => user.RID == roleId).ToList();
            }

            public int InsertRole(AuthorityRole role)
            {
                InsertRoleCallCount++;
                LastInsertedRole = role;
                return 1;
            }

            public int UpdateRole(AuthorityRole role)
            {
                UpdateRoleCallCount++;
                return 1;
            }

            public int DeleteRole(int roleId)
            {
                DeleteRoleCallCount++;
                return 1;
            }

            public int InsertUser(AuthorityUser user)
            {
                LastInsertedUser = user;
                return 1;
            }

            public int UpdateUser(AuthorityUser user)
            {
                return 1;
            }

            public int DeleteUser(int userId)
            {
                return 1;
            }
        }
    }
}
