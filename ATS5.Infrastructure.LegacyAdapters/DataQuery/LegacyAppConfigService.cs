using ATS5.Application.DataQuery;
using ATSCommon;
using ATS5.Infrastructure.LegacyAdapters.Runtime;

namespace ATS5.Infrastructure.LegacyAdapters.DataQuery
{
    public sealed class LegacyAppConfigService : IAppConfigService
    {
        private readonly LegacyRuntimeContext _runtimeContext;

        public LegacyAppConfigService(LegacyRuntimeContext runtimeContext)
        {
            _runtimeContext = runtimeContext;
        }

        public string GetValue(string key)
        {
            return _runtimeContext.Execute(() => ConfigHelper.GetValueApp(key));
        }

        public bool SetValue(string key, string value)
        {
            return _runtimeContext.Execute(() => ConfigHelper.SetValueApp(key, value));
        }
    }
}
