using System;
using System.Collections.Generic;
using System.Linq;
using ATS5.Application.Authority;
using ATS5.Modules.Authority.ViewModels;
using ATS5.Wpf.Core;
using Xunit;

namespace ATS5.Tests.Authority
{
    public sealed class AuthorityViewModelDialogTests
    {
        [Fact]
        public void Constructor_SelectsFirstRoleAndLoadsItsUsers()
        {
            var firstRole = new AuthorityRole { ID = 1, RoleName = "管理员" };
            var secondRole = new AuthorityRole { ID = 2, RoleName = "操作员" };
            var firstUser = new AuthorityUser { ID = 10, RID = 1, UserName = "admin" };
            var repository = new FakeAuthorityRepository
            {
                Roles = new[] { firstRole, secondRole },
                Users = new[] { firstUser, new AuthorityUser { ID = 20, RID = 2, UserName = "operator" } }
            };

            var viewModel = CreateViewModel(repository, new FakeAuthorityDialogService());

            Assert.Equal(1, viewModel.SelectedRole?.ID);
            Assert.Equal("admin", viewModel.Users.Single().UserName);
            Assert.Contains(1, repository.GetUsersRoleIds);
        }

        [Fact]
        public void LoadRoleData_KeepsValidSelectionOrRestoresFirstRole_AndLoadsUsers()
        {
            var firstRole = new AuthorityRole { ID = 1, RoleName = "管理员" };
            var secondRole = new AuthorityRole { ID = 2, RoleName = "操作员" };
            var repository = new FakeAuthorityRepository
            {
                Roles = new[] { firstRole, secondRole },
                Users = new[] { new AuthorityUser { ID = 20, RID = 2, UserName = "operator" } }
            };
            var viewModel = CreateViewModel(repository, new FakeAuthorityDialogService());
            viewModel.SelectedRole = secondRole;

            repository.Roles = new[]
            {
                new AuthorityRole { ID = 2, RoleName = "操作员-改名" },
                new AuthorityRole { ID = 3, RoleName = "访客" }
            };
            viewModel.LoadRoleData();

            Assert.Equal(2, viewModel.SelectedRole?.ID);
            Assert.Equal("operator", viewModel.Users.Single().UserName);

            repository.Roles = new[]
            {
                new AuthorityRole { ID = 3, RoleName = "访客" }
            };
            repository.Users = new[] { new AuthorityUser { ID = 30, RID = 3, UserName = "guest" } };
            viewModel.LoadRoleData();

            Assert.Equal(3, viewModel.SelectedRole?.ID);
            Assert.Equal("guest", viewModel.Users.Single().UserName);
        }

        [Fact]
        public void PublicOperations_ShowErrorMessage_WhenRepositoryThrows()
        {
            var repository = new FakeAuthorityRepository
            {
                Roles = new[] { new AuthorityRole { ID = 1, RoleName = "管理员" } },
                Users = new[] { new AuthorityUser { ID = 2, RID = 1, UserName = "alice", PW = "secret" } }
            };
            var messages = new FakeMessageDialogService();
            var viewModel = CreateViewModel(repository, new FakeAuthorityDialogService(), messages);

            repository.ThrowOnInsertRole = new InvalidOperationException("insert role failed");
            repository.ThrowOnUpdateRole = new InvalidOperationException("update role failed");
            repository.ThrowOnInsertUser = new InvalidOperationException("insert user failed");
            repository.ThrowOnUpdateUser = new InvalidOperationException("update user failed");

            viewModel.AddRole(new AuthorityRole { RoleName = "新增" });
            viewModel.EditRole(new AuthorityRole { ID = 1, RoleName = "修改" });
            viewModel.AddUser(new AuthorityUser { RID = 1, UserName = "bob", PW = "new-secret" });
            viewModel.EditUser(new AuthorityUser { ID = 2, RID = 1, UserName = "alice", PW = "edit-secret" });

            Assert.Equal(new[]
            {
                "insert role failed",
                "update role failed",
                "insert user failed",
                "update user failed"
            }, messages.ErrorMessages);
            Assert.DoesNotContain(messages.ErrorMessages, message => message.Contains("new-secret") || message.Contains("edit-secret"));
        }

