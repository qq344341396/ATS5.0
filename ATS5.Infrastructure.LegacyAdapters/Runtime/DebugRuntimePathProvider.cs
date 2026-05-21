using System;
using System.IO;

namespace ATS5.Infrastructure.LegacyAdapters.Runtime
{
    public sealed class DebugRuntimePathProvider : IRuntimePathProvider
    {
        private readonly string _legacyRuntimeRoot;

        public DebugRuntimePathProvider()
            : this(ResolveDefaultLegacyRuntimeRoot())
        {
        }

        public DebugRuntimePathProvider(string legacyRuntimeRoot)
        {
            if (string.IsNullOrWhiteSpace(legacyRuntimeRoot))
            {
                throw new ArgumentException("Legacy runtime root is required.", nameof(legacyRuntimeRoot));
            }

            _legacyRuntimeRoot = Path.GetFullPath(legacyRuntimeRoot);
        }

        public string LegacyRuntimeRoot => _legacyRuntimeRoot;

        private static string ResolveDefaultLegacyRuntimeRoot()
        {
            var currentDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (currentDirectory != null)
            {
                var candidate = Path.Combine(currentDirectory.FullName, "01_code", "ATS", "bin", "Debug");
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                currentDirectory = currentDirectory.Parent;
            }

            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "ATS", "bin", "Debug"));
        }
    }
}
