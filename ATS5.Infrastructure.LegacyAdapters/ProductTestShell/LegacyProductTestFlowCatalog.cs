using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ATS5.Application.ProductTestShell;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using ATSCommon;
using ATSCore;

namespace ATS5.Infrastructure.LegacyAdapters.ProductTestShell
{
    public sealed class LegacyProductTestFlowCatalog : IProductTestFlowCatalog
    {
        private readonly LegacyRuntimeContext _runtimeContext;

        public LegacyProductTestFlowCatalog(LegacyRuntimeContext runtimeContext)
        {
            _runtimeContext = runtimeContext ?? throw new ArgumentNullException(nameof(runtimeContext));
        }

        public IReadOnlyList<string> GetFlowNames()
        {
            try
            {
                return _runtimeContext.Execute<IReadOnlyList<string>>(
                    () =>
                    {
                        if (!Directory.Exists(SysCache.PathFlows))
                        {
                            return Array.Empty<string>();
                        }

                        return new DirectoryInfo(SysCache.PathFlows)
                            .GetFiles("*.fw")
                            .Select(file => DirFileHelper.GetFileNameNoExtension(file.Name))
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .ToList();
                    });
            }
            catch (IOException)
            {
                return Array.Empty<string>();
            }
            catch (UnauthorizedAccessException)
            {
                return Array.Empty<string>();
            }
        }
    }
}
