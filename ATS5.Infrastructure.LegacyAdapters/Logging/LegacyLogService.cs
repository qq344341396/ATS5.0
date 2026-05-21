using System;
using ATS5.Application.Flow;
using ATS5.Application.Logging;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using ATSCommon;

namespace ATS5.Infrastructure.LegacyAdapters.Logging
{
    public sealed class LegacyLogService : ILogService, IOperationLogger
    {
        private readonly LegacyRuntimeContext _runtimeContext;

        public LegacyLogService(LegacyRuntimeContext runtimeContext)
        {
            _runtimeContext = runtimeContext;
        }

        public void Init()
        {
            _runtimeContext.Execute(() => LogHelper.Init("log4net.config"));
        }

        public void Info(string message)
        {
            _runtimeContext.Execute(() => LogHelper.Info(message));
        }

        public void Operate(string message)
        {
            _runtimeContext.Execute(() => LogHelper.Operate(message));
        }

        public void Error(string message, Exception exception)
        {
            _runtimeContext.Execute(() => LogHelper.Error(message, exception));
        }

        public void Test(string message, string key = "")
        {
            _runtimeContext.Execute(() => LogHelper.Test(message, key));
        }

        public void Mes(string message)
        {
            _runtimeContext.Execute(() => LogHelper.Mes(message));
        }

        public void CanTool(string message)
        {
            _runtimeContext.Execute(() => LogHelper.CanTool(message));
        }
    }
}
