using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace ATS5.Tests.Infrastructure
{
    public sealed class WpfSmokeTests
    {
        private const string DataQueryViewName = "DataQueryView";
        private const string FlowEditorViewName = "FlowEditorView";
        private const string AuthorityViewName = "AuthorityView";
        private const string DeviceManagerViewName = "DeviceManagerView";

        [Fact]
        public void Shell_DefinesStableSampleNavigationAutomationIdsAndContentRegion()
        {
            var shellPath = GetRepositoryPath("ATS5.Wpf", "Views", "Shell.xaml");
            var document = XDocument.Load(shellPath);
            var text = File.ReadAllText(shellPath);

            Assert.Contains("AutomationProperties.AutomationId=\"NavDataQuery\"", text);
            Assert.Contains("AutomationProperties.AutomationId=\"NavFlow\"", text);
            Assert.Contains("AutomationProperties.AutomationId=\"NavProductTest\"", text);
            Assert.Contains("AutomationProperties.AutomationId=\"NavDevice\"", text);
            Assert.Contains("AutomationProperties.AutomationId=\"NavAuthority\"", text);
            Assert.Contains("AutomationProperties.AutomationId=\"NavMes\"", text);
            Assert.Contains("AutomationProperties.AutomationId=\"NavAutoTest\"", text);
            Assert.Contains("AutomationProperties.AutomationId=\"NavStatisticalInfo\"", text);
            Assert.Contains("prism:RegionManager.RegionName=\"{x:Static core:RegionNames.ContentRegion}\"", text);

            Assert.Equal("NavigateProductTestCommand", GetButtonBinding(document, "NavProductTest", "Command"));
            Assert.Equal("NavigateDataQueryCommand", GetButtonBinding(document, "NavDataQuery", "Command"));
            Assert.Equal("NavigateFlowCommand", GetButtonBinding(document, "NavFlow", "Command"));
            Assert.Equal("NavigateDeviceCommand", GetButtonBinding(document, "NavDevice", "Command"));
            Assert.Equal("NavigateAuthorityCommand", GetButtonBinding(document, "NavAuthority", "Command"));
            AssertUnmigratedButtonIsDisabledOrHidden(document, "NavMes");
            AssertUnmigratedButtonIsDisabledOrHidden(document, "NavAutoTest");
            AssertUnmigratedButtonIsDisabledOrHidden(document, "NavStatisticalInfo");
        }

        [Fact]
        public void ShellViewModel_NavigationCommandsRequestSampleViewsInContentRegion()
        {
            var shellViewModelPath = GetRepositoryPath("ATS5.Wpf", "ViewModels", "ShellViewModel.cs");
            var text = File.ReadAllText(shellViewModelPath);

            Assert.Contains("NavigateDataQueryCommand = new DelegateCommand(NavigateDataQuery);", text);
            Assert.Contains("NavigateProductTestCommand = new DelegateCommand(NavigateProductTest);", text);
            Assert.Contains("NavigateFlowCommand = new DelegateCommand(NavigateFlow);", text);
            Assert.Contains("NavigateDeviceCommand = new DelegateCommand(NavigateDevice);", text);
            Assert.Contains("NavigateAuthorityCommand = new DelegateCommand(NavigateAuthority);", text);
            Assert.Contains("_regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(DataQueryView));", text);
            Assert.Contains("_regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(ProductTestShellView));", text);
            Assert.Contains("_regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(FlowEditorView));", text);
            Assert.Contains("_regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(DeviceManagerView));", text);
            Assert.Contains("_regionManager.RequestNavigate(RegionNames.ContentRegion, nameof(AuthorityView));", text);
            Assert.Contains(DataQueryViewName, text);
            Assert.Contains("ProductTestShellView", text);
            Assert.Contains(FlowEditorViewName, text);
            Assert.Contains(DeviceManagerViewName, text);
            Assert.Contains(AuthorityViewName, text);
        }

        [Fact]
        public void WpfProject_DefinesExpectedDebugExecutableLocationForUiSmokeProbe()
        {
            var projectPath = GetRepositoryPath("ATS5.Wpf", "ATS5.Wpf.csproj");
            var document = XDocument.Load(projectPath);
            var outputType = document.Descendants()
                .FirstOrDefault(element => element.Name.LocalName == "OutputType")
                ?.Value;
            var targetFramework = document.Descendants()
                .FirstOrDefault(element => element.Name.LocalName == "TargetFramework")
                ?.Value;
            var useWpf = document.Descendants()
                .FirstOrDefault(element => element.Name.LocalName == "UseWPF")
                ?.Value;
            var expectedExecutablePath = GetRepositoryPath("ATS5.Wpf", "bin", "Debug", "net461", "ATS5.Wpf.exe");

            Assert.Equal("WinExe", outputType);
            Assert.Equal("net461", targetFramework);
            Assert.Equal("true", useWpf);
            Assert.EndsWith(@"ATS5.Wpf\bin\Debug\net461\ATS5.Wpf.exe", expectedExecutablePath);
        }

        private static string? GetButtonBinding(XDocument document, string automationId, string attributeName)
        {
            var value = GetButton(document, automationId)?.Attribute(attributeName)?.Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value!
                .Replace("{Binding ", string.Empty)
                .Replace("}", string.Empty)
                .Trim();
        }

        private static void AssertUnmigratedButtonIsDisabledOrHidden(XDocument document, string automationId)
        {
            var button = GetButton(document, automationId);

            Assert.True(button != null, $"Missing navigation button AutomationId: {automationId}");
            var isDisabled = button!.Attribute("IsEnabled")?.Value == "False";
            var isHidden = button.Attribute("Visibility")?.Value == "Collapsed" ||
                button.Attribute("Visibility")?.Value == "Hidden";
            Assert.True(isDisabled || isHidden, $"{automationId} must remain disabled or hidden until migrated.");
            Assert.Null(button.Attribute("Command"));
        }

        private static XElement? GetButton(XDocument document, string automationId)
        {
            return document.Descendants()
                .Where(element => element.Name.LocalName == "Button")
                .FirstOrDefault(element => element.Attributes().Any(attribute =>
                    attribute.Name.LocalName == "AutomationProperties.AutomationId" &&
                    attribute.Value == automationId));
        }

        private static string GetRepositoryPath(params string[] segments)
        {
            return Path.Combine(GetRepositoryRoot(), Path.Combine(segments));
        }

        private static string GetRepositoryRoot()
        {
            return Path.GetFullPath(Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", ".."));
        }
    }
}
