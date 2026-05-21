using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Xunit;

namespace ATS5.Tests.Infrastructure
{
    public sealed class FlowEditorViewStructureTests
    {
        [Fact]
        public void FlowEditorView_ContainsLegacyToolbarProjectScriptDeviceAndBottomTabs()
        {
            var viewPath = GetFlowEditorViewPath();
            var document = XDocument.Load(viewPath);
            var text = File.ReadAllText(viewPath);

            Assert.Contains("Content=\"打开\"", text);
            Assert.Contains("Command=\"{Binding OpenCommand}\"", text);
            Assert.Contains("Content=\"保存\"", text);
            Assert.Contains("Command=\"{Binding SaveCommand}\"", text);
            Assert.Contains("Text=\"测试项目\"", text);
            Assert.Contains("ItemsSource=\"{Binding Projects}\"", text);
            Assert.Contains("Header=\"项目脚本\"", text);
            Assert.Contains("Text=\"{Binding SelectedProject.Script, UpdateSourceTrigger=PropertyChanged}\"", text);
            Assert.Contains("Header=\"输出项\"", text);
            Assert.Contains("ItemsSource=\"{Binding SelectedProject.Outputs}\"", text);
            Assert.Contains("Header=\"临时变量\"", text);
            Assert.Contains("ItemsSource=\"{Binding SelectedProject.TempVariables}\"", text);
            Assert.Contains("Header=\"设备控制\"", text);
            Assert.Contains("Text=\"已关联设备配置：\"", text);
            Assert.Contains("Text=\"{Binding DevConfigName, UpdateSourceTrigger=PropertyChanged}\"", text);
            Assert.Contains("Header=\"DBC信号\"", text);
            Assert.Contains("Header=\"UDS信号\"", text);
            Assert.Contains("Header=\"公共变量\"", text);

            var tabHeaders = document.Descendants()
                .Where(element => element.Name.LocalName == "TabItem")
                .Select(element => element.Attribute("Header")?.Value)
                .Where(header => !string.IsNullOrEmpty(header))
                .ToList();

            Assert.Contains("项目脚本", tabHeaders);
            Assert.Contains("输出项", tabHeaders);
            Assert.Contains("临时变量", tabHeaders);
            Assert.Contains("设备控制", tabHeaders);
            Assert.Contains("DBC信号", tabHeaders);
            Assert.Contains("UDS信号", tabHeaders);
            Assert.Contains("公共变量", tabHeaders);
        }

        [Fact]
        public void FlowEditorView_KeepsUnfinishedButtonsDisabled()
        {
            var document = XDocument.Load(GetFlowEditorViewPath());
            var disabledContents = new[]
            {
                "新建",
                "编辑",
                "导出",
                "导入",
                "另存为",
                "取消",
                "编辑公共变量",
                "修改流程名称",
                "格式化",
                "导入设备",
                "新增",
                "删除",
                "上移",
                "下移",
                "复制"
            };

            foreach (var content in disabledContents)
            {
                var minimumCount = IsOutputAndTemporaryVariableButton(content) ? 2 : 1;
                AssertDisabledButtons(document, content, minimumCount);
            }

            AssertDisabledButtons(document, "+", 1);
            AssertDisabledButtons(document, "E", 1);
            AssertDisabledButtons(document, "-", 1);
            AssertDisabledButtons(document, "^", 1);
            AssertDisabledButtons(document, "v", 1);
            AssertDisabledButtons(document, "ABC", 1);
            AssertDisabledButtons(document, "[]", 1);
            AssertDisabledButtons(document, ">", 1);
            AssertDisabledButtons(document, "<", 1);
            AssertDisabledButtons(document, "R", 1);
        }

        [Fact]
        public void FlowEditorView_KeepsOutputAndTemporaryVariableGridsReadOnly()
        {
            var document = XDocument.Load(GetFlowEditorViewPath());

            Assert.True(StyleHasSetter(document, "GridStyle", "IsReadOnly", "True"), "输出项和临时变量共享表格样式必须设置 IsReadOnly=True。");
            Assert.True(HasReadOnlyGridBinding(document, "SelectedProject.Outputs"), "输出项表格必须保持只读，防止改路径启用。");
            Assert.True(HasReadOnlyGridBinding(document, "SelectedProject.TempVariables"), "临时变量表格必须保持只读，防止改路径启用。");
        }

        [Fact]
        public void FlowEditorView_KeepsProjectEnabledCheckBoxReadOnly()
        {
            var document = XDocument.Load(GetFlowEditorViewPath());

            Assert.True(HasDisabledProjectEnabledCheckBox(document), "项目启用 CheckBox 必须保持禁用，只允许查看。");
        }

        private static bool HasDisabledProjectEnabledCheckBox(XDocument document)
        {
            return document.Descendants()
                .Where(element => element.Name.LocalName == "CheckBox")
                .Any(element =>
                    element.Attribute("IsChecked")?.Value == "{Binding IsEnable}" &&
                    element.Attribute("IsEnabled")?.Value == "False");
        }

        private static bool IsOutputAndTemporaryVariableButton(string content)
        {
            return content == "新增" ||
                content == "删除" ||
                content == "上移" ||
                content == "下移" ||
                content == "复制";
        }

        private static void AssertDisabledButtons(XDocument document, string content, int minimumCount)
        {
            var buttons = document.Descendants()
                .Where(element => element.Name.LocalName == "Button")
                .Where(element => element.Attribute("Content")?.Value == content)
                .ToList();

            Assert.True(buttons.Count >= minimumCount, $"{content} 按钮数量不足，至少应有 {minimumCount} 个。");
            Assert.All(buttons, button => Assert.Equal("False", button.Attribute("IsEnabled")?.Value));
        }

        private static bool HasReadOnlyGridBinding(XDocument document, string binding)
        {
            return document.Descendants()
                .Where(element => element.Name.LocalName == "DataGrid")
                .Any(element =>
                    element.Attribute("ItemsSource")?.Value == $"{{Binding {binding}}}" &&
                    (element.Attribute("IsReadOnly")?.Value == "True" ||
                        element.Attribute("Style")?.Value == "{StaticResource GridStyle}"));
        }

        private static bool StyleHasSetter(XDocument document, string styleKey, string propertyName, string value)
        {
            return document.Descendants()
                .Where(element => element.Name.LocalName == "Style")
                .Where(element => element.Attributes().Any(attribute => attribute.Name.LocalName == "Key" && attribute.Value == styleKey))
                .Descendants()
                .Any(element =>
                    element.Name.LocalName == "Setter" &&
                    element.Attribute("Property")?.Value == propertyName &&
                    element.Attribute("Value")?.Value == value);
        }

        private static string GetFlowEditorViewPath()
        {
            var roots = new[]
            {
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                System.AppContext.BaseDirectory,
                Directory.GetCurrentDirectory()
            };

            foreach (var root in roots)
            {
                var path = FindFlowEditorViewPath(root);
                if (!string.IsNullOrEmpty(path))
                {
                    return path!;
                }
            }

            throw new FileNotFoundException("无法定位 FlowEditorView.xaml。");
        }

        private static string? FindFlowEditorViewPath(string? directory)
        {
            while (!string.IsNullOrEmpty(directory))
            {
                var currentDirectory = directory;
                var solutionPath = Path.Combine(currentDirectory, "ATS5.Wpf.sln");
                if (File.Exists(solutionPath))
                {
                    var viewPath = Path.Combine(currentDirectory, "ATS5.Modules.Flow", "Views", "FlowEditorView.xaml");
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
