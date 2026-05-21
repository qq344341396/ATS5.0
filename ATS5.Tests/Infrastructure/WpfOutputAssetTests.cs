using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace ATS5.Tests.Infrastructure
{
    public sealed class WpfOutputAssetTests
    {
        [Fact]
        public void DebugOutput_ContainsLegacyRuntimeAssetsRequiredByWave1()
        {
            var outputDirectory = GetWpfDebugOutputDirectory();
            var requiredRelativePaths = new[]
            {
                "ATS.exe.config",
                "log4net.config",
                "ATSCommon.dll",
                "ATSCore.dll",
                "ATSModel.dll",
                "ATSMes.dll",
                "ATSAutoTest.dll",
                @"AppDll\Dapper.dll",
                @"AppDll\log4net.dll",
                @"AppDll\Newtonsoft.Json.dll",
                @"AppDll\sqlite\System.Data.SQLite.dll",
                @"AppDll\sqlite\x86\SQLite.Interop.dll",
                @"AppDll\sqlite\x64\SQLite.Interop.dll",
                @"SysCache\Devices\DeviceHelper.dll"
            };

            var missingPaths = requiredRelativePaths
                .Where(relativePath => !File.Exists(Path.Combine(outputDirectory, relativePath)))
                .ToList();

            Assert.True(
                missingPaths.Count == 0,
                $"Missing WPF output assets under '{outputDirectory}': {string.Join(", ", missingPaths)}");
        }

        private static string GetWpfDebugOutputDirectory()
        {
            var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            return Path.Combine(repositoryRoot, "ATS5.Wpf", "bin", "Debug", "net461");
        }
    }
}
