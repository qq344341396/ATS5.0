using System.Windows;
using ATS5.Application.DataQuery;
using ATS5.Application.DeviceConfig;
using ATS5.Application.Flow;
using ATS5.Application.Logging;
using ATS5.Application.Authority;
using ATS5.Application.ProductTestShell;
using ATS5.Infrastructure.LegacyAdapters.Authority;
using ATS5.Infrastructure.LegacyAdapters.DataQuery;
using ATS5.Infrastructure.LegacyAdapters.DeviceConfig;
using ATS5.Infrastructure.LegacyAdapters.Flow;
using ATS5.Infrastructure.LegacyAdapters.Logging;
using ATS5.Infrastructure.LegacyAdapters.ProductTestShell;
using ATS5.Infrastructure.LegacyAdapters.Runtime;
using ATS5.Modules.Authority;
using ATS5.Modules.DataQuery;
using ATS5.Modules.Device;
using ATS5.Modules.Flow;
using ATS5.Modules.ProductTest;
using ATS5.Wpf.Core;
using ATS5.Wpf.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;

namespace ATS5.Wpf
{
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IRuntimePathProvider, DebugRuntimePathProvider>();
            containerRegistry.RegisterSingleton<LegacyRuntimeContext>();
            containerRegistry.Register<IAppConfigService, LegacyAppConfigService>();
            containerRegistry.Register<ITestRecordRepository, LegacyTestRecordRepository>();
            containerRegistry.Register<IManualMesGateway, LegacyManualMesGateway>();
            containerRegistry.Register<IFlowRepository, LegacyFlowRepository>();
            containerRegistry.Register<IFlowScriptValidator, LegacyFlowScriptValidator>();
            containerRegistry.Register<IAuthorityRepository, LegacyAuthorityRepository>();
            containerRegistry.Register<IDeviceConfigRepository, LegacyDeviceConfigRepository>();
            containerRegistry.Register<IProductTestFlowCatalog, LegacyProductTestFlowCatalog>();
            containerRegistry.Register<ICurrentUserContext, LegacyCurrentUserContext>();
            containerRegistry.Register<ILogService, LegacyLogService>();
            containerRegistry.Register<IOperationLogger, LegacyLogService>();
            containerRegistry.Register<IFlowChangePublisher, LegacyFlowChangePublisher>();
            containerRegistry.Register<IMessageDialogService, WpfMessageDialogService>();
            containerRegistry.Register<DataQueryService>();
            containerRegistry.Register<ManualMesUploadService>();
            containerRegistry.Register<FlowEditorService>();
            containerRegistry.Register<AuthorityService>();
            containerRegistry.Register<DeviceConfigService>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<DataQueryModule>();
            moduleCatalog.AddModule<FlowModule>();
            moduleCatalog.AddModule<AuthorityModule>();
            moduleCatalog.AddModule<DeviceModule>();
            moduleCatalog.AddModule<ProductTestModule>();
        }
    }
}
