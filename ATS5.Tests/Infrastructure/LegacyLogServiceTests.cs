using System;
using System.IO;
using System.Linq;
using ATS5.Application.Logging;
using ATS5.Infrastructure.LegacyAdapters.Logging;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using Xunit;

namespace ATS5.Tests.Infrastructure
{
    public sealed class LegacyLogServiceTests
    {
        [Fact]
        public void LogService_WritesAllLegacyChannels_ToOriginalDirectories()
        {
            var outputDirectory = AppContext.BaseDirectory;
            CopyLegacyLogConfig(outputDirectory);
            var service = CreateLogService(outputDirectory);
            var marker = Guid.NewGuid().ToString("N");

            service.Init();
            service.Info($"wave1-info-{marker}");
            service.Error($"wave1-error-{marker}", new InvalidOperationException($"wave1-error-exception-{marker}"));
            service.Test($"wave1-test-{marker}", "CH1");
            service.Mes($"wave1-mes-{marker}");
            service.Operate($"wave1-operate-{marker}");
            service.CanTool($"wave1-cantool-{marker}");

            AssertLogContains(outputDirectory, "Info", $"wave1-info-{marker}", "INFO");
            AssertLogContains(outputDirectory, "Error", $"wave1-error-{marker}", "ERROR");
            AssertLogContains(outputDirectory, "Test", $"[CH1]wave1-test-{marker}", "INFO");
            AssertLogContains(outputDirectory, "MES", $"wave1-mes-{marker}", "INFO");
            AssertLogContains(outputDirectory, "Operate", $"wave1-operate-{marker}", "INFO");
            AssertLogContains(outputDirectory, "Cantool", $"wave1-cantool-{marker}", "INFO");
        }

        private static ILogService CreateLogService(string runtimeDirectory)
        {
            var runtimeContext = new LegacyRuntimeContext(new DebugRuntimePathProvider(runtimeDirectory));
            return new LegacyLogService(runtimeContext);
        }

        private static void CopyLegacyLogConfig(string outputDirectory)
        {
            File.Copy(
                @"D:\CODE\ATE\01_code\ATS\bin\Debug\log4net.config",
                Path.Combine(outputDirectory, "log4net.config"),
                true);
        }

        private static void AssertLogContains(string runtimeDirectory, string channelName, string expectedText, string expectedLevel)
        {
            var channelDirectory = Path.Combine(runtimeDirectory, "Log", channelName);
            Assert.True(Directory.Exists(channelDirectory), $"Missing log directory: {channelDirectory}");

            var logFile = Directory.GetFiles(channelDirectory, "*.log").SingleOrDefault();
            Assert.False(string.IsNullOrWhiteSpace(logFile), $"Missing log file in: {channelDirectory}");

            var content = ReadShared(logFile);
            Assert.Contains(expectedText, content);
            Assert.Contains(expectedLevel, content);
        }

        private static string ReadShared(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
