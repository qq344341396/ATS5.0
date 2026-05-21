using ATS5.Application.DeviceConfig;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using ATSCore;
using System;

namespace ATS5.Infrastructure.LegacyAdapters.DeviceConfig
{
    public sealed class LegacyCurrentUserContext : ICurrentUserContext
    {
        private readonly LegacyRuntimeContext _runtimeContext;

        public LegacyCurrentUserContext(LegacyRuntimeContext runtimeContext)
        {
            _runtimeContext = runtimeContext ?? throw new ArgumentNullException(nameof(runtimeContext));
        }

        public string RoleName => _runtimeContext.Execute(() => SysCache.CurrentRole?.RoleName ?? string.Empty);

        public string UserName => _runtimeContext.Execute(() => SysCache.CurrentUser?.UserName ?? string.Empty);
    }
}
