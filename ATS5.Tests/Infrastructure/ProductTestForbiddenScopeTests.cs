using System.Collections.Generic;
using System.IO;
using Xunit;

namespace ATS5.Tests.Infrastructure
{
    public sealed class ProductTestForbiddenScopeTests
    {
        [Fact]
        public void ProductTestImplementationScope_DoesNotEnterForbiddenWave4Areas()
        {
            var sourceText = ReadAllExistingSource(
                GetRepositoryPath("ATS5.Application", "ProductTest"),
                GetRepositoryPath("ATS5.Infrastructure.LegacyAdapters", "ProductTest"),
                GetRepositoryPath("ATS5.Modules.ProductTest"));

            Assert.DoesNotContain("UcMain_DB_GP12_EVB", sourceText);
            Assert.DoesNotContain("DB_GP12", sourceText);
            Assert.DoesNotContain("MesType=\"12\"", sourceText);
            Assert.DoesNotContain("MesType = \"12\"", sourceText);
            Assert.DoesNotContain("AutoType 9", sourceText);
            Assert.DoesNotContain("AutoType=\"9\"", sourceText);
            Assert.DoesNotContain("AutoType = \"9\"", sourceText);
            Assert.DoesNotContain("ATSAutoTest", sourceText);
            Assert.DoesNotContain("ATSDevice", sourceText);
            Assert.DoesNotContain("FrmDevDebug", sourceText);
            Assert.DoesNotContain("DeviceDebugHost", sourceText);
            Assert.DoesNotContain("ATS5.Modules.DeviceDebug", sourceText);
        }

        private static string ReadAllExistingSource(params string[] directories)
        {
            var files = new List<string>();
            foreach (var directory in directories)
            {
                if (!Directory.Exists(directory))
                {
                    continue;
                }

                files.AddRange(Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories));
            }

            var sourceText = string.Empty;
            foreach (var file in files)
            {
                if (file.EndsWith(".cs") || file.EndsWith(".xaml") || file.EndsWith(".csproj"))
                {
                    sourceText += File.ReadAllText(file);
                }
            }

            return sourceText;
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