        [Fact]
        public void PublicOperations_ReturnNonNullFailureResult_WhenRepositoryThrows()
        {
            var repository = new FakeAuthorityRepository
            {
                Roles = new[] { new AuthorityRole { ID = 1, RoleName = "管理员" } },
                Users = new[] { new AuthorityUser { ID = 2, RID = 1, UserName = "alice", PW = "secret" } },
                ThrowOnInsertRole = new InvalidOperationException("insert role failed"),
                ThrowOnUpdateRole = new InvalidOperationException("update role failed"),
                ThrowOnInsertUser = new InvalidOperationException("insert user failed"),
                ThrowOnUpdateUser = new InvalidOperationException("update user failed")
            };
            var messages = new FakeMessageDialogService();
            var viewModel = CreateViewModel(repository, new FakeAuthorityDialogService(), messages);

            var addRoleResult = viewModel.AddRole(new AuthorityRole { RoleName = "新增" });
            var editRoleResult = viewModel.EditRole(new AuthorityRole { ID = 1, RoleName = "修改" });
            var addUserResult = viewModel.AddUser(new AuthorityUser { RID = 1, UserName = "bob", PW = "new-secret" });
            var editUserResult = viewModel.EditUser(new AuthorityUser { ID = 2, RID = 1, UserName = "alice", PW = "edit-secret" });

            AssertFailure(addRoleResult, "insert role failed");
            AssertFailure(editRoleResult, "update role failed");
            AssertFailure(addUserResult, "insert user failed");
            AssertFailure(editUserResult, "update user failed");
            Assert.Equal(4, messages.ErrorMessages.Count);
        }

        [Fact]
        public void LoadRoleData_ShowsErrorAndClearsCollections_WhenGetRolesThrows()
        {
            var repository = new FakeAuthorityRepository
            {
                ThrowOnGetRoles = new InvalidOperationException("get roles failed")
            };
            var messages = new FakeMessageDialogService();

            var exception = Record.Exception(() => CreateViewModel(repository, new FakeAuthorityDialogService(), messages));

            Assert.Null(exception);
            Assert.Equal("get roles failed", messages.ErrorMessages.Single());
        }

        [Fact]
        public void LoadUserData_ShowsErrorAndKeepsUsersEmpty_WhenGetUsersThrows()
        {
            var repository = new FakeAuthorityRepository
            {
                Roles = new[] { new AuthorityRole { ID = 1, RoleName = "管理员" } },
                ThrowOnGetUsers = new InvalidOperationException("get users failed")
            };
            var messages = new FakeMessageDialogService();

            var viewModel = CreateViewModel(repository, new FakeAuthorityDialogService(), messages);

            Assert.Equal(1, viewModel.SelectedRole?.ID);
            Assert.Empty(viewModel.Users);
            Assert.Equal("get users failed", messages.ErrorMessages.Single());
        }

