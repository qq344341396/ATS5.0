using ATS5.Modules.ProductTest.ViewModels;
using ATS5.Modules.ProductTest.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace ATS5.Modules.ProductTest
{
    public sealed class ProductTestModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IBarcodeDialogService, BarcodeDialogService>();
            containerRegistry.RegisterForNavigation<ProductTestShellView>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
