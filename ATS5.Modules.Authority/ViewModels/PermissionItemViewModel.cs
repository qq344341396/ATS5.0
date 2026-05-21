using ATS5.Application.Authority;
using Prism.Mvvm;

namespace ATS5.Modules.Authority.ViewModels
{
    public sealed class PermissionItemViewModel : BindableBase
    {
        private bool _isChecked;

        public PermissionItemViewModel(AuthorityPermissionItem item)
        {
            Tag = item.Tag;
            Name = item.Name;
        }

        public string Tag { get; }

        public string Name { get; }

        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }
    }
}
