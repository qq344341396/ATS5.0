using System;
using System.Collections.ObjectModel;
using System.Linq;
using ATS5.Application.Authority;
using ATS5.Wpf.Core;
using Prism.Commands;
using Prism.Mvvm;

namespace ATS5.Modules.Authority.ViewModels
{
    public sealed class AuthorityViewModel : BindableBase
    {
        private readonly AuthorityService _authorityService;
        private readonly IMessageDialogService _messageDialogService;
        private readonly IAuthorityDialogService _authorityDialogService;
        private AuthorityRole? _selectedRole;
        private AuthorityUser? _selectedUser;
        private string _message = string.Empty;

        public AuthorityViewModel(
            AuthorityService authorityService,
            IMessageDialogService messageDialogService,
            IAuthorityDialogService authorityDialogService)
        {
            _authorityService = authorityService ?? throw new ArgumentNullException(nameof(authorityService));
            _messageDialogService = messageDialogService ?? throw new ArgumentNullException(nameof(messageDialogService));
            _authorityDialogService = authorityDialogService ?? throw new ArgumentNullException(nameof(authorityDialogService));
            AddRoleCommand = new DelegateCommand(AddRole);
            EditRoleCommand = new DelegateCommand(EditRole, () => SelectedRole != null).ObservesProperty(() => SelectedRole);
            DeleteRoleCommand = new DelegateCommand(DeleteRole, () => SelectedRole != null).ObservesProperty(() => SelectedRole);
            AddUserCommand = new DelegateCommand(AddUser, () => SelectedRole != null).ObservesProperty(() => SelectedRole);
            EditUserCommand = new DelegateCommand(EditUser, () => SelectedUser != null).ObservesProperty(() => SelectedUser);
            DeleteUserCommand = new DelegateCommand(DeleteUser, () => SelectedUser != null).ObservesProperty(() => SelectedUser);
            LoadRoleData();
        }

        public ObservableCollection<AuthorityRole> Roles { get; } = new ObservableCollection<AuthorityRole>();

        public ObservableCollection<AuthorityUser> Users { get; } = new ObservableCollection<AuthorityUser>();

        public ObservableCollection<PermissionItemViewModel> PermissionItems { get; } = new ObservableCollection<PermissionItemViewModel>();

