using System.IO;
using Xunit;

namespace ATS5.Tests.Infrastructure
{
    public sealed class ProductTestShellViewStructureTests
    {
        [Fact]
        public void ProductTestShellView_ContainsLegacyShellToolbarAndPanels()
        {
            var viewPath = GetRepositoryPath("ATS5.Modules.ProductTest", "Views", "ProductTestShellView.xaml");
            var text = File.ReadAllText(viewPath);

            Assert.Contains("Content=\"执行\"", text);
            Assert.Contains("Content=\"暂停\"", text);
            Assert.Contains("Content=\"停止\"", text);
            Assert.Contains("Content=\"清除报警\"", text);
            Assert.Contains("【流程：{0}】", text);
            Assert.Contains("Content=\"导出\"", text);
            Assert.Contains("Content=\"CAN监控\"", text);
            Assert.Contains("Header=\"测试信息\"", text);
            Assert.Contains("Header=\"当前测试\"", text);
            Assert.Contains("Header=\"系统消息\"", text);
            Assert.Contains("Header=\"消息\"", text);
            Assert.Contains("Header=\"测试项目\"", text);
            Assert.Contains("Header=\"测试点\"", text);
            Assert.Contains("Header=\"数据类型\"", text);
            Assert.Contains("Header=\"下限\"", text);
            Assert.Contains("Header=\"上限\"", text);
            Assert.Contains("Header=\"单位\"", text);
            Assert.Contains("Header=\"测试值\"", text);
            Assert.Contains("Header=\"结果\"", text);
            Assert.Contains("Header=\"判定符号\"", text);
            Assert.Contains("Text=\"WaitTest\"", text);
        }

        [Fact]
        public void ProductTestShellScope_DoesNotCallRealExecutionDeviceMesOrAutoTest()
        {
            var productTestRoot = GetRepositoryPath("ATS5.Modules.ProductTest");
            var applicationRoot = GetRepositoryPath("ATS5.Application", "ProductTestShell");
            var sourceText = ReadAllSource(productTestRoot);
            if (Directory.Exists(applicationRoot))
            {
                sourceText += ReadAllSource(applicationRoot);
            }

            Assert.DoesNotContain("ProductTestCore", sourceText);
            Assert.DoesNotContain("SysCache.StartTest", sourceText);
            Assert.DoesNotContain("SysCache.UploadData", sourceText);
            Assert.DoesNotContain("AutoTestGetBarcodeDic", sourceText);
            Assert.DoesNotContain("AutoTestCheckBarcodeDic", sourceText);
            Assert.DoesNotContain("AutoTestPreTestMesCheckDic", sourceText);
            Assert.DoesNotContain("AutoTestStartTestDic", sourceText);
            Assert.DoesNotContain("AutoTestStopTestDic", sourceText);
            Assert.DoesNotContain("AutoTestStateDic", sourceText);
            Assert.DoesNotContain("DevicePool", sourceText);
            Assert.DoesNotContain(".Init(", sourceText);
            Assert.DoesNotContain("GP12", sourceText);
            Assert.DoesNotContain("AutoType 9", sourceText);
        }

        [Fact]
        public void BarcodeDialog_ContainsLegacyFlowBarcodeAndButtonText()
        {
            var viewPath = GetRepositoryPath("ATS5.Modules.ProductTest", "Views", "BarcodeDialog.xaml");
            var text = File.ReadAllText(viewPath);

            Assert.Contains("通道：{0} 条码输入", text);
            Assert.Contains("Text=\"流程文件\"", text);
            Assert.Contains("Text=\"单通道条码数量\"", text);
            Assert.Contains("Text=\"{Binding SelectedFlowName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}\"", text);
            Assert.Contains("Content=\"确定\"", text);
            Assert.Contains("Content=\"取消\"", text);
            Assert.Contains("Topmost=\"True\"", text);
            Assert.Contains("ResizeMode=\"NoResize\"", text);
        }

        private static string ReadAllSource(string directory)
        {
            var source = string.Empty;
            foreach (var file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".cs") || file.EndsWith(".xaml"))
                {
                    source += File.ReadAllText(file);
                }
            }

            return source;
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