        [Fact]
        public void CommandOperations_ShowErrorMessage_WhenRepositoryThrows()
        {
            var selectedRole = new AuthorityRole { ID = 1, RoleName = "管理员" };
            var selectedUser = new AuthorityUser { ID = 2, RID = 1, UserName = "alice", PW = "secret" };
            var repository = new FakeAuthorityRepository
            {
                Roles = new[] { selectedRole },
                Users = new[] { selectedUser },
                ThrowOnInsertRole = new InvalidOperationException("command insert role failed"),
                ThrowOnUpdateRole = new InvalidOperationException("command update role failed"),
                ThrowOnDeleteRole = new InvalidOperationException("command delete role failed"),
                ThrowOnInsertUser = new InvalidOperationException("command insert user failed"),
                ThrowOnUpdateUser = new InvalidOperationException("command update user failed"),
                ThrowOnDeleteUser = new InvalidOperationException("command delete user failed")
            };
            var messages = new FakeMessageDialogService();
            var dialog = new FakeAuthorityDialogService
            {
                RoleDialogResult = new AuthorityDialogResult<AuthorityRole>(true, new AuthorityRole { ID = 1, RoleName = "角色" }),
                UserDialogResult = new AuthorityDialogResult<AuthorityUser>(true, new AuthorityUser { ID = 2, UserName = "user", PW = "dialog-secret" })
            };
            var viewModel = CreateViewModel(repository, dialog, messages);
            viewModel.SelectedRole = selectedRole;
            viewModel.SelectedUser = selectedUser;

            viewModel.AddRoleCommand.Execute();
            viewModel.EditRoleCommand.Execute();
            viewModel.DeleteRoleCommand.Execute();
            viewModel.AddUserCommand.Execute();
            viewModel.EditUserCommand.Execute();
            viewModel.DeleteUserCommand.Execute();

            Assert.Equal(new[]
            {
                "command insert role failed",
                "command update role failed",
                "command delete role failed",
                "command insert user failed",
                "command update user failed",
                "command delete user failed"
            }, messages.ErrorMessages);
            Assert.DoesNotContain(messages.ErrorMessages, message => message.Contains("dialog-secret"));
        }

        [Fact]
        public void RoleDialogException_ShowsError_AndCommandDoesNotThrow()
        {
            var repository = new FakeAuthorityRepository
            {
                Roles = new[] { new AuthorityRole { ID = 1, RoleName = "管理员" } }
            };
            var messages = new FakeMessageDialogService();
            var dialog = new FakeAuthorityDialogService
            {
                ThrowOnShowRoleDialog = new InvalidOperationException("role dialog failed")
            };
            var viewModel = CreateViewModel(repository, dialog, messages);

            var addException = Record.Exception(() => viewModel.AddRoleCommand.Execute());
            viewModel.SelectedRole = repository.Roles.Single();
            var editException = Record.Exception(() => viewModel.EditRoleCommand.Execute());

            Assert.Null(addException);
            Assert.Null(editException);
            Assert.Equal(new[] { "role dialog failed", "role dialog failed" }, messages.ErrorMessages);
        }

        [Fact]
        public void UserDialogException_ShowsError_AndCommandDoesNotThrow()
        {
            var selectedRole = new AuthorityRole { ID = 1, RoleName = "管理员" };
            var selectedUser = new AuthorityUser { ID = 2, RID = 1, UserName = "alice", PW = "secret" };
            var repository = new FakeAuthorityRepository
            {
                Roles = new[] { selectedRole },
                Users = new[] { selectedUser }
            };
            var messages = new FakeMessageDialogService();
            var dialog = new FakeAuthorityDialogService
            {
                ThrowOnShowUserDialog = new InvalidOperationException("user dialog failed")
            };
            var viewModel = CreateViewModel(repository, dialog, messages);
            viewModel.SelectedRole = selectedRole;
            viewModel.SelectedUser = selectedUser;

            var addException = Record.Exception(() => viewModel.AddUserCommand.Execute());
            var editException = Record.Exception(() => viewModel.EditUserCommand.Execute());

            Assert.Null(addException);
            Assert.Null(editException);
            Assert.Equal(new[] { "user dialog failed", "user dialog failed" }, messages.ErrorMessages);
        }

