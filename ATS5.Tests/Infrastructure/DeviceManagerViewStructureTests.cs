using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Xunit;

namespace ATS5.Tests.Infrastructure
{
    public sealed class DeviceManagerViewStructureTests
    {
        [Fact]
        public void DeviceManagerView_ContainsLegacyThreePanelToolbarAndGridColumns()
        {
            var viewPath = GetDeviceManagerViewPath();
            var document = XDocument.Load(viewPath);
            var text = File.ReadAllText(viewPath);

            Assert.Contains("Text=\"设备库\"", text);
            Assert.Contains("Text=\"{Binding SearchText", text);
            Assert.Contains("Placeholder=\"请输入检索内容\"", text);
            Assert.Contains("TreeView", text);
            Assert.Contains("HierarchicalDataTemplate", text);
            Assert.Contains("DeviceLibraryNodes", text);
            Assert.Contains("Property=\"IsExpanded\"", text);
            Assert.Contains("Value=\"True\"", text);
            Assert.Contains("Text=\"设备配置信息\"", text);
            Assert.Contains("Text=\"设备初始化参数说明\"", text);
            Assert.Contains("MouseDoubleClick=\"DeviceLibrary_MouseDoubleClick\"", text);
            Assert.DoesNotContain("MouseAction=\"LeftDoubleClick\"", text);
            Assert.Contains("Content=\"新建\"", text);
            Assert.Contains("Content=\"编辑\"", text);
            Assert.Contains("Content=\"打开\"", text);
            Assert.Contains("Content=\"保存\"", text);
            Assert.Contains("Content=\"另存为\"", text);
            Assert.Contains("Content=\"取消\"", text);
            Assert.Contains("Content=\"配置设备调试\"", text);
            Assert.Contains("Content=\"清除全局参数\"", text);

            var headers = document.Descendants()
                .Where(element => element.Name.LocalName.EndsWith("Column"))
                .Select(element => element.Attribute("Header")?.Value)
                .Where(header => !string.IsNullOrEmpty(header))
                .ToList();

            Assert.Contains("启用", headers);
            Assert.Contains("设备类型", headers);
            Assert.Contains("设备名称", headers);
            Assert.Contains("底层类名", headers);
            Assert.Contains("设备编码", headers);
            Assert.Contains("初始化参数", headers);
            Assert.Contains("备注", headers);
            Assert.Contains("全局参数", headers);
            AssertDisabledButton(document, "配置设备调试");
            AssertDisabledButton(document, "清除全局参数");
        }

        [Fact]
        public void DeviceConfigDialogService_UsesInteractiveLegacyDialogText()
        {
            var servicePath = GetDeviceConfigDialogServicePath();
            var text = File.ReadAllText(servicePath);

            Assert.DoesNotContain("return configNames.FirstOrDefault()?.Name", text);
            Assert.DoesNotContain("return configNames.FirstOrDefault()?.Name;", text);
            Assert.DoesNotContain(".Trim()", text);
            Assert.Contains("设备配置清单", text);
            Assert.Contains("FileName", text);
            Assert.Contains("设备配置类别", text);
            Assert.Contains("设备配置类别不能为空", text);
            Assert.Contains("确定要覆盖原文件？", text);
            Assert.Contains("AssignOwner(dialog)", text);
        }

        private static void AssertDisabledButton(XDocument document, string content)
        {
            var button = document.Descendants()
                .Where(element => element.Name.LocalName == "Button")
                .FirstOrDefault(element => element.Attribute("Content")?.Value == content);

            Assert.NotNull(button);
            Assert.Equal("False", button!.Attribute("IsEnabled")?.Value);
        }

        private static string GetDeviceManagerViewPath()
        {
            return GetDeviceModuleFilePath("DeviceManagerView.xaml");
        }

        private static string GetDeviceConfigDialogServicePath()
        {
            return GetDeviceModuleFilePath("DeviceConfigDialogService.cs");
        }

        private static string GetDeviceModuleFilePath(string fileName)
        {
            var roots = new[]
           {
               Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
               System.AppContext.BaseDirectory,
               Directory.GetCurrentDirectory()
           };

            foreach (var root in roots)
            {
                var path = FindDeviceModuleFilePath(root, fileName);
                if (!string.IsNullOrEmpty(path))
                {
                    return path!;
                }
            }

            throw new FileNotFoundException($"无法定位 {fileName}。");
        }

        private static string? FindDeviceModuleFilePath(string? directory, string fileName)
        {
            while (!string.IsNullOrEmpty(directory))
            {
                var currentDirectory = directory;
                var solutionPath = Path.Combine(currentDirectory, "ATS5.Wpf.sln");
                if (File.Exists(solutionPath))
                {
                    var viewPath = Path.Combine(currentDirectory, "ATS5.Modules.Device", "Views", fileName);
                    if (File.Exists(viewPath))
                    {
                        return viewPath;
                    }
                }

                directory = Directory.GetParent(currentDirectory)?.FullName;
            }

            return null;
        }
    }
}
