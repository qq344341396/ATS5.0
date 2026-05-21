using Xunit;

namespace ATS5.Tests.Infrastructure
{
    public static class LegacyTestCollections
    {
        public const string SysCacheStaticState = "Legacy SysCache static state";
    }

    [CollectionDefinition(LegacyTestCollections.SysCacheStaticState)]
    public sealed class LegacySysCacheCollection
    {
    }
}