        [Fact]
        public void ConfirmException_ShowsError_AndDeleteCommandsDoNotThrow()
        {
            var selectedRole = new AuthorityRole { ID = 1, RoleName = "管理员" };
            var selectedUser = new AuthorityUser { ID = 2, RID = 1, UserName = "alice", PW = "secret" };
            var repository = new FakeAuthorityRepository
            {
                Roles = new[] { selectedRole },
                Users = new[] { selectedUser }
            };
            var messages = new FakeMessageDialogService
            {
                ThrowOnConfirm = new InvalidOperationException("confirm failed")
            };
            var viewModel = CreateViewModel(repository, new FakeAuthorityDialogService(), messages);
            viewModel.SelectedRole = selectedRole;
            viewModel.SelectedUser = selectedUser;

            var deleteRoleException = Record.Exception(() => viewModel.DeleteRoleCommand.Execute());
            var deleteUserException = Record.Exception(() => viewModel.DeleteUserCommand.Execute());

            Assert.Null(deleteRoleException);
            Assert.Null(deleteUserException);
            Assert.Equal(new[] { "confirm failed", "confirm failed" }, messages.ErrorMessages);
        }

        [Fact]
        public void AddRoleCommand_ShowsAddRoleDialog_AndSavesOnlyWhenOk()
        {
            var repository = new FakeAuthorityRepository();
            var dialog = new FakeAuthorityDialogService
            {
                RoleDialogResult = new AuthorityDialogResult<AuthorityRole>(
                    true,
                    new AuthorityRole
                    {
                        RoleName = "工艺",
                        Remark = "remark",
                        SelectedPermissionTags = new[] { "1001", "1005" }
                    })
            };
            var viewModel = CreateViewModel(repository, dialog);

            viewModel.AddRoleCommand.Execute();

            Assert.Equal("角色【新增】", dialog.LastRoleTitle);
            Assert.Null(dialog.LastRoleModel);
            Assert.Equal("[\"1001\",\"1005\"]", repository.LastInsertedRole?.Powers);
            Assert.Equal("工艺", repository.LastInsertedRole?.RoleName);

            dialog.RoleDialogResult = new AuthorityDialogResult<AuthorityRole>(false, new AuthorityRole { RoleName = "取消" });
            viewModel.AddRoleCommand.Execute();

            Assert.Equal(1, repository.InsertRoleCallCount);
        }

        [Fact]
        public void EditRoleCommand_ShowsEditRoleDialogWithSelectedRole_AndSavesReturnedRole()
        {
            var selectedRole = new AuthorityRole
            {
                ID = 7,
                RoleName = "旧角色",
                Powers = "[\"1002\"]",
                Remark = "old"
            };
            var repository = new FakeAuthorityRepository { Roles = new[] { selectedRole } };
            var dialog = new FakeAuthorityDialogService
            {
                RoleDialogResult = new AuthorityDialogResult<AuthorityRole>(
                    true,
                    new AuthorityRole
                    {
                        ID = 7,
                        RoleName = "新角色",
                        Remark = "new",
                        SelectedPermissionTags = new[] { "1003", "1008" }
                    })
            };
            var viewModel = CreateViewModel(repository, dialog);
            viewModel.SelectedRole = selectedRole;

            viewModel.EditRoleCommand.Execute();

            Assert.Equal("角色【修改】", dialog.LastRoleTitle);
            Assert.Equal(7, dialog.LastRoleModel?.ID);
            Assert.Equal("旧角色", dialog.LastRoleModel?.RoleName);
            Assert.Equal("[\"1003\",\"1008\"]", repository.LastUpdatedRole?.Powers);
            Assert.Equal("新角色", repository.LastUpdatedRole?.RoleName);
        }