        public AuthorityRole? SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (SetProperty(ref _selectedRole, value))
                {
                    LoadUserData();
                    RefreshPermissionItems();
                }
            }
        }

        public AuthorityUser? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public DelegateCommand AddRoleCommand { get; }

        public DelegateCommand EditRoleCommand { get; }

        public DelegateCommand DeleteRoleCommand { get; }

        public DelegateCommand AddUserCommand { get; }

        public DelegateCommand EditUserCommand { get; }

        public DelegateCommand DeleteUserCommand { get; }

        public void LoadRoleData()
        {
            var previousRoleId = SelectedRole?.ID;
            Roles.Clear();
            foreach (var role in ExecuteQuery(() => _authorityService.GetRoles()))
            {
                Roles.Add(role);
            }

            SelectedRole = Roles.FirstOrDefault(role => role.ID == previousRoleId) ?? Roles.FirstOrDefault();
            if (SelectedRole == null)
            {
                Users.Clear();
                SelectedUser = null;
                RefreshPermissionItems();
            }
        }

        public AuthorityOperationResult AddRole(AuthorityRole role)
        {
            return ExecuteResultOperation(
                () =>
                {
                    var result = _authorityService.AddRole(role);
                    ApplyRoleResult(result);
                    return result;
                });
        }

        public AuthorityOperationResult EditRole(AuthorityRole role)
        {
            return ExecuteResultOperation(
                () =>
                {
                    var result = _authorityService.UpdateRole(role);
                    ApplyRoleResult(result);
                    return result;
                });
        }

        public AuthorityOperationResult AddUser(AuthorityUser user)
        {
            return ExecuteResultOperation(
                () =>
                {
                    var result = _authorityService.AddUser(user);
                    ApplyUserResult(result);
                    return result;
                });
        }

        public AuthorityOperationResult EditUser(AuthorityUser user)
        {
            return ExecuteResultOperation(
                () =>
                {
                    var result = _authorityService.UpdateUser(user);
                    ApplyUserResult(result);
                    return result;
                });
        }

        private void AddRole()
        {
            ExecuteCommandOperation(
                () =>
                {
                    var result = _authorityDialogService.ShowRoleDialog("角色【新增】", null);
                    if (result.IsOk)
                    {
                        AddRole(result.Value);
                    }
                });
        }

        private void EditRole()
        {
            ExecuteCommandOperation(
                () =>
                {
                    if (SelectedRole != null)
                    {
                        var result = _authorityDialogService.ShowRoleDialog("角色【修改】", SelectedRole.Clone());
                        if (result.IsOk)
                        {
                            EditRole(result.Value);
                        }
                    }
                });
        }

        private void DeleteRole()
        {
            ExecuteCommandOperation(
                () =>
                {
                    if (SelectedRole == null)
                    {
                        return;
                    }

                    var isConfirmed = _messageDialogService.Confirm("确定要删除该角色吗？");
                    ExecuteResultOperation(
                        () =>
                        {
                            var result = _authorityService.DeleteRole(SelectedRole.ID, isConfirmed);
                            ApplyRoleResult(result);
                            return result;
                        });
                });
        }

        private void AddUser()
        {
            ExecuteCommandOperation(
                () =>
                {
                    if (SelectedRole != null)
                    {
                        var result = _authorityDialogService.ShowUserDialog("用户【新增】", null);
                        if (result.IsOk)
                        {
                            result.Value.RID = SelectedRole.ID;
                            AddUser(result.Value);
                        }
                    }
                });
        }

        private void EditUser()
        {
            ExecuteCommandOperation(
                () =>
                {
                    if (SelectedUser != null)
                    {
                        var result = _authorityDialogService.ShowUserDialog("用户【修改】", SelectedUser.Clone());
                        if (result.IsOk)
                        {
                            result.Value.RID = SelectedRole?.ID ?? SelectedUser.RID;
                            EditUser(result.Value);
                        }
                    }
                });
        }

        private void DeleteUser()
        {
            ExecuteCommandOperation(
                () =>
                {
                    if (SelectedUser == null)
                    {
                        return;
                    }

                    var isConfirmed = _messageDialogService.Confirm("确定要删除该用户吗？");
                    ExecuteResultOperation(
                        () =>
                        {
                            var result = _authorityService.DeleteUser(SelectedUser.ID, isConfirmed);
                            ApplyUserResult(result);
                            return result;
                        });
                });
        }

        private void LoadUserData()
        {
            Users.Clear();
            SelectedUser = null;
            if (SelectedRole == null)
            {
                return;
            }

            foreach (var user in ExecuteQuery(() => _authorityService.GetUsers(SelectedRole.ID)))
            {
                Users.Add(user);
            }
        }

        private void ApplyRoleResult(AuthorityOperationResult result)
        {
            ApplyMessage(result);
            if (result.IsSuccess)
            {
                LoadRoleData();
            }
        }

        private void ApplyUserResult(AuthorityOperationResult result)
        {
            ApplyMessage(result);
            if (result.IsSuccess)
            {
                LoadUserData();
            }
        }

        private void ApplyMessage(AuthorityOperationResult result)
        {
            Message = result.Message;
            if (!string.IsNullOrEmpty(result.Message))
            {
                _messageDialogService.ShowWarning(result.Message);
            }
        }

        private void RefreshPermissionItems()
        {
            PermissionItems.Clear();
            var selectedTags = AuthorityPermissionSerializer.ParseTags(SelectedRole?.Powers);
            foreach (var item in AuthorityService.GetPermissionItems())
            {
                PermissionItems.Add(new PermissionItemViewModel(item)
                {
                    IsChecked = selectedTags.Contains(item.Tag)
                });
            }
        }

        private AuthorityOperationResult ExecuteResultOperation(Func<AuthorityOperationResult> action)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                ShowLegacyError(ex);
                return AuthorityOperationResult.Failed(ex.Message);
            }
        }

        private System.Collections.Generic.IReadOnlyList<T> ExecuteQuery<T>(Func<System.Collections.Generic.IReadOnlyList<T>> action)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                ShowLegacyError(ex);
                return Array.Empty<T>();
            }
        }

        private void ExecuteCommandOperation(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                ShowLegacyError(ex);
            }
        }

        private void ShowLegacyError(Exception exception)
        {
            Message = exception.Message;
            _messageDialogService.ShowError(exception.Message);
        }
    }
}
