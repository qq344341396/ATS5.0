using System.IO;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using Xunit;

namespace ATS5.Tests.Infrastructure
{
    public sealed class DebugRuntimePathProviderTests
    {
        [Fact]
        public void DefaultProvider_ResolvesToOriginalWinFormsDebugRuntime()
        {
            var provider = new DebugRuntimePathProvider();

            Assert.Equal(
                Path.GetFullPath(@"D:\CODE\ATE\01_code\ATS\bin\Debug"),
                provider.LegacyRuntimeRoot);
            Assert.True(Directory.Exists(provider.LegacyRuntimeRoot));
        }
    }
}
