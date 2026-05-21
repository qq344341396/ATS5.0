using System;
using System.IO;

namespace ATS5.Infrastructure.LegacyAdapters.Runtime
{
    public sealed class LegacyRuntimeContext
    {
        private static readonly object RuntimeLock = new object();

        private readonly IRuntimePathProvider _runtimePathProvider;

        public LegacyRuntimeContext(IRuntimePathProvider runtimePathProvider)
        {
            _runtimePathProvider = runtimePathProvider ?? throw new ArgumentNullException(nameof(runtimePathProvider));
        }

        public T Execute<T>(Func<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (RuntimeLock)
            {
                var previousDirectory = Environment.CurrentDirectory;
                try
                {
                    Environment.CurrentDirectory = _runtimePathProvider.LegacyRuntimeRoot;
                    return action();
                }
                finally
                {
                    Environment.CurrentDirectory = previousDirectory;
                }
            }
        }

        public void Execute(Action action)
        {
            Execute(
                () =>
                {
                    action();
                    return true;
                });
        }

        public string GetLegacyPath(string relativePath)
        {
            return Path.Combine(_runtimePathProvider.LegacyRuntimeRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
