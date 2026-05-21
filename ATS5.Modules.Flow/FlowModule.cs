using ATS5.Modules.Flow.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace ATS5.Modules.Flow
{
    public sealed class FlowModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<FlowEditorView>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
