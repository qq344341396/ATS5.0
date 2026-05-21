using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace ATS5.Tests.Infrastructure
{
    public sealed class ShellNavigationStateTests
    {
        private static readonly XNamespace PresentationNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        [Fact]
        public void ShellNavigation_KeepsUnmigratedEntriesDisabled()
        {
            var buttons = LoadNavigationButtons();

            AssertEnabledCommand(buttons, "NavProductTest", "产品测试", "NavigateProductTestCommand");
            AssertEnabledCommand(buttons, "NavDataQuery", "数据查询", "NavigateDataQueryCommand");
            AssertEnabledCommand(buttons, "NavFlow", "流程管理", "NavigateFlowCommand");
            AssertEnabledCommand(buttons, "NavDevice", "设备管理", "NavigateDeviceCommand");
            AssertEnabledCommand(buttons, "NavAuthority", "权限管理", "NavigateAuthorityCommand");
            AssertDisabled(buttons, "NavMes", "MES");
            AssertDisabled(buttons, "NavAutoTest", "自动化");
            AssertCollapsed(buttons, "NavStatisticalInfo", "统计信息");
        }

        private static IReadOnlyDictionary<string, XElement> LoadNavigationButtons()
        {
            var shellPath = Path.Combine(GetRepositoryRoot(), "ATS5.Wpf", "Views", "Shell.xaml");
            var document = XDocument.Load(shellPath);
            return document
                .Descendants(PresentationNamespace + "Button")
                .Where(button => GetAutomationId(button) != null)
                .ToDictionary(
                    button => GetAutomationId(button) ?? string.Empty,
                    button => button);
        }

        private static string? GetAutomationId(XElement button)
        {
            return button
                .Attributes()
                .FirstOrDefault(attribute => attribute.Name.LocalName == "AutomationProperties.AutomationId")
                ?.Value;
        }

        private static void AssertDisabled(IReadOnlyDictionary<string, XElement> buttons, string automationId, string content)
        {
            var button = GetButton(buttons, automationId, content);

            Assert.Equal("False", button.Attribute("IsEnabled")?.Value);
            Assert.Null(button.Attribute("Command"));
        }

        private static void AssertEnabledCommand(
            IReadOnlyDictionary<string, XElement> buttons,
            string automationId,
            string content,
            string commandName)
        {
            var button = GetButton(buttons, automationId, content);

            Assert.Null(button.Attribute("IsEnabled"));
            Assert.Contains(commandName, button.Attribute("Command")?.Value ?? string.Empty);
        }

        private static void AssertCollapsed(IReadOnlyDictionary<string, XElement> buttons, string automationId, string content)
        {
            var button = GetButton(buttons, automationId, content);

            Assert.Equal("Collapsed", button.Attribute("Visibility")?.Value);
        }

        private static XElement GetButton(IReadOnlyDictionary<string, XElement> buttons, string automationId, string content)
        {
            Assert.True(buttons.TryGetValue(automationId, out var button), $"Missing navigation button AutomationId: {automationId}");
            Assert.Equal(content, button.Attribute("Content")?.Value);
            return button;
        }

        private static string GetRepositoryRoot()
        {
            return Path.GetFullPath(Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", ".."));
        }
    }
}
