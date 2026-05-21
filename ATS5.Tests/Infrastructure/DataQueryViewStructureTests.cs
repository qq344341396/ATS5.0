using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Xunit;

namespace ATS5.Tests.Infrastructure
{
    public sealed class DataQueryViewStructureTests
    {
        [Fact]
        public void DataQueryView_ContainsLegacyIndexDetailStatisticsAndMesUploadEntrances()
        {
            var dataQueryViewPath = GetDataQueryViewPath();
            var document = XDocument.Load(dataQueryViewPath);
            var text = File.ReadAllText(dataQueryViewPath);

            Assert.Contains("ItemsSource=\"{Binding Records}\"", text);
            Assert.Contains("ItemsSource=\"{Binding Details}\"", text);
            Assert.Contains("统计图区域", text);
            Assert.Contains("MES上传", text);
            Assert.Contains("上传MES状态", text);
            Assert.Contains("Command=\"{Binding ManualMesUploadCommand}\"", text);
            Assert.Contains("SelectionChanged=\"RecordsGrid_OnSelectionChanged\"", text);
            Assert.True(HasDisabledButton(document, "日志导出"));
            Assert.True(HasDisabledButton(document, "数据导出"));
            Assert.True(HasDisabledButton(document, "全部展开"));
            Assert.True(HasDisabledButton(document, "全部收合"));
            Assert.Contains("Text=\"PageSize: 200\"", text);

            var dataGridCount = document.Descendants().Count(element => element.Name.LocalName == "DataGrid");
            Assert.True(dataGridCount >= 2, "数据查询页面必须保留左索引、右明细两个表格。");
        }

        [Fact]
        public void DataQueryView_CodeBehindOnlySynchronizesSelectedRowsForManualMesUpload()
        {
            var codeBehindText = File.ReadAllText(GetDataQueryViewCodeBehindPath());

            Assert.Contains("RecordsGrid_OnSelectionChanged", codeBehindText);
            Assert.Contains("SelectedUploadRecords.Clear()", codeBehindText);
            Assert.Contains("SelectedUploadRecords.Add(record)", codeBehindText);
        }

        private static bool HasDisabledButton(XDocument document, string content)
        {
            return document.Descendants()
                .Where(element => element.Name.LocalName == "Button")
                .Any(element =>
                    element.Attribute("Content")?.Value == content &&
                    element.Attribute("IsEnabled")?.Value == "False");
        }

        private static string GetDataQueryViewPath()
        {
            var roots = new[]
            {
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                System.AppContext.BaseDirectory,
                Directory.GetCurrentDirectory()
            };

            foreach (var root in roots)
            {
                var path = FindDataQueryViewPath(root);
                if (!string.IsNullOrEmpty(path))
                {
                    return path!;
                }
            }

            throw new FileNotFoundException("无法定位 DataQueryView.xaml。");
        }

        private static string GetDataQueryViewCodeBehindPath()
        {
            var viewPath = GetDataQueryViewPath();
            var codeBehindPath = viewPath + ".cs";
            if (!File.Exists(codeBehindPath))
            {
                throw new FileNotFoundException("无法定位 DataQueryView.xaml.cs。", codeBehindPath);
            }

            return codeBehindPath;
        }

        private static string? FindDataQueryViewPath(string? directory)
        {
            while (!string.IsNullOrEmpty(directory))
            {
                var currentDirectory = directory;
                var solutionPath = Path.Combine(currentDirectory, "ATS5.Wpf.sln");
                if (File.Exists(solutionPath))
                {
                    var viewPath = Path.Combine(currentDirectory, "ATS5.Modules.DataQuery", "Views", "DataQueryView.xaml");
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
