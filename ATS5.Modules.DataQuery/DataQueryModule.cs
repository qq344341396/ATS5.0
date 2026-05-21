using ATS5.Modules.DataQuery.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace ATS5.Modules.DataQuery
{
    public sealed class DataQueryModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<DataQueryView>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