        [Fact]
        public void AddUserCommand_UsesSelectedRoleId_AndCancelDoesNotSave()
        {
            var selectedRole = new AuthorityRole { ID = 11, RoleName = "角色" };
            var repository = new FakeAuthorityRepository { Roles = new[] { selectedRole } };
            var dialog = new FakeAuthorityDialogService
            {
                UserDialogResult = new AuthorityDialogResult<AuthorityUser>(
                    true,
                    new AuthorityUser
                    {
                        UserName = "alice",
                        PW = "secret"
                    })
            };
            var viewModel = CreateViewModel(repository, dialog);
            viewModel.SelectedRole = selectedRole;

            viewModel.AddUserCommand.Execute();

            Assert.Equal("用户【新增】", dialog.LastUserTitle);
            Assert.Null(dialog.LastUserModel);
            Assert.Equal(11, repository.LastInsertedUser?.RID);
            Assert.Equal("alice", repository.LastInsertedUser?.UserName);
            Assert.Equal("secret", repository.LastInsertedUser?.PW);

            dialog.UserDialogResult = new AuthorityDialogResult<AuthorityUser>(false, new AuthorityUser { UserName = "cancel" });
            viewModel.AddUserCommand.Execute();

            Assert.Equal(1, repository.InsertUserCallCount);
        }

        [Fact]
        public void EditUserCommand_ShowsEditUserDialogWithSelectedUser_AndKeepsSelectedRoleId()
        {
            var selectedRole = new AuthorityRole { ID = 13, RoleName = "角色" };
            var selectedUser = new AuthorityUser { ID = 3, RID = 13, UserName = "old", PW = "old-pw" };
            var repository = new FakeAuthorityRepository
            {
                Roles = new[] { selectedRole },
                Users = new[] { selectedUser }
            };
            var dialog = new FakeAuthorityDialogService
            {
                UserDialogResult = new AuthorityDialogResult<AuthorityUser>(
                    true,
                    new AuthorityUser
                    {
                        ID = 3,
                        UserName = "new",
                        PW = "new-pw"
                    })
            };
            var viewModel = CreateViewModel(repository, dialog);
            viewModel.SelectedRole = selectedRole;
            viewModel.SelectedUser = selectedUser;

            viewModel.EditUserCommand.Execute();

            Assert.Equal("用户【修改】", dialog.LastUserTitle);
            Assert.Equal(3, dialog.LastUserModel?.ID);
            Assert.Equal("old", dialog.LastUserModel?.UserName);
            Assert.Equal(13, repository.LastUpdatedUser?.RID);
            Assert.Equal("new", repository.LastUpdatedUser?.UserName);
            Assert.Equal("new-pw", repository.LastUpdatedUser?.PW);
        }

        private static AuthorityViewModel CreateViewModel(
            FakeAuthorityRepository repository,
            FakeAuthorityDialogService dialog,
            FakeMessageDialogService? messages = null)
        {
            return new AuthorityViewModel(
                new AuthorityService(repository),
                messages ?? new FakeMessageDialogService(),
                dialog);
        }

        private static void AssertFailure(AuthorityOperationResult result, string message)
        {
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(message, result.Message);
        }

        private sealed class FakeAuthorityDialogService : IAuthorityDialogService
        {
            public AuthorityDialogResult<AuthorityRole> RoleDialogResult { get; set; } =
                new AuthorityDialogResult<AuthorityRole>(false, new AuthorityRole());

            public AuthorityDialogResult<AuthorityUser> UserDialogResult { get; set; } =
                new AuthorityDialogResult<AuthorityUser>(false, new AuthorityUser());

            public string? LastRoleTitle { get; private set; }

            public AuthorityRole? LastRoleModel { get; private set; }

            public string? LastUserTitle { get; private set; }

            public AuthorityUser? LastUserModel { get; private set; }

            public Exception? ThrowOnShowRoleDialog { get; set; }

            public Exception? ThrowOnShowUserDialog { get; set; }

            public AuthorityDialogResult<AuthorityRole> ShowRoleDialog(string title, AuthorityRole? role)
            {
                if (ThrowOnShowRoleDialog != null)
                {
                    throw ThrowOnShowRoleDialog;
                }

                LastRoleTitle = title;
                LastRoleModel = role;
                return RoleDialogResult;
            }

