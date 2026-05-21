using System;
using System.IO;
using ATS5.Application.DataQuery;
using ATS5.Infrastructure.LegacyAdapters.DataQuery;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using Xunit;

namespace ATS5.Tests.Infrastructure
{
    public sealed class LegacyAppConfigServiceTests
    {
        [Fact]
        public void SetValue_UpdatesExistingKey_InTemporaryAtsConfigOnly()
        {
            var runtimeDirectory = CreateRuntimeDirectory();
            IAppConfigService service = new LegacyAppConfigService(
                new LegacyRuntimeContext(new DebugRuntimePathProvider(runtimeDirectory)));
            var originalValue = service.GetValue("Language");
            var expectedValue = originalValue == "0" ? "1" : "0";

            var result = service.SetValue("Language", expectedValue);

            Assert.True(result);
            Assert.Equal(expectedValue, service.GetValue("Language"));
            Assert.Equal(originalValue, ReadOriginalLanguageValue());
        }

        [Fact]
        public void SetValue_ReturnsFalse_WhenKeyDoesNotExist()
        {
            var runtimeDirectory = CreateRuntimeDirectory();
            IAppConfigService service = new LegacyAppConfigService(
                new LegacyRuntimeContext(new DebugRuntimePathProvider(runtimeDirectory)));

            var result = service.SetValue("Wave1MissingKey", "value");

            Assert.False(result);
            Assert.Equal(string.Empty, service.GetValue("Wave1MissingKey"));
        }

        private static string CreateRuntimeDirectory()
        {
            var directory = Path.Combine(Path.GetTempPath(), "ATS5-Wave1-Config-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            File.Copy(GetOriginalConfigPath(), Path.Combine(directory, "ATS.exe.config"));
            return directory;
        }

        private static string ReadOriginalLanguageValue()
        {
            var text = File.ReadAllText(GetOriginalConfigPath());
            var marker = "key=\"Language\" value=\"";
            var start = text.IndexOf(marker, StringComparison.Ordinal);
            Assert.True(start >= 0, "Original ATS.exe.config does not contain Language key.");
            start += marker.Length;
            var end = text.IndexOf("\"", start, StringComparison.Ordinal);
            return text.Substring(start, end - start);
        }

        private static string GetOriginalConfigPath()
        {
            return @"D:\CODE\ATE\01_code\ATS\bin\Debug\ATS.exe.config";
        }
    }
}
