using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace ATS5.Tests.Infrastructure
{
    public sealed class AuthorityViewStructureTests
    {
        [Fact]
        public void AuthorityView_ContainsLegacyRoleAndUserTablesAndToolbars()
        {
            var viewPath = Path.Combine(GetRepositoryRoot(), "ATS5.Modules.Authority", "Views", "AuthorityView.xaml");
            var document = XDocument.Load(viewPath);
            var text = File.ReadAllText(viewPath);

            Assert.Contains("ItemsSource=\"{Binding Roles}\"", text);
            Assert.Contains("ItemsSource=\"{Binding Users}\"", text);
            Assert.Contains("SelectedItem=\"{Binding SelectedRole", text);
            Assert.Contains("Command=\"{Binding AddRoleCommand}\"", text);
            Assert.Contains("Command=\"{Binding EditRoleCommand}\"", text);
            Assert.Contains("Command=\"{Binding DeleteRoleCommand}\"", text);
            Assert.Contains("Command=\"{Binding AddUserCommand}\"", text);
            Assert.Contains("Command=\"{Binding EditUserCommand}\"", text);
            Assert.Contains("Command=\"{Binding DeleteUserCommand}\"", text);
            Assert.True(HasColumn(document, "角色名称", "RoleName"));
            Assert.True(HasColumn(document, "备注", "Remark"));
            Assert.True(HasColumn(document, "用户名", "UserName"));
            Assert.DoesNotContain("Header=\"ID\"", text);
            Assert.DoesNotContain("Header=\"RID\"", text);
            Assert.DoesNotContain("Header=\"PW\"", text);

            var buttons = document.Descendants()
                .Where(element => element.Name.LocalName == "Button")
                .Select(element => element.Attribute("Content")?.Value)
                .Where(value => value == "新增" || value == "修改" || value == "删除")
                .ToList();

            Assert.Equal(2, buttons.Count(value => value == "新增"));
            Assert.Equal(2, buttons.Count(value => value == "修改"));
            Assert.Equal(2, buttons.Count(value => value == "删除"));
        }

        [Fact]
        public void RoleAndUserDialogs_KeepLegacyFieldsAndPermissionItems()
        {
            var root = GetRepositoryRoot();
            var roleDialogText = File.ReadAllText(Path.Combine(root, "ATS5.Modules.Authority", "Views", "RoleDialog.xaml"));
            var userDialogText = File.ReadAllText(Path.Combine(root, "ATS5.Modules.Authority", "Views", "UserDialog.xaml"));

            Assert.Contains("Title=\"角色【新增】\"", roleDialogText);
            Assert.Contains("角色名称", roleDialogText);
            Assert.Contains("备注", roleDialogText);
            Assert.Contains("授权信息", roleDialogText);
            Assert.Contains("1001", roleDialogText);
            Assert.Contains("产品测试", roleDialogText);
            Assert.Contains("1009", roleDialogText);
            Assert.Contains("系统", roleDialogText);

            Assert.Contains("Title=\"用户【新增】\"", userDialogText);
            Assert.Contains("用户名", userDialogText);
            Assert.Contains("密码", userDialogText);
        }

        [Fact]
        public void AuthorityDialogService_AssignsOwnerBeforeShowingDialog()
        {
            var serviceText = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "ATS5.Modules.Authority", "Views", "AuthorityDialogService.cs"));

            Assert.Contains("ResolveOwner()", serviceText);
            Assert.Contains("dialog.Owner = owner", serviceText);
            Assert.Contains("Application.Current?.Windows", serviceText);
            Assert.Contains("Application.Current?.MainWindow", serviceText);
        }

        [Fact]
        public void AuthorityPermissionJson_UsesCentralSerializerOnly()
        {
            var root = GetRepositoryRoot();
            var serviceText = File.ReadAllText(Path.Combine(root, "ATS5.Application", "Authority", "AuthorityService.cs"));
            var viewModelText = File.ReadAllText(Path.Combine(root, "ATS5.Modules.Authority", "ViewModels", "AuthorityViewModel.cs"));
            var roleDialogCodeText = File.ReadAllText(Path.Combine(root, "ATS5.Modules.Authority", "Views", "RoleDialog.xaml.cs"));

            Assert.Contains("AuthorityPermissionSerializer.SerializeTags", serviceText);
            Assert.Contains("AuthorityPermissionSerializer.ParseTags", viewModelText);
            Assert.Contains("AuthorityPermissionSerializer.ParseTags", roleDialogCodeText);
            Assert.DoesNotContain("Split(new[] { ',' }", viewModelText);
            Assert.DoesNotContain("Split(new[] { ',' }", roleDialogCodeText);
            Assert.DoesNotContain("EscapeJsonString", serviceText);
        }

        private static bool HasColumn(XDocument document, string header, string bindingPath)
        {
            return document.Descendants()
                .Any(element =>
                    element.Name.LocalName == "DataGridTextColumn" &&
                    element.Attribute("Header")?.Value == header &&
                    (element.Attribute("Binding")?.Value ?? string.Empty).Contains(bindingPath));
        }

        private static string GetRepositoryRoot()
        {
            return Path.GetFullPath(Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", ".."));
        }
    }
}
