using ATS5.Modules.Authority.ViewModels;
using ATS5.Modules.Authority.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace ATS5.Modules.Authority
{
    public sealed class AuthorityModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IAuthorityDialogService, AuthorityDialogService>();
            containerRegistry.RegisterForNavigation<AuthorityView>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