            public AuthorityDialogResult<AuthorityUser> ShowUserDialog(string title, AuthorityUser? user)
            {
                if (ThrowOnShowUserDialog != null)
                {
                    throw ThrowOnShowUserDialog;
                }

                LastUserTitle = title;
                LastUserModel = user;
                return UserDialogResult;
            }
        }

        private sealed class FakeAuthorityRepository : IAuthorityRepository
        {
            public IReadOnlyList<AuthorityRole> Roles { get; set; } = Array.Empty<AuthorityRole>();

            public IReadOnlyList<AuthorityUser> Users { get; set; } = Array.Empty<AuthorityUser>();

            public List<int> GetUsersRoleIds { get; } = new List<int>();

            public AuthorityRole? LastInsertedRole { get; private set; }

            public AuthorityRole? LastUpdatedRole { get; private set; }

            public AuthorityUser? LastInsertedUser { get; private set; }

            public AuthorityUser? LastUpdatedUser { get; private set; }

            public int InsertRoleCallCount { get; private set; }

            public int InsertUserCallCount { get; private set; }

            public Exception? ThrowOnInsertRole { get; set; }

            public Exception? ThrowOnGetRoles { get; set; }

            public Exception? ThrowOnGetUsers { get; set; }

            public Exception? ThrowOnUpdateRole { get; set; }

            public Exception? ThrowOnDeleteRole { get; set; }

            public Exception? ThrowOnInsertUser { get; set; }

            public Exception? ThrowOnUpdateUser { get; set; }

            public Exception? ThrowOnDeleteUser { get; set; }

            public IReadOnlyList<AuthorityRole> GetRoles()
            {
                if (ThrowOnGetRoles != null)
                {
                    throw ThrowOnGetRoles;
                }

                return Roles;
            }

            public IReadOnlyList<AuthorityUser> GetUsers(int roleId)
            {
                if (ThrowOnGetUsers != null)
                {
                    throw ThrowOnGetUsers;
                }

                GetUsersRoleIds.Add(roleId);
                return Users.Where(user => user.RID == roleId).ToList();
            }

            public int InsertRole(AuthorityRole role)
            {
                if (ThrowOnInsertRole != null)
                {
                    throw ThrowOnInsertRole;
                }

                InsertRoleCallCount++;
                LastInsertedRole = role;
                return 1;
            }

            public int UpdateRole(AuthorityRole role)
            {
                if (ThrowOnUpdateRole != null)
                {
                    throw ThrowOnUpdateRole;
                }

                LastUpdatedRole = role;
                return 1;
            }

            public int DeleteRole(int roleId)
            {
                if (ThrowOnDeleteRole != null)
                {
                    throw ThrowOnDeleteRole;
                }

                return 1;
            }

            public int InsertUser(AuthorityUser user)
            {
                if (ThrowOnInsertUser != null)
                {
                    throw ThrowOnInsertUser;
                }

                InsertUserCallCount++;
                LastInsertedUser = user;
                return 1;
            }

            public int UpdateUser(AuthorityUser user)
            {
                if (ThrowOnUpdateUser != null)
                {
                    throw ThrowOnUpdateUser;
                }

                LastUpdatedUser = user;
                return 1;
            }

            public int DeleteUser(int userId)
            {
                if (ThrowOnDeleteUser != null)
                {
                    throw ThrowOnDeleteUser;
                }

                return 1;
            }
        }

        private sealed class FakeMessageDialogService : IMessageDialogService
        {
            public List<string> ErrorMessages { get; } = new List<string>();

            public Exception? ThrowOnConfirm { get; set; }

            public bool Confirm(string message)
            {
                if (ThrowOnConfirm != null)
                {
                    throw ThrowOnConfirm;
                }

                return true;
            }

            public void ShowInfo(string message)
            {
            }

            public void ShowWarning(string message)
            {
            }

            public void ShowError(string message)
            {
                ErrorMessages.Add(message);
            }
        }
    }
}
